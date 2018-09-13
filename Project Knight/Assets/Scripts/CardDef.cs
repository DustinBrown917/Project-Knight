using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CardDef
{
    [SerializeField]
    protected string _focusImage = "Card Images\\Focus Images\\Default";
    public string FocusImage { get { return _focusImage; } }

    [SerializeField]
    protected string _backgroundImage = "Card Images\\Background Images\\Default";
    public string BackgroundImage { get { return _backgroundImage; } }

    [SerializeField]
    protected string _backsideImage = "Card Images\\Backside Images\\Default";
    public string BacksideImage { get { return _backsideImage; } }

}

[Serializable]
public class MovementCardDef : CardDef
{
    [SerializeField]
    private GridAddress[] targetTiles;
}
