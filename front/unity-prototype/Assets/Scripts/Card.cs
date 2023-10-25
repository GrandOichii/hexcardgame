using TMPro;
using UnityEngine;

public class Card : MonoBehaviour
{
    public TMP_Text nameDisplay;
    public TMP_Text costDisplay;
    public TMP_Text typeLineDisplay;
    public TMP_Text textDisplay;
    public TMP_Text powerDisplay;
    public TMP_Text lifeDisplay;

    private CardState _data;
    public CardState Data {
        get => _data;
        set {
            _data = value;

            nameDisplay.text = _data.name;
            costDisplay.text = _data.cost.ToString();
            typeLineDisplay.text = _data.type;
            textDisplay.text = _data.text;
            powerDisplay.text = _data.power.ToString();
            lifeDisplay.text = _data.life.ToString();
        }
    }
}