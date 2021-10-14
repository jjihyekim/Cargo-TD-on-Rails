using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Module Unlock", order= -1)]
public class ModuleUnlockUpgrade : Upgrade {
    public TrainBuilding module;
    
    
    public override void _ApplyUniqueName() {
        upgradeUniqueName = module.uniqueName;
    }
}
