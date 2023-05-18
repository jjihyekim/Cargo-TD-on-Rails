using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RarityDisplay : MonoBehaviour {

    public Material common;
    public Material rare;
    public Material epic;
    
    // Start is called before the first frame update
    void Start() {
        var cart = GetComponentInParent<Cart>();

        var renderers = GetComponentsInChildren<MeshRenderer>();
        Material material = common;

        switch (cart.myRarity) {
            case UpgradesController.CartRarity.epic:
                material = epic;
                break;
            case UpgradesController.CartRarity.rare:
                material = rare;
                break;
        }

        for (int i = 0; i < renderers.Length; i++) {
            renderers[i].material = material;
        }
    }
}
