using UnityEngine;

public class PatrolTest : MonoBehaviour
{
	public Vector3[] pointsToMove;
	void Update()
	{
		OnDrawGizmosSelected();
	}
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(pointsToMove[0], 0.2f);
	}
}
