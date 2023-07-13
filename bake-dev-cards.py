# converts easy-to-read Lua files to json fields
import json
import os.path as path
import os
import shutil

DEV_DIR = 'dev-cards'
TARGET_DIR = 'cards'

manifest = json.loads(open(path.join(DEV_DIR, 'manifest.json'), 'r').read())
shutil.rmtree(TARGET_DIR, ignore_errors=True)
os.mkdir(TARGET_DIR)

open(path.join(TARGET_DIR, 'manifest.json'), 'w').write(json.dumps(manifest, indent=4))

for card_path in manifest:
    if card_path[0] == '!': continue
    card = json.loads(open(path.join(DEV_DIR, card_path), 'r').read())

    card['script'] = open(path.join(DEV_DIR, card['script']), 'r').read()

    open(path.join(TARGET_DIR, card_path), 'w').write(json.dumps(card, indent=4))