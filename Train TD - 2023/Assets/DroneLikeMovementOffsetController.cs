using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneLikeMovementOffsetController : MonoBehaviour
{
    public Transform[] wheels;

    public Vector2 randomBumpTimer = new Vector2(0.05f, 0.7f);
    public float curTime = 1f;
    public Vector2 randomBumpForce = new Vector2(200, 600);

    public float positionDelta = 0.2f;
    public float lerpSpeed = 3f;
    public float lerpForce = 200;
    public float targetMoveSpeed = 0.1f;
    public Vector3 target;
    public Vector3 targetTargetPosition;

    float lookRotationDelta = 20;
    
    public Vector3 centerOfMass  = new Vector3(0, -0.1f, 0);

    private Rigidbody rg;

    public float yOffset = 1;
    public float randomYOffset;

    public float velocityToRotation = 10;

     PossibleTarget velProvider;
    private void Start() {
        rg = GetComponent<Rigidbody>();
        rg.centerOfMass = centerOfMass;
        transform.position += Vector3.up*1;
        rg.velocity = Vector3.zero;
        velProvider = GetComponentInParent<PossibleTarget>();
    }

    private void FixedUpdate() {
        UpdatePosition();
        //ApplyForces();
    }

    private float posChangeTimer;
    private Vector3 force;
    private void UpdatePosition() {
        target.y = yOffset + randomYOffset;
        
        //var targetReal = transform.parent.TransformPoint(target);
        //var targetSpeed = Vector3.Lerp(transform.localPosition, target, lerpSpeed * Time.deltaTime) - transform.localPosition;
        var targetSpeed =  target- transform.localPosition;
        targetSpeed *= lerpSpeed;
        rg.velocity = Vector3.MoveTowards(rg.velocity, targetSpeed, lerpForce * Time.deltaTime);
        //rg.AddForce(targetSpeed * Time.deltaTime*lerpForce);

        targetTargetPosition.y = target.y;
        target = Vector3.MoveTowards(target, targetTargetPosition, targetMoveSpeed * Time.fixedDeltaTime * GetRealMoveSpeed());

        if (Vector3.Distance(target, targetTargetPosition) < 0.01f && posChangeTimer <=0) {
            var randomInCircle = Random.insideUnitCircle;
            randomInCircle *= positionDelta;
            targetTargetPosition = new Vector3(randomInCircle.x, 0, randomInCircle.y);
            posChangeTimer = 1f;
            randomYOffset = Random.Range(-0.1f, 0.15f);
        }

        posChangeTimer -= Time.fixedDeltaTime;

        /*if (transform.position.y < 0.19f) {
            transform.position += Vector3.up*1;
            rg.velocity = Vector3.zero;
        }*/

        var velocity = velProvider.velocity + Vector3.forward * (GetRealMoveSpeed());
        //Debug.Log($"Velocity {velProvider.velocity} - speed {Vector3.forward * (LevelReferences.s.speed)}");
        
        //Debug.DrawLine(transform.position, transform.position+velocity);

        var targetRotation = Quaternion.Euler(Mathf.Clamp(velocity.z*velocityToRotation,-60,60),0,Mathf.Clamp(-velocity.x*velocityToRotation*2,-60,60));
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, lookRotationDelta * Time.fixedDeltaTime);
    }

    private void ApplyForces() {
        curTime -= Time.fixedDeltaTime;
        if (curTime <= 0) {
            var randomWheel = wheels[Random.Range(0, wheels.Length)];
            var force = Random.Range(randomBumpForce.x, randomBumpForce.y) * GetRealMoveSpeed();
            rg.AddForceAtPosition(Vector3.up * force, randomWheel.transform.position);

            curTime = Random.Range(randomBumpTimer.x, randomBumpTimer.y);
        }
    }

    float GetRealMoveSpeed() {
        return Mathf.Clamp(LevelReferences.s.speed/3f, -2, 2);
    }
}
