using System.Collections;
using UnityEngine;

public class FadeEffect : MonoBehaviour
{
    [Header("Fade On")]
    [SerializeField]
    private bool _fadeIn = true;
    [SerializeField]
    private bool _fadeOut = true;

    [Header("Fade Time")]
    [SerializeField]
    private float _fadeTime = 4f;
    [SerializeField]
    private float _delayTime = 4f;

    [Header("Alpha Value")]
    [SerializeField]
    private float _maxAlpha = 1f;
    [SerializeField]
    private float _minAlpha = 0f;

    private CanvasGroup target = null;

    void Awake()
    {
        target = GetComponent<CanvasGroup>();

        if (_fadeIn == true)
        {
            target.alpha = 0;
        }
        else
        {
            target.alpha = 1;
        }
    }

    void Start()
    {
        if (_fadeIn == true)
        {
            StartCoroutine("FadeIn");
        }
        else
        {
            StartCoroutine("FadeOut");
        }
    }

    private IEnumerator FadeIn()
    {
        float currentTime = 0.0f;
        float alpha = 0.0f;

        while (alpha < 1f)
        {
            currentTime += Time.deltaTime;
            alpha = currentTime / _fadeTime;

            target.alpha = Mathf.Lerp(_minAlpha, _maxAlpha, alpha);

            yield return null;
        }

        StartCoroutine("FadeInToOut");
    }

    private IEnumerator FadeInToOut()
    {
        yield return new WaitForSeconds(_delayTime);
        StopCoroutine("FadeIn");

        if (_fadeOut == true)
        {
            StartCoroutine("FadeOut");
        }
    }

    private IEnumerator FadeOut()
    {
        float currentTime = 0.0f;
        float alpha = 0.0f;

        while (alpha < 1f)
        {
            currentTime += Time.deltaTime;
            alpha = currentTime / _fadeTime;

            target.alpha = Mathf.Lerp(_maxAlpha, _minAlpha, alpha);

            yield return null;
        }

        StartCoroutine("FadeOutToDestroy");
    }

    private IEnumerator FadeOutToDestroy()
    {
        yield return null;
        StopCoroutine("FadeOut");
        Destroy(this.gameObject);
    }

    public void Stop()
    {
        this.StopAllCoroutines();
        Destroy(this.gameObject);
    }
}
