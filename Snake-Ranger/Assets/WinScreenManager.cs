using UnityEngine;
using System.Collections;

public class WinScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject winScreen;
    [SerializeField] private bool pauseOnWin = true;

    public void ShowWin()
    {
        var am = FindObjectOfType<AudioManager>();
        if (am != null && am.win != null)
        {
            am.PlaySFX(am.win);
        }
        if (winScreen != null)
        {
            winScreen.SetActive(true);
            if (pauseOnWin) Time.timeScale = 0f;
        }
        else Debug.LogWarning("WinScreenManager: winScreen not assigned!");
    }

    public void ShowWinDelayed(float delay)
    {
        StartCoroutine(ShowWinDelayedCoroutine(delay));
    }

    private IEnumerator ShowWinDelayedCoroutine(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        ShowWin();
    }
    public void HideWin()
    {
        if (winScreen != null) winScreen.SetActive(false);
        Time.timeScale = 1f;
    }
}