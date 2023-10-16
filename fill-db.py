import json
from os import path
import psycopg2
import sys

CARDS_DIR = 'cards'


def fill_cards():
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
                expansion VARCHAR NOT NULL,
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

                    card INTEGER,
                    FOREIGN KEY (card) REFERENCES ExpansionCards(id)
                );

''')
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
        print(exp)
    cur.execute('SELECT * FROM Expansions;')
    print(cur.fetchall())

def fill_decks():
    # cur.execute('create table decks()')
    pass

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

fill_cards()
conn.commit()

cur.close()

print('Disconnecting')
conn.close()
print('Disconnected')