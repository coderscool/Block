using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public List<Vector2Int> cells = new List<Vector2Int>();
    public Vector2Int origin;
    public string parentId;

    public void SetOrigin(Vector2Int newOrigin)
    {
        origin = newOrigin;
    }

    public void SetParentId(string id)
    {
        parentId = id;
    }
}
