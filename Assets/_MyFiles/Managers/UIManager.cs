using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    //build noisemeter visual and interaction text here.
    //might add a debug menu as well
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
}
