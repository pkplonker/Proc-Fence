using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
#endif
public class FenceSpawner : MonoBehaviour
{
	[SerializeField] private GameObject postPrefab;
	[SerializeField] private GameObject barPrefab;
	[SerializeField] private float barLength;
	[SerializeField] private float distance;
	[SerializeField] private List<Vector3> points;

	[SerializeField] private List<float> heights;

	private GameObject fenceParent;

	public void SpawnFence()
	{
		if (points.Count == 0 || heights.Count == 0) throw new ArgumentNullException();

		if (fenceParent != null)
			DestroyImmediate(fenceParent);

		fenceParent = new GameObject("FenceParent");
		fenceParent.transform.parent = this.transform;

		for (var i = 0; i < points.Count; i++)
		{
			var startPoint = points[i];
			var endPoint = points[(i + 1) % points.Count];
			var direction = (endPoint - startPoint).normalized;
			var distance = Vector3.Distance(startPoint, endPoint);
			var quantity = Mathf.CeilToInt(distance / barLength);
			var remainder = distance % barLength;

			for (var j = 0; j < quantity; j++)
			{
				var position = startPoint + direction * (j * barLength);
				Instantiate(postPrefab, position, Quaternion.identity, fenceParent.transform);

				foreach (var height in heights)
				{
					var barPosition = startPoint + direction * ((j + 0.5f) * barLength + (barLength * 0.5f));
					barPosition.y += height;
					var bar = Instantiate(barPrefab, barPosition, Quaternion.LookRotation(direction),
						fenceParent.transform);
					if (j != quantity - 1) continue;

					var barScale = bar.transform.localScale;
					barScale.z *= remainder / barLength;
					bar.transform.localScale = barScale;

					float adjustment = (barLength * barScale.z);
					bar.transform.position = startPoint + direction * ((j + 0.5f) * barLength + (barLength * 0.5f)) -
					                         (direction * (barLength - adjustment)) +
					                         new Vector3(0, height, 0);
				}
			}
		}
	}
}