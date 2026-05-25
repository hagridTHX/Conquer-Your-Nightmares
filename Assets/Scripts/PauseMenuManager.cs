using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 
using TMPro;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel; 
    public Transform statsContainer; 
    public GameObject statPrefab;    

    [Header("Logic References")]
    public UpgradeManager upgradeManager;

    private bool isPaused = false;

    void Start()
    {
        pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Time.timeScale == 0f && !isPaused) return; 

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; 
        pauseMenuPanel.SetActive(true);
        RefreshStatsUI(); 
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; 
        pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
        Debug.Log("Wyświetlam Ustawienia (Placeholder)");
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu"); 
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }

    private void RefreshStatsUI()
    {
        if (statsContainer == null || statPrefab == null || upgradeManager == null) return;

        foreach (Transform child in statsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var entry in upgradeManager.activeUpgrades)
        {
            GameObject newStat = Instantiate(statPrefab, statsContainer);
            
            TextMeshProUGUI textComp = newStat.GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null)
            {
                textComp.text = $"{entry.data.upgradeName} (LVL {entry.currentLevel})";
            }
            
            Image[] images = newStat.GetComponentsInChildren<Image>();
            foreach(var img in images)
            {
                if (img.gameObject.name == "IconImage" && entry.data.icon != null)
                {
                    img.sprite = entry.data.icon;
                }
            }
        }
    }
}