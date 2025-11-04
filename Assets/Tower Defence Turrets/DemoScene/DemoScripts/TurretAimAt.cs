using UnityEngine;
using System.Collections;

public class TurretAimAt : MonoBehaviour {
public Transform target;
public Transform RotationPiece;
public float turnSpeed = 10f;

	void Update() {
		
		LockOnTarget();
	}

	void LockOnTarget (){
		
		Vector3 dir = target.position - transform.position;
		Quaternion lookRotation = Quaternion.LookRotation(dir);
		Vector3 rotation = Quaternion.Lerp(RotationPiece.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
		RotationPiece.rotation = Quaternion.Euler(0f, rotation.y, 0f);
	}

}