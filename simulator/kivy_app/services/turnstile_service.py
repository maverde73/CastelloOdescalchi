"""
Turnstile device service — UDP communication with validation server.
Runs UDP operations in a background thread, callbacks update Kivy UI via Clock.
"""

import socket
import threading
import xml.etree.ElementTree as ET
from datetime import datetime
from kivy.clock import Clock
import config_manager


class TurnstileService:
    def __init__(self, on_display=None, on_log=None, on_connection=None):
        """
        Callbacks (called on main thread via Clock):
            on_display(text, color, pax_remaining)
            on_log(direction, addr, xml, timestamp)
            on_connection(connected: bool)
        """
        self.on_display = on_display or (lambda *a: None)
        self.on_log = on_log or (lambda *a: None)
        self.on_connection = on_connection or (lambda *a: None)

        self.sock = None
        self.server_host = "127.0.0.1"
        self.server_port = 10000
        self.bind_port = 10001
        self.transaction_id = 0
        self.pax_remaining = 0
        self.connected = False
        self._heartbeat_event = None
        self._lock = threading.Lock()

    def start(self):
        cfg = config_manager.get_turnstile()
        self.server_host = cfg["server_host"]
        self.server_port = cfg["server_udp_port"]
        self.bind_port = cfg["udp_port"]

        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.sock.bind(("0.0.0.0", self.bind_port))
        self.sock.settimeout(5.0)

        # Start periodic heartbeat
        self._heartbeat_event = Clock.schedule_interval(lambda dt: self._bg(self._heartbeat), 10)
        # Initial heartbeat
        self._bg(self._heartbeat)

    def stop(self):
        if self._heartbeat_event:
            self._heartbeat_event.cancel()
            self._heartbeat_event = None
        if self.sock:
            try:
                self.sock.close()
            except Exception:
                pass
            self.sock = None

    def update_config(self, server_host=None, server_port=None):
        if server_host:
            self.server_host = server_host
        if server_port:
            self.server_port = server_port
        config_manager.update_turnstile(
            server_host=self.server_host,
            server_udp_port=self.server_port
        )

    def scan(self, barcode):
        """Send entry request with barcode."""
        self._bg(self._do_scan, barcode)

    def send_pass(self):
        """Send pass confirmation."""
        self._bg(self._do_pass)

    def send_panic(self, state="on"):
        """Send panic mode command."""
        self._bg(self._do_panic, state)

    def force_open(self):
        """Send open gate command."""
        self._bg(self._do_open)

    # --- Background thread operations ---

    def _bg(self, fn, *args):
        threading.Thread(target=fn, args=args, daemon=True).start()

    def _next_id(self):
        self.transaction_id += 1
        return str(self.transaction_id)

    def _send_recv(self, xml_str):
        if not self.sock:
            return None
        with self._lock:
            addr = (self.server_host, self.server_port)
            ts = datetime.now().isoformat()
            Clock.schedule_once(lambda dt: self.on_log("sent", f"{addr[0]}:{addr[1]}", xml_str, ts))

            self.sock.sendto(xml_str.encode("utf-8"), addr)
            try:
                data, recv_addr = self.sock.recvfrom(4096)
                recv_xml = data.decode("utf-8")
                ts2 = datetime.now().isoformat()
                Clock.schedule_once(lambda dt: self.on_log("recv", f"{recv_addr[0]}:{recv_addr[1]}", recv_xml, ts2))
                return recv_xml
            except socket.timeout:
                Clock.schedule_once(lambda dt: self.on_log("timeout", "", "", ts))
                return None

    def _heartbeat(self):
        tid = self._next_id()
        now = datetime.now().strftime("%Y%m%d%H%M%S")
        xml = (f'<?xml version="1.0" encoding="UTF-8"?>'
               f'<cmf id="{tid}" addr="1" protocol="1.0">'
               f'<test><time>{now}</time></test></cmf>')
        result = self._send_recv(xml)
        ok = result is not None
        if ok != self.connected:
            self.connected = ok
            Clock.schedule_once(lambda dt: self.on_connection(ok))

    def _do_scan(self, barcode):
        tid = self._next_id()
        xml = (f'<?xml version="1.0" encoding="UTF-8"?>'
               f'<cmf id="{tid}" addr="1" protocol="1.0">'
               f'<entry><chip>{barcode}</chip></entry></cmf>')
        result = self._send_recv(xml)
        if result is None:
            Clock.schedule_once(lambda dt: self.on_display("Server non raggiungibile", "red", 0))
            return

        parsed = self._parse_entry(result)
        text = parsed.get("text", "")
        count = parsed.get("count", "0")

        if count == "1":
            color = "green"
            # Extract pax from text "Biglietto valido per N"
            try:
                self.pax_remaining = int(text.split("per ")[-1])
            except (ValueError, IndexError):
                self.pax_remaining = 1
            Clock.schedule_once(lambda dt, t=text, p=self.pax_remaining: self.on_display(t, "green", p))
            # Auto multi-pax sequence
            self._schedule_multi_pax()
        else:
            self.pax_remaining = 0
            color = "red"
            if "scaduto" in text.lower():
                color = "yellow"
            elif "vidimato" in text.lower():
                color = "orange"
            elif "annullato" in text.lower():
                color = "orange"
            Clock.schedule_once(lambda dt, t=text, c=color: self.on_display(t, c, 0))

    def _schedule_multi_pax(self):
        """Schedule automatic pass events for multi-pax tickets."""
        for i in range(self.pax_remaining):
            delay = 0.5 * (i + 1)
            Clock.schedule_once(lambda dt: self._bg(self._do_auto_pass), delay)

    def _do_auto_pass(self):
        self._do_pass()
        self.pax_remaining = max(0, self.pax_remaining - 1)
        Clock.schedule_once(lambda dt, p=self.pax_remaining: self.on_display(
            f"Passaggio... ({p} rimanenti)" if p > 0 else "Tutti passati",
            "green" if p > 0 else "gray",
            p
        ))

    def _do_pass(self):
        tid = self._next_id()
        xml = (f'<?xml version="1.0" encoding="UTF-8"?>'
               f'<cmf id="{tid}" addr="1" protocol="1.0">'
               f'<pass><act>l</act></pass></cmf>')
        self._send_recv(xml)

    def _do_panic(self, state):
        tid = self._next_id()
        txt = "Tornelli sbloccati" if state == "on" else ""
        state_xml = f'<state>{state}</state>'
        txt_xml = f'<txt>{txt}</txt>' if txt else ''
        xml = (f'<?xml version="1.0" encoding="UTF-8"?>'
               f'<cmf id="{tid}" addr="1" protocol="1.0">'
               f'<panic>{state_xml}{txt_xml}</panic></cmf>')
        result = self._send_recv(xml)
        msg = "PANIC: Tornelli sbloccati" if state == "on" else "Tornelli riattivati"
        Clock.schedule_once(lambda dt: self.on_display(msg, "orange" if state == "on" else "gray", 0))

    def _do_open(self):
        tid = self._next_id()
        xml = (f'<?xml version="1.0" encoding="UTF-8"?>'
               f'<cmf id="{tid}" addr="1" protocol="1.0">'
               f'<open><act>l</act><cnt>1</cnt><txt>Apertura forzata</txt></open></cmf>')
        self._send_recv(xml)
        Clock.schedule_once(lambda dt: self.on_display("Apertura forzata", "green", 0))

    def _parse_entry(self, xml_str):
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
        return {}
