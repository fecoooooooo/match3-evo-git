using System;
using System.Collections.Generic;
using UnityEngine;

namespace Match3_Evo
{

    [System.Serializable]
    public class Field
    {
        public EnumFieldState FieldState;
        public int rowIndex;
        public int columnIndex;
        public float breakAfterSeconds;
        public Vector2 fieldPosition;
        public FieldUI fieldUI;
        
        public int FieldVariant { get => (int)FieldType % GM.boardMng.gameParameters.TileVariantMax(); }
        public int EvoLvl { get => (int)FieldType / GM.boardMng.gameParameters.TileVariantMax(); }

		[HideInInspector] public Field Left = null;
        [HideInInspector] public Field Right = null;
        [HideInInspector] public Field Top = null;
        [HideInInspector] public Field Bottom = null;
        
        public bool SpecialType { get => (int)FieldType.SPECIAL <= (int)FieldType; }

        private Field swapField;
        private bool swapDone = false;
        private bool swapToUseable = false;

        public FieldType FieldType { get; set; }

        public Field(int _rowIndex, int _columnIndex, FieldType fieldType, int _score, Vector2 _fieldPosition, FieldUI _fieldUI)
        {
            rowIndex = _rowIndex;
            columnIndex = _columnIndex;
            fieldPosition = _fieldPosition;
            fieldUI = _fieldUI;

            this.FieldType = fieldType;
        }

        public Field(Field _field)
        {
            FieldType = _field.FieldType;
            fieldUI = _field.fieldUI;
        }

        public void FindRelations()
        {
            if (rowIndex > 0)
                Top = GM.boardMng.Fields[rowIndex - 1, columnIndex];

            if (columnIndex > 0)
                Left = GM.boardMng.Fields[rowIndex, columnIndex - 1];

            if (rowIndex < GM.boardMng.rows - 1)
                Bottom = GM.boardMng.Fields[rowIndex + 1, columnIndex];

            if (columnIndex < GM.boardMng.columns - 1)
                Right = GM.boardMng.Fields[rowIndex, columnIndex + 1];
        }

        public bool IsEmpty()
        {
            return fieldUI == null;
        }

        public int GetScore()
        {
			switch (FieldType)
			{
				case FieldType.DNS:
                    return 0;
				case FieldType.TREASURE:
                    return GM.boardMng.gameParameters.treasureScore;
				default:
                    return GM.boardMng.gameParameters.tileScore;
			}
        }

        public override string ToString()
        {
            return string.Format("[Field: rowIndex={0}, columnIndex={1}, color={2}, state={3}, swapDone={4}, fieldUI={5}, fieldPosition={6}]", rowIndex, columnIndex, FieldVariant, FieldState, swapDone, fieldUI.Rect.anchoredPosition, fieldPosition);
        }

        public void SwapWithField(Field _newSwapField)
        {
            GM.soundMng.Play(EnumSoundID.Swap);

            Field lvTemp = new Field(_newSwapField);

            _newSwapField.FieldType = FieldType;
            _newSwapField.fieldUI = fieldUI;

            FieldType = lvTemp.FieldType;
            fieldUI = lvTemp.fieldUI;
            swapField = _newSwapField;

            fieldUI.Initialize(this);
            _newSwapField.fieldUI.Initialize(_newSwapField);

            if (FieldState == EnumFieldState.Useable && _newSwapField.FieldState == EnumFieldState.Useable)
            {
                List<Mergeable> lvBreakable = GM.boardMng.FindBreakableFields();
                bool lvFound = false;

                for (int i = 0; i < lvBreakable.Count; i++)
                {
                    if (lvBreakable[i].Fields.Contains(this) || lvBreakable[i].Fields.Contains(_newSwapField))
                    {
                        lvFound = true;
                        break;
                    }
                }

                if (lvFound)
                {
                    GM.boardMng.BreakMergeables(lvBreakable);

                    swapToUseable = FieldState != EnumFieldState.Break;
                    _newSwapField.swapToUseable = _newSwapField.FieldState != EnumFieldState.Break;

                    ChangeFieldState(EnumFieldState.Swap);

                    _newSwapField.ChangeFieldState(EnumFieldState.Swap);
                }
                else
                {
                    ChangeFieldState(EnumFieldState.SwapBack);

                    _newSwapField.ChangeFieldState(EnumFieldState.SwapBack);
                }
            }

            fieldUI.StartTransition();
            _newSwapField.fieldUI.StartTransition();
        }

        public void MoveFieldHere(Field _movedField)
        {
            FieldUI lvSwapUI = fieldUI;

            _movedField.ChangeFieldState(EnumFieldState.Empty);

            ChangeFieldState(EnumFieldState.Move);

            FieldType = _movedField.FieldType;
            fieldUI = _movedField.fieldUI;

            _movedField.fieldUI = lvSwapUI;
            _movedField.fieldUI.gameObject.SetActive(false);

            fieldUI.Initialize(this);
            fieldUI.StartTransition();
        }

        public void RandomSwap(Field _randomSwapField)
        {
            Field lvSwapTmp = new Field(this);

            _randomSwapField.ChangeFieldState(EnumFieldState.Move);

            ChangeFieldState(EnumFieldState.Move);

            FieldType = _randomSwapField.FieldType;
            fieldUI = _randomSwapField.fieldUI;

            _randomSwapField.FieldType = lvSwapTmp.FieldType;
            _randomSwapField.fieldUI = lvSwapTmp.fieldUI;

            fieldUI.Initialize(this);
            fieldUI.StartTransition();

            _randomSwapField.fieldUI.Initialize(_randomSwapField);
            _randomSwapField.fieldUI.StartTransition();
        }

        public void Break(float _afterSeconds, bool _extraBreak = true)
        {
            if (!_extraBreak)
            {
                if (FieldState == EnumFieldState.Useable)
                {
                    breakAfterSeconds = _afterSeconds;
                    ChangeFieldState(EnumFieldState.Break);
                    fieldUI.StartTransition();
                }
                else
                {
                    if (breakAfterSeconds < _afterSeconds)
                    {
                        breakAfterSeconds = _afterSeconds;
                        fieldUI.CancelInvoke();
                        fieldUI.StartTransition();
                    }
                }

                GM.scoreMng.AddComboBonus(this);
            }
            else if (_extraBreak)
            {
                if (FieldState == EnumFieldState.Useable)
                {
                    breakAfterSeconds = _afterSeconds;
                    ChangeFieldState(EnumFieldState.Break);
                    fieldUI.StartTransition();
                }
                else
                {
                    if (breakAfterSeconds < _afterSeconds)
                    {
                        breakAfterSeconds = _afterSeconds;
                        fieldUI.CancelInvoke();
                        fieldUI.StartTransition();
                    }
                }
            }
        }

        public void ChangeFieldState(EnumFieldState _newFieldState)
        {
            GM.boardMng.fieldStateGlobalChangeTime = 2f;
            GM.boardMng.fieldStateGlobalIdealTime = GM.boardMng.gameParameters.hintTime;

            BreakBackground.HideHintBreakBackground();

            GM.boardMng.fieldStateCounter[(int)FieldState]--;
            GM.boardMng.fieldStateCounter[(int)_newFieldState]++;

            FieldState = _newFieldState;
        }

        public void EndTransition()
        {
            if (FieldState == EnumFieldState.Swap)
            {
                swapField = null;
                if (swapToUseable)
                {
                    ChangeFieldState(EnumFieldState.Useable);

                    swapToUseable = false;
                }
                else
                    ChangeFieldState(EnumFieldState.Break);
            }
            else if (FieldState == EnumFieldState.SwapBack && swapField != null)
            {
                if (!swapDone)
                {
                    swapDone = true;
                    SwapWithField(swapField);
                    GM.soundMng.Play(EnumSoundID.SwapWrong);
                }
                else
                {
                    swapField.fieldUI.ResetPosition();
                    ChangeFieldState(EnumFieldState.Useable);
                    swapField.ChangeFieldState(EnumFieldState.Useable);
                    swapDone = false;
                    swapField = null;
                }
            }
            else if (FieldState == EnumFieldState.Break)
            {
                GM.scoreMng.AddTileBreak(GetScore());
                
                if(!SpecialType)
                    GM.boardMng.collectedTileRoot.GetChild(FieldVariant).GetComponent<CollectedTile>().AddToCounter(1);
				else
				{
                    if (FieldType == FieldType.DNS)
                        GM.boardMng.DnsBreak(fieldUI);
                    else if (FieldType == FieldType.TREASURE)
                        GM.boardMng.TreasureBreak(fieldUI);
                }

                ChangeFieldState(EnumFieldState.Empty);
                fieldUI.gameObject.SetActive(false);
#if DEBUG
                fieldUI.fieldImage.color = Color.white;
#endif
            }
            else if (FieldState == EnumFieldState.Move)
                ChangeFieldState(EnumFieldState.ComboReady);
        }

        public int NeighborsWithFieldType(int _fieldType)
        {
            int lvSimilarCount = 0;

            if (FieldVariant == _fieldType)
                lvSimilarCount++;

            if (Left != null && Left.FieldVariant == _fieldType)
                lvSimilarCount++;

            if (Right != null && Right.FieldVariant == _fieldType)
                lvSimilarCount++;

            if (Top != null && Top.FieldVariant == _fieldType)
                lvSimilarCount++;

            if (Bottom != null && Bottom.FieldVariant == _fieldType)
                lvSimilarCount++;

            return lvSimilarCount;
        }

        internal static bool IsSameVariantOnSmallerEvoLvlThanCurrent(FieldType fieldType, int variantIndex)
        {
            return (int)fieldType % GM.boardMng.gameParameters.TileVariantMax() == variantIndex && (int)fieldType < GM.boardMng.currentEvolutionLvlPerVariant[variantIndex];
        }

        public static int TypeToVariant(FieldType fieldType)
		{
            return (int)fieldType % GM.boardMng.gameParameters.TileVariantMax();
        }

        public static int TypeToVEvoLvl(FieldType fieldType)
        {
            return (int)fieldType / GM.boardMng.gameParameters.TileVariantMax();
        }

        public static FieldType EvoLvlAndVariantToType(int evoLvl, int variant)
		{
            return (FieldType)(evoLvl * GM.boardMng.gameParameters.TileVariantMax() + variant);
		}
    }

    public enum EnumFieldState
    {
        Useable = 0,
        Swap = 1,
        SwapBack = 2,
        Break = 3,
        Empty = 4,
        Move = 5,
        ComboReady = 6,
        Hidden,
    }

    public enum FieldType
	{
        NONE = -1,

        V1_E0,
        V2_E0,
        V3_E0,
        V4_E0,

        STARTER_TYPE = V4_E0,

        V1_E1,
        V2_E1,
        V3_E1,
        V4_E1,

        V1_E2,
        V2_E2,
        V3_E2,
        V4_E2,

        V1_E3,
        V2_E3,
        V3_E3,
        V4_E3,

        V1_E4,
        V2_E4,
        V3_E4,
        V4_E4,

        V1_E5,
        V2_E5,
        V3_E5,
        V4_E5,
        LAST_NORMAL = V4_E5,

        SPECIAL,
        DNS = SPECIAL,
        TREASURE,
	}
}