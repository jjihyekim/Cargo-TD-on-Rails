using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPicker : MonoBehaviour {
    private IComponentWithTarget targeter;

    public float rotationSpan = 60f;
    public float range = 5f;

    public DirectionalRotationSpanAndRangeOverride myOverride;


    [Serializable]
    public class DirectionalRotationSpanAndRangeOverride {
        public bool isOverride = false;
        public List<TrainBuilding.Rots> rotations;
        public float rotationSpanOverride;
        //public float range;
    }

    private Transform origin;

    public List<PossibleTarget.Type> myPossibleTargets;
    
    private void Start() {
        targeter = GetComponent<IComponentWithTarget>();
        origin = targeter.GetRangeOrigin();


        if (myOverride != null && myOverride.isOverride) {
            var module = GetComponent<TrainBuilding>();
            if (myOverride.rotations.Contains(module.myRotation)) {
                rotationSpan = myOverride.rotationSpanOverride;
                //range = myOverride.range;
            }
        }
    }

    private void Update() {
        var closestTargetDistance = range + 1;
        Transform closestTarget = null;
        var allTargets = LevelReferences.allTargets;
        for (int i = 0; i < allTargets.Count; i++) {
            var canTarget = (allTargets[i] != GetComponentInParent<PossibleTarget>()) && (myPossibleTargets.Contains(allTargets[i].myType));
            if (canTarget) {
                if (IsPointInsideCone(allTargets[i].targetTransform.position, origin.position, origin.forward, rotationSpan, range, out float distance)) {
                    if (distance < closestTargetDistance) {
                        closestTarget = allTargets[i].targetTransform;
                        closestTargetDistance = distance;
                    }
                }
            }
        }

        if (closestTarget != null) {
            targeter.SetTarget(closestTarget);
        } else {
            targeter.UnsetTarget();
        }
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
}

public interface IComponentWithTarget {
    public void SetTarget(Transform target);
    public void UnsetTarget();

    public Transform GetRangeOrigin();
    public Transform GetActiveTarget();
} 
