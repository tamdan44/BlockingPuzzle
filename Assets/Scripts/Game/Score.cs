using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public Text scoreText;
    public int currentScore;
    void Start()
    {
        currentScore = 0;
        UpdateScore();
    }
    private void OnEnable()
    {
        GameEvents.AddScore += AddScore;
        GameEvents.ResetScore += ResetScore;
    }

    private void OnDisable()
    {
        GameEvents.AddScore -= AddScore;
        GameEvents.ResetScore -= ResetScore;
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

    void ResetScore()
    {
        currentScore -= currentScore;
        UpdateScore();
    }


}
