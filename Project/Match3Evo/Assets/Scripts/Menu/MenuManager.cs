using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Match3_Evo
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] Text SinceStartText;
        [SerializeField] Text SinceLevelLoadText;
        [SerializeField] Animator audioControlsAnimator;

        string audioControlsAnimationStateKey = "state";
        bool audioControlsOn = false;

        void Awake()
        {
            Debug.Log("MenuManagerAwake");
        }

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            GM.soundMng.StopAll();
            GM.soundMng.Play(EnumSoundID.MenuMusic);
        }

        void Update()
        {
            SinceStartText.text = ((int)Time.realtimeSinceStartup).ToString();
            SinceLevelLoadText.text = ((int)Time.timeSinceLevelLoad).ToString();
        }

        public void OnPlayNow()
        {
            GM.Instance.PlayNow(false);
        }

        public void OnPractice()
        {
            GM.Instance.PlayNow(true);
        }

        public void OnLearnToPlay()
        {
            Debug.Log("OnLearnToPlay");
            GM.Instance.tutorialGame = true;
            SceneManager.LoadScene(GM.Instance.gameSceneName);
        }

        public void OnClickAudioControls()
        {
            audioControlsOn = !audioControlsOn;
            audioControlsAnimator.SetBool(audioControlsAnimationStateKey, audioControlsOn);
        }
    }
}