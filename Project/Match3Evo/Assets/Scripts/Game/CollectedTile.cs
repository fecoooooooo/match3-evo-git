using UnityEngine;
using UnityEngine.UI;

namespace Match3_Evo {

    public class CollectedTile : MonoBehaviour {

        public Image image;
        public Text text;

        public int counter;

        void Awake() {
            counter = 0;
            AddToCounter(0);
        }

        public void AddToCounter(int _value)
        {
            counter += _value;
            text.text = counter.ToString("0");
        }

    }

}