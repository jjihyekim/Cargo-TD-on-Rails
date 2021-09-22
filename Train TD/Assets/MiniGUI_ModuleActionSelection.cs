using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MiniGUI_ModuleActionSelection : MonoBehaviour {

	public GameObject singleActionPrefab;
	public Transform singleActionParent;

	private TrainBuilding myModule;
	
	public void SetUp(TrainBuilding module) {
		singleActionParent.DeleteAllChildren();
		buttons.Clear();

		myModule = module;

		var myActions = myModule.GetComponents<ModuleAction>();
		
		for (int i = 0; i < myActions.Length; i++) {
			var button = Instantiate(singleActionPrefab, singleActionParent).GetComponent<MiniGUI_ModuleSingleAction>().SetUp(myActions[i]);
			buttons.Add(button.GetComponent<RectTransform>());
		}


		sourceTransform = myModule.uiTargetTransform;
		
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

		if (sourceTransform == null || myModule == null || !myModule.isBuilt) {
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
    public List<RectTransform> buttons = new List<RectTransform>();

    public bool IsMouseOverMenu() {
	    if (!gameObject.activeSelf)
		    return false;
	    
	    Vector2 mousePos = Mouse.current.position.ReadValue();
	    var rect = reticle;
	    var isOverRect = RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos);

	    for (int i = 0; i < buttons.Count; i++) {
		    if (buttons[i] != null) {
			    var isOverButton = RectTransformUtility.RectangleContainsScreenPoint(buttons[i], mousePos);
			    isOverRect = isOverRect || isOverButton;
		    }
	    }

	    return isOverRect;
    }
}
