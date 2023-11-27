using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Match3_Evo
{
    [System.Serializable]
    public class Mergeable
    {
        public EnumMergeableType mergeableType;
        public List<Field> Fields { get; private set; }
        public Vector2 breakUIWidth;
        public PossibleSwap possibleSwapData = null;

        Field topLeftField;
        int replaceableIndex = 0;
        public FieldType FieldType { get; private set; } = FieldType.NONE;

        int scoreMultiplier = -1;

        public List<int> RowsWithMatch { get; private set; } = new List<int>();
        public List<int> ColsWithMatch { get; private set; } = new List<int>();

        public Field TopLeftField
        {
            get
            {
                return topLeftField;
            }
            set
            {
                topLeftField = value;
                GM.boardMng.StartCoroutine(GM.boardMng.ShowBreakBackground(this));
            }
        }

        public Field LastField
        {
            get
            {
                return Fields[Fields.Count - 1];
            }
        }

        public Field BoxField { get; private set; }

        public Mergeable(int count, List<int> rowsWithMatch, List<int> colsWithMatch, FieldType fieldType = FieldType.NONE)
        {
            this.RowsWithMatch = rowsWithMatch;
            this.ColsWithMatch = colsWithMatch;

            Fields = new List<Field>(count);
            FieldType = fieldType;

            DecideMergableType(count);
        }

        void DecideMergableType(int count)
		{
            if (count == 2)
                mergeableType = EnumMergeableType.Hint;
            else if (count == 3)
                mergeableType = EnumMergeableType.Three;
            else if (count == 4)
                mergeableType = EnumMergeableType.Four;
            else if (count == 5)
                mergeableType = EnumMergeableType.Five;
            else if (count == 6)
                mergeableType = EnumMergeableType.Six;
            else if (7 <= count)
                mergeableType = EnumMergeableType.SevenOrBigger;
            else
                throw new Exception("No proper count for mergable");
        }

        public Vector2 GetScoreFXPosition()
        {
            if (Fields.Count % 2 == 0)
                return Fields[Fields.Count / 2].fieldPosition + new Vector2(0f, -GM.boardMng.fieldSize * 0.5f);
            else
                return Fields[Fields.Count / 2].fieldPosition + new Vector2(GM.boardMng.fieldSize * 0.5f, -GM.boardMng.fieldSize * 0.5f);
        }

        public void AddField(Field f)
		{
            if(scoreMultiplier == -1)
			{
                int currentVariant = f.FieldVariant;
                int evolutionLvlForVariant = GM.boardMng.currentEvolutionLvlPerVariant[currentVariant];
                scoreMultiplier = GM.boardMng.gameParameters.scoreMultiplierPerEvolution[evolutionLvlForVariant];
            }

            Fields.Add(f);
		}

        public void AddFieldRange(List<Field> allFieldToAdd)
        {
            if (scoreMultiplier == -1)
            {
                int currentVariant = allFieldToAdd[0].FieldVariant;
                int evolutionLvlForVariant = GM.boardMng.currentEvolutionLvlPerVariant[currentVariant];
                scoreMultiplier = GM.boardMng.gameParameters.scoreMultiplierPerEvolution[evolutionLvlForVariant];
            }

            Fields.AddRange(allFieldToAdd);
        }

        public int GetScoreBonus()
        {
            int lvScore = 0;

            for (int i = 0; i < Fields.Count; i++)
            {
                lvScore += GM.boardMng.gameParameters.tileScore;
                //if (mergeableType == EnumMergeableType.Line || mergeableType == EnumMergeableType.Three)
                //else if (mergeableType == EnumMergeableType.Box)
                //{
                //    lvScore += GM.boardMng.gameParameters.tileScore;
                //        
                //    if (isRow)
                //    {
                //        if (Fields[i].Top != null)
                //        {
                //            lvScore += GM.boardMng.gameParameters.tileScore;
                //
                //            if (Fields[i].Top.Top != null)
                //                lvScore += GM.boardMng.gameParameters.tileScore;
                //        }
                //        if (Fields[i].Bottom != null)
                //        {
                //            lvScore += GM.boardMng.gameParameters.tileScore;
                //
                //            if (Fields[i].Bottom.Bottom != null)
                //                lvScore += GM.boardMng.gameParameters.tileScore;
                //        }
                //    }
                //}
            }

            return lvScore * scoreMultiplier;
        }

        public Field GetReplaceableField()
        {
            int lvIndex = replaceableIndex;

            while (lvIndex - replaceableIndex < 3 && lvIndex < Fields.Count - 1)
            {
                if (Fields[lvIndex].FieldType == FieldType)
                    lvIndex++;
                else
                {
                    if (lvIndex - replaceableIndex < 2)
                        replaceableIndex = ++lvIndex;
                    else
                        break;
                }
            }

            if (lvIndex - replaceableIndex >= 2)
            {
                replaceableIndex = lvIndex;
                return Fields[lvIndex - 1];
            }
            else
                return null;
        }

        public void PlayBreakSound()
        {
            if (mergeableType == EnumMergeableType.Three)
                GM.soundMng.PlayDelayed(EnumSoundID.Break3, GM.boardMng.breakDelayTimeFast * 0.9f);
        }

        public class PossibleSwap
        {
            Field inLineField;
            Field sideField;

            public PossibleSwap(Field _inLineField, Field _sideField)
            {
                inLineField = _inLineField;
                sideField = _sideField;
                //inLineField.fieldUI.GetComponentInChildren<Image>().color = new Color(1f, 1f, 1f, 0.3f);
            }
        }

        public void UpdateBoxFieldTo(Field _field = null)
        {
            if (_field != null)
                BoxField = _field;
            else
                BoxField = Fields[Fields.Count / 2];
        }

        public void ClearFields()
        {
            Fields.Clear();
        }

		internal bool IsNeighbour(Mergeable possibleNeighbour)
		{
            if (FieldType != possibleNeighbour.FieldType)
                return false;

			foreach(var f in Fields)
			{
                foreach(var otherF in possibleNeighbour.Fields)
				{
                    if (f.Left == otherF || f.Right == otherF || f.Top == otherF || f.Bottom == otherF)
                        return true;
				}
			}

            return false;
		}

		internal void UniteWith(Mergeable other)
		{
            foreach (var f in other.Fields)
			{
                if(!Fields.Contains(f))
                    Fields.Add(f);
			}

            DecideMergableType(Fields.Count);

            RowsWithMatch = RowsWithMatch.Union(other.RowsWithMatch).ToList();
            ColsWithMatch = ColsWithMatch.Union(other.ColsWithMatch).ToList();
        }
	}


    public enum EnumMergeableType
    {
        Three,
        Four,
        Five,
        Six,
        SevenOrBigger,
        Hint
    }
}