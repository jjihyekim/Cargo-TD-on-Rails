using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityStandardAssets.Utility;

public class MiniGUI_IncomingWave : MonoBehaviour {
    public Vector3 sourcePosition;
    private RectTransform CanvasRect;
    private RectTransform ParentRect;
    private RectTransform UIRect;
    private Camera mainCam;

    public EnemyWave myWave;

    public TMP_Text text;

    public float counter;

    

    public void SetUp(EnemyWave enemyWave) {
        myWave = enemyWave;
        sourcePosition = enemyWave.myCircuit.Waypoints[1].position;
        sourcePosition = new Vector3(sourcePosition.x * (myWave.myData.isLeft ? 1 : -1), sourcePosition.y, sourcePosition.z);
        CanvasRect = transform.root.GetComponent<RectTransform>();
        UIRect = GetComponent<RectTransform>();
        ParentRect = transform.parent.GetComponent<RectTransform>();
        mainCam = LevelReferences.s.mainCam;
        counter = myWave.myData.headsUpTime;
        Update();
    }

    const float edgeGive = 5;
    private void Update() {
        counter -= Time.deltaTime;
        SetPosition();
        SetText();
    }

    private void LateUpdate() {
        AdjustPositionBasedOnOtherPositions();
    }

    private void SetPosition() {
        //then you calculate the position of the UI element
        //0,0 for the canvas is at the center of the screen, whereas WorldToViewPortPoint
        //treats the lower left corner as 0,0. Because of this, you need to subtract the height / width of the canvas * 0.5 to get the correct position.
        Vector2 ViewportPosition = mainCam.WorldToViewportPoint(sourcePosition);
        Vector2 WorldObject_ScreenPosition = new Vector2(
            ((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
            ((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)));

        //now you can set the position of the ui element
        var halfWidthLimit = (CanvasRect.rect.width - (UIRect.rect.width + edgeGive)) / 2f;
        var halfHeightLimit = (CanvasRect.rect.height - (UIRect.rect.height + edgeGive)) / 2f;
        WorldObject_ScreenPosition.x = Mathf.Clamp(WorldObject_ScreenPosition.x,
            -halfWidthLimit,
            halfWidthLimit
        );
        WorldObject_ScreenPosition.y = Mathf.Clamp(WorldObject_ScreenPosition.y,
            -halfHeightLimit + (0.2f * CanvasRect.rect.height),
            halfHeightLimit - (0.2f * CanvasRect.rect.height)
        );

        myVecRef.vector2 = WorldObject_ScreenPosition;
        UIRect.anchoredPosition = WorldObject_ScreenPosition;
    }
    
    class Vector2Reference {
        public Vector2 vector2;
    }
    
    static List<Vector2Reference> globalPositions = new List<Vector2Reference>();
    private Vector2Reference myVecRef;
    
    private void OnEnable() {
        myVecRef = new Vector2Reference();
        globalPositions.Add(myVecRef);
    }

    private void OnDisable() {
        globalPositions.Remove(myVecRef);
    }

    void AdjustPositionBasedOnOtherPositions() {
        for (int i = 0; i < globalPositions.Count; i++) {
            var curPos = globalPositions[i];
            if (curPos != myVecRef) {
                if (Vector2.Distance(curPos.vector2, myVecRef.vector2) < UIRect.sizeDelta.x) {
                    myVecRef.vector2.x += UIRect.sizeDelta.x;
                }
            }
        }

        Vector2 adjustedPosition = myVecRef.vector2;

        UIRect.anchoredPosition = adjustedPosition;
    }

    void SetText() {
        text.text = myWave.myData.enemyUniqueName+ ((myWave.myData.enemyData != -1) ?  " " + myWave.myData.enemyData.ToString() : "") + "\n" + GetTime(counter);
    }

    string GetTime(float time) {
        var minutes = (int) (time / 60);
        var remainingSeconds = (int) (time - minutes * 60);
        return (minutes.ToString("00") + ':' + remainingSeconds.ToString("00"));
    }


    public void PointerEnter() {
        myWave.ShowPath();
    }

    public void PointerExit() {
        myWave.HidePath();
    }
}
