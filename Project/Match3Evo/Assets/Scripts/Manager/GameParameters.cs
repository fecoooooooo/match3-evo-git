using System.Collections.Generic;
using UnityEngine;

namespace Match3_Evo
{

    [System.Serializable]
    public class GameParameters
    {
        public int matchTimer;
        public int hintTime;
        public int tileVariantMax;
        public int tileScore;
        public int gamePauseCount;
        public bool finaleClear;

        public int boostCooldown = 30;
        public float timeTillFirstBreak = 0.3f;
        public float timeBetweenBreaks = 0.05f;
        
        public float fireTime = 2f;
        public float fireSpreadTime = .5f;

        public int[] mergesToNextEvolution = new int[4];
        public int[] scoreMultiplierPerEvolution = new int[6];
        public int scoreForEvo;
        public int scoreForMaxEvo;

        public int treasureCount;
        public int treasureScore;

        public int dnsCount;
        public float dnsAmountPerCollect = .5f;

        string match_timerKey = "match_timer";
        string hint_timeKey = "hint_time";
        string tile_variantMaxKey = "tile_id";
        string tile_scoreKey = "tile_add_score";
        string game_pause_countKey = "game_pause_Count";
        string finale_clearKey = "finale_Clear";

        public int TileVariantMax()
        {
            return tileVariantMax;
        }

        public void UpdateFromSkillz(Dictionary<string, string> _gameParams)
        {
            int tryParseInt;

            if (_gameParams != null)
            {
                string log = "UpdateFromSkillz dictionary\n";

                foreach (KeyValuePair<string, string> entry in _gameParams)
                    log += "[" + entry.Key + "|" + entry.Value + "]";

                Debug.Log(log);

                if (_gameParams.ContainsKey(match_timerKey) && int.TryParse(_gameParams[match_timerKey], out tryParseInt))
                    matchTimer = tryParseInt;

                if (_gameParams.ContainsKey(hint_timeKey) && int.TryParse(_gameParams[hint_timeKey], out tryParseInt))
                    hintTime = tryParseInt;

                if (_gameParams.ContainsKey(tile_variantMaxKey) && int.TryParse(_gameParams[tile_variantMaxKey], out tryParseInt))
                    tileVariantMax = tryParseInt;

                if (_gameParams.ContainsKey(tile_scoreKey) && int.TryParse(_gameParams[tile_scoreKey], out tryParseInt))
                    tileScore = tryParseInt;

                 if (_gameParams.ContainsKey(game_pause_countKey) && int.TryParse(_gameParams[game_pause_countKey], out tryParseInt))
                    gamePauseCount = tryParseInt;

                if (_gameParams.ContainsKey(finale_clearKey) && int.TryParse(_gameParams[finale_clearKey], out tryParseInt))
                    finaleClear = tryParseInt != 0;

                Debug.Log("UpdateFromSkillz" + JsonUtility.ToJson(this));
            }
        }

        public void TestUpdateFromSkillz()
        {
            Dictionary<string, string> lvGameParams = new Dictionary<string, string>();

            lvGameParams.Add(match_timerKey, "180");
            lvGameParams.Add(hint_timeKey, "5");
            lvGameParams.Add(tile_variantMaxKey, "6");

            lvGameParams.Add(tile_scoreKey, "10");
            lvGameParams.Add(game_pause_countKey, "3");
            lvGameParams.Add(finale_clearKey, "1");

            UpdateFromSkillz(lvGameParams);
        }
    }

}