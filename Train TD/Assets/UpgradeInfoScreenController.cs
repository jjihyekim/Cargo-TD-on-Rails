using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeInfoScreenController : MonoBehaviour {
    
    [ReadOnly]
    public Upgrade selectedUpgrade;
    
    [Header("Upgrade Info Display")] 
    public Image icon;
    public Image extraInfoImage;
    public TMP_Text moduleName;
    public TMP_Text moduleCost;
    [Space] 
    public TMP_Text upgradeName;
    public TMP_Text upgradeDescription;
    public MiniGUI_UpgradeButton upgradeButton;

    public Sprite armorPenetrationIcon;


    public void ChangeSelectedUpgrade(Upgrade toSelect) {
        selectedUpgrade = toSelect;
        var parentUpgrade = selectedUpgrade.parentUpgrade;
        
        if (previewBuilding == null || previewBuilding.uniqueName != parentUpgrade.module.uniqueName) {
            if (previewBuilding != null) {
                myArea.previewSlots[curSlot].RemoveBuilding(previewBuilding);
                Destroy(previewBuilding.gameObject);
            }

            previewBuilding = Instantiate(parentUpgrade.module.gameObject, myArea.previewSlots[curSlot].transform).GetComponent<TrainBuilding>();
            previewBuilding.transform.localPosition = Vector3.zero;
            previewBuilding.transform.localRotation = Quaternion.identity;
            previewBuilding.CompleteBuilding(false);
			
            myArea.previewSlots[curSlot].AddBuilding(previewBuilding, previewBuilding.mySlotIndex, true);
			
            var rangeShower = previewBuilding.GetComponentInChildren<RangeVisualizer>(true);
            //rangeShower.ChangeVisualizerStatus(true);
            if(rangeShower != null)
                Destroy(rangeShower.gameObject);

            var audioSources = previewBuilding.GetComponentsInChildren<AudioSource>(true);

            foreach (var audio in audioSources) {
                audio.enabled = false;
            }

            var gunModule = previewBuilding.GetComponent<GunModule>();
            if (gunModule != null) {
                extraInfoImage.gameObject.SetActive(gunModule.canPenetrateArmor);
            } else {
                extraInfoImage.gameObject.SetActive(false);
            }
        }

        icon.sprite = parentUpgrade.icon;
        moduleName.text = parentUpgrade.module.displayName;
        if (selectedUpgrade.activationCost >= 0) {
            moduleCost.text = selectedUpgrade.activationCost.ToString();
        } else {
            moduleCost.text = "passive";
        }

        upgradeName.text = selectedUpgrade.upgradeName;
        upgradeDescription.text = selectedUpgrade.upgradeDescription;

        if (selectedUpgrade == parentUpgrade) {
            var parentInfo = parentUpgrade.module.GetComponent<ClickableEntityInfo>();

            if (parentInfo != null) {
                upgradeDescription.text = parentInfo.GetTooltip().text;
            }
        }
        
        upgradeButton.SetUp(selectedUpgrade);
        
        upgradeButton.Refresh();
    }

    private void OnEnable() {
        myArea.previewCam.enabled = true;
    }

    private void OnDisable() {
        myArea.previewCam.enabled = false;
    }

    [Header("Gun Preview Area")] 
    public UpgradePreviewArea myArea;
    private int curSlot = 0;
    public TrainBuilding previewBuilding;

    public Vector3 cartRotate = new Vector3(0, 20, 0);

    public float buildStateSwitchTime = 0.5f;
    private float curTime;
    private int curCycleCount = 0;

    private void Update() {
        myArea.previewCart.transform.Rotate(cartRotate * Time.deltaTime);

        if (curTime <= 0) {
            curTime = buildStateSwitchTime;

            if (curCycleCount < previewBuilding.rotationCount) {
                previewBuilding.CycleRotation(true);
                curCycleCount += 1;
            } else {
                curCycleCount = 0;
                myArea.previewSlots[curSlot].RemoveBuilding(previewBuilding);
                curSlot += 1;
                curSlot %= 2;
                previewBuilding.transform.position = myArea.previewSlots[curSlot].transform.position;
                myArea.previewSlots[curSlot].AddBuilding(previewBuilding, previewBuilding.mySlotIndex, true);
                previewBuilding.SetUpBasedOnRotation();
            }
        } else {
            curTime -= Time.deltaTime;

        }
    }


    public void GetUpgrade() {
        SelectUpgradePanelController.s.GetUpgrade(selectedUpgrade);
    }
}
