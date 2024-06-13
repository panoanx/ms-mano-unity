﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace RFUniverse.DebugTool
{
    public class PoseGizmo : MonoBehaviour
    {
        public Transform target;
        public Transform axis;
        public Canvas canvas;
        public TextMeshProUGUI text;

        void FixedUpdate()
        {
            if (target && target.gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
                transform.position = target.position;
                axis.rotation = target.rotation;
                text.text = $"position:{target.position.ToString("f2")}\nrotation euler:{target.eulerAngles.ToString("f2")}\nrotation qura:{target.rotation.ToString("f2")}\nscale: {target.lossyScale.ToString("f2")}";
                canvas.transform.rotation = Camera.main.transform.rotation;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
