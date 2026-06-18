using UnityEngine;

public class MainMenuAnimation : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform backgroundImage;
    public RectTransform titleText;

    [Header("Background Drift Settings")]
    public float bgDriftSpeed = 0.5f;
    public float bgDriftAmountX = 15f;
    public float bgDriftAmountY = 8f;

    [Header("Title Float Settings")]
    public float titleFloatSpeed = 2f;
    public float titleFloatHeight = 10f;

    private Vector3 bgStartPos;
    private Vector3 titleStartPos;

    void Start()
    {
        if (backgroundImage != null)
        {
            bgStartPos = backgroundImage.localPosition;
            backgroundImage.localScale = new Vector3(1.05f, 1.05f, 1f);
        }

        if (titleText != null)
        {
            titleStartPos = titleText.localPosition;
        }
    }

    void Update()
    {
        if (backgroundImage != null)
        {
            float newBgX = bgStartPos.x + (Mathf.Sin(Time.time * bgDriftSpeed) * bgDriftAmountX);
            float newBgY = bgStartPos.y + (Mathf.Cos(Time.time * bgDriftSpeed * 0.8f) * bgDriftAmountY);
            backgroundImage.localPosition = new Vector3(newBgX, newBgY, bgStartPos.z);
        }

        if (titleText != null)
        {
            float newTitleY = titleStartPos.y + (Mathf.Sin(Time.time * titleFloatSpeed) * titleFloatHeight);
            titleText.localPosition = new Vector3(titleStartPos.x, newTitleY, titleStartPos.z);
        }
    }
}