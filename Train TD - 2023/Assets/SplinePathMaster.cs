using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplinePathMaster : MonoBehaviour {
    public static SplinePathMaster s;

    private void Awake() {
        s = this;
    }

    public GameObject splinePrefab;

    public SplineSegment currentSegment;

    public List<SplineSegment> segmentsToBeDeleted;
    
    private ConstructedLevel activeLevel => PlayStateMaster.s.currentLevel;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void GenerateInitialSegments() {
        for (int i = 0; i < segmentsToBeDeleted.Count; i++) {
            Destroy(segmentsToBeDeleted[i].gameObject);
        }
        segmentsToBeDeleted.Clear();
        
        
        var firstSegmentLength = activeLevel.mySegmentsA[0].segmentLength;
        currentSegment.MakeSegment(firstSegmentLength + 20, 30);
        currentSegment.transform.position = Vector3.back*20;
        GenerateForkSegments(currentSegment, activeLevel.mySegmentsA[1].segmentLength, activeLevel.mySegmentsB[1].segmentLength);
    }


    public void GenerateForkSegments(SplineSegment segment, float lengthA, float lengthB) {
        var segmentA = Instantiate(splinePrefab).GetComponent<SplineSegment>();
        segmentA.MakeSegment(lengthA);
        segmentA.AttachToSegment(segment);
        
        var segmentB = Instantiate(splinePrefab).GetComponent<SplineSegment>();
        segmentB.MakeSegment(lengthB);
        segmentB.AttachToSegment(segment);

        segment.pathA = segmentA;
        segment.pathB = segmentB;
    }

    public void MoveThroughCurrentFork(bool isPathA, float lengthA, float lengthB) {
        for (int i = 0; i < segmentsToBeDeleted.Count; i++) {
            Destroy(segmentsToBeDeleted[i].gameObject);
        }
        segmentsToBeDeleted.Clear();
        
        segmentsToBeDeleted.Add(currentSegment);
        
        if (isPathA) {
            segmentsToBeDeleted.Add(currentSegment.pathB);
            currentSegment = currentSegment.pathA;
        } else {
            segmentsToBeDeleted.Add(currentSegment.pathA);
            currentSegment = currentSegment.pathB;
        }
        
        
        GenerateForkSegments(currentSegment, lengthA, lengthB);
    }


}
