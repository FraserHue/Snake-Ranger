using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }
    public GameObject gameOverPanel;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        //var am = FindObjectOfType<AudioManager>();
        //if (am != null && am.death != null)
        //{
        //    am.PlaySFX(am.death);
        //}
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
    }

    public void Restart()
    {
        WaveSpawner.ResetGlobalCounters();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        WaveSpawner.ResetGlobalCounters();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
}
