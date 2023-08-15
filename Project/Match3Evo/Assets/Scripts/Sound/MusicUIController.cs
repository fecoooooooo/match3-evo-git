using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Match3_Evo {
    public class MusicUIController : MonoBehaviour {
        [SerializeField] Slider musicSlider;
        bool ignoreVolumeChanged = true;

        public IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            musicSlider.value = GM.soundMng.MusicVolume;
        }

        public void OnMusicVolumeChanged(float musicVolume)
        {
            if (ignoreVolumeChanged)
            {
                ignoreVolumeChanged = false;
            }
            else
            {
                GM.soundMng.MusicVolume = musicVolume;
                GM.soundMng.SaveSettings();
            }
        }
    }
}