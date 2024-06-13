using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointTest : MonoBehaviour
{
    CharacterJoint joint;
    Vector3 twistAxis, swing1Axis, swing2Axis;
    void Awake()
    {
        joint = GetComponent<CharacterJoint>();
        twistAxis = joint.axis.normalized; // axes are in local space
        swing1Axis = joint.swingAxis.normalized;
        swing2Axis = Vector3.Cross(twistAxis, swing1Axis).normalized;
        swing1Axis = Vector3.Cross(swing2Axis, twistAxis).normalized;
    }

    void FixedUpdate()
    {
        Vector3 localX = Vector3.right;
        Vector3 localY = Vector3.up;
        Vector3 localZ = Vector3.forward;

        Quaternion local2joint = Quaternion.LookRotation(swing2Axis, swing1Axis);

        Vector3 jointX = local2joint * localX;
        Vector3 jointY = local2joint * localY;
        Vector3 jointZ = local2joint * localZ;
        Debug.DrawRay(transform.position, transform.TransformVector(jointX), Color.red);
        Debug.DrawRay(transform.position, transform.TransformVector(jointY), Color.green);
        Debug.DrawRay(transform.position, transform.TransformVector(jointZ), Color.blue);

        Quaternion jointRot = transform.localRotation * local2joint;
        // Quaternion jointRot = Quaternion.FromToRotation(joint.axis, joint.transform.rotation.eulerAngles);
        Quaternion jointBias = Quaternion.Inverse(local2joint) * jointRot;

        Quaternion localRot = (local2joint * jointBias) * Quaternion.Inverse(local2joint); 
        Debug.Log(jointBias.eulerAngles.ToString() + localRot.eulerAngles.ToString());
        // Debug.Log(jointRot.eulerAngles);
        // Debug.Log((transform.rotation * local2joint).eulerAngles);
        // Debug.Log(local2joint * transform.localEulerAngles);
    }
}
