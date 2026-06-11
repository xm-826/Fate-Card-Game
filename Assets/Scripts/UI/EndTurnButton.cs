using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    public void OnClick()
    {
        
        EnemyTurnGA enemyTurnGA = new();
        ActionSystem.Instance.PerForm(enemyTurnGA);
    }
}
