using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artifact_PlayerActionAffector : ActivateWhenOnArtifactRow {
    public float repairAmountMultiplier = 1f;
    public float reloadAmountMultiplier = 1f;

    public bool makeCannotRepair;
    public bool makeCannotReload;
    
    protected override void _Arm() {
        PlayerWorldInteractionController.s.repairAmountPerClick *= repairAmountMultiplier;
        PlayerWorldInteractionController.s.reloadAmountPerClick *= reloadAmountMultiplier;

        if (makeCannotRepair)
            PlayerWorldInteractionController.s.canRepair = false;
        
        if (makeCannotReload)
            PlayerWorldInteractionController.s.canReload = false;
    }

    protected override void _Disarm() {
        PlayerWorldInteractionController.s.repairAmountPerClick /= repairAmountMultiplier;
        PlayerWorldInteractionController.s.reloadAmountPerClick /= reloadAmountMultiplier;
        
        
        if (makeCannotRepair)
            PlayerWorldInteractionController.s.canRepair = true;
        
        if (makeCannotReload)
            PlayerWorldInteractionController.s.canReload = true;
    }
}
