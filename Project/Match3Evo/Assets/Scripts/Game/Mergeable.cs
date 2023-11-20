using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Match3_Evo
{
    [System.Serializable]
    public class Mergeable
    {
        public bool isRow;
        public EnumMergeableType mergeableType;
        public List<Field> Fields { get; private set; }
        public Vector2 breakUIWidth;
        public PossibleSwap possibleSwapData = null;

        Field topLeftField;
        int replaceableIndex = 0;
        FieldType fieldType = FieldType.NONE;

        int scoreMultiplier = -1;

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

        public Mergeable(int _count, bool _isRow, FieldType _fieldType = FieldType.NONE)
        {
            isRow = _isRow;
            Fields = new List<Field>(_count);
            fieldType = _fieldType;

            if (_count == 2)
                mergeableType = EnumMergeableType.Hint;
            else if (_count == 3)
                mergeableType = EnumMergeableType.Three;
            else if (_count == 5)
                mergeableType = EnumMergeableType.Three;    //TODO redo this
            else
                mergeableType = EnumMergeableType.Three;    //TODO redo this
        }

        public Vector2 GetScoreFXPosition()
        {
            if (isRow)
            {
                if (Fields.Count % 2 == 0)
                    return Fields[Fields.Count / 2].fieldPosition + new Vector2(0f, -GM.boardMng.fieldSize * 0.5f);
                else
                    return Fields[Fields.Count / 2].fieldPosition + new Vector2(GM.boardMng.fieldSize * 0.5f, -GM.boardMng.fieldSize * 0.5f);
            }
            else
            {
                if (Fields.Count % 2 == 0)
                    return Fields[Fields.Count / 2].fieldPosition + new Vector2(GM.boardMng.fieldSize * 0.5f, GM.boardMng.fieldSize * 0.5f);
                else
                    return Fields[Fields.Count / 2].fieldPosition + new Vector2(GM.boardMng.fieldSize * 0.5f, -GM.boardMng.fieldSize * 0.5f);
            }
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
                if (mergeableType == EnumMergeableType.Line || mergeableType == EnumMergeableType.Three)
                        lvScore += GM.boardMng.gameParameters.tileScore;
                else if (mergeableType == EnumMergeableType.Box)
                {
                    lvScore += GM.boardMng.gameParameters.tileScore;
                        
                    if (isRow)
                    {
                        if (Fields[i].Top != null)
                        {
                            lvScore += GM.boardMng.gameParameters.tileScore;

                            if (Fields[i].Top.Top != null)
                                lvScore += GM.boardMng.gameParameters.tileScore;
                        }
                        if (Fields[i].Bottom != null)
                        {
                            lvScore += GM.boardMng.gameParameters.tileScore;

                            if (Fields[i].Bottom.Bottom != null)
                                lvScore += GM.boardMng.gameParameters.tileScore;
                        }
                    }
                }
            }

            return lvScore * scoreMultiplier;
        }

        public Field GetReplaceableField()
        {
            int lvIndex = replaceableIndex;

            while (lvIndex - replaceableIndex < 3 && lvIndex < Fields.Count - 1)
            {
                if (Fields[lvIndex].FieldType == fieldType)
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
            else if (mergeableType == EnumMergeableType.Line)
                GM.soundMng.PlayDelayed(EnumSoundID.BreakLine, GM.boardMng.breakDelayTime * 0.9f);
            else if (mergeableType == EnumMergeableType.Box)
                GM.soundMng.PlayDelayed(EnumSoundID.Break5, GM.boardMng.breakDelayTime * 0.9f);
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
    }


    public enum EnumMergeableType
    {
        Three,
        Line,
        Box,
        Hint
    }
}