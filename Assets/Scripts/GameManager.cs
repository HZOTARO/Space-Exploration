using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindFirstObjectByType<Player>();
    }

    public bool InAction()
    {
        return player != null && player.inAction;
    }
    public void Move(string dir)
    {
        switch (dir)
        {
            case "N":
                player.Move(Direction.Forward);
                break;
            case "S":
                player.Move(Direction.Backward);
                break;
            case "E":
                player.Move(Direction.Right);
                break;
            case "W":
                player.Move(Direction.Left);
                break;
            default:
                Debug.LogWarning($"Unknown direction: {dir}");
                break;
        }
    }
}
