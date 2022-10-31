using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGUI_BuyBuilding : MonoBehaviour {
    public TrainBuilding myBuilding;
    public TrainModuleHolder holder;

    public int moneyCost = 25;
    //public int rewardCount = 1;

    public TMP_Text nameText;
    public Image icon;

    public Button myButton;
    public TMP_Text costText;

    public void Setup(TrainModuleHolder holder) {
        this.holder = holder;
        myBuilding = DataHolder.s.GetBuilding(holder.moduleUniqueName);
        moneyCost = myBuilding.localShopCost;
        nameText.text = myBuilding.displayName;
        icon.sprite = myBuilding.Icon;
        costText.text = moneyCost.ToString();
    }

    public void Buy() {
        curRun.myResources.money -= moneyCost;

        UpgradesController.s.AddModulesToAvailableModules(myBuilding, 1);
        ModuleRewardsMaster.s.ShopModuleBought(holder);
        
        Destroy(gameObject);
    }


    private DataSaver.RunState curRun;


    private void Start() {
        curRun = DataSaver.s.GetCurrentSave().currentRun;
    }

    private void Update() {
        if (DataSaver.s.GetCurrentSave().isInARun) {
            myButton.interactable = curRun.myResources.money >= moneyCost;
        }
    }
}
