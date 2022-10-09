using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MiniGUI_ModuleActionSelection : MonoBehaviour {

	public GameObject singleActionPrefab;
	public GameObject singleInfoPrefab;
	public Transform singleActionParent;

	private TrainBuilding myModule;
	private EnemyHealth myEnemy;
	
	public void SetUp(TrainBuilding module) {
		singleActionParent.DeleteAllChildren();
		extraRects.Clear();

		myModule = module;

		var myActions = myModule.GetComponents<ModuleAction>();

		for (int i = 0; i < myActions.Length; i++) {
			if (myActions[i].isUnlocked && myActions[i].enabled) {
				var button = Instantiate(singleActionPrefab, singleActionParent).GetComponent<MiniGUI_ModuleSingleAction>().SetUp(myActions[i]);
				extraRects.Add(button.GetComponent<RectTransform>());
			}
		}
		
		
		var myInfo = myModule.GetComponents<IClickableInfo>();
		for (int i = 0; i < myInfo.Length; i++) {
			var info = Instantiate(singleInfoPrefab, singleActionParent).GetComponent<MiniGUI_SingleInfo>().SetUp(myInfo[i]);
			extraRects.Add(info.GetComponent<RectTransform>());
		}


		sourceTransform = myModule.uiTargetTransform;
		
		CanvasRect = transform.root.GetComponent<RectTransform>();
		UIRect = GetComponent<RectTransform>();
		mainCam = LevelReferences.s.mainCam;
		Update();
	}

	public void SetUp(EnemyHealth enemyHealth) {
		singleActionParent.DeleteAllChildren();
		extraRects.Clear();
		
		myEnemy = enemyHealth;
		var myInfo = myEnemy.GetComponents<IClickableInfo>();
		for (int i = 0; i < myInfo.Length; i++) {
			var info = Instantiate(singleInfoPrefab, singleActionParent).GetComponent<MiniGUI_SingleInfo>().SetUp(myInfo[i]);
			extraRects.Add(info.GetComponent<RectTransform>());
		}


		sourceTransform = myEnemy.uiTransform;
		
		CanvasRect = transform.root.GetComponent<RectTransform>();
		UIRect = GetComponent<RectTransform>();
		mainCam = LevelReferences.s.mainCam;
		Update();
	}
	
	
	
	public Transform sourceTransform;
	private RectTransform CanvasRect;
	private RectTransform UIRect;
	private Camera mainCam;


	private Dictionary<TrainBuilding.Rots, Vector3> rotToOffset;


	private void Update() {
		if (sourceTransform == null || ((myModule == null || !myModule.isBuilt) && (myEnemy == null))) {
			PlayerModuleSelector.s.HideModuleActionSelector();
		}

		SetPosition();
	}


	private void SetPosition() {
	    if (sourceTransform != null) {
		    //then you calculate the position of the UI element
		    //0,0 for the canvas is at the center of the screen, whereas WorldToViewPortPoint
		    //treats the lower left corner as 0,0. Because of this, you need to subtract the height / width of the canvas * 0.5 to get the correct position.
		    //SetOffset(); // for debugging
		    Vector2 ViewportPosition = mainCam.WorldToViewportPoint(sourceTransform.position);
		    Vector2 WorldObject_ScreenPosition = new Vector2(
			    ((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
			    ((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)));

		    /*//now you can set the position of the ui element
		    var halfWidthLimit = (CanvasRect.rect.width - (UIRect.rect.width + edgeGive)) / 2f;
		    var halfHeightLimit = (CanvasRect.rect.height - (UIRect.rect.height + edgeGive)) / 2f;
		    WorldObject_ScreenPosition.x = Mathf.Clamp(WorldObject_ScreenPosition.x,
		        -halfWidthLimit,
		        halfWidthLimit
		    );
		    WorldObject_ScreenPosition.y = Mathf.Clamp(WorldObject_ScreenPosition.y,
		        -halfHeightLimit + (0.1f * CanvasRect.rect.height),
		        halfHeightLimit
		    );*/

		    UIRect.anchoredPosition = WorldObject_ScreenPosition;
	    }
    }


    public RectTransform reticle;
    public List<RectTransform> extraRects = new List<RectTransform>();

    public bool IsMouseOverMenu() {
	    if (!gameObject.activeSelf)
		    return false;
	    
	    Vector2 mousePos = Mouse.current.position.ReadValue();
	    var rect = reticle;
	    var isOverRect = RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos, OverlayCamsReference.s.uiCam);

	    for (int i = 0; i < extraRects.Count; i++) {
		    if (extraRects[i] != null) {
			    var isOverButton = RectTransformUtility.RectangleContainsScreenPoint(extraRects[i], mousePos, OverlayCamsReference.s.uiCam);
			    isOverRect = isOverRect || isOverButton;
		    }
	    }

	    return isOverRect;
    }
}
