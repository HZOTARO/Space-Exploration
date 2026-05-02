using UnityEngine;

[CreateAssetMenu(fileName = "HintSO", menuName = "Scriptable Objects/HintSO")]
public class HintSO : ScriptableObject
{
    public string hintId;
    public string displayName;
    public Sprite image;
    [TextArea] public string description;
}
