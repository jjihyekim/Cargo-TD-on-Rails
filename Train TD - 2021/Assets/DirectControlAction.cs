using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectControlAction : ModuleAction, IActiveDuringCombat {
    public Transform directControlTransformSide;
    public Transform directControlTransformTop;


    public Transform GetDirectControlTransform() {
        if (GetComponent<TrainBuilding>().IsPointingSide()) {
            return directControlTransformSide;
        } else {
            return directControlTransformTop;
        }
    }
    protected override void _EngageAction() {
        DirectControlMaster.s.AssumeDirectControl(this);
    }

    public void ActivateForCombat() {
        this.enabled = true;
    }

    public void Disable() {
        this.enabled = false;
    }
}
