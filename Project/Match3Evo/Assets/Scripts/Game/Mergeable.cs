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
        public List<Field> fields;
        public Vector2 breakUIWidth;
        public PossibleSwap possibleSwapData = null;

        Field topLeftField;
        int replaceableIndex = 0;
        int fieldType = -1;

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
                return fields[fields.Count - 1];
            }
        }

        public Field BoxField { get; private set; }

        public Mergeable(int _count, bool _isRow, int _fieldType = -1)
        {
            isRow = _isRow;
            fields = new List<Field>(_count);
            fieldType = _fieldType;

            if (_count == 2)
                mergeableType = EnumMergeableType.Hint;
            else if (_count == 3)
                mergeableType = EnumMergeableType.Three;
            else if (_count == 5)
                mergeableType = EnumMergeableType.Box;
            else
                mergeableType = EnumMergeableType.Line;
        }

        public Vector2 GetScoreFXPosition()
        {
            Vector2 lvPosition = Vector2.zero;

            if (isRow)
            {
                if (fields.Count % 2 == 0)
                    return fields[fields.Count / 2].fieldPosition + new Vector2(0f, -GM.boardMng.fieldSize * 0.5f);
                else
                    return fields[fields.Count / 2].fieldPosition + new Vector2(GM.boardMng.fieldSize * 0.5f, -GM.boardMng.fieldSize * 0.5f);
            }
            else
            {
                if (fields.Count % 2 == 0)
                    return fields[fields.Count / 2].fieldPosition + new Vector2(GM.boardMng.fieldSize * 0.5f, GM.boardMng.fieldSize * 0.5f);
                else
                    return fields[fields.Count / 2].fieldPosition + new Vector2(GM.boardMng.fieldSize * 0.5f, -GM.boardMng.fieldSize * 0.5f);
            }
        }

        public int GetScoreBonus()
        {
            int lvScore = 0;

            for (int i = 0; i < fields.Count; i++)
            {
                if (mergeableType == EnumMergeableType.Line || mergeableType == EnumMergeableType.Three)
                        lvScore += GM.boardMng.gameParameters.tileScore;
                else if (mergeableType == EnumMergeableType.Box)
                {
                    lvScore += GM.boardMng.gameParameters.tileScore;
                        
                    if (isRow)
                    {
                        if (fields[i].Top != null)
                        {
                            lvScore += GM.boardMng.gameParameters.tileScore;

                            if (fields[i].Top.Top != null)
                                lvScore += GM.boardMng.gameParameters.tileScore;
                        }
                        if (fields[i].Bottom != null)
                        {
                            lvScore += GM.boardMng.gameParameters.tileScore;

                            if (fields[i].Bottom.Bottom != null)
                                lvScore += GM.boardMng.gameParameters.tileScore;
                        }
                    }
                }
            }
            return lvScore;
        }

        public Field GetReplaceableField()
        {
            int lvIndex = replaceableIndex;

            while (lvIndex - replaceableIndex < 3 && lvIndex < fields.Count - 1)
            {
                if (fields[lvIndex].fieldVariant == fieldType)
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
                return fields[lvIndex - 1];
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
                BoxField = fields[fields.Count / 2];
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