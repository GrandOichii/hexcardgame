using core.cards;
using NLua;
using util;

namespace core.match;

public class Zone<T> where T : MCard {

    public List<T> Cards { get; private set; }

    public T? this[string mID] => Cards.Find(card => card.MID == mID);

    public Zone(List<T> cards) {
        Cards = cards;
    }

    public void Shuffle(Random rnd) {
        Cards = Utility.Shuffled(Cards, rnd);
    }

    public List<T> PopTop(int amount) {
        if (amount > Cards.Count) amount = Cards.Count;
        var result = Cards.GetRange(0, amount);
        Cards.RemoveRange(0, amount);
        return result;
    }

    public void AddToBack(List<T> cards) {
        foreach (var card in cards)
            AddToBack(card);
    }

    public void AddToBack(T card) {
        Cards.Add(card);
    }

    public void AddToFront(T card) {
        Cards.Insert(0, card);
    }

    public LuaTable ToLuaTable(Lua lState) {
        var result = LuaUtility.CreateTable(lState);
        for (int i = 0; i < Cards.Count; i++)
            result[i+1] = Cards[i].Data;
        return result;
    }
}
