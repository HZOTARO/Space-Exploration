using UnityEngine;

public class CaveTile_BlackOre : ValueTile_Floating
{
    [HideInInspector]
    public bool isMined = false;
    [HideInInspector]
    public bool isCollected = false;
    [HideInInspector]
    public bool isPurified = false;

    [Header("References")]
    public GameObject normalOre;
    public GameObject destroyedOre;
    public GameObject pointLight;
    public GameObject explosionPrefab;

    protected override void Start()
    {
        base.Start();
        isWalkable = false;
        isFloating = false;
    }

    public void Purify()
    {
        if (!isMined && !isPurified)
        {
            isPurified = true;
            if (pointLight.TryGetComponent<Light>(out Light myLight))
            {
                myLight.color = new Color(0.5073529f, 0.7361954f, 1f);
            }
            Debug.Log("You purified a Black Ore into a Purple Vein!");
        }
        else
        {
            Debug.Log("Cannot purify.");
        }
    }

    public bool Mine()
    {
        bool exploded = false;
        if (!isMined)
        {
            isMined = true;

            if (isPurified)
            {
                Debug.Log("You mined a Black Ore!");
            }
            else
            {
                value /= 2;
                if(explosionPrefab) Instantiate(explosionPrefab, transform.position + new Vector3(1, 0, 1.25f), Quaternion.identity);
                Debug.Log("Ore exploded");
                exploded = true;
            }

            if (normalOre) normalOre.SetActive(false);
            if (destroyedOre) destroyedOre.SetActive(true);
            if (floatingItem) floatingItem.SetActive(true);
            isFloating = true;
            isWalkable = true;
        }
        else
        {
            Debug.Log("This Black Ore has already been mined.");
        }
        return exploded;
    }
    public override int Collect()
    {
        if (isMined)
        {
            if (!isCollected)
            {
                isCollected = true;
                Debug.Log("You collected a Black Ore!");
                if (floatingItem) floatingItem.SetActive(false);
                if (pointLight) pointLight.SetActive(false);
                isFloating = false;

                return value;
            }
            else
            {
                Debug.Log("This Black Ore has already been collected.");
            }
        }
        else
        {
            Debug.Log("You need to mine the Black Ore before collecting.");
        }
        return -1;
    }
}
