using UnityEngine;
using UnityEngine.UI;

namespace Match3_Evo
{

    public class CountDown : MonoBehaviour
    {
        [SerializeField] Text countDonwText;
        [SerializeField] Animator animator;
        public float blurFaid;

        public void Init()
        {
            gameObject.SetActive(true);
            Invoke(nameof(EnableAnimation), 1.0f);
            countDonwText.text = string.Empty;
        }

        void EnableAnimation()
        {
            animator.enabled = true;
        }

        public void OnCountDownTick(string _countText)
        {
            GM.soundMng.Play(EnumSoundID.GameStartCountDown);
            countDonwText.text = _countText;
        }

        public void OnStartGame()
        {
            GM.boardMng.StartGame();
        }

        public void OnCountDownEnded()
        {
            GM.boardMng.StartGame();
            Destroy(gameObject);
        }
    }
}