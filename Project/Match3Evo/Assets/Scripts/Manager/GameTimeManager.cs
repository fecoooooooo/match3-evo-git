using UnityEngine;
using UnityEngine.UI;

namespace Match3_Evo
{
    public class GameTimeManager : MonoBehaviour
    {
        [SerializeField] Text gameTimeText;
        [SerializeField] float warningStartTime = 15f;

        float gameTime;

        [Header("Testing"), Tooltip("You can test animations in slow motion with it.")]
        public float timeScaleInGame = 1.0f;
        public bool disableCountDown = false;

        public float GameTime
        {
            get { return gameTime; }
            set
            {
                gameTime = value;
                UpdateUI();
            }
        }

        public bool StopTime { get; set; }

        /// <summary>
        /// In the tutorial the game can end early and we want the clock to run down.
        /// </summary>
        public bool OverrunTime { get; set; }

        void Awake()
        {
            GM.timeMng = this;
        }

        public void InitGameTime()
        {
            gameTime = GM.boardMng.gameParameters.matchTimer;
            UpdateUI();
        }

        public void HandleGameTime()
        {
#if UNITY_EDITOR
            Time.timeScale = timeScaleInGame;
#endif

            if (OverrunTime || GM.boardMng.GameRunning && !StopTime)
            {
                gameTime -= Time.deltaTime;
                UpdateUI();
            }
        }

        void UpdateUI()
        {
            if (gameTime <= 0)
            {
                gameTime = 0;
                
                if (OverrunTime)
                    OverrunTime = false;
                else
                {
                    GM.boardMng.EndGame();
                }
            }
            else if (gameTime < warningStartTime)
            {
                warningStartTime = float.MinValue;
                GM.soundMng.Play(EnumSoundID.TimeWarningStart);
                GM.soundMng.PlayDelayed(EnumSoundID.TimeWarning, 1f);
            }
            gameTimeText.text = ((int)gameTime / 60).ToString("D2") + ":" + ((int)gameTime % 60).ToString("D2");
        }

        void Update()
        {
            HandleGameTime();
        }
    }
}