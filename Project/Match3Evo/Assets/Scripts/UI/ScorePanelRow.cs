using UnityEngine;
using UnityEngine.UI;

namespace Match3_Evo
{
    public class ScorePanelRow : MonoBehaviour
    {
        [SerializeField] Text text;
        [SerializeField] Text count;
        [SerializeField] Text score;
        [SerializeField] Text newScore;
        [SerializeField] Color newScoreColor;
        [SerializeField] Animator scoreAnimator;

        CanvasGroup canvasGroup;
        int countTarget = 0;
        int scoreTarget = 0;
        float interpolation = 0;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Setup(int _score, bool _newScore = false, int _count = -1)
        {
            countTarget = _count;
            scoreTarget = _score;

            score.text = "0";

            if (_newScore)
            {
                text.color = newScoreColor;
                score.color = newScoreColor;
            }
            else
                newScore.gameObject.SetActive(false);
        }

        public void PlayScoreAnimation()
        {
            scoreAnimator.enabled = true;
        }

        private void Update()
        {
            if (canvasGroup.alpha > 0.1 && interpolation < 1f)
            {
                interpolation = Mathf.Clamp01(interpolation + Time.deltaTime);
                count.text = GM.scoreMng.FormatScore(Mathf.FloorToInt(countTarget * interpolation));
                score.text = GM.scoreMng.FormatScore(Mathf.FloorToInt(scoreTarget * interpolation));
            }
        }
    }
}