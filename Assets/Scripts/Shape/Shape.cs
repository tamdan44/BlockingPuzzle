using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Shape : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject squareShapeImage;
    public Vector3 shapeSelectedScale;
    public Vector2 offset = new(0f, 700f);

    [HideInInspector]
    public int TotalSquareNumber {get; set;}
    public ShapeData currentShapeData;

    private Canvas _canvas;
    private RectTransform _transform;
    private List<GameObject> _currentSquares = new();
    private Vector3 _shapeStartScale;
    private Vector3 _startPosition;
    private bool _shapeActive = true;
    private bool _shapeDraggable = true;

    public void Awake()
    {   //no list, scale, active get component -> no need?
        _canvas = GetComponentInParent<Canvas>();
        _transform = GetComponent<RectTransform>();
        _shapeStartScale = GetComponent<RectTransform>().localScale;
        _startPosition =_transform.localPosition;
        _shapeDraggable = true;
    }

    private void OnEnable() // -> GameEvents.cs
    {
        GameEvents.MoveShapeToStartPosition += MoveShapeToStartPosition;
        GameEvents.SetShapeInactive += SetShapeInactive;
    }
    private void OnDisable() 
    {
        GameEvents.MoveShapeToStartPosition -= MoveShapeToStartPosition;
        GameEvents.SetShapeInactive -= SetShapeInactive;
    }

    public bool IsOnStartPosition() //returns true if the shape is on the _startPosition
    {
        return _transform.localPosition == _startPosition;
    }
    private void MoveShapeToStartPosition() //change the shape position to the _startPosition
    {
        _transform.transform.localPosition = _startPosition;
    }

    public bool IsAnyOfSquareActive()   //returns true if at least 1 object is active
    {
        foreach (var square in _currentSquares){
            if (square.activeSelf){
                return true;
            }
        }
        return false;
    }
    //currently reading
    public void SetShapeInactive()
    {   //if not on start position and still active -> deactivate the object (each square)
        if(!IsOnStartPosition() && IsAnyOfSquareActive())
            foreach (var square in _currentSquares)
            {
                square.SetActive(false);
            }
    }
    public void ActivateShape()
    {
        if(!_shapeActive)
        {
            foreach(var square in _currentSquares){
                square.GetComponent<ShapeSquare>().ActivateSquare();
            }
        }
        _shapeActive = true;
    }
    public void RequestNewShape(ShapeData shapeData)
    {
        _transform.localPosition = _startPosition;
        CreateShape(shapeData);
    }
    public List<ShapeData> newShapeData;
    public void CreateShape(ShapeData shapeData)
    {
        currentShapeData = shapeData;
        TotalSquareNumber = GetNumberOfSquares(shapeData);
        newShapeData.Add(shapeData);    //add the shape to the list to transfer to ShapeRevert
        if (newShapeData.Count > 1) { newShapeData.Remove(newShapeData[0]); }

        while (_currentSquares.Count < TotalSquareNumber)
        {   //ensures enough squares are instantiated -> to represent the current shape
            _currentSquares.Add(Instantiate(squareShapeImage, transform) as GameObject);
        }
        foreach (GameObject square in _currentSquares)
        {   //reset the position and deactivate all the shape
            square.transform.position = Vector3.zero;
            square.SetActive(false);
        }
        //calculate the square spacing
        var squareRect = squareShapeImage.GetComponent<RectTransform>();
        Vector2 moveDistance = new(squareRect.rect.width * squareRect.localScale.x, squareRect.rect.height * squareRect.localScale.y);
        int currentIndexInList  = 0;

        // set position to form final shapes
        for (int row = 0; row < shapeData.rows; row++)
        {   //loops through the rows and columns to position and activate the squares
            for (int column = 0; column < shapeData.columns; column++)
            {
                if (shapeData.board[row].column[column])
                {
                    _currentSquares[currentIndexInList].SetActive(true);
                    _currentSquares[currentIndexInList].GetComponent<RectTransform>().localPosition = 
                        new Vector2(GetXPositionForShapeSquare(shapeData, column, moveDistance), GetYPositionForShapeSquare(shapeData, row, moveDistance));
                    currentIndexInList++;
                }
            }
        }
    }

    private float GetXPositionForShapeSquare(ShapeData shapeData, int column, Vector2 moveDistance)
    {
        float shiftOnX = 0f;
        if (shapeData.columns > 1)
        {
            float startXPos;
            if (shapeData.columns % 2 != 0)
            {
                startXPos = (shapeData.columns / 2) * moveDistance.x;
            } else
            {
                startXPos = ((shapeData.columns / 2) - 1) * moveDistance.x + moveDistance.x / 2;
            }
            shiftOnX = startXPos - column * moveDistance.x;
        }
        return shiftOnX;
    }
    private float GetYPositionForShapeSquare(ShapeData shapeData, int row, Vector2 moveDistance)
    {
        float shiftOnY = 0f;
        if (shapeData.rows > 1)
        {
            float startYPos;
            if (shapeData.rows % 2 != 0)
            {
                startYPos = (shapeData.rows / 2) * moveDistance.y;
            } else
            {
                startYPos = ((shapeData.rows / 2) - 1) * moveDistance.y + moveDistance.y / 2;
            }
            shiftOnY = startYPos - row * moveDistance.y;
        }
        return shiftOnY;
    }

    private int GetNumberOfSquares(ShapeData shapeData)
    {
        int number = 0; //check the rows -> column -> active -> add 1 digit

        foreach (var rowData in shapeData.board)
        {
            foreach (bool active in rowData.column)
            {
                if(active)
                {
                    number++;
                }
            }
        }
        return number;
    }
//this whole thing is to move the object and everything including them
    public void OnPointerClick(PointerEventData eventData) { }  //what is its use
    public void OnPointerUp(PointerEventData eventData) { }
    public void OnPointerDown(PointerEventData eventData) { }

    public void OnBeginDrag(PointerEventData eventData)
    {   //set the scale and the position relative to the parent
        GetComponent<RectTransform>().localScale = shapeSelectedScale;
        _transform.anchorMin = new Vector2(0.5f, 0.5f);
        _transform.anchorMax = new Vector2(0.5f, 0.5f);
        _transform.pivot = new Vector2(0.5f, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {   //can use simplier code if not in world space, less precision ->_tranform.GetComponent<...
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform,
            eventData.position, Camera.main, out Vector2 pos);  //convert the point -> local position
        _transform.localPosition = pos + offset; //move the UI to that pos (+ offset) -> not jump to pos
    }
    public Grid grid;
    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<RectTransform>().localScale = _shapeStartScale;    //change the shape scale
        RevertShape.AddRevertData(currentShapeData);
        GameEvents.CheckIfShapeCanBePlaced();   //check if it can be placed down -> GameEvents.cs
    }
}
