"""Validation Server screen — simulates GestioneTornelloREA."""

from kivy.uix.screenmanager import Screen
from kivy.lang import Builder
from kivy.properties import StringProperty, ListProperty, NumericProperty
from services.server_service import ServerService

Builder.load_string("""
<ServerScreen>:
    name: 'server'
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
                text: 'VALIDATION SERVER'
                font_size: '13sp'
                bold: True
                color: 0.4, 0.6, 1, 1
            BoxLayout:
                size_hint_x: None
                width: '180dp'
                spacing: 5
                Widget:
                    size_hint_x: None
                    width: '8dp'
                    canvas:
                        Color:
                            rgba: root.status_color
                        Ellipse:
                            pos: self.center_x - 4, self.center_y - 4
                            size: 8, 8
                Label:
                    text: root.status_text
                    font_size: '11sp'
                    color: 0.5, 0.5, 0.5, 1
                    text_size: self.size
                    halign: 'left'
                    valign: 'middle'

        # Controls row
        BoxLayout:
            size_hint_y: None
            height: '45dp'
            padding: [10, 5]
            spacing: 8
            Button:
                text: root.start_btn_text
                font_size: '13sp'
                bold: True
                background_color: (0.15, 0.5, 0.2, 1) if not root._running else (0.5, 0.15, 0.1, 1)
                on_release: root.toggle_service()
                size_hint_x: None
                width: '90dp'
            Button:
                text: 'Open'
                font_size: '12sp'
                background_color: 0.5, 0.4, 0.1, 1
                on_release: root.do_open()
                size_hint_x: None
                width: '70dp'
            # Direction
            Label:
                text: 'Dir:'
                font_size: '11sp'
                color: 0.5, 0.5, 0.5, 1
                size_hint_x: None
                width: '30dp'
            Button:
                text: root.direction_text
                font_size: '11sp'
                background_color: 0.25, 0.25, 0.35, 1
                on_release: root.cycle_direction()
                size_hint_x: None
                width: '70dp'
            # Spacer
            Widget:

        # Manual check row
        BoxLayout:
            size_hint_y: None
            height: '40dp'
            padding: [10, 3]
            spacing: 8
            TextInput:
                id: manual_barcode
                hint_text: 'Barcode per verifica manuale...'
                font_size: '13sp'
                multiline: False
                background_color: 0.15, 0.15, 0.2, 1
                foreground_color: 1, 1, 1, 1
                cursor_color: 0.4, 0.6, 1, 1
                padding: [10, 6]
            Button:
                text: 'Verifica'
                font_size: '12sp'
                background_color: 0.3, 0.3, 0.5, 1
                on_release: root.do_manual_check()
                size_hint_x: None
                width: '90dp'

        # Result display
        BoxLayout:
            size_hint_y: None
            height: '70dp'
            padding: 10
            canvas.before:
                Color:
                    rgba: root.result_bg
                RoundedRectangle:
                    pos: self.x + 10, self.y + 3
                    size: self.width - 20, self.height - 6
                    radius: [6]
            BoxLayout:
                orientation: 'vertical'
                padding: [5, 0]
                Label:
                    text: root.result_state
                    font_size: '16sp'
                    bold: True
                    color: 1, 1, 1, 1
                Label:
                    text: root.result_detail
                    font_size: '11sp'
                    color: 0.7, 0.7, 0.7, 1
                    size_hint_y: None
                    height: '20dp'

        # Log
        BoxLayout:
            orientation: 'vertical'
            padding: [10, 5]
            Label:
                text: root.log_header
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

RESULT_COLORS = {
    "Valido": (0.1, 0.35, 0.15, 1),
    "Invalido": (0.4, 0.1, 0.1, 1),
    "Scaduto": (0.4, 0.35, 0.1, 1),
    "Validato": (0.4, 0.25, 0.1, 1),
    "Annullato": (0.4, 0.25, 0.1, 1),
}

DIRECTIONS = ["Left", "Right", "Denied"]


class ServerScreen(Screen):
    status_color = ListProperty([0.5, 0.5, 0.5, 1])
    status_text = StringProperty("Fermo")
    start_btn_text = StringProperty("Start")
    direction_text = StringProperty("Left")
    result_state = StringProperty("In attesa...")
    result_detail = StringProperty("")
    result_bg = ListProperty([0.15, 0.15, 0.2, 1])
    log_text = StringProperty("Log server...")
    log_header = StringProperty("Log | Messaggi: 0")

    _running = False
    _direction_idx = 0
    _log_lines = []
    _service = None

    def on_enter(self):
        if self._service is None:
            self._service = ServerService(
                on_log=self._on_log,
                on_validation=self._on_validation,
                on_status=self._on_status,
            )

    def on_leave(self):
        if self._service and self._service.running:
            self._service.stop()
        self._service = None

    def go_home(self):
        self.manager.transition.direction = 'right'
        self.manager.current = 'home'

    def toggle_service(self):
        if not self._service:
            return
        if self._running:
            self._service.stop()
        else:
            self._service.start()

    def cycle_direction(self):
        self._direction_idx = (self._direction_idx + 1) % 3
        self.direction_text = DIRECTIONS[self._direction_idx]
        if self._service:
            self._service.set_direction(self._direction_idx)

    def do_open(self):
        if self._service:
            self._service.send_open()

    def do_manual_check(self):
        bc = self.ids.manual_barcode.text.strip()
        if bc and self._service:
            self.result_state = "Verifica..."
            self.result_bg = [0.15, 0.15, 0.2, 1]
            self._service.manual_check(bc)

    def _on_status(self, running, msg_count):
        self._running = running
        if running:
            self.status_color = [0.2, 0.8, 0.3, 1]
            self.status_text = f"Attivo :{self._service.udp_port}"
            self.start_btn_text = "Stop"
        else:
            self.status_color = [0.8, 0.2, 0.2, 1]
            self.status_text = "Fermo"
            self.start_btn_text = "Start"
        self.log_header = f"Log | Messaggi: {msg_count}"

    def _on_validation(self, result):
        state = result.get("state", "Invalido")
        self.result_state = f"{state}"
        barcode = result.get("barcode", "")
        pax = result.get("pax", 0)
        detail = result.get("detail", "")
        self.result_detail = f"Barcode: {barcode[:16]}... | Pax: {pax} | {detail}"
        self.result_bg = RESULT_COLORS.get(state, RESULT_COLORS["Invalido"])

    def _on_log(self, direction, addr, xml, timestamp):
        ts = timestamp[11:23] if len(timestamp) > 23 else timestamp
        if direction == "recv":
            line = f"[color=66cccc]{ts} <<< RECV {addr}[/color]\n  {self._short_xml(xml)}"
        elif direction == "sent":
            line = f"[color=6699ff]{ts} >>> SENT {addr}[/color]\n  {self._short_xml(xml)}"
        elif direction == "step":
            # Validation step
            color = "aaaaaa"
            if "RESULT:" in xml:
                if "VALIDO" in xml and "INVALIDO" not in xml and "VALIDATO" not in xml:
                    color = "66ff66"
                elif "INVALIDO" in xml:
                    color = "ff6666"
                elif "SCADUTO" in xml:
                    color = "ffcc66"
                else:
                    color = "ffaa66"
            line = f"[color={color}]{ts} {xml.strip()}[/color]"
        elif direction == "error":
            line = f"[color=ff4444]{ts} {xml}[/color]"
        else:
            line = f"{ts} {direction} {xml}"

        self._log_lines.append(line)
        if len(self._log_lines) > 100:
            self._log_lines = self._log_lines[-50:]
        self.log_text = "\n".join(self._log_lines)

    def _short_xml(self, xml):
        if not xml:
            return "(vuoto)"
        return xml.replace('<?xml version="1.0" encoding="UTF-8"?>', '').strip()[:120]
