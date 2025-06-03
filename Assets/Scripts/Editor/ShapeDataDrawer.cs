using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShapeData), false)]    //default isFallBack = false
[CanEditMultipleObjects]
[System.Serializable]
public class ShapeDataDrawer : Editor
{
    private ShapeData ShapeDataInstance => target as ShapeData;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        ClearBoardButton(); //method
        EditorGUILayout.Space();    //create small gap

        DrawColumnsInputFields();   //method
        EditorGUILayout.Space();

        if (ShapeDataInstance.board != null && ShapeDataInstance.columns > 0 && ShapeDataInstance.rows > 0)
        {
            DrawBoardTable();   //method
        }

        serializedObject.ApplyModifiedProperties(); //apply changes made -> from line 21

        if (GUI.changed)    //if any of the value changed
        {
            EditorUtility.SetDirty(ShapeDataInstance);  //mark an object as modified -> save/update the asset
        }
    }

    private void ClearBoardButton()
    {
        if (GUILayout.Button("Clear Board"))    //when clicked on the button labeled Clear Board -> clear
        {
            ShapeDataInstance.Clear();
        }
    }

    private void DrawColumnsInputFields()
    {
        var columnsTemp = ShapeDataInstance.columns;
        var rowsTemp = ShapeDataInstance.rows;
        //any changes made by the user are immidiately written to ShapeDataInstance
        ShapeDataInstance.columns = EditorGUILayout.IntField("Columns", ShapeDataInstance.columns);
        ShapeDataInstance.rows = EditorGUILayout.IntField("Rows", ShapeDataInstance.rows);
        //if any changes made and greater than 0
        if ((ShapeDataInstance.columns != columnsTemp || ShapeDataInstance.rows != rowsTemp) &&
        ShapeDataInstance.columns > 0 && ShapeDataInstance.rows > 0)
        {
            ShapeDataInstance.CreateNewBoard();
        }
    }

    private void DrawBoardTable()
    {
        // var tableStyle = new GUIStyle("box");
        // tableStyle.padding = new RectOffset(10, 10, 10, 10);
        // tableStyle.margin.left = 32;

        var headerColumnStyle = new GUIStyle
        {
            fixedWidth = 65,
            alignment = TextAnchor.MiddleCenter
        };

        var rowStyle = new GUIStyle
        {
            fixedHeight = 25,
            alignment = TextAnchor.MiddleCenter
        };
        //buttons will be gray when turned off (normal), white when turned on
        var dataFieldStyle = new GUIStyle(EditorStyles.miniButtonMid);
        dataFieldStyle.normal.background = Texture2D.grayTexture;
        dataFieldStyle.onNormal.background = Texture2D.whiteTexture;

        for (var row = 0; row < ShapeDataInstance.rows; row++)
        {
            EditorGUILayout.BeginHorizontal(headerColumnStyle); //create a line of rows (in horizontal)

            for (var column = 0; column < ShapeDataInstance.columns; column++)
            {   //create a line of rows in each column
                EditorGUILayout.BeginHorizontal(rowStyle);
                var data = EditorGUILayout.Toggle(ShapeDataInstance.board[row].column[column], dataFieldStyle);
                ShapeDataInstance.board[row].column[column] = data;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
