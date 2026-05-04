using UnityEngine;

public class CaveTile_PurpleVein : ValueTile
{
    [HideInInspector]
    public bool isDrilled = false;
    [HideInInspector]
    public bool isPumped = false;

    public Renderer matRenderer;
    public Material drilledMat;
    public Material pumpedMat;

    protected override void Start()
    {
        base.Start();
        isWalkable = true;
    }

    public void Drill()
    {
        if (!isDrilled)
        {
            isDrilled = true;
            if (matRenderer && drilledMat)
            {
                matRenderer.material = drilledMat;
            }
            Debug.Log("You drilled a Purple Vein!");
        }
        else
        {
            Debug.Log("This Purple Vein has already been drilled.");
        }
    }

    public int Pump()
    {
        if (!isPumped)
        {
            if (isDrilled)
            {
                isPumped = true;
                Debug.Log("You pumped a Purple Vein!");
                if (matRenderer && pumpedMat)
                {
                    matRenderer.material = pumpedMat;
                    return value;
                }
            }
            else
            {
                Debug.Log("You need to drill the Purple Vein before pumping.");
            }
        }
        else
        {
            Debug.Log("Cannot pump.");
        }
        return -1;
    }
}
