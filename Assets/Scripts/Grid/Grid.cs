using System.Collections.Generic;
using UnityEngine;
public class Grid : MonoBehaviour
{
    public RevertShape revertShape;
    public ShapeStorage shapeStorage;
    public int columns = 0;
    public int rows = 0;
    public GameObject gridSquare;
    public Vector2 startPosition = new(0f, 0f);
    public float squareScale = .5f;
    public float everySquareOffset = 0f;
    public float squaresGap = .1f;

    private Vector2 _offset = new(0f, 0f);
    private LineIndicator _lineIndicator;
    private List<GameObject> _gridSquares = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _lineIndicator = GetComponent<LineIndicator>();
        CreateGrid();
    }

    private void OnEnable()
    {
        GameEvents.CheckIfShapeCanBePlaced += CheckIfShapeCanBePlaced;
    }

    private void OnDisable()
    {
        GameEvents.CheckIfShapeCanBePlaced -= CheckIfShapeCanBePlaced;
    }

    private void CreateGrid()
    {
        SpawnGridSquares();
        SetGridSquaresPositions();
    }

    private void SpawnGridSquares()
    {
        int square_index = 0;

        for (var row = 0; row < rows; ++row)
        {
            for (var column = 0; column < columns; ++column)
            {
                _gridSquares.Add(Instantiate(gridSquare) as GameObject);
                _gridSquares[^1].transform.SetParent(this.transform);
                _gridSquares[^1].transform.localScale = new Vector3(squareScale, squareScale, 1);
                _gridSquares[^1].GetComponent<GridSquare>().SetImage(_lineIndicator.GetGridSquareIndex(square_index) % 2 == 0);
                _gridSquares[^1].GetComponent<GridSquare>().SquareIndex = square_index;
                square_index++;
            }
        }
    }

    // this entire function can be shorten into a few lines, by setting the position as u spawn each squares, ep4 
    private void SetGridSquaresPositions()
    {
        int column_number = 0;
        int row_number = 0;
        Vector2 square_gap_number = new(0f, 0f);
        bool row_moved = false;

        var square_rect = _gridSquares[0].GetComponent<RectTransform>();

        // space each square take
        _offset.x = square_rect.rect.width * square_rect.transform.localScale.x + everySquareOffset;
        _offset.y = square_rect.rect.height * square_rect.transform.localScale.y + everySquareOffset;
        
        foreach (GameObject square in _gridSquares)
        {
            if (column_number + 1 > columns)
            {
                square_gap_number.x = 0;
                // go to next row
                column_number = 0;
                row_number++;
                row_moved = false;
            }
            var pos_x_offset = _offset.x * column_number + (square_gap_number.x * squaresGap);
            var pos_y_offset = _offset.y * row_number + (square_gap_number.y * squaresGap);

            if (column_number > 0 && column_number % 3 == 0)
            {
                square_gap_number.x++;
                pos_x_offset += squaresGap;
            }

            if (row_number > 0 && row_number % 3 == 0 && row_moved == false)
            {
                square_gap_number.y++;
                row_moved=true;
                pos_y_offset += squaresGap;
            }

            square.GetComponent<RectTransform>().anchoredPosition = new Vector2(startPosition.x + pos_x_offset, startPosition.y - pos_y_offset);
            square.GetComponent<RectTransform>().localPosition = new Vector3(startPosition.x + pos_x_offset, startPosition.y - pos_y_offset, 0.0f);
            column_number++;
        }
    }

    // check if shape can be placed, if it can, place it on the grid, check and add scores
    public List<int> squareIndices = new();

    private void CheckIfShapeCanBePlaced()
    {
        squareIndices.Clear();
        foreach (var square in _gridSquares)
        {
            var gridSquare = square.GetComponent<GridSquare>();
            if (gridSquare.Selected && !gridSquare.SquareOccupied)
            {
                squareIndices.Add(gridSquare.SquareIndex);
                gridSquare.Selected = false;
            }
        }
        var currentSelectedShape = shapeStorage.GetCurrentSelectedShape();
        if (currentSelectedShape == null) return; //there's no selected shape
        if (currentSelectedShape.TotalSquareNumber == squareIndices.Count)
        {
            foreach (int i in squareIndices)
            {
                _gridSquares[i].GetComponent<GridSquare>().PlaceShapeOnBoard();
            }
            revertShape.AddRevertValue(squareIndices);

            int shapeLeft = 0;
            foreach(Shape shape in shapeStorage.shapeList)
            {
                if(shape.IsOnStartPosition() && shape.IsAnyOfSquareActive())
                    shapeLeft++;
            }

            if(shapeLeft == 0) {
                GameEvents.RequestNewShapes();
            } else {
                GameEvents.SetShapeInactive();
            }
            CheckIfCompleted();
            
        } else
        {
            GameEvents.MoveShapeToStartPosition();
        }
    }

    // get all lines and squares data, clear completed ones, play animation, add score
    void CheckIfCompleted()
    {
        List<int[]> clearData = new();

        //rows
        foreach(var line in _lineIndicator.line_data){
            clearData.Add(line);
        }
        //cols
        for(int i=0; i<9; i++){
            clearData.Add(_lineIndicator.GetVerticalLine(i));
        }
        //squares
        foreach(var line in _lineIndicator.square_data){
            clearData.Add(line);
        }

        int numCompleted = ClearAndGetCompleted(clearData);

        if(numCompleted > 2){
            //TODO
        }

        int totalScore = 10 * numCompleted;
        GameEvents.AddScore(totalScore);
        CheckIfPlayerLost(GetShapeStorage());
    }

    // clear successful lines and squares, return the number of clear
    private int ClearAndGetCompleted(List<int[]> data)
    {
        int numCompleted = 0;
        List<int[]> linesCompleted = new();

        foreach(int[] line in data) {

            bool completed = true;

            foreach(int i in line) {
                var comp = _gridSquares[i].GetComponent<GridSquare>();
                if(!comp.SquareOccupied) {
                    completed = false;
                }
            }
            if(completed) {
                numCompleted++;
                linesCompleted.Add(line);
            }
        }

        foreach(int[] line in linesCompleted) { 
            foreach(int i in line) {
                var comp = _gridSquares[i].GetComponent<GridSquare>();
                comp.Deactivate();
                comp.ClearOccupied();
            }
        }
        return numCompleted;
    }

    private ShapeStorage GetShapeStorage()
    {
        return shapeStorage;
    }

    private void CheckIfPlayerLost(ShapeStorage shapeStorage)
    {
        int validShapes = 0;

        for(int index = 0; index < shapeStorage.shapeList.Count; index++) {
            var isShapeActive = shapeStorage.shapeList[index].IsAnyOfSquareActive();
            if(CheckIfShapeCanBePlaced(shapeStorage.shapeList[index]) && isShapeActive){
                shapeStorage.shapeList[index].ActivateShape();
                validShapes++;
            }
        }
        if(validShapes == 0){
            GameEvents.GameOver(false);
        }
    }

    private bool CheckIfShapeCanBePlaced(Shape currentShape)
    {
        var currentShapeData = currentShape.currentShapeData;
        var shapeCols = currentShapeData.columns;
        var shapeRows = currentShapeData.rows;

        // all indexes of filled squares
        List<int> filledUpSquaresIndices = new();
        var squareIndex = 0;

        for(var row = 0; row < shapeRows; row++){
            for(var col = 0; col < shapeCols; col++){
                if(currentShapeData.board[row].column[col]){
                    filledUpSquaresIndices.Add(squareIndex);
                }
                squareIndex++;
            }
        }

        if(currentShape.TotalSquareNumber != filledUpSquaresIndices.Count){
            Debug.LogError("Number of filled up shape are not the same");
        }

        var squareList = GetAllSquaresCombinations(shapeRows, shapeCols);
        bool canBePlaced = false;

        foreach(int[] number in squareList) {
            bool shapeCanBePlaced = true;
            foreach(int squareIndexToCheck in filledUpSquaresIndices){
                var comp = _gridSquares[number[squareIndexToCheck]].GetComponent<GridSquare>();
                if(comp.SquareOccupied) {
                    shapeCanBePlaced = false;
                }
            }

            if(shapeCanBePlaced) {
                canBePlaced = true;
            }
        }
        return canBePlaced;
    }

    private List<int[]> GetAllSquaresCombinations(int rows, int cols){
        var squareList = new List<int[]>();
        var lastRowIndex = 0;
        var lastColIndex = 0;

        int safeIndex = 0;

        while(lastRowIndex + (rows-1) < 9){
            var rowData = new List<int>();

            for(int rowIndex = lastRowIndex; rowIndex < lastRowIndex + rows; rowIndex++){
                for(int colIndex = lastColIndex; colIndex < lastColIndex + cols; colIndex++){
                    rowData.Add(_lineIndicator.line_data[rowIndex][colIndex]);
                }
            }

            squareList.Add(rowData.ToArray());
            lastColIndex++;
            if(lastColIndex + (columns-1) >= 9){
                lastRowIndex++;
                lastColIndex=0;
            }

            safeIndex++;
            if(safeIndex>100){
                break;
            }
        }
        return squareList;
    }
    public int undoTimes;
    public void Revert()
    {
        undoTimes -= 1;
        if (undoTimes > 0)
        {
            
            foreach (int i in revertShape.revertSquares[^1])
            {
                _gridSquares[i].GetComponent<GridSquare>().ClearOccupied();
                _gridSquares[i].GetComponent<GridSquare>().Deactivate();
                GameEvents.MoveShapeToStartPosition();
                GameEvents.SetShapeActive();
            }
            revertShape.revertSquares.RemoveAt(revertShape.revertSquares.Count - 1);
        }
        if (undoTimes == 0)
        {
            Debug.Log("Cannot undo");
            undoTimes = 0;
        }
    }
}
