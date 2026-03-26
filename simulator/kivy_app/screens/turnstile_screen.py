"""Turnstile simulator screen."""

import socket
from kivy.uix.screenmanager import Screen
from kivy.lang import Builder
from kivy.properties import StringProperty, ListProperty, NumericProperty
from services.turnstile_service import TurnstileService
import config_manager


def get_local_ip():
    """Get the local network IP (not 127.0.0.1)."""
    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        s.connect(("8.8.8.8", 80))
        ip = s.getsockname()[0]
        s.close()
        return ip
    except Exception:
        return "?.?.?.?"

Builder.load_string("""
<TurnstileScreen>:
    name: 'turnstile'
    BoxLayout:
        orientation: 'vertical'
        spacing: 0

        # Top bar
        BoxLayout:
            size_hint_y: None
            height: '40dp'
            padding: [10, 5]
            canvas.before:
                Color:
                    rgba: 0.1, 0.1, 0.15, 1
                Rectangle:
                    pos: self.pos
                    size: self.size
            Button:
                text: '<  Home'
                size_hint_x: None
                width: '80dp'
                font_size: '13sp'
                background_color: 0, 0, 0, 0
                color: 0.6, 0.6, 0.6, 1
                on_release: root.go_home()
            Label:
                text: 'TORNELLO SIMULATO'
                font_size: '13sp'
                bold: True
                color: 1, 0.75, 0.2, 1
            BoxLayout:
                size_hint_x: None
                width: '200dp'
                spacing: 5
                Widget:
                    size_hint_x: None
                    width: '8dp'
                    canvas:
                        Color:
                            rgba: root.conn_color
                        Ellipse:
                            pos: self.center_x - 4, self.center_y - 4
                            size: 8, 8
                Label:
                    text: root.conn_text
                    font_size: '11sp'
                    color: 0.5, 0.5, 0.5, 1
                    text_size: self.size
                    halign: 'left'
                    valign: 'middle'

        # My address info
        BoxLayout:
            size_hint_y: None
            height: '22dp'
            padding: [10, 2]
            canvas.before:
                Color:
                    rgba: 0.08, 0.12, 0.08, 1
                Rectangle:
                    pos: self.pos
                    size: self.size
            Label:
                text: root.my_address
                font_size: '11sp'
                color: 0.4, 0.8, 0.4, 1
                halign: 'center'
                text_size: self.size
                valign: 'middle'

        # Server target config
        BoxLayout:
            size_hint_y: None
            height: '35dp'
            padding: [10, 3]
            spacing: 5
            Label:
                text: 'Server:'
                font_size: '12sp'
                color: 0.5, 0.5, 0.5, 1
                size_hint_x: None
                width: '50dp'
            TextInput:
                id: server_host_input
                text: root.server_host
                font_size: '12sp'
                multiline: False
                background_color: 0.15, 0.15, 0.2, 1
                foreground_color: 1, 1, 1, 1
                padding: [8, 5]
                on_text_validate: root.apply_server_config()
            Label:
                text: ':'
                font_size: '12sp'
                color: 0.5, 0.5, 0.5, 1
                size_hint_x: None
                width: '10dp'
            TextInput:
                id: server_port_input
                text: root.server_port
                font_size: '12sp'
                multiline: False
                input_filter: 'int'
                background_color: 0.15, 0.15, 0.2, 1
                foreground_color: 1, 1, 1, 1
                padding: [8, 5]
                size_hint_x: None
                width: '70dp'
                on_text_validate: root.apply_server_config()
            Button:
                text: 'Applica'
                font_size: '11sp'
                size_hint_x: None
                width: '70dp'
                background_color: 0.3, 0.5, 0.3, 1
                on_release: root.apply_server_config()

        # Display area
        BoxLayout:
            size_hint_y: None
            height: '100dp'
            padding: 15
            canvas.before:
                Color:
                    rgba: root.display_bg
                RoundedRectangle:
                    pos: self.x + 10, self.y + 5
                    size: self.width - 20, self.height - 10
                    radius: [8]
            BoxLayout:
                orientation: 'vertical'
                padding: [5, 0]
                Label:
                    text: root.display_text
                    font_size: '20sp'
                    bold: True
                    color: 1, 1, 1, 1
                Label:
                    text: root.display_pax
                    font_size: '14sp'
                    color: 0.8, 0.8, 0.8, 0.8
                    size_hint_y: None
                    height: '25dp'

        # Emit controls
        BoxLayout:
            size_hint_y: None
            height: '35dp'
            padding: [10, 3]
            spacing: 5
            Label:
                text: 'Pax:'
                font_size: '11sp'
                color: 0.5, 0.5, 0.5, 1
                size_hint_x: None
                width: '30dp'
            TextInput:
                id: emit_pax
                text: '1'
                font_size: '12sp'
                multiline: False
                input_filter: 'int'
                background_color: 0.15, 0.15, 0.2, 1
                foreground_color: 1, 1, 1, 1
                padding: [8, 5]
                size_hint_x: None
                width: '40dp'
            Label:
                text: 'Tipo:'
                font_size: '11sp'
                color: 0.5, 0.5, 0.5, 1
                size_hint_x: None
                width: '35dp'
            TextInput:
                id: emit_tipo
                text: '1'
                font_size: '12sp'
                multiline: False
                input_filter: 'int'
                background_color: 0.15, 0.15, 0.2, 1
                foreground_color: 1, 1, 1, 1
                padding: [8, 5]
                size_hint_x: None
                width: '40dp'
            Button:
                text: 'Genera Biglietto'
                font_size: '12sp'
                bold: True
                background_color: 0.6, 0.4, 0.1, 1
                on_release: root.do_emit()

        # Scan controls
        BoxLayout:
            size_hint_y: None
            height: '90dp'
            padding: 10
            spacing: 8
            orientation: 'vertical'

            BoxLayout:
                spacing: 8
                TextInput:
                    id: barcode_input
                    hint_text: 'Barcode...'
                    font_size: '14sp'
                    multiline: False
                    background_color: 0.15, 0.15, 0.2, 1
                    foreground_color: 1, 1, 1, 1
                    cursor_color: 1, 0.75, 0.2, 1
                    padding: [10, 8]
                Button:
                    text: 'Scansiona'
                    size_hint_x: None
                    width: '110dp'
                    font_size: '14sp'
                    bold: True
                    background_color: 0.15, 0.6, 0.25, 1
                    on_release: root.do_scan()

            BoxLayout:
                spacing: 8
                Button:
                    text: 'Pass'
                    font_size: '12sp'
                    background_color: 0.2, 0.4, 0.7, 1
                    on_release: root.do_pass()
                Button:
                    text: 'Test'
                    font_size: '12sp'
                    background_color: 0.3, 0.3, 0.3, 1
                    on_release: root.do_test()
                Button:
                    text: root.panic_text
                    font_size: '12sp'
                    background_color: 0.7, 0.2, 0.1, 1
                    on_release: root.do_panic()
                Button:
                    text: 'Open'
                    font_size: '12sp'
                    background_color: 0.5, 0.4, 0.1, 1
                    on_release: root.do_open()

        # Log
        BoxLayout:
            orientation: 'vertical'
            padding: [10, 5]
            Label:
                text: 'Protocollo UDP'
                font_size: '11sp'
                color: 0.5, 0.5, 0.5, 1
                size_hint_y: None
                height: '20dp'
                halign: 'left'
                text_size: self.size
            ScrollView:
                canvas.before:
                    Color:
                        rgba: 0.05, 0.05, 0.08, 1
                    RoundedRectangle:
                        pos: self.pos
                        size: self.size
                        radius: [6]
                Label:
                    id: log_label
                    text: root.log_text
                    font_size: '10sp'
                    color: 0.6, 0.7, 0.6, 1
                    markup: True
                    size_hint_y: None
                    height: self.texture_size[1] + 20
                    text_size: self.width - 10, None
                    padding: [5, 5]
                    valign: 'top'
""")

COLOR_MAP = {
    "green": (0.1, 0.35, 0.15, 1),
    "red": (0.4, 0.1, 0.1, 1),
    "yellow": (0.4, 0.35, 0.1, 1),
    "orange": (0.4, 0.25, 0.1, 1),
    "gray": (0.15, 0.15, 0.2, 1),
}


class TurnstileScreen(Screen):
    display_text = StringProperty("In attesa...")
    display_pax = StringProperty("")
    display_bg = ListProperty([0.15, 0.15, 0.2, 1])
    conn_color = ListProperty([0.5, 0.5, 0.5, 1])
    conn_text = StringProperty("Non connesso")
    log_text = StringProperty("Log protocollo UDP...")
    panic_text = StringProperty("Panic ON")
    my_address = StringProperty("")
    server_host = StringProperty("127.0.0.1")
    server_port = StringProperty("10000")

    _panic_state = False
    _log_lines = []
    _service = None

    def on_enter(self):
        cfg = config_manager.get_turnstile()
        local_ip = get_local_ip()
        udp_port = cfg.get("udp_port", 10001)
        self.my_address = f"Per collegarti a questo tornello usa  {local_ip}:{udp_port}"
        self.server_host = cfg.get("server_host", "127.0.0.1")
        self.server_port = str(cfg.get("server_udp_port", 10000))

        if self._service is None:
            self._service = TurnstileService(
                on_display=self._on_display,
                on_log=self._on_log,
                on_connection=self._on_connection,
            )
            self._service.start()

    def on_leave(self):
        if self._service:
            self._service.stop()
            self._service = None

    def go_home(self):
        self.manager.transition.direction = 'right'
        self.manager.current = 'home'

    def do_emit(self):
        """Generate a barcode via the dashboard API (which inserts into DB)."""
        import threading
        pax = int(self.ids.emit_pax.text or 1)
        tipo = int(self.ids.emit_tipo.text or 1)

        self.display_text = "Generazione..."
        self.display_bg = COLOR_MAP["gray"]

        def _emit():
            from datetime import datetime
            from kivy.clock import Clock as _Clock
            # Try HTTP call to dashboard API to emit (inserts into DB)
            dashboard_host = self.ids.server_host_input.text.strip() or "127.0.0.1"
            try:
                import urllib.request, json
                url = f"http://{dashboard_host}:8000/api/emit"
                print(f"[EMIT] Calling {url} pax={pax} tipo={tipo}")
                data = json.dumps({"pax": pax, "tipo": tipo}).encode()
                req = urllib.request.Request(url, data=data, headers={"Content-Type": "application/json"})
                resp = urllib.request.urlopen(req, timeout=5)
                ticket = json.loads(resp.read())
                code = str(ticket["code"])
                print(f"[EMIT] OK: code={code}")
                _Clock.schedule_once(lambda dt: self._on_emit_ok(code, pax, tipo))
                return
            except Exception as ex:
                print(f"[EMIT] HTTP failed: {type(ex).__name__}: {ex}")

            # Fallback: generate barcode locally (won't be in DB)
            from barcode import encode
            try:
                code = encode(pax=pax, date=datetime.now(), cont=0, tipo=tipo)
                _Clock.schedule_once(lambda dt: self._on_emit_fallback(str(code), pax, tipo))
            except Exception as ex:
                _Clock.schedule_once(lambda dt: self._on_emit_error(str(ex)))

        threading.Thread(target=_emit, daemon=True).start()

    def _on_emit_ok(self, code, pax, tipo):
        from datetime import datetime
        self.ids.barcode_input.text = code
        self.display_text = f"Biglietto emesso: {code}"
        self.display_bg = COLOR_MAP["green"]
        self._on_log("emit", "", f"Emesso (DB) barcode={code} pax={pax} tipo={tipo}", datetime.now().isoformat())

    def _on_emit_fallback(self, code, pax, tipo):
        from datetime import datetime
        self.ids.barcode_input.text = code
        self.display_text = f"Generato (locale, no DB): {code}"
        self.display_bg = [0.4, 0.35, 0.1, 1]
        self._on_log("emit", "", f"Locale (no DB) barcode={code} pax={pax}", datetime.now().isoformat())

    def _on_emit_error(self, err):
        self.display_text = f"Errore: {err}"
        self.display_bg = COLOR_MAP["red"]

    def apply_server_config(self):
        host = self.ids.server_host_input.text.strip()
        port = self.ids.server_port_input.text.strip()
        if host and port:
            self.server_host = host
            self.server_port = port
            if self._service:
                self._service.update_config(server_host=host, server_port=int(port))
            self.display_text = f"Server: {host}:{port}"
            self.display_bg = COLOR_MAP["gray"]

    def do_scan(self):
        bc = self.ids.barcode_input.text.strip()
        if bc and self._service:
            self.display_text = "Validazione..."
            self.display_bg = COLOR_MAP["gray"]
            self._service.scan(bc)

    def do_pass(self):
        if self._service:
            self._service.send_pass()

    def do_test(self):
        if self._service:
            self._service._bg(self._service._heartbeat)

    def do_panic(self):
        if self._service:
            self._panic_state = not self._panic_state
            state = "on" if self._panic_state else "off"
            self.panic_text = "Panic OFF" if self._panic_state else "Panic ON"
            self._service.send_panic(state)

    def do_open(self):
        if self._service:
            self._service.force_open()

    def _on_display(self, text, color, pax):
        self.display_text = text
        self.display_bg = COLOR_MAP.get(color, COLOR_MAP["gray"])
        self.display_pax = f"Pax rimanenti: {pax}" if pax > 0 else ""

    def _on_log(self, direction, addr, xml, timestamp):
        ts = timestamp[11:23] if len(timestamp) > 23 else timestamp
        if direction == "sent":
            line = f"[color=6699ff]{ts} >>> SENT {addr}[/color]\n  {self._short_xml(xml)}"
        elif direction == "recv":
            line = f"[color=66cccc]{ts} <<< RECV {addr}[/color]\n  {self._short_xml(xml)}"
        else:
            line = f"[color=ff6666]{ts} TIMEOUT[/color]"

        self._log_lines.append(line)
        if len(self._log_lines) > 100:
            self._log_lines = self._log_lines[-50:]
        self.log_text = "\n".join(self._log_lines)

    def _on_connection(self, connected):
        if connected:
            self.conn_color = [0.2, 0.8, 0.3, 1]
            cfg = config_manager.get_turnstile()
            self.conn_text = f"{cfg['server_host']}:{cfg['server_udp_port']}"
        else:
            self.conn_color = [0.8, 0.2, 0.2, 1]
            self.conn_text = "Non connesso"

    def _short_xml(self, xml):
        if not xml:
            return "(vuoto)"
        return xml.replace('<?xml version="1.0" encoding="UTF-8"?>', '').strip()[:120]
