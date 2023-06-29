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
			for (var j = 0; j < quantity; j++)
			{
				Vector3 previousPosition = startPoint + direction * ((j == 0 ? 0 : j - 1) * BarLength);
				previousPosition.y = GetTerrainHeight(previousPosition);

				var position = startPoint + direction * (j * BarLength);
				position.y = GetTerrainHeight(position);

				float threeDDistance = Vector3.Distance(previousPosition, position);
				while (threeDDistance > BarLength)
				{
					position -= direction * 0.01f;
					position.y = GetTerrainHeight(position);
					threeDDistance = Vector3.Distance(previousPosition, position);
				}
			}

			for (var j = 0; j < quantity; j++)
			{
				var position = startPoint + (direction * (j * BarLength));
				position.y = GetTerrainHeight(position);
				Instantiate(PostPrefab, position, Quaternion.identity, fenceParent.transform);
				
				
				foreach (var height in Heights)
				{
					var barPosition = startPoint + direction * ((j + 0.5f) * BarLength + (BarLength * 0.5f));
					barPosition.y += height;
					var bar = Instantiate(BarPrefab, barPosition, Quaternion.LookRotation(direction),
						fenceParent.transform);
					if (j != quantity - 1) continue;

					var barScale = bar.transform.localScale;
					barScale.z *= remainder / BarLength;
					bar.transform.localScale = barScale;

					float adjustment = (BarLength * barScale.z);
					bar.transform.position = startPoint + direction * ((j + 0.5f) * BarLength + (BarLength * 0.5f)) -
					                         (direction * (BarLength - adjustment)) +
					                         new Vector3(0, height, 0);
				}
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