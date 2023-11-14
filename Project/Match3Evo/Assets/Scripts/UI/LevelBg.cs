using Match3_Evo;
using UnityEngine;

public class LevelBg : MonoBehaviour
{
	public RectTransform rectTransform;
    int heightMultiplicationComparedToWidth = 8;

	internal void SetSize()
    {
        rectTransform.sizeDelta = new Vector2(
            GM.boardMng.fieldSize * GM.boardMng.rows, 
            GM.boardMng.fieldSize * GM.boardMng.columns * heightMultiplicationComparedToWidth);
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -rectTransform.sizeDelta.y / 2);
    }

    internal void ShiftToY()
	{
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y - GM.boardMng.fieldSize);
    }

}
