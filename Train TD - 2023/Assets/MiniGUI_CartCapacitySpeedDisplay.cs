using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class MiniGUI_CartCapacitySpeedDisplay : MonoBehaviour {

    public RectTransform carryCapacityDisplay;
    public RectTransform balancingCarts;
    public RectTransform excess1;
    public RectTransform excess2;
    public RectTransform excess3;

    public RectTransform delicate_carryCapacityLow;
    public RectTransform delicate_carryCapacityExcess;
    void Start()
    {
        SpeedController.s.OnSpeedChangedBasedOnCartCapacity.AddListener(UpdateCarryCapacityAndSpeedThings);
        UpdateCarryCapacityAndSpeedThings();
    }



    [Button]
    void UpdateCarryCapacityAndSpeedThings() {
        var cartCapacity = SpeedController.s.cartCapacity;
        var cartCount = Train.s.carts.Count - 1;

        var excessCarts = cartCount - cartCapacity;
        
        excess1.gameObject.SetActive(false);
        excess2.gameObject.SetActive(false);
        excess3.gameObject.SetActive(false);
        balancingCarts.gameObject.SetActive(false);
        carryCapacityDisplay.gameObject.SetActive(true);
        
        delicate_carryCapacityLow.gameObject.SetActive(false);
        delicate_carryCapacityExcess.gameObject.SetActive(false);

        if (!SpeedController.s.delicateMachinery) {
            var size = carryCapacityDisplay.sizeDelta;
            size.x = 60 * cartCapacity - 5;
            carryCapacityDisplay.sizeDelta = size;

            if (excessCarts <= 0) {
                size = balancingCarts.sizeDelta;
                size.x = 60 * (-excessCarts) - 5;
                balancingCarts.sizeDelta = size;
                balancingCarts.gameObject.SetActive(true);

            } else {
                excess1.gameObject.SetActive(true);
                if (excessCarts > 1) {
                    excess2.gameObject.SetActive(true);
                }

                if (excessCarts > 2) {
                    excess3.gameObject.SetActive(true);
                    size = excess3.sizeDelta;
                    size.x = 60 * (excessCarts - 2) - 5;
                    excess3.sizeDelta = size;
                    //balancingCarts.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 60*(excessCarts)-5);

                }
            }
        } else {
            if (excessCarts == 0) {
                var size = carryCapacityDisplay.sizeDelta;
                size.x = 60 * cartCapacity - 5;
                carryCapacityDisplay.sizeDelta = size;
                
            }else if (excessCarts < 0) {
                carryCapacityDisplay.gameObject.SetActive(false);
                delicate_carryCapacityLow.gameObject.SetActive(true);
                
                var size = delicate_carryCapacityLow.sizeDelta;
                size.x = 60 * cartCapacity - 5;
                delicate_carryCapacityLow.sizeDelta = size;
                
                size = balancingCarts.sizeDelta;
                size.x = 60 * (-excessCarts) - 5;
                balancingCarts.sizeDelta = size;
                balancingCarts.gameObject.SetActive(true);
            } else {
                var size = carryCapacityDisplay.sizeDelta;
                size.x = 60 * cartCapacity - 5;
                carryCapacityDisplay.sizeDelta = size;
                
                
                delicate_carryCapacityExcess.gameObject.SetActive(true);
                size = delicate_carryCapacityExcess.sizeDelta;
                size.x = 60 * (excessCarts) - 5;
                delicate_carryCapacityExcess.sizeDelta = size;
                
            }
        }
    }
}
