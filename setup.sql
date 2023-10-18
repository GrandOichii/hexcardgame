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
    FOREIGN KEY (expansionName) REFERENCES Expansions(name) ON DELETE CASCADE,

    cardName VARCHAR NOT NULL,
    FOREIGN KEY (cardName) REFERENCES Cards(name) ON DELETE CASCADE,

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
    FOREIGN KEY (deckName) REFERENCES Decks(name) ON DELETE CASCADE,

    cardID INTEGER,
    FOREIGN KEY (cardID) REFERENCES ExpansionCards(id) ON DELETE CASCADE,

    UNIQUE (deckName, cardID)
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