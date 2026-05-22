using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthComponent : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI References")]
    public Image healthBar;
    public TextMeshProUGUI healthText;

    public event System.Action OnPlayerDeath;

    [HideInInspector] public Player player;
    public void Initialize()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void DamagePlayer(int damage)
    {
        Debug.Log($"<color=red>Player took {damage} damage!</color>");
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        UpdateUI();

        if (player) player.PlayDamagedEffect();

        if (currentHealth <= 0)
        {
            OnPlayerDeath?.Invoke();
        }
        else if (player)
        {
            player.PerformAction(PlayerAction.Hurt, null);
        }
    }

    private void UpdateUI()
    {
        if (healthBar) healthBar.fillAmount = (float)currentHealth / maxHealth;
        if (healthText) healthText.text = $"{currentHealth} / {maxHealth}";
    }
}
