using UnityEngine;
using UnityEngine.UI;

public class ShapeSquare : MonoBehaviour
{
    public Image occupiedImage;
    void Start()
    {
        occupiedImage.gameObject.SetActive(false);
    }
}
