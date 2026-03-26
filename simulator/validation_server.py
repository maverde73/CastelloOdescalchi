"""
Standalone validation server — simulates GestioneTornelloREA.
Listens for turnstile UDP messages, validates tickets against DB, responds via UDP.
Exposes HTTP API for status/config.

Run: python validation_server.py
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

from ticket_validator import check_ticket, TicketState

CONFIG_PATH = os.path.join(os.path.dirname(os.path.abspath(__file__)), "config.json")

# 0 = left, 1 = right, 2 = denied
VERSO_ENTRATA = 0

passage_remaining = 0
message_count = 0
server_active = False


def load_config():
    with open(CONFIG_PATH) as f:
        return json.load(f)["validation_server"]


def get_direction() -> str:
    if VERSO_ENTRATA == 0:
        return "l"
    elif VERSO_ENTRATA == 1:
        return "r"
    return "x"


def build_xml(tag_name: str, tid: str, content_builder) -> bytes:
    lines = ['<?xml version="1.0" encoding="UTF-8"?>']
    lines.append(f'<cmf id="{tid}" addr="1" protocol="1.0">')
    lines.append(content_builder())
    lines.append('</cmf>')
    return ''.join(lines).encode('utf-8')


def handle_test(tid: str) -> bytes:
    now = datetime.now().strftime("%Y%m%d%H%M%S")
    return build_xml("return", tid,
                     lambda: f'<return><time>{now}</time></return>')


def handle_entry(tid: str, barcode: str, logger=None) -> bytes:
    global passage_remaining

    result = check_ticket(barcode, 3, logger)
    passage_remaining = 0

    if result["state"] == TicketState.VALIDO:
        direction = get_direction()
        passage_remaining = result["pax"]
        msg = f"Biglietto valido per {result['pax']}"
        passage = "1"
    else:
        direction = "x"
        passage = "0"
        msg_map = {
            TicketState.SCADUTO: "Biglietto scaduto",
            TicketState.VALIDATO: "Biglietto gia vidimato",
            TicketState.ANNULLATO: "Biglietto annullato",
            TicketState.INVALIDO: "Biglietto non valido",
        }
        msg = msg_map.get(result["state"], "Biglietto non valido")

    return build_xml("return", tid,
                     lambda: f'<return><act>{direction}</act>'
                             f'<cnt>{passage}</cnt>'
                             f'<txt>{msg}</txt></return>')


def handle_pass(tid: str) -> bytes:
    global passage_remaining
    passage_remaining -= 1
    return build_xml("return", tid, lambda: '<return>true</return>')


def run_udp_server(host, port, on_message=None, logger=None, stop_event=None):
    global passage_remaining, message_count, server_active

    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    sock.bind((host, port))
    sock.settimeout(1.0)

    server_active = True
    print(f"Validation server (UDP) listening on {host}:{port}")

    while True:
        if stop_event and stop_event.is_set():
            break

        try:
            data, addr = sock.recvfrom(4096)
        except socket.timeout:
            continue
        except OSError:
            break

        try:
            xml_str = data.decode("utf-8")
            message_count += 1
            print(f"<<< RECV from {addr}: {xml_str[:80]}...")

            if on_message:
                on_message("recv", addr, xml_str)

            root = ET.fromstring(xml_str)
            tid = root.get("id", "1")
            response = None

            if root.find("test") is not None:
                response = handle_test(tid)
            elif root.find("entry") is not None:
                chip = root.find("entry/chip")
                barcode = chip.text if chip is not None else ""
                response = handle_entry(tid, barcode, logger=logger)
            elif root.find("pass") is not None:
                response = handle_pass(tid)

            if response is not None:
                sock.sendto(response, addr)
                response_str = response.decode('utf-8')
                print(f">>> SENT to {addr}: {response_str[:80]}...")

                if on_message:
                    on_message("send", addr, response_str)

        except Exception as ex:
            print(f"  ERROR: {type(ex).__name__}: {ex}")

    sock.close()
    server_active = False
    print("Validation server (UDP) stopped.")


# --- HTTP API ---

http_app = FastAPI(title="Validation Server")


@http_app.get("/status")
def status():
    return {
        "active": server_active,
        "message_count": message_count,
        "passage_remaining": passage_remaining
    }


@http_app.get("/config")
def get_config():
    return load_config()


class ConfigUpdate(BaseModel):
    udp_port: int = None


@http_app.post("/config")
def update_config(body: ConfigUpdate):
    cfg = load_config()
    if body.udp_port is not None:
        cfg["udp_port"] = body.udp_port
    with open(CONFIG_PATH) as f:
        full = json.load(f)
    full["validation_server"] = cfg
    with open(CONFIG_PATH, "w") as f:
        json.dump(full, f, indent=2)
    return {"status": "ok", "config": cfg, "note": "restart required for port change"}


def main():
    cfg = load_config()

    # Start UDP server in background thread
    udp_thread = threading.Thread(
        target=run_udp_server,
        args=(cfg["host"], cfg["udp_port"]),
        daemon=True
    )
    udp_thread.start()

    print(f"Validation server HTTP API: http://{cfg['host']}:{cfg['http_port']}")

    # Start HTTP API (blocking)
    uvicorn.run(http_app, host=cfg["host"], port=cfg["http_port"])


if __name__ == "__main__":
    main()
