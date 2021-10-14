using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScrapCollector : MonoBehaviour {
    public GameObject ring;
    public float ringCollectionRange = 1f;
    public float ringHeight = 0.5f;


    public void DisableRingBeforeLevelStart() {
        ring.gameObject.SetActive(false);
    }

    public void ActivateRingOnLevelStart() {
        ring.gameObject.SetActive(true);
        ChangeRingVisibility(false);
    }

    public Material ringMaterial;
    private void Start() {
        ring.transform.SetParent(LevelReferences.s.playerTransform);
        ringMaterial = ring.GetComponentInChildren<Renderer>().material;
        DisableRingBeforeLevelStart();
    }

    private float ringVisibleTimer = 0;
    public float ringStayVisibleTime = 1f;
    void Update() {
        if (SceneLoader.s.isLevelInProgress) {
            ring.transform.position = GetMousePositionOnPlane();

            var removedAny = false;
            for (int i = 0; i < LevelReferences.allScraps.Count; i++) {
                var scrap = LevelReferences.allScraps[i];
                if (scrap != null) {
                    if (Vector3.Distance(ring.transform.position, scrap.transform.position) < ringCollectionRange) {
                        scrap.CollectPile();
                        LevelReferences.allScraps.Remove(scrap);
                        removedAny = true;
                    }
                }
            }

            if (removedAny) {
                ringVisibleTimer = ringStayVisibleTime;
                LevelReferences.allScraps.TrimExcess();
            }

            if (ringVisibleTimer <= 0) {
                ChangeRingVisibility(false);
            } else {
                ChangeRingVisibility(true);
                ringVisibleTimer -= Time.deltaTime;
            }
        }
    }

    public float ringVisibleAlpha = 1f;
    public float ringGoVisibleSpeed = 10f;
    public float ringInvisibleAlpha = 0.07f;
    public float ringGoInvisibleSpeed = 1f;
    public void ChangeRingVisibility(bool isVisible) {
        if (isVisible) {
            ringMaterial.color = new Color(1, 1, 1, 
                Mathf.Lerp(ringMaterial.color.a, ringVisibleAlpha, ringGoVisibleSpeed * Time.deltaTime)
                );
        }else {
            ringMaterial.color = new Color(1, 1, 1, 
                Mathf.Lerp(ringMaterial.color.a, ringInvisibleAlpha, ringGoInvisibleSpeed * Time.deltaTime)
            );
        }
    }

    Vector3 GetMousePositionOnPlane() {
        Plane plane = new Plane(Vector3.up, new Vector3(0,ringHeight,0));

        float distance;
        Ray ray = LevelReferences.s.mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        } else {
            return Vector3.zero;
        }
    }
}
