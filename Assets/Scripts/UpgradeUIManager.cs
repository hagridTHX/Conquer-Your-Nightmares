using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradeUIManager : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;
    public UpgradeManager upgradeManager;
    
    [Header("UI Elements")]
    public GameObject levelUpPanel; 
    public Button[] upgradeButtons; 

    [System.Serializable]
    public class ButtonUI
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descText;
        public Image iconImage;
    }
    public ButtonUI[] buttonUIs;

    void Start()
    {
        levelUpPanel.SetActive(false);
        if (playerStats != null)
        {
            playerStats.OnLevelUp.AddListener(HandleLevelUp);
        }
    }

    private void HandleLevelUp(int newLevel)
    {
        if (upgradeManager.disableUpgradesForTesting) return;

        Time.timeScale = 0f; 
        levelUpPanel.SetActive(true);

        List<UpgradeData> drawnCards = upgradeManager.GetRandomUpgrades(3);

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i < drawnCards.Count)
            {
                UpgradeData cardData = drawnCards[i];
                upgradeButtons[i].gameObject.SetActive(true);
                
                buttonUIs[i].nameText.text = cardData.upgradeName;
                buttonUIs[i].descText.text = cardData.description;
                if(cardData.icon != null) buttonUIs[i].iconImage.sprite = cardData.icon;

                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].onClick.AddListener(() => OnUpgradeSelected(cardData));
            }
            else
            {
                upgradeButtons[i].gameObject.SetActive(false); 
            }
        }
    }

    private void OnUpgradeSelected(UpgradeData selectedUpgrade)
    {
        upgradeManager.AddUpgrade(selectedUpgrade);

        levelUpPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}