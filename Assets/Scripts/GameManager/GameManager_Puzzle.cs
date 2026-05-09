using UnityEngine;
using System;

public class GameManager_Puzzle : GameManager
{
    protected override void RegisterLevelSpecificPythonCommands()
    {
        void Bind(string pyName, Action action) => PythonExecutor.instance.RegisterPythonFunction(pyName, action);

        // Placeholder for future puzzle commands
        Bind("interact", Interact);
        Bind("push", PushObject);
    }

    public void Interact()
    {
        Debug.Log("Interacting with a puzzle element (lever, button, etc.)");
    }

    public void PushObject()
    {
        Debug.Log("Pushing a block forward.");
    }
}