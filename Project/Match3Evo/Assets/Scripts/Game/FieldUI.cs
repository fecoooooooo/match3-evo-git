using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Match3_Evo
{
    public class FieldUI : MonoBehaviour
    {
        public RectTransform Rect { get { return rect; } }

        [SerializeField] GameObject fire;
        [SerializeField] public Image fieldImage;
        [SerializeField] Image shadowImage;
        [SerializeField] Image lockedImage;
        [SerializeField] Image debugImg;
        public TextMeshProUGUI debugText;
        public int matchOnSides = 0;
        
        public bool Locked { get; private set; }
        public bool Unbreakable { get; private set; }

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

            if (Field.breakEvent.GetPersistentEventCount() == 0)
                Field.breakEvent.AddListener(OnBreak);

			if (_field.SpecialType)
			{
				switch (Field.FieldType)
				{
					case FieldType.JOKER:
                        fieldImage.sprite = GM.boardMng.jokerSprite;
						break;
					case FieldType.DNS:
                        fieldImage.sprite = GM.boardMng.dnsSprite;
						break;
					case FieldType.TREASURE:
                        fieldImage.sprite = GM.boardMng.treasureSprite;
						break;
					default:
						break;
				}
            }
            else
			{
                fieldImage.sprite = GM.boardMng.GetFieldDataForFieldType(Field.FieldVariant, Field.EvoLvl).basic;
                shadowImage.sprite = GM.boardMng.GetFieldDataForFieldType(Field.FieldVariant, Field.EvoLvl).basic;
			}
        }

		private void OnBreak()
		{
            fire.SetActive(false);
		}

		void Update()
        {
            PositionTransition();
            UpdateUI();
            HandleFire();

            HandleDebug();
        }

		private void HandleDebug()
		{
            string debugTxt = "";

            if (GM.boardMng.DebugEnabled != debugText.IsActive())
                debugText.gameObject.SetActive(GM.boardMng.DebugEnabled);

            if (GM.boardMng.DebugState == 1)
                debugTxt = Field.FieldState.ToString()[0].ToString();
            else if(GM.boardMng.DebugState == 2)
                debugTxt += Field.CanFall ? "c" : "n";
            else if(GM.boardMng.DebugState == 3)
                debugTxt = Field.Is2x2 ? "2" : "1";
            else if(GM.boardMng.DebugState == 4)
                debugTxt = Field.FieldType.ToString().Substring(0, 2);

            if (GM.boardMng.DebugEnabled)
                debugText.text = debugTxt;
		}

		public void UpdateUI()
        {
			if (Field.Is2x2)
			{
                foreach (var f in GM.boardMng.Fields)
                    f.fieldUI.RemoveDebugMark();

                //Field.TopRight2x2.fieldUI.DebugMark();
                //Field.BottomLeft2x2.fieldUI.DebugMark();
                //Field.BottomRight2x2.fieldUI.DebugMark();
			}

            if (Field.SpecialType)
                return;

            animationTime += Time.deltaTime * GM.boardMng.fieldAnimationFPS;
            
            Sprite newImage;

            if(animationQueue.Count == 0 && UnityEngine.Random.Range(0f, 1000f) < GM.boardMng.animationProbability)
			{
                animationQueue.Add(GM.boardMng.GetFieldDataForFieldType(Field.FieldVariant, Field.EvoLvl).bubbleAnimation);
                animationTime = 0;
            }

            if (animationQueue.Count > 0)
            {
                if (animationTime >= animationQueue[0].Count)
                {
                    animationQueue.RemoveAt(0);
                    newImage = GM.boardMng.GetFieldDataForFieldType(Field.FieldVariant, Field.EvoLvl).basic;
                }
                else
                {
                    int repeatedTime = Mathf.FloorToInt(Mathf.Clamp(animationTime, 0f, animationQueue[0].Count));
                    if (animationQueue[0].Count > 0 && repeatedTime != 0 && repeatedTime < animationQueue[0].Count)
                        newImage = animationQueue[0][Mathf.FloorToInt(repeatedTime)];
                    else
                        newImage = GM.boardMng.GetFieldDataForFieldType(Field.FieldVariant, Field.EvoLvl).basic;
                }
            }
            else
            {
                newImage = GM.boardMng.GetFieldDataForFieldType(Field.FieldVariant, Field.EvoLvl).basic;
            }
         
            if (newImage != fieldImage.sprite)
                fieldImage.sprite = newImage;

        }
        
        private void HandleFire()
		{
			if (OnFire)
			{
                if (!fire.activeSelf)
                    fire.SetActive(true);

                fireTime -= Time.deltaTime;
                if (fireTime < 0)
                {
                    OnFire = false;
                }
            }
		}

		public void OnBeginDrag(BaseEventData _baseEventData)
        {
            if (Locked || false == GM.boardMng.InputEnabled)
                return;

            PointerEventData lvPointerEventData = _baseEventData as PointerEventData;
            Vector2 lvDelta = lvPointerEventData.position - lvPointerEventData.pressPosition;
            EnumSwapDirection lvSwapDirection = (EnumSwapDirection)Mathf.CeilToInt(Vector2.SignedAngle(Vector2.one, lvDelta) / 90f);

            if(Field.Is2x2)
                GM.boardMng.OnSwapFields2x2(Field, lvSwapDirection);
            else
                GM.boardMng.OnSwapFields(Field, lvSwapDirection);
        }

        public void DebugMark()
		{
            debugImg.gameObject.SetActive(true);
		}
        public void RemoveDebugMark()
        {
            debugImg.gameObject.SetActive(false);
        }

        public void OnClick()
        {
            if(Input.GetKey(KeyCode.LeftShift))
                GM.boardMng.TurnFieldTo2x2(Field.rowIndex, Field.columnIndex);
            if(Input.GetKey(KeyCode.LeftControl))
                DebugMakeType(FieldType.V1_E0);

            if (false == Field.SpecialType || Field.FieldType == FieldType.JOKER)
                return;

            GM.boardMng.HammerBreak(this);
        }

        void DebugMakeJoker()
		{
            Field.JokerAfterBreak = true;
            Field.Break(0);
        }

        void DebugMakeType(FieldType fieldType)
		{
            Field.FieldType = fieldType;
            Initialize(Field);
		}

        #region FieldMovement

        public void StartTransition()
        {
            if (Field.FieldState == EnumFieldState.Break)
			{
                Invoke(nameof(Break), Field.breakAfterSeconds);
            }
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
            rect.anchoredPosition3D = new Vector3(Field.fieldPosition.x, Field.fieldPosition.y, 0);
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
        }

        internal void SetLocked()
		{
            Locked = true;
            lockedImage.gameObject.SetActive(true);
		}

		internal void SetUnlockedIfNotUnbreakable()
		{
            if (Unbreakable)
                return;

            Locked = false;
            lockedImage.gameObject.SetActive(false);
        }

		internal void SetUnbreakableAndLocked()
		{
            Locked = true;
            Unbreakable = true;
            lockedImage.gameObject.SetActive(true);
        }

        public void TurnTo2x2()
		{
            Field.TurnTo2x2();
            rect.sizeDelta = Vector2.one * GM.boardMng.fieldSize * 2;
        }

        public void TurnToNormalFrom2x2()
        {
            Field.TurnToNormalFrom2x2();
            rect.sizeDelta = Vector2.one * GM.boardMng.fieldSize;
        }

        public void TurnTo2x2Part()
		{
            Field.FieldType = FieldType.PART_2x2;
            Field.CanFall = false;
            fieldImage.gameObject.SetActive(false);
            shadowImage.gameObject.SetActive(false);
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