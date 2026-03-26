#!/usr/bin/env python3
"""
FGL Ticket Renderer — Boca Systems Printer Simulator
Renders FGL template commands to a PNG image.
"""

import re
import math
from PIL import Image, ImageDraw, ImageFont
from datetime import datetime

# --- Configuration ---

DPI = 200
TICKET_ROWS = 962      # height in dots (Y)
TICKET_COLS = 624       # width in dots (X)
BG_COLOR = (255, 255, 255)
FG_COLOR = (0, 0, 0)

# Font mapping: RTF ID -> (font_file, is_bold)
# DejaVu Sans ≈ Verdana, DejaVu Sans Bold ≈ Verdana Bold
FONT_MAP = {
    1: "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",          # Comic Sans → DejaVu Sans
    2: "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",     # Comic Sans Bold
    3: "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",          # Verdana → DejaVu Sans
    4: "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",     # Verdana Bold → DejaVu Sans Bold
    5: "/usr/share/fonts/truetype/dejavu/DejaVuSans-Oblique.ttf",  # Verdana Italic
    6: "/usr/share/fonts/truetype/dejavu/DejaVuSans-BoldOblique.ttf",  # Verdana Bold Italic
    7: "/usr/share/fonts/truetype/liberation/LiberationMono-Regular.ttf",
    8: "/usr/share/fonts/truetype/liberation/LiberationMono-Bold.ttf",
    9: "/usr/share/fonts/truetype/liberation/LiberationMono-Italic.ttf",
    10: "/usr/share/fonts/truetype/liberation/LiberationMono-BoldItalic.ttf",
}

BITMAP_FONT_MAP = {
    3: ("/usr/share/fonts/truetype/dejavu/DejaVuSansMono.ttf", 10),  # F3 OCRB ~10px
}


def pt_to_px(pt):
    """Convert typographic points to pixels at 200 DPI."""
    return max(1, round(pt * DPI / 72))


# --- Code 39 Barcode Generator ---

CODE39_CHARS = {
    '0': 'nnnwwnwnn', '1': 'wnnwnnnnw', '2': 'nnwwnnnnw', '3': 'wnwwnnnn',
    '4': 'nnnwwnnnw', '5': 'wnnwwnnn', '6': 'nnwwwnnn', '7': 'nnnwnnwnw',
    '8': 'wnnwnnwn', '9': 'nnwwnnwn',  # simplified - not using full set
    'A': 'wnnnnwnnw', 'B': 'nnwnnwnnw', 'C': 'wnwnnwnn', 'D': 'nnnnwwnnw',
    'E': 'wnnnwwnn', 'F': 'nnwnwwnn', 'G': 'nnnnnwwnw', 'H': 'wnnnnwwn',
    'I': 'nnwnnwwn', 'J': 'nnnnwwwn', 'K': 'wnnnnnnww', 'L': 'nnwnnnnww',
    'M': 'wnwnnnnw', 'N': 'nnnnwnnww', 'O': 'wnnnwnnw', 'P': 'nnwnwnnw',
    'Q': 'nnnnnnwww', 'R': 'wnnnnnww', 'S': 'nnwnnnww', 'T': 'nnnnwnww',
    'U': 'wwnnnnnnw', 'V': 'nwwnnnnnw', 'W': 'wwwnnnnnn', 'X': 'nwnnwnnnw',  # approx
    'Y': 'wwnnwnnnn', 'Z': 'nwwnwnnnn',
    '-': 'nwnnnnwnw', '.': 'wwnnnnnw', ' ': 'nwwnnnnnw',
    '$': 'nwnwnwnn', '/': 'nwnwnnnw', '+': 'nwnnnwnw', '%': 'nnnwnwnw',
    '*': 'nwnnwnwnn',  # start/stop
}

# More accurate Code 39 patterns (9 elements: 5 bars + 4 spaces, n=narrow w=wide)
CODE39_PATTERNS = {
    '0': 'nnnwwnwnn', '1': 'wnnwnnnnw', '2': 'nnwwnnnnw', '3': 'wnwwnnnn' + 'n',
    '4': 'nnnwwnnnw', '5': 'wnnwwnnn' + 'n', '6': 'nnwwwnnn' + 'n', '7': 'nnnwnnwnw',
    '8': 'wnnwnnwnn', '9': 'nnwwnnwnn',
    '*': 'nwnnwnwnn',
}
# For simplicity, just render bars as alternating black/white


def render_code39_image(data, bar_height, expansion=1):
    """
    Render a Code 39 barcode as a PIL Image.
    Returns image with bars vertical (picket fence style).
    Caller rotates for ladder orientation.
    """
    # Simple Code 39: narrow = 1 unit, wide = 3 units, inter-char gap = 1 unit
    narrow = expansion
    wide = expansion * 3

    # Build binary pattern
    chars = list(data.replace('*', ''))
    all_chars = ['*'] + chars + ['*']

    modules = []
    for i, ch in enumerate(all_chars):
        pattern = CODE39_PATTERNS.get(ch.upper(), CODE39_PATTERNS.get('0'))
        if pattern is None:
            pattern = 'nnnwwnwnn'  # fallback to '0'
        for j, elem in enumerate(pattern):
            width = wide if elem == 'w' else narrow
            is_bar = (j % 2 == 0)  # odd positions are spaces
            modules.append((width, is_bar))
        # inter-character gap
        if i < len(all_chars) - 1:
            modules.append((narrow, False))

    total_width = sum(w for w, _ in modules)
    img = Image.new('1', (total_width, bar_height), 1)  # 1-bit, white bg
    draw = ImageDraw.Draw(img)

    x = 0
    for width, is_bar in modules:
        if is_bar:
            draw.rectangle([x, 0, x + width - 1, bar_height - 1], fill=0)
        x += width

    return img


# --- FGL Parser & Renderer ---

class FGLRenderer:
    def __init__(self, width=TICKET_COLS, height=TICKET_ROWS):
        self.width = width
        self.height = height
        self.img = Image.new('RGB', (width, height), BG_COLOR)
        self.draw = ImageDraw.Draw(self.img)

        # Printer state
        self.row = 0
        self.col = 0
        self.rotation = 'NR'
        self.font_id = None       # RTF font id
        self.font_pt = 10         # point size
        self.font_type = 'rtf'    # 'rtf' or 'bitmap'
        self.bitmap_font = 3      # F3 default
        self.hw_h = 1
        self.hw_w = 1
        self.line_thickness = 1
        self.barcode_expansion = 1
        self.codepage = 3         # default: Win Latin I (1252)

        self._default_font_path = FONT_MAP[4]
        self._default_font_size = pt_to_px(10)

    def _get_font(self, size_override=None):
        """Get PIL font for current state."""
        if self.font_type == 'rtf' and self.font_id in FONT_MAP:
            path = FONT_MAP[self.font_id]
            size = size_override or pt_to_px(self.font_pt)
        elif self.font_type == 'bitmap' and self.bitmap_font in BITMAP_FONT_MAP:
            path, size = BITMAP_FONT_MAP[self.bitmap_font]
            size = size_override or size
        else:
            path = self._default_font_path
            size = size_override or self._default_font_size

        # Apply HW multiplier to size
        size = int(size * max(self.hw_h, self.hw_w))

        try:
            return ImageFont.truetype(path, size)
        except Exception:
            return ImageFont.load_default()

    def _draw_text(self, text, row, col):
        """Draw text at (row, col) with current rotation."""
        if not text:
            return

        font = self._get_font()

        # Render text on temporary image to get size and handle rotation
        # First measure
        bbox = font.getbbox(text)
        tw = bbox[2] - bbox[0]
        th = bbox[3] - bbox[1]

        # Add padding
        tmp = Image.new('RGBA', (tw + 4, th + 4), (255, 255, 255, 0))
        tmp_draw = ImageDraw.Draw(tmp)
        tmp_draw.text((-bbox[0], -bbox[1]), text, font=font, fill=FG_COLOR)

        if self.rotation == 'NR':
            # No rotation: paste at (col, row)
            self.img.paste(tmp, (col, row), tmp)
        elif self.rotation == 'RL':
            # Rotate left (270° = 90° CW in display, text goes bottom to top)
            rotated = tmp.rotate(90, expand=True)
            # Position: the START of the text (first char) is at bottom
            # So the rotated image's bottom-left corner goes to (col, row)
            paste_x = col
            paste_y = row - rotated.size[1]
            self.img.paste(rotated, (paste_x, paste_y), rotated)
        elif self.rotation == 'RR':
            # Rotate right (90° = 270° CW, text goes top to bottom)
            rotated = tmp.rotate(-90, expand=True)
            paste_x = col - rotated.size[0]
            paste_y = row
            self.img.paste(rotated, (paste_x, paste_y), rotated)
        elif self.rotation == 'RU':
            # Rotate upside down (180°)
            rotated = tmp.rotate(180, expand=True)
            paste_x = col - rotated.size[0]
            paste_y = row - rotated.size[1]
            self.img.paste(rotated, (paste_x, paste_y), rotated)

    def _draw_box(self, rows, cols):
        """Draw a box at current position."""
        x0, y0 = self.col, self.row
        x1, y1 = x0 + cols - 1, y0 + rows - 1
        t = self.line_thickness
        # Top
        self.draw.rectangle([x0, y0, x1, y0 + t - 1], fill=FG_COLOR)
        # Bottom
        self.draw.rectangle([x0, y1 - t + 1, x1, y1], fill=FG_COLOR)
        # Left
        self.draw.rectangle([x0, y0, x0 + t - 1, y1], fill=FG_COLOR)
        # Right
        self.draw.rectangle([x1 - t + 1, y0, x1, y1], fill=FG_COLOR)
        self.line_thickness = 1

    def _draw_vline(self, length):
        """Draw vertical line at current position."""
        x0, y0 = self.col, self.row
        t = self.line_thickness
        self.draw.rectangle([x0, y0, x0 + t - 1, y0 + length - 1], fill=FG_COLOR)
        self.line_thickness = 1

    def _draw_hline(self, length):
        """Draw horizontal line at current position."""
        x0, y0 = self.col, self.row
        t = self.line_thickness
        self.draw.rectangle([x0, y0, x0 + length - 1, y0 + t - 1], fill=FG_COLOR)
        self.line_thickness = 1

    def _draw_barcode(self, data, bar_units):
        """Draw Code 39 ladder barcode."""
        # Strip * delimiters from data for display, keep for encoding
        bar_height = bar_units * 8  # each unit = 8 dots
        expansion = self.barcode_expansion

        bc_img = render_code39_image(data, bar_height, expansion)

        # Ladder orientation: rotate 90° so bars are horizontal
        # For NL (ladder), bars go horizontally, barcode extends vertically
        bc_rotated = bc_img.rotate(90, expand=True)

        # Convert to RGBA for pasting
        bc_rgba = Image.new('RGBA', bc_rotated.size, (255, 255, 255, 0))
        for px in range(bc_rotated.size[0]):
            for py in range(bc_rotated.size[1]):
                if bc_rotated.getpixel((px, py)) == 0:
                    bc_rgba.putpixel((px, py), (0, 0, 0, 255))

        # Position: for default ladder (RR-like), bars extend leftward
        paste_x = self.col - bc_rgba.size[0]
        paste_y = self.row

        # Clamp to image bounds
        paste_x = max(0, paste_x)
        paste_y = max(0, paste_y)

        self.img.paste(bc_rgba, (paste_x, paste_y), bc_rgba)
        self.barcode_expansion = 1

    def _draw_logo_placeholder(self, logo_id, row, col):
        """Draw a placeholder rectangle for a logo."""
        # Estimate logo size (typical: ~150x130 dots)
        logo_w = 130
        logo_h = 400

        if self.rotation == 'RL':
            # Logo rotated left: extends upward from position
            x0, y0 = col, row - logo_h
            x1, y1 = col + logo_w, row
        else:
            x0, y0 = col, row
            x1, y1 = col + logo_w, row + logo_h

        x0 = max(0, x0)
        y0 = max(0, y0)
        x1 = min(self.width - 1, x1)
        y1 = min(self.height - 1, y1)

        self.draw.rectangle([x0, y0, x1, y1], outline=FG_COLOR, width=1)
        # Draw "LOGO" text in center
        font = ImageFont.truetype(FONT_MAP[4], pt_to_px(8))
        label = f"[LOGO {logo_id}]"
        bbox = font.getbbox(label)
        lw, lh = bbox[2] - bbox[0], bbox[3] - bbox[1]

        if self.rotation == 'RL':
            tmp = Image.new('RGBA', (lw + 4, lh + 4), (255, 255, 255, 0))
            tmp_draw = ImageDraw.Draw(tmp)
            tmp_draw.text((2, 2), label, font=font, fill=FG_COLOR)
            tmp = tmp.rotate(90, expand=True)
            cx = x0 + (x1 - x0 - tmp.size[0]) // 2
            cy = y0 + (y1 - y0 - tmp.size[1]) // 2
            self.img.paste(tmp, (cx, cy), tmp)
        else:
            cx = x0 + (x1 - x0 - lw) // 2
            cy = y0 + (y1 - y0 - lh) // 2
            self.draw.text((cx, cy), label, font=font, fill=FG_COLOR)

    def parse_and_render(self, fgl_string):
        """Parse FGL command string and render to image."""
        # Remove newlines (the C# code does this)
        fgl_string = fgl_string.replace('\r\n', '').replace('\n', '')

        pos = 0
        length = len(fgl_string)
        sp_row = 0
        sp_col = 0
        barcode_units = 4  # default

        while pos < length:
            if fgl_string[pos] == '<':
                # Find closing >
                end = fgl_string.find('>', pos)
                if end == -1:
                    break
                cmd = fgl_string[pos + 1:end]
                pos = end + 1

                # Parse command
                if cmd.startswith('RC'):
                    parts = cmd[2:].split(',')
                    if len(parts) == 2:
                        self.row = int(parts[0])
                        self.col = int(parts[1])

                elif cmd == 'NR':
                    self.rotation = 'NR'
                elif cmd == 'RL':
                    self.rotation = 'RL'
                elif cmd == 'RR':
                    self.rotation = 'RR'
                elif cmd == 'RU':
                    self.rotation = 'RU'

                elif cmd.startswith('RTF'):
                    parts = cmd[3:].split(',')
                    if len(parts) == 2:
                        self.font_id = int(parts[0])
                        self.font_pt = int(parts[1])
                        self.font_type = 'rtf'

                elif cmd.startswith('TTF'):
                    parts = cmd[3:].split(',')
                    if len(parts) == 2:
                        self.font_id = int(parts[0])
                        self.font_pt = int(parts[1])
                        self.font_type = 'rtf'

                elif cmd.startswith('F') and cmd[1:].isdigit():
                    self.bitmap_font = int(cmd[1:])
                    self.font_type = 'bitmap'

                elif cmd.startswith('HW'):
                    parts = cmd[2:].split(',')
                    if len(parts) == 2:
                        self.hw_h = int(parts[0])
                        self.hw_w = int(parts[1])

                elif cmd.startswith('LT'):
                    self.line_thickness = int(cmd[2:])

                elif cmd.startswith('BX'):
                    parts = cmd[2:].split(',')
                    if len(parts) == 2:
                        self._draw_box(int(parts[0]), int(parts[1]))

                elif cmd.startswith('VX'):
                    self._draw_vline(int(cmd[2:]))

                elif cmd.startswith('HX'):
                    self._draw_hline(int(cmd[2:]))

                elif cmd.startswith('SP'):
                    parts = cmd[2:].split(',')
                    if len(parts) == 2:
                        sp_row = int(parts[0])
                        sp_col = int(parts[1])

                elif cmd.startswith('LD'):
                    logo_id = int(cmd[2:])
                    self._draw_logo_placeholder(logo_id, sp_row, sp_col)

                elif cmd.startswith('NL'):
                    barcode_units = int(cmd[2:])
                    # barcode data will be consumed as text below

                elif cmd.startswith('X') and cmd[1:].isdigit():
                    self.barcode_expansion = int(cmd[1:])

                elif cmd.startswith('TTCP'):
                    self.codepage = int(cmd[4:])

                elif cmd == 'CB':
                    pass  # clear buffer - no visual effect
                elif cmd in ('p', 'q', 'z', 'h', 'r'):
                    pass  # print commands - no visual effect in simulation
                elif cmd == 'BI':
                    pass  # barcode interpretation - handle separately

                # Ignore other commands silently

            else:
                # Text data - collect until next <
                text_end = fgl_string.find('<', pos)
                if text_end == -1:
                    text_end = length
                text = fgl_string[pos:text_end]
                pos = text_end

                if not text.strip() and not text:
                    continue

                # Check if this is barcode data (preceded by NL command)
                if barcode_units > 0 and text.startswith('*') and text.endswith('*'):
                    self._draw_barcode(text, barcode_units)
                    barcode_units = 4  # reset to default
                else:
                    # Regular text
                    self._draw_text(text, self.row, self.col)
                    barcode_units = 4  # reset

                continue

        return self.img


def substitute_placeholders(template, values=None):
    """
    Substitute placeholders in the FGL template.
    Uses default example values if none provided.
    """
    if values is None:
        values = {}

    now = datetime.now()

    defaults = {
        '~': "Visita Guidata Castello",
        '#': "Intero",
        '@': "12.00",
        '&': "000123456789",
        '??': "2",
        'TicketNotRefudable': "Biglietto non rimborsabile - Ticket not refundable",
        'Protocol': "Protocollo: 2025-0001",
        'Serial': "Seriale: 1",
        '<DATE>': now.strftime('%d/%m/%Y'),
        '<TIME>': now.strftime('%H:%M:%S'),
    }

    # Merge user values with defaults
    merged = {**defaults, **values}

    result = template
    result = result.replace('\r\n', '').replace('\n', '')

    # Order matters! Replace longer strings first to avoid partial matches
    result = result.replace('TicketNotRefudable', merged['TicketNotRefudable'])
    result = result.replace('Protocol', merged['Protocol'])
    result = result.replace('Serial', merged['Serial'])
    result = result.replace('<DATE>', merged['<DATE>'])
    result = result.replace('<TIME>', merged['<TIME>'])
    result = result.replace('~', merged['~'])
    result = result.replace('#', merged['#'])
    result = result.replace('@', merged['@'])
    result = result.replace('??', merged['??'])
    result = result.replace('&', merged['&'])

    return result


# --- Main ---

if __name__ == '__main__':
    # The original template
    template = """<RC1,1><LT3><BX958,620> <RL><SP170,25><LD1> <RL><RC905,57><RTF4,8>CASTELLO ODESCALCHI DI BRACCIANO <RL><RC905,105><RTF4,6>TicketNotRefudable <RC1,160><LT3><VX958> <RC707,165><RTF4,10><RL>~ <RC707,210><RTF4,10><RL># <RC707,260><RTF4,6><RL>Protocol <RC350,260><RTF4,6><RL>Serial <HW1,1><RC913,165><RTF4,9>PAX<RC900,195><RL><RTF4,23>?? <RC1,300><LT3><VX958><RC726,160><LT3><HX140> <RC296,304><RTF3,6><RL>Prezzo / Price<RC296,343><RTF4,9><RL><TTCP10>€<HW1,1><RTF4,15> @ <RC940,319><RL><RTF3,6>Data e ora <RC940,368><RL>Date and time<RC700,343><RL><RTF4,7><DATE><RC462,343><RL><TIME> <RC1,427><LT3><VX958><RC300,300><LT3><HX127> <RC206,572><NL15><X3>*&*<RC623,571><RTF4,7><RL>&"""

    # Substitute placeholders
    filled = substitute_placeholders(template)

    # Render
    renderer = FGLRenderer()
    img = renderer.parse_and_render(filled)

    # Save printer orientation (vertical, as printed)
    output_raw = '/home/claude/ticket_raw.png'
    img.save(output_raw, 'PNG', dpi=(DPI, DPI))

    # Save reading orientation (rotated 90° CW — as you hold the ticket)
    img_rotated = img.rotate(-90, expand=True)
    output_read = '/mnt/user-data/outputs/biglietto_simulato.png'
    img_rotated.save(output_read, 'PNG', dpi=(DPI, DPI))

    # Also save the raw orientation
    output_raw_out = '/mnt/user-data/outputs/biglietto_orientamento_stampa.png'
    img.save(output_raw_out, 'PNG', dpi=(DPI, DPI))

    print(f"Biglietto (orientamento di lettura): {output_read}")
    print(f"Biglietto (orientamento di stampa): {output_raw_out}")
    print(f"Dimensioni: {img.size[0]}x{img.size[1]} px at {DPI} DPI")
