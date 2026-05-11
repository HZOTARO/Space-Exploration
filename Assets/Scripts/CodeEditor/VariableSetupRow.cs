using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VariableSetupRow : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI labelText;
    public TMP_InputField nameInputField;
    public Button removeButton;

    public string VariableName => nameInputField != null ? nameInputField.text.Trim() : "";

    public void Setup(string name, System.Action<VariableSetupRow> onRemoveCallback)
    {
        if (labelText != null) 
        { 
            labelText.text = name;
        }
        if (removeButton != null)
        {
            removeButton.onClick.AddListener(() => onRemoveCallback(this));
        }
    }
}