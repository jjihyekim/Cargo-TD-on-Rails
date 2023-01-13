using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EncounterRequirement : MonoBehaviour
{
    public Sprite icon;

    public ResourceTypes myType;
    public int amount = 50;
    public bool randomizeAmount = true;
    readonly float randomPercent = 0.1f;

    public void RandomizeRequirement() {
        if (randomizeAmount) {
            amount = (int)(amount * (1 + Random.Range(-randomPercent, randomPercent)));
        }
    }


    public bool CanFulfillRequirement() {
        return MoneyController.s.HasResource(myType, amount);
    }

    public void UseResource() {
        MoneyController.s.ModifyResource(myType, -amount);
    }
}
