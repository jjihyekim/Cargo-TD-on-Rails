using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class MiniGUI_LevelEditorLevelButton : MonoBehaviour
{
    [ReadOnly]
    public LevelDataJson myData;
    public TMP_Text levelName;

    public Image selectedBg;
    public Color selectedColor;
    public Color notSelectedColor;

    public Button selectButton;
    public TMP_Text selectButtonText;
    

    public MiniGUI_LevelEditorLevelButton SetUp(LevelDataJson data) {
        myData = data;
        levelName.text = myData.levelName;
        return this;
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


    public void SelectLevel() {
        LevelEditorController.s.SelectLevel(myData);
    }

    public void ChangeLevelOrder(int order) {
        Assert.IsTrue(order == 1 || order == -1);

        var pastIndex = 0;
        var newIndex = 1;
        if (order > 0) {
            pastIndex = myData.levelMenuOrder;
            newIndex = myData.levelMenuOrder + order;
        } else {
            pastIndex = myData.levelMenuOrder;
            newIndex = myData.levelMenuOrder - order;
        }

        if (pastIndex >= 0 && newIndex >= 0 && pastIndex < LevelDataLoader.s.allLevels.Count && newIndex < LevelDataLoader.s.allLevels.Count) {
            var temp = LevelDataLoader.s.allLevels[newIndex];
            temp.levelMenuOrder = pastIndex;
            LevelDataLoader.s.allLevels[newIndex] = myData;
            LevelDataLoader.s.allLevels[pastIndex] = temp;
            myData.levelMenuOrder = newIndex;

            LevelDataLoader.s.SaveLevel(myData);
            LevelDataLoader.s.SaveLevel(temp);
        }
        
        transform.SetSiblingIndex(newIndex);
    }
    
}
