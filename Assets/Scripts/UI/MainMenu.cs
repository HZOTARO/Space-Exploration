using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public string hub;
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(hub);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
