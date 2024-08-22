using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlcoholMachine : CocktailMachine
{
    public override void OnClickDecisionButton()
    {
        if (_isUsed is true)
        {
            return;
        }
        _isUsed = true;
    }
}
