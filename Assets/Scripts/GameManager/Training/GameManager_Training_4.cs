using UnityEngine;
using System.Text.RegularExpressions;

public class GameManager_Training_4 : GameManager_Training
{
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

        levelLength = 1;
        levelWidth = Random.Range(100, 201);

        int generatedGoalX = Random.Range(levelWidth / 2, levelWidth);

        if (tileManager)
        {
            TileManager_Training_4 tileManager4 = tileManager as TileManager_Training_4;
            if (tileManager4 != null)
            {
                tileManager4.goalX = generatedGoalX;
            }
        }

        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = $"The map is {levelWidth} tiles long.\nThe Goal is precisely on Tile {generatedGoalX}.\nReach it by writing 'move_forward()' only once in your code!",
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

    //public override void MoveForward()
    //{
    //    int startLine = 1;
    //    int endLine = 1;

    //    if (PythonExecutor.instance != null && !string.IsNullOrEmpty(PythonExecutor.instance.currentCode))
    //    {
    //        string[] codeLines = PythonExecutor.instance.currentCode.Split('\n');
    //        endLine = Mathf.Max(1, codeLines.Length);
    //    }

    //    bool calledInsideLoop = PythonExecutor.instance.CheckASTPattern(startLine, endLine, "FuncInsideFor", "move_forward");

    //    if (calledInsideLoop)
    //    {
    //        base.MoveForward();
    //    }
    //    else
    //    {
    //        PrintToDisplay("Action Blocked! You must use a 'for' loop to automate your movement on this level.");
    //        if (PythonExecutor.instance != null) PythonExecutor.instance.StopRunningCode();
    //    }
    //}

    private void CheckPrecisionGoal()
    {
        string code = PythonExecutor.instance.currentCode;

        if (!string.IsNullOrEmpty(code))
        {
            int moveCount = Regex.Matches(code, "move_forward\\(\\)").Count;

            if (moveCount > 1)
            {
                PrintToDisplay($"<color=red>Constraint Failed! You wrote move_forward() {moveCount} times. You are only allowed to write it 1 time!</color>");
                ResetPlayerToStart();
                return;
            }
        }

        TileObject finalTile = GetCurrentTile();

        if (finalTile != null && finalTile.type == TileType.Goal)
        {
            PrintToDisplay("<color=green>Target Acquired! Precision destination matched perfectly.</color>");
            ObjectiveManager.instance.TriggerCustomEvent("MovedWithLoop");
            base.OnLevelComplete();
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
