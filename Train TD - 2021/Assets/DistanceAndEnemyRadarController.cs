using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DistanceAndEnemyRadarController : MonoBehaviour {
    public static DistanceAndEnemyRadarController s;

    private void Awake() {
        s = this;
    }

    public List<IShowOnDistanceRadar> myUnits = new List<IShowOnDistanceRadar>();

    public Transform unitsParent;
    public GameObject unitsPrefab;
    public GameObject trainPrefab;

    public List<GameObject> unitDisplays = new List<GameObject>();

    public RectTransform unitsArea;

    public int playerTrainStaticLocation = 60;
    public float playerTrainCurrentLocation;

    [NonSerialized]
    public float UISizeMultiplier = 4;

    public void RegisterUnit(IShowOnDistanceRadar unit) {
        myUnits.Add(unit);
        if (unit.IsTrain()) {
            unitDisplays.Add(Instantiate(trainPrefab, unitsParent));
            unitDisplays[unitDisplays.Count - 1].transform.GetChild(0).GetComponent<Image>().sprite = unit.GetIcon();
        } else {
            unitDisplays.Add(Instantiate(unitsPrefab, unitsParent));
            unitDisplays[unitDisplays.Count - 1].GetComponent<MiniGUI_RadarUnit>().SetUp(unit.GetIcon(), unit.isLeftUnit(), -1);
            
            var totalDistance = SpeedController.s.missionDistance;
            var width = unitsArea.rect.width;
            var percentage = unit.GetDistance() / totalDistance;
            var distance = percentage * width;
            unitDisplays[unitDisplays.Count - 1].GetComponent<RectTransform>().anchoredPosition = new Vector2(distance, baseHeight);
        }

        Update();
    }

    public void RemoveUnit(IShowOnDistanceRadar unit) {
        var index = myUnits.IndexOf(unit);

        if (index > 0) {
            myUnits.RemoveAt(index);
            Destroy(unitDisplays[index]);
            unitDisplays.RemoveAt(index);
        }
    }

    public float baseHeight = 25f;
    public float increaseHeight = 25f;
    // Update is called once per frame
    void Update() {
        var totalDistance = SpeedController.s.missionDistance;
        var width = unitsArea.rect.width;

        var lastDistance = float.NegativeInfinity;
        var curHeight = 0f;
        //myUnits.Sort((x, y) => x.GetDistance().CompareTo(y.GetDistance()));
        for (int i = 0; i < myUnits.Count; i++) {

            /*var percentage = myUnits[i].GetDistance() / totalDistance;
            var distance = percentage * width;*/
            var distance = myUnits[i].GetDistance();

            if (Mathf.Abs(lastDistance - distance) < 25) {
                curHeight += 1;
            } else {
                curHeight = 0;
            }

            if (myUnits[i].IsTrain()) {
                playerTrainCurrentLocation = distance;
                if (distance > playerTrainStaticLocation) {
                    distance = playerTrainStaticLocation;
                }
            } else {
                if (playerTrainCurrentLocation > playerTrainStaticLocation) {
                    distance -= playerTrainCurrentLocation - playerTrainStaticLocation;
                }
            }
            
            unitDisplays[i].GetComponent<RectTransform>().anchoredPosition =
                Vector2.Lerp(
                    unitDisplays[i].GetComponent<RectTransform>().anchoredPosition,
                    new Vector2(distance*UISizeMultiplier, baseHeight + (curHeight * increaseHeight)),
                    10 * Time.deltaTime
                );

            lastDistance = distance;
        }
    }

    public void ClearRadar() {
        for (int i = 0; i < unitDisplays.Count; i++) {
            Destroy(unitDisplays[i]);
        }
        
        myUnits.Clear();
        unitDisplays.Clear();
    }
}

public interface IShowOnDistanceRadar {
    public bool IsTrain();
    public float GetDistance();
    public Sprite GetIcon();
    public bool isLeftUnit();
}
