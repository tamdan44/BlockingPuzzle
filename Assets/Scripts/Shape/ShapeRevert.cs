using System.Collections.Generic;
using UnityEngine;

public class RevertShape : MonoBehaviour
{
    public static List<ShapeData> Revert = new();

    public GridSquare gridSquare;
    public Shape shape;
    public static void RemoveRevertData()
    {
        Revert.Remove(Revert[^1]);
    }
    public static void AddRevertData(ShapeData shapeData)
    {
        Revert.Add(shapeData);
        if (Revert.Count > 3 ) { Revert.Remove(Revert[0]); }
    }
    public void DoRevert()
    {
        gridSquare.Deactivate();
    }
    private void OnEnable()
    {

    }
    private void OnDisable()
    {

    }
}
