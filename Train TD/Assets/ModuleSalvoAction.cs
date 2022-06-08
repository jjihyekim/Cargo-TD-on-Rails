using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleSalvoAction : ModuleAction {
    
    [Space] 
    public float actionTime = 5f;
    public float initialDelay = 2f;
    public float endDelay = 2f;
    
    public float rangeBoost = 2f;
    public float fireSpeedBoost = 10f;
    public float angleBoost = 30;

    protected override void _EngageAction() {
        GetComponent<TargetPicker>().range += rangeBoost;
        GetComponent<TargetPicker>().rotationSpan += angleBoost;

        GetComponent<GunModule>().fireDelay /= fireSpeedBoost;
        GetComponent<GunModule>().DeactivateGun();

        Invoke(nameof(StartShooting), initialDelay);
		
        Invoke(nameof(StopAction), initialDelay+actionTime);
    }

    void StartShooting() {
        GetComponent<GunModule>().ActivateGun();
    }

    void StopAction() {
        GetComponent<GunModule>().fireDelay *= fireSpeedBoost;
        GetComponent<GunModule>().DeactivateGun();
        Invoke(nameof(SetRangeBackToNormal), endDelay);
    }

    void SetRangeBackToNormal() {
        GetComponent<TargetPicker>().range -= rangeBoost;
        GetComponent<TargetPicker>().rotationSpan -= angleBoost;
        
        GetComponent<GunModule>().ActivateGun();
    }
}
