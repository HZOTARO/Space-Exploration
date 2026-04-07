using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI terminalText;
    private int maxLines = 10;

    public Player player;
    private TileManager tileManager;
    Vector2Int playerGridLoc = new();

    void Start()
    {
        if (!player) player = FindFirstObjectByType<Player>();
        if (!tileManager) tileManager = FindFirstObjectByType<TileManager>();
    }

    public void PrintToDisplay(string message)
    {
        if (terminalText == null) return;

        terminalText.text += "> " + message + "\n";

        string[] lines = terminalText.text.Split('\n');
        if (lines.Length > maxLines)
        {
            terminalText.text = string.Join("\n", lines, 1, Mathf.Min(lines.Length - 1, 10));
        }
    }

    /// <summary>
    /// In Animation or Moving
    /// </summary>
    public bool InAction()
    {
        return player != null && player.inAction;
    }
    public TileObject GetCurrentTile()
    {
        return tileManager.gridArray[playerGridLoc.y, playerGridLoc.x];
    }
    public void Move(string dir)
    {
        switch (dir)
        {
            case "N":
                if (playerGridLoc.y < tileManager.length - 1)
                {
                    player.Move(Direction.Forward);
                    playerGridLoc.y++;
                }
                break;
            case "S":
                if (playerGridLoc.y > 0)
                {
                    player.Move(Direction.Backward);
                    playerGridLoc.y--;
                }
                break;
            case "E":
                if (playerGridLoc.x < tileManager.width - 1)
                {
                    player.Move(Direction.Right);
                    playerGridLoc.x++;
                }
                break;
            case "W":
                if (playerGridLoc.x > 0)
                {
                    player.Move(Direction.Left);
                    playerGridLoc.x--;
                }
                break;
        }
    }
    public void Scan()
    {
        Debug.Log($"Current Tile: {GetCurrentTile().tileInstance.name}");
    }

    public void Mine()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile.type == TileType.WhiteOre)
        {
            CaveTile_WhiteOre ore = currentTile.tileInstance as CaveTile_WhiteOre;
            ore.Mine();
        }
        else if (currentTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = currentTile.tileInstance as CaveTile_BlackOre;
            ore.Mine();
        }
        else
        {
            Debug.Log("No mineable resource at current location.");
        }
    }
    public void Collect()
    {
        // Placeholder for collecting action, can be expanded based on game design
        PrintToDisplay("Collecting action performed at location: " + playerGridLoc);
    }
    public void Purify()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = currentTile.tileInstance as CaveTile_BlackOre;
            ore.Purify();
        }
        else
        {
            Debug.Log("No purifable resource at current location.");
        }
    }
    public void Drill()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile.type == TileType.PurpleVein)
        {
            CaveTile_PurpleVein vein = currentTile.tileInstance as CaveTile_PurpleVein;
            vein.Drill();
        }
        else
        {
            Debug.Log("No drillable resource at current location.");
        }
    }
    public void Pump()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile.type == TileType.PurpleVein)
        {
            CaveTile_PurpleVein vein = currentTile.tileInstance as CaveTile_PurpleVein;
            vein.Pump();
        }
        else
        {
            Debug.Log("No pumpable resource at current location.");
        }
    }
    public void Measure()
    {
        IMeasureable measureableTile = GetCurrentTile().tileInstance as IMeasureable;
        if (measureableTile != null)
        {
            int measurement = measureableTile.Measured();
            Debug.Log("Measurement result: " + measurement);
        }
        else
        {
            Debug.Log("Current tile is not measurable.");
        }
    }
}