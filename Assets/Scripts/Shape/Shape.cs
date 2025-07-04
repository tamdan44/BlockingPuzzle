using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Shape : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject squareShapeImage;
    public Vector3 shapeSelectedScale;
    public Vector2 offset = new Vector2(0f, 700f);

    [HideInInspector]
    public int shapeIndex {get; set;}
    public int TotalSquareNumber { get; set; }
    public ShapeData currentShapeData;
    private List<GameObject> _currentSquares = new List<GameObject>();
    private Vector3 _shapeStartScale;
    private RectTransform _transform;
    private bool _shapeDraggable = true;
    private Canvas _canvas;
    private Vector3 _startPosition;
    private bool _shapeActive = true;

    public void Awake()
    {
        _shapeStartScale = this.GetComponent<RectTransform>().localScale;
        _transform = this.GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _shapeDraggable = true;
        _startPosition =_transform.localPosition;
    }

    void Start()
    {
        
    }

    public float[] GetCurrentShapeDataSquares()
    {
        float[,] shapeDataArr = new float[3,3];

        for (int x = 0; x < currentShapeData.board.Length; x++)
        {
            for (int y = 0; y < currentShapeData.board[x].column.Length; y++)
            {
                shapeDataArr[x, y] = currentShapeData.board[x].column[y] ? 1f : 0f;
            }
        }

        return shapeDataArr.Cast<float>().ToArray();
    }

    private void OnEnable()
    {
        GameEvents.MoveShapeToStartPosition += MoveShapeToStartPosition;
        GameEvents.SetShapeInactive += SetShapeInactive;
    }

    private void OnDisable() 
    {
        GameEvents.MoveShapeToStartPosition -= MoveShapeToStartPosition;
        GameEvents.SetShapeInactive -= SetShapeInactive;
    }

    public bool IsOnStartPosition()
    {
        return _transform.localPosition == _startPosition;
    }
    private void MoveShapeToStartPosition()
    {
        _transform.transform.localPosition = _startPosition;
    }
    public bool IsAnyOfSquareActive()
    {
        foreach (var square in _currentSquares)
        {
            if (square.gameObject.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    public void SetShapeInactive()
    {
        if(!IsOnStartPosition() && IsAnyOfSquareActive())
            foreach (var square in _currentSquares)
            {
                square.gameObject.SetActive(false);
            }
    }

    public void ActivateShape(){
        if(!_shapeActive){
            foreach(var square in _currentSquares){
                square?.GetComponent<ShapeSquare>().ActivateSquare();
            }
        }
        _shapeActive = true;
    }

    public void RequestNewShape(ShapeData shapeData)
    {
        _transform.localPosition = _startPosition;
        CreateShape(shapeData);
    }
    public void CreateShape(ShapeData shapeData)
    {
        currentShapeData = shapeData;
        TotalSquareNumber = GetNumberOfSquares(shapeData);

        while (_currentSquares.Count <= TotalSquareNumber)
        {
            _currentSquares.Add(Instantiate(squareShapeImage, transform) as GameObject);
        }
        foreach (GameObject square in _currentSquares)
        {
            square.gameObject.transform.position = Vector3.zero;
            square.gameObject.SetActive(false);
        }

        var squareRect = squareShapeImage.GetComponent<RectTransform>();
        Vector2 moveDistance = new Vector2(squareRect.rect.width * squareRect.localScale.x, squareRect.rect.height * squareRect.localScale.y);
        int currentIndexInList  = 0;

        // set position to form final shapes
        for (int row = 0; row < shapeData.rows; row++)
        {
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
        int number = 0;

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

    public void OnPointerClick(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        this.GetComponent<RectTransform>().localScale = shapeSelectedScale;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _transform.anchorMin = new Vector2 (0.5f,0.5f);
        _transform.anchorMax = new Vector2 (0.5f,0.5f);
        _transform.pivot = new Vector2 (0.5f,0.5f);

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform,
            eventData.position, Camera.main, out pos);
        _transform.localPosition = pos + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.GetComponent<RectTransform>().localScale = _shapeStartScale;
        GameEvents.CheckIfShapeCanBePlaced();
    }

}
