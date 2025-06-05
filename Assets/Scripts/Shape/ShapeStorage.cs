using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShapeStorage : MonoBehaviour
{
    public List<ShapeData> shapeDataList;
    public List<Shape> shapeList;
    void Start()
    {
        GameEvents.RequestNewShapes();
    }

    void OnEnable()
    {
        GameEvents.RequestNewShapes += RequestNewShapes;
    }

    void OnDisable()
    {
        GameEvents.RequestNewShapes -= RequestNewShapes;
    }
    public void RequestNewShapes()
    {
        foreach (var shape in shapeList)
        {
            int shapeIndex = Random.Range(0, shapeDataList.Count);
            shape.RequestNewShape(shapeDataList[shapeIndex]);
        }
    }

    public Shape GetCurrentSelectedShape()
    {
        foreach (var shape in shapeList)
        {
            if(!shape.IsOnStartPosition() && shape.IsAnyOfSquareActive())
                return shape;
        }

        Debug.LogError("There is no shape selected!");
        return null;
    }
}
