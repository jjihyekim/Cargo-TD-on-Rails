using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artifact_PlayerActionAffector : ActivateWhenOnArtifactRow {
    public float repairAmountMultiplier = 1f;
    public float reloadAmountMultiplier = 1f;

    public bool makeCannotRepair;
    public bool makeCannotReload;
    
    protected override void _Arm() {
        PlayerWorldInteractionController.s.repairAmountMultiplier += repairAmountMultiplier -1;
        PlayerWorldInteractionController.s.reloadAmountMultiplier += reloadAmountMultiplier-1;

        if (makeCannotRepair)
            PlayerWorldInteractionController.s.canRepair = false;
        
        if (makeCannotReload)
            PlayerWorldInteractionController.s.canReload = false;
    }

    protected override void _Disarm() {
        // do nothing
    }
}
