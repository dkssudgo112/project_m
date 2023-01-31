using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FadeEffect : MonoBehaviour
{
    [Header("Fade On")]
    public bool _fadeIn = true;
    public bool _fadeOut = true;

    [Header("Fade Time")]
    public float _fadeTime = 4f;
    public float _delayTime = 4f;

    [Header("Alpha Value")]
    public float _imageMaxAlpha = 0.5f;
    public float _textMaxAlpha = 1f;
    public float _imageMinAlpha = 0f;
    public float _textMinAlpha = 0f;

    private TMP_Text text = null;
    private Image image = null;

    void Awake()
    {
        GameObject obj = this.gameObject;

        image = obj.GetComponent<Image>();
        if (image == null)
        {
            text = obj.GetComponent<TMP_Text>();
            text.color -= new Color(0, 0, 0, text.color.a);
}
        else if (obj.transform.childCount > 0)
        {
            image.color -= new Color(0, 0, 0, image.color.a);
            text = obj.transform.GetChild(0).GetComponent<TMP_Text>();
        }
        else
        {
            image.color -= new Color(0, 0, 0, image.color.a);
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

            if (image != null)
            {
                Color imgColor = image.color;
                imgColor.a = Mathf.Lerp(_imageMinAlpha, _imageMaxAlpha, alpha);
                image.color = imgColor;
            }
            
            if (text != null)
            {
                Color txtColor = text.color;
                txtColor.a = Mathf.Lerp(_textMinAlpha, _textMaxAlpha, alpha);
                text.color = txtColor;
            }            

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

            if (image != null)
            {
                Color imgColor = image.color;
                imgColor.a = Mathf.Lerp(_imageMaxAlpha, _imageMinAlpha, alpha);
                image.color = imgColor;
            }

            if (text != null)
            {
                Color txtColor = text.color;
                txtColor.a = Mathf.Lerp(_textMaxAlpha, _textMinAlpha, alpha);
                text.color = txtColor;
            }

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
