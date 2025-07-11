using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    void Awake()
    {
        if (!Application.isEditor)
        {
            Debug.unityLogger.logEnabled = false;
        }
    }

    public void LoadScreen(string name)
    {
        SceneManager.LoadScene(name);
    }
}
