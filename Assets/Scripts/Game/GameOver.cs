using UnityEngine;

public class GameOver : MonoBehaviour
{
    public GameObject gameOverPopup;
    public GameObject highScoreText;
    public GameObject gameOverText;

    void Start()
    {
        gameOverPopup.SetActive(false);
    }
    private void OnEnable()
    {
        GameEvents.GameOver += GameOverPopup;
    }

    private void OnDisable()
    {
        GameEvents.GameOver -= GameOverPopup;
    }

    void GameOverPopup(bool highScoreText){
        gameOverPopup.SetActive(true);
        gameOverText.SetActive(true);
    }
}
