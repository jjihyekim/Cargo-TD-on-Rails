using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGUI_ReloadHint : MiniGUI_TutorialHint {

    public ModuleAmmo myAmmo;

    protected override void _SetUp() {
        myAmmo = target.GetComponentInChildren<ModuleAmmo>();
    }

    protected override void _Update() {
        var show = myAmmo.curAmmo <= 0;
        SetStatus(show);
        if(show)
            FirstTimeTutorialController.s.ReloadHintShown();
    }
}
