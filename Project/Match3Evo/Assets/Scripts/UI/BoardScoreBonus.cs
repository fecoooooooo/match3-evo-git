using UnityEngine;
using UnityEngine.UI;

namespace Match3_Evo
{
    public class BoardScoreBonus : MonoBehaviour
    {
        [SerializeField] string scoreBonusTextTemplate;
        [SerializeField] float tween;

        [SerializeField] Text blockCountText;
        [SerializeField] string[] blockCountTextList;

        int scoreBonus;
        Vector3 fromPosition;
        Vector3 toPosition;
        RectTransform rectTransform;

        public void Init(int _scoreBonus, RectTransform _fieldUI, RectTransform _parent, RectTransform _target, int _size)
        {
            rectTransform = GetComponent<RectTransform>();
            scoreBonus = _scoreBonus;
            GetComponent<Text>().text = scoreBonusTextTemplate.Replace("<SCORE>", _scoreBonus.ToString());
            blockCountText.text = blockCountTextList[_size];

            Vector2 lvRectSize = rectTransform.sizeDelta;
            Vector2 lvFieldUIOffset = _fieldUI.sizeDelta;

            lvFieldUIOffset.Scale(new Vector2(0.5f, -0.5f));
            
            Vector2 lvFieldUIPos = _fieldUI.anchoredPosition + lvFieldUIOffset;

            rectTransform.anchoredPosition = lvFieldUIPos;
            rectTransform.SetParent(_parent, true);

            Vector2 lvPosition = rectTransform.anchoredPosition;
            float lvParentWidth = rectTransform.parent.GetComponent<RectTransform>().sizeDelta.x;
            
            lvPosition.x = Mathf.Clamp(lvPosition.x, lvRectSize.x / 2f, lvParentWidth - lvRectSize.x / 2f);
            rectTransform.anchoredPosition = lvPosition;
            fromPosition = rectTransform.position;
            toPosition = _target.position;
        }

        public void Update()
        {
            transform.position = Vector3.Lerp(fromPosition, toPosition, tween);
        }

        public void Destory()
        {
            Destroy(gameObject);
        }
    }
}