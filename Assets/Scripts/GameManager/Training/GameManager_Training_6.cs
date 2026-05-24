public class GameManager_Training_6 : GameManager_Training
{
    protected override void RegisterLevelSpecificPythonCommands()
    {
        BindReturn("scan", Scan);
        Bind("turn_right", TurnRight);
        Bind("turn_left", TurnLeft);

        Bind("mine", Mine);
        Bind("collect", Collect);
        Bind("drill", Drill);
        Bind("pump", Pump);
        Bind("purify", Purify);
    }

    protected override void SetLevelAllowedSyntax()
    {
        base.SetLevelAllowedSyntax();
        allowedSyntaxNodes.AddRange(SyntaxDictionary.Variables);
        allowedSyntaxNodes.AddRange(SyntaxDictionary.Logic);

        allowedSyntaxNodes.Remove("If");
        allowedSyntaxNodes.Remove("IfExp");

        customLevelErrors["If"] = "If statements are banned in this unstable environment! Use match/case instead.";
        customLevelErrors["IfExp"] = "If statements are banned in this unstable environment! Use match/case instead.";
    }

    protected override void SetLevelObjectives()
    {
        base.SetLevelObjectives();

        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = "Unstable Environment! Use match/case to handle the random resources and collect them all in a single run.",
            type = ObjectiveType.CustomEvent,
            customEventId = "LevelSolved"
        });
    }

    protected override void StartValuesSetup()
    {
        levelLength = 2;
        levelWidth = 2;
        cargoSize = 2;
    }

    protected override void Start()
    {
        base.Start();
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionStarted += RandomizeUnstableEnvironment;
            PythonExecutor.instance.OnExecutionFinishedBefore += CheckWinCondition;
            PythonExecutor.instance.OnRuntimeError += HandleRuntimeError;
            PythonExecutor.instance.OnExecutionAborted += HandleAbort;
        }
    }

    public void RandomizeUnstableEnvironment()
    {
        PrintToDisplay("<color=yellow>Warning: Unstable Environment shifting...</color>");
        if (tileManager != null) tileManager.GenerateMap();
    }

    public override void Mine()
    {
        TileObject targetTile = GetTileInFront();
        if (targetTile == null) { PrintToDisplay("Nothing to mine. You are facing the edge of the map!"); return; }

        if (targetTile.type == TileType.WhiteOre)
        {
            CaveTile_WhiteOre ore = targetTile.tileInstance as CaveTile_WhiteOre;
            if (!ore.isMined) player.PerformAction(PlayerAction.Mine, () => ore.Mine());
            else PrintToDisplay("This White Ore has already been mined.");
        }
        else if (targetTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = targetTile.tileInstance as CaveTile_BlackOre;
            if (!ore.isMined) player.PerformAction(PlayerAction.Mine, () => { if (ore.Mine()) healthComponent.DamagePlayer(100); });
            else PrintToDisplay("This Black Ore has already been mined.");
        }
        else { PrintToDisplay("No mineable resource in front of you."); }
    }

    public override void Collect()
    {
        TileObject targetTile = GetTileInFront();
        if (targetTile == null) { PrintToDisplay("You are facing the edge of the map!"); return; }
        if (cargoComponent && cargoComponent.IsFull()) { PrintToDisplay("Cargo is full."); return; }

        if (targetTile.type == TileType.WhiteOre)
        {
            CaveTile_WhiteOre ore = targetTile.tileInstance as CaveTile_WhiteOre;
            if (ore.isMined && !ore.isCollected)
            {
                player.PerformAction(PlayerAction.Collect, () =>
                {
                    int amountCollected = ore.Collect();
                    if (amountCollected > 0)
                    {
                        cargoComponent.AddToCargo(ore.itemOnTile, amountCollected);
                        PrintToDisplay($"<color=white>Collected {amountCollected} White Ore.</color>");
                        targetTile.type = TileType.Floor;
                        if (targetTile.tileInstance != null) { Destroy(targetTile.tileInstance.gameObject); targetTile.tileInstance = null; }
                    }
                });
            }
        }
        else if (targetTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = targetTile.tileInstance as CaveTile_BlackOre;
            if (ore.isMined && !ore.isCollected)
            {
                player.PerformAction(PlayerAction.Collect, () =>
                {
                    int amountCollected = ore.Collect();
                    if (amountCollected > 0)
                    {
                        cargoComponent.AddToCargo(ore.itemOnTile, amountCollected);
                        PrintToDisplay($"<color=black>Collected {amountCollected} Black Ore.</color>");
                        targetTile.type = TileType.Floor;
                        if (targetTile.tileInstance != null) { Destroy(targetTile.tileInstance.gameObject); targetTile.tileInstance = null; }
                    }
                });
            }
        }
    }

    public virtual void Drill()
    {
        TileObject targetTile = GetTileInFront();
        if (targetTile == null) return;
        if (targetTile.type == TileType.PurpleEssence)
        {
            CaveTile_PurpleVein vein = targetTile.tileInstance as CaveTile_PurpleVein;
            if (!vein.isDrilled) player.PerformAction(PlayerAction.Drill, () => vein.Drill());
        }
    }

    public virtual void Pump()
    {
        TileObject targetTile = GetTileInFront();
        if (targetTile == null) return;
        if (targetTile.type == TileType.PurpleEssence)
        {
            CaveTile_PurpleVein vein = targetTile.tileInstance as CaveTile_PurpleVein;
            if (vein.isDrilled && !vein.isPumped)
            {
                player.PerformAction(PlayerAction.Pump, () =>
                {
                    int amountPumped = vein.Pump();
                    if (amountPumped > 0)
                    {
                        cargoComponent.AddToCargo(vein.itemOnTile, amountPumped);
                        PrintToDisplay($"<color=purple>Collected {amountPumped} Purple Liquid.</color>");
                        targetTile.type = TileType.Floor;
                        if (targetTile.tileInstance != null) targetTile.tileInstance = null;
                    }
                });
            }
        }
    }

    public virtual void Purify()
    {
        TileObject targetTile = GetTileInFront();
        if (targetTile != null && targetTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = targetTile.tileInstance as CaveTile_BlackOre;
            if (!ore.isPurified) player.PerformAction(PlayerAction.Purify, () => ore.Purify());
        }
    }

    private void CheckWinCondition()
    {
        if (tileManager.objectsArray[1, 0].type == TileType.Floor &&
            tileManager.objectsArray[0, 1].type == TileType.Floor)
        {
            PrintToDisplay("<color=green>Success! You successfully matched and handled both resources.</color>");
            ObjectiveManager.instance.TriggerCustomEvent("LevelSolved");
        }
        else
        {
            PrintToDisplay("<color=orange>Level failed! Make sure you collect both resources. Watch out for the Black Ore!</color>");
            ResetPlayerToStart();
        }
    }

    protected override void ResetPlayerToStart()
    {
        base.ResetPlayerToStart();

        RandomizeUnstableEnvironment();

        if (cargoComponent != null) cargoComponent.cargoSize = cargoSize;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionStarted -= RandomizeUnstableEnvironment;
            PythonExecutor.instance.OnExecutionFinishedBefore -= CheckWinCondition;
            PythonExecutor.instance.OnRuntimeError -= HandleRuntimeError;
            PythonExecutor.instance.OnExecutionAborted -= HandleAbort;
        }
    }
}