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
        if (data == null)
        {
            Debug.LogError("RewardCard.Bind called with null reward!");
            return;
        }

        reward = data;
        onClick = onClicked;

        // Update visuals
        if (iconImage != null && data.icon != null)
            iconImage.sprite = data.icon;

        if (titleText != null)
            titleText.text = data.rewardName;

        if (descriptionText != null)
            descriptionText.text = data.description;

        // Setup button
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                Debug.Log("RewardCard clicked: " + reward.rewardName);
                onClick?.Invoke(reward);
            });
        }
        else
        {
            Debug.LogError("RewardCard has no Button reference set!");
        }
    }
}