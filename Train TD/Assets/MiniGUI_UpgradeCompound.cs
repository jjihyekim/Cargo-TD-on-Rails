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
        skillUpgrade1Button.myUpgrade = skillUpgrade1;
        skillUpgrade1Button.Initialize();
        skillUpgrade2Button.myUpgrade = skillUpgrade2;
        skillUpgrade2Button.Initialize();
    }
}
