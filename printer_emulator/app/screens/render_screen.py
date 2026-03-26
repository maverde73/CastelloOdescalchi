"""
Main screen: placeholder form + FGL template editor + ticket render output.
Multi-template management with save/load/delete.
"""

import json
import os
import shutil
from datetime import datetime

from kivy.app import App
from kivy.lang import Builder
from kivy.metrics import sp
from kivy.uix.screenmanager import Screen
from kivy.uix.popup import Popup
from kivy.uix.boxlayout import BoxLayout
from kivy.uix.label import Label
from kivy.uix.textinput import TextInput
from kivy.uix.button import Button
from kivy.properties import StringProperty, ListProperty

from services.renderer_service import render_ticket

# Pre-compute integer font sizes (SDL2 on Android requires int, not float)
_sp11 = int(sp(11))
_sp12 = int(sp(12))
_sp13 = int(sp(13))
_sp14 = int(sp(14))
_sp16 = int(sp(16))
_sp18 = int(sp(18))

Builder.load_string("""
#:import dp kivy.metrics.dp

<FormField@BoxLayout>:
    orientation: 'vertical'
    size_hint_y: None
    height: dp(62)
    spacing: dp(2)
    label_text: ''
    hint_text: ''
    field_id: ''
    input_filter: None
    Label:
        text: root.label_text
        font_size: """ + str(_sp12) + """
        color: 0.6, 0.6, 0.7, 1
        size_hint_y: None
        height: dp(18)
        text_size: self.width, None
        halign: 'left'
    TextInput:
        id: field_input
        hint_text: root.hint_text
        input_filter: root.input_filter
        font_size: """ + str(_sp14) + """
        multiline: False
        size_hint_y: None
        height: dp(38)
        background_color: 0.12, 0.12, 0.18, 1
        foreground_color: 1, 1, 1, 1
        hint_text_color: 0.4, 0.4, 0.5, 1
        cursor_color: 1, 0.75, 0.2, 1
        padding: [dp(8), dp(8)]

<RenderScreen>:
    name: 'render'
    BoxLayout:
        orientation: 'vertical'
        canvas.before:
            Color:
                rgba: 0.07, 0.07, 0.12, 1
            Rectangle:
                pos: self.pos
                size: self.size

        # --- Header ---
        BoxLayout:
            size_hint_y: None
            height: dp(48)
            padding: [dp(12), dp(6)]
            canvas.before:
                Color:
                    rgba: 0.1, 0.1, 0.16, 1
                Rectangle:
                    pos: self.pos
                    size: self.size
            Label:
                text: 'FGL Ticket Renderer'
                font_size: """ + str(_sp18) + """
                bold: True
                color: 1, 0.75, 0.2, 1
                text_size: self.width, None
                halign: 'left'

        ScrollView:
            do_scroll_x: False
            bar_color: 1, 0.75, 0.2, 0.6
            BoxLayout:
                orientation: 'vertical'
                size_hint_y: None
                height: self.minimum_height
                padding: [dp(12), dp(8)]
                spacing: dp(8)

                # --- Template selector ---
                BoxLayout:
                    size_hint_y: None
                    height: dp(36)
                    spacing: dp(6)
                    Label:
                        text: 'Template:'
                        font_size: """ + str(_sp12) + """
                        size_hint_x: None
                        width: dp(70)
                        color: 0.6, 0.6, 0.7, 1
                        text_size: self.width, None
                        halign: 'left'
                    Spinner:
                        id: template_spinner
                        text: root.current_template_name
                        values: root.template_names
                        font_size: """ + str(_sp13) + """
                        size_hint_x: 1
                        background_color: 0.15, 0.15, 0.22, 1
                        color: 1, 0.85, 0.4, 1
                        on_text: root.on_template_selected(self.text)

                # --- Template action buttons ---
                BoxLayout:
                    size_hint_y: None
                    height: dp(34)
                    spacing: dp(6)
                    Button:
                        text: 'Nuovo'
                        font_size: """ + str(_sp12) + """
                        background_color: 0.2, 0.35, 0.2, 1
                        color: 0.8, 1, 0.8, 1
                        on_release: root.new_template()
                    Button:
                        text: 'Salva'
                        font_size: """ + str(_sp12) + """
                        background_color: 0.2, 0.2, 0.4, 1
                        color: 0.8, 0.8, 1, 1
                        on_release: root.save_current_template()
                    Button:
                        text: 'Rinomina'
                        font_size: """ + str(_sp12) + """
                        background_color: 0.3, 0.25, 0.15, 1
                        color: 1, 0.9, 0.7, 1
                        on_release: root.rename_template()
                    Button:
                        text: 'Elimina'
                        font_size: """ + str(_sp12) + """
                        background_color: 0.35, 0.12, 0.12, 1
                        color: 1, 0.6, 0.6, 1
                        on_release: root.delete_template()

                # --- FGL Template editor (always visible, editable) ---
                Label:
                    text: 'Template FGL'
                    font_size: """ + str(_sp12) + """
                    color: 0.6, 0.6, 0.7, 1
                    size_hint_y: None
                    height: dp(18)
                    text_size: self.width, None
                    halign: 'left'
                TextInput:
                    id: template_input
                    size_hint_y: None
                    height: dp(220)
                    font_size: """ + str(_sp11) + """
                    font_name: 'Roboto'
                    background_color: 0.08, 0.08, 0.14, 1
                    foreground_color: 0.7, 0.9, 0.7, 1
                    cursor_color: 1, 0.75, 0.2, 1
                    padding: [dp(8), dp(8)]

                # --- Placeholder Fields ---
                FormField:
                    label_text: 'Evento (~)'
                    hint_text: 'Visita Guidata Castello'
                    field_id: 'event'
                    id: f_event
                FormField:
                    label_text: 'Tipo Biglietto (#)'
                    hint_text: 'Intero'
                    field_id: 'ticket_type'
                    id: f_ticket_type
                FormField:
                    label_text: 'Prezzo (@)'
                    hint_text: '12,00'
                    field_id: 'price'
                    id: f_price
                FormField:
                    label_text: 'Barcode (&)'
                    hint_text: '000123456789'
                    field_id: 'barcode'
                    id: f_barcode
                FormField:
                    label_text: 'PAX (??)'
                    hint_text: '2'
                    field_id: 'pax'
                    id: f_pax
                FormField:
                    label_text: 'Note Rimborso'
                    hint_text: 'Biglietto non rimborsabile - Ticket not refundable'
                    field_id: 'refund_note'
                    id: f_refund_note
                FormField:
                    label_text: 'Protocollo'
                    hint_text: 'Protocollo: 2025-0001'
                    field_id: 'protocol'
                    id: f_protocol
                FormField:
                    label_text: 'Seriale'
                    hint_text: 'Seriale: 1'
                    field_id: 'serial'
                    id: f_serial

                # --- Date/Time row ---
                BoxLayout:
                    size_hint_y: None
                    height: dp(62)
                    spacing: dp(8)
                    FormField:
                        label_text: 'Data'
                        hint_text: '25/03/2026'
                        field_id: 'date'
                        id: f_date
                    FormField:
                        label_text: 'Ora'
                        hint_text: '10:30:00'
                        field_id: 'time'
                        id: f_time

                # --- Render button ---
                Button:
                    id: btn_render
                    text: 'Stampa Biglietto'
                    size_hint_y: None
                    height: dp(50)
                    font_size: """ + str(_sp16) + """
                    bold: True
                    background_color: 0.8, 0.6, 0.1, 1
                    color: 1, 1, 1, 1
                    on_release: root.do_render()

                # --- Status ---
                Label:
                    id: status_label
                    text: root.status_text
                    size_hint_y: None
                    height: dp(24) if root.status_text else dp(0)
                    font_size: """ + str(_sp12) + """
                    color: 0.9, 0.5, 0.5, 1 if 'Errore' in root.status_text else (0.5, 0.9, 0.5, 1)

                # --- Rendered ticket image (pinch-to-zoom) ---
                Scatter:
                    id: scatter
                    size_hint_y: None
                    height: dp(300)
                    do_rotation: False
                    scale_min: 0.5
                    scale_max: 4.0
                    Image:
                        id: ticket_image
                        source: root.ticket_image_path
                        size: scatter.size
                        fit_mode: 'contain'

                # --- Save button ---
                Button:
                    id: btn_save
                    text: 'Salva Immagine'
                    size_hint_y: None
                    height: dp(42) if root.ticket_image_path else dp(0)
                    opacity: 1 if root.ticket_image_path else 0
                    disabled: not root.ticket_image_path
                    font_size: """ + str(_sp14) + """
                    background_color: 0.15, 0.4, 0.15, 1
                    color: 1, 1, 1, 1
                    on_release: root.save_image()

                # --- Bottom padding ---
                Widget:
                    size_hint_y: None
                    height: dp(20)
""")

TEMPLATES_FILE = "templates.json"
DEFAULT_TEMPLATE_NAME = "Boca Default"


class RenderScreen(Screen):
    status_text = StringProperty("")
    ticket_image_path = StringProperty("")
    current_template_name = StringProperty(DEFAULT_TEMPLATE_NAME)
    template_names = ListProperty([DEFAULT_TEMPLATE_NAME])

    def on_enter(self):
        self._load_templates_list()
        self._load_defaults()

    # --- Template storage ---

    def _templates_path(self):
        """Path to templates.json — persistent on Android, local on desktop."""
        app = App.get_running_app()
        if app:
            base = app.user_data_dir
        else:
            base = self._get_app_dir()
        return os.path.join(base, TEMPLATES_FILE)

    def _load_templates_db(self) -> dict:
        path = self._templates_path()
        if os.path.isfile(path):
            with open(path, "r") as f:
                return json.load(f)
        return {}

    def _save_templates_db(self, db: dict):
        path = self._templates_path()
        os.makedirs(os.path.dirname(path), exist_ok=True)
        with open(path, "w") as f:
            json.dump(db, f, ensure_ascii=False, indent=2)

    def _get_builtin_template(self) -> str:
        app_dir = self._get_app_dir()
        template_path = os.path.join(app_dir, "templates", "default_template.txt")
        print(f"[FGL] app_dir={app_dir}")
        print(f"[FGL] template_path={template_path}")
        print(f"[FGL] exists={os.path.isfile(template_path)}")
        try:
            with open(template_path, "r") as f:
                content = f.read()
                print(f"[FGL] template loaded, len={len(content)}")
                return content
        except Exception as e:
            print(f"[FGL] template load error: {e}")
            return ""

    def _load_templates_list(self):
        db = self._load_templates_db()
        names = [DEFAULT_TEMPLATE_NAME]
        for name in sorted(db.keys()):
            if name != DEFAULT_TEMPLATE_NAME:
                names.append(name)
        self.template_names = names

    # --- Template actions ---

    def on_template_selected(self, name):
        if not name or name == self.current_template_name:
            return
        self.current_template_name = name
        if name == DEFAULT_TEMPLATE_NAME:
            self.ids.template_input.text = self._get_builtin_template()
        else:
            db = self._load_templates_db()
            self.ids.template_input.text = db.get(name, "")
        self.status_text = f"Template: {name}"

    def save_current_template(self):
        name = self.current_template_name
        content = self.ids.template_input.text
        if name == DEFAULT_TEMPLATE_NAME:
            # Save builtin override with a different name
            self._show_text_popup(
                "Salva come...",
                "Nome template:",
                DEFAULT_TEMPLATE_NAME + " (mod)",
                self._do_save_as,
            )
            return
        db = self._load_templates_db()
        db[name] = content
        self._save_templates_db(db)
        self.status_text = f"Template '{name}' salvato"

    def _do_save_as(self, name):
        if not name.strip():
            return
        name = name.strip()
        db = self._load_templates_db()
        db[name] = self.ids.template_input.text
        self._save_templates_db(db)
        self._load_templates_list()
        self.current_template_name = name
        self.ids.template_spinner.text = name
        self.status_text = f"Template '{name}' salvato"

    def new_template(self):
        self._show_text_popup(
            "Nuovo Template",
            "Nome:",
            "",
            self._do_new_template,
        )

    def _do_new_template(self, name):
        if not name.strip():
            return
        name = name.strip()
        db = self._load_templates_db()
        db[name] = ""
        self._save_templates_db(db)
        self._load_templates_list()
        self.current_template_name = name
        self.ids.template_spinner.text = name
        self.ids.template_input.text = ""
        self.template_visible = True
        self.status_text = f"Template '{name}' creato"

    def rename_template(self):
        name = self.current_template_name
        if name == DEFAULT_TEMPLATE_NAME:
            self.status_text = "Non puoi rinominare il template di default"
            return
        self._show_text_popup(
            "Rinomina Template",
            "Nuovo nome:",
            name,
            self._do_rename,
        )

    def _do_rename(self, new_name):
        if not new_name.strip():
            return
        new_name = new_name.strip()
        old_name = self.current_template_name
        db = self._load_templates_db()
        content = db.pop(old_name, "")
        db[new_name] = content
        self._save_templates_db(db)
        self._load_templates_list()
        self.current_template_name = new_name
        self.ids.template_spinner.text = new_name
        self.status_text = f"Rinominato '{old_name}' -> '{new_name}'"

    def delete_template(self):
        name = self.current_template_name
        if name == DEFAULT_TEMPLATE_NAME:
            self.status_text = "Non puoi eliminare il template di default"
            return
        db = self._load_templates_db()
        db.pop(name, None)
        self._save_templates_db(db)
        self._load_templates_list()
        self.current_template_name = DEFAULT_TEMPLATE_NAME
        self.ids.template_spinner.text = DEFAULT_TEMPLATE_NAME
        self.ids.template_input.text = self._get_builtin_template()
        self.status_text = f"Template '{name}' eliminato"

    # --- Popup helper ---

    def _show_text_popup(self, title, label_text, default_value, callback):
        content = BoxLayout(orientation="vertical", spacing=8, padding=8)
        content.add_widget(
            Label(text=label_text, size_hint_y=None, height=30, font_size=_sp14)
        )
        text_input = TextInput(
            text=default_value,
            multiline=False,
            size_hint_y=None,
            height=40,
            font_size=_sp14,
        )
        content.add_widget(text_input)
        btn_row = BoxLayout(size_hint_y=None, height=40, spacing=8)
        popup = Popup(
            title=title,
            content=content,
            size_hint=(0.85, None),
            height=200,
        )
        btn_cancel = Button(text="Annulla", font_size=_sp14)
        btn_cancel.bind(on_release=lambda *_: popup.dismiss())
        btn_ok = Button(
            text="OK",
            font_size=_sp14,
            background_color=(0.2, 0.4, 0.2, 1),
        )

        def on_ok(*_):
            popup.dismiss()
            callback(text_input.text)

        btn_ok.bind(on_release=on_ok)
        btn_row.add_widget(btn_cancel)
        btn_row.add_widget(btn_ok)
        content.add_widget(btn_row)
        popup.open()

    # --- Existing logic ---

    def _get_app_dir(self):
        # On Android, files are extracted to ANDROID_ARGUMENT dir
        android_app = os.environ.get("ANDROID_ARGUMENT")
        if android_app:
            return android_app
        return os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

    def _load_defaults(self):
        now = datetime.now()
        fields = {
            "f_event": "Visita Guidata Castello",
            "f_ticket_type": "Intero",
            "f_price": "12,00",
            "f_barcode": "000123456789",
            "f_pax": "2",
            "f_refund_note": "Biglietto non rimborsabile - Ticket not refundable",
            "f_protocol": "Protocollo: 2025-0001",
            "f_serial": "Seriale: 1",
            "f_date": now.strftime("%d/%m/%Y"),
            "f_time": now.strftime("%H:%M:%S"),
        }
        for field_id, value in fields.items():
            widget = self.ids.get(field_id)
            if widget:
                text_input = widget.ids.get("field_input")
                if text_input and not text_input.text:
                    text_input.text = value

        # Load current template directly (don't use on_template_selected
        # because the guard skips when name == current_template_name)
        if not self.ids.template_input.text:
            self.ids.template_input.text = self._get_builtin_template()

    def _get_field_text(self, field_id: str) -> str:
        widget = self.ids.get(field_id)
        if widget:
            text_input = widget.ids.get("field_input")
            if text_input:
                return text_input.text
        return ""

    def reset_template(self):
        self.ids.template_input.text = self._get_builtin_template()
        self.current_template_name = DEFAULT_TEMPLATE_NAME
        self.ids.template_spinner.text = DEFAULT_TEMPLATE_NAME

    def _build_values(self) -> dict:
        return {
            "~": self._get_field_text("f_event"),
            "#": self._get_field_text("f_ticket_type"),
            "@": self._get_field_text("f_price"),
            "&": self._get_field_text("f_barcode"),
            "??": self._get_field_text("f_pax"),
            "TicketNotRefudable": self._get_field_text("f_refund_note"),
            "Protocol": self._get_field_text("f_protocol"),
            "Serial": self._get_field_text("f_serial"),
            "<DATE>": self._get_field_text("f_date"),
            "<TIME>": self._get_field_text("f_time"),
        }

    def do_render(self):
        self.ids.btn_render.disabled = True
        self.status_text = "Rendering..."
        self.ticket_image_path = ""

        template = self.ids.template_input.text
        values = self._build_values()
        app_dir = self._get_app_dir()

        render_ticket(template, values, app_dir, self._on_render_done)

    def _on_render_done(self, png_path, error):
        self.ids.btn_render.disabled = False
        if error:
            self.status_text = f"Errore: {error}"
        else:
            self.status_text = "Biglietto generato"
            self.ticket_image_path = ""
            self.ticket_image_path = png_path

    def save_image(self):
        if not self.ticket_image_path:
            return
        try:
            from kivy.utils import platform
            if platform == "android":
                from android.storage import primary_external_storage_path  # type: ignore
                dest_dir = os.path.join(primary_external_storage_path(), "Pictures")
            else:
                dest_dir = os.path.expanduser("~")

            os.makedirs(dest_dir, exist_ok=True)
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            dest = os.path.join(dest_dir, f"ticket_{timestamp}.png")
            shutil.copy2(self.ticket_image_path, dest)
            self.status_text = f"Salvato: {dest}"
        except Exception as e:
            self.status_text = f"Errore salvataggio: {e}"
