using System;
using System.Collections;
using UnityEngine;

public class LoadingCurtain : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeTime = 0.5f;


    public void Show(Action onComplete = null)
    {
        gameObject.SetActive(true);
        _canvasGroup.alpha = 0;
        StartCoroutine(FadeIn(onComplete));
    }

    public void Hide(Action onComplete = null)
    {
        StartCoroutine(FadeOut(onComplete));
    }

    private IEnumerator FadeIn(Action onComplete)
    {
        _canvasGroup.blocksRaycasts = true;
        float time = 0;
        while (time < _fadeTime)
        {
            _canvasGroup.alpha = Mathf.Lerp(0, 1, time / _fadeTime);
            time += Time.deltaTime;
            yield return null;
        }
        _canvasGroup.alpha = 1;
        _canvasGroup.blocksRaycasts = true;
        onComplete?.Invoke();
    }

    private IEnumerator FadeOut(Action onComplete)
    {
        _canvasGroup.blocksRaycasts = false;
        float time = 0;
        while (time < _fadeTime)
        {
            _canvasGroup.alpha = Mathf.Lerp(1, 0, time / _fadeTime);
            time += Time.deltaTime;
            yield return null;
        }
        _canvasGroup.alpha = 0;
        gameObject.SetActive(false);
        onComplete?.Invoke();
    }
}