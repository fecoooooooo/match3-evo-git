using UnityEngine;

public class CanvasGroupFade : MonoBehaviour {
    public CanvasGroup canvasGroup;
    public bool disableGOWhenTransparent = false;
    float targetAlpha = 0f;
    float alphaDelta = 0f;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Fade(float _alpha, float _alphaDelta = float.MaxValue)
    {
        if (_alphaDelta == float.MaxValue)
        {
            canvasGroup.alpha = _alpha;

            if (disableGOWhenTransparent && _alpha == 0f)
                canvasGroup.gameObject.SetActive(false);
        }
        else
        {
            enabled = true;
            targetAlpha = _alpha;
            alphaDelta = _alphaDelta;

            if (disableGOWhenTransparent && _alpha > 0f)
                canvasGroup.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        float lvAlpha = canvasGroup.alpha;

        if (lvAlpha != targetAlpha)
        {
            if (targetAlpha - lvAlpha > 0)
            {
                lvAlpha += alphaDelta * Time.deltaTime;

                if (lvAlpha > targetAlpha)
                    lvAlpha = targetAlpha;
            }
            else
            {
                lvAlpha -= alphaDelta * Time.deltaTime;

                if (lvAlpha < targetAlpha)
                    lvAlpha = targetAlpha;
            }

            canvasGroup.alpha = lvAlpha;
        }
        else
        {
            enabled = false;

            if (disableGOWhenTransparent && lvAlpha == 0f)
                canvasGroup.gameObject.SetActive(false);
        }
    }
}
