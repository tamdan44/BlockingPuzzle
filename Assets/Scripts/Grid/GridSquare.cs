using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GridSquare : MonoBehaviour
{
    public Image hooverImage;
    public Image activeImage;
    public Image normalImage;
    public List<Sprite> normalImages;
    public bool Selected {get; set;}
    public bool SquareOccupied {get; set;}
    public int SquareIndex { get; set; }

    void Start()
    {
        Selected = false;
        SquareOccupied = false;
    }

    public bool CanUseThisSquare()
    {
        return hooverImage.gameObject.activeSelf;
    }

    public void PlaceShapeOnBoard()
    {
        ActivateSquare();
    }

    public void ActivateSquare()
    {
        hooverImage.gameObject.SetActive(false);
        activeImage.gameObject.SetActive(true);
        Selected = true;
        SquareOccupied = true;
    }

    public void Deactivate()
    {
        activeImage.gameObject.SetActive(false);
    }

    public void ClearOccupied()
    {
        Selected = false;
        SquareOccupied =false;
        normalImage.gameObject.SetActive(true);
    }

    public void SetImage(bool setFirstImage)
    {
        normalImage.GetComponent<Image>().sprite = setFirstImage ? normalImages[1] : normalImages[0];
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!SquareOccupied)
        {
            Selected = true;
            hooverImage.gameObject.SetActive(true);
        }
        else if(collision.GetComponent<ShapeSquare>()!=null)
        {
            collision.GetComponent<ShapeSquare>().SetOccupied();
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        Selected = true;
        if(!SquareOccupied)
        {            
            hooverImage.gameObject.SetActive(true);
        }
        else if(collision.GetComponent<ShapeSquare>()!=null)
        {
            collision.GetComponent<ShapeSquare>().SetOccupied();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {        
        if(!SquareOccupied)
        {
            Selected = false;            
            hooverImage.gameObject.SetActive(false);
        }
        else if(collision.GetComponent<ShapeSquare>()!=null)
        {
            collision.GetComponent<ShapeSquare>().SetUnoccupied();
        }
    }
}
