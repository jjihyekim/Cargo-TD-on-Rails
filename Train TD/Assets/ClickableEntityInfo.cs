using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableEntityInfo : MonoBehaviour, IClickableInfo {
    public Tooltip tooltip;
    
    [TextArea]
    public string info;

    public string GetInfo() {
        return info;
    }

    public Tooltip GetTooltip() {
        return tooltip;
    }
}

public interface IClickableInfo {
    public string GetInfo();
    public Tooltip GetTooltip();
}