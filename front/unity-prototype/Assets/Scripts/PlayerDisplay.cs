using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDisplay : MonoBehaviour
{
    public TMP_Text nameDisplay;
    public TMP_Text energyDisplay;
    public GameObject Background;
    public Color CurrentPlayerColor;
    
    [HideInInspector]
    public int playerI;

    private Color _defaultBgColor;

    private Color _bgColor {
        get => Background.GetComponent<Image>().color;
        set => Background.GetComponent<Image>().color = value;
    }

    public void Start() {
        _defaultBgColor = _bgColor;
    }

    public void Load(MatchState state) {
        var data = state.players[playerI];
    
        nameDisplay.text = data.name;
        energyDisplay.text = "Energy: " + data.energy;

        _bgColor = state.curPlayerID == data.id ? CurrentPlayerColor : _defaultBgColor;
    }
}
