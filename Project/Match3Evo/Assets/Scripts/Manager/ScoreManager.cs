using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Match3_Evo
{
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] Text gameScoreText;
        [SerializeField] Text submitButtonText;
        [SerializeField] ScorePanel scoreSummaryPanel;
        [SerializeField] BoardScoreBonus boardScoreBonusPrefab;
        [SerializeField] RectTransform board;
        [SerializeField] RectTransform boardScoreUIParent;

        public int gameScore;
        public int tileBreakCount;

        bool scoreMaxReached = false;
        //Infromation about the last merge score value, valid until the board is refilled after a merge
        bool lastMergeWasScoreMax = false;
        List<int> scoreBonuses = new List<int>();
        int allTimeBestScore = 0;
        int todaysBestScore = 0;
        int bonusScoreSum = 0;
        bool newTodayScore = true;
        bool newAllTimeScore = false;
        // bool showScoreSummaryPanel = false;

        [SerializeField, Header("Editor Test")] bool resetStatistics = false;

        public bool ScoreMaxReached { get { return scoreMaxReached; } set { scoreMaxReached = value; } }

        public bool LastMergeWasScoreMax { get { return lastMergeWasScoreMax; } }

        string allTimeBestScorePrefsKey = "allTimeBestPrefsKey";
        string todaysBestScorePrefsKey = "todaysBestScorePrefsKey";
        string lastSavedDayPrefsKey = "lastSavedDayPrefsKey";

        void Awake()
        {
            GM.scoreMng = this;
        }

        void Update()
        {
            HandleAddingScore();
        }

        void OnEnable()
        {
            GM.boardMng.colorTransitionEndedDelegate += ColorTransitionEndedCallBack;
        }

        void OnDisable()
        {
            GM.boardMng.colorTransitionEndedDelegate -= ColorTransitionEndedCallBack;
        }

        void ColorTransitionEndedCallBack()
        {
            //Every time a merge is ended we reset lastMergeWasScoreMax
            lastMergeWasScoreMax = false;
        }

        void HandleAddingScore()
        {
            gameScoreText.text = FormatScore(gameScore);
        }

        void AddAllBonusScore()
        {
            int lvFinalBonus = 0;

            for (int lvBonusIndex = 0; lvBonusIndex < scoreBonuses.Count; lvBonusIndex++)
                lvFinalBonus += scoreBonuses[lvBonusIndex];

            scoreBonuses.Clear();
            if (lvFinalBonus > 0)
                scoreBonuses.Add(lvFinalBonus);
        }

        void UpdateScoreStatictics()
        {
#if UNITY_EDITOR
            if (resetStatistics)
            {
                PlayerPrefs.DeleteKey(allTimeBestScorePrefsKey);
                PlayerPrefs.DeleteKey(todaysBestScorePrefsKey);
                PlayerPrefs.DeleteKey(lastSavedDayPrefsKey);
            }
#endif

            if (PlayerPrefs.HasKey(allTimeBestScorePrefsKey))
                allTimeBestScore = PlayerPrefs.GetInt(allTimeBestScorePrefsKey);

            if (allTimeBestScore < gameScore)
            {
                newAllTimeScore = true;
                allTimeBestScore = gameScore;
                PlayerPrefs.SetInt(allTimeBestScorePrefsKey, allTimeBestScore);
            }

            if (PlayerPrefs.HasKey(lastSavedDayPrefsKey))
            {
                if (DateTime.Now.Day != PlayerPrefs.GetInt(lastSavedDayPrefsKey))
                {
                    PlayerPrefs.SetInt(lastSavedDayPrefsKey, DateTime.Now.Day);
                    PlayerPrefs.DeleteKey(todaysBestScorePrefsKey);
                }
            }
            else
                PlayerPrefs.SetInt(lastSavedDayPrefsKey, DateTime.Now.Day);

            if (PlayerPrefs.HasKey(todaysBestScorePrefsKey))
            {
                todaysBestScore = PlayerPrefs.GetInt(todaysBestScorePrefsKey);
                if (todaysBestScore < gameScore)
                {
                    todaysBestScore = gameScore;
                    PlayerPrefs.SetInt(todaysBestScorePrefsKey, todaysBestScore);
                }
                else
                    newTodayScore = false;
            }
            else
            {
                todaysBestScore = gameScore;
                PlayerPrefs.SetInt(todaysBestScorePrefsKey, todaysBestScore);
            }
        }

        void ShowScoreSummaryPanel()
        {
            GM.boardMng.graphicRaycaster.enabled = false;
            UpdateScoreStatictics();
            GM.boardMng.boardCanvasGroupFade.Fade(0f, GM.boardMng.canvasGroupFadeSpeed);
            scoreSummaryPanel.canvasGroupFade.Fade(1f, GM.boardMng.canvasGroupFadeSpeed);
        }

        #region ExternalCalls

        public void Initialize()
        {
            tileBreakCount = 0;
        }

        //Called every time a merge is may score
        public void OnScoreMaxReached()
        {
            scoreMaxReached = true;
            lastMergeWasScoreMax = true;
        }

        public void AddTileBreak()
        {
            tileBreakCount++;
            gameScore += GM.boardMng.gameParameters.tileScore;

            if (GM.IsSkillzMatchInProgress())
                GM.UpdatePlayersCurrentScore((float)gameScore);
        }

        public void AddComboBonus(Field _onField, int _combo = 1)
        {
            // coinBar.AddFeel(_combo, _onField);
        }

        internal void ChangeUIAsTutorial()
        {
            submitButtonText.text = "Close Practice";
        }

        public void ScoreSummary()
        {
            ShowScoreSummaryPanel();
        }

        public int GetAllTimeBestScore()
        {
            return allTimeBestScore;
        }

        public bool IsAllTimeBestScoreNew()
        {
            return newAllTimeScore;
        }

        public int GetTodaysBestScore()
        {
            return todaysBestScore;
        }

        public bool IsTodaysBestScoreNew()
        {
            return newTodayScore;
        }

        public int GetBonusScore()
        {
            return bonusScoreSum;
        }

        public string FormatScore(int _scoreToFormat)
        {
            return _scoreToFormat.ToString("N0").Replace(" ", ",");
        }
        #endregion
    }
}