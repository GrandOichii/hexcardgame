import json
from os.path import join

EACH_CARD_COUNT = 3
MANA_DRILL_RATIO = 6.0 / 7

DIR = 'cards'
DATA = json.loads(open(join(DIR, 'manifest.json'), 'r').read())

result = []
for line in DATA:
    if line[0] == '!': continue
    card_data = json.loads(open(join(DIR, line), 'r').read())
    if ('deckUsable' in card_data and card_data['deckUsable'] == False):
        continue
    result += [card_data['expansion'] + '::' + card_data['name'] + f'#{EACH_CARD_COUNT}']
# result += [f'dev::Mana Drill#{int(len(result) * EACH_CARD_COUNT * MANA_DRILL_RATIO)}']

open('decks/generated.deck', 'w').write('|'.join(result))