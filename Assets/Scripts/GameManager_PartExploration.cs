using UnityEngine;
using System;

public class GameManager_PartExploration : GameManager
{
    protected override void RegisterLevelSpecificPythonCommands()
    {
        void Bind(string pyName, Action action) => PythonExecutor.instance.RegisterPythonFunction(pyName, action);

        // Add commands specific to avoiding/fighting enemies
        Bind("wait", Wait);
        Bind("attack", Attack);
        // Bind("scan_enemy", ScanEnemy); 
    }

    public void Wait()
    {
        // Tell the player to skip a turn while the enemy patrols
        //player.PerformAction(PlayerAction.Idle, () => Debug.Log("Player waited for 1 turn."));
    }

    public void Attack()
    {
        // Check if enemy is in front of player and deal damage
        Debug.Log("Player attacks the enemy!");
    }
}