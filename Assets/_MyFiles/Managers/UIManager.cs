using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //might add a debug menu later
    [Header("UI Elements")]
    [SerializeField] private Slider noiseMeter;
    [SerializeField] private TextMeshProUGUI interactionText;
    
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject deadMenuUI;
    private bool _bIsPaused = false;

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

        if (stateToReceive == EEnemyState.Chase && canSeeIcon)
        {
            enemyAwarenessImage.GetComponent<Image>().sprite = canSeeIcon;
            enemyAwarenessImage.gameObject.SetActive(true);
            return;
        }
        else if (stateToReceive == EEnemyState.Curious && canHearIcon)
        {
            enemyAwarenessImage.GetComponent<Image>().sprite = canHearIcon;
            enemyAwarenessImage.gameObject.SetActive(true);
            return;
        }
        enemyAwarenessImage.gameObject.SetActive(false);
    }

    public void TogglePause() 
    {
        if (_bIsPaused)
        {
            Resume();
        }
        else
        { 
            Pause();
        }
        _bIsPaused = !_bIsPaused;
    }

    public void Pause()
    {
        SetPlayerMouse(true);
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume() 
    {
        SetPlayerMouse(false);
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }
    public void RestartButton() 
    {
        Debug.Log("Restart");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void QuitButton()
    {
        Debug.Log("Exit");
        Application.Quit();
    }
    public void TriggerDeadMenu() 
    {
        SetPlayerMouse(true);
        deadMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }
    public void SetPlayerMouse(bool stateToSet) 
    {
        if (stateToSet) 
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        { 
            Cursor.lockState = CursorLockMode.Locked;
        }
        Cursor.visible = stateToSet;
    }
}
