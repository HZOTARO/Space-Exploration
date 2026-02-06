using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_InputField))]
public class CodeEditor : MonoBehaviour
{
    [Header("References")]
    public TMP_InputField inputField;
    public RectTransform viewport;

    [Header("Layout")]
    public float padding = 20f;

    RectTransform contentRect;

    void Awake()
    {
        if (!inputField)
            inputField = GetComponentInChildren<TMP_InputField>();

        if (!viewport)
            viewport = GetComponentInChildren<ScrollRect>().viewport;

        contentRect = inputField.GetComponent<RectTransform>();

        inputField.onValueChanged.AddListener(_ => Resize());
    }

    void Start()
    {
        Canvas.ForceUpdateCanvases();
        Resize();
    }
    void Resize()
    {
        inputField.textComponent.ForceMeshUpdate();

        float textHeight = inputField.textComponent.preferredHeight + padding;
        float minHeight = viewport.rect.height;

        float finalHeight = Mathf.Max(textHeight, minHeight);
        contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalHeight);
    }
}
