// # define SAVE_DATA 
using System;
using System.Collections.Generic;
using System.Management.Instrumentation;
using RFUniverse.Attributes;
using Robotflow.RFUniverse;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using Robotflow.RFUniverse.SideChannels;
using Kinesis.Demo;
using Newtonsoft.Json;
using RFUniverse;
using RFUniverse.Manager;
using Robotflow;

public class PDController : MonoBehaviour
{
    public List<MuscleSimulation> agents = new List<MuscleSimulation>();
#if SAVE_DATA
    struct toSave {
        public List<float> targetPos;
        public List<float> targetVel;
        public List<float> realPos;
        public List<float> realVel;
    }
    List<toSave> toSaveList = new List<toSave>();
#endif

    protected void SetPDTarget(object[] obj)
    // public void SetPDTarget(int index, List<Vector3> pdTargets, bool ifReg)
    {
        int index = (int)obj[0];
        List<float> _pdRotTargets = // n * 3 , euler angles, n = joint num
            obj[1].ConvertType<List<float>>();
        // List<float> _pdPosTargets = // n * 3 , world position, n = joint num\
        // msg.ReadFloatList().ToList();
        // List<float> to List<Vector3>
        bool ifReg = (bool)obj[2];
        foreach (var a in agents)
        {
            if (a.myTag == index)
            {
                for (int i = 0; i < a.pids.Count; i++)
                {
                    // Vector3 targetLocalRotEuler = new Vector3(
                    //     _pdRotTargets[i * 3],
                    //     _pdRotTargets[i * 3 + 1],
                    //     _pdRotTargets[i * 3 + 2]
                    // );

                    // Quaternion targetLocalRot = Quaternion.Euler(targetLocalRotEuler);
                    // a.pids[i].targetJointRot = (
                    //     targetLocalRot * a.pids[i].local2joint
                    // ).eulerAngles;
                    Vector3 target = new Vector3(
                        _pdRotTargets[i * 3],
                        _pdRotTargets[i * 3 + 1],
                        _pdRotTargets[i * 3 + 2]
                    );
                    a.pids[i].SetTarget(target, ifReg);

                    // if (i == 1) Debug.Log(a.pids[i].targetLocalVel);
                }
                // a.graph.Reset();
            }
        }
    }

    protected void SetPose(object[] obj)
    // public void SetPose(int index, List<Vector3> pose, bool ifReg)
    {
        int index = (int)obj[0];
        List<float> _pose = // n * 3 , euler angles, n = joint num
            obj[1].ConvertType<List<float>>();
        bool ifReg = (bool)obj[2];
        string representation = (string)obj[3];
        foreach (var a in agents)
        {
            if (a.myTag == index)
            {
                for (int i = 0; i < a.pids.Count; i++)
                {
                    Vector3 target = new Vector3(
                        _pose[i * 3],
                        _pose[i * 3 + 1],
                        _pose[i * 3 + 2]
                    );
                    if (ifReg == true && representation == "euler")
                    {
                        Vector3 targetJointBiasEuler = a.pids[i].Reg2Bias(target);
                        Quaternion targetLocalRot = a.pids[i].local2joint * Quaternion.Euler(targetJointBiasEuler) * Quaternion.Inverse(a.pids[i].local2joint);
                        a.pids[i].transform.localRotation = targetLocalRot;
                    }
                    else if (ifReg == false && representation == "euler")
                    {
                        Quaternion targetLocalRot = Quaternion.Euler(target);
                        a.pids[i].transform.localRotation = targetLocalRot;
                    }
                    else if (ifReg == false && representation == "axis_angle")
                    {
                        AxisAngle targetLocalRot = new AxisAngle(target);
                        a.pids[i].transform.localRotation = targetLocalRot.quat;
                    } 
                    else {
                        Debug.LogError("SetPose: representation not supported ");
                    }
                }

            }
        }
    }

    protected void SetAngVel(object[] obj)
    {
        int index = (int)obj[0];
        List<float> _angVel = // n * 3 
            obj[1].ConvertType<List<float>>();
        foreach (var a in agents)
        {
            if (a.myTag == index)
            {
                for (int i = 0; i < a.pids.Count; i++)
                {
                    Vector3 angVel = new Vector3(
                        _angVel[i * 3],
                        _angVel[i * 3 + 1],
                        _angVel[i * 3 + 2]
                    );
                    a.pids[i].rigidbody.angularVelocity = angVel;
                }
            }
        }
    }

    void SetPoseByPD(object[] obj) 
    {
        int index = (int)obj[0];
        List<float> _pose = // n * 3 , euler angles, n = joint num
            obj[1].ConvertType<List<float>>();
        bool ifReg = (bool)obj[2];
        string representation = (string)obj[3];
        foreach (var a in agents)
        {
            if (a.myTag == index)
            {
                for (int i = 0; i < a.pids.Count; i++)
                {
                    Vector3 target = new Vector3(
                        _pose[i * 3],
                        _pose[i * 3 + 1],
                        _pose[i * 3 + 2]
                    );
                    a.pids[i].SetTarget(target, ifReg);
                }
            }
        }
    }

    protected void ApplyPDTorqueAll(object[] obj)
    // public void ApplyPDTorqueAll(List<int> env_idxs, int interpolate)
    {
        // int length = (int)obj[0];
        List<int> env_idxs = obj[0].ConvertType<List<int>>();
        int interpolate = (int)obj[1];

        if (interpolate > 0)
        {
            Physics.autoSimulation = false;
            for (int i = 0; i < interpolate; i++)
            {
                // Debug.Log("ApplyPDTorque to " + env_idxs.Count + " agents");
                for (int j = 0; j < env_idxs.Count; j++)
                {
                    int index = (int)env_idxs[j];
                    foreach (var a in agents)
                    {
                        if (a.myTag == index)
                        {
                            a.ApplyPDTorque();
#if SAVE_DATA
                        toSaveList.Add(new toSave{
                            targetPos = v3_lf(a.pids[4].targetJointRotEuler),
                            targetVel = v3_lf(a.pids[4].targetLocalVel),
                            realPos   = v3_lf(a.pids[4].jointRotEuler),
                            realVel   = v3_lf(a.pids[4].localVel),
                        });
#endif
                        }
                    }
                }
                Physics.Simulate(Time.fixedDeltaTime / interpolate);
            }
            Physics.autoSimulation = true;
        }
        else
        {
            for (int j = 0; j < env_idxs.Count; j++)
            {
                int index = (int)env_idxs[j];
                foreach (var a in agents)
                {
                    if (a.myTag == index)
                    {
                        a.ApplyPDTorque();
                    }
                }
            }
        }

    }



    protected void CollectJointBias()
    {
        int pC = agents[0].pids.Count;
        float[] jointBias = new float[agents.Count * pC * 3]; // n * 15 * 3
        for (int i = 0; i < agents.Count; i++)
        {
            for (int j = 0; j < pC; j++)
            {
                JointPID pid = agents[i].pids[j];
                Quaternion localRot = pid.transform.localRotation;


                // Vector3 angles = pid.DecomposeSwingTwist(localRot);
                // Quaternion dedecomp = pid.ComposeSwingTwist(angles);

                Quaternion jointRot = localRot * pid.local2joint;
                Vector3 jointBiasEuler = pid.JointRot2Bias(jointRot.eulerAngles);
                Vector3 jointBiasEulerReg = pid.Bias2Reg(jointBiasEuler);

                jointBias[(i * pC + j) * 3] = jointBiasEulerReg[0];
                jointBias[(i * pC + j) * 3 + 1] = jointBiasEulerReg[1];
                jointBias[(i * pC + j) * 3 + 2] = jointBiasEulerReg[2];
            }
        }
        // string debugString = "";
        // foreach (var jb in jointBias)
        // {
        //     debugString += jb.ToString() + " ";
        // }
        // Debug.Log(debugString);

        PlayerMain.Instance.SendObject("CollectJointBias", agents.Count, jointBias.ToList());
    }

    protected void CollectTargetJointAngVel()
    {
        float[] jv = new float[agents.Count * agents[0].pids.Count * 3];
        for (int i = 0; i < agents.Count; i++)
        {
            for (int j = 0; j < agents[i].pids.Count; j++)
            {
                Vector3 lav = agents[i].pids[j].targetLocalVel;
                Vector3 jav = agents[i].pids[j].joint2local * lav;

                jv[(i) * agents[i].pids.Count * 3 + j * 3] = jav.x;
                jv[(i) * agents[i].pids.Count * 3 + j * 3 + 1] = jav.y;
                jv[(i) * agents[i].pids.Count * 3 + j * 3 + 2] = jav.z;
            }
        }
        PlayerMain.Instance.SendObject("CollectTargetJointAngVel", agents.Count, jv.ToList());
    }


# if SAVE_DATA
    List<float> v3_lf(Vector3 foo)
    {
        List<float> bar = new List<float>();
        bar.Add(foo.x);
        bar.Add(foo.y);
        bar.Add(foo.z);
        return bar;
    }
# endif

    public MuscleController mc;
    void Start()
    {
        PlayerMain.Instance.AddListenerObject("SetPDTarget", SetPDTarget);
        PlayerMain.Instance.AddListenerObject("ApplyPDTorqueAll", ApplyPDTorqueAll);
        PlayerMain.Instance.AddListenerObject("SetPose", SetPose);
        PlayerMain.Instance.AddListenerObject("SetPoseByPD", SetPoseByPD);
        PlayerMain.Instance.AddListenerObject("FakeSimulate", FakeSimulate);
        
        // mc = FindObjectOfType<MuscleController>();
        // agents = FindObjectsOfType<MuscleSimulation>().ToList();
        // agents = agents.OrderBy(a => a.myTag).ToList();
        // // Physics.autoSimulation = false;
    }

    void FakeSimulate(object[] obj) 
    {
        int num = 200;
        Physics.simulationMode = SimulationMode.Script;
        for (int i = 0; i < num; i++) 
        {
            foreach (var a in agents)
            {
                // if (a.myTag == index)
                {
                    a.ApplyPDTorque();
                }
            }

            Physics.Simulate(Time.fixedDeltaTime);   
        }
        Physics.simulationMode = SimulationMode.FixedUpdate;
    }

    void FixedUpdate()
    {
        if (mc.agentsCloned == true)
        {
            CollectJointBias();
            CollectTargetJointAngVel();
        }
    }

# if SAVE_DATA
    void OnApplicationQuit()
    {
        string json = JsonConvert.SerializeObject(toSaveList);
        System.IO.File.WriteAllText("tosave.json", json);
    }
# endif
}

