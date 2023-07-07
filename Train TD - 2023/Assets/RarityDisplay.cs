using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RarityDisplay : MonoBehaviour {

    public Material common;
    public Material rare;
    public Material epic;
    public Material special;
    
    // Start is called before the first frame update
    void Start() {
        var cart = GetComponentInParent<Cart>();
        if(cart == null)
            return;

        var renderers = GetComponentsInChildren<MeshRenderer>();
        Material material = common;

        switch (cart.myRarity) {
            case UpgradesController.CartRarity.epic:
                material = epic;
                break;
            case UpgradesController.CartRarity.rare:
                material = rare;
                break;
            case UpgradesController.CartRarity.special :
                material = special;
                break;
        }

        for (int i = 0; i < renderers.Length; i++) {
            renderers[i].material = material;
        }
    }
}
