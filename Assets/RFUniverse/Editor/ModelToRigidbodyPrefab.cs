﻿using System.IO;
using UnityEngine;
using UnityEditor;
using RFUniverse.Attributes;
public class ModelToRigidbodyPrefab : Editor
{
    [MenuItem("RFUniverse/Model To Rigidbody Prefab")]
    public static void Generate()
    {
        GameObject[] objs = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
        Debug.Log(objs.Length);
        foreach (var item in objs)
        {
            string file = AssetDatabase.GetAssetPath(item);
            string path = Path.GetDirectoryName(file);
            string name = Path.GetFileNameWithoutExtension(file);
            Debug.Log(path);
            Debug.Log(name);

            GameObject prefab = GameObject.Instantiate(item);
            RigidbodyAttr attr = prefab.AddComponent<RigidbodyAttr>();
            RigidbodyAttr.SaveMeshs($"{path}/{name}_VHACD.asset", attr.GenerateVHACDCollider());
            PrefabUtility.SaveAsPrefabAsset(prefab, $"{path}/{name}_RigidbodyAttr.prefab");
            DestroyImmediate(prefab);
        }
        AssetDatabase.Refresh();
    }
}
