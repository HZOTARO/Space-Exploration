using UnityEngine;

public class GameManager_Training_4 : GameManager_Training
{
    private Vector3 startingPhysicalPos;
    private Quaternion startingPhysicalRot;

    protected override void RegisterLevelSpecificPythonCommands()
    {
        Bind("move_forward", MoveForward);
        Bind("move_backward", MoveBackward);
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

        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = "Reach your goal in one play execution(Play)\nAutomate your movement by using move_forward() inside a 'for' loop",
            type = ObjectiveType.CustomEvent,
            customEventId = "MovedWithLoop"
        });
    }

    protected override void Start()
    {
        base.Start();

        if (player != null)
        {
            startingPhysicalPos = player.transform.position;
            startingPhysicalRot = player.transform.rotation;
        }

        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinished += CheckPrecisionGoal;
            PythonExecutor.instance.OnRuntimeError += HandleRuntimeError;
            PythonExecutor.instance.OnExecutionAborted += HandleAbort;
        }
    }

    private void HandleAbort()
    {
        PrintToDisplay("<color=orange>Program Aborted. Resetting position...</color>");
        ResetPlayerToStart();
    }

    private void HandleRuntimeError(int line, string message)
    {
        PrintToDisplay($"<color=red>Code Error: {message}</color> Resetting...");
        ResetPlayerToStart();
    }

    public override void MoveForward()
    {
        int startLine = 1;
        int endLine = 1;

        if (PythonExecutor.instance != null && !string.IsNullOrEmpty(PythonExecutor.instance.currentCode))
        {
            string[] codeLines = PythonExecutor.instance.currentCode.Split('\n');
            endLine = Mathf.Max(1, codeLines.Length);
        }

        bool calledInsideLoop = PythonExecutor.instance.CheckASTPattern(startLine, endLine, "FuncInsideFor", "move_forward");

        if (calledInsideLoop)
        {
            base.MoveForward();
        }
        else
        {
            PrintToDisplay("Action Blocked! You must use a 'for' loop to automate your movement on this level.");
            if (PythonExecutor.instance != null) PythonExecutor.instance.StopRunningCode();
        }
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
            PrintToDisplay("<color=red>Missed! You did not stop precisely on the Goal tile (Tile 7). Resetting position...</color>");
            ResetPlayerToStart();
        }
    }

    private void ResetPlayerToStart()
    {
        playerGridLoc = Vector2Int.zero;
        playerFacing = 0;

        if (player != null)
        {
            player.transform.position = startingPhysicalPos;
            player.transform.rotation = startingPhysicalRot;

            player.ResetPlayerState();
        }
    }

    protected override void StartValuesSetup()
    {
        levelLength = 1;
        levelWidth = 8;
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
