using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class MiniGUI_InfoCard_GunAndAmmo : MonoBehaviour, IBuildingInfoCard {

    public TMP_Text damageAndFireRate;
    public Toggle armorPenet;
    public Toggle usesAmmo;
    public TMP_Text ammoUse;
    

    [ReadOnly] public ModuleAmmo ammoModule;
    [ReadOnly] public bool doesUseAmmo;
    
    public void SetUp(Cart building) {
        var gunModule = building.GetComponentInChildren<GunModule>();
        
        if (gunModule == null) {
            gameObject.SetActive(false);
            return;
        } else {
            gameObject.SetActive(true);
        }

        damageAndFireRate.text = $"Damage: {gunModule.GetDamage():F1}\n" +
                                 $"Firerate: {1f / gunModule.GetFireDelay():0:0.##}/s";

        armorPenet.isOn = gunModule.canPenetrateArmor;
        
        ammoModule = building.GetComponentInChildren<ModuleAmmo>();
        doesUseAmmo = ammoModule != null;
        usesAmmo.isOn = doesUseAmmo;
        ammoUse.gameObject.SetActive(doesUseAmmo);
        
        Update();

        
    }

    private void Update() {
        if (doesUseAmmo) {
            ammoUse.text = $"Mag: {ammoModule.curAmmo}/{ammoModule.maxAmmo}";
        }
    }
}
