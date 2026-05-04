using UnityEngine;

public class CaveTile_WhiteOre : ValueTile_Floating
{
    [HideInInspector]
    public bool isMined = false;
    [HideInInspector]
    public bool isCollected = false;

    [Header("References")]
    public GameObject normalOre;
    public GameObject destroyedOre;
    public GameObject pointLight;

    protected override void Start()
    {
        base.Start();
        isFloating = false;
    }
    protected override int CalculateUpgradeIndex(int upgradeTier)
    {
        if (upgradeTier == 0)
        {
            return 1;
        }
        return upgradeTier + 1;
    }

    public void Mine()
    {
        if (!isMined)
        {
            isMined = true;
            if (normalOre) normalOre.SetActive(false);
            if (destroyedOre) destroyedOre.SetActive(true);
            if (floatingItem) floatingItem.SetActive(true);
            isFloating = true;
            Debug.Log("You mined a White Ore!");
        }
    }

    public override int Collect()
    {
        if (isMined && !isCollected)
        {
            isCollected = true;
            Debug.Log("You collected a White Ore!");
            if (floatingItem) floatingItem.SetActive(false);
            if (pointLight) pointLight.SetActive(false);
            isFloating = false;
            return value;
        }
        return -1;
    }
}
