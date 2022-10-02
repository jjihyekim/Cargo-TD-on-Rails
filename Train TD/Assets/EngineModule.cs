using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineModule : MonoBehaviour, IActiveDuringCombat {
   public int enginePower = 100;
   private void OnEnable() {
      SpeedController.s.AddEngine(this);
   }

   private void OnDisable() {
      if (SpeedController.s != null) {
         SpeedController.s.RemoveEngine(this);
      }
   }
   
   public void ActivateForCombat() {
      this.enabled = true;
   }

   public void Disable() {
      this.enabled = false;
   }
}
