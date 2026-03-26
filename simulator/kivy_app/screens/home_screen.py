"""Home screen — mode selection."""

from kivy.uix.screenmanager import Screen
from kivy.lang import Builder
from kivy.properties import StringProperty
import config_manager

Builder.load_string("""
<HomeScreen>:
    name: 'home'
    BoxLayout:
        orientation: 'vertical'
        padding: 30
        spacing: 20

        # Title
        Label:
            text: 'Castello Odescalchi'
            font_size: '28sp'
            color: 1, 0.75, 0.2, 1
            bold: True
            size_hint_y: None
            height: '50dp'
        Label:
            text: 'Simulator'
            font_size: '18sp'
            color: 0.6, 0.6, 0.6, 1
            size_hint_y: None
            height: '30dp'

        Widget:
            size_hint_y: 0.1

        # Turnstile card
        BoxLayout:
            orientation: 'vertical'
            size_hint_y: None
            height: '120dp'
            canvas.before:
                Color:
                    rgba: 0.12, 0.18, 0.12, 1
                RoundedRectangle:
                    pos: self.pos
                    size: self.size
                    radius: [12]
            padding: 15
            spacing: 5
            Button:
                text: 'Tornello Simulato'
                font_size: '18sp'
                bold: True
                color: 0.2, 0.9, 0.4, 1
                background_color: 0, 0, 0, 0
                on_release: root.go_turnstile()
                size_hint_y: None
                height: '40dp'
            Label:
                text: root.turnstile_info
                font_size: '12sp'
                color: 0.5, 0.7, 0.5, 1
            Label:
                text: 'Simula il dispositivo fisico REA/DCP'
                font_size: '11sp'
                color: 0.4, 0.4, 0.4, 1

        Widget:
            size_hint_y: 0.05

        # Server card
        BoxLayout:
            orientation: 'vertical'
            size_hint_y: None
            height: '120dp'
            canvas.before:
                Color:
                    rgba: 0.12, 0.12, 0.18, 1
                RoundedRectangle:
                    pos: self.pos
                    size: self.size
                    radius: [12]
            padding: 15
            spacing: 5
            Button:
                text: 'Validation Server'
                font_size: '18sp'
                bold: True
                color: 0.4, 0.6, 1, 1
                background_color: 0, 0, 0, 0
                on_release: root.go_server()
                size_hint_y: None
                height: '40dp'
            Label:
                text: root.server_info
                font_size: '12sp'
                color: 0.5, 0.5, 0.7, 1
            Label:
                text: 'Simula GestioneTornelloREA.exe'
                font_size: '11sp'
                color: 0.4, 0.4, 0.4, 1

        Widget:
            size_hint_y: 0.3
""")


class HomeScreen(Screen):
    turnstile_info = StringProperty("...")
    server_info = StringProperty("...")

    def on_enter(self):
        try:
            tc = config_manager.get_turnstile()
            self.turnstile_info = f"UDP :{tc['udp_port']} → {tc['server_host']}:{tc['server_udp_port']}"
        except Exception:
            self.turnstile_info = "Config non disponibile"
        try:
            sc = config_manager.get_server()
            self.server_info = f"UDP :{sc['udp_port']}"
        except Exception:
            self.server_info = "Config non disponibile"

    def go_turnstile(self):
        self.manager.transition.direction = 'left'
        self.manager.current = 'turnstile'

    def go_server(self):
        self.manager.transition.direction = 'left'
        self.manager.current = 'server'
