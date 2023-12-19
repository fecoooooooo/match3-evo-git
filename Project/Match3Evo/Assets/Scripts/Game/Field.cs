using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
        
        public bool Is2x2 { get; set; }
        public bool CanFall { get; set; } = true;
        public bool JokerAfterBreak { get; set; }
        public bool WillBreakX { get; set; }
        public bool WillBreakY { get; set; }
        
        public bool SpecialType { get => (int)FieldType.SPECIAL <= (int)FieldType; }

        public UnityEvent breakEvent;

        public Field swapField;
        private bool swapDone = false;
        public bool swapToUseable = false;

        public Field TopRight2x2;
        public Field BottomLeft2x2;
        public Field BottomRight2x2;

        public FieldType FieldType { get; set; }

        public Field(int _rowIndex, int _columnIndex, FieldType fieldType, int _score, Vector2 _fieldPosition, FieldUI _fieldUI)
        {
            rowIndex = _rowIndex;
            columnIndex = _columnIndex;
            fieldPosition = _fieldPosition;
            fieldUI = _fieldUI;

            breakEvent = new UnityEvent();

            this.FieldType = fieldType;
        }

        public Field(Field _field)
        {
            FieldType = _field.FieldType;
            fieldUI = _field.fieldUI;

            if (_field.Is2x2)
                TurnTo2x2();
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

        //public void SwapWithField(Field _newSwapField)
        //{
        //    GM.soundMng.Play(EnumSoundID.Swap);
        //
        //    Field lvTemp = new Field(_newSwapField);
        //
        //    _newSwapField.FieldType = FieldType;
        //    _newSwapField.fieldUI = fieldUI;
        //    _newSwapField.Is2x2 = Is2x2;
        //
        //    FieldType = lvTemp.FieldType;
        //    fieldUI = lvTemp.fieldUI;
        //    Is2x2 = lvTemp.Is2x2;
        //
        //    swapField = _newSwapField;
        //
        //    fieldUI.Initialize(this);
        //    _newSwapField.fieldUI.Initialize(_newSwapField);
        //
        //    if (FieldState == EnumFieldState.Useable && _newSwapField.FieldState == EnumFieldState.Useable)
        //    {
        //        List<Mergeable> lvBreakable = GM.boardMng.FindBreakableFields();
        //        bool lvFound = false;
        //
        //        for (int i = 0; i < lvBreakable.Count; i++)
        //        {
        //            if (lvBreakable[i].Fields.Contains(this) || lvBreakable[i].Fields.Contains(_newSwapField))
        //            {
        //                lvFound = true;
        //                break;
        //            }
        //        }
        //
        //        if (lvFound)
        //        {
        //            GM.boardMng.BreakMergeables(lvBreakable);
        //
        //            swapToUseable = FieldState != EnumFieldState.Break;
        //            _newSwapField.swapToUseable = _newSwapField.FieldState != EnumFieldState.Break;
        //
        //            ChangeFieldState(EnumFieldState.Swap);
        //
        //            _newSwapField.ChangeFieldState(EnumFieldState.Swap);
        //        }
        //        else
        //        {
        //            ChangeFieldState(EnumFieldState.SwapBack);
        //
        //            _newSwapField.ChangeFieldState(EnumFieldState.SwapBack);
        //        }
        //    }
        //
        //    fieldUI.StartTransition();
        //    _newSwapField.fieldUI.StartTransition();
        //}

        public void MoveFieldHere(Field _movedField)
        {
            FieldUI lvSwapUI = fieldUI;

            _movedField.ChangeFieldState(EnumFieldState.Empty);

            ChangeFieldState(EnumFieldState.Move);

            FieldType = _movedField.FieldType;
            fieldUI = _movedField.fieldUI;
            
            if (_movedField.Is2x2)
                TurnTo2x2(); 

            _movedField.fieldUI = lvSwapUI;
            _movedField.fieldUI.gameObject.SetActive(false);
            _movedField.Is2x2 = false;

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
                    GM.boardMng.DoSwapFields(new List<Swap>() {new Swap(this, swapField) });
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

                WillBreakX = false;
                WillBreakY = false;
                breakEvent?.Invoke();

				if (Is2x2)
				{
                    

                    //TODO: what to do after break?

                    fieldUI.Undo_TurnToFrom2x2();
                }

                if (JokerAfterBreak)
                    BecomeJoker();
				else
				{
                    ChangeFieldState(EnumFieldState.Empty);
                    fieldUI.gameObject.SetActive(false);
				}
#if DEBUG
                fieldUI.fieldImage.color = Color.white;
#endif
            }
            else if (FieldState == EnumFieldState.Move)
                ChangeFieldState(EnumFieldState.Useable);
        }

		internal void TurnToNormalFrom2x2()
		{
            Is2x2 = false;

            TopRight2x2.fieldUI.Undo_TurnTo2x2Part();
            TopRight2x2.Break(0);

            BottomLeft2x2.fieldUI.Undo_TurnTo2x2Part();
            BottomLeft2x2.Break(0);

            BottomRight2x2.fieldUI.Undo_TurnTo2x2Part();
            BottomRight2x2.Break(0);

            TopRight2x2 = null;
            BottomLeft2x2 = null;
            BottomRight2x2 = null;
        }

        internal void TurnTo2x2()
		{
            Is2x2 = true;
            
            TopRight2x2 = GM.boardMng.Fields[rowIndex, columnIndex + 1];
            BottomLeft2x2 = GM.boardMng.Fields[rowIndex + 1, columnIndex];
            BottomRight2x2 = GM.boardMng.Fields[rowIndex + 1, columnIndex + 1];
        }

		private void BecomeJoker()
		{
            FieldType = FieldType.JOKER;
            fieldUI.Initialize(this);
            ChangeFieldState(EnumFieldState.Useable);
            fieldUI.gameObject.SetActive(true);
            JokerAfterBreak = false;
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
        JOKER = SPECIAL,
        DNS,
        TREASURE,

        PART_2x2,
    }
}