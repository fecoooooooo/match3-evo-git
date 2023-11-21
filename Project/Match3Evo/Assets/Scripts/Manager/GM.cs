using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SkillzSDK;

namespace Match3_Evo
{
    public class GM : MonoBehaviour, SkillzMatchDelegate
    {
        public static GM Instance;

        public static BoardManager boardMng;
        public static ScoreManager scoreMng;
        public static SoundManager soundMng;
        public static GameTimeManager timeMng;
        public static DnsManager dnsManager;
        public static ObjectPool Pool;

        public string gameSceneName;
        public string menuSceneName;

        public static bool skillzRandomSeedReceived;
        public bool tutorialGame;
        public bool firstLoadUp;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                SceneManager.activeSceneChanged += SceneChanged;
                //FB.Init();
                HandleFirstLoadUp();
            }
        }

        private void Update()
        {
            //KeyCode.Escape is the Android back key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Optionally, you could show a "Are you sure you want to exit" prompt
                // (Just make sure the prompt can be confirmed via hitting back again)
                Application.Quit();
            }
        }

        private void HandleFirstLoadUp()
        {
            if (!PlayerPrefs.HasKey("HandleFirstLoadUp"))
            {
                PlayerPrefs.SetInt("HandleFirstLoadUp", 1);
                firstLoadUp = true;
            }
        }

        void SceneChanged(Scene arg0, Scene arg1)
        {
            // Debug.Log("SceneChanged" + arg0.name + "___" + arg1.name);
        }

        public void PlayNow(bool practice)
        {
            Debug.Log("OnPlayNow");
            tutorialGame = practice;
#if UNITY_EDITOR || UNITY_STANDALONE
            SceneManager.LoadScene(GM.Instance.gameSceneName);
#elif UNITY_ANDROID || UNITY_IOS
            if (practice)
            {
                SceneManager.LoadScene(GM.Instance.gameSceneName);
            }
            else
            {
                SkillzCrossPlatform.setSkillzMusicVolume(soundMng.MusicVolume);
                SkillzCrossPlatform.setSFXVolume(soundMng.SoundVolume);
                SkillzCrossPlatform.setSkillzBackgroundMusic("music.mp3");
                SkillzCrossPlatform.LaunchSkillz(this);
            }
#endif
        }

        public static int GetRandom(int _min, int _max)
        {
            InitSkillzRandom();
            return Random.Range(_min, _max);
        }

        public static float GetRandom()
        {
            InitSkillzRandom();
            return Random.value;
        }

        static void InitSkillzRandom()
        {
            if (!skillzRandomSeedReceived)
            {
                if (GM.IsSkillzMatchInProgress())
                {
                    int seed = 0;
                    seed = SkillzCrossPlatform.Random.Range(0, int.MaxValue);
                    Debug.Log("InitSkillzRandom:" + seed);
                    Random.InitState(seed);
                }

                skillzRandomSeedReceived = true;
            }
        }

        public static Dictionary<string, string> GetMatchRules()
        {
#if UNITY_EDITOR
            return null;
#elif UNITY_ANDROID || UNITY_IOS
            Debug.Log("InitializeGameUpdateFromSkillzGetMatchRules");
            Hashtable table = SkillzCrossPlatform.GetMatchRules();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (string key in table.Keys) {
                dic.Add(key, table[key].ToString());
            }
            return dic;
#endif

            // return null;
        }

        public static bool IsSkillzMatchInProgress()
        {
#if UNITY_EDITOR
            return false;
#elif UNITY_ANDROID || UNITY_IOS
            return SkillzCrossPlatform.IsMatchInProgress();
#endif

            // return false;
        }

        public static void UpdatePlayersCurrentScore(float score)
        {
            SkillzCrossPlatform.UpdatePlayersCurrentScore(score.ToString());
        }

        public static void FinishTournament(float score)
        {
            SkillzCrossPlatform.ReportFinalScore(score);
        }

        public static void AbortMatch()
        {
            SkillzCrossPlatform.AbortMatch();
        }

        public void OnMatchWillBegin(Match matchInfo)
        {
            soundMng.LoadSettings();
            Debug.Log("OnMatchWillBegin");
            skillzRandomSeedReceived = false;
            SceneManager.LoadScene(Instance.gameSceneName);
        }

        public void OnSkillzWillExit()
        {
            soundMng.LoadSettings();
            Debug.Log("OnSkillzWillExit");
            SceneManager.LoadScene(Instance.menuSceneName);
        }
    }
}
