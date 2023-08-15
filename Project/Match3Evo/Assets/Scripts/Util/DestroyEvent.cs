using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEvent : MonoBehaviour {

    public void Destroy() {
        Destroy(gameObject);
    }
}
