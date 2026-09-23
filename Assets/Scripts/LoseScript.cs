using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseScript : MonoBehaviour
{
    public void LoseGame()
    {
        //Scene currentScene = SceneManager.GetCurrentScene();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
