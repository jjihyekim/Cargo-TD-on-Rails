using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableCanvasGroupWhenActive : MonoBehaviour {
   public CanvasGroup target;

   private void OnEnable() {
      target.interactable = false;
   }

   private void OnDisable() {
      if(target != null)
         target.interactable = true;
   }
}
