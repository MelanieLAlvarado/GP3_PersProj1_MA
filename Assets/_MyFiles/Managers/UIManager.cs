using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //build noisemeter visual and interaction text here.
    //might add a debug menu as well
    [Header("UI Elements")]
    [SerializeField] private Slider noiseMeter;
    [SerializeField] private TextMeshProUGUI interactionText;
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
    public void UpdateNoiseMeterUI(float valToUpdate) 
    {
        Debug.Log(valToUpdate);

        noiseMeter.value = valToUpdate;
    }
}
