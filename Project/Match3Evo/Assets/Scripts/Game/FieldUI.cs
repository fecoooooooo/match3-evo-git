using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Match3_Evo
{
    public class FieldUI : MonoBehaviour
    {
        public RectTransform Rect { get { return rect; } }

        [SerializeField] public Image fieldImage;
        [SerializeField] Image shadowImage;
        public Text debugText;
        public int matchOnSides = 0;
        
        public bool OnFire { get; private set; }
        float fireTime;

        public Field Field { get; private set; }
        RectTransform rect;

        bool updateMovement = false;
        float movementAcceleration = 0f;
        Vector2 movementDirection = Vector2.zero;
        bool bounceMovement;
        float bounceTime;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
            rect.sizeDelta = Vector2.one * GM.boardMng.fieldSize;
            debugText.text = "";
        }

        public void Initialize(Field _field)
        {
            Field = _field;
            fieldImage.sprite = GM.boardMng.FieldDatas[_field.fieldVariant].basic;
            shadowImage.sprite = GM.boardMng.FieldDatas[_field.fieldVariant].basic;
        }

        void Update()
        {
            PositionTransition();
            HandleFire();
        }

		private void HandleFire()
		{
			if (OnFire)
			{
                fireTime -= Time.deltaTime;
                if (fireTime < 0)
                {
                    OnFire = false;
#if DEBUG
                    fieldImage.color = Color.gray;
#endif
                }
            }
		}

		public void OnBeginDrag(BaseEventData _baseEventData)
        {
            PointerEventData lvPointerEventData = _baseEventData as PointerEventData;
            Vector2 lvDelta = lvPointerEventData.position - lvPointerEventData.pressPosition;
            EnumSwapDirection lvSwapDirection = (EnumSwapDirection)Mathf.CeilToInt(Vector2.SignedAngle(Vector2.one, lvDelta) / 90f);
            GM.boardMng.OnSwapFields(Field, lvSwapDirection);
        }

        #region FieldMovement

        public void StartTransition()
        {
            if (Field.FieldState == EnumFieldState.Break)
                Invoke(nameof(Break), Field.breakAfterSeconds);
            else
            {
                updateMovement = true;
                movementAcceleration = 0f;
                movementDirection = (rect.anchoredPosition - Field.fieldPosition);
                movementDirection.Normalize();
                bounceMovement = false;
            }
        }

        public void ResetPosition()
        {
            updateMovement = false;
            rect.anchoredPosition = Field.fieldPosition;
        }

        public void ResetPositionToRefil(int _positionIndex)
        {
            Vector2 lvPosition = Field.fieldPosition;
            lvPosition.y = GM.boardMng.fieldSize * _positionIndex;
            rect.anchoredPosition = lvPosition;
            StartTransition();
        }

        void PositionTransition()
        {
            if (updateMovement)
            {
                if (!bounceMovement)
                {
                    if (movementAcceleration < GM.boardMng.transitionMaxSpeed)
                    {
                        movementAcceleration += GM.boardMng.transitionSpeed * Time.deltaTime;
                        movementAcceleration = Mathf.Clamp(movementAcceleration, 0f, GM.boardMng.transitionMaxSpeed);
                    }

                    Vector2 lvPosition = rect.anchoredPosition;
                    lvPosition = Vector2.MoveTowards(lvPosition, Field.fieldPosition, movementAcceleration);
                    rect.anchoredPosition = lvPosition;
                    if ((lvPosition - Field.fieldPosition).sqrMagnitude < 0.1f)
                    {
                        rect.anchoredPosition = Field.fieldPosition;
                        bounceMovement = true;
                        bounceTime = GM.boardMng.fieldBounceCurve[GM.boardMng.fieldBounceCurve.length - 1].time;

                        if (Field.FieldState == EnumFieldState.Move)
                            GM.soundMng.Play(EnumSoundID.TileArrive);
                    }
                }
                else
                {
                    bounceTime -= Time.deltaTime;
                    bounceTime = bounceTime < 0 ? 0 : bounceTime;

                    rect.anchoredPosition = Field.fieldPosition + movementDirection * GM.boardMng.fieldBounceCurve.Evaluate(bounceTime);

                    if (bounceTime == 0)
                    {
                        rect.anchoredPosition = Field.fieldPosition;
                        updateMovement = false;
                        Field.EndTransition();
                    }
                }
            }
        }

        private void Break()
        {
            Field.EndTransition();
        }
        #endregion

        internal void SetOnFire()
        {
            OnFire = true;
            fireTime = GM.boardMng.gameParameters.fireTime;
#if DEBUG
            fieldImage.color = Color.red;
#endif
        }
    }
}

public enum EnumSwapDirection
{
    Down = -1,
    Right = 0,
    Up = 1,
    Left = 2
}