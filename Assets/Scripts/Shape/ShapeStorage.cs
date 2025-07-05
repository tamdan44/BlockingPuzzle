using System.Collections;
using System.Collections.Generic;
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
        foreach (Shape shape in shapeList)
        {
            int shapeIndex = UnityEngine.Random.Range(0, shapeDataList.Count);
            shape.RequestNewShape(shapeDataList[shapeIndex]);
            shape.shapeIndex = shapeIndex;
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
