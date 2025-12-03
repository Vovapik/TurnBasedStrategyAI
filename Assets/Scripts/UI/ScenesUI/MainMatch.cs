using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMatch : MonoBehaviour
{
    public GameObject savePanel;

    public void SaveGame()
    {
        savePanel.SetActive(true);
    }

    public void ExitToMenu()
    {
        SceneManager.LoadScene("StartMenuScene");
    }
}
