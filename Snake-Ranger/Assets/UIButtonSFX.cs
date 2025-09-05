using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSFX : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public AudioClip hoverClip;
    public AudioClip clickClip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        var am = AudioManager.Instance;
        if (am == null) return;
        am.PlaySFX(hoverClip != null ? hoverClip : am.uiHover);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var am = AudioManager.Instance;
        if (am == null) return;
        am.PlaySFX(clickClip != null ? clickClip : am.uiClick);
    }
}