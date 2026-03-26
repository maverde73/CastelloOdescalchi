"""
Cross-platform font resolver for FGL renderer.
Resolves font paths with priority: Verdana (optional) > Liberation Sans (bundled) > system paths.
"""

import os


def _first_existing(*paths):
    """Return the first path that exists on disk, or None."""
    for p in paths:
        if p and os.path.isfile(p):
            return p
    return None


def resolve_font_map(app_dir: str) -> tuple:
    """
    Return (FONT_MAP, BITMAP_FONT_MAP) with valid font paths for the current platform.

    Priority per font slot:
    1. Verdana (bundled manually by user) - pixel-perfect match to Boca printer
    2. Liberation Sans (bundled) - metrically compatible fallback
    3. System DejaVu Sans (Debian/Fedora) - good fallback
    """
    fonts_dir = os.path.join(app_dir, "fonts")

    def bundled(name):
        return os.path.join(fonts_dir, name)

    # --- Sans (Verdana on the printer) ---
    sans_regular = _first_existing(
        bundled("verdana.ttf"),
        bundled("LiberationSans-Regular.ttf"),
        "/usr/share/fonts/liberation-sans-fonts/LiberationSans-Regular.ttf",
        "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf",
        "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
        "/usr/share/fonts/dejavu-sans-fonts/DejaVuSans.ttf",
    )
    sans_bold = _first_existing(
        bundled("verdanab.ttf"),
        bundled("LiberationSans-Bold.ttf"),
        "/usr/share/fonts/liberation-sans-fonts/LiberationSans-Bold.ttf",
        "/usr/share/fonts/truetype/liberation/LiberationSans-Bold.ttf",
        "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
        "/usr/share/fonts/dejavu-sans-fonts/DejaVuSans-Bold.ttf",
    )
    sans_italic = _first_existing(
        bundled("verdanai.ttf"),
        bundled("LiberationSans-Italic.ttf"),
        "/usr/share/fonts/liberation-sans-fonts/LiberationSans-Italic.ttf",
        "/usr/share/fonts/truetype/liberation/LiberationSans-Italic.ttf",
        "/usr/share/fonts/truetype/dejavu/DejaVuSans-Oblique.ttf",
        "/usr/share/fonts/dejavu-sans-fonts/DejaVuSans-Oblique.ttf",
    )
    sans_bold_italic = _first_existing(
        bundled("verdanaz.ttf"),
        bundled("LiberationSans-BoldItalic.ttf"),
        "/usr/share/fonts/liberation-sans-fonts/LiberationSans-BoldItalic.ttf",
        "/usr/share/fonts/truetype/liberation/LiberationSans-BoldItalic.ttf",
        "/usr/share/fonts/truetype/dejavu/DejaVuSans-BoldOblique.ttf",
        "/usr/share/fonts/dejavu-sans-fonts/DejaVuSans-BoldOblique.ttf",
    )

    # --- Mono (Liberation Mono = original Boca font) ---
    mono_regular = _first_existing(
        bundled("LiberationMono-Regular.ttf"),
        "/usr/share/fonts/liberation-mono-fonts/LiberationMono-Regular.ttf",
        "/usr/share/fonts/truetype/liberation/LiberationMono-Regular.ttf",
    )
    mono_bold = _first_existing(
        bundled("LiberationMono-Bold.ttf"),
        "/usr/share/fonts/liberation-mono-fonts/LiberationMono-Bold.ttf",
        "/usr/share/fonts/truetype/liberation/LiberationMono-Bold.ttf",
    )
    mono_italic = _first_existing(
        bundled("LiberationMono-Italic.ttf"),
        "/usr/share/fonts/liberation-mono-fonts/LiberationMono-Italic.ttf",
        "/usr/share/fonts/truetype/liberation/LiberationMono-Italic.ttf",
    )
    mono_bold_italic = _first_existing(
        bundled("LiberationMono-BoldItalic.ttf"),
        "/usr/share/fonts/liberation-mono-fonts/LiberationMono-BoldItalic.ttf",
        "/usr/share/fonts/truetype/liberation/LiberationMono-BoldItalic.ttf",
    )

    # --- Mono for bitmap font (OCRB substitute) ---
    mono_bitmap = _first_existing(
        bundled("LiberationMono-Regular.ttf"),
        "/usr/share/fonts/liberation-mono-fonts/LiberationMono-Regular.ttf",
        "/usr/share/fonts/truetype/liberation/LiberationMono-Regular.ttf",
    )

    font_map = {
        1: sans_regular,        # Comic Sans -> sans
        2: sans_bold,           # Comic Sans Bold -> sans bold
        3: sans_regular,        # Verdana -> sans
        4: sans_bold,           # Verdana Bold -> sans bold
        5: sans_italic,         # Verdana Italic -> sans italic
        6: sans_bold_italic,    # Verdana Bold Italic -> sans bold italic
        7: mono_regular,        # Liberation Mono Regular (original)
        8: mono_bold,           # Liberation Mono Bold (original)
        9: mono_italic,         # Liberation Mono Italic (original)
        10: mono_bold_italic,   # Liberation Mono Bold Italic (original)
    }

    bitmap_font_map = {
        3: (mono_bitmap, 10),   # F3 OCRB substitute
    }

    return font_map, bitmap_font_map
