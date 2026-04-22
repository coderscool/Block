using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Match", menuName = "Game/Shape Match")]
public class ShapeMatchPattern : ScriptableObject
{
    public string name;
    public List<Vector2Int> offsets = new List<Vector2Int>();
    public List<int> values = new List<int>();
    public int requiredUniqueBlocks = 2;
}