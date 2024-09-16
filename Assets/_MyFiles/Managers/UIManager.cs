using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //might add a debug menu later
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
    public void UpdateNoiseMeterUI(float valToUpdate) //value should be between 0 - 100
    {
        //Debug.Log(valToUpdate);
        noiseMeter.value = valToUpdate;
    }
}
