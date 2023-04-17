using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleGraphics : MonoBehaviour {
    public GameObject activeGfx;
    public GameObject hologramGfx;

    private Renderer[] _renderers;

    private void Start() {
        _renderers = hologramGfx.GetComponentsInChildren<Renderer>(true);
            
        for (int i = 0; i < _renderers.Length; i++) {
            _renderers[i].material =  LevelReferences.s.hologramBuildable;
        }
    }


    public void SetBuildingMode(bool isBuildingHologram, bool isBuildable) {
        activeGfx.SetActive(!isBuildingHologram);
        hologramGfx.SetActive(isBuildingHologram);

        if (isBuildingHologram) {
            if(_renderers == null || _renderers.Length == 0)
                _renderers = hologramGfx.GetComponentsInChildren<Renderer>(true);
            
            for (int i = 0; i < _renderers.Length; i++) {
                _renderers[i].material = isBuildable ? LevelReferences.s.hologramBuildable : LevelReferences.s.hologramCantBuild;
            }
        }
    }
}
