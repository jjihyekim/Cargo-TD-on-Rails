using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSpaceDebug : MonoBehaviour {

    public GameObject[] toCycle;
    private int cycle = 0;

    public float timer;
    void Update()
    {
        //Debug.Log(QualitySettings.activeColorSpace);

        timer += Time.deltaTime;
        if(timer > 2){
            for (int i = 0; i < toCycle.Length; i++) {
                if (toCycle[i] != null) {
                    toCycle[i].SetActive(false);
                }
            }

            if (toCycle[cycle] != null) {
                toCycle[cycle].SetActive(true);
            }

            cycle += 1;

            timer = 0;

            if (cycle >= toCycle.Length) {
                cycle = 0;
            }
        }
    }
}
