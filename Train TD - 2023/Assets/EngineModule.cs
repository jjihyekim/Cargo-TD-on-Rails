using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EngineModule : MonoBehaviour, IActiveDuringCombat, IActiveDuringShopping {
   public int enginePower = 100;
   public bool isNuclear = false;

   public bool hasFuel = false;

   public UnityEvent OnEngineStart = new UnityEvent();
   public UnityEvent OnEngineStop = new UnityEvent();
   private void OnEnable() {
      SpeedController.s.AddEngine(this);
      OnEngineStart?.Invoke();
   }

   private void OnDisable() {
      if (SpeedController.s != null) {
         SpeedController.s.RemoveEngine(this);
      }
      OnEngineStop?.Invoke();
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
