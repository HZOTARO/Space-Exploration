using UnityEngine;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI terminalText;
    private int maxLines = 10;

    public Player player;
    private TileManager tileManager;
    Vector2Int playerGridLoc = new();

    [Header("Resources")]
    public TextMeshProUGUI whiteOreText;
    int whiteOreCount = 0;
    public TextMeshProUGUI purpleLiquidText;
    int purpleLiquidCount = 0;
    public TextMeshProUGUI blackOreText;
    int blackOreCount = 0;

    void Start()
    {
        if (!player) player = FindFirstObjectByType<Player>();
        if (!tileManager) tileManager = FindFirstObjectByType<TileManager>();

        if (whiteOreText) whiteOreText.text = $"{whiteOreCount}";
        if (purpleLiquidText) purpleLiquidText.text = $"{purpleLiquidCount}";
        if (blackOreText) blackOreText.text = $"{blackOreCount}";

        PythonExecutor.instance.RegisterPythonFunction("move_up", new Action(() => Move("N")));
        PythonExecutor.instance.RegisterPythonFunction("move_down", new Action(() => Move("S")));
        PythonExecutor.instance.RegisterPythonFunction("move_left", new Action(() => Move("W")));
        PythonExecutor.instance.RegisterPythonFunction("move_right", new Action(() => Move("E")));

        PythonExecutor.instance.RegisterPythonFunction("mine", new Action(() => Mine()));
        PythonExecutor.instance.RegisterPythonFunction("collect", new Action(() => Collect()));
        PythonExecutor.instance.RegisterPythonFunction("purify", new Action(() => Purify()));
        PythonExecutor.instance.RegisterPythonFunction("drill", new Action(() => Drill()));
        PythonExecutor.instance.RegisterPythonFunction("pump", new Action(() => Pump()));

        PythonExecutor.instance.RegisterPythonFunction("scan", new Func<string>(() => Scan()));
        PythonExecutor.instance.RegisterPythonFunction("measure", new Action(() => Measure())); 

        PythonExecutor.instance.CanStepCode = () => !InAction();
        PythonExecutor.instance.OnPythonPrint += PrintToDisplay;
    }

    void OnDestroy()
    {
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnPythonPrint -= PrintToDisplay;
        }

        if (SaveManager.instance != null)
        {
            SaveManager.saveData.whiteOre += whiteOreCount;
            SaveManager.saveData.purpleLiquid += purpleLiquidCount;
            SaveManager.saveData.blackOre += blackOreCount;

            SaveManager.instance.SaveGame(1);
        }
        else
        {
            Debug.LogWarning("SaveManager was already destroyed! Could not auto-save.");
        }
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
    public string Scan()
    {
        TileObject currentTile = GetCurrentTile();

        string tileTypeName = currentTile.type.ToString();

        Debug.Log($"Player scanned the tile: {tileTypeName}");

        return tileTypeName;
    }

    public void Mine()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile.type == TileType.WhiteOre)
        {
            CaveTile_WhiteOre ore = currentTile.tileInstance as CaveTile_WhiteOre;

            if (!ore.isMined)
            {
                player.PerformAction(PlayerAction.Mine, () => ore.Mine());
            }
            else
            {
                Debug.Log("This White Ore has already been mined.");
            }
        }
        else if (currentTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = currentTile.tileInstance as CaveTile_BlackOre;
            if (!ore.isMined)
            {
                player.PerformAction(PlayerAction.Mine, () => ore.Mine());
            }
            else
            {
                Debug.Log("This Black Ore has already been mined.");
            }
        }
        else
        {
            Debug.Log("No mineable resource at current location.");
        }
    }
    public void Purify()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = currentTile.tileInstance as CaveTile_BlackOre;

            if (!ore.isPurified)
            {
                player.PerformAction(PlayerAction.Purify, () => ore.Purify());
            }
            else
            {
                Debug.Log("This Black Ore has already been purified.");
            }
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

            if (!vein.isDrilled)
            {
                player.PerformAction(PlayerAction.Drill, () => vein.Drill());
            }
            else
            {
                Debug.Log("This Purple Vein has already been drilled.");
            }
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

            if (vein.isDrilled && !vein.isPumped)
            {
                player.PerformAction(PlayerAction.Pump, () =>
                {
                    int amountPumped = vein.Pump();
                    if (amountPumped > 0)
                    {
                        purpleLiquidCount += amountPumped;
                        purpleLiquidText.text = $"{purpleLiquidCount}";
                        Debug.Log($"<color=purple>Collected {amountPumped} Purple Liquid.</color>");
                    }
                });
            }
            else
            {
                Debug.Log("Cannot pump! It is either not drilled yet, or already empty.");
            }
        }
        else
        {
            Debug.Log("No pumpable resource at current location.");
        }
    }

    public void Collect()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile.type == TileType.WhiteOre)
        {
            CaveTile_WhiteOre ore = currentTile.tileInstance as CaveTile_WhiteOre;
            if (ore.isMined && !ore.isCollected)
            {
                player.PerformAction(PlayerAction.Collect, () =>
                {
                    int amountCollected = ore.Collect();
                    if (amountCollected > 0)
                    {
                        whiteOreCount += amountCollected;
                        whiteOreText.text = $"{whiteOreCount}";
                        Debug.Log($"<color=white>Collected {amountCollected} White Ore.</color>");
                    }
                });
            }
            else
            {
                Debug.Log("Cannot collect! It is either not mined, or already collected.");
            }
        }
        else if (currentTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = currentTile.tileInstance as CaveTile_BlackOre;
            if (ore.isMined && !ore.isCollected)
            {
                player.PerformAction(PlayerAction.Collect, () =>
                {
                    int amountCollected = ore.Collect();
                    if (amountCollected > 0)
                    {
                        blackOreCount += amountCollected;
                        blackOreText.text = $"{blackOreCount}";
                        Debug.Log($"<color=black>Collected {amountCollected} Black Ore.</color>");
                    }
                });
            }
            else
            {
                Debug.Log("Cannot collect! It is either not mined, or already collected.");
            }
        }
        else
        {
            Debug.Log("No collectable resource at current location.");
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