using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPicker : MonoBehaviour, IActiveDuringCombat {
    private IComponentWithTarget targeter;

    public float rotationSpan = 60f;
    public float range = 5f;

    private Transform origin;

    public List<PossibleTarget.Type> myPossibleTargets;

    private PossibleTarget mySelfTarget;

    public bool canHitFlying = false;
    
    private void Start() {
        targeter = GetComponent<IComponentWithTarget>();
        origin = targeter.GetRangeOrigin();
        mySelfTarget = GetComponentInParent<PossibleTarget>();
    }

    private void Update() {
        var closestTargetNotAvoided = ClosestTarget(true);

        if (closestTargetNotAvoided!=null)  {
            targeter.SetTarget(closestTargetNotAvoided);
        } else {
            var closestTarget = ClosestTarget(false);

            if (closestTarget != null) {
                targeter.SetTarget(closestTarget);
            } else {
                targeter.UnsetTarget();
            }
        }
    }

    private Transform ClosestTarget(bool doAvoidCheck) {
        if (LevelReferences.targetsDirty) {
            return null;
        }
        
        var closestTargetDistance = range + 1;
        Transform closestTarget = null;
        var allTargets = LevelReferences.allTargetValues;
        var allTargetsReal = LevelReferences.allTargets;
        var myId = -1;
        if (mySelfTarget != null) {
            myId = mySelfTarget.myId;
        }

        //var myDamage = targeter.GetDamage();
        var myPosition = origin.position;
        var myForward = origin.forward;

        for (int i = 0; i < allTargets.Length; i++) {
            if (i != myId) {
                var target = allTargets[i];
                var canTarget = myPossibleTargets.Contains(target.type);
                //var targetHasEnoughHealth = !doHealthCheck || (allTargets[i].health >= myDamage);
                var targetNotAvoided = !target.avoid || !doAvoidCheck;
                var canHitBecauseFlying = canHitFlying || !target.flying;

                if (canTarget && targetNotAvoided && canHitBecauseFlying) {
                    if (IsPointInsideCone(allTargets[i].position, myPosition, myForward, rotationSpan, range, out float distance)) {
                        if (distance < closestTargetDistance) {
                            closestTarget = allTargetsReal[i].targetTransform;
                            closestTargetDistance = distance;
                        }
                    }
                }
            }
        }

        return closestTarget;
    }


    private void OnDrawGizmosSelected() {
        targeter = GetComponent<IComponentWithTarget>();
        if (targeter != null) {
            origin = targeter.GetRangeOrigin();
            if (origin != null) {
                var radians = Mathf.Deg2Rad * rotationSpan;
                var leftEdge = new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians));
                var rightEdge = new Vector3(Mathf.Sin(-radians), 0, Mathf.Cos(-radians));

                var scaleAdjustment = (1f / origin.lossyScale.x);
                leftEdge = origin.TransformPoint(leftEdge * range* scaleAdjustment) ;
                rightEdge = origin.TransformPoint(rightEdge * range* scaleAdjustment) ;
                Gizmos.DrawLine(origin.position, leftEdge);
                Gizmos.DrawLine(origin.position, rightEdge);

                var resolution = radians / 0.1f;
                var resolutionStep = (radians * 2 / resolution);
                for (int i = 0; i < resolution; i++) {
                    var start = -radians + i * resolutionStep;
                    var stop = -radians + (i + 1) * resolutionStep;

                    var leftEdgeNew = new Vector3(Mathf.Sin(start), 0, Mathf.Cos(start));
                    var rightEdgeNew = new Vector3(Mathf.Sin(stop), 0, Mathf.Cos(stop));
                    leftEdgeNew = origin.TransformPoint(leftEdgeNew * range* scaleAdjustment);
                    rightEdgeNew = origin.TransformPoint(rightEdgeNew * range* scaleAdjustment);
                    Gizmos.DrawLine(leftEdgeNew, rightEdgeNew);
                }
            }
        }
    }


    public static bool IsPointInsideCone ( Vector3 point, Vector3 coneOrigin, Vector3 coneDirection, float maxAngle, float maxDistance, out float distance)
    {
        distance = ( point - coneOrigin ).magnitude;
        if ( distance < maxDistance )
        {
            var pointDirection = point - coneOrigin;
            var angle = Vector3.Angle ( coneDirection, pointDirection );
            if ( angle < maxAngle )
                return true;
        }
        return false;
    }
    
    public void ActivateForCombat() {
        this.enabled = true;
    }

    public void Disable() {
        this.enabled = false;
    }
}

public interface IComponentWithTarget {
    public void SetTarget(Transform target);
    public void UnsetTarget();

    public Transform GetRangeOrigin();
    public Transform GetActiveTarget();

    public float GetDamage();
} 
