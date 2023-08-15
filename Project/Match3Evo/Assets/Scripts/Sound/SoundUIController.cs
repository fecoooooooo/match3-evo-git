using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Match3_Evo {
    public class SoundUIController : MonoBehaviour {
        [SerializeField] Slider soundSlider;
        bool ignoreVolumeChanged = true;

        public IEnumerator Start() {
            yield return new WaitForEndOfFrame();
            soundSlider.value = GM.soundMng.SoundVolume;
        }

        public void OnSoundVolumeChanged(float soundVolume) {
            if (ignoreVolumeChanged) {
                ignoreVolumeChanged = false;
            }
            else {
                GM.soundMng.Play(EnumSoundID.Break3);
                GM.soundMng.SoundVolume = soundVolume;
                GM.soundMng.SaveSettings();
            }
        }
    }
}