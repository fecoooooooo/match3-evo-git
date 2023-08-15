using UnityEngine;

namespace Match3_Evo {

    public class ScorePanelSoundFX : MonoBehaviour {
        
        public void PlayFinalScoreSound()
        {
            GM.soundMng.PlayDelayed(EnumSoundID.FinalScore, 0.5f);
            FindObjectOfType<ScorePanel>().final.PlayScoreAnimation();
        }

        public void PlayBestTodayScoreSound()
        {
            if (GM.scoreMng.IsTodaysBestScoreNew())
            {
                GM.soundMng.PlayDelayed(EnumSoundID.FinalScore, 0.5f);
                FindObjectOfType<ScorePanel>().today.PlayScoreAnimation();
            }
        }

        public void PlayBestAllTimeScoreSound()
        {
            if (GM.scoreMng.IsAllTimeBestScoreNew())
            {
                GM.soundMng.PlayDelayed(EnumSoundID.FinalScore, 0.5f);
                FindObjectOfType<ScorePanel>().allTime.PlayScoreAnimation();
            }
        }
    }
}