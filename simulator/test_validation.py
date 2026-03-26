"""
End-to-end test: emit tickets and validate them.
Tests all scenarios without UDP (direct function calls).
Requires: SQL Server running in Docker + db_setup.py executed.
"""

from datetime import datetime, timedelta
from barcode import encode, decode, check_date_capacity
from ticket_emitter import emit_ticket
from ticket_validator import check_ticket, TicketState


def test_barcode_capacity():
    """Test 1: Check if today's date causes barcode overflow."""
    print("=" * 60)
    print("TEST 1: Barcode date capacity check")
    print("=" * 60)
    check_date_capacity()
    print()


def test_valid_ticket():
    """Test 2: Emit and validate a ticket for today."""
    print("=" * 60)
    print("TEST 2: Valid ticket (today)")
    print("=" * 60)
    ticket = emit_ticket(pax=2, tipo=1)
    print()
    result = check_ticket(ticket["barcode_str"])
    assert result["state"] == TicketState.VALIDO, f"Expected VALIDO, got {result}"
    print(f"\nPASS: {result}\n")


def test_double_validation():
    """Test 3: Validate same ticket twice (second should be VALIDATO)."""
    print("=" * 60)
    print("TEST 3: Double validation (second = VALIDATO)")
    print("=" * 60)
    ticket = emit_ticket(pax=1, tipo=1)
    print()

    r1 = check_ticket(ticket["barcode_str"])
    assert r1["state"] == TicketState.VALIDO, f"First: expected VALIDO, got {r1}"
    print(f"\nFirst validation: {r1['state']} (OK)")

    r2 = check_ticket(ticket["barcode_str"])
    assert r2["state"] == TicketState.VALIDATO, f"Second: expected VALIDATO, got {r2}"
    print(f"Second validation: {r2['state']} (OK)\n")


def test_expired_ticket():
    """Test 4: Ticket emitted 5 days ago (should be SCADUTO with 3-day validity)."""
    print("=" * 60)
    print("TEST 4: Expired ticket (5 days old)")
    print("=" * 60)
    old_date = datetime.now() - timedelta(days=5)
    ticket = emit_ticket(pax=1, tipo=1, emission_date=old_date)
    print()
    result = check_ticket(ticket["barcode_str"])
    assert result["state"] == TicketState.SCADUTO, f"Expected SCADUTO, got {result}"
    print(f"\nPASS: {result}\n")


def test_ticket_not_in_db():
    """Test 5: Valid barcode but not inserted in DB."""
    print("=" * 60)
    print("TEST 5: Barcode not in DB (INVALIDO)")
    print("=" * 60)
    # Generate barcode without inserting in DB (use cont=3 to avoid collision)
    code = encode(pax=1, date=datetime.now(), cont=3, tipo=3)
    print(f"Generated barcode (not in DB): {code}")
    result = check_ticket(str(code))
    assert result["state"] == TicketState.INVALIDO, f"Expected INVALIDO, got {result}"
    assert "not found in DB" in result["detail"]
    print(f"\nPASS: {result}\n")


def test_garbage_barcode():
    """Test 6: Invalid barcode string."""
    print("=" * 60)
    print("TEST 6: Garbage barcode (INVALIDO via exception)")
    print("=" * 60)
    result = check_ticket("not_a_number")
    assert result["state"] == TicketState.INVALIDO, f"Expected INVALIDO, got {result}"
    assert "EXCEPTION" in result["detail"]
    print(f"\nPASS: {result}\n")


def test_borderline_expiry():
    """Test 7: Ticket emitted exactly 3 days ago (boundary case)."""
    print("=" * 60)
    print("TEST 7: Borderline expiry (exactly 3 days)")
    print("=" * 60)
    # Exactly 3 days ago at start of day -> should be SCADUTO (<=)
    border_date = (datetime.now() - timedelta(days=3)).replace(
        hour=0, minute=0, second=0, microsecond=0)
    ticket = emit_ticket(pax=1, tipo=1, emission_date=border_date)
    print()
    result = check_ticket(ticket["barcode_str"])
    print(f"\nBorderline result: {result}")
    print(f"(C# uses <= so exactly 3 days ago at midnight = SCADUTO)\n")


def test_cancelled_ticket():
    """Test 8: Cancelled ticket (ANNULLATO)."""
    print("=" * 60)
    print("TEST 8: Cancelled ticket (ANNULLATO)")
    print("=" * 60)
    ticket = emit_ticket(pax=1, tipo=1, cont=2)
    # Manually cancel in DB (before validation, so Vidimato is still 0)
    from db_connection import get_connection
    conn = get_connection()
    conn.autocommit(True)
    conn.cursor().execute(
        "UPDATE Biglietto SET Annullato = 1 WHERE Codice = %s", (ticket["code"],))
    conn.commit()
    conn.close()
    print("  (ticket manually cancelled in DB)")

    result = check_ticket(ticket["barcode_str"])
    assert result["state"] == TicketState.ANNULLATO, f"Expected ANNULLATO, got {result}"
    print(f"\nPASS: {result}\n")


if __name__ == "__main__":
    print("\n" + "=" * 60)
    print("   GESTIONE TORNELLO REA — VALIDATION TEST SUITE")
    print("=" * 60 + "\n")

    test_barcode_capacity()
    test_valid_ticket()
    test_double_validation()
    test_expired_ticket()
    test_ticket_not_in_db()
    test_garbage_barcode()
    test_borderline_expiry()
    test_cancelled_ticket()

    print("=" * 60)
    print("   ALL TESTS COMPLETE")
    print("=" * 60)
