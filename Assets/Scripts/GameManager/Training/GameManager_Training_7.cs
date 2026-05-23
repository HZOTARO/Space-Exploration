using UnityEngine;

public class GameManager_Training_7 : GameManager_Training
{
    protected override void RegisterLevelSpecificPythonCommands()
    {
        BindReturn("move_forward", MoveForward);
        BindReturn("move_backward", MoveBackward);
        Bind("turn_right", TurnRight);
        Bind("turn_left", TurnLeft);
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
            description = $"The map is {levelWidth} tiles long.\nThe Goal is precisely on Tile {generatedGoalX + 1}.\nReach it by writing 'move_forward()' only once in your code!",
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
            PythonExecutor.instance.OnRuntimeError += HandleRuntimeError;
            PythonExecutor.instance.OnExecutionAborted += HandleAbort;
        }
    }

    public override bool MoveForward()
    {
        if (PythonExecutor.instance == null) return false;

        string currentCode = PythonExecutor.instance.currentCode;

        if (!ValidateFunctionCallCount("move_forward", 1, true)) return false;

        bool isLoopValid = PythonExecutor.instance.CheckASTPattern(1, 999, "FuncInsideFor", "move_forward");

        if (!isLoopValid)
        {
            PrintToDisplay("<color=red>Action Blocked! You must use a 'for' loop to automate your movement on this level.</color>");
            PythonExecutor.instance.StopRunningCode();
            return false;
        }

        return base.MoveForward();
    }

    private void CheckPrecisionGoal()
    {
        TileObject finalTile = GetCurrentTile();

        if (finalTile != null && finalTile.type == TileType.Goal)
        {
            PrintToDisplay("<color=green>Target Acquired! Precision destination matched perfectly.</color>");
            ObjectiveManager.instance.TriggerCustomEvent("MovedWithLoop");
        }
        else
        {
            PrintToDisplay("<color=red>Missed! You did not stop precisely on the Goal tile. Resetting position...</color>");
            ResetPlayerToStart();
        }
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
            PythonExecutor.instance.OnRuntimeError -= HandleRuntimeError;
            PythonExecutor.instance.OnExecutionAborted -= HandleAbort;
        }
    }
}
