using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SellCargoAction : ModuleAction, IActiveDuringShopping
{
   protected override void _EngageAction() {
      Instantiate(DataHolder.s.sellPrefab, transform.position, transform.rotation);
      Destroy(gameObject);
   }

   public void ActivateForShopping() {
      this.enabled = true;
      var cargo = GetComponent<CargoModule>();
      this.cost = 0;
      //this.cost = -cargo.moneyCost;
   }

   public void Disable() {
      this.enabled = false;
   }
}

