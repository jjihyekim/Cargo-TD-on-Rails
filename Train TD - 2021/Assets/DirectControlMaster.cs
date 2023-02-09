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
	public ModuleAmmo myAmmo;
	public GameObject exitToReload;
	private TargetPicker myTargetPicker;
	public float curCooldown;


	private AudioSource hitAud;
	public Image hitImage;
	private Color hitImageColor;
	public Slider cooldownSlider;
	public Slider ammoSlider;
	public GameObject DirectControlGameObject;

	public bool directControlInProgress = false;

	public float directControlDamageBoost = 1.2f;
	public float directControlFireRateBoost = 1.2f;
	public float directControlAmmoConservationBoost = 1.2f;
	
	private bool doShake = true;
	public bool hasAmmo = true;
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
			myAmmo = myGun.GetComponent<ModuleAmmo>();
			ammoSlider.gameObject.SetActive(myAmmo != null);
			if (myAmmo != null) {
				hasAmmo = myAmmo.curAmmo > 0;
			} else {
				exitToReload.SetActive(false);
				ammoSlider.value = 0;
				hasAmmo = true;
			}
			
			myTargetPicker = directControlAction.GetComponent<TargetPicker>();

			myGun.DeactivateGun();
			myTargetPicker.enabled = false;
			doShake = myGun.gunShakeOnShoot;
			myGun.beingDirectControlled = true;

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
				myGun.beingDirectControlled = false;
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

			if (hasAmmo && directControlShootAction.action.IsPressed()) {
				if (curCooldown <= 0) {
					myGun.ShootBarrage(false,OnShoot, OnHit);
					curCooldown = myGun.GetFireDelay();
				}
			}

			curCooldown -= Time.deltaTime;
			cooldownSlider.value = Mathf.Clamp01(curCooldown / myGun.GetFireDelay());

			if (myAmmo != null) {
				ammoSlider.value = myAmmo.curAmmo / myAmmo.maxAmmo;
				hasAmmo = myAmmo.curAmmo > 0;
				exitToReload.SetActive(!hasAmmo);
			}

			hitImageColor.a = onHitAlpha;
			hitImage.color = hitImageColor;
			onHitAlpha -= onHitAlphaDecay * Time.deltaTime;
			onHitAlpha = Mathf.Clamp01(onHitAlpha);
		}

		curOnHitDecayTime -= Time.deltaTime;
		if (curOnHitDecayTime <= 0) {
			if (currentOnHit > 0)
				currentOnHit -= 1;

			curOnHitDecayTime = onHitSoundDecayTime;
		}
	}

	void OnShoot() {
		//if (doShake) {
		var range = Mathf.Clamp01(myGun.projectileDamage / 10f) + Mathf.Clamp01(myGun.projectileDamage / 10f);
		range /= 2f;
		
		//print(range);
		if (doShake) {
			CameraShakeController.s.ShakeCamera(
				Mathf.Lerp(0.1f, 0.7f, range),
				Mathf.Lerp(0.005f, 0.045f, range),
				Mathf.Lerp(2, 10, range),
				Mathf.Lerp(0.1f, 0.5f, range),
				true
			);
			
			CameraController.s.ProcessDirectControl(new Vector2(Random.Range(-range*2, range*2), range*5));
		} else {
			/*CameraShakeController.s.ShakeCamera(
				Mathf.Lerp(0.1f, 0.7f, range),
				Mathf.Lerp(0.001f, 0.05f, range),
				Mathf.Lerp(1, 2, range),
				Mathf.Lerp(0.1f, 0.5f, range),
				true
			);*/
		}
		//}
	}

	float hitPitch = 0.8f;
	float hitPitchRandomness = 0.1f;
	float onHitSoundDecayTime = 0.1f;
	private float curOnHitDecayTime = 0;
	int maxOnHitSimultaneously = 10;
	private int currentOnHit = 0;

	void OnHit() {
		onHitAlpha = 1;
		if (currentOnHit < maxOnHitSimultaneously) {
			hitAud.pitch = hitPitch + Random.Range(-hitPitchRandomness, hitPitchRandomness);
			hitAud.PlayOneShot(hitAud.clip);
			currentOnHit += 1;
		}
	}
}
