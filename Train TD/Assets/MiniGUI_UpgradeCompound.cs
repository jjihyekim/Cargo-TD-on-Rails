using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGUI_UpgradeCompound : MonoBehaviour {
    public ModuleUnlockUpgrade unlockUpgrade;
    public Upgrade skillUpgrade1;
    public Upgrade skillUpgrade2;
    
    [Space]
    public MiniGUI_UpgradeButton unlockUpgradeButton;
    public MiniGUI_UpgradeButton skillUpgrade1Button;
    public MiniGUI_UpgradeButton skillUpgrade2Button;


    public void Initialize() {
        unlockUpgradeButton.myUpgrade = unlockUpgrade;
        unlockUpgradeButton.Initialize();
        
        if (skillUpgrade1 != null) {
            skillUpgrade1Button.myUpgrade = skillUpgrade1;
            skillUpgrade1Button.Initialize();
        } else {
            if (skillUpgrade1Button != null) {
                Destroy(skillUpgrade1Button.gameObject);
                skillUpgrade1Button = null;
            }
        }
        
        if (skillUpgrade2 != null) {
            skillUpgrade2Button.myUpgrade = skillUpgrade2;
            skillUpgrade2Button.Initialize();
        } else {
            if (skillUpgrade2Button != null) {
                Destroy(skillUpgrade2Button.gameObject);
                skillUpgrade2Button = null;
            }
        }
    }
}
