using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

public class Encounter_CompleteWithReward : RandomEncounter {

    public RandomResourceReward[] myPossibleRewards;
    private RandomResourceReward chosenReward;
    
    public override bool[] SetOptionTexts() {
        chosenReward = myPossibleRewards[Random.Range(0, myPossibleRewards.Length)];
        chosenReward.RandomizeReward();
        
        
        options[0] = options[0].Replace("[amount]", $"{chosenReward.amount}");
        options[0] = options[0].Replace("[type]", $"{DataSaver.RunResources.GetTypeInNiceString(chosenReward.myType)}");

        
        return null;
    }


    [Title("this one only should have one option to pick which is to get reward and continue")]
    public override RandomEncounter OptionPicked(int option) {

        var reward = myPossibleRewards[Random.Range(0, myPossibleRewards.Length)];

        reward.GainReward();
        return null;
    }
}



[Serializable]
public class RandomResourceReward {
    public DataSaver.RunResources.Types myType;
    public int amount = 50;
    public float randomPercent = 0.1f;

    public void RandomizeReward() {
        amount = (int)(amount * (1+ Random.Range(-randomPercent, randomPercent)));
    }
    public void GainReward() {
        DataSaver.s.GetCurrentSave().currentRun.myResources.AddResource((int)amount, myType);
        DataSaver.s.SaveActiveGame();
    }
}