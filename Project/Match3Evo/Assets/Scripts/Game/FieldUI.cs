using System;
using System.Collections.Generic;
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
        [SerializeField] Image lockedImage;
        public Text debugText;
        public int matchOnSides = 0;
        
        public bool Locked { get; private set; }

        public bool OnFire { get; private set; }
        float fireTime;

        public Field Field { get; private set; }
        RectTransform rect;

        bool updateMovement = false;
        float movementAcceleration = 0f;
        Vector2 movementDirection = Vector2.zero;
        bool bounceMovement;
        float bounceTime;

        List<List<Sprite>> animationQueue = new List<List<Sprite>>();
        float animationTime = 0f;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
            rect.sizeDelta = Vector2.one * GM.boardMng.fieldSize;
            debugText.text = "";
        }

        public void Initialize(Field _field)
        {
            animationQueue.Clear();
            animationTime = 0;

            Field = _field;
            fieldImage.sprite = GM.boardMng.FieldDatas[_field.fieldVariant].basic;
            shadowImage.sprite = GM.boardMng.FieldDatas[_field.fieldVariant].basic;
        }

        void Update()
        {
            PositionTransition();
            UpdateUI();
            HandleFire();
        }

        public void UpdateUI()
        {
            animationTime += Time.deltaTime * GM.boardMng.fieldAnimationFPS;
            
            Sprite newImage;

            if (animationQueue.Count > 0)
            {
                if (animationTime >= animationQueue[0].Count)
                {
                    animationQueue.RemoveAt(0);
                    newImage = GM.boardMng.FieldDatas[Field.fieldVariant].basic;
                }
                else
                {
                    int repeatedTime = Mathf.FloorToInt(Mathf.Clamp(animationTime, 0f, animationQueue[0].Count));
                    if (animationQueue[0].Count > 0 && repeatedTime != 0 && repeatedTime < animationQueue[0].Count)
                        newImage = animationQueue[0][Mathf.FloorToInt(repeatedTime)];
                    else
                        newImage = GM.boardMng.FieldDatas[Field.fieldVariant].basic;
                }
            }
            else
            {
                newImage = GM.boardMng.FieldDatas[Field.fieldVariant].basic;
            }
         
            if (newImage != fieldImage.sprite)
                fieldImage.sprite = newImage;
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
            if (Locked)
                return;

            PointerEventData lvPointerEventData = _baseEventData as PointerEventData;
            Vector2 lvDelta = lvPointerEventData.position - lvPointerEventData.pressPosition;
            EnumSwapDirection lvSwapDirection = (EnumSwapDirection)Mathf.CeilToInt(Vector2.SignedAngle(Vector2.one, lvDelta) / 90f);
            GM.boardMng.OnSwapFields(Field, lvSwapDirection);
        }

        #region FieldMovement

        public void StartTransition()
        {
            if (Field.FieldState == EnumFieldState.Break)
			{
                Invoke(nameof(Break), Field.breakAfterSeconds);
                animationQueue.Add(GM.boardMng.FieldDatas[Field.fieldVariant].bubbleAnimation);
                animationQueue.Add(GM.boardMng.FieldDatas[Field.fieldVariant].bubbleAnimation);
            }
            else
            {
                updateMovement = true;
                movementAcceleration = 0f;
                movementDirection = (rect.anchoredPosition - Field.fieldPosition);
                movementDirection.Normalize();
                bounceMovement = false;
                animationTime = 0;

                if (Field.FieldState == EnumFieldState.Move || Field.FieldState == EnumFieldState.SwapBack)
                {
                    animationQueue.Add(GM.boardMng.FieldDatas[Field.fieldVariant].bubbleAnimation);
                    animationQueue.Add(GM.boardMng.FieldDatas[Field.fieldVariant].bubbleAnimation);
                }
                else if (Field.FieldState == EnumFieldState.Swap)
                {
                    animationQueue.Insert(0, GM.boardMng.FieldDatas[Field.fieldVariant].bubbleAnimation);
                }
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

        internal void SetLocked()
		{
            Locked = true;
            lockedImage.gameObject.SetActive(true);
		}

		internal void SetUnlocked()
		{
            Locked = false;
            lockedImage.gameObject.SetActive(false);
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