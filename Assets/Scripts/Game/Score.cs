using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public Text scoreText;
    int currentScore;
    void Start()
    {
        currentScore = 0;
        UpdateScore();
    }
    private void OnEnable()
    {
        GameEvents.AddScore += AddScore;
    }

    private void OnDisable()
    {
        GameEvents.AddScore -= AddScore;
    }

    void AddScore(int score)
    {
        currentScore += score;
        UpdateScore();
    }
    void UpdateScore()
    {
        scoreText.text = currentScore.ToString();
    }


}
