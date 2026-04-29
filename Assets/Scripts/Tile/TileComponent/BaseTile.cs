using UnityEngine;

public class BaseTile : MonoBehaviour
{
    [HideInInspector]
    public bool haveTile = false;
    [HideInInspector]
    public int z, x;

    public ItemSO itemOnTile;
}
