using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { WeaponSelection, Playing, BossFight, GameOver, Victory }
    
    [Header("Status Gry")]
    public GameState currentState;
    public float gameTime = 0f;

    [Header("Skalowanie Trudności")]
    [Tooltip("O ile procent rosną statystyki wrogów z każdą minutą (np. 0.5 = 50% mocniejsi co minutę).")]
    public float difficultyIncreasePerMinute = 0.5f;

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