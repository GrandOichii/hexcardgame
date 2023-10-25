using System.Collections;
using System.Collections.Generic;
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

    public CardData Data { get; private set; }


}