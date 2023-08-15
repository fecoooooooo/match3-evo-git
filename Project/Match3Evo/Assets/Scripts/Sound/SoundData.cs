using UnityEngine;

namespace Match3_Evo
{
    public class SoundData : MonoBehaviour
    {
        public EnumSoundID SoundID;
        [HideInInspector] public AudioSource AudioSource;

        private void Start()
        {
            if (GM.soundMng == GetComponentInParent<SoundManager>())
            {
                AudioSource = GetComponent<AudioSource>();
                GM.soundMng.AddSoundData(this);
            }
        }
    }
}