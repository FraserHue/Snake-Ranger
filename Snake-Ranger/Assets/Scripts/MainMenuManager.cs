using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    [SerializeField] private bool _debugMode;
    [SerializeField] private string _sceneToLoadAfterClickingPlay;

    [SerializeField] GameObject _MainMenuContainer;
    [SerializeField] GameObject _CreditsMenuContainer;
    [SerializeField] GameObject _SettingsMenuContainer;

    public enum MainMenuButtons { play, options, credits, quit }
    public enum CreditsButtons { back }
    public enum SettingsButtons { back }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Duplicate MainMenuManager detected — destroying extra instance.");
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        // ensure containers are in correct initial state
        if (_MainMenuContainer != null) _MainMenuContainer.SetActive(true);
        if (_CreditsMenuContainer != null) _CreditsMenuContainer.SetActive(false);
        if (_SettingsMenuContainer != null) _SettingsMenuContainer.SetActive(false);
    }

    // public methods (unchanged)
    public void OpenMenu(GameObject menuToOpen)
    {
        if (_MainMenuContainer != null) _MainMenuContainer.SetActive(menuToOpen == _MainMenuContainer);
        if (_CreditsMenuContainer != null) _CreditsMenuContainer.SetActive(menuToOpen == _CreditsMenuContainer);
        if (_SettingsMenuContainer != null) _SettingsMenuContainer.SetActive(menuToOpen == _SettingsMenuContainer);
    }

    public void PlayClicked()
    {
        if (!string.IsNullOrEmpty(_sceneToLoadAfterClickingPlay))
            SceneManager.LoadScene(_sceneToLoadAfterClickingPlay);
    }

    public void OptionsClicked() => OpenMenu(_SettingsMenuContainer);
    public void CreditsClicked() => OpenMenu(_CreditsMenuContainer);

    public void ReturnToMainMenu() => OpenMenu(_MainMenuContainer);

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
        #else
            Application.Quit();
        #endif
    }

}