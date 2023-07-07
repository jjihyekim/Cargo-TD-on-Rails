using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MysteriousCargoModule : MonoBehaviour
{
    public GameObject highlight;
    public void HighlightForDelivery() {
        highlight.SetActive(true);
    }
}
