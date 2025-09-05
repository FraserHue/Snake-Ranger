using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI containers")]
    [SerializeField] private GameObject settingsMenuContainer;
    [SerializeField] private GameObject pauseMenuContainer;

    [Header("Settings")]
    public static bool isPaused;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ReinitializeUIState();
    }

    private void ReinitializeUIState()
    {

        Time.timeScale = 1f;
        isPaused = false;

        if (pauseMenuContainer == null)
        {
            var pm = GameObject.Find("PauseMenu"); 
            if (pm != null) pauseMenuContainer = pm;
        }
        if (settingsMenuContainer == null)
        {
            var sm = GameObject.Find("SettingsMenuContainer"); 
            if (sm != null) settingsMenuContainer = sm;
        }

        if (pauseMenuContainer) pauseMenuContainer.SetActive(false);
        if (settingsMenuContainer) settingsMenuContainer.SetActive(false);

        if (EventSystem.current == null)
        {
            var esGO = new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));

        }

        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (pauseMenuContainer) pauseMenuContainer.SetActive(false);
        if (settingsMenuContainer) settingsMenuContainer.SetActive(false);
        isPaused = false;
        ReinitializeUIState();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
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
        if (pauseMenuContainer) pauseMenuContainer.SetActive(true);
        if (settingsMenuContainer) settingsMenuContainer.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void ResumeGame()
    {
        if (pauseMenuContainer) pauseMenuContainer.SetActive(false);
        if (settingsMenuContainer) settingsMenuContainer.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void OptionsClicked()
    {
        OpenMenu(settingsMenuContainer);
    }

    public void ReturnToPauseMenu()
    {
        OpenMenu(pauseMenuContainer);
    }

    public void Restart()
    {
        WaveSpawner.ResetGlobalCounters();
        Time.timeScale = 1f;
        isPaused = false;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OpenMenu(GameObject menuToOpen)
    {
        if (pauseMenuContainer != null) pauseMenuContainer.SetActive(menuToOpen == pauseMenuContainer);
        if (settingsMenuContainer != null) settingsMenuContainer.SetActive(menuToOpen == settingsMenuContainer);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void GoToMainMenu()
    {
        WaveSpawner.ResetGlobalCounters();
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("Main Menu");
    }

    public void QuitGame()
    {
        WaveSpawner.ResetGlobalCounters();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
        #else
                    Application.Quit();
        #endif
    }
}
