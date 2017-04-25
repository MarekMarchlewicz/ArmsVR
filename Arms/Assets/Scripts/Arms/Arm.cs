using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Arm : MonoBehaviour 
{
	private enum State
	{
		Idle,
		Following,
		CommingBack
	}

    [Header("Setup")]
    [SerializeField] private SteamVR_TrackedController controller;
	[SerializeField] private State state = State.Idle;
	[SerializeField] private Glove glove;
	[SerializeField] private float maxDistanceToTravel = 30f;
    [SerializeField] private float minAngleToAddLinePoint = 0.5f;

    [Header("Speeds")]
	[SerializeField] private float followingSpeed = 10f;
    [SerializeField] private float returnSpeed = 20f;
    [SerializeField] private float rotationSpeed = 5f;

    private float lastHapticTime;

	private List<Vector3> points = new List<Vector3> (Max_Points);

	private float currentDistance = 0f;

	private Vector3 currentPosition;

	private Vector3 currentRotation;

	private const int Max_Points = 50;

	private LineRenderer line;

	private void Awake()
	{
		line = GetComponent<LineRenderer> ();

		currentRotation = Vector3.zero;

        glove.OnHit += OnHit;
	}

    private void Start()
    {
        if (controller != null)
        {
            controller.Gripped += OnGripped;
            controller.Ungripped += OnUngripped;
        }
        else
        {
            Debug.LogError("NO CONTROLLER");
        }

    }
    private void OnGripped(object sender, ClickedEventArgs e)
    {
        StartFollowing();
    }

    private void OnUngripped(object sender, ClickedEventArgs e)
    {
        if (state == State.Following)
        {
            StopFollowing();
        }
    }

    private void OnHit()
    {
        StopFollowing();

        StartCoroutine(HitHaptic());
    }

    private IEnumerator HitHaptic()
    {
        for (int i = 0; i < 10; i++)
        {
            SteamVR_Controller.Input((int)controller.GetComponent<SteamVR_TrackedObject>().index).TriggerHapticPulse(1000);

            yield return new WaitForSeconds(0.01f);
        }
    }

    private void StartFollowing()
    {
        if (state == State.Idle)
        {
            state = State.Following;

            currentDistance = 0f;
            currentPosition = transform.position;

            line.enabled = true;
            line.positionCount = 2;
            line.SetPosition(0, currentPosition);
            line.SetPosition(1, currentPosition);
            points.Add(currentPosition);
            points.Add(currentPosition);
        }
    }

    private void StopFollowing()
    {
        if (state == State.Following)
        {
            state = State.CommingBack;
        }
    }

    private void Update () 
	{
		UpdateRotation ();

		switch (state) 
		{
            case State.Idle:
                UpdatePosition();
                break;
		    case State.Following:
			    UpdateFollowing ();
                UpdateHaptic();
			    break;
		    case State.CommingBack:
			    UpdateCommingBack ();
			    break;
		}
	}

	private void UpdateFollowing()
	{
        Vector3 newPosition = currentPosition + (transform.rotation * Vector3.forward * Time.deltaTime * followingSpeed);

		currentDistance += Vector3.Distance (currentPosition, newPosition);

		currentPosition = newPosition;

		if (Vector3.Angle (currentPosition - points [points.Count - 1], points [points.Count - 1] - points [points.Count - 2]) > minAngleToAddLinePoint) 
		{
			points.Add (currentPosition);
			line.positionCount++;
			line.SetPosition (line.positionCount - 1, currentPosition);
		} 
		else 
		{
			line.SetPosition (line.positionCount - 1, currentPosition);
		}

		glove.transform.position = currentPosition;
        glove.transform.rotation = transform.rotation;

        if (currentDistance > maxDistanceToTravel) 
		{
			state = State.CommingBack;
		} 
	}

    private void UpdateHaptic()
    {
        if(Time.time - lastHapticTime > 0.05f)
        {
            SteamVR_Controller.Input((int)controller.GetComponent<SteamVR_TrackedObject>().index).TriggerHapticPulse(500);

            lastHapticTime = Time.time;
        }
    }

	private void UpdateCommingBack()
	{
		float allowedDistance = returnSpeed * Time.deltaTime;

		currentDistance = 0f;
        
		Vector3 targetPosition = points [points.Count - 1];
		Vector3 newPosition = Vector3.MoveTowards (currentPosition, targetPosition, allowedDistance);
		currentDistance += Vector3.Distance (newPosition, currentPosition);
		currentPosition = newPosition;
        
		while (currentDistance < allowedDistance * 0.99f && points.Count > 2) 
		{
			points.RemoveAt (points.Count - 1);
			line.positionCount--;

			targetPosition = points [points.Count - 1];
			newPosition = Vector3.MoveTowards (currentPosition, points [points.Count - 1], allowedDistance - currentDistance);
			currentDistance += Vector3.Distance (newPosition, currentPosition);
			currentPosition = newPosition;
		}

		glove.transform.position = currentPosition;
        glove.transform.rotation = transform.rotation;

		if (line.positionCount > 2) 
		{
			line.SetPosition (line.positionCount - 1, currentPosition);
		} 
		else 
		{
			line.enabled = false;

			points.Clear ();

			state = State.Idle;
		}
	}

	private void UpdateRotation()
    { 
        transform.localRotation = Quaternion.Lerp(transform.localRotation, controller.transform.localRotation * Quaternion.Euler(Vector3.right * 90f), Time.deltaTime * rotationSpeed);
	}

    private void UpdatePosition()
    {
        transform.position = controller.transform.position;

        glove.transform.position = transform.position;
        glove.transform.rotation = transform.rotation;
    }
}
