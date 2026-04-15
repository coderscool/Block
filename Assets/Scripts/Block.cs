using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public List<Vector2Int> cells = new List<Vector2Int>();
    public Vector2Int origin;

    public void SetOrigin(Vector2Int newOrigin)
    {
        origin = newOrigin;
    }
}
