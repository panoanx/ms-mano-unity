using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using Kinesis.Components;

public class mimicker : MonoBehaviour
{
    public GameObject skeleton_prefab;
    private GameObject skeleton;
    // read external file
    // https://stackoverflow.com/questions/49400093/how-to-read-a-csv-file-in-unity
    public string path;
    public int startFrame = 0;
    public int endFrame = -1;

    private int frame_id;
    private double[,,] localrot;
    private double[,,] worldrot;
    private bool isRepeat = false;
    private Dictionary<string, int> handDict = new Dictionary<string, int>()
    {
        // // {"Left_Hand", 0},
        // {"Left_ThumbProximal", 13},
        // {"Left_ThumbIntermediate", 14},
        // {"Left_ThumbDistal", 15},
        // // {"Left_ThumbDistalEnd", 16},
        // {"Left_IndexProximal", 1},
        // {"Left_IndexIntermediate", 2},
        // {"Left_IndexDistal", 3},
        // // {"Left_IndexDistalEnd", 17},
        // {"Left_MiddleProximal", 4},
        // {"Left_MiddleIntermediate", 5},
        // {"Left_MiddleDistal", 6},
        // // {"Left_MiddleDistalEnd", 18},
        // {"Left_RingProximal", 10},
        // {"Left_RingIntermediate", 11},
        // {"Left_RingDistal", 12},
        // // {"Left_RingDistalEnd", 19},
        // {"Left_PinkyProximal", 7},
        // {"Left_PinkyIntermediate", 8},
        // {"Left_PinkyDistal", 9},
        // // {"Left_PinkyDistalEnd", 20}
        // {"right_wrist", 0},
        {"right_thumb1", 13},
        {"right_thumb2", 14},
        {"right_thumb3", 15},
        {"right_index1", 1},
        {"right_index2", 2},
        {"right_index3", 3},
        {"right_middle1", 4},
        {"right_middle2", 5},
        {"right_middle3", 6},
        {"right_ring1", 10},
        {"right_ring2", 11},
        {"right_ring3", 12},
        {"right_pinky1", 7},
        {"right_pinky2", 8},
        {"right_pinky3", 9},
    };

    private Dictionary<string, Quaternion> baseJointRotation;

    // Start is called before the first frame update
    void Awake()
    {
        string jsonString = File.ReadAllText(path);
        Debug.Log(jsonString);
        localrot = JsonConvert.DeserializeObject<double[,,]>(jsonString);
        worldrot = new double[localrot.GetLength(0), localrot.GetLength(1), localrot.GetLength(2)];
        frame_id = startFrame;
        if (endFrame == -1)
        {
            endFrame = localrot.GetLength(0);
        }
    }

    void Start()
    {
        // skeleton = Instantiate(skeleton_prefab, transform); 
        skeleton = skeleton_prefab;
        // foreach (var i in skeleton.GetComponentsInChildren<JointData>())
        // {
        //     Destroy(i);
        // }
        // foreach (var i in skeleton.GetComponentsInChildren<CharacterJoint>())
        // {
        //     Destroy(i);
        // }
        // foreach (var i in skeleton.GetComponentsInChildren<Rigidbody>())
        // {
        //     Destroy(i);
        // }

        baseJointRotation = new Dictionary<string, Quaternion>();
        foreach (var hand in handDict)
        {
            var joint = FindChildByRecursion(skeleton.transform, hand.Key);
            if (joint != null)
            {
                baseJointRotation.Add(hand.Key, joint.transform.localRotation);
            }
        }
    }

    void FixedUpdate()
    {
        Mimicker();
    }

    void Mimicker()
    {
        Debug.Log("frame_id: " + frame_id + ", angles: " + localrot[frame_id, 13, 0] + ", " + localrot[frame_id, 13, 1] + ", " + localrot[frame_id, 13, 2]);

        // if (frame_id > 1) {
        //     return;
        // }
        foreach (var hand in handDict)
        {
            var joint = FindChildByRecursion(skeleton.transform, hand.Key);
            if (joint != null)
            {
                Quaternion targetRotation = Quaternion.Euler(
                    (float)localrot[frame_id, hand.Value - 1, 0],
                    (float)localrot[frame_id, hand.Value - 1, 1],
                    (float)localrot[frame_id, hand.Value - 1, 2]
                );
                joint.transform.localRotation = targetRotation;
            }

        }

        foreach (var hand in handDict)
        {
            var joint = FindChildByRecursion(skeleton.transform, hand.Key);
            if (joint != null)
            {
                worldrot[frame_id, hand.Value - 1, 0] = joint.transform.rotation.eulerAngles.x;
                worldrot[frame_id, hand.Value - 1, 1] = joint.transform.rotation.eulerAngles.y;
                worldrot[frame_id, hand.Value - 1, 2] = joint.transform.rotation.eulerAngles.z;
            }
        }

        frame_id += 5;
        if (frame_id >= endFrame)
        {
            if (isRepeat == false)
            {
                // Save relativepos to file of the same directory as path
                string json = JsonConvert.SerializeObject(worldrot);
                string filename = Path.GetFileNameWithoutExtension(path);
                string newpath = Path.GetDirectoryName(path) + "/" + filename + "_worldrot.json";
                File.WriteAllText(newpath, json);
            }
            frame_id = startFrame;
            // isRepeat = true;
        }
    }

    public Transform FindChildByRecursion(Transform aParent, string aName)
    {
        if (aParent == null) return null;
        var result = aParent.Find(aName);
        if (result != null)
            return result;
        foreach (Transform child in aParent)
        {
            result = FindChildByRecursion(child, aName);
            if (result != null)
                return result;
        }
        return null;
    }
}
