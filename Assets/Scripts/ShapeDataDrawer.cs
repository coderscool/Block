using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShapeData))]
public class ShapeDataDrawer : Editor
{
    ShapeData data;

    private void OnEnable()
    {
        data = (ShapeData)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        DrawSizeFields();

        GUILayout.Space(10);

        if (GUILayout.Button("Create Board"))
        {
            data.CreateNewBoard();
        }

        if (GUILayout.Button("Clear Board"))
        {
            data.Clear();
        }

        GUILayout.Space(10);

        if (IsValid())
        {
            DrawBoard();
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }
    }

    bool IsValid()
    {
        return data.board != null &&
               data.rows > 0 &&
               data.columns > 0;
    }

    void DrawSizeFields()
    {
        int oldRow = data.rows;
        int oldCol = data.columns;

        data.columns = EditorGUILayout.IntField("Columns", data.columns);
        data.rows = EditorGUILayout.IntField("Rows", data.rows);
        data.axisX = EditorGUILayout.IntField("Axis X", data.axisX);
        data.axisY = EditorGUILayout.IntField("Axis Y", data.axisY);

        // THÊM DÒNG NÀY
        data.shapePrefab = (GameObject)EditorGUILayout.ObjectField(
            "Shape Prefab",
            data.shapePrefab,
            typeof(GameObject),
            false
        );

        if ((oldRow != data.rows || oldCol != data.columns)
            && data.rows > 0 && data.columns > 0)
        {
            data.CreateNewBoard();
        }
    }

    void DrawBoard()
    {
        for (int y = 0; y < data.rows; y++)
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < data.columns; x++)
            {
                bool value = data.board[y].column[x];

                GUI.backgroundColor = value ? Color.green : Color.white;

                bool newValue = GUILayout.Toggle(value, "",
                    GUILayout.Width(30),
                    GUILayout.Height(30));

                data.board[y].column[x] = newValue;
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }
    }
}