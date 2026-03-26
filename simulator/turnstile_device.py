"""
Standalone turnstile device simulator.
Simulates the physical REA/DCP turnstile terminal.

- Persistent UDP socket bound to a fixed port
- Sends CMF XML messages to a configurable validation server
- Exposes HTTP API for dashboard control

Run: python turnstile_device.py
"""

import json
import os
import socket
import threading
import xml.etree.ElementTree as ET
from datetime import datetime

import uvicorn
from fastapi import FastAPI
from pydantic import BaseModel

CONFIG_PATH = os.path.join(os.path.dirname(os.path.abspath(__file__)), "config.json")


def load_config():
    with open(CONFIG_PATH) as f:
        return json.load(f)["turnstile"]


def save_config(turnstile_cfg):
    with open(CONFIG_PATH) as f:
        full = json.load(f)
    full["turnstile"] = turnstile_cfg
    with open(CONFIG_PATH, "w") as f:
        json.dump(full, f, indent=2)


# --- UDP Turnstile ---

class TurnstileDevice:
    def __init__(self, bind_host, bind_port, server_host, server_port):
        self.bind_host = bind_host
        self.bind_port = bind_port
        self.server_host = server_host
        self.server_port = server_port
        self.transaction_id = 1
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.sock.bind((bind_host, bind_port))
        self.sock.settimeout(5.0)
        self.lock = threading.Lock()
        self.message_count = 0

    @property
    def server_addr(self):
        return (self.server_host, self.server_port)

    def _next_id(self):
        tid = str(self.transaction_id)
        self.transaction_id += 1
        return tid

    def _send_and_receive(self, xml_str: str) -> dict:
        with self.lock:
            self.message_count += 1
            sent_xml = xml_str
            self.sock.sendto(xml_str.encode("utf-8"), self.server_addr)

            try:
                data, addr = self.sock.recvfrom(4096)
                recv_xml = data.decode("utf-8")
                return {
                    "sent_xml": sent_xml,
                    "sent_to": f"{self.server_host}:{self.server_port}",
                    "recv_xml": recv_xml,
                    "recv_from": f"{addr[0]}:{addr[1]}",
                    "timeout": False
                }
            except socket.timeout:
                return {
                    "sent_xml": sent_xml,
                    "sent_to": f"{self.server_host}:{self.server_port}",
                    "recv_xml": None,
                    "recv_from": None,
                    "timeout": True
                }

    def send_test(self) -> dict:
        tid = self._next_id()
        now = datetime.now().strftime("%Y%m%d%H%M%S")
        xml = (f'<?xml version="1.0" encoding="UTF-8"?>'
               f'<cmf id="{tid}" addr="1" protocol="1.0">'
               f'<test><time>{now}</time></test></cmf>')
        return self._send_and_receive(xml)

    def send_entry(self, barcode: str) -> dict:
        tid = self._next_id()
        xml = (f'<?xml version="1.0" encoding="UTF-8"?>'
               f'<cmf id="{tid}" addr="1" protocol="1.0">'
               f'<entry><chip>{barcode}</chip></entry></cmf>')
        result = self._send_and_receive(xml)
        result["parsed"] = self._parse_entry_response(result.get("recv_xml"))
        return result

    def send_pass(self, act: str = "l") -> dict:
        tid = self._next_id()
        xml = (f'<?xml version="1.0" encoding="UTF-8"?>'
               f'<cmf id="{tid}" addr="1" protocol="1.0">'
               f'<pass><act>{act}</act></pass></cmf>')
        return self._send_and_receive(xml)

    def _parse_entry_response(self, xml_str):
        if not xml_str:
            return {"direction": None, "count": None, "text": None}
        try:
            root = ET.fromstring(xml_str)
            ret = root.find("return")
            if ret is not None:
                act = ret.find("act")
                cnt = ret.find("cnt")
                txt = ret.find("txt")
                return {
                    "direction": act.text if act is not None else None,
                    "count": cnt.text if cnt is not None else None,
                    "text": txt.text if txt is not None else None,
                }
        except ET.ParseError:
            pass
        return {"direction": None, "count": None, "text": None}

    def update_server(self, host, port):
        self.server_host = host
        self.server_port = port

    def close(self):
        self.sock.close()


# --- HTTP API ---

app = FastAPI(title="Turnstile Device")
device: TurnstileDevice = None


@app.get("/status")
def status():
    return {
        "active": device is not None,
        "bind": f"{device.bind_host}:{device.bind_port}" if device else None,
        "server_target": f"{device.server_host}:{device.server_port}" if device else None,
        "message_count": device.message_count if device else 0,
        "transaction_id": device.transaction_id if device else 0
    }


class ScanRequest(BaseModel):
    barcode: str


@app.post("/scan")
async def scan(body: ScanRequest):
    import asyncio
    return await asyncio.to_thread(device.send_entry, body.barcode)


@app.post("/pass")
async def do_pass():
    import asyncio
    return await asyncio.to_thread(device.send_pass)


@app.post("/test")
async def do_test():
    import asyncio
    return await asyncio.to_thread(device.send_test)


@app.get("/config")
def get_config():
    return load_config()


class ConfigUpdate(BaseModel):
    server_host: str = None
    server_udp_port: int = None


@app.post("/config")
def update_config(body: ConfigUpdate):
    cfg = load_config()
    if body.server_host is not None:
        cfg["server_host"] = body.server_host
        device.server_host = body.server_host
    if body.server_udp_port is not None:
        cfg["server_udp_port"] = body.server_udp_port
        device.server_port = body.server_udp_port
    save_config(cfg)
    return {"status": "ok", "config": cfg}


def main():
    global device
    cfg = load_config()

    device = TurnstileDevice(
        bind_host=cfg["host"],
        bind_port=cfg["udp_port"],
        server_host=cfg["server_host"],
        server_port=cfg["server_udp_port"]
    )

    print(f"Turnstile device started")
    print(f"  UDP bind: {cfg['host']}:{cfg['udp_port']}")
    print(f"  Server target: {cfg['server_host']}:{cfg['server_udp_port']}")
    print(f"  HTTP API: http://{cfg['host']}:{cfg['http_port']}")

    uvicorn.run(app, host=cfg["host"], port=cfg["http_port"])


if __name__ == "__main__":
    main()
