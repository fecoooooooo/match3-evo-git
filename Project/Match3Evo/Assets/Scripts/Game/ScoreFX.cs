using UnityEngine;
using UnityEngine.UI;

namespace Match3_Evo
{
    public class ScoreFX : MonoBehaviour
    {
        public float transitionAnimation;
        public float transitionDistance = 200f;
        Vector2 startPosition;
        Vector2 endPosition;
        // static bool endPositionCalculated = false;
        RectTransform rect;
        [SerializeField]Text text;

        public static void Create(Mergeable _mergeable)
        {
            //if (!endPositionCalculated)
            //{
            //    endPositionCalculated = true;
            //    RectTransformUtility.ScreenPointToLocalPointInRectangle(GM.boardMng.TopFXParent, (Vector2)GM.boardMng.gameCamera.WorldToScreenPoint(GM.boardMng.scoreFXEndPosition.position), GM.boardMng.gameCamera, out endPosition);
            //}
            Instantiate(GM.boardMng.scoreFXPrefab, GM.boardMng.topFXParent, false).Setup(_mergeable.GetScoreFXPosition(), _mergeable.GetScoreBonus());
        }

        public static void CreateForTreasure(Field treasureField)
        {
            Instantiate(GM.boardMng.scoreFXPrefab, GM.boardMng.topFXParent, false).Setup(treasureField.fieldUI.Field.fieldPosition, GM.boardMng.treasureScore);
        }

        public void Setup(Vector2 _anchoredStartPosition, int _score)
        {
            text.text = GM.scoreMng.FormatScore(_score);
            rect = GetComponent<RectTransform>();
            rect.anchoredPosition = _anchoredStartPosition;
            startPosition = rect.localPosition;
            endPosition = startPosition + Vector2.up * transitionDistance;
        }

        private void Update()
        {
            rect.localPosition = Vector2.Lerp(startPosition, endPosition, transitionAnimation);

            if (transitionAnimation == 1)
                Destroy(gameObject);
        }
    }
}