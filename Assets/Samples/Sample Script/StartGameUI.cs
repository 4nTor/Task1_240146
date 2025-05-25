using UnityEngine;

public class StartGameUI : MonoBehaviour
{
    public GameObject startUI;
    public GameObject heartUIContainer;

    public void StartGame()
    {
        if (startUI != null)
            startUI.SetActive(false);
        
        if (heartUIContainer != null)
            heartUIContainer.SetActive(true);

        Time.timeScale = 1f;
    }

    void Start()
    {
        Time.timeScale = 0f;
        if (startUI != null)
            startUI.SetActive(true);
    }
}