using System;
using UnityEngine;

namespace Robotflow
{
    public class AxisAngle
    {
        private Vector3 axis;
        private float angle;

        // constructors
        public AxisAngle(Vector3 axis, float angle)
        {
            this.axis = axis;
            this.angle = angle;
        }

        public AxisAngle(Quaternion quat)
        {
            quat.ToAngleAxis(out angle, out axis);
        }

        public AxisAngle(Vector3 axisAngle)
        {
            this.axis=axisAngle.normalized;
            this.angle = axisAngle.magnitude;
        }

        public AxisAngle FromEuler(Vector3 euler)
        {
            return new AxisAngle(Quaternion.Euler(euler));
        }

        // properties
        public Vector3 Axis
        {
            get { return axis; }
            set { axis = value; }
        }

        public float Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        public Quaternion quat
        {
            get { return Quaternion.AngleAxis(angle, axis); }
            set
            {
                value.ToAngleAxis(out angle, out axis);
            }
        }

        public Vector3 eulerAngles
        {
            get { return quat.eulerAngles; }
            set { quat = Quaternion.Euler(value); }
        }

        public static AxisAngle operator +(AxisAngle a, AxisAngle b)
        {
            return new AxisAngle(a.quat * b.quat);
        }

        public static AxisAngle operator -(AxisAngle a, AxisAngle b)
        {
            return new AxisAngle(a.quat * Quaternion.Inverse(b.quat));
        }

        public static AxisAngle operator *(AxisAngle a, float b)
        {
            return new AxisAngle(a.axis, a.angle * b);
        }

        public static AxisAngle operator /(AxisAngle a, float b)
        {
            return new AxisAngle(a.axis, a.angle / b);
        }

        public static AxisAngle operator -(AxisAngle a)
        {
            return new AxisAngle(-a.axis, a.angle);
        }


    }
}