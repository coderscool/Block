using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShapeData))]
[CanEditMultipleObjects]
[System.Serializable]
public class ShapeDataDrawer : Editor
{
    private ShapeData ShapeDataInstance => (ShapeData)target;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawClearButton();
        EditorGUILayout.Space();

        DrawColumnsInputFields();
        EditorGUILayout.Space();

        if (IsBoardValid())
        {
            DrawBoardTable();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private bool IsBoardValid()
    {
        return ShapeDataInstance.board != null &&
               ShapeDataInstance.rows > 0 &&
               ShapeDataInstance.columns > 0;
    }

    private void DrawClearButton()
    {
        if (GUILayout.Button("Clear Board"))
        {
            ShapeDataInstance.Clear();
        }
    }

    private void DrawBoardTable()
    {
        var tableStyle = new GUIStyle("box");
        tableStyle.padding = new RectOffset(10, 10, 10, 10);
        tableStyle.margin.left = 32;

        var headerColumnStyle = new GUIStyle();
        headerColumnStyle.fixedWidth = 65;
        headerColumnStyle.alignment = TextAnchor.MiddleCenter;

        var rowStyle = new GUIStyle();
        rowStyle.fixedHeight = 25;
        rowStyle.alignment = TextAnchor.MiddleCenter;

        var dataFieldStyle = new GUIStyle(EditorStyles.miniButtonMid);
        dataFieldStyle.normal.background = Texture2D.whiteTexture;
        dataFieldStyle.active.background = Texture2D.grayTexture;

        for (int row = 0; row < ShapeDataInstance.rows; row++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int col = 0; col < ShapeDataInstance.columns; col++)
            {
                bool value = ShapeDataInstance.board[row].column[col];

                GUI.backgroundColor = value ? Color.green : Color.white;

                bool newValue = GUILayout.Toggle(value, "", GUILayout.Width(25), GUILayout.Height(25));

                ShapeDataInstance.board[row].column[col] = newValue;
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawColumnsInputFields()
    {
        int oldColumns = ShapeDataInstance.columns;
        int oldRows = ShapeDataInstance.rows;

        ShapeDataInstance.columns = EditorGUILayout.IntField("Columns", ShapeDataInstance.columns);
        ShapeDataInstance.rows = EditorGUILayout.IntField("Rows", ShapeDataInstance.rows);

        bool sizeChanged = ShapeDataInstance.columns != oldColumns || ShapeDataInstance.rows != oldRows;

        if (sizeChanged && ShapeDataInstance.columns > 0 && ShapeDataInstance.rows > 0)
        {
            ShapeDataInstance.CreateNewBoard();
        }
    }
}
