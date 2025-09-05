using UnityEngine;
using UnityEngine.UI;

public class WASDToggle : MonoBehaviour
{
    public Toggle keyboardToggle;
    public SnakeController snake;
    public AudioClip clickClip;

    void Start()
    {
        // default to keyboard = true if nothing saved
        bool keyboardEnabled = PlayerPrefs.GetInt("UseKeyboardControl", 1) == 1;
        if (keyboardToggle != null)
        {
            keyboardToggle.isOn = keyboardEnabled;
            keyboardToggle.onValueChanged.AddListener(OnToggleChanged);
        }

        // also apply to snake at start (in case start order differs)
        if (snake != null) snake.SetUseKeyboardControl(keyboardEnabled);
    }

    private void OnToggleChanged(bool isOn)
    {
        // persist and update snake
        var am = AudioManager.Instance;
        if (am == null) return;
        am.PlaySFX(clickClip != null ? clickClip : am.uiClick);
        PlayerPrefs.SetInt("UseKeyboardControl", isOn ? 1 : 0);
        PlayerPrefs.Save();
        if (snake != null) snake.SetUseKeyboardControl(isOn);
    }

    void OnDestroy()
    {
        if (keyboardToggle != null) keyboardToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
}
