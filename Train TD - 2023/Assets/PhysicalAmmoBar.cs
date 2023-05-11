using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class PhysicalAmmoBar : MonoBehaviour {


    public GameObject ammoBox;

    public GameObject currentAmmoBox;

    public Transform noAmmoPos;

    public Transform fullAmmoPos;

    public Transform reloadSpawnPos;

    [ReadOnly]
    public ModuleAmmo moduleAmmo;
    void Start() {
        moduleAmmo = GetComponentInParent<ModuleAmmo>();
        currentAmmoBox = Instantiate(ammoBox, transform);
        moduleAmmo.OnReload.AddListener(OnReload);
        moduleAmmo.OnUse.AddListener(OnUse);
    }


    void OnUse() {
        currentAmmoBox.transform.position = Vector3.Lerp(noAmmoPos.position, fullAmmoPos.position, moduleAmmo.AmmoPercent());
        if (moduleAmmo.curAmmo == 0) {
            currentAmmoBox.SetActive(false);
        } else {
            currentAmmoBox.SetActive(true);
        }
    }
    void OnReload() {
        var oldAmmoBox = currentAmmoBox;
        currentAmmoBox = Instantiate(ammoBox, transform);
        currentAmmoBox.transform.position = reloadSpawnPos.position;
        currentAmmoBox.SetActive(true);
        Destroy(oldAmmoBox, 0);

        StartCoroutine(PlayAmmoBoxAnim(currentAmmoBox));
    }

    private float acceleration = 2;
    IEnumerator PlayAmmoBoxAnim(GameObject box) {
        var speed = 0f;
        
        var targetPos = Vector3.Lerp(noAmmoPos.position, fullAmmoPos.position, moduleAmmo.AmmoPercent());
        //Debug.Break();
        

        while (Vector3.Distance(targetPos, box.transform.position) > 0.01f) {
            targetPos = Vector3.Lerp(noAmmoPos.position, fullAmmoPos.position, moduleAmmo.AmmoPercent());
            box.transform.position = Vector3.MoveTowards(box.transform.position, targetPos, speed * Time.deltaTime);
            speed += acceleration * Time.deltaTime;
            yield return null;

            if (box == null) {
                yield break;
            }
        }
        
        targetPos = Vector3.Lerp(noAmmoPos.position, fullAmmoPos.position, moduleAmmo.AmmoPercent());
        box.transform.position = targetPos;
    }
}
