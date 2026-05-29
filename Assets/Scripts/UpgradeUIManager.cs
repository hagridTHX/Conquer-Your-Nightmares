using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradeUIManager : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private GameObject levelUpPanel; 
    [SerializeField] private Button[] upgradeButtons; 
    [SerializeField] private ButtonUI[] buttonUIs;

    public PlayerStats PlayerStats { get => playerStats; set => playerStats = value; }
    public UpgradeManager UpgradeManager { get => upgradeManager; set => upgradeManager = value; }
    public GameObject LevelUpPanel { get => levelUpPanel; set => levelUpPanel = value; }
    public Button[] UpgradeButtons { get => upgradeButtons; set => upgradeButtons = value; }
    public ButtonUI[] ButtonUIs { get => buttonUIs; set => buttonUIs = value; }

    [System.Serializable]
    public class ButtonUI
    {
        public TextMeshProUGUI descText;
        public Image iconImage;
    }

    private void Start()
    {
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        if (playerStats != null)
        {
            playerStats.OnLevelUp.AddListener(HandleLevelUp);
        }
    }

    private void HandleLevelUp(int newLevel)
    {
        if (upgradeManager.DisableUpgradesForTesting) return;

        Time.timeScale = 0f; 
        if (levelUpPanel != null) levelUpPanel.SetActive(true);

        List<UpgradeManager.UpgradeOption> drawnCards = upgradeManager.GetRandomUpgrades(3);

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i < drawnCards.Count)
            {
                UpgradeManager.UpgradeOption cardOption = drawnCards[i];
                upgradeButtons[i].gameObject.SetActive(true);
                
                if (buttonUIs[i].descText != null) 
                {
                    buttonUIs[i].descText.text = $"<b>{cardOption.BaseData.UpgradeName}</b>\n\n" +
                                                 $"{cardOption.BaseData.GetFormattedDescription(cardOption.Tier.multiplier)}";

                    buttonUIs[i].descText.color = Color.black;

                    buttonUIs[i].descText.outlineWidth = 0.25f; 
                    buttonUIs[i].descText.outlineColor = cardOption.Tier.rarityColor;
                }
                
                if (buttonUIs[i].iconImage != null && cardOption.Tier.rarityIcon != null) 
                {
                    buttonUIs[i].iconImage.sprite = cardOption.Tier.rarityIcon;
                }

                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].onClick.AddListener(() => OnUpgradeSelected(cardOption));
            }
            else
            {
                upgradeButtons[i].gameObject.SetActive(false); 
            }
        }
    }

    private void OnUpgradeSelected(UpgradeManager.UpgradeOption selectedOption)
    {
        upgradeManager.AddUpgrade(selectedOption);

        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}