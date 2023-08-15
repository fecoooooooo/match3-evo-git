using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Match3_Evo
{
    public class FieldUI : MonoBehaviour
    {
        public RectTransform Rect { get { return rect; } }

        [SerializeField] Image fieldImage;
        [SerializeField] Image shadowImage;
        public Text debugText;
        public int matchOnSides = 0;

        Field field;
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
            field = _field;
            fieldImage.sprite = GM.boardMng.FieldDatas[_field.fieldVariant].basic;
            shadowImage.sprite = GM.boardMng.FieldDatas[_field.fieldVariant].basic;
        }

        void Update()
        {
            PositionTransition();            
        }

        public void OnBeginDrag(BaseEventData _baseEventData)
        {
            PointerEventData lvPointerEventData = _baseEventData as PointerEventData;
            Vector2 lvDelta = lvPointerEventData.position - lvPointerEventData.pressPosition;
            EnumSwapDirection lvSwapDirection = (EnumSwapDirection)Mathf.CeilToInt(Vector2.SignedAngle(Vector2.one, lvDelta) / 90f);
            GM.boardMng.OnSwapFields(field, lvSwapDirection);
        }

        #region FieldMovement

        public void StartTransition()
        {
            if (field.FieldState == EnumFieldState.Break)
                Invoke(nameof(Break), field.breakAfterSeconds);
            else
            {
                updateMovement = true;
                movementAcceleration = 0f;
                movementDirection = (rect.anchoredPosition - field.fieldPosition);
                movementDirection.Normalize();
                bounceMovement = false;
            }
        }

        public void ResetPosition()
        {
            updateMovement = false;
            rect.anchoredPosition = field.fieldPosition;
        }

        public void ResetPositionToRefil(int _positionIndex)
        {
            Vector2 lvPosition = field.fieldPosition;
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
                    lvPosition = Vector2.MoveTowards(lvPosition, field.fieldPosition, movementAcceleration);
                    rect.anchoredPosition = lvPosition;
                    if ((lvPosition - field.fieldPosition).sqrMagnitude < 0.1f)
                    {
                        rect.anchoredPosition = field.fieldPosition;
                        bounceMovement = true;
                        bounceTime = GM.boardMng.fieldBounceCurve[GM.boardMng.fieldBounceCurve.length - 1].time;

                        if (field.FieldState == EnumFieldState.Move)
                            GM.soundMng.Play(EnumSoundID.TileArrive);
                    }
                }
                else
                {
                    bounceTime -= Time.deltaTime;
                    bounceTime = bounceTime < 0 ? 0 : bounceTime;

                    rect.anchoredPosition = field.fieldPosition + movementDirection * GM.boardMng.fieldBounceCurve.Evaluate(bounceTime);

                    if (bounceTime == 0)
                    {
                        rect.anchoredPosition = field.fieldPosition;
                        updateMovement = false;
                        field.EndTransition();
                    }
                }
            }
        }

        private void Break()
        {
            field.EndTransition();
        }
        #endregion
    }
}

public enum EnumSwapDirection
{
    Down = -1,
    Right = 0,
    Up = 1,
    Left = 2
}