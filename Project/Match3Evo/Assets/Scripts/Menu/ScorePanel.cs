using UnityEngine;

namespace Match3_Evo
{
    public class ScorePanel : MonoBehaviour
    {
        public CanvasGroupFade canvasGroupFade;
        public ScorePanelRow tilesDestroyed;
        public ScorePanelRow final;
        public ScorePanelRow today;
        public ScorePanelRow allTime;

        void Awake()
        {
            tilesDestroyed.Setup(GM.scoreMng.tileBreakCount * GM.boardMng.gameParameters.tileScore, false, GM.scoreMng.tileBreakCount);
            final.Setup(GM.scoreMng.gameScore);
            today.Setup(GM.scoreMng.GetTodaysBestScore(), GM.scoreMng.IsTodaysBestScoreNew());
            allTime.Setup(GM.scoreMng.GetAllTimeBestScore(), GM.scoreMng.IsAllTimeBestScoreNew());
        }

        public void PlayFinalScoreSound()
        {
            GM.soundMng.Play(EnumSoundID.FinalScore);
        }

        public void PlayBestTodayScoreSound()
        {
            if (GM.scoreMng.IsTodaysBestScoreNew())
                GM.soundMng.Play(EnumSoundID.FinalScore);
        }

        public void PlayBestAllTimeScoreSound()
        {
            if (GM.scoreMng.IsAllTimeBestScoreNew())
                GM.soundMng.Play(EnumSoundID.FinalScore);
        }
    }
}