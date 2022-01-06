using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour {

	public Rigidbody rb;

	public Transform raycast;
	public Transform start;
	public Transform target;

	public float speed = 2.5f;
	public bool findNewPath;
	public bool drawPathPreview;

	Vector3 currentWaypoint;
	Vector3[] path;
	int targetIndex;

	int layerMask = 1 << 8;

	void Start() 
	{
		rb.isKinematic = false;
		InvokeRepeating("Find", 3, 0.5f);	
	}

    void Update()
    {
		Vector3 dir = currentWaypoint - transform.position;

		if (dir != Vector3.zero)
		{
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), 10 * Time.deltaTime);
			transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
		}
	}

	void Find()
	{
		RaycastHit hit;

		if (Physics.Raycast(raycast.transform.position, raycast.transform.forward * 2, out hit, 1f, layerMask))
		{
			if (hit.collider)
			{
				rb.AddForce(transform.up * 250);
			}
		}

		if (findNewPath)
		{
			PathRequestManager.RequestPath(start.transform.position, target.position, OnPathFound);
		}
	}

	public void OnPathFound(Vector3[] newPath, bool pathSuccessful) {
		if (pathSuccessful)
		{
			StopCoroutine("FollowPath");

			targetIndex = 0;

			path = newPath;
			currentWaypoint = path[targetIndex];

			StartCoroutine("FollowPath");
		}
	}

	IEnumerator FollowPath()
	{
		while (true) 
		{
			if (Vector3.Distance(transform.position, currentWaypoint) <= 0.1) 
			{
				targetIndex++;

				if (targetIndex >= path.Length) {
					yield break;
				}

				currentWaypoint = path[targetIndex];
			}

			if (transform.position != currentWaypoint)
			{
				transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
			}

			yield return null;
		}
	}

	public void OnDrawGizmos() {
		if (!drawPathPreview) { return; }

		if (path != null) {
			for (int i = targetIndex; i < path.Length; i ++) {
				Gizmos.color = Color.black;
				Gizmos.DrawCube(path[i], Vector3.one);

				if (i == targetIndex) {
					Gizmos.DrawLine(transform.position, path[i]);
				}
				else {
					Gizmos.DrawLine(path[i-1],path[i]);
				}
			}

			if (path.Length > 0)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawCube(path[0], Vector3.one);

				Gizmos.color = Color.cyan;
				Gizmos.DrawCube(currentWaypoint, Vector3.one);
			}
		}
	}
}
