using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemyPathReference
{
    public EnemyMark direction;
    public GameObject pathReference;
}

public enum EnemyMark
{
    Middle, Right, Up, Down, Left
}

public class EnemyPath : BaseTile
{
    public EnemyPathReference[] pathDirectionInput;
    Dictionary<EnemyMark, GameObject> pathReferences = new Dictionary<EnemyMark, GameObject>();

    private void Awake()
    {
        foreach (EnemyPathReference reference in pathDirectionInput)
        {
            pathReferences.Add(reference.direction, reference.pathReference);
            reference.pathReference.SetActive(false);
        }
    }

    public void SetPathDirection(EnemyMark direction)
    {
        if (pathReferences.ContainsKey(direction))
        {
            pathReferences[direction].SetActive(true);
        }
        else
        {
            Debug.LogWarning($"EnemyPath is missing a visual reference for: {direction}");
        }
    }
}
