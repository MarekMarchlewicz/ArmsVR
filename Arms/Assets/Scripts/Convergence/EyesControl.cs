using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EyesControl : MonoBehaviour 
{
	[SerializeField] private bool enableUpdate;

	[SerializeField] private Transform target;

	[SerializeField] private Transform leftEye, rightEye;

	[SerializeField] private float distanceBetweenEyesInMm;

	[SerializeField] private Color lineColor;

	private void Update () 
	{
		if (enableUpdate) 
		{
			UpdateEyesPosition ();

			LookAtTarget (target);

			DebugAngle ();
		}
	}

	private void UpdateEyesPosition()
	{
		leftEye.localPosition = Vector3.left * distanceBetweenEyesInMm / 2f;
		rightEye.localPosition = Vector3.right * distanceBetweenEyesInMm / 2f;
	}

	private void LookAtTarget(Transform targetToLookAt)
	{
		leftEye.LookAt (targetToLookAt.position);
		rightEye.LookAt (targetToLookAt.position);
	}

	private void DebugAngle()
	{
		float rightEyeAngle = Vector3.Angle (Vector3.left, rightEye.forward);
		float leftEyeAngle = Vector3.Angle (Vector3.right, leftEye.forward);

		Debug.Log ("Right eye: " + rightEyeAngle);
		Debug.Log ("Left eye: " + leftEyeAngle);
	}

	public void OnDrawGizmos ()
	{
		Gizmos.color = lineColor;

		Gizmos.DrawLine (leftEye.position, target.position);
		Gizmos.DrawLine (rightEye.position, target.position);
	}
}
