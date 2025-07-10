using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeStorage : MonoBehaviour
{
    public List<ShapeData> shapeDataList;
    public List<Shape> shapeList;
    void Start()
    {
        Debug.Log($"[ShapeStorage] Starting with {shapeList.Count} shapes in list");
        for (int i = 0; i < shapeList.Count; i++)
        {
            Debug.Log($"[ShapeStorage] Shape {i}: {(shapeList[i] != null ? shapeList[i].name : "NULL")}");
        }
        
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
        Debug.Log($"[ShapeStorage] RequestNewShapes called for {shapeList.Count} shapes");
        
        foreach (Shape shape in shapeList)
        {
            if (shape != null)
            {
                int shapeIndex = UnityEngine.Random.Range(0, shapeDataList.Count);
                shape.RequestNewShape(shapeDataList[shapeIndex]);
                shape.shapeIndex = shapeIndex;
                Debug.Log($"[ShapeStorage] Assigned ShapeData {shapeIndex} ({shapeDataList[shapeIndex].name}) to shape {shape.name}");
            }
            else
            {
                Debug.LogError("[ShapeStorage] Found null shape in shapeList!");
            }
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
