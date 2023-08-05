using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using Sirenix.OdinInspector;
using UnityEngine;

public class SplineSegment : MonoBehaviour {

    public SplineSegment pathA;
    public SplineSegment pathB;
    

    public SplineComputer splineComputer => GetComponent<SplineComputer>();

    private const float yVal = 0.353f;
    [Button]
    public void MakeSegment(float length, float flatSection = 0) {
        var splinePoints = new SplinePoint[2];
        
        var point = new SplinePoint(new Vector3(0, yVal, 0));
        point.tangent = new Vector3(0, yVal, -5);
        point.tangent2 = new Vector3(0, yVal, 5);
        splinePoints[0] = point;
        
        
        point = new SplinePoint(new Vector3(0, yVal, length));
        point.tangent = new Vector3(0, yVal, length-5);
        point.tangent2 = new Vector3(0, yVal, length+5);
        splinePoints[1] = point;
        
        splineComputer.SetPoints(splinePoints);
        splineComputer.sampleRate = Mathf.CeilToInt(length);
    }
    
    
    [Button]
    public void MakeSwitchSegment(float length, bool rotateToTheRight) {
        var splinePoints = new SplinePoint[3];
        
        
        var point = new SplinePoint(new Vector3(0, yVal, 0));
        point.tangent = new Vector3(0, yVal, -5);
        point.tangent2 = new Vector3(0, yVal, 5);
        splinePoints[0] = point;


        var angle = 30;
        var bigAngle = angle + 15;
        if (rotateToTheRight) {
            angle = -angle;
            bigAngle = -bigAngle;
        }
        var radius = 14;

        var pointX = Mathf.Sin(Mathf.Deg2Rad*angle)*radius;
        var pointY = Mathf.Cos(Mathf.Deg2Rad*angle)*radius;

        Quaternion rotation = Quaternion.AngleAxis( bigAngle, Vector3.up);
        var forwardVector =rotation* Vector3.forward ;

        var startPoint = new Vector3(pointX, yVal, pointY);
        point = new SplinePoint(startPoint);
        point.tangent = startPoint - (forwardVector*5);
        point.tangent2 = startPoint + (forwardVector*5);
        splinePoints[1] = point;


        var endPoint = startPoint + forwardVector * length;
        point = new SplinePoint(endPoint);
        point.tangent = endPoint  - (forwardVector*5);
        point.tangent2 = endPoint + (forwardVector*5);
        splinePoints[2] = point;
        
        splineComputer.SetPoints(splinePoints);
        print((Mathf.PI * (Mathf.Deg2Rad * angle)));
        splineComputer.sampleRate = Mathf.CeilToInt((length + (Mathf.PI * (Mathf.Deg2Rad * angle)*8) )/2);
    }


    [Button]
    public void AttachToSegment(SplineSegment segment) { // attach to the very end
        var endPoint = segment.splineComputer.Evaluate(segment.splineComputer.pointCount-1);
        //print(endPoint.position);
        transform.position = endPoint.position;
        transform.rotation = Quaternion.LookRotation(endPoint.forward);
    }
}
