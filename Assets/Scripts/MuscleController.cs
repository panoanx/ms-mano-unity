using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kinesis.Demo;
using Robotflow.RFUniverse.SideChannels;
using UnityEngine;
using RFUniverse;

public class MuscleController : MonoBehaviour
{
    bool collect = false;
    [HideInInspector]
    public bool agentsCloned = false;
    public List<MuscleSimulation> agents = new List<MuscleSimulation>();
    // Start is called before the first frame update
    private int mode = 0;
    protected void SetMuscles(object[] obj)
    {
        // Debug.Log("-----" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        int index = (int)obj[0];
        List<float> muscle_excitations = obj[1].ConvertType<List<float>>();
        // Debug.Log(muscle_excitations.Count);
        // Debug.Log("+++++" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        foreach (var m in agents)
        {
            if (m.myTag == index)
            {
                m.excite_muscles(muscle_excitations);
            }
        }
        collect = true;
        // Debug.Log("=====" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        // Debug.Log("+++++" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
    }

    protected void SetMuscleReload(object[] obj)
    {
        int index = (int)obj[0];
        foreach (var m in agents)
        {
            if (m.myTag == index)
            {
                Debug.Log("SetMuscleReload");
                MuscleSimulation muscleSimulations = this.transform.parent.GetComponent<MuscleSimulation>();
                muscleSimulations.reset_muscles();
            }
        }

    }

    protected void CollectMuscleSegmentTorque()
    {
        float[] mt = new float[(agents.Count) * agents[0].muscleSegmentTorques.Count * 3];
        for (int i = 0; i < agents.Count; i++) // agent 0 does not have muscles
        {
            var ms = agents[i];
            for (int j = 0; j < ms.muscleSegmentTorques.Count; j++)
            {
                var mtj = ms.muscleSegmentTorques[j];
                mt[(i) * ms.muscleSegmentTorques.Count * 3 + j * 3] = mtj.x;
                mt[(i) * ms.muscleSegmentTorques.Count * 3 + j * 3 + 1] = mtj.y;
                mt[(i) * ms.muscleSegmentTorques.Count * 3 + j * 3 + 2] = mtj.z;
            }
        }
        PlayerMain.Instance.SendObject("CollectMuscleSegmentTorque", agents.Count, mt.ToList());
    }
    

    protected void CollectMuscleJointTorque()
    {
        float[] mt = new float[(agents.Count) * agents[0].muscleJointTorques.Count * 3];
        for (int i = 0; i < agents.Count; i++) // agent 0 does not have muscles
        {
            var ms = agents[i];
            for (int j = 0; j < ms.muscleJointTorques.Count; j++)
            {
                var mtj = ms.muscleJointTorques[j];
                mt[(i) * ms.muscleJointTorques.Count * 3 + j * 3] = mtj.x;
                mt[(i) * ms.muscleJointTorques.Count * 3 + j * 3 + 1] = mtj.y;
                mt[(i) * ms.muscleJointTorques.Count * 3 + j * 3 + 2] = mtj.z;
            }
        }
        PlayerMain.Instance.SendObject("CollectMuscleJointTorque",agents.Count, mt.ToList());
    }

    protected void CollectPDTorques()
    {
        float[] pt = new float[(agents.Count) * agents[0].pids.Count * 3];
        for (int i = 0; i < agents.Count; i++)
        {
            var pdt = agents[i].pdJointTorques;
            for (int j = 0; j < pdt.Count; j++)
            {
                var pdtj = pdt[j];
                pt[(i) * pdt.Count * 3 + j * 3] = pdtj.x;
                pt[(i) * pdt.Count * 3 + j * 3 + 1] = pdtj.y;
                pt[(i) * pdt.Count * 3 + j * 3 + 2] = pdtj.z;
            }
        }
        PlayerMain.Instance.SendObject("CollectPDTorque", agents.Count, pt.ToList());
        
    }
    protected void CollectJointAngVels()
    {
        float[] jv = new float[(agents.Count) * agents[0].pids.Count * 3];
        for (int i = 0; i < agents.Count; i++)
        {
            for (int j = 0; j < agents[i].pids.Count; j++)
            {
                Vector3 av = agents[i].pids[j].rigidbody.angularVelocity;
                Vector3 lav = agents[i].pids[j].transform.InverseTransformDirection(av);
                Vector3 jav = agents[i].pids[j].joint2local * lav;

                jv[(i) * agents[i].pids.Count * 3 + j * 3] = jav.x;
                jv[(i) * agents[i].pids.Count * 3 + j * 3 + 1] = jav.y;
                jv[(i) * agents[i].pids.Count * 3 + j * 3 + 2] = jav.z;
            }
        }
        PlayerMain.Instance.SendObject("CollectJointAngVel", agents.Count, jv.ToList());
    }
    
    public MuscleSimulation muscleAgent;
    void SetNumAgent(object[] obj)
    {
        int numAgent = (int) obj[0];
        SetNumAgent(numAgent);
    }

    void SetNumAgent(int numAgent)
    {
        for (int i = 0; i < numAgent; i++)
        {
            var ms = Instantiate(muscleAgent);
            ms.myTag = i;
            ms.MODE = mode;
            ms._awake();
            agents.Add(ms);
        }
        agentsCloned = true;
        PDController pc = FindObjectOfType<PDController>();
        pc.agents = agents;
    }
    
    void SetMode(object[] obj)
    {
        this.mode = (int) obj[0];
    }


    void Start()
    {
        PlayerMain.Instance.AddListenerObject("SetMuscles", SetMuscles);
        PlayerMain.Instance.AddListenerObject("SetMuscleReload", SetMuscleReload);
        PlayerMain.Instance.AddListenerObject("SetNumAgent", SetNumAgent);
        PlayerMain.Instance.AddListenerObject("SetMode", SetMode);  

        agents = FindObjectsOfType<MuscleSimulation>().ToList();
        agents = agents.OrderBy(x => x.myTag).ToList();
    }
    
    void FixedUpdate()
    {
        if (collect)
        {
            CollectMuscleSegmentTorque();
            CollectMuscleJointTorque();
            CollectPDTorques();
            CollectJointAngVels();
            collect = false;
        }
    }
}
