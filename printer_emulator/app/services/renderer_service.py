"""
Thread-safe wrapper around fgl_renderer.
Patches fonts at runtime, renders in background thread, returns PNG path via callback.
"""

import os
import sys
import threading

# Add app dir and parent dir to path for fgl_renderer import
_android_app = os.environ.get("ANDROID_ARGUMENT")
if _android_app:
    _app_dir = _android_app
else:
    _app_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
_renderer_dir = os.path.dirname(_app_dir)
for _p in (_app_dir, _renderer_dir):
    if _p not in sys.path:
        sys.path.insert(0, _p)

import fgl_renderer
from services.font_resolver import resolve_font_map

_fonts_patched = False


def _patch_fonts(app_dir: str) -> None:
    global _fonts_patched
    if _fonts_patched:
        return
    font_map, bitmap_font_map = resolve_font_map(app_dir)
    fgl_renderer.FONT_MAP.update(
        {k: v for k, v in font_map.items() if v is not None}
    )
    for k, v in bitmap_font_map.items():
        if v[0] is not None:
            fgl_renderer.BITMAP_FONT_MAP[k] = v
    _fonts_patched = True


def render_ticket(template: str, values: dict, app_dir: str, callback) -> None:
    """
    Render FGL template in a background thread.
    callback(png_path: str | None, error: str | None) is called on the main thread via Clock.
    """
    from kivy.clock import Clock

    def _work():
        try:
            _patch_fonts(app_dir)
            filled = fgl_renderer.substitute_placeholders(template, values)
            renderer = fgl_renderer.FGLRenderer()
            img = renderer.parse_and_render(filled)
            # Rotate to reading orientation (landscape)
            img_rotated = img.rotate(-90, expand=True)
            # Save to writable cache dir (user_data_dir on Android)
            from kivy.app import App
            running = App.get_running_app()
            if running:
                cache_dir = os.path.join(running.user_data_dir, ".cache")
            else:
                cache_dir = os.path.join(app_dir, ".cache")
            os.makedirs(cache_dir, exist_ok=True)
            out_path = os.path.join(cache_dir, "ticket_render.png")
            img_rotated.save(out_path, "PNG", dpi=(200, 200))
            Clock.schedule_once(lambda dt: callback(out_path, None))
        except Exception as e:
            Clock.schedule_once(lambda dt: callback(None, str(e)))

    threading.Thread(target=_work, daemon=True).start()
