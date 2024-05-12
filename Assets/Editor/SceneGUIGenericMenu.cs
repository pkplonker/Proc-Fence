using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class SceneGUIGenericMenu : Editor
{
	static SceneGUIGenericMenu()
	{
		SceneView.duringSceneGui += OnSceneGUI;
	}

	static void OnSceneGUI(SceneView sceneview)
	{
		// if (Event.current.button == 1 && Input.GetKeyDown(KeyCode.LeftShift))
		// {
		// 	if (Event.current.type == EventType.MouseDown)
		// 	{
		// 		GenericMenu menu = new GenericMenu();
		// 		menu.AddItem(new GUIContent("Item 1"), false, Callback, 1);
		// 		menu.AddItem(new GUIContent("Item 2"), false, Callback, 2);
		// 		menu.ShowAsContext();
		// 	}
		// }
	}

	static void Callback(object obj)
	{
		// Do something
	}
}