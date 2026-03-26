"""
Sets up the test database schema in Docker SQL Server.
Schema reverse-engineered from EF entities and DAL code.
"""

import sys
from db_connection import get_connection, get_master_connection, DATABASE as DB_NAME


def create_database():
    conn = get_master_connection()
    cursor = conn.cursor()
    cursor.execute(f"""
        IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '{DB_NAME}')
        CREATE DATABASE [{DB_NAME}]
    """)
    conn.close()
    print(f"Database '{DB_NAME}' ready.")


def create_tables():
    conn = get_connection()
    cursor = conn.cursor()

    cursor.execute("""
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LK_TipoBiglietto')
        CREATE TABLE LK_TipoBiglietto (
            Id_TipoBiglietto INT PRIMARY KEY IDENTITY(1,1),
            Descrizione NVARCHAR(200),
            Prezzo DECIMAL(10,2),
            Attivo BIT DEFAULT 1
        )
    """)

    cursor.execute("""
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Biglietto')
        CREATE TABLE Biglietto (
            Id_Biglietto INT PRIMARY KEY IDENTITY(1,1),
            Codice BIGINT NOT NULL,
            Id_VisitaProgrammata INT NOT NULL DEFAULT 0,
            Id_TipoBiglietto INT NOT NULL DEFAULT 1,
            Pax INT NOT NULL,
            DataOraEmissione DATETIME NOT NULL,
            Vidimato BIT NOT NULL DEFAULT 0,
            Annullato BIT NOT NULL DEFAULT 0,
            Passed INT NOT NULL DEFAULT 0,
            DataOraVidimazione DATETIME NULL
        )
    """)

    cursor.execute("""
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Biglietto_Codice')
        CREATE INDEX IX_Biglietto_Codice ON Biglietto(Codice)
    """)

    conn.close()
    print("Tables created.")


def seed_data():
    conn = get_connection()
    cursor = conn.cursor()

    cursor.execute("SELECT COUNT(*) FROM LK_TipoBiglietto")
    if cursor.fetchone()[0] == 0:
        cursor.execute("""
            INSERT INTO LK_TipoBiglietto (Descrizione, Prezzo) VALUES
            ('Intero', 10.00),
            ('Ridotto', 7.00),
            ('Gratuito', 0.00),
            ('Cumulativo', 15.00),
            ('Scolastico', 5.00)
        """)
        print("Ticket types seeded.")
    else:
        print("Ticket types already present.")

    conn.close()


if __name__ == "__main__":
    try:
        create_database()
        create_tables()
        seed_data()
        print("\nDB setup complete.")
    except Exception as e:
        print(f"DB Error: {e}")
        sys.exit(1)
