using UnityEngine;

namespace Match3_Evo
{
    public class PoolObject : MonoBehaviour
    {
        public bool DisableOnDisableCallback = true;
        public bool ActiveObject { get; set; }

        private void OnDisable()
        {
            if (DisableOnDisableCallback)
            {
                ActiveObject = false;
            }
        }
    }
}