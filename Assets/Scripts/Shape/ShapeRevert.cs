using System.Collections.Generic;
using UnityEngine;
using System;

public class RevertShape : MonoBehaviour
{
    public List<List<int>> revertSquares = new();
    public List<int> newList = new();

    public void AddRevertValue(List<int> squareIndex)
    {
        newList.Clear();
        foreach (int i in squareIndex)
        {
            newList.Add(i);
        }
        revertSquares.Add(newList);
        if (revertSquares.Count > 3) revertSquares.RemoveAt(0);
    }
}
