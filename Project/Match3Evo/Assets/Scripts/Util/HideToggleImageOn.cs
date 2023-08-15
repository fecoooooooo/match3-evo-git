using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HideToggleImageOn : MonoBehaviour {
    [SerializeField] Image toHide;
    float alfaOff = 1.0f;

    public void IsOn(bool isOn) {
        Color color = toHide.color;
        if (isOn) {
            alfaOff = color.a;
            color.a = 0f;
        } else {
            color.a = alfaOff;
        }
        toHide.color = color;
    }
}
