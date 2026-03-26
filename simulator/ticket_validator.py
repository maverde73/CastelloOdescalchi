"""
Simulates CheckTicket logic — exact port from GestioneTornelloREA/Business/TicketChecker.cs
"""

from datetime import datetime
from barcode import decode, EPOCH
from db_connection import get_connection

GIORNI_VALIDITA = 3


class TicketState:
    VALIDO = "Valido"
    SCADUTO = "Scaduto"
    INVALIDO = "Invalido"
    VALIDATO = "Validato"
    ANNULLATO = "Annullato"


def check_ticket(barcode_str: str, giorni_validita: int = GIORNI_VALIDITA, logger=None) -> dict:
    """
    Validate a ticket barcode. Returns dict with state and details.
    Exact port of TicketChecker.CheckTicket()
    Optional logger callback receives step messages (defaults to print).
    """
    if logger is None:
        logger = print
    result = {"state": TicketState.INVALIDO, "pax": 0, "date": None, "detail": ""}

    try:
        # Step 1: load ticket types (just verify DB connection)
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("SELECT COUNT(*) FROM LK_TipoBiglietto")
        type_count = cursor.fetchone()[0]
        logger(f"  Step 1: DB connected, {type_count} ticket types loaded")

        # Step 2: decode barcode
        code = int(barcode_str)
        logger(f"  Step 2: Barcode parsed as int: {code}")

        decoded = decode(code)
        date = decoded["date"]
        pax = decoded["pax"]
        tipo = decoded["tipo"]
        cont = decoded["cont"]
        logger(f"  Step 2: Decoded -> date={date}, pax={pax}, tipo={tipo}, cont={cont}")

        # Step 3: expiry check
        now = datetime.now()
        from datetime import timedelta
        expiry_threshold = (now - timedelta(days=giorni_validita)).date()
        logger(f"  Step 3: Expiry check -> ticketDate={date.date()}, "
               f"threshold={expiry_threshold}, now={now}")

        if date.date() <= expiry_threshold:
            result["state"] = TicketState.SCADUTO
            result["date"] = date
            result["detail"] = (f"ticketDate={date.date()} <= threshold={expiry_threshold}")
            logger(f"  RESULT: SCADUTO - {result['detail']}")
            conn.close()
            return result

        result["pax"] = pax
        result["date"] = date

        # Step 4: DB lookup
        logger(f"  Step 4: DB lookup for code={code}...")
        cursor.execute("SELECT Pax, Vidimato, Annullato FROM Biglietto WHERE Codice = %s", (code,))
        row = cursor.fetchone()

        if row is not None:
            db_pax, vidimato, annullato = row
            logger(f"  Step 4: Found in DB. Vidimato={vidimato}, Annullato={annullato}, Pax={db_pax}")

            if vidimato:
                result["state"] = TicketState.VALIDATO
                result["detail"] = "already stamped"
                logger(f"  RESULT: VALIDATO (already used)")
            elif annullato:
                result["state"] = TicketState.ANNULLATO
                result["detail"] = "cancelled"
                logger(f"  RESULT: ANNULLATO")
            else:
                # Step 5: vidimazione
                logger(f"  Step 5: Vidimazione...")
                cursor.execute(
                    """UPDATE Biglietto
                       SET Vidimato = 1, DataOraVidimazione = %s, Passed = Passed + 1
                       WHERE Codice = %s AND Vidimato = 0""",
                    (now, code)
                )
                if cursor.rowcount > 0:
                    conn.commit()
                    result["state"] = TicketState.VALIDO
                    result["pax"] = db_pax
                    result["detail"] = f"pax={db_pax}"
                    logger(f"  RESULT: VALIDO, pax={db_pax}")
                else:
                    result["state"] = TicketState.INVALIDO
                    result["detail"] = "Vidima affected 0 rows"
                    logger(f"  RESULT: INVALIDO (Vidima failed)")
        else:
            result["state"] = TicketState.INVALIDO
            result["detail"] = f"ticket not found in DB for code={code}"
            logger(f"  RESULT: INVALIDO (not found in DB)")

        conn.close()
        return result

    except Exception as ex:
        result["state"] = TicketState.INVALIDO
        result["detail"] = f"EXCEPTION: {type(ex).__name__}: {ex}"
        logger(f"  RESULT: INVALIDO (EXCEPTION) - {type(ex).__name__}: {ex}")
        return result


if __name__ == "__main__":
    import sys
    if len(sys.argv) > 1:
        barcode = sys.argv[1]
    else:
        # Quick self-test: emit then validate
        from ticket_emitter import emit_ticket
        print("=== Emitting test ticket ===")
        ticket = emit_ticket(pax=2, tipo=1)
        barcode = ticket["barcode_str"]
        print()

    print(f"=== Validating barcode: {barcode} ===")
    result = check_ticket(barcode)
    print(f"\nFinal result: {result}")

    print(f"\n=== Re-validating same barcode (should be VALIDATO) ===")
    result2 = check_ticket(barcode)
    print(f"\nFinal result: {result2}")
