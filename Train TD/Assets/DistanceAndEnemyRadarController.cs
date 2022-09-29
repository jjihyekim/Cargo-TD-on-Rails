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
        
        myUnits.RemoveAt(index);
        Destroy(unitDisplays[index]);
        unitDisplays.RemoveAt(index);
        
        Update();
    }

    // Update is called once per frame
    void Update() {
        var totalDistance = SpeedController.s.missionDistance;
        var width = unitsArea.rect.width;
        
        for (int i = 0; i < myUnits.Count; i++) {

            var percentage = myUnits[i].GetDistance() / totalDistance;

            unitDisplays[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(percentage * width, 25);
        }
    }
}

public interface IShowOnDistanceRadar {
    public float GetDistance();
    public Sprite GetIcon();
}
