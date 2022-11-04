using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(GridMap))]
public class GridMapInspector : Editor {

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		GridMap grid = target as GridMap;

		if(GUILayout.Button("Generate Hex Grid"))
			grid.GenerateGrid();

		if(GUILayout.Button("Clear Hex Grid"))
			grid.ClearGrid();
	}
}