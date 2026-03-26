"""
Castello Odescalchi Simulator — Kivy App
Dual-mode: Turnstile device simulator OR Validation Server.
"""

import sys
import os

# Add simulator/ to path so we can import barcode, ticket_validator, etc.
sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), ".."))

from kivy.app import App
from kivy.lang import Builder
from kivy.uix.screenmanager import ScreenManager, SlideTransition

from screens.home_screen import HomeScreen
from screens.turnstile_screen import TurnstileScreen
from screens.server_screen import ServerScreen

KV = """
#:import C kivy.utils.get_color_from_hex

<RoundedButton@Button>:
    background_color: 0, 0, 0, 0
    canvas.before:
        Color:
            rgba: self.bg_color if hasattr(self, 'bg_color') else (0.3, 0.3, 0.3, 1)
        RoundedRectangle:
            pos: self.pos
            size: self.size
            radius: [8]

ScreenManager:
    HomeScreen:
    TurnstileScreen:
    ServerScreen:
"""


class CastleSimulatorApp(App):
    title = "Castello Odescalchi Simulator"

    def build(self):
        from kivy.core.window import Window
        Window.clearcolor = (0.07, 0.07, 0.12, 1)  # Dark background

        self.sm = Builder.load_string(KV)
        return self.sm


if __name__ == "__main__":
    CastleSimulatorApp().run()
