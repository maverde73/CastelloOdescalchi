# Simulatore GestioneTornelloREA

Ambiente di test per simulare l'intero flusso:
1. Emissione biglietto (encoding barcode + inserimento DB)
2. Scansione al tornello (protocollo UDP/XML)
3. Validazione (logica CheckTicket)

## Requisiti

- Docker (per SQL Server)
- Python 3.10+

## Setup

```bash
# 1. Avvia SQL Server
docker compose up -d

# 2. Inizializza il DB (schema + dati base)
pip install pyodbc
python db_setup.py

# 3. Esegui test di validazione (senza UDP)
python test_validation.py

# 4. Simulazione completa con protocollo UDP
# Terminal 1: avvia il server di validazione
python validation_server.py
# Terminal 2: simula il tornello
python turnstile_simulator.py
```
