using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInfoScreenController : MonoBehaviour {
    
    [ReadOnly]
    public TrainBuilding selectedBuilding;
    
    [Header("Upgrade Info Display")] 
    public Image icon;
    public Image extraInfoImage;
    public TMP_Text moduleName;
    [Space] 
    public TMP_Text upgradeDescription;


    public void ChangeSelectedUpgrade(string toSelect) {
        selectedBuilding = DataHolder.s.GetBuilding(toSelect);
        
        if (previewBuilding == null || previewBuilding.uniqueName != selectedBuilding.uniqueName) {
            if (previewBuilding != null) {
                myArea.previewSlots[curSlot].RemoveBuilding(previewBuilding);
                Destroy(previewBuilding.gameObject);
            }

            previewBuilding = Instantiate(selectedBuilding.gameObject, myArea.previewSlots[curSlot].transform).GetComponent<TrainBuilding>();
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

        icon.sprite = selectedBuilding.Icon;
        moduleName.text = selectedBuilding.displayName;
        
        upgradeDescription.text = selectedBuilding.GetComponent<ClickableEntityInfo>().GetTooltip().text;
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
        SelectTrainBuildingPanelController.s.GetBuilding(selectedBuilding.uniqueName);
    }
}
