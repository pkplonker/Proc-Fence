using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
[ExecuteInEditMode]
#endif
public class FenceSpawner : MonoBehaviour
{
	[SerializeField]
	private GameObject PostPrefab;

	[SerializeField]
	private GameObject BarPrefab;

	[SerializeField]
	private GameObject VerticalSlat;

	[SerializeField]
	private float BarLength;

	[SerializeField]
	private List<Vector3> Points;

	[SerializeField]
	private Vector3 BarRaycastOffset;

	[SerializeField]
	private float PostInsertionDepth = 0.2f;

	[SerializeField]
	private bool Randomisation;

	[SerializeField]
	private List<float> HorizontalHeights;

	[SerializeField]
	private bool closedLoop;

	private GameObject fenceParent;

	[SerializeField]
	public bool ProcessOnUpdate;

	[SerializeField]
	private bool AvoidGroundIntersectionWithHorizontals;

	public void SpawnFence()
	{
		if (Points.Count == 0 || HorizontalHeights.Count == 0) throw new ArgumentNullException();

		if (fenceParent != null)
			DestroyImmediate(fenceParent);

		fenceParent = new GameObject("FenceParent")
		{
			transform =
			{
				parent = transform
			}
		};

		var postInsertion = new Vector3(0, PostInsertionDepth, 0);
		var numberOfPostsToProcess = (Points.Count > 2 ? Points.Count - (closedLoop ? 0 : 1) : 1);

		for (var i = 0; i < numberOfPostsToProcess; i++)
		{
			ProcessPoint(i, postInsertion);
		}
	}

	private void ProcessPoint(int i, Vector3 postInsertion)
	{
		var startPoint = Points[i];
		var endPoint = Points[(i + 1) % Points.Count];
		var direction = (endPoint - startPoint).normalized;
		var distance = Vector3.Distance(startPoint, endPoint);
		var quantity = Mathf.CeilToInt(distance / BarLength);
		var remainder = distance % BarLength;

		var previousPost = Vector3.zero;
		for (var j = 0; j <= quantity; j++)
		{
			previousPost = ProcessFenceSection(postInsertion, direction, j, distance, startPoint, quantity,
				previousPost, remainder);
		}
	}

	private Vector3 ProcessFenceSection(Vector3 postInsertion, Vector3 direction, int j, float distance,
		Vector3 startPoint,
		int quantity, Vector3 previousPost, float remainder)
	{
		var positionVector = direction * (j * BarLength);
		positionVector = Vector3.ClampMagnitude(positionVector, distance);
		var position = startPoint + positionVector;
		position.y = GetTerrainHeight(position);
		if (j != quantity || !closedLoop || (closedLoop && j == quantity))
			Instantiate(PostPrefab, position - postInsertion, Quaternion.identity, fenceParent.transform);

		if (previousPost == Vector3.zero)
		{
			previousPost = position;
			return previousPost;
		}

		foreach (var height in HorizontalHeights)
		{
			var barPosition = ((position - previousPost) / 2) + previousPost;
			barPosition.y += height;
			var barDirection = (position - previousPost).normalized;
			var barLength = Mathf.Sqrt(Mathf.Pow(position.x - previousPost.x, 2) +
			                           Mathf.Pow(position.y - previousPost.y, 2) +
			                           Mathf.Pow(position.z - previousPost.z, 2));

			var raycastOrigin = previousPost + new Vector3(0, height, 0) - BarRaycastOffset;
			var hit = Physics.Raycast(raycastOrigin, barDirection, barLength);

			if (AvoidGroundIntersectionWithHorizontals && hit)
			{
				continue;
			}

			var barRotation = Quaternion.LookRotation(barDirection);
			var bar = Instantiate(BarPrefab, barPosition, barRotation, fenceParent.transform);
			var barScale = bar.transform.localScale;

			barScale.z *= ((j != quantity) ? barLength : remainder) / BarLength;

			bar.transform.localScale = barScale;
			if (j != quantity)
			{
				continue;
			}

			bar.transform.position = CalculateBarPosition(previousPost, position, height);
		}

		previousPost = position;
		return previousPost;
	}

	private static Vector3 CalculateBarPosition(Vector3 previousPost, Vector3 position, float height) =>
		((position - previousPost) / 2) + previousPost + new Vector3(0, height, 0);

#if UNITY_EDITOR
	private void Update()
	{
		if (!ProcessOnUpdate) return;
		SpawnFence();
	}
#endif

	private float GetTerrainHeight(Vector3 position)
	{
		var ray = new Ray(position + Vector3.up * 1000f, Vector3.down);
		return Physics.Raycast(ray, out RaycastHit hitInfo) ? hitInfo.point.y : position.y;
	}
}