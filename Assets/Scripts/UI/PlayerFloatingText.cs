using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFloatingText : MonoBehaviour
{
    [Header("UI References")]
    public Transform canvas;        
    public Image backgroundImage;         
    public TextMeshProUGUI floatingText;  

    [Header("Settings")]
    public float displayTime = 2f;
    public float fadeDuration = 1f;

    private Coroutine fadeCoroutine;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (canvas != null)
        {
            canvas.gameObject.SetActive(false);
        }
    }

    void LateUpdate()
    {
        if (mainCamera != null && canvas != null && canvas.gameObject.activeInHierarchy)
        {
            canvas.forward = mainCamera.transform.forward;
        }
    }

    public void ShowText(string message)
    {
        if (canvas == null || backgroundImage == null || floatingText == null) return;

        canvas.gameObject.SetActive(true);
        floatingText.text = message;

        SetAlpha(1f);

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeOutRoutine());
    }

    public void HideText()
    {
        if (canvas == null) return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        canvas.gameObject.SetActive(false);
    }

    private IEnumerator FadeOutRoutine()
    {
        yield return new WaitForSecondsRealtime(displayTime);

        float currentTime = 0f;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.unscaledDeltaTime;
            float currentAlpha = Mathf.Lerp(1f, 0f, currentTime / fadeDuration);

            SetAlpha(currentAlpha);

            yield return null;
        }

        canvas.gameObject.SetActive(false);
    }

    private void SetAlpha(float alpha)
    {
        if (backgroundImage != null)
        {
            Color bgColor = backgroundImage.color;
            bgColor.a = alpha;
            backgroundImage.color = bgColor;
        }

        if (floatingText != null)
        {
            Color textColor = floatingText.color;
            textColor.a = alpha;
            floatingText.color = textColor;
        }
    }
}