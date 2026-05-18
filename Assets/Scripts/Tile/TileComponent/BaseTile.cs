using UnityEngine;

public class BaseTile : MonoBehaviour
{
    [HideInInspector]
    public int z, x;

    [Header("Movement Settings")]
    public bool isWalkable = false;
}
