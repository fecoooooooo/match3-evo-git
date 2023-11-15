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
        public int fieldVariant;
        public float breakAfterSeconds;
        public Vector2 fieldPosition;
        public FieldUI fieldUI;

        [HideInInspector] public Field Left = null;
        [HideInInspector] public Field Right = null;
        [HideInInspector] public Field Top = null;
        [HideInInspector] public Field Bottom = null;

        private Field swapField;
        private bool swapDone = false;
        private bool swapToUseable = false;

		public Field(int _rowIndex, int _columnIndex, int _fieldVariant, int _score, Vector2 _fieldPosition, FieldUI _fieldUI)
        {
            rowIndex = _rowIndex;
            columnIndex = _columnIndex;
            fieldVariant = _fieldVariant;
            fieldPosition = _fieldPosition;
            fieldUI = _fieldUI;
        }

        public Field(Field _field)
        {
            fieldVariant = _field.fieldVariant;
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

        public int GameScore()
        {
            //We return only the simple because the max srore merge bonus is handleld separatly
            return GM.boardMng.gameParameters.tileScore;
        }

        public override string ToString()
        {
            return string.Format("[Field: rowIndex={0}, columnIndex={1}, color={2}, state={3}, swapDone={4}, fieldUI={5}, fieldPosition={6}]", rowIndex, columnIndex, fieldVariant, FieldState, swapDone, fieldUI.Rect.anchoredPosition, fieldPosition);
        }

        public void SwapWithField(Field _newSwapField)
        {
            GM.soundMng.Play(EnumSoundID.Swap);

            Field lvTemp = new Field(_newSwapField);

            _newSwapField.fieldVariant = fieldVariant;
            _newSwapField.fieldUI = fieldUI;

            fieldVariant = lvTemp.fieldVariant;
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

            fieldVariant = _movedField.fieldVariant;
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

            fieldVariant = _randomSwapField.fieldVariant;
            fieldUI = _randomSwapField.fieldUI;

            _randomSwapField.fieldVariant = lvSwapTmp.fieldVariant;
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
                GM.scoreMng.AddTileBreak();
                
                GM.boardMng.collectedTileRoot.GetChild(fieldVariant).GetComponent<CollectedTile>().AddToCounter(1);

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

            if (fieldVariant == _fieldType)
                lvSimilarCount++;

            if (Left != null && Left.fieldVariant == _fieldType)
                lvSimilarCount++;

            if (Right != null && Right.fieldVariant == _fieldType)
                lvSimilarCount++;

            if (Top != null && Top.fieldVariant == _fieldType)
                lvSimilarCount++;

            if (Bottom != null && Bottom.fieldVariant == _fieldType)
                lvSimilarCount++;

            return lvSimilarCount;
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
}