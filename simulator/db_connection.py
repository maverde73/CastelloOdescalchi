"""Shared DB connection helper using pymssql. Reads config from config.json."""

import json
import os
import pymssql

_config_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "config.json")


def _load_db_config():
    try:
        with open(_config_path) as f:
            return json.load(f).get("database", {})
    except (FileNotFoundError, json.JSONDecodeError):
        return {}


def _cfg():
    c = _load_db_config()
    return {
        "server": c.get("host", "localhost"),
        "port": c.get("port", 1433),
        "user": c.get("user", "sa"),
        "password": c.get("password", "Password15!"),
        "database": c.get("database", "Castello_Odescalchi_Bracciano_Visite"),
    }


def get_connection(database=None):
    c = _cfg()
    return pymssql.connect(
        server=c["server"], port=c["port"], user=c["user"], password=c["password"],
        database=database or c["database"], autocommit=True
    )


def get_master_connection():
    c = _cfg()
    return pymssql.connect(
        server=c["server"], port=c["port"], user=c["user"], password=c["password"],
        autocommit=True
    )


# Expose for external use
DATABASE = _cfg()["database"]
