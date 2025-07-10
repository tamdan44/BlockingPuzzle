using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections;
using System.Collections.Generic;

public class MoveBlockAgent : Agent
{
    [SerializeField] Grid grid;
    [SerializeField] ShapeStorage shapeStorage;
    
    [Header("ML Agent Control")]
    [SerializeField] bool enableMLAgent = true;
    
    private bool isTrainingMode = false;
    private Coroutine currentDragCoroutine = null; // Lưu coroutine kéo thả hiện tại

    void Start()
    {
        isTrainingMode = Academy.Instance.IsCommunicatorOn;
        // Debug.Log($"[ML-Agent] Training mode: {isTrainingMode}");
        
        // Simple validation
        if (grid == null) Debug.LogError("Grid reference is null!");
        if (shapeStorage == null) Debug.LogError("ShapeStorage reference is null!");
        
        // Log grid boundaries info for debugging
        if (isTrainingMode)
        {
            StartCoroutine(LogGridInfoDelayed());
        }
    }
    
    private IEnumerator LogGridInfoDelayed()
    {
        // Wait a moment for grid to be initialized
        yield return new WaitForSeconds(1f);
        LogGridBoundariesInfo();
    }

    public override void OnEpisodeBegin()
    {
        if (!isTrainingMode) return;
        // Nếu đang kéo block thì dừng lại
        if (currentDragCoroutine != null)
        {
            StopCoroutine(currentDragCoroutine);
            currentDragCoroutine = null;
        }
        isExecutingAction = false;
        totalReward = 0f;
        consecutiveInvalidActions = 0;
        StartCoroutine(ClearAllPopups());
        if (grid != null)
        {
            grid.ClearGrid();
        }
        // Fix null shape bug: ensure all shapes have valid data after reset
        if (shapeStorage != null)
        {
            shapeStorage.RequestNewShapes();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Read grid state using reflection to avoid modifying game code
        var gridSquares = GetGridSquaresViaReflection();
        
        // Add 81 observations for grid (1.0 = empty, 0.0 = occupied)
        if (gridSquares != null && gridSquares.Count == 81)
        {
            for (int i = 0; i < 81; i++)
            {
                var gridSquare = gridSquares[i].GetComponent<GridSquare>();
                sensor.AddObservation(gridSquare.SquareOccupied ? 0f : 1f);
            }
        }
        else
        {
            // Fallback: add zeros if can't read grid
            for (int i = 0; i < 81; i++)
            {
                sensor.AddObservation(0f);
            }
        }

        // Add shape observations (3 shapes x 16 floats each, 4x4 max shape size)
        int maxShapes = 3;
        int shapeCount = 0;

        if (shapeStorage?.shapeList != null)
        {
            foreach (Shape shape in shapeStorage.shapeList)
            {
                if (shapeCount >= maxShapes) break;

                if (shape != null && shape.IsAnyOfSquareActive() && shape.currentShapeData != null)
                {
                    // Convert shape data to 4x4 array manually
                    var shapeData = GetShapeDataManually4x4(shape);
                    sensor.AddObservation(shapeData);
                    // Add rows and columns as extra info
                    sensor.AddObservation(shape.currentShapeData.rows);
                    sensor.AddObservation(shape.currentShapeData.columns);
                }
                else
                {
                    // Empty shape: 16 zeros + 2 zeros for rows/cols
                    sensor.AddObservation(new float[16]);
                    sensor.AddObservation(0f);
                    sensor.AddObservation(0f);
                }
                shapeCount++;
            }
        }

        // Fill remaining shapes with zeros
        while (shapeCount < maxShapes)
        {
            sensor.AddObservation(new float[16]);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            shapeCount++;
        }
    }

    private bool isExecutingAction = false; // Flag to prevent multiple actions at once
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        // ONLY act during training, never interfere with manual play
        if (!isTrainingMode) return;
        if (isExecutingAction) return; // Không block action, chỉ chờ đến khi xong mới nhận action tiếp theo
        int shapeIndex = actions.DiscreteActions[0];
        int gridPosition = actions.DiscreteActions[1];
        int gridRow = gridPosition / 9;
        int gridCol = gridPosition % 9;
        if (gridPosition < 0 || gridPosition >= 81)
        {
            LogAndAddReward(-0.1f, "Invalid grid position");
            RequestDecision();
            return;
        }
        if (shapeStorage?.shapeList == null || shapeIndex >= shapeStorage.shapeList.Count)
        {
            LogAndAddReward(-0.1f, "Invalid shape index");
            RequestDecision();
            return;
        }
        Shape selectedShape = shapeStorage.shapeList[shapeIndex];
        if (selectedShape == null || !selectedShape.IsAnyOfSquareActive())
        {
            LogAndAddReward(-0.1f, "Shape not available");
            RequestDecision();
            return;
        }
        isExecutingAction = true;
        currentDragCoroutine = StartCoroutine(AutomatedDragAndDrop(selectedShape, gridPosition));
    }
    
    private IEnumerator AutomatedDragAndDrop(Shape shape, int targetGridPosition)
    {
        try
        {
            // Get target grid square
            var gridSquares = GetGridSquaresViaReflection();
            if (gridSquares == null || targetGridPosition >= gridSquares.Count) 
            {
                LogAndAddReward(-0.1f, "Invalid grid position");
                yield break;
            }

            var targetGridSquare = gridSquares[targetGridPosition];
            var shapeTransform = shape.GetComponent<RectTransform>();
            var canvas = shape.GetComponentInParent<Canvas>();
            var gridSquareComponent = targetGridSquare.GetComponent<GridSquare>();

            // Store original position
            Vector3 originalPosition = shapeTransform.position;
            Vector3 originalLocalPosition = shapeTransform.localPosition;
            Vector3 originalScale = shapeTransform.localScale;

            // Create PointerEventData for the drag simulation
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            var pointerEventData = new UnityEngine.EventSystems.PointerEventData(eventSystem);

            // Step 1: Begin drag (scale up shape like real drag)
            shape.OnBeginDrag(pointerEventData);

            // Wait a frame for begin drag to take effect
            yield return new WaitForFixedUpdate();

            // Step 2: Animated movement to target position
            float moveTime = 1.0f;
            float elapsedTime = 0f;

            Vector3 startPos = shapeTransform.position;
            Vector3 targetPos = targetGridSquare.transform.position;

            while (elapsedTime < moveTime)
            {
                elapsedTime += Time.fixedDeltaTime;
                float t = elapsedTime / moveTime;
                t = Mathf.SmoothStep(0f, 1f, t);
                Vector3 currentWorldPos = Vector3.Lerp(startPos, targetPos, t);
                Vector3 screenPos = Camera.main.WorldToScreenPoint(currentWorldPos);
                pointerEventData.position = screenPos;
                shape.OnDrag(pointerEventData);
                yield return new WaitForFixedUpdate();
            }

            // Step 3: Final positioning and end drag
            Vector3 finalScreenPos = Camera.main.WorldToScreenPoint(targetPos);
            pointerEventData.position = finalScreenPos;
            shape.OnDrag(pointerEventData);

            // Wait a frame before ending drag
            // Tăng thời gian delay giữa lúc block đã tới nơi và thả
            yield return new WaitForSeconds(2.5f);
            yield return new WaitForFixedUpdate();

            // Step 4: Manually select grid squares that shape should occupy
            bool validPlacement = ManuallySelectGridSquares(shape, targetGridPosition);
            if (!validPlacement)
            {
                LogAndAddReward(-0.1f, $"Invalid placement - cannot place {shape.name} at grid[{targetGridPosition}]");
                consecutiveInvalidActions++;
                if (consecutiveInvalidActions >= maxInvalidActions)
                {
                    LogAndAddReward(-1.0f, "End episode due to too many invalid actions");
                    EndEpisode();
                }
                // Move shape back to start position
                shape.OnEndDrag(pointerEventData); 
                yield break;
            }
            // Nếu tới đây là action hợp lệ, reset đếm lỗi
            consecutiveInvalidActions = 0;

            // Step 5: End drag (this triggers placement logic)
            shape.OnEndDrag(pointerEventData);
            // Đợi thêm một chút để đảm bảo block được đặt thành công
            yield return new WaitForSeconds(1f);
            // Wait for placement to be processed
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            // Step 5: Check if placement was successful
            bool wasPlaced = !shape.IsOnStartPosition();

            // Give reward based on result
            if (wasPlaced)
            {
                LogAndAddReward(1.0f, $"SUCCESS: Shape {shape.name} placed successfully!");
                consecutiveInvalidActions = 0;
                AddReward(1.0f);
            }
            else
            {
                LogAndAddReward(-0.1f, $"FAILED: Shape {shape.name} returned to start position");
                consecutiveInvalidActions++;
                if (consecutiveInvalidActions >= maxInvalidActions)
                {
                    LogAndAddReward(-1.0f, "End episode due to too many invalid actions");
                    EndEpisode();
                }
            }

            // Check for game over
            if (grid.isGameOver)
            {
                LogAndAddReward(-1.0f, "Game over");
                StartCoroutine(HandleGameOverPopup());
                EndEpisode();
            }

            // Reset flag to allow next action
            isExecutingAction = false;
            RequestDecision();
        }
        finally
        {
            isExecutingAction = false;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = Random.Range(0, 3);
        discreteActionsOut[1] = Random.Range(0, 81);
    }

    // --- LOGGING REWARD ---
    private float totalReward = 0f;
    private int consecutiveInvalidActions = 0;
    private const int maxInvalidActions = 30; // Số lần thử liên tiếp không thành công trước khi end episode (tăng lên cho dễ)

    private void LogAndAddReward(float value, string reason)
    {
        totalReward += value;
        Debug.Log($"[AgentReward] Reward: {value}, Total: {totalReward}, InvalidCount: {consecutiveInvalidActions}");
        AddReward(value);
    }

    // Helper methods that work with game's existing systems without modifying them
    
    private System.Collections.Generic.List<GameObject> GetGridSquaresViaReflection()
    {
        // Use reflection to access private _gridSquares field without modifying Grid.cs
        var gridType = grid.GetType();
        var field = gridType.GetField("_gridSquares", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(grid) as System.Collections.Generic.List<GameObject>;
    }
    
    // Convert shape data to 4x4 array (pad 0 nếu nhỏ hơn)
    private float[] GetShapeDataManually4x4(Shape shape)
    {
        float[] shapeData = new float[16];
        if (shape.currentShapeData?.board != null)
        {
            int index = 0;
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    if (row < shape.currentShapeData.rows &&
                        col < shape.currentShapeData.columns &&
                        shape.currentShapeData.board[row]?.column != null &&
                        col < shape.currentShapeData.board[row].column.Length)
                    {
                        shapeData[index] = shape.currentShapeData.board[row].column[col] ? 1f : 0f;
                    }
                    else
                    {
                        shapeData[index] = 0f;
                    }
                    index++;
                }
            }
        }
        return shapeData;
    }

    private IEnumerator HandleGameOverPopup()
    {
        // Wait a moment for popup to appear
        yield return new WaitForSeconds(0.1f);
        
        // Try to find and dismiss game over popup
        var gameOverPopup = GameObject.Find("GameOverPopup");
        if (gameOverPopup != null && gameOverPopup.activeInHierarchy)
        {
            var tryAgainButton = gameOverPopup.GetComponentInChildren<UnityEngine.UI.Button>();
            if (tryAgainButton != null)
            {
                tryAgainButton.onClick.Invoke();
            }
            else
            {
                gameOverPopup.SetActive(false);
            }
        }
        
        // Also try alternative popup names
        var alternativePopups = new string[] { "WinLosePopup", "GameOver", "PopupGameOver" };
        foreach (var popupName in alternativePopups)
        {
            var popup = GameObject.Find(popupName);
            if (popup != null && popup.activeInHierarchy)
            {
                var button = popup.GetComponentInChildren<UnityEngine.UI.Button>();
                if (button != null)
                {
                    button.onClick.Invoke();
                }
                else
                {
                    popup.SetActive(false);
                }
                break;
            }
        }
    }
    
    private IEnumerator ClearAllPopups()
    {
        yield return new WaitForSeconds(0.05f);
        
        // List of possible popup names to clear
        var popupNames = new string[] { 
            "GameOverPopup", "WinLosePopup", "GameOver", "PopupGameOver",
            "gameOverPopup", "winLosePopup", "Popup"
        };
        
        foreach (var popupName in popupNames)
        {
            var popup = GameObject.Find(popupName);
            if (popup != null && popup.activeInHierarchy)
            {
                popup.SetActive(false);
            }
        }
        
        // Also look for any active UI buttons that might be "Try Again" buttons
        var allButtons = FindObjectsOfType<UnityEngine.UI.Button>();
        foreach (var button in allButtons)
        {
            if (button.gameObject.activeInHierarchy && 
                (button.name.ToLower().Contains("tryagain") || 
                 button.name.ToLower().Contains("try_again") ||
                 button.name.ToLower().Contains("restart")))
            {
                button.onClick.Invoke();
            }
        }
    }

    // Helper method to check if a grid position is valid
    private bool IsValidGridPosition(int gridPosition)
    {
        return gridPosition >= 0 && gridPosition < 81; // 9x9 grid = 81 squares
    }
    
    // Helper method to convert grid position to row/col
    private (int row, int col) GridPositionToRowCol(int gridPosition)
    {
        return (gridPosition / 9, gridPosition % 9);
    }
    
    // Helper method to convert row/col to grid position
    private int RowColToGridPosition(int row, int col)
    {
        return row * 9 + col;
    }
    
    // Helper method to get grid boundaries info
    private void LogGridBoundariesInfo()
    {
        var gridSquares = GetGridSquaresViaReflection();
        if (gridSquares == null || gridSquares.Count == 0) return;
        
        Vector3 minPos = gridSquares[0].transform.position;
        Vector3 maxPos = gridSquares[0].transform.position;
        
        foreach (var square in gridSquares)
        {
            Vector3 pos = square.transform.position;
            if (pos.x < minPos.x) minPos.x = pos.x;
            if (pos.y < minPos.y) minPos.y = pos.y;
            if (pos.x > maxPos.x) maxPos.x = pos.x;
            if (pos.y > maxPos.y) maxPos.y = pos.y;
        }
        
        // Debug.Log($"[ML-Agent] Grid boundaries: Min({minPos.x:F2}, {minPos.y:F2}) to Max({maxPos.x:F2}, {maxPos.y:F2})");
        // Debug.Log($"[ML-Agent] Grid size: {maxPos.x - minPos.x:F2} x {maxPos.y - minPos.y:F2}");
    }
    
    private bool ManuallySelectGridSquares(Shape shape, int targetGridPosition)
    {
        var gridSquares = GetGridSquaresViaReflection();
        if (gridSquares == null)
        {
            return false;
        }
        
        // Clear any existing selections first
        foreach (var square in gridSquares)
        {
            var gridSquareComponent = square.GetComponent<GridSquare>();
            if (gridSquareComponent != null && !gridSquareComponent.SquareOccupied)
            {
                gridSquareComponent.Selected = false;
            }
        }
        
        // Get shape data
        var shapeData = shape.currentShapeData;
        if (shapeData?.board == null)
        {
            return false;
        }
        
        // Calculate grid position (row, col) from index
        int targetRow = targetGridPosition / 9;
        int targetCol = targetGridPosition % 9;
        
        // Check if shape fits and select squares
        List<int> squaresToSelect = new List<int>();
        
        // Đảo lại shapeRow để kiểm tra nếu shape bị lật dọc khi đặt
        for (int shapeRow = 0; shapeRow < shapeData.rows; shapeRow++)
        {
            for (int shapeCol = 0; shapeCol < shapeData.columns; shapeCol++)
            {
                // Đảo chiều cột để khớp với UI render (fix lật trái-phải)
                int realRow = shapeRow;
                int realCol = shapeData.columns - 1 - shapeCol;
                if (shapeData.board[realRow].column[realCol])
                {
                    int gridRow = targetRow + shapeRow;
                    int gridCol = targetCol + shapeCol;
                    int gridIndex = gridRow * 9 + gridCol;
                    if (gridRow >= 9 || gridCol >= 9 || gridIndex >= 81)
                    {
                        return false;
                    }
                    var gridSquareComponent = gridSquares[gridIndex].GetComponent<GridSquare>();
                    if (gridSquareComponent.SquareOccupied)
                    {
                        return false;
                    }
                    squaresToSelect.Add(gridIndex);
                }
            }
        }
        
        // All squares are valid, now select them
        foreach (int gridIndex in squaresToSelect)
        {
            var gridSquareComponent = gridSquares[gridIndex].GetComponent<GridSquare>();
            gridSquareComponent.Selected = true;
            if (gridSquareComponent.hooverImage != null)
            {
                gridSquareComponent.hooverImage.gameObject.SetActive(true);
            }
        }
        return true;
    }
}
