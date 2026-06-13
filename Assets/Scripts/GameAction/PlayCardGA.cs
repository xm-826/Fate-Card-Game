using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayCardGA : GameAction
{
    public Card Card {get;set;}
    public PlayCardGA(Card card)
    {
        Card = card;
    }
}
