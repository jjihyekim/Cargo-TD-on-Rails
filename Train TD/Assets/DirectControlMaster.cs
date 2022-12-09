using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DirectControlMaster : MonoBehaviour {
	public static DirectControlMaster s;

	private void Awake() {
		s = this;
	}

	private void Start() {
		DirectControlGameObject.SetActive(false);
		directControlInProgress = false;
		hitImageColor = hitImage.color;
		hitAud = GetComponent<AudioSource>();
	}

	public InputActionProperty cancelDirectControlAction;
	public InputActionProperty directControlShootAction;

	[Space]
	public ModuleHealth directControlTrainBuilding;
	public DirectControlAction directControlAction;
	private GunModule myGun;
	private TargetPicker myTargetPicker;
	public float cooldown;
	public float curCooldown;


	private AudioSource hitAud;
	public Image hitImage;
	private Color hitImageColor;
	public Slider cooldownSlider;
	public GameObject DirectControlGameObject;

	public bool directControlInProgress = false;

	private void OnEnable() {
		cancelDirectControlAction.action.Enable();
		directControlShootAction.action.Enable();
		cancelDirectControlAction.action.performed += DisableDirectControl;
	}

	private void OnDisable() {
		cancelDirectControlAction.action.Disable();
		directControlShootAction.action.Disable();
		cancelDirectControlAction.action.performed -= DisableDirectControl;
	}
	
	public void AssumeDirectControl(DirectControlAction source) {
		if (!directControlInProgress) {
			CameraController.s.ActivateDirectControl(source.GetDirectControlTransform());
			directControlAction = source;

			directControlTrainBuilding = directControlAction.GetComponent<ModuleHealth>();
			myGun = directControlAction.GetComponent<GunModule>();
			myTargetPicker = directControlAction.GetComponent<TargetPicker>();

			myGun.DeactivateGun();
			cooldown = myGun.fireDelay;
			myTargetPicker.enabled = false;

			curCooldown = 0;

			DirectControlGameObject.SetActive(true);
			directControlInProgress = true;

			PlayerModuleSelector.s.DeselectObject();
			PlayerModuleSelector.s.DisableModuleSelecting();

			onHitAlpha = 0;

			CameraShakeController.s.rotationalShake = true;
		}
	}

	private void DisableDirectControl(InputAction.CallbackContext obj) {
		if (directControlInProgress) {
			CameraController.s.DisableDirectControl();

			if (myGun != null) {
				myGun.ActivateGun();
				myTargetPicker.enabled = true;
			}

			DirectControlGameObject.SetActive(false);
			directControlInProgress = false;

			PlayerModuleSelector.s.EnableModuleSelecting();
			
			CameraShakeController.s.rotationalShake = false;
		}
	}

	public void DisableDirectControl() {
		DisableDirectControl(new InputAction.CallbackContext());
	}

	public LayerMask lookMask;

	private float onHitAlpha = 0f;
	public float onHitAlphaDecay = 2f;

	private void Update() {
		if (directControlInProgress) {
			if (directControlTrainBuilding == null || directControlTrainBuilding.isDead || myGun == null) {
				// in case our module gets destroyed
				DisableDirectControl(new InputAction.CallbackContext());
				return;
			}

			var camTrans = MainCameraReference.s.cam.transform;
			Ray ray = new Ray(camTrans.position, camTrans.forward);

			if (Physics.Raycast(ray, out RaycastHit hit, 1000, lookMask)) {
				myGun.LookAtLocation(hit.point);
				//Debug.DrawLine(ray.origin, hit.point);
			} else {
				myGun.LookAtLocation(ray.GetPoint(10));
				//Debug.DrawLine(ray.origin, ray.GetPoint(10));
			}

			if (directControlShootAction.action.IsPressed()) {
				if (curCooldown <= 0) {
					myGun.ShootBarrageFree(OnShoot, OnHit);
					curCooldown = cooldown;
				}
			}

			curCooldown -= Time.deltaTime;
			cooldownSlider.value = Mathf.Clamp01(curCooldown / cooldown);

			hitImageColor.a = onHitAlpha;
			hitImage.color = hitImageColor;
			onHitAlpha -= onHitAlphaDecay * Time.deltaTime;
			onHitAlpha = Mathf.Clamp01(onHitAlpha);
		}
	}

	void OnShoot() {
		var range = Mathf.Clamp01(myGun.projectileDamage / 10f);
		//print(range);
		CameraShakeController.s.ShakeCamera(
			Mathf.Lerp(0.1f,0.7f,range ),
			Mathf.Lerp(0.005f,0.045f,range),
			Mathf.Lerp(2,10,range),
			Mathf.Lerp(0.1f,0.5f,range),
			true
			);
	}

	public float hitPitch = 0.8f;
	public float hitPitchRandomness = 0.1f;
	void OnHit() {
		onHitAlpha = 1;
		hitAud.pitch = hitPitch + Random.Range(-hitPitchRandomness, hitPitchRandomness);
		hitAud.PlayOneShot(hitAud.clip);
	}
}
