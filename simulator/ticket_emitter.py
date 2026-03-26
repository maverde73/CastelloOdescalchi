"""
Simulates ticket emission: generates barcode and inserts into DB.
Mirrors the logic in Scv_Presentation/viewmodels/TiketViewModel.cs + Scv_Printer/Barcode.cs
"""

from datetime import datetime
from barcode import encode, decode
from db_connection import get_connection


def emit_ticket(pax: int, tipo: int = 1, cont: int = 0,
                emission_date: datetime = None) -> dict:
    """
    Emit a ticket: encode barcode and insert into DB.
    Returns ticket info dict.
    """
    if emission_date is None:
        emission_date = datetime.now()

    code = encode(pax=pax, date=emission_date, cont=cont, tipo=tipo)
    decoded = decode(code)

    conn = get_connection()
    cursor = conn.cursor()
    cursor.execute(
        """INSERT INTO Biglietto (Codice, Id_TipoBiglietto, Pax, DataOraEmissione, Vidimato, Annullato, Passed)
           VALUES (%s, %s, %s, %s, 0, 0, 0)""",
        (code, tipo, pax, emission_date)
    )
    conn.commit()

    cursor.execute("SELECT SCOPE_IDENTITY()")
    ticket_id = int(cursor.fetchone()[0])
    conn.close()

    ticket = {
        "id": ticket_id,
        "code": code,
        "barcode_str": str(code),
        "pax": pax,
        "tipo": tipo,
        "cont": cont,
        "emission_date": emission_date,
        "decoded_date": decoded["date"],
        "decoded_pax": decoded["pax"],
    }

    print(f"Ticket emitted: id={ticket_id}, code={code}, "
          f"pax={pax}, tipo={tipo}, date={emission_date}")
    print(f"  Decode check: date={decoded['date']}, pax={decoded['pax']}, "
          f"tipo={decoded['tipo']}, cont={decoded['cont']}")

    # Verify round-trip
    if decoded["date"].date() != emission_date.date():
        print(f"  WARNING: Date round-trip mismatch! "
              f"emitted={emission_date.date()} decoded={decoded['date'].date()}")
    if decoded["pax"] != pax:
        print(f"  WARNING: Pax round-trip mismatch! emitted={pax} decoded={decoded['pax']}")

    return ticket


def get_ticket_from_db(code: int) -> dict:
    """Lookup a ticket by barcode code (mirrors Get_Biglietto_ByCode)."""
    conn = get_connection()
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM Biglietto WHERE Codice = %s", (code,))
    row = cursor.fetchone()
    conn.close()

    if row is None:
        return None
    return {
        "id": row.Id_Biglietto,
        "code": row.Codice,
        "pax": row.Pax,
        "emission_date": row.DataOraEmissione,
        "vidimato": row.Vidimato,
        "annullato": row.Annullato,
        "passed": row.Passed,
    }


if __name__ == "__main__":
    print("=== Emitting test tickets ===\n")

    # 1. Valid ticket for today
    t1 = emit_ticket(pax=2, tipo=1)
    print()

    # 2. Ticket for 1 person
    t2 = emit_ticket(pax=1, tipo=2, cont=1)
    print()

    # 3. Check DB lookup
    print(f"=== DB lookup for code={t1['code']} ===")
    found = get_ticket_from_db(t1["code"])
    print(f"  Found: {found}")
