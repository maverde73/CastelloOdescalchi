"""
Dashboard — orchestrator for turnstile simulation.
Proxies requests to standalone turnstile_device and validation_server processes.
Serves the web UI and broadcasts events via WebSocket.

Run: python app.py
"""

import asyncio
import json
import os
from datetime import datetime
from typing import Optional

import httpx
import uvicorn
from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from fastapi.responses import FileResponse
from pydantic import BaseModel

from db_connection import get_connection
from ticket_emitter import emit_ticket as _emit_ticket

CONFIG_PATH = os.path.join(os.path.dirname(os.path.abspath(__file__)), "config.json")


def load_config():
    with open(CONFIG_PATH) as f:
        return json.load(f)


def save_config(cfg):
    with open(CONFIG_PATH, "w") as f:
        json.dump(cfg, f, indent=2)


# --- WebSocket Manager ---

class ConnectionManager:
    def __init__(self):
        self.connections: list[WebSocket] = []

    async def connect(self, ws: WebSocket):
        await ws.accept()
        self.connections.append(ws)

    def disconnect(self, ws: WebSocket):
        if ws in self.connections:
            self.connections.remove(ws)

    async def broadcast(self, message: dict):
        dead = []
        for ws in self.connections:
            try:
                await ws.send_json(message)
            except Exception:
                dead.append(ws)
        for ws in dead:
            self.disconnect(ws)


manager = ConnectionManager()
app = FastAPI(title="Turnstile Dashboard")


# --- Helpers ---

def get_local_ip():
    """Get the local network IP."""
    import socket as _sock
    try:
        s = _sock.socket(_sock.AF_INET, _sock.SOCK_DGRAM)
        s.connect(("8.8.8.8", 80))
        ip = s.getsockname()[0]
        s.close()
        return ip
    except Exception:
        return "127.0.0.1"


def dt_to_str(v):
    if isinstance(v, datetime):
        return v.isoformat()
    return v


def _turnstile_url():
    cfg = load_config()["turnstile"]
    return f"http://127.0.0.1:{cfg['http_port']}"


def _server_url():
    cfg = load_config()["validation_server"]
    return f"http://127.0.0.1:{cfg['http_port']}"


# --- Routes ---

@app.get("/")
async def index():
    return FileResponse(os.path.join(os.path.dirname(os.path.abspath(__file__)), "index.html"))


# --- Config ---

@app.get("/api/config")
async def get_config():
    return load_config()


class ConfigUpdateRequest(BaseModel):
    turnstile: Optional[dict] = None
    validation_server: Optional[dict] = None
    database: Optional[dict] = None


@app.post("/api/config")
async def update_config(body: ConfigUpdateRequest):
    cfg = load_config()
    if body.turnstile:
        cfg["turnstile"].update(body.turnstile)
    if body.validation_server:
        cfg["validation_server"].update(body.validation_server)
    if body.database:
        cfg["database"].update(body.database)
    save_config(cfg)

    # Propagate to turnstile device if server target changed
    if body.turnstile and ("server_host" in body.turnstile or "server_udp_port" in body.turnstile):
        try:
            async with httpx.AsyncClient(timeout=3) as client:
                await client.post(f"{_turnstile_url()}/config", json=body.turnstile)
        except Exception:
            pass

    return {"status": "ok", "config": cfg}


# --- Component Status ---

@app.get("/api/status")
async def get_status():
    turnstile_status = None
    server_status = None

    async with httpx.AsyncClient(timeout=2) as client:
        try:
            r = await client.get(f"{_turnstile_url()}/status")
            turnstile_status = r.json()
        except Exception:
            turnstile_status = {"active": False, "error": "unreachable"}

        try:
            r = await client.get(f"{_server_url()}/status")
            server_status = r.json()
        except Exception:
            server_status = {"active": False, "error": "unreachable"}

    cfg = load_config()
    local_ip = get_local_ip()
    return {
        "turnstile": turnstile_status,
        "validation_server": server_status,
        "local_ip": local_ip,
        "turnstile_connect_addr": f"{local_ip}:{cfg['turnstile']['udp_port']}",
        "server_connect_addr": f"{local_ip}:{cfg['validation_server']['udp_port']}"
    }


# --- Ticket Types ---

@app.get("/api/ticket-types")
async def get_ticket_types():
    def query():
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("SELECT Id_TipoBiglietto, Descrizione, Prezzo FROM LK_TipoBiglietto ORDER BY Id_TipoBiglietto")
        rows = cursor.fetchall()
        conn.close()
        return [{"id": r[0], "descrizione": r[1], "prezzo": float(r[2])} for r in rows]
    return await asyncio.to_thread(query)


# --- Emit Ticket ---

class EmitRequest(BaseModel):
    pax: int
    tipo: int = 1
    cont: int = 0
    date: Optional[str] = None


@app.post("/api/emit")
async def emit(body: EmitRequest):
    emission_date = datetime.fromisoformat(body.date) if body.date else None

    def do_emit():
        return _emit_ticket(pax=body.pax, tipo=body.tipo, cont=body.cont,
                            emission_date=emission_date)

    ticket = await asyncio.to_thread(do_emit)
    ticket_json = {k: dt_to_str(v) for k, v in ticket.items()}

    await manager.broadcast({
        "type": "ticket_emitted",
        "ticket": ticket_json,
        "timestamp": datetime.now().isoformat()
    })

    return ticket_json


# --- Turnstile Actions (proxy to turnstile_device) ---

class TurnstileActionRequest(BaseModel):
    barcode: str = ""
    action: str = "entry"  # "entry", "pass", "test"


@app.post("/api/turnstile/action")
async def turnstile_action(body: TurnstileActionRequest):
    try:
        async with httpx.AsyncClient(timeout=10) as client:
            if body.action == "test":
                r = await client.post(f"{_turnstile_url()}/test")
            elif body.action == "entry":
                r = await client.post(f"{_turnstile_url()}/scan", json={"barcode": body.barcode})
            elif body.action == "pass":
                r = await client.post(f"{_turnstile_url()}/pass")
            else:
                return {"error": f"Unknown action: {body.action}"}

            result = r.json()

            # Broadcast XML exchange to WebSocket
            if result.get("sent_xml"):
                await manager.broadcast({
                    "type": "udp_sent",
                    "xml": result["sent_xml"],
                    "to": result.get("sent_to", ""),
                    "action": body.action,
                    "timestamp": datetime.now().isoformat()
                })

            if result.get("recv_xml"):
                await manager.broadcast({
                    "type": "udp_recv",
                    "xml": result["recv_xml"],
                    "from": result.get("recv_from", ""),
                    "action": body.action,
                    "timestamp": datetime.now().isoformat()
                })
            elif result.get("timeout"):
                await manager.broadcast({
                    "type": "udp_timeout",
                    "action": body.action,
                    "timestamp": datetime.now().isoformat()
                })

            return result

    except httpx.ConnectError:
        return {"error": "Turnstile device unreachable", "timeout": True}
    except Exception as ex:
        return {"error": str(ex)}


# --- DB View ---

@app.get("/api/tickets")
async def get_tickets():
    def query():
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("""
            SELECT TOP 50 b.Id_Biglietto, b.Codice, t.Descrizione, b.Pax,
                   b.DataOraEmissione, b.Vidimato, b.Annullato, b.Passed,
                   b.DataOraVidimazione
            FROM Biglietto b
            LEFT JOIN LK_TipoBiglietto t ON b.Id_TipoBiglietto = t.Id_TipoBiglietto
            ORDER BY b.Id_Biglietto DESC
        """)
        rows = cursor.fetchall()
        conn.close()
        return [{
            "id": r[0],
            "codice": r[1],
            "tipo": r[2] or "N/A",
            "pax": r[3],
            "emissione": dt_to_str(r[4]),
            "vidimato": bool(r[5]),
            "annullato": bool(r[6]),
            "passed": r[7],
            "vidimazione": dt_to_str(r[8])
        } for r in rows]
    return await asyncio.to_thread(query)


# --- WebSocket ---

@app.websocket("/ws")
async def websocket_endpoint(ws: WebSocket):
    await manager.connect(ws)
    try:
        while True:
            await ws.receive_text()
    except WebSocketDisconnect:
        manager.disconnect(ws)


if __name__ == "__main__":
    cfg = load_config()["dashboard"]
    print(f"Dashboard: http://{cfg['host']}:{cfg['port']}")
    print(f"Make sure validation_server.py and turnstile_device.py are running!")
    uvicorn.run(app, host=cfg["host"], port=cfg["port"])
