using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_LevelButton : MonoBehaviour {
    [ReadOnly]
    public LevelData myData;
    public TMP_Text levelName;

    public Image selectedBg;
    public Color selectedColor;
    public Color notSelectedColor;

    public Button selectButton;
    public TMP_Text selectButtonText;

    public Image winStar;
    public Image[] speedStars;
    public Image[] cargoStars;

    public GameObject notUnlockedOverlay;
    public TMP_Text starRequirement;



    public Color starActiveColor = Color.white;
    public Color starLostColor = Color.grey;

    public MiniGUI_LevelButton SetUp(LevelData data) {
        myData = data;
        levelName.text = myData.levelName;

        var mySave = DataSaver.s.GetCurrentSave();
        var mySaveIndex = mySave.missionStatsList.FindIndex(x => x.levelName == myData.levelName);

        if (mySaveIndex != -1) {
            var mySaveData = mySave.missionStatsList[mySaveIndex];
            winStar.color = mySaveData.isWon ? starActiveColor : starLostColor;
            SetStarAmount(speedStars, mySaveData.speedStars);
            SetStarAmount(cargoStars, mySaveData.cargoStars);
        } else {
            winStar.color = false ? starActiveColor : starLostColor;
            SetStarAmount(speedStars, 0);
            SetStarAmount(cargoStars, 0);
        }
        
        SetLevelLockedStatus();

        return this;
    }

    public void SetLevelLockedStatus() {
        if (myData.reputationRequirement > DataSaver.s.GetCurrentSave().reputation) {
            notUnlockedOverlay.SetActive(true);
            starRequirement.text = myData.reputationRequirement.ToString();
        } else {
            notUnlockedOverlay.SetActive(false);
        }
    }


    public void SetSelected(bool isSelected) {
        if (isSelected) {
            selectButton.interactable = false;
            selectButtonText.text = "Selected";
            selectedBg.color = selectedColor;
        } else {
            selectButton.interactable = true;
            selectButtonText.text = "Select";
            selectedBg.color = notSelectedColor;
        }
    }


    public void StartLevel() {
        StarterUIController.s.SelectLevel(myData);
    }
    
    void SetStarAmount(Image[] stars, int amount) {
        if(stars[0] == null || stars[1] == null)
            return;
		
        switch (amount) {
            case 2:
                stars[0].color = starActiveColor;
                stars[1].color = starActiveColor;
                break;
            case 1:
                stars[0].color = starActiveColor;
                stars[1].color = starLostColor;
                break;
            case 0:
                stars[0].color = starLostColor;
                stars[1].color = starLostColor;
                break;
        }
    }
}
