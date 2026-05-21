using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct HintBlock
{
    public string subtitle;
    public Sprite image;
    [TextArea(3, 10)]
    public string description;
}

[CreateAssetMenu(fileName = "HintSO", menuName = "Scriptable Objects/HintSO")]
public class HintSO : ScriptableObject
{
    public string hintId;
    public bool isUnlockedByDefault;
    public bool ignoreLock;

    [Header("Hint Content")]
    public string title;
    public Sprite image;
    [TextArea(3, 10)]
    public string description;

    [Header("Formatting")]
    public bool isImageBig;
}