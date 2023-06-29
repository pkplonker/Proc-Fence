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
	[SerializeField] private float AngleThreshhold = 25f;

	[SerializeField] private List<float> Heights;

	private GameObject fenceParent;

	public void SpawnFence()
	{
		if (Points.Count == 0 || Heights.Count == 0) throw new ArgumentNullException();

		if (fenceParent != null)
			DestroyImmediate(fenceParent);

		fenceParent = new GameObject("FenceParent");
		fenceParent.transform.parent = this.transform;

		for (var i = 0; i < (Points.Count>2? Points.Count: 1); i++)
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
				var positionVector = direction * (j * BarLength);
				positionVector = Vector3.ClampMagnitude(positionVector, distance);
				var position = startPoint + positionVector;
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
					var barPosition = ((position - previousPost)/2) + previousPost;
					barPosition.y += height;
					var barDirection = (position - previousPost).normalized;
					var barLength = Mathf.Sqrt(Mathf.Pow(position.x - previousPost.x, 2) + Mathf.Pow(position.y - previousPost.y, 2) + Mathf.Pow(position.z - previousPost.z, 2));

					var barRotation = Quaternion.LookRotation(barDirection);
					var bar = Instantiate(BarPrefab, barPosition, barRotation, fenceParent.transform);
					var barScale = bar.transform.localScale;
					barScale.z *= barLength / BarLength;
					bar.transform.localScale = barScale;

					if (j != quantity ) continue;

					barScale.z *= remainder / BarLength;
					bar.transform.localScale = barScale;

					bar.transform.position = ((position - previousPost)/2) + previousPost + new Vector3(0,height,0);
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