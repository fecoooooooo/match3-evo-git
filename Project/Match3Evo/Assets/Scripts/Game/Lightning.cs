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

        public void Initialize(RectTransform _parent, bool isRow, int index)
        {
            GM.soundMng.Play(EnumSoundID.LineLightning);

            if (_parent != null)
                GetComponent<RectTransform>().SetParent(_parent, false);

			if (isRow)
			{
                startPosition.anchoredPosition = GM.boardMng.Fields[index, 0].fieldPosition + Vector2.down * (GM.boardMng.fieldSize * 0.5f);
                endPosition.anchoredPosition = GM.boardMng.Fields[index, GM.boardMng.columns - 1].fieldPosition + new Vector2(GM.boardMng.fieldSize, GM.boardMng.fieldSize * -0.5f);
                gameObject.SetActive(true);
            }
            else 
            {
                int rowEndIndex = GM.boardMng.rows- 1;
                while (GM.boardMng.Fields[rowEndIndex, index].fieldUI.Locked)
                    rowEndIndex--;

                startPosition.anchoredPosition = GM.boardMng.Fields[0, index].fieldPosition + Vector2.right * (GM.boardMng.fieldSize * 0.5f);
                endPosition.anchoredPosition = GM.boardMng.Fields[rowEndIndex, index].fieldPosition + new Vector2(GM.boardMng.fieldSize * 0.5f, -GM.boardMng.fieldSize);
                gameObject.SetActive(true);
            }

            //if (_mergeable.isRow)
            //{
            //    startPosition.anchoredPosition = _mergeable.TopLeftField.fieldPosition + Vector2.down * (GM.boardMng.fieldSize * 0.5f);
            //    endPosition.anchoredPosition = _mergeable.LastField.fieldPosition + new Vector2(GM.boardMng.fieldSize, GM.boardMng.fieldSize * -0.5f);
            //    gameObject.SetActive(true);
            //}
            //else
            //{
            //    startPosition.anchoredPosition = _mergeable.TopLeftField.fieldPosition + Vector2.right * (GM.boardMng.fieldSize * 0.5f);
            //    endPosition.anchoredPosition = _mergeable.LastField.fieldPosition + new Vector2(GM.boardMng.fieldSize * 0.5f, -GM.boardMng.fieldSize);
            //    gameObject.SetActive(true);
            //}
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