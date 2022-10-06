using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeCamera : MonoBehaviour
{
    public bool shakeAtStart = true;

    public float shakeMagnitude = 0.4f;
    public float shakeTime = 0.15f;
    /*public float shakeRange = 20f;
    public float shakeTrailOffDistance = 10f;*/

    // Start is called before the first frame update
    void Start() {
        if (shakeAtStart) {
            Shake();
        }
    }

    public void Shake() {
        
        /*var distance = Vector3.Distance(PlayerReference.s.playerTrackingBase.position, transform.position);

        if (distance > shakeTrailOffDistance) {
            var trailDistance = (shakeRange - shakeTrailOffDistance);
            var magnitude = shakeMagnitude * ((trailDistance - (distance - shakeTrailOffDistance)) / trailDistance);
            if (magnitude > 0f) {
                PlayerReference.s.shaker.ShakeCamera(shakeTime,magnitude);
            }
        } else {
            PlayerReference.s.shaker.ShakeCamera(shakeTime,shakeMagnitude);
        }*/

        CameraShakeController.s.ShakeCamera(shakeTime, shakeMagnitude);
    }
}
