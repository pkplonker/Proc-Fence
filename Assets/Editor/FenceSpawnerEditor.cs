using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FenceSpawner))]
public class FenceSpawnerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		FenceSpawner fenceSpawner = (FenceSpawner)target;
		if (GUILayout.Button("Spawn Fence"))
		{
			fenceSpawner.SpawnFence();
		}
		// if (GUI.changed)
		// {
		// 	fenceSpawner.SpawnFence();
		// }
	}
}