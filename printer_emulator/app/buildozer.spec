[app]
title = FGL Ticket Renderer
package.name = fglticketrenderer
package.domain = it.taal

source.dir = .
source.include_exts = py,png,jpg,kv,atlas,json,txt,ttf
source.exclude_dirs = .buildozer,.cache,.venv,__pycache__

version = 1.0.0

requirements = python3,kivy==2.3.1,pillow,filetype

orientation = portrait
fullscreen = 0

[app:android]
permissions = INTERNET,WRITE_EXTERNAL_STORAGE,READ_EXTERNAL_STORAGE

android.api = 33
android.minapi = 21
android.ndk = 25b
android.archs = arm64-v8a
android.allow_backup = True

[buildozer]
log_level = 2
warn_on_root = 1
