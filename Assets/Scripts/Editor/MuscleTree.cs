using Kinesis.Core;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;

[Serializable]
public class MuscleTendonUnitData
{
    public string name;
    public float normalizedSEESlackLength;
    public float maxIsometricForce;
    public float activationConstant;
    public List<MuscleNodeData> muscleNodeDatas = new List<MuscleNodeData>();
    public List<MuscleSegmentData> muscleSegmentDatas = new List<MuscleSegmentData>();
}
[Serializable]
public class MuscleNodeData
{
    public string boneName;
    public Vector3 offset;
}
[Serializable]
public class MuscleSegmentData
{
    public MuscleNodeData head;
    public MuscleNodeData tail;
    public string jiontName;
    public string jiontAnchorBodyName;
}

public class MuscleTree : Editor
{
    [MenuItem("Tool/SaveMuscleTendonUnitData %O")]
    public static void SaveMuscleTendonUnitData()
    {
        GameObject m = Selection.activeGameObject;
        if (m == null)
        {
            Debug.LogWarning($"Unselected Muscle GameObject");
            return;
        }

        string path = EditorUtility.SaveFilePanel("SaveXML", "", "MuscleTendonUnitData", "xml");
        if (!Directory.Exists(Path.GetDirectoryName(path))) return;

        MuscleTendonUnit[] muscleTendonUnits = m.GetComponentsInChildren<MuscleTendonUnit>();

        List<MuscleTendonUnitData> unitDatas = new List<MuscleTendonUnitData>();
        foreach (var unit in muscleTendonUnits)
        {
            MuscleTendonUnitData localUnitData = new MuscleTendonUnitData();
            localUnitData.name = unit.gameObject.name;
            localUnitData.normalizedSEESlackLength = unit.normalizedSEESlackLength;
            localUnitData.maxIsometricForce = unit.maxIsometricForce;
            localUnitData.activationConstant = unit.activationConstant;
            foreach (var node in unit.muscleNodes)
            {
                MuscleNodeData localNodeData = new MuscleNodeData();
                localNodeData.boneName = node.bone.gameObject.name;
                localNodeData.offset = node.offset;
                localUnitData.muscleNodeDatas.Add(localNodeData);
            }
            foreach (var segment in unit.muscleSegments)
            {
                MuscleSegmentData localSegmentData = new MuscleSegmentData();

                MuscleNodeData localHeadData = new MuscleNodeData();
                localHeadData.boneName = segment.head.bone.gameObject.name;
                localHeadData.offset = segment.head.offset;
                localSegmentData.head = localHeadData;

                MuscleNodeData localTailData = new MuscleNodeData();
                localTailData.boneName = segment.tail.bone.gameObject.name;
                localTailData.offset = segment.tail.offset;
                localSegmentData.tail = localTailData;

                if (segment.joint)
                    localSegmentData.jiontName = segment.joint.gameObject.name;
                if (segment.jointAnchorBody)
                    localSegmentData.jiontAnchorBodyName = segment.jointAnchorBody.gameObject.name;

                localUnitData.muscleSegmentDatas.Add(localSegmentData);
            }
            unitDatas.Add(localUnitData);
        }
        XMLHelper.SaveToFile(unitDatas, path);
        Debug.Log($"Save Done : Unit Count {unitDatas.Count}");
    }

    [MenuItem("Tool/LoadMuscleTendonUnitData %P")]
    public static void LoadMuscleTendonUnitData()
    {
        GameObject m = Selection.activeGameObject;
        if (m == null)
        {
            Debug.LogWarning($"Unselected Muscle GameObject");
            return;
        }

        string path = EditorUtility.OpenFilePanel("LoadXML", "", "xml");
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Path not Exist");
            return;
        }

        List<MuscleTendonUnitData> unitDatas = XMLHelper.LoadFromFile<List<MuscleTendonUnitData>>(path);

        foreach (var localUnitData in unitDatas)
        {
            // create a child gameobject named localUnitData.name if not exist
            MuscleTendonUnit u = null;
            foreach (var unitObj in m.GetComponentsInChildren<MuscleTendonUnit>())
            {
                if (unitObj.gameObject.name == localUnitData.name)
                {
                    u = unitObj;
                }
            }
            if (u == null)
            {
                GameObject unitObj = new GameObject(localUnitData.name);
                unitObj.transform.parent = m.transform;
                u = unitObj.AddComponent<MuscleTendonUnit>();
            }

            foreach (var unit in m.GetComponentsInChildren<MuscleTendonUnit>())
            {
                if (unit.gameObject.name == localUnitData.name)
                {
                    //NormalizedSEESlackLength
                    unit.normalizedSEESlackLength = localUnitData.normalizedSEESlackLength;
                    //MaxIsometricForce
                    unit.maxIsometricForce = localUnitData.maxIsometricForce;
                    //ActivationConstant
                    unit.activationConstant = localUnitData.activationConstant;

                    //Nodes
                    unit.muscleNodes.Clear();
                    foreach (var localNodeData in localUnitData.muscleNodeDatas)
                    {
                        //Node
                        MuscleNode node = new MuscleNode();
                        unit.muscleNodes.Add(node);
                        //Bone
                        if (!string.IsNullOrEmpty(localNodeData.boneName))
                        {
                            foreach (var nodeObj in m.GetComponentsInChildren<Transform>())
                            {
                                if (nodeObj.gameObject.name == localNodeData.boneName)
                                {

                                    node.bone = nodeObj.gameObject;
                                }
                            }
                        }
                        //Offset
                        node.offset = localNodeData.offset;

                    }

                    //Segments
                    unit.muscleSegments.Clear();
                    foreach (var localSegmentData in localUnitData.muscleSegmentDatas)
                    {
                        //Segment
                        MuscleSegment segment = new MuscleSegment();
                        unit.muscleSegments.Add(segment);
                        //Head
                        MuscleNode head = new MuscleNode();
                        segment.head = head;
                        //Bone
                        if (!string.IsNullOrEmpty(localSegmentData.head.boneName))
                        {
                            foreach (var headObj in m.GetComponentsInChildren<Transform>())
                            {
                                if (headObj.gameObject.name == localSegmentData.head.boneName)
                                {
                                    head.bone = headObj.gameObject;
                                }
                            }
                        }
                        //Offset
                        head.offset = localSegmentData.head.offset;


                        //Tail
                        MuscleNode tail = new MuscleNode();
                        segment.tail = tail;
                        //Bone
                        if (!string.IsNullOrEmpty(localSegmentData.tail.boneName))
                        {
                            foreach (var tailObj in m.GetComponentsInChildren<Transform>())
                            {
                                if (tailObj.gameObject.name == localSegmentData.tail.boneName)
                                {

                                    tail.bone = tailObj.gameObject;
                                }
                            }
                        }
                        //Offset
                        tail.offset = localSegmentData.tail.offset;

                        //Joint
                        if (!string.IsNullOrEmpty(localSegmentData.jiontName))
                        {
                            foreach (var jiontObj in m.GetComponentsInChildren<Joint>())
                            {
                                if (jiontObj.gameObject.name == localSegmentData.jiontName)
                                {
                                    segment.joint = jiontObj;
                                }
                            }
                        }

                        //JointAnchorBody
                        if (!string.IsNullOrEmpty(localSegmentData.jiontAnchorBodyName))
                        {
                            foreach (var jointAnchorBodyObj in m.GetComponentsInChildren<Rigidbody>())
                            {
                                if (jointAnchorBodyObj.gameObject.name == localSegmentData.jiontAnchorBodyName)
                                {
                                    segment.jointAnchorBody = jointAnchorBodyObj;
                                }
                            }
                        }


                    }
                }
            }
        }
        Debug.Log($"Load Done : Unit Count {unitDatas.Count}");
    }
}