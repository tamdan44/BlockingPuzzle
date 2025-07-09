using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Struct để lưu thông tin về một nước đi TỐT NHẤT
public struct BestMoveInfo
{
    public Shape shapeToPlace;
    public List<int> squareIndices;
    public float score;
}

public class GameAI : MonoBehaviour
{
    public GameGrid gameGrid;
    public ShapeStorage shapeStorage;
    private bool isAIThinking = false;

    [Header("AI Timing Settings")]
    public float delayBetweenPlacements = 1.0f;
    public float delayBetweenTurns = 2.0f;

    private void Start()
    {
        StartCoroutine(AutoPlay());
    }

    private IEnumerator AutoPlay()
    {
        while (true)
        {
            if (!isAIThinking && !gameGrid.isGameOver)
            {
                yield return StartCoroutine(FindAndExecuteBestMove());
            }
            yield return new WaitForSeconds(delayBetweenTurns);
        }
    }

    private IEnumerator FindAndExecuteBestMove()
    {
        isAIThinking = true;
        Debug.Log("--- 🤖 AI BẮT ĐẦU SUY NGHĨ ---");

        int placedCount = 0;

        foreach (var shape in shapeStorage.shapeList)
        {
            // Bỏ qua shape không còn ô nào
            if (!shape.IsAnyOfSquareActive())
            {
                Debug.Log($"[⏩ BỎ QUA] Shape {shape.name} không còn ô nào hoạt động");
                continue;
            }

            // Tìm tất cả vị trí có thể đặt cho shape
            List<List<int>> placements = FindAllPossiblePlacementsForShape(shape);
            if (placements.Count == 0)
            {
                Debug.Log($"[❌ KHÔNG ĐẶT ĐƯỢC] Shape {shape.name} không có vị trí hợp lệ");
                continue;
            }

            // Lấy vị trí đầu tiên (hoặc sau này có thể chọn vị trí tốt nhất)
            var placement = placements[0];

            // Giả lập hành vi người chơi
            shapeStorage.SetCurrentSelectedShape(shape);

            // Bỏ chọn toàn bộ trước đó
            foreach (var square in gameGrid.GetComponentsInChildren<GridSquare>())
            {
                square.Selected = false;
                if (placement.Contains(square.SquareIndex))
                {
                    square.Selected = true;
                }
            }

            Debug.Log($"[✅ ĐẶT] Shape {shape.name} tại vị trí: {string.Join(", ", placement)}");

            // Gọi đúng event để xử lý như người chơi
            GameEvents.CheckIfShapeCanBePlaced();

            placedCount++;
            yield return new WaitForSeconds(delayBetweenPlacements);
        }

        // Nếu dùng hết 3 shape hoặc không còn shape nào có thể đặt
        if (placedCount == 3 || AllShapesUsed())
        {
            Debug.Log("🆕 Xin 3 shape mới");
            GameEvents.RequestNewShapes();
        }
        else
        {
            Debug.Log("🔁 Còn shape chưa dùng được, không xin shape mới");
        }

        isAIThinking = false;
        Debug.Log("--- 🧠 AI SUY NGHĨ XONG ---\n\n");
    }

    private bool AllShapesUsed()
    {
        foreach (var shape in shapeStorage.shapeList)
        {
            if (shape.IsOnStartPosition() && shape.IsAnyOfSquareActive())
            {
                var placements = FindAllPossiblePlacementsForShape(shape);
                if (placements.Count > 0)
                    return false;
            }
        }
        return true;
    }

    private List<List<int>> FindAllPossiblePlacementsForShape(Shape shape)
    {
        var possiblePlacements = new List<List<int>>();
        var shapeData = shape.currentShapeData;
        List<int> filledIndices = new List<int>();
        int squareIndex = 0;

        for (int r = 0; r < shapeData.rows; r++)
        {
            for (int c = 0; c < shapeData.columns; c++)
            {
                if (shapeData.board[r].column[c]) filledIndices.Add(squareIndex);
                squareIndex++;
            }
        }

        var combos = gameGrid.GetAllSquaresCombinations(shapeData.rows, shapeData.columns);
        foreach (var combo in combos)
        {
            bool canPlace = true;
            List<int> indices = new List<int>();
            foreach (var i in filledIndices)
            {
                int index = combo[i];
                if (gameGrid.GetGridSquareAtIndex(index).SquareOccupied)
                {
                    canPlace = false;
                    break;
                }
                indices.Add(index);
            }
            if (canPlace) possiblePlacements.Add(indices);
        }
        return possiblePlacements;
    }

    private float EvaluatePlacement(List<int> placementIndices)
    {
        float score = 0f;
        bool[] simGrid = new bool[gameGrid.columns * gameGrid.rows];
        foreach (var square in gameGrid.GetComponentsInChildren<GridSquare>())
        {
            simGrid[square.SquareIndex] = square.SquareOccupied;
        }
        foreach (var index in placementIndices)
        {
            simGrid[index] = true;
        }

        var lineIndicator = gameGrid.GetComponent<LineIndicator>();
        int linesCleared = 0;
        List<int[]> allLines = new List<int[]>();
        allLines.AddRange(lineIndicator.line_data);
        allLines.AddRange(lineIndicator.square_data);
        for (int i = 0; i < 9; i++) allLines.Add(lineIndicator.GetVerticalLine(i));

        foreach (var line in allLines)
        {
            bool complete = true;
            foreach (var idx in line)
            {
                if (!simGrid[idx]) { complete = false; break; }
            }
            if (complete) linesCleared++;
        }

        score += linesCleared * 100;

        int adjacent = 0;
        foreach (var index in placementIndices)
        {
            int row = index / gameGrid.columns;
            int col = index % gameGrid.columns;
            if (row > 0 && simGrid[index - gameGrid.columns]) adjacent++;
            if (row < gameGrid.rows - 1 && simGrid[index + gameGrid.columns]) adjacent++;
            if (col > 0 && simGrid[index - 1]) adjacent++;
            if (col < gameGrid.columns - 1 && simGrid[index + 1]) adjacent++;
        }
        score += adjacent * 5;

        return score;
    }
} 