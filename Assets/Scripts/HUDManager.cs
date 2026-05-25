using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class HUDManager : MonoBehaviour
{
    [Header("Player References")]
    public PlayerStats playerStats;

    [Header("Bars and Text")]
    public Image healthBarFill;
    public Image xpBarFill;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI timerText;

    [Header("Artifacts")]
    public Transform artifactsContainer; 
    public GameObject artifactIconPrefab; 

    // Timer is sourced from GameManager to avoid local drift and double-accounting.

    void Start()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged.AddListener(UpdateHealth);
            playerStats.OnXPChanged.AddListener(UpdateXP);
            playerStats.OnLevelUp.AddListener(UpdateLevel);
        }
    }

    void Update()
    {
        if (GameManager.Instance != null)
        {
            UpdateTimerDisplay(GameManager.Instance.GameTime);
        }
    }

    private void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthBarFill != null) healthBarFill.fillAmount = currentHealth / maxHealth;
    }

    private void UpdateXP(float currentXP, float xpToNextLevel)
    {
        if (xpBarFill != null) xpBarFill.fillAmount = currentXP / xpToNextLevel;
    }

    private void UpdateLevel(int newLevel)
    {
        if (levelText != null) levelText.text = "LVL: " + newLevel;
    }

    // Accept authoritative session time so UI stays consistent with game state.
    private void UpdateTimerDisplay(float currentTime)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60F);
            int seconds = Mathf.FloorToInt(currentTime - minutes * 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void AddArtifactIcon(Sprite artifactSprite)
    {
        if (artifactIconPrefab == null || artifactsContainer == null) return;
        GameObject newIcon = Instantiate(artifactIconPrefab, artifactsContainer);
        Image iconImage = newIcon.GetComponent<Image>();
        if (iconImage != null) iconImage.sprite = artifactSprite;
    }
}