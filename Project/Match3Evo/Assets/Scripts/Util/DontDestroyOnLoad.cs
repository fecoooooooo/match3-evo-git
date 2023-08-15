using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour {
    static DontDestroyOnLoad dontDestroyOnLoad;

    void Awake() {
        if (dontDestroyOnLoad == null) {
            dontDestroyOnLoad = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
}
