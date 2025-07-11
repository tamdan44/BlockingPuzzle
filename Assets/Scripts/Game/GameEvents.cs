using System;
using NUnit.Framework;
using UnityEngine;

public class GameEvents : MonoBehaviour
{ 
    public static Action<bool> GameOver;
    public static Action<int> AddScore;
    public static Action ResetScore;
    public static Action CheckIfShapeCanBePlaced;

    public static Action MoveShapeToStartPosition;
    public static Action SetShapeInactive;
    public static Action RequestNewShapes;
    public static Action SetShapeActive;
}
