using UnityEngine;
using UnityEngine.SceneManagement;


public class StartMenu : MonoBehaviour
{
    public int mapSize = 8;
    
    private void Start() { StatsManager.Load(); }
    public void PlayGame()
    {
        SaveSession.isLoadRequested = false;
        PlayerPrefs.SetInt("MapSize", mapSize);
        SceneManager.LoadScene("MatchScene");
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("SaveSlotsScene"); 
    }
    public void Stats()
    {
        SceneManager.LoadScene("StatisticsScene"); 
    }


    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit requested.");
    }
}