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
	[SerializeField] private GameObject PostPrefab;
	[SerializeField] private GameObject BarPrefab;
	[SerializeField] private float BarLength;
	[SerializeField] private List<Vector3> Points;

	[SerializeField] private List<float> Heights;

	private GameObject fenceParent;

	public void SpawnFence()
	{
		if (Points.Count == 0 || Heights.Count == 0) throw new ArgumentNullException();

		if (fenceParent != null)
			DestroyImmediate(fenceParent);

		fenceParent = new GameObject("FenceParent");
		fenceParent.transform.parent = this.transform;

		for (var i = 0; i < Points.Count; i++)
		{
			var startPoint = Points[i];
			var endPoint = Points[(i + 1) % Points.Count];
			var direction = (endPoint - startPoint).normalized;
			var distance = Vector3.Distance(startPoint, endPoint);
			var quantity = Mathf.CeilToInt(distance / BarLength);
			var remainder = distance % BarLength;

			Vector3 previousPost = Vector3.zero;
			for (var j = 0; j <= quantity; j++)
			{
				var position = startPoint + (direction * (j * BarLength));
				position.y = GetTerrainHeight(position);
				if (j != quantity)
				{
					Instantiate(PostPrefab, position, Quaternion.identity, fenceParent.transform);
				}

				if (previousPost == Vector3.zero)
				{
					previousPost = position;
					continue;
				}

				foreach (var height in Heights)
				{
					var barPosition = position - previousPost + previousPost;
					barPosition.y += height;
					var bar = Instantiate(BarPrefab, barPosition, Quaternion.LookRotation(direction),
						fenceParent.transform);
					if (j != quantity ) continue;

					var barScale = bar.transform.localScale;
					barScale.z *= remainder / BarLength;
					bar.transform.localScale = barScale;

					float adjustment = (BarLength * barScale.z);
					bar.transform.position = startPoint + direction * ((j-1 + 0.5f) * BarLength + (BarLength * 0.5f)) -
					                         (direction * (BarLength - adjustment)) +
					                         new Vector3(0, height, 0);
				}

				previousPost = position;
			}
		}
	}

	private float GetTerrainHeight(Vector3 position)
	{
		Ray ray = new Ray(position + Vector3.up * 1000f, Vector3.down);
		if (Physics.Raycast(ray, out RaycastHit hitInfo))
		{
			return hitInfo.point.y;
		}
		else
		{
			return position.y;
		}
	}
}