#if UNITY_EDITOR
using Kinesis.Components;
using Kinesis.Core;
using Newtonsoft.Json;
using RFUniverse;
using RFUniverse.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


public class SaveData : MonoBehaviour
{
    public GameObject mus;
    public List<MuscleStimulator> muscle;
    public List<Rigidbody> sceneJoint;

    [Serializable]
    class Frame
    {
        public List<Vector3> localRotation = new List<Vector3>();
        public List<Vector3> angularVelocity = new List<Vector3>();
        public List<Vector3> jointTorques = new List<Vector3>();
    }
    IEnumerator Start()
    {
        for (int k = 0; k < 31; k++)
        {
            GameObject muscleIns = Instantiate(mus);
            muscle = muscleIns.GetComponentsInChildren<MuscleStimulator>().ToList();
            sceneJoint = muscleIns.GetComponentsInChildren<RigidbodyAttr>().Select(x => x.GetComponent<Rigidbody>()).ToList();
            MuscleStimulator t = muscle[k];
            for (int j = 0; j < 100; j++)
            {
                foreach (var v in muscle)
                {
                    v.excitation = 0;
                    v.jointTorques.Clear();
                }
                foreach (var v in sceneJoint)
                {
                    v.transform.localRotation = Quaternion.identity;
                    v.velocity = Vector3.zero;
                    v.angularVelocity = Vector3.zero;
                    v.sleepThreshold = 0;
                }
                yield return new WaitForFixedUpdate();
            }

            t.excitation = 1;
            List<Frame> data = new List<Frame>();
            for (int i = 0; i < 300; i++)
            {
                yield return new WaitForFixedUpdate();
                Frame f = new Frame();
                foreach (var v in sceneJoint)
                    f.localRotation.Add(v.transform.localEulerAngles);
                foreach (var v in sceneJoint)
                    f.angularVelocity.Add(v.angularVelocity);

                Dictionary<Rigidbody, Vector3> jointTorques = new Dictionary<Rigidbody, Vector3>();
                foreach (var v in muscle)
                {
                    foreach (var item in v.jointTorques)
                    {
                        if (!jointTorques.ContainsKey(item.Key.jointAnchorBody))
                            jointTorques.Add(item.Key.jointAnchorBody, item.Value);
                        else
                            jointTorques[item.Key.jointAnchorBody] += item.Value;
                    }
                }
                foreach (var v in sceneJoint)
                    f.jointTorques.Add(jointTorques[v]);

                data.Add(f);
            }

            File.WriteAllText($"{Application.streamingAssetsPath}/{t.name}.json", JsonConvert.SerializeObject(data, RFUniverseUtility.JsonSerializerSettings));

            Destroy(muscleIns);
        }
        yield break;
    }
}

#endif