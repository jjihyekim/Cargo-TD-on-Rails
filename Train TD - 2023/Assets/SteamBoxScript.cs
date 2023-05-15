using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SteamBoxScript : MonoBehaviour
{

    public GameObject wheel;
    public Transform lineConnectPoint1;
    public Transform lineConnectPoint2;

    public Transform piston;

    public GameObject[] gatesLeft;
    public GameObject[] gatesRight;

    public LineRenderer line;

    ScrapBoxScript myScrapPile;

    public Transform steamRemovedThreshold;
    public Transform steamDeleteThreshold;

    private void Start() {
        myScrapPile = GetComponent<ScrapBoxScript>();
        for (int i = 0; i < gatesLeft.Length; i++) {
            gatesLeft[i].SetActive(false);
        }
        for (int i = 0; i < gatesLeft.Length; i++) {
            gatesRight[i].SetActive(true);
        }
    }

    public float wheelSegment1Length = 0.34f;
    public float wheelSegment2Length = 0.45f;

    public float wheelSpeedPerRealSpeed = -30;

    public List<ScrapPieceScript> removedSteam = new List<ScrapPieceScript>();

    private float magicZ = 7.685041f;
    
    void Update()
    {
        wheel.transform.Rotate(0,0,LevelReferences.s.speed*wheelSpeedPerRealSpeed*Time.deltaTime);

        Vector3 wheelPos = lineConnectPoint1.position;
        wheelPos.z = magicZ;
        
        line.SetPosition(0, wheelPos);

        Vector3 midPos = lineConnectPoint2.position;
        midPos.z = magicZ;

        var height = wheel.transform.position.y - lineConnectPoint1.transform.position.y;
        var length = wheelSegment1Length;
        // height^2 + xpos^2 = length^2
        // xpos = root (length^2-heigth^2)
        var xPos = Mathf.Sqrt((length * length) -  (height * height));
        midPos.x = xPos + wheelPos.x;
        line.SetPosition(1, midPos);

        midPos.x += wheelSegment2Length;
        piston.transform.position = midPos;
        
        Vector3 pistonPos = lineConnectPoint2.position;
        pistonPos.z = magicZ;
        
        line.SetPosition(2, pistonPos);

        if (height > 0) {
                for (int i = 0; i < gatesLeft.Length; i++) {
                    gatesLeft[i].SetActive(false);
                }
                for (int i = 0; i < gatesLeft.Length; i++) {
                    gatesRight[i].SetActive(true);
                }
            
        } else {
                for (int i = 0; i < gatesLeft.Length; i++) {
                    gatesLeft[i].SetActive(true);
                }

                for (int i = 0; i < gatesLeft.Length; i++) {
                    gatesRight[i].SetActive(false);
                }
        }


        var removeThreshold = steamRemovedThreshold.position.y;
        var deleteThreshold = steamDeleteThreshold.position.y;
        
        for (int i = myScrapPile.myScraps.Count-1; i >= 0; i--) {
            var curScrap = myScrapPile.myScraps[i];
            if (curScrap.transform.position.y > removeThreshold) {
                myScrapPile.myScraps.Remove(curScrap);
                myScrapPile.curScrap -= curScrap.myAmount;
                removedSteam.Add(curScrap);
            }
        }

        for (int i = removedSteam.Count -1; i >=0 ; i--) {
            var curScrap = removedSteam[i];
            if (curScrap.transform.position.y > deleteThreshold) {
                removedSteam.Remove(curScrap);
                curScrap.DestroySelf();
            }
        }
    }
}
