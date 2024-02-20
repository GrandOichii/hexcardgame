import json
from os import path
from pymongo import MongoClient


CARDS_DIR = '../cards'
CONNECTION_STRING = 'mongodb://localhost:27017'

manifest = json.loads(open(path.join(CARDS_DIR, 'manifest.json'), 'r').read())
cards = []

for card_path in manifest:
    if card_path[0] == '!': continue
    card = json.loads(open(path.join(CARDS_DIR, card_path), 'r').read())
    result = {
        'Power': card['power'] if 'power' in card else -1,
        'Life': card['power'] if 'power' in card else -1,
        'DeckUsable': card['deckUsable'] if 'deckUsable' in card else True,
        'Name': card['name'],
        'Cost': card['cost'],
        'Type': card['type'],
        'Expansion': card['expansion'],
        'Text': card['text'],
        'Script': card['script']
    }
    
    cards += [result]


client = MongoClient(CONNECTION_STRING)
client['hex']['cards'].insert_many(cards)