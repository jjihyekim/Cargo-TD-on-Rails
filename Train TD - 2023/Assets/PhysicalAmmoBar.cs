using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class PhysicalAmmoBar : MonoBehaviour {


    public GameObject ammoChunk;
    public float ammoChunkHeight;

    public List<GameObject> allAmmoChunks = new List<GameObject>();
    public List<float> velocity = new List<float>();

    public Transform noAmmoPos;
    public Transform reloadSpawnPos;

    [ReadOnly]
    public ModuleAmmo moduleAmmo;
    void Start() {
        moduleAmmo = GetComponentInParent<ModuleAmmo>();
        moduleAmmo.OnReload.AddListener(OnReload);
        moduleAmmo.OnUse.AddListener(OnUse);
        OnReload();
        OnUse();
    }


    void OnUse() {
        while ( allAmmoChunks.Count > moduleAmmo.curAmmo) {
            var firstOne = allAmmoChunks[0];
            allAmmoChunks.RemoveAt(0);
            velocity.RemoveAt(0);
            Destroy(firstOne);
        }
    }
    void OnReload() {
        var delta = Vector3.zero;
        while ( allAmmoChunks.Count < moduleAmmo.curAmmo) {
            var newOne = Instantiate(ammoChunk, reloadSpawnPos);
            newOne.transform.position += delta;
            newOne.SetActive(true);
            allAmmoChunks.Add(newOne);
            velocity.Add(0);

            delta.y += ammoChunkHeight;
        }
    }


    private float acceleration = 2;
    private void Update() {
        var target = noAmmoPos.transform.position;
        for (int i = 0; i < allAmmoChunks.Count; i++) {
            if (allAmmoChunks[i].transform.position.y > target.y) {
                allAmmoChunks[i].transform.position = Vector3.MoveTowards(allAmmoChunks[i].transform.position, target, velocity[i] * Time.deltaTime);
                velocity[i] += acceleration * Time.deltaTime;
            } else {
                velocity[i] = 0;
            }

            target.y += ammoChunkHeight;
        }
    }
}
