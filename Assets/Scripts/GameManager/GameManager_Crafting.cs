using UnityEngine;
using System;

public class GameManager_Crafting : GameManager
{
    protected override void RegisterLevelSpecificPythonCommands()
    {
        void Bind(string pyName, Action action) => PythonExecutor.instance.RegisterPythonFunction(pyName, action);

        Bind("pickup_part", PickupPart);
        Bind("submit_to_machine", SubmitToMachine);
    }

    public void PickupPart()
    {
        TileObject currentTile = GetCurrentTile();
        // Logic to check if a machine part is on the ground, and add to inventory
        Debug.Log("Picked up a machine part.");
    }

    public void SubmitToMachine()
    {
        TileObject currentTile = GetCurrentTile();
        // Logic to check if player is facing the Crafting Machine, and remove parts from inventory
        Debug.Log("Submitted parts to the Crafting Machine!");
    }
}