using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeStorage : MonoBehaviour
{
    public List<ShapeData> shapeDataList;
    public List<Shape> shapeList;

    // ✅ Biến để lưu shape đang được chọn
    public Shape currentSelectedShape;

    void Start()
    {
        GameEvents.RequestNewShapes?.Invoke();
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

        // Reset lựa chọn khi tạo shape mới
        currentSelectedShape = null;
    }

    public Shape GetCurrentSelectedShape()
    {
        if (currentSelectedShape != null)
            return currentSelectedShape;

        foreach (var shape in shapeList)
        {
            if (!shape.IsOnStartPosition() && shape.IsAnyOfSquareActive())
                return shape;
        }

        Debug.LogError("There is no shape selected!");
        return null;
    }

    // ✅ Hàm cho AI chọn shape cụ thể
    public void SetCurrentSelectedShape(Shape shape)
    {
        currentSelectedShape = shape;
        Debug.Log("✅ Shape đã được AI chọn: " + shape.name);
    }
}
