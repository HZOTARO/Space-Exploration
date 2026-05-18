using UnityEngine;

public class Enemy : MonoBehaviour
{
    [HideInInspector] public Vector2Int gridLoc;
    [HideInInspector] public Vector2Int patrolDir;

    public void Setup(Vector2Int startPosition, Vector2Int patrolDirection)
    {
        gridLoc = startPosition;
        patrolDir = patrolDirection;
        UpdateVisualPosition();
    }

    public void UpdateVisualPosition()
    {
        transform.localPosition = new Vector3(gridLoc.x, transform.localPosition.y, gridLoc.y);
    }
}