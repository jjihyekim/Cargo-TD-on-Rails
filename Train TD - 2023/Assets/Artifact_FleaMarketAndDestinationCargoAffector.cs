using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artifact_FleaMarketAndDestinationCargoAffector : ActivateWhenOnArtifactRow {


    public int fleaMarketCountModifier = 0;
    public bool rewardDestinationArtifact = true;
    
    protected override void _Arm() {
        UpgradesController.s.fleaMarketLocationCount += fleaMarketCountModifier;

        if (!rewardDestinationArtifact) {
            UpgradesController.s.rewardDestinationArtifact = rewardDestinationArtifact;
        }
    }

    protected override void _Disarm() {
        // do nothing
    }
}
