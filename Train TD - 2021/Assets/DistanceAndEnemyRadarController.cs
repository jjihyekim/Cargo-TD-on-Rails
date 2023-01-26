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

    public List<GameObject> unitDisplays = new List<GameObject>();

    public RectTransform unitsArea;

    public void RegisterUnit(IShowOnDistanceRadar unit) {
        myUnits.Add(unit);
        unitDisplays.Add(Instantiate(unitsPrefab, unitsParent));
        unitDisplays[unitDisplays.Count - 1].GetComponentInChildren<Image>().sprite = unit.GetIcon();
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

            var percentage = myUnits[i].GetDistance() / totalDistance;
            var distance = percentage * width;

            if (Mathf.Abs(lastDistance - distance) < 25) {
                curHeight += 1;
            } else {
                curHeight = 0;
            }

            unitDisplays[i].GetComponent<RectTransform>().anchoredPosition = 
                Vector2.Lerp(
                    unitDisplays[i].GetComponent<RectTransform>().anchoredPosition, 
                    new Vector2(distance, baseHeight + (curHeight*increaseHeight)), 
                    10*Time.deltaTime
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
    public float GetDistance();
    public Sprite GetIcon();
}
