using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager _;
    [SerializeField] private bool _debugMode;
    [SerializeField] private string _sceneToLoadAfterClickingPlay;
    public enum MainMenuButtons { play, options, credits, quit };

    public enum CreditsButtons { back };

    public enum SettingsButtons { back };

    [SerializeField] GameObject _MainMenuContainer;
    [SerializeField] GameObject _CreditsMenuContainer;
    [SerializeField] GameObject _SettingsMenuContainer;

    
    public void Awake()
    {
        if (_ == null)
        {
            _ = this;
        }
        else
        {
            Debug.LogError("There are more than 1 MainMenuManagers in the scene");
        }
    }

    private void Start()
    {
        OpenMenu(_MainMenuContainer);

    }

    public void MainMenuButtonClicked(MainMenuButtons buttonClicked)
    {
        DebugMessage("Button Clicked: " + buttonClicked.ToString());
        switch(buttonClicked)
        {
            case MainMenuButtons.play:
                PlayClicked();
                break;
            case MainMenuButtons.options:
                OptionsClicked();
                break;
            case MainMenuButtons.credits:
                CreditsClicked();
                break;
            case MainMenuButtons.quit:
                QuitGame();
                break;
            default:
                Debug.Log("Button Clicked that wasn't implemented in MainMenuManager Method");
                break;
        }
    }
    
    public void CreditsClicked()
    {
        OpenMenu(_CreditsMenuContainer);
    }

    public void OptionsClicked()
    {
        OpenMenu(_SettingsMenuContainer);
    }

    public void ReturnToMainMenu()
    {
        OpenMenu(_MainMenuContainer);
    }

    public void CreditsButtonClicked(CreditsButtons buttonClicked)
    {
        switch(buttonClicked)
        {
            case CreditsButtons.back:
                ReturnToMainMenu();
                break;
        }
    }

    public void SettingsButtonClicked(SettingsButtons buttonClicked)
    {
        switch (buttonClicked)
        {
            case SettingsButtons.back:
                ReturnToMainMenu();
                break;
        }
    }

    private void DebugMessage(string message)
    {
        if (_debugMode)
        {
            Debug.Log(message);
        }
    } 

    public void PlayClicked()
    {
        SceneManager.LoadScene(_sceneToLoadAfterClickingPlay);
    }

    public void QuitGame()
    {
        # if UNITY_EDITOR
                UnityEditor.EditorApplication.ExitPlaymode();
        #else
            Application.Quit();
        #endif
    }

    public void OpenMenu(GameObject menuToOpen)
    {
        _MainMenuContainer.SetActive(menuToOpen == _MainMenuContainer);
        _CreditsMenuContainer.SetActive(menuToOpen == _CreditsMenuContainer);
        _SettingsMenuContainer.SetActive(menuToOpen == _SettingsMenuContainer);
    }
}
