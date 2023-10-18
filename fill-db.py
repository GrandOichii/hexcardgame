import json
from os import path
import os
import psycopg2
import sys

CARDS_DIR = 'cards'


def fill():
    cur.execute('''
                DROP TABLE IF EXISTS Expansions CASCADE ;
                CREATE TABLE Expansions (
                    name VARCHAR NOT NULL PRIMARY KEY
                );

                DROP TABLE IF EXISTS Cards CASCADE ;
                CREATE TABLE Cards (
                    name VARCHAR NOT NULL PRIMARY KEY,
                    cost INTEGER NOT NULL,
                    type VARCHAR NOT NULL,
                    text VARCHAR NOT NULL,
                    power INTEGER NOT NULL,
                    life INTEGER NOT NULL,
                    deckUsable BOOLEAN NOT NULL,
                    script TEXT NOT NULL
                );

                DROP TABLE IF EXISTS ExpansionCards CASCADE ;
                CREATE TABLE ExpansionCards (
                    id SERIAL NOT NULL PRIMARY KEY,

                    expansionName VARCHAR NOT NULL,
                    FOREIGN KEY (expansionName) REFERENCES Expansions(name),

                    cardName VARCHAR NOT NULL,
                    FOREIGN KEY (cardName) REFERENCES Cards(name),

                    UNIQUE (expansionName, cardName)
                );

                DROP TABLE IF EXISTS Decks CASCADE ;
                CREATE TABLE Decks (
                    name VARCHAR NOT NULL PRIMARY KEY,
                    description TEXT NOT NULL
                );

                DROP TABLE IF EXISTS DeckCards;
                CREATE TABLE DeckCards (
                    amount INTEGER NOT NULL,

                    deckName VARCHAR,
                    FOREIGN KEY (deckName) REFERENCES Decks(name),

                    cardID INTEGER,
                    FOREIGN KEY (cardID) REFERENCES ExpansionCards(id),
                
                    UNIQUEq (deckName, cardID)
                );

                DROP TABLE IF EXISTS MatchConfigs;
                CREATE TABLE MatchConfigs (
                    name VARCHAR NOT NULL PRIMARY KEY,
                    turnStartDraw INTEGER NOT NULL,
                    seed INTEGER NOT NULL,
                    setupScript TEXT NOT NULL,
                    addons VARCHAR[] NOT NULL,
                    map TEXT
                );

''')
    # fill cards
    manifest = json.loads(open(path.join(CARDS_DIR, 'manifest.json'), 'r').read())
    expansions = set()
    cards = []
    
    for card_path in manifest:
        if card_path[0] == '!': continue
        card = json.loads(open(path.join(CARDS_DIR, card_path), 'r').read())
        cards += [card]
        exp = card['expansion']
        expansions.add(exp)

    for exp in expansions:
        cur.execute(f"INSERT INTO Expansions VALUES ('{exp}');")
    
    cid_map = {}
    for card in cards:
        power = card['power'] if 'power' in card else -1
        life = card['life'] if 'life' in card else -1
        deckUsable = card['deckUsable'] if 'deckUsable' in card else True
        cur.execute("INSERT INTO Cards VALUES (%s, %s, %s, %s, %s, %s, %s, %s);", (card['name'], card['cost'], card['type'], card['text'], power, life, deckUsable, card['script']))
        cur.execute("INSERT INTO ExpansionCards (expansionName, cardName) VALUES (%s, %s);", (card['expansion'], card['name']))
        cur.execute("select id from expansioncards where cardName=%s and expansionName=%s;", (card['name'], card['expansion']))
        id = cur.fetchone()[0]
        cid_map[card['expansion'] + '::' + card['name']] = id

    # fill decks

    deck_paths = os.listdir('decks')
    d_i = 0
    for dp in deck_paths:
        d_i += 1
        dp = path.join('decks', dp)
        if not dp.endswith('.deck'):
            continue
        text = open(dp, 'r').read()
        s = text.split(';')

        # create descriptors
        d_name = 'deck' + str(d_i)
        d_description = ''
        if len(s) > 1:
            descriptors_raw = s[1]
            ss = descriptors_raw.split(',')
            for desc in ss:
                d_s = desc.split('=')
                key = d_s[0]
                value = d_s[1]
                match key:
                    case 'name':
                        d_name = value
                        break
                    case 'description':
                        d_description = value
        cur.execute("INSERT INTO Decks (name, description) VALUES (%s, %s);", (d_name, d_description))
        # create card index
        cards_raw = s[0].split('|')
        for c_raw in cards_raw:
            c_s = c_raw.split('#')
            cid = c_s[0]
            amount = int(c_s[1])
            cur.execute("INSERT INTO DeckCards (amount, deckName, cardID) VALUES (%s, %s, %s);", (amount, d_name, cid_map[cid]))

    # fill configurations
    config_paths = os.listdir('configs')
    for config_path in config_paths:
        if not config_path.endswith('.json'): continue

        config = json.loads(open(path.join('configs', config_path), 'r').read())
        seed = 0
        name = path.splitext(config_path)[0]
        if 'seed' in config:
            seed = config['seed']
        map_raw = config['map']
        m = []
        for row in map_raw:
            m += [''.join([str(i) for i in row])]
        cur.execute("INSERT INTO MatchConfigs VALUES (%s, %s, %s, %s, %s, %s);", (name, config['turnStartDraw'], seed, config['setupScript'], config['addons'], '|'.join(m)))

DB_NAME = sys.argv[1]
PASS = sys.argv[2]

print('Requesting connection')

conn = psycopg2.connect(
    host='localhost',
    database=DB_NAME,
    user='postgres',
    password=PASS
)
print(DB_NAME, PASS)
cur = conn.cursor()

print('Connected')

cur.execute('SELECT version();')
print('Version: ', end='')
ver = cur.fetchone()
print(ver)

fill()
conn.commit()

cur.close()

print('Disconnecting')
conn.close()
print('Disconnected')