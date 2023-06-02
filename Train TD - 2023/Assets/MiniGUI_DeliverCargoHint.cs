using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGUI_DeliverCargoHint : MiniGUI_TutorialHint {


    protected override void _SetUp() {
        SetStatus(true);
       
    }

    protected override void _Update() {
        if (target == null || target.gameObject == null) {
            Destroy(gameObject);
        }
    }
}
