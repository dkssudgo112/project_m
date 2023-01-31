using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 22.08.26_jayjeong
/// <br> �������� ���� Poisson-disc Sample�� �������� Ȯ���� �� �ִ� �ùķ����� </br>
/// </summary>
public class PoissonSimulator : MonoBehaviour
{
	public float _radius = 1;
	public Vector2 _regionSize = Vector2.one;
	public int _rejectionSamples = 30;
	public float _displayRadius = 1;

	List<Vector2> points;

	void OnValidate()
	{
		points = PoissonDiscSampler.GeneratePoints(_radius, _regionSize, _rejectionSamples);
	}

	void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(_regionSize / 2, _regionSize);
		
		if (points != null)
		{
			foreach (Vector2 point in points)
			{
				Gizmos.DrawSphere(point, _displayRadius);
			}
		}
	}
}
