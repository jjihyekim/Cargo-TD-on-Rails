using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineModule : MonoBehaviour, IActiveDuringCombat, IActiveDuringShopping {
   public int enginePower = 100;
   public bool isNuclear = false;

   public bool hasFuel = true;
   private void OnEnable() {
      SpeedController.s.AddEngine(this);
   }

   private void OnDisable() {
      if (SpeedController.s != null) {
         SpeedController.s.RemoveEngine(this);
      }
   }

   private int lastSelfDamageAmount = -1;
   public void SetSelfDamageState(int amount) {
      if (amount != lastSelfDamageAmount) {
         lastSelfDamageAmount = amount;
         switch (amount) {
            case 0:
               GetComponent<ModuleHealth>().selfDamage = false;
               break;
            case 1:
               GetComponent<ModuleHealth>().selfDamage = true;
               GetComponent<ModuleHealth>().selfDamageMultiplier = 1;
               break;
            case 2:
               GetComponent<ModuleHealth>().selfDamage = true;
               GetComponent<ModuleHealth>().selfDamageMultiplier = 2;
               break;
         }
      }
   }

   public float baseFuelUsePerSecond = 1;

   public void UseFuel(float fuelUsePercent) {
      if (!isNuclear) {
         GetComponent<ModuleAmmo>().UseFuel(fuelUsePercent * baseFuelUsePerSecond * Time.deltaTime);
      }
   }

   public void ActivateForCombat() {
      this.enabled = true;
   }

   public void ActivateForShopping() {
      this.enabled = true;
   }

   public void Disable() {
      this.enabled = false;
   }
}
