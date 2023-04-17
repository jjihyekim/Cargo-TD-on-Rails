using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineModule : MonoBehaviour, IActiveDuringCombat, IActiveDuringShopping {
   public int enginePower = 100;
   public bool isNuclear = false;

   public bool hasFuel = false;
   private void OnEnable() {
      SpeedController.s.AddEngine(this);
   }

   private void OnDisable() {
      if (SpeedController.s != null) {
         SpeedController.s.RemoveEngine(this);
      }
   }

   private bool lastSelfDamageAmount = false;
   public void SetSelfDamageState(bool doSelfDamage) {
      if (doSelfDamage != lastSelfDamageAmount) {
         lastSelfDamageAmount = doSelfDamage;
         GetComponent<ModuleHealth>().selfDamage = doSelfDamage;
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
