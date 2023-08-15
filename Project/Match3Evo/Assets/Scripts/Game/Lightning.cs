using UnityEngine;

namespace Match3_Evo
{
    public class Lightning : MonoBehaviour
    {
        public Color startColor;
        public Color endColor;
        public LineRenderer lineRenderer;
        public RectTransform startPosition;
        public RectTransform endPosition;
        public Vector2 endPointDirection;

        public void Initialize(RectTransform _parent, Mergeable _mergeable)
        {
            GM.soundMng.Play(EnumSoundID.LineLightning);

            if (_parent != null)
                GetComponent<RectTransform>().SetParent(_parent, false);

            if (_mergeable.mergeableType == EnumMergeableType.Box)
            {
                Vector2 lvReposition = endPointDirection;
                if (_mergeable.BoxField.rowIndex == 0 && endPointDirection.y > 0f ||
                    _mergeable.BoxField.rowIndex == GM.boardMng.rows - 1 && endPointDirection.y < 0f ||
                    _mergeable.BoxField.columnIndex == 0 && endPointDirection.x < 0f ||
                    _mergeable.BoxField.columnIndex == GM.boardMng.columns - 1 && endPointDirection.x > 0f)
                {
                    gameObject.SetActive(false);
                    return;
                }

                if (_mergeable.BoxField.rowIndex == 1 && endPointDirection.y > 0f)
                    lvReposition.y--;
                else if (_mergeable.BoxField.rowIndex == GM.boardMng.rows - 2 && endPointDirection.y < 0f)
                    lvReposition.y++;

                if (_mergeable.BoxField.columnIndex == 1 && endPointDirection.x < 0f)
                    lvReposition.x++;
                else if (_mergeable.BoxField.columnIndex == GM.boardMng.columns - 2 && endPointDirection.x > 0f)
                    lvReposition.x--;

                endPosition.anchoredPosition = lvReposition * GM.boardMng.fieldSize;
                gameObject.SetActive(true);
            }
            
            if (_mergeable.mergeableType == EnumMergeableType.Line)
            {
                if (_mergeable.isRow)
                {
                    startPosition.anchoredPosition = _mergeable.TopLeftField.fieldPosition + Vector2.down * (GM.boardMng.fieldSize * 0.5f);
                    endPosition.anchoredPosition = _mergeable.LastField.fieldPosition + new Vector2(GM.boardMng.fieldSize, GM.boardMng.fieldSize * -0.5f);
                    gameObject.SetActive(true);
                }
                else
                {
                    startPosition.anchoredPosition = _mergeable.TopLeftField.fieldPosition + Vector2.right * (GM.boardMng.fieldSize * 0.5f);
                    endPosition.anchoredPosition = _mergeable.LastField.fieldPosition + new Vector2(GM.boardMng.fieldSize * 0.5f, -GM.boardMng.fieldSize);
                    gameObject.SetActive(true);
                }
            }
        }

        private void Update()
        {
            lineRenderer.startColor = startColor;
            lineRenderer.endColor = endColor;
        }

        public void Disable()
        {
            lineRenderer.positionCount = 0;
            gameObject.SetActive(false);
        }
    }
}