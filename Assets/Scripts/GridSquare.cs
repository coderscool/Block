using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridSquare : MonoBehaviour
{
    public Image normalImage;
    public Image hooverImage;
    public Image activeImage;
    public List<Sprite> normalImages;

    public bool Selected { get; set; }
    public int SquareIndex { get; set; }
    public bool SquareOccupied { get; set; }

    void Start()
    {
        Selected = false;
        SquareOccupied = false;
    }

    public void SetImage(bool setFirstImage)
    {
        normalImage.GetComponent<Image>().sprite = setFirstImage ? normalImages[1] : normalImages[0];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        hooverImage.gameObject.SetActive(true);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        hooverImage.gameObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        hooverImage.gameObject.SetActive(false);
    }
}