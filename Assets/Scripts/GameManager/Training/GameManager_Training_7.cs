using UnityEngine;

public class GameManager_Training_7 : GameManager_Training
{
    protected override void RegisterLevelSpecificPythonCommands()
    {
        base.RegisterLevelSpecificPythonCommands();
        BindReturn("scan", Scan);
        BindReturn("measure", Measure);
        Bind("mine", Mine);
        Bind("collect", Collect);
    }
    protected override void SetLevelAllowedSyntax()
    {
        base.SetLevelAllowedSyntax();
        
        allowedSyntaxNodes.AddRange(SyntaxDictionary.Variables);
        allowedSyntaxNodes.AddRange(SyntaxDictionary.Logic);
        allowedSyntaxNodes.AddRange(SyntaxDictionary.Loops);
    }

    protected override void SetLevelObjectives()
    {
        base.SetLevelObjectives();

        levelLength = 1;
        levelWidth = Random.Range(30, 51);

        int generatedGoalX = Random.Range(levelWidth / 2, levelWidth);

        TileManager_Training_7 tileManager4 = FindFirstObjectByType<TileManager_Training_7>();
        if (tileManager4 != null)
        {
            tileManager4.goalX = generatedGoalX;
        }

        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = $"The map is {levelWidth} tiles long.\nThe goal is located on Tile {generatedGoalX + 1}.\nWrite 'move()' only once in your code!\nDo it in a single run.",
            type = ObjectiveType.CustomEvent,
            customEventId = "MovedWithLoop"
        });
    }

    protected override void Start()
    {
        base.Start();

        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinished += CheckPrecisionGoal;
            PythonExecutor.instance.OnExecutionAborted += HandleAbort;
        }
    }

    public override void Move(string directionString, int distance)
    {
        if (PythonExecutor.instance == null) return;

        string currentCode = PythonExecutor.instance.currentCode;

        if (!ValidateFunctionCallCount("move", 1, true)) return;

        bool isLoopValid = PythonExecutor.instance.CheckASTPattern(1, 999, "FuncInsideFor", "move_forward");

        if (!isLoopValid)
        {
            PythonExecutor.instance.TriggerRuntimeError("<color=red>You must use move inside a 'for' loop on this level.</color>", true);
            return;
        }

        base.Move(directionString, distance);
    }

    private void CheckPrecisionGoal()
    {
        if (completed) return;

        TileObject finalTile = GetCurrentTile();

        if (finalTile != null && finalTile.type == TileType.Goal)
        {
            PrintToDisplay("<color=green>Target Acquired! Precision destination matched perfectly.</color>");
            ObjectiveManager.instance.TriggerCustomEvent("MovedWithLoop");
        }
        else
        {
            PrintToDisplay("<color=red>Missed! You did not stop precisely on the Goal tile.</color>");
            ResetPlayerToStart();
        }
    }

    protected override void ResetPlayerToStart()
    {
        if (completed) return;

        base.ResetPlayerToStart();

        if (tileManager != null) tileManager.GenerateMap();
    }

    protected override void StartValuesSetup()
    {
        cargoSize = 0;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinished -= CheckPrecisionGoal;
            PythonExecutor.instance.OnExecutionAborted -= HandleAbort;
        }
    }
}
