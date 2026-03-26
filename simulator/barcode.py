"""
Barcode encoder/decoder — port from Scv_Printer/Barcode.cs
41-bit packed format:
  bits 1-29  (29 bit): seconds since 2014-01-01
  bits 30-36  (7 bit): pax (max 127)
  bits 37-39  (3 bit): tipo (max 7)
  bits 40-41  (2 bit): cont (max 3)
"""

from datetime import datetime, timedelta

TOTALLEN = 41
EPOCH = datetime(2014, 1, 1)
MAX_SECONDS = (1 << 29) - 1  # 536_870_911


def _get_bits(code: int, pos: int, length: int) -> int:
    mask = (1 << length) - 1
    sw = (TOTALLEN - length) - (pos - 1)
    return (code >> sw) & mask


def _set_bits(code: int, pos: int, val: int, length: int) -> int:
    mask = (1 << length) - 1
    if (mask & val) != val:
        raise OverflowError(
            f"Value {val} exceeds {length}-bit capacity (max {mask}). "
            f"This is the likely cause of barcode corruption!"
        )
    sw = (TOTALLEN - length) - (pos - 1)
    return code | (val << sw)


def encode(pax: int, date: datetime, cont: int = 0, tipo: int = 0) -> int:
    """Encode ticket data into a 41-bit barcode (as integer)."""
    secs = int((date - EPOCH).total_seconds())

    if secs < 0:
        raise ValueError(f"Date {date} is before epoch {EPOCH}")
    if secs > MAX_SECONDS:
        overflow_date = EPOCH + timedelta(seconds=MAX_SECONDS)
        raise OverflowError(
            f"Date {date} exceeds 29-bit seconds capacity! "
            f"Max representable date: {overflow_date} "
            f"(seconds={secs}, max={MAX_SECONDS})"
        )

    code = 0
    code = _set_bits(code, 1, secs, 29)
    code = _set_bits(code, 30, pax, 7)
    code = _set_bits(code, 37, tipo, 3)
    code = _set_bits(code, 40, cont, 2)
    return code


def decode(code: int) -> dict:
    """Decode a barcode integer into its components."""
    secs = _get_bits(code, 1, 29)
    pax = _get_bits(code, 30, 7)
    tipo = _get_bits(code, 37, 3)
    cont = _get_bits(code, 40, 2)
    date = EPOCH + timedelta(seconds=secs)
    return {"date": date, "pax": pax, "tipo": tipo, "cont": cont, "seconds": secs}


def check_date_capacity():
    """Check when the 29-bit seconds field overflows."""
    max_date = EPOCH + timedelta(seconds=MAX_SECONDS)
    now = datetime.now()
    remaining = max_date - now
    secs_now = int((now - EPOCH).total_seconds())

    print(f"Epoch:            {EPOCH}")
    print(f"Max seconds:      {MAX_SECONDS} (2^29 - 1)")
    print(f"Max date:         {max_date}")
    print(f"Now:              {now}")
    print(f"Seconds now:      {secs_now}")
    print(f"Capacity used:    {secs_now / MAX_SECONDS * 100:.1f}%")
    print(f"Remaining:        {remaining.days} days ({remaining.days / 365:.1f} years)")

    # Test: what happens with today's date in C# (silent overflow)
    print(f"\n--- Overflow simulation ---")
    try:
        code = encode(pax=2, date=now)
        decoded = decode(code)
        print(f"Encode today:     OK -> code={code}")
        print(f"Decode back:      date={decoded['date']}, pax={decoded['pax']}")
        if decoded['date'].date() != now.date():
            print(f"WARNING: Date mismatch! Encoded {now.date()} but decoded {decoded['date'].date()}")
    except OverflowError as e:
        print(f"OVERFLOW:         {e}")


if __name__ == "__main__":
    check_date_capacity()
    print()

    # Test encoding/decoding a ticket for today
    now = datetime.now()
    print(f"--- Test encode/decode ---")
    code = encode(pax=3, date=now, cont=1, tipo=2)
    print(f"Encoded:  {code}")
    decoded = decode(code)
    print(f"Decoded:  {decoded}")

    # Test what C# does with setBits overflow (silent catch)
    print(f"\n--- C# silent overflow test ---")
    print("In C#, setBits catches OverflowError silently (empty catch).")
    print("If sec overflows 29 bits, the bits simply don't get set -> code stays 0 or partial.")
    print("DecodeBarCode would then return epoch date (2014-01-01) -> always 'Scaduto'.")
