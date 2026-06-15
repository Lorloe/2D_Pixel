using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private bool _pauseTimeOnGameOver = true;

    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void TriggerGameOver()
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;

        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);
        }

        if (_pauseTimeOnGameOver)
        {
            Time.timeScale = 0f;
        }
    }
}
