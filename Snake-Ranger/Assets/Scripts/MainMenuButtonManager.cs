using UnityEngine;

public class MainMenuButtonManager : MonoBehaviour
{
    [SerializeField] private MainMenuManager.MainMenuButtons _buttonType;

    public void buttonClicked()
    {
        MainMenuManager._.MainMenuButtonClicked(_buttonType);

    }

}
