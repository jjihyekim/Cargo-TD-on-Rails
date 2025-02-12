using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Artifact : MonoBehaviour
{
    public string displayName = "Unnamed But Nice in game name";
    public string uniqueName = "unnamed";

    public UpgradesController.CartRarity myRarity;
    
    public bool isGenericArtifact;

    public Transform uiTransform;

    public GameObject worldPart;
    public GameObject attachedToCartPart;

    public Sprite mySprite;


    public bool isAttached = false;
    private void Start() {
        ApplyNameAndSprite();
    }

    [Button]
    private void ApplyNameAndSprite() {
        foreach (var spriteRenderer in GetComponentsInChildren<SpriteRenderer>(true)) {
            spriteRenderer.sprite = mySprite;
        }
    }

    public void AttachToCart(Cart cart, bool doSave = true) {
        UpgradesController.s.RemoveArtifactFromShop(this, doSave);
        var oldCart = GetComponentInParent<Cart>();
        if (oldCart != null) {
            if(oldCart.myAttachedArtifact == this)
                oldCart.myAttachedArtifact = null;
        }
        
        cart.myAttachedArtifact = this;
        transform.SetParent(cart.artifactParent);
        //transform.ResetTransformation();

        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().useGravity = false;
        
        attachedToCartPart.SetActive(true);
        worldPart.SetActive(false);
        
        
        Train.s.ArtifactsChanged();

        isAttached = true;
    }

    public void DetachFromCart( bool doSave = true) {
        UpgradesController.s.AddArtifactToShop(this, doSave);
        var oldCart = GetComponentInParent<Cart>();
        if (oldCart != null) {
            if(oldCart.myAttachedArtifact == this)
                oldCart.myAttachedArtifact = null;
        }
        
        attachedToCartPart.SetActive(false);
        worldPart.SetActive(true);
        
        transform.SetParent(null);
        
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().useGravity = true;
        
        
        Train.s.ArtifactsChanged();
        
        isAttached = false;
    }

    /*public void InstantEquipArtifact() {
        worldPart.SetActive(false);
        uiPart.SetActive(true);
        gameObject.AddComponent<RectTransform>();
        uiPart.transform.ResetTransformation();
    }

    public void EquipArtifact() {
        worldPart.SetActive(false);
        uiPart.SetActive(true);
        Vector3 ViewportPosition = MainCameraReference.s.cam.WorldToViewportPoint(worldPart.transform.position);

        var CanvasRect = LevelReferences.s.uiDisplayParent.root.GetComponent<RectTransform>();

        Vector2 WorldObject_ScreenPosition = new Vector2(
            ((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
            ((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)));

        
        gameObject.AddComponent<RectTransform>();
        var UIRect = uiPart.GetComponent<RectTransform>();
        UIRect.SetParent(LevelReferences.s.uiDisplayParent);
        UIRect.ResetTransformation();
        UIRect.anchoredPosition = WorldObject_ScreenPosition;
        var realStartPos = UIRect.position;
        //print(realStartPos);
        UIRect.SetParent(transform);
        transform.SetParent(ArtifactsController.s.artifactsParent);
        transform.ResetTransformation();
        UIRect.ResetTransformation();
        LayoutRebuilder.ForceRebuildLayoutImmediate(ArtifactsController.s.artifactsParent as RectTransform);
        
        
        UIRect.position = realStartPos;
        
        StartCoroutine(AnimateMoveToPos());
        
        ArtifactsController.s.EquipArtifact(this);
    }

    IEnumerator AnimateMoveToPos() {
        var UIRect = uiPart.GetComponent<RectTransform>();
        
        Vector3 velocity = Vector3.zero;
        

        while (UIRect.localPosition.sqrMagnitude > 0.01f) {
            UIRect.localPosition = Vector3.SmoothDamp(UIRect.localPosition, Vector3.zero, ref velocity, 0.2f);
            yield return null;
        }
        
        UIRect.localPosition = Vector2.zero;
    }
    public void OnUIClick() { 
        CharacterSelector.s.SelectStartingArtifact(this);   
    }*/
}
