using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artifact_HomingBullets : ActivateWhenOnArtifactRow {

    [Header("not functioning anymore")] public bool yeet;
    
    //public static bool isHomingBullets = false;

    protected override void _Arm() {
        //isHomingBullets = true;
    }

    protected override void _Disarm() {
        //isHomingBullets = false;
    }
    
    
    private void OnDestroy() {
        _Disarm();
    }
}
