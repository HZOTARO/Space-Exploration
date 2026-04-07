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
        }
        else if (currentTile.type == TileType.BlackOre)
        {
            
        }
    }
    public void Collect()
    {
        // Placeholder for collecting action, can be expanded based on game design
        PrintToDisplay("Collecting action performed at location: " + playerGridLoc);
    }
    public void Purify()
    {
        // Placeholder for purifying action, can be expanded based on game design
        PrintToDisplay("Purifying action performed at location: " + playerGridLoc);
    }
    public void Drill()
    {
        // Placeholder for drilling action, can be expanded based on game design
        PrintToDisplay("Drilling action performed at location: " + playerGridLoc);
    }
    public void Pump()
    {
        // Placeholder for pumping action, can be expanded based on game design
        PrintToDisplay("Pumping action performed at location: " + playerGridLoc);
    }
    public void Measure()
    {
        // Placeholder for measuring action, can be expanded based on game design
        PrintToDisplay("Measuring action performed at location: " + playerGridLoc);
    }
}