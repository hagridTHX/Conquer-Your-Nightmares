using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { WeaponSelection, Playing, BossFight, GameOver, Victory }
    
    [Header("Game State")]
    public GameState currentState;

    [SerializeField] private float gameTime = 0f;
    public float GameTime => gameTime;

    [Header("Difficulty Scaling")]
    [Tooltip("Percent growth in enemy stats per minute (e.g., 0.5 = +50% per minute).")]
    [SerializeField] private float difficultyIncreasePerMinute = 0.05f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentState = GameState.WeaponSelection;
    }

    void Update()
    {
        if (currentState == GameState.Playing || currentState == GameState.BossFight)
        {
            gameTime += Time.deltaTime;
        }
    }

    public void StartRun()
    {
        if (currentState == GameState.WeaponSelection)
        {
            currentState = GameState.Playing;
            Debug.Log("Gra rozpoczeta!");
        }
    }

    public float GetCurrentDifficultyMultiplier()
    {
        float minutesPlayed = gameTime / 60f;
        return 1f + (minutesPlayed * difficultyIncreasePerMinute);
    }
}