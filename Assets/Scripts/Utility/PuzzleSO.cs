using UnityEngine;

[CreateAssetMenu(fileName = "New Puzzle", menuName = "Scriptable Objects/PuzzleSO")]
public class PuzzleSO : ScriptableObject
{
    public string id;
    public string puzzleName;

    public int levelSize;

    [Header("Requirements")]
    public UpgradeSO[] prerequisiteUpgrades;
    public PuzzleSO[] prerequisitePuzzles;
}