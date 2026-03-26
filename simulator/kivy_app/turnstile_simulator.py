"""
Simulates the physical turnstile terminal.
Sends UDP XML messages mimicking the REA DCP protocol.
"""

import socket
import time
import xml.etree.ElementTree as ET
from datetime import datetime

SERVER_HOST = "127.0.0.1"
SERVER_PORT = 1000
LISTEN_PORT = 1001


class TurnstileSimulator:
    def __init__(self, server_host=SERVER_HOST, server_port=SERVER_PORT,
                 listen_port=LISTEN_PORT):
        self.server = (server_host, server_port)
        self.listen_port = listen_port
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.sock.bind(("0.0.0.0", listen_port))
        self.sock.settimeout(5.0)
        self.transaction_id = 1

    def _next_id(self) -> str:
        tid = str(self.transaction_id)
        self.transaction_id += 1
        return tid

    def _send_and_receive(self, xml_str: str) -> str:
        print(f"\n>>> SEND to {self.server}:")
        print(f"    {xml_str}")

        self.sock.sendto(xml_str.encode("utf-8"), self.server)

        try:
            data, addr = self.sock.recvfrom(4096)
            response = data.decode("utf-8")
            print(f"<<< RECV from {addr}:")
            print(f"    {response}")
            return response
        except socket.timeout:
            print("<<< TIMEOUT: no response received")
            return None

    def send_test(self) -> str:
        """Send heartbeat/test message."""
        tid = self._next_id()
        now = datetime.now().strftime("%Y%m%d%H%M%S")
        xml = (f'<?xml version="1.0" encoding="UTF-8"?>'
               f'<cmf id="{tid}" addr="1" protocol="1.0">'
               f'<test><time>{now}</time></test></cmf>')
        return self._send_and_receive(xml)

    def send_entry(self, barcode: str) -> str:
        """Send barcode scan (entry request)."""
        tid = self._next_id()
        xml = (f'<?xml version="1.0" encoding="UTF-8"?>'
               f'<cmf id="{tid}" addr="1" protocol="1.0">'
               f'<entry><chip>{barcode}</chip></entry></cmf>')
        return self._send_and_receive(xml)

    def send_pass(self, act: str = "l") -> str:
        """Send physical passage confirmation."""
        tid = self._next_id()
        xml = (f'<?xml version="1.0" encoding="UTF-8"?>'
               f'<cmf id="{tid}" addr="1" protocol="1.0">'
               f'<pass><act>{act}</act></pass></cmf>')
        return self._send_and_receive(xml)

    def send_event(self, code: str) -> str:
        """Send generic event."""
        tid = self._next_id()
        xml = (f'<?xml version="1.0" encoding="UTF-8"?>'
               f'<cmf id="{tid}" addr="1" protocol="1.0">'
               f'<event><code>{code}</code></event></cmf>')
        return self._send_and_receive(xml)

    def parse_entry_response(self, response: str) -> dict:
        """Parse the server's response to an entry request."""
        if response is None:
            return {"direction": None, "count": None, "text": None}
        try:
            root = ET.fromstring(response)
            ret = root.find("return")
            if ret is not None:
                act = ret.find("act")
                cnt = ret.find("cnt")
                txt = ret.find("txt")
                return {
                    "direction": act.text if act is not None else None,
                    "count": cnt.text if cnt is not None else None,
                    "text": txt.text if txt is not None else None,
                }
        except ET.ParseError:
            pass
        return {"direction": None, "count": None, "text": None, "raw": response}

    def close(self):
        self.sock.close()


def run_full_scenario(barcode: str):
    """Run a complete turnstile scenario: test -> entry -> pass."""
    sim = TurnstileSimulator()

    print("=" * 60)
    print("SCENARIO: Full turnstile flow")
    print("=" * 60)

    # 1. Heartbeat
    print("\n--- Step 1: Heartbeat (test) ---")
    sim.send_test()
    time.sleep(0.5)

    # 2. Barcode scan
    print(f"\n--- Step 2: Scan barcode '{barcode}' ---")
    response = sim.send_entry(barcode)
    parsed = sim.parse_entry_response(response)
    print(f"    Parsed: {parsed}")

    if parsed.get("count") == "1" or parsed.get("count") == "0":
        allowed = parsed["count"] != "0"
        direction = parsed.get("direction", "?")

        if allowed:
            # 3. Physical passage
            time.sleep(0.5)
            print(f"\n--- Step 3: Person passes through (direction={direction}) ---")
            sim.send_pass(direction)
        else:
            print(f"\n--- Step 3: ACCESS DENIED - gate stays closed ---")
            print(f"    Reason: {parsed.get('text', 'unknown')}")

    sim.close()
    print("\n" + "=" * 60)


if __name__ == "__main__":
    import sys
    if len(sys.argv) > 1:
        barcode = sys.argv[1]
        run_full_scenario(barcode)
    else:
        print("Usage: python turnstile_simulator.py <barcode>")
        print()
        print("To generate a test barcode first:")
        print("  python -c \"from barcode import encode; from datetime import datetime; print(encode(2, datetime.now()))\"")
