import json
from os import path, listdir
from pymongo import MongoClient


CARDS_DIR = '../cards'
CONFIGS_DIR = '../configs'
CORE_FILE_PATH = '../HexCore/core.lua'
CONNECTION_STRING = 'mongodb://localhost:27017'

def fill_cards():
    manifest = json.loads(open(path.join(CARDS_DIR, 'manifest.json'), 'r').read())
    cards = []

    for card_path in manifest:
        if card_path[0] == '!': continue
        card = json.loads(open(path.join(CARDS_DIR, card_path), 'r').read())
        result = {
            'Power': card['power'] if 'power' in card else -1,
            'Life': card['life'] if 'life' in card else -1,
            'DeckUsable': card['deckUsable'] if 'deckUsable' in card else True,
            'Name': card['name'],
            'Cost': card['cost'],
            'Type': card['type'],
            'Expansion': card['expansion'],
            'Text': card['text'],
            'Script': card['script']
        }
        
        cards += [result]

    client['hex']['cards'].insert_many(cards)

def fill_match_scripts():
    script = open(CORE_FILE_PATH, 'r').read()
    client['hex']['matchScripts'].insert_one({
        'Name': 'core',
        'Script': script
    })


client = MongoClient(CONNECTION_STRING)

def fill_configs():
    configs = []
    for config_path in listdir(CONFIGS_DIR):
        try:
            config = json.loads(open(path.join(CONFIGS_DIR, config_path), 'r').read())
            config = {
                'StartingHandSize': config['startingHandSize'],
                'TurnStartDraw': config['turnStartDraw'],
                'SetupScript': config['setupScript'],
                'Map': config['map'],
                'AddonPaths': config['addonPaths'],
                'Name': path.splitext(config_path)[0]
            }
            configs += [config]
        except:
            continue
    client['hex']['configs'].insert_many(configs)

# fill_cards()
# fill_configs()
fill_match_scripts()

