using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [SerializeField] private int currentScore = 0;
    
    public int CurrentScore => currentScore;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
    
    private void AddScore(int score)
    {
        currentScore += score;
        Debug.Log($"Score added: {score}, Total: {currentScore}");
    }
    
    private void ResetScore()
    {
        currentScore = 0;
        Debug.Log("Score reset to 0");
    }
}
