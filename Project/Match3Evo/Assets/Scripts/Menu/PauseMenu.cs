using UnityEngine;
using UnityEngine.UI;

namespace Match3_Evo
{
    public class PauseMenu : MonoBehaviour
    {
        bool audioShowen = false;

        [SerializeField] PauseButtonHandler pauseButton;
        [SerializeField] CanvasGroupFade canvasGroupFade;
        [SerializeField] Animator soundAnimator;
        [SerializeField] Animator musicAnimator;
        [SerializeField] Text pauseText;
        [SerializeField] Text continueButtonText;
        [SerializeField, TextArea(3, 10)] string pauseTextTemplate;

        public void Show()
        {
            if (GM.boardMng.gameParameters.gamePauseCount > 0)
            {
                GM.boardMng.graphicRaycaster.enabled = false;
                GM.boardMng.gameParameters.gamePauseCount--;
                pauseText.text = pauseTextTemplate.Replace("<COUNT>", GM.boardMng.gameParameters.gamePauseCount.ToString());
                GM.boardMng.boardCanvasGroupFade.Fade(0f, GM.boardMng.canvasGroupFadeSpeed);
                canvasGroupFade.Fade(0f);
                canvasGroupFade.Fade(1f, GM.boardMng.canvasGroupFadeSpeed);

                if (GM.boardMng.gameParameters.gamePauseCount == 0)
                    pauseButton.gameObject.SetActive(false);
            }
        }

        public void Hide(bool _quick = false)
        {
            GM.boardMng.graphicRaycaster.enabled = true;
            if (_quick)
            {
                GM.boardMng.boardCanvasGroupFade.Fade(1f);
                canvasGroupFade.Fade(0f);
            }
            else
            {
                GM.boardMng.boardCanvasGroupFade.Fade(1f, GM.boardMng.canvasGroupFadeSpeed);
                canvasGroupFade.Fade(0f, GM.boardMng.canvasGroupFadeSpeed);
            }
        }

        public void OnLeaveGame()
        {
            GM.boardMng.OnLeaveGame();
        }

        public void OnAudioControlsPressed()
        {
            audioShowen = !audioShowen;
            soundAnimator.SetBool("show", audioShowen);
            musicAnimator.SetBool("show", audioShowen);
        }

        public void OpenTutorial()
        {
            Instantiate(Resources.Load("Tutorial"), GetComponent<Transform>(), false);
        }

        internal void ChangeUIAsTutorial()
        {
            continueButtonText.text = "Practice";
            pauseButton.OnClick();
            pauseButton.gameObject.SetActive(false);
        }

        internal void ShowPauseButton()
        {
            pauseButton.gameObject.SetActive(true);
        }
    }
}
