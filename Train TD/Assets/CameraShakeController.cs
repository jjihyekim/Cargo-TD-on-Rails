using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraShakeController : MonoBehaviour {
    public static CameraShakeController s;

    private void Awake() {
        s = this;
    }

    private bool isShaking = false;
    public float curDuration;
    public float curMagnitude;

    public float magnitudeDecay;

    public Transform cameraShakeDummy;


    public void ShakeCamera(float duration, float magnitude) {
		
        /*var bigDur = Mathf.Max(curDuration, duration);
        var smallDur = Mathf.Min(curDuration, duration);
        curDuration = bigDur + (Mathf.Log(smallDur+1)*2);
        
        var bigMag = Mathf.Max(curMagnitude, magnitude);
        var smallMag = Mathf.Min(curMagnitude, magnitude);
        curMagnitude = bigMag + (Mathf.Log(smallMag+1)*2);*/
		
        /*curDuration += duration;
        curMagnitude += magnitude;*/
		
        curDuration = Mathf.Max(curDuration, duration);
        curMagnitude = Mathf.Max(curMagnitude, magnitude);
		
        magnitudeDecay = curMagnitude / curDuration;
        if (!isShaking) {
            StartCoroutine(Shake());
        }
    }
	
    IEnumerator Shake() {
        isShaking = true;
        //var originalLocalPosition = PlayerReference.s.playerHead.localPosition;
        while (curDuration > 0f) {
            cameraShakeDummy.localPosition = Random.insideUnitSphere * curMagnitude;

            curMagnitude = curMagnitude - magnitudeDecay*Time.deltaTime;
			
            curDuration -= Time.deltaTime;
            yield return 0;
        }
        cameraShakeDummy.localPosition = Vector3.zero;
        //PlayerReference.s.playerHead.localPosition = originalLocalPosition;
        isShaking = false;
    }
}
