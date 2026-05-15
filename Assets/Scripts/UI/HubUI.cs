using UnityEngine;



public class HubUI : MonoBehaviour, IResourceUpdatable
{
    private Button_Id[] buttons;

    private void Awake()
    {
        buttons = GetComponentsInChildren<Button_Id>(true);
    }

    void Start()
    {
        UpdateResource(SaveManager.saveData);
    }

    public void UpdateResource(SaveData saveData)
    {
        foreach (Button_Id button in buttons)
        {
            button.SetupButton();
        }
    }
}