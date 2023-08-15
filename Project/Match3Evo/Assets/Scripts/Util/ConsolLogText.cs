using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ConsolLogText : MonoBehaviour {
    Text logText;

    void Awake() {
        logText = GetComponent<Text>();
    }

    void OnEnable() {
        ConsolLog.onHandleLog += OnLogChanged;
        OnLogChanged();
    }

    void OnDisable() {
        ConsolLog.onHandleLog -= OnLogChanged;
    }

    void OnLogChanged() {
        if (gameObject.activeInHierarchy) {
            logText.text = ConsolLog.Instance.GetLog();
        }
    }
}
