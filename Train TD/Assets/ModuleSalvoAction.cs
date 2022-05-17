using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleSalvoAction : ModuleAction {
    
    [Space] 
    public float actionTime = 5f;
    public float initialDelay = 2f;
    
    public float rangeBoost = 2f;
    public float fireSpeedBoost = 10f;
    public float angleBoost = 30;

    protected override void _EngageAction() {
        GetComponent<TargetPicker>().range += rangeBoost;
        GetComponent<TargetPicker>().rotationSpan += angleBoost;
        
        var gun = GetComponent<GunModule>();

        gun.fireDelay /= fireSpeedBoost;
        gun.StopShooting();

        Invoke(nameof(StartShooting), initialDelay);
		
        Invoke(nameof(StopAction), initialDelay+actionTime);
    }

    void StartShooting() {
        var gun = GetComponent<GunModule>();
        gun.StartShooting();
    }

    void StopAction() {
        GetComponent<TargetPicker>().range -= rangeBoost;
        GetComponent<TargetPicker>().rotationSpan -= angleBoost;
        GetComponent<GunModule>().fireDelay *= fireSpeedBoost;
    }
}
