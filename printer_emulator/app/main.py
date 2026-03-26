#!/usr/bin/env python3
"""
FGL Ticket Renderer — Boca printer emulator for mobile.
Renders FGL template commands to a ticket image.
"""

# Monkey-patch Kivy SDL2 text renderer to accept float font_size (bug in 2.3.1)
try:
    import kivy.core.text.text_sdl2 as _sdl2_mod
    _orig_render = _sdl2_mod._SurfaceContainer.render  # type: ignore
    def _patched_render(self, text, font_size, *args, **kwargs):
        return _orig_render(self, text, int(font_size), *args, **kwargs)
    _sdl2_mod._SurfaceContainer.render = _patched_render  # type: ignore
except Exception:
    pass  # not on SDL2 backend (desktop)

from kivy.app import App
from kivy.core.window import Window
from kivy.uix.screenmanager import ScreenManager

from screens.render_screen import RenderScreen


class FGLTicketApp(App):
    title = "FGL Ticket Renderer"

    def build(self):
        if Window:
            Window.clearcolor = (0.07, 0.07, 0.12, 1)
        sm = ScreenManager()
        sm.add_widget(RenderScreen(name="render"))
        return sm


if __name__ == "__main__":
    FGLTicketApp().run()
