"""Shared configuration manager — reads/writes config.json."""

import json
import os

# On Android, files are in the app directory. On desktop, check local first, then parent.
_here = os.path.dirname(os.path.abspath(__file__))
_local = os.path.join(_here, "config.json")
_parent = os.path.join(_here, "..", "config.json")
CONFIG_PATH = _local if os.path.exists(_local) else _parent


def load():
    with open(CONFIG_PATH) as f:
        return json.load(f)


def save(cfg):
    with open(CONFIG_PATH, "w") as f:
        json.dump(cfg, f, indent=2)


def get_turnstile():
    return load()["turnstile"]


def get_server():
    return load()["validation_server"]


def get_database():
    return load()["database"]


def update_turnstile(**kwargs):
    cfg = load()
    cfg["turnstile"].update(kwargs)
    save(cfg)
    return cfg["turnstile"]


def update_server(**kwargs):
    cfg = load()
    cfg["validation_server"].update(kwargs)
    save(cfg)
    return cfg["validation_server"]
