using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 
using TMPro;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsPanel; 
    [SerializeField] private Transform statsContainer; 
    [SerializeField] private GameObject statPrefab;    
    [SerializeField] private UpgradeManager upgradeManager;

    public GameObject PauseMenuPanel { get => pauseMenuPanel; set => pauseMenuPanel = value; }
    public GameObject SettingsPanel { get => settingsPanel; set => settingsPanel = value; }
    public Transform StatsContainer { get => statsContainer; set => statsContainer = value; }
    public GameObject StatPrefab { get => statPrefab; set => statPrefab = value; }
    public UpgradeManager UpgradeManager { get => upgradeManager; set => upgradeManager = value; }

    private bool isPaused = false;

    private void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void Update()
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
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        RefreshStatsUI(); 
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; 
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
        Debug.Log("Wyświetlam Ustawienia (Placeholder)");
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
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

        foreach (var entry in upgradeManager.ActiveUpgrades)
        {
            GameObject newStat = Instantiate(statPrefab, statsContainer);
            
            TextMeshProUGUI textComp = newStat.GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null)
            {
                textComp.text = $"{entry.data.UpgradeName} (LVL {entry.currentLevel})";
            }
        }
    }
}