using UnityEngine;

namespace Match3_Evo
{
    public class BreakBackground : MonoBehaviour
    {
        [SerializeField] GameObject left;
        [SerializeField] GameObject right;

        RectTransform rect;
        float delay = 0f;
        static GameObject hintBreakBackground = null;

        public void Initialize(RectTransform _parent, Mergeable _mergeable)
        {
            rect = GetComponent<RectTransform>();
            rect.SetParent(_parent, false);
            rect.anchoredPosition = _mergeable.TopLeftField.fieldPosition;
            rect.sizeDelta = _mergeable.breakUIWidth * GM.boardMng.fieldSize;

            if (_mergeable.mergeableType == EnumMergeableType.Hint)
            {
                delay = -1;
                hintBreakBackground = gameObject;
            }
            else if (_mergeable.mergeableType == EnumMergeableType.Three)
            {
                delay = GM.boardMng.breakDelayTimeFast;
            }
            else
            {
                delay = GM.boardMng.breakDelayTime;
            }

            left.SetActive(_mergeable.mergeableType != EnumMergeableType.Hint);
            right.SetActive(_mergeable.mergeableType != EnumMergeableType.Hint);
            gameObject.SetActive(true);
        }

        void OnEnable()
        {
            if (delay > 0f)
                Invoke(nameof(Disable), delay);
        }

        private void Disable()
        {
            gameObject.SetActive(false);
        }

        public static void HideHintBreakBackground()
        {
            if (hintBreakBackground != null)
            {
                hintBreakBackground.SetActive(false);
                hintBreakBackground = null;
            }
        }
    }
}