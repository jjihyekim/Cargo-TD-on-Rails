using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGUI_RepairHint :MiniGUI_TutorialHint
{
    protected override void _Update() {
        var show = target.GetHealthModule().GetHealthPercent() < 0.25f;
        SetStatus(show);
        if(show)
            FirstTimeTutorialController.s.RepairHintShown();
    }
}

