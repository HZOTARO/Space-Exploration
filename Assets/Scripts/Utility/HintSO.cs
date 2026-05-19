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
    [Header("Hint Meta Data")]
    public string hintId;
    public string title;

    [Header("Hint Content (Stacked)")]
    public List<HintBlock> hintBlocks = new List<HintBlock>();

    [Header("Unlock Settings")]
    public bool isUnlockedByDefault = false;
    public bool ignoreLock = false;
}