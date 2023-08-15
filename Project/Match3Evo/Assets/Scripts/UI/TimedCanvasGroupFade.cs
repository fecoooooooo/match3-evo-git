using UnityEngine;

public class TimedCanvasGroupFade : CanvasGroupFade
{
    [SerializeField] float fadeInDelay = 0f;
    [SerializeField] float fadeInTime = 1f;

    private void Start()
    {
        Fade(0f);
        Invoke(nameof(FadeIn), fadeInDelay);
    }

    private void FadeIn()
    {
        if (fadeInTime == 0f)
            Fade(1f);
        else
            Fade(1f, fadeInTime);
    }
}
