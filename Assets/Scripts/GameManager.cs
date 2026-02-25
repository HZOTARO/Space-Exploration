using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI terminalText;
    private int maxLines = 10;

    public Player player;

    void Start()
    {
        player = FindFirstObjectByType<Player>();
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
