# AGENTS.md — Scv_Printer

## Namespaces

- Namespaces do NOT match the project folder name. Two separate namespace trees:
  - `Thera.Biglietteria.Cassa` — serial printer logic (`SerialPrinter.cs`, `Utility/`)
  - `Thera.Biglietteria.Boca` — Boca thermal printer logic (`BocaPrinter.cs`, `Barcode.cs`)
- Do not rename or "fix" these — they are consumed by other projects in the solution.

## Template system

- `xml/PrintTemplate.xml` — command-based template for the serial (ESC/POS) printer. `Mapping` attributes (e.g. `Mapping="Pax"`, `Mapping="Price"`) are runtime-resolved placeholders, not hardcoded values.
- `Templates/BocaPrintTemplate.txt` — uses **Boca FGL** (Friendly Ghost Language), a proprietary printer command language. The escape codes (`<RC>`, `<RTF>`, `<RL>`, etc.) are Boca-specific, not standard ESC/POS.
- `xml/EN.xml`, `xml/ES.xml` — localization label files loaded by `LocalizeLabelMapper`. The `Language` attribute on `PrintTemplate.xml`'s root element selects which localization file to load.
- `xml/InternationalChars.xml` — character encoding mapping for printer-specific character sets.

## Landmines

- `HAlignType.Rigth` (typo in `SerialPrinter.cs:1577`) is a known historical typo. Do NOT rename it — the enum value is referenced throughout the solution.
- `TemplateManager.cs` uses `Hashtable` (non-generic) extensively with string keys that must match XML attribute values exactly (case-sensitive). Do not change to `Dictionary<>` without verifying all XML template files.
