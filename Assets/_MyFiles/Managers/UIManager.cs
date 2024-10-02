using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //might add a debug menu later
    [Header("UI Elements")]
    [SerializeField] private Slider noiseMeter;
    [SerializeField] private TextMeshProUGUI interactionText;

    [Header("Enemy Awareness Icons")]
    [SerializeField] private Sprite canSeeIcon;
    [SerializeField] private Sprite canHearIcon;

    [SerializeField] private Image enemyAwarenessImage;

    public void SetInteractionText(string textToSet) 
    {
        interactionText.text = textToSet;
        interactionText.gameObject.SetActive(true);
    }
    public void HideInteractionText()
    {
        interactionText.text = "";
        interactionText.gameObject.SetActive(false);
    }
    public void UpdateNoiseMeterUI(float valToUpdate) ///value should be between 0 - 100
    {
        valToUpdate = Mathf.Clamp(valToUpdate, noiseMeter.minValue, noiseMeter.maxValue);
        noiseMeter.value = valToUpdate;
    }
    public void UpdateEnemyAwarenessIcon(EEnemyState stateToReceive) 
    {
        if (!enemyAwarenessImage) { return; }

        if (stateToReceive == EEnemyState.chase && canSeeIcon)
        {
            enemyAwarenessImage.GetComponent<Image>().sprite = canSeeIcon;
            enemyAwarenessImage.gameObject.SetActive(true);
            return;
        }
        else if (stateToReceive == EEnemyState.curious && canHearIcon)
        {
            enemyAwarenessImage.GetComponent<Image>().sprite = canHearIcon;
            enemyAwarenessImage.gameObject.SetActive(true);
            return;
        }
        enemyAwarenessImage.gameObject.SetActive(false);
    }
}
