using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class RewardCard : MonoBehaviour
{
    public Button button;
    public Image iconImage;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    private Reward reward;
    private UnityAction<Reward> onClick;

    public void Bind(Reward data, UnityAction<Reward> onClicked)
    {
        reward = data;
        onClick = onClicked;

        if (iconImage) iconImage.sprite = data.icon;
        if (titleText) titleText.text = data.rewardName;
        if (descriptionText) descriptionText.text = data.description;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(reward));
    }
}