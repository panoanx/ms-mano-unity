using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngularVelocity : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody rigidbody;
    Quaternion prevRot;
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.AddTorque(-100, 60, -13);
        prevRot = rigidbody.transform.rotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Quaternion rot = rigidbody.transform.rotation;
        // Quaternion deltaRot = rot * Quaternion.Inverse(prevRot);
        // Vector3 angVelEuler = AngVelEuler(deltaRot);
        // Vector3 angVelAxisAngle = AngVelAxisAngle(deltaRot);
        // Debug.Log(rigidbody.angularVelocity.ToString() + angVelEuler.ToString() + angVelAxisAngle.ToString());
        // prevRot = rot;


        Quaternion rot = new Quaternion(-0.00414f, 0.00820f, -0.00296f, 0.99995f);
        rot = rot.normalized;
        Debug.Log(AngVelAxisAngle(rot).ToString() + AngVelAxisAngle1(rot).ToString());
    }

    Vector3 AngVelAxisAngle(Quaternion q)
    {
        float angle;
        Vector3 axis;
        q.ToAngleAxis(out angle, out axis);
        return axis * angle * Mathf.Deg2Rad / Time.fixedDeltaTime;
    }
    Vector3 AngVelAxisAngle1(Quaternion q)
    {
        float angle;
        Vector3 axis;
        q.ToAngleAxis(out angle, out axis);
        if (angle > 180)
        {
            angle -= 360;
            axis *= -1;
        }
        return axis * angle * Mathf.Deg2Rad / Time.fixedDeltaTime;
    }
    Vector3 AngVelEuler(Quaternion q)
    {
        Vector3 angVel = q.eulerAngles;
        for (int i = 0; i < 3; i++)
            angVel[i] = (angVel[i] + 540) % 360 - 180;
        angVel = angVel / Time.fixedDeltaTime * Mathf.Deg2Rad;
        return angVel;
    }
}
