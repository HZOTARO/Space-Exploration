using UnityEngine;

public class ValueTile_Floating : ValueTile
{
    [Header("Floating Settings")]
    public bool isFloating = true;
    public GameObject floatingItem;
    public float floatSpeed = 2f;
    public float floatHeight = 0.5f;
    private Vector3 startPos;

    protected override void Start()
    {
        base.Start();
        isWalkable = true;
        if (floatingItem)
        {
            startPos = floatingItem.transform.position;
        }
    }
    protected virtual void Update()
    {
        if (isFloating && floatingItem)
        {
            float shiftedSin = (Mathf.Sin(Time.time * floatSpeed) + 1f) / 2f;
            float newY = startPos.y + (shiftedSin * floatHeight);

            floatingItem.transform.position = new Vector3(startPos.x, newY, startPos.z);
        }
    }
}