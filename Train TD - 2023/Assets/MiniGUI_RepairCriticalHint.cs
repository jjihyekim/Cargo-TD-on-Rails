using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGUI_RepairCriticalHint : MiniGUI_TutorialHint
{
    protected override void _Update() {
        var show = target.GetHealthModule().GetHealthPercent() < 0.5f;
        SetStatus(show);
        if(show)
            FirstTimeTutorialController.s.RepairCriticalHintShown();
    }
}