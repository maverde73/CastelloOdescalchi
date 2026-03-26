"""
Validation server service — UDP server that validates tickets.
Runs UDP listener in background thread, callbacks update Kivy UI via Clock.
"""

import os
import socket
import sys
import threading
import xml.etree.ElementTree as ET
from datetime import datetime
from kivy.clock import Clock
import config_manager

# Ensure simulator/ is on path
sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))

from barcode import decode, EPOCH, MAX_SECONDS


class TicketState:
    VALIDO = "Valido"
    SCADUTO = "Scaduto"
    INVALIDO = "Invalido"
    VALIDATO = "Validato"
    ANNULLATO = "Annullato"


def check_ticket(barcode_str, giorni_validita=3, logger=None):
    """Formal validation only — checks barcode format and expiry, no DB lookup."""
    if logger is None:
        logger = lambda m: None
    result = {"state": TicketState.INVALIDO, "pax": 0, "date": None, "detail": ""}

    try:
        logger(f"  Step 1: Formal validation (no DB)")
        code = int(barcode_str)
        logger(f"  Step 2: Barcode parsed as int: {code}")

        decoded = decode(code)
        date = decoded["date"]
        pax = decoded["pax"]
        tipo = decoded["tipo"]
        cont = decoded["cont"]
        logger(f"  Step 2: Decoded -> date={date}, pax={pax}, tipo={tipo}, cont={cont}")

        # Expiry check
        now = datetime.now()
        from datetime import timedelta
        expiry_threshold = (now - timedelta(days=giorni_validita)).date()
        logger(f"  Step 3: Expiry check -> ticketDate={date.date()}, threshold={expiry_threshold}, now={now}")

        if date.date() <= expiry_threshold:
            result["state"] = TicketState.SCADUTO
            result["date"] = date
            result["detail"] = f"ticketDate={date.date()} <= threshold={expiry_threshold}"
            logger(f"  RESULT: SCADUTO - {result['detail']}")
            return result

        # Formal valid
        result["state"] = TicketState.VALIDO
        result["pax"] = pax
        result["date"] = date
        result["detail"] = f"pax={pax} (formal validation, no DB)"
        logger(f"  RESULT: VALIDO (formal) pax={pax}")
        return result

    except Exception as ex:
        result["detail"] = f"EXCEPTION: {type(ex).__name__}: {ex}"
        logger(f"  RESULT: INVALIDO (EXCEPTION) - {type(ex).__name__}: {ex}")
        return result


class ServerService:
    def __init__(self, on_log=None, on_validation=None, on_status=None):
        """
        Callbacks (called on main thread via Clock):
            on_log(direction, addr, xml, timestamp)
            on_validation(result_dict)
            on_status(running: bool, msg_count: int)
        """
        self.on_log = on_log or (lambda *a: None)
        self.on_validation = on_validation or (lambda *a: None)
        self.on_status = on_status or (lambda *a: None)

        self.sock = None
        self.host = "0.0.0.0"
        self.udp_port = 10000
        self.running = False
        self.message_count = 0
        self.passage_remaining = 0
        self.direction = 0  # 0=left, 1=right, 2=denied
        self.giorni_validita = 3
        self._stop_event = threading.Event()
        self._thread = None

    def start(self):
        if self.running:
            return
        cfg = config_manager.get_server()
        self.host = cfg["host"]
        self.udp_port = cfg["udp_port"]

        self._stop_event.clear()
        self._thread = threading.Thread(target=self._run_loop, daemon=True)
        self._thread.start()

    def stop(self):
        self._stop_event.set()
        if self.sock:
            try:
                self.sock.close()
            except Exception:
                pass
        self.running = False
        Clock.schedule_once(lambda dt: self.on_status(False, self.message_count))

    def set_direction(self, d):
        """0=left, 1=right, 2=denied"""
        self.direction = d

    def get_direction_str(self):
        if self.direction == 0:
            return "l"
        elif self.direction == 1:
            return "r"
        return "x"

    def manual_check(self, barcode):
        """Run CheckTicket without UDP — returns result on main thread via callback."""
        def do_check():
            steps = []
            def logger(msg):
                steps.append(msg)
                Clock.schedule_once(lambda dt, m=msg: self.on_log("step", "", m, datetime.now().isoformat()))

            result = check_ticket(barcode, self.giorni_validita, logger)
            Clock.schedule_once(lambda dt: self.on_validation({
                "state": result["state"],
                "pax": result["pax"],
                "date": str(result["date"]) if result["date"] else None,
                "detail": result["detail"],
                "barcode": barcode,
                "steps": steps
            }))
        threading.Thread(target=do_check, daemon=True).start()

    def send_open(self, target_host=None, target_port=None):
        """Send forced open command to a turnstile."""
        if not self.sock:
            return
        def do_send():
            tid = str(self.message_count + 1)
            direction = self.get_direction_str()
            xml_str = (f'<?xml version="1.0" encoding="UTF-8"?>'
                       f'<cmf id="{tid}" addr="1" protocol="1.0">'
                       f'<open><act>{direction}</act><cnt>1</cnt>'
                       f'<txt>Apertura forzata</txt></open></cmf>')
            # Send to last known turnstile or configured address
            addr = (target_host or "127.0.0.1", target_port or 10001)
            self.sock.sendto(xml_str.encode("utf-8"), addr)
            ts = datetime.now().isoformat()
            Clock.schedule_once(lambda dt: self.on_log("sent", f"{addr[0]}:{addr[1]}", xml_str, ts))
        threading.Thread(target=do_send, daemon=True).start()

    # --- UDP Server Loop ---

    def _run_loop(self):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        try:
            self.sock.bind((self.host, self.udp_port))
        except OSError as e:
            Clock.schedule_once(lambda dt: self.on_log("error", "", f"Bind failed: {e}", datetime.now().isoformat()))
            return

        self.sock.settimeout(1.0)
        self.running = True
        Clock.schedule_once(lambda dt: self.on_status(True, 0))

        while not self._stop_event.is_set():
            try:
                data, addr = self.sock.recvfrom(4096)
            except socket.timeout:
                continue
            except OSError:
                break

            try:
                xml_str = data.decode("utf-8")
                self.message_count += 1
                ts = datetime.now().isoformat()
                Clock.schedule_once(lambda dt, a=addr, x=xml_str, t=ts: self.on_log("recv", f"{a[0]}:{a[1]}", x, t))

                root = ET.fromstring(xml_str)
                tid = root.get("id", "1")
                response = None

                if root.find("test") is not None:
                    response = self._handle_test(tid)

                elif root.find("entry") is not None:
                    chip = root.find("entry/chip")
                    barcode = chip.text if chip is not None else ""
                    response = self._handle_entry(tid, barcode)

                elif root.find("pass") is not None:
                    response = self._handle_pass(tid)

                if response is not None:
                    self.sock.sendto(response, addr)
                    resp_str = response.decode('utf-8')
                    ts2 = datetime.now().isoformat()
                    Clock.schedule_once(lambda dt, a=addr, x=resp_str, t=ts2: self.on_log("sent", f"{a[0]}:{a[1]}", x, t))

                Clock.schedule_once(lambda dt: self.on_status(True, self.message_count))

            except Exception as ex:
                Clock.schedule_once(lambda dt, e=str(ex): self.on_log("error", "", f"ERROR: {e}", datetime.now().isoformat()))

        try:
            self.sock.close()
        except Exception:
            pass
        self.running = False
        Clock.schedule_once(lambda dt: self.on_status(False, self.message_count))

    def _build_xml(self, tid, content):
        return (f'<?xml version="1.0" encoding="UTF-8"?>'
                f'<cmf id="{tid}" addr="1" protocol="1.0">'
                f'{content}</cmf>').encode('utf-8')

    def _handle_test(self, tid):
        now = datetime.now().strftime("%Y%m%d%H%M%S")
        return self._build_xml(tid, f'<return><time>{now}</time></return>')

    def _handle_entry(self, tid, barcode):
        steps = []
        def logger(msg):
            steps.append(msg)
            Clock.schedule_once(lambda dt, m=msg: self.on_log("step", "", m, datetime.now().isoformat()))

        result = check_ticket(barcode, self.giorni_validita, logger)
        self.passage_remaining = 0

        if result["state"] == TicketState.VALIDO:
            direction = self.get_direction_str()
            self.passage_remaining = result["pax"]
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

        Clock.schedule_once(lambda dt: self.on_validation({
            "state": result["state"],
            "pax": result["pax"],
            "date": str(result["date"]) if result["date"] else None,
            "detail": result["detail"],
            "barcode": barcode,
            "message": msg,
            "steps": steps
        }))

        return self._build_xml(tid,
            f'<return><act>{direction}</act>'
            f'<cnt>{passage}</cnt>'
            f'<txt>{msg}</txt></return>')

    def _handle_pass(self, tid):
        self.passage_remaining = max(0, self.passage_remaining - 1)
        return self._build_xml(tid, '<return>true</return>')
