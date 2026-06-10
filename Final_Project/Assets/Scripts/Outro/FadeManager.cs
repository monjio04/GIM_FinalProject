using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    public Image fadeImage;

    public float fadeDuration = 2f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        Color color = fadeImage.color;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            color.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            fadeImage.color = color;

            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;

        fadeImage.gameObject.SetActive(false);
    }

    public void FadeOut(Action onComplete = null)
    {
        StartCoroutine(FadeOutRoutine(onComplete));
    }

    IEnumerator FadeOutRoutine(Action onComplete)
    {
        fadeImage.gameObject.SetActive(true);

        Color color = fadeImage.color;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            color.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);

            fadeImage.color = color;

            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;

        onComplete?.Invoke();
    }
}