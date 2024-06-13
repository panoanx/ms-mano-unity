using Robotflow;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEditor;

// public class TestAxisAngle : MonoBehaviour
// {
//     [MenuItem("Tool/PreprocessMano")]
//     public static void PreprocessMano()
//     {
        
//         // Open file explorer to select a JSON file
//         string filePath = UnityEditor.EditorUtility.OpenFilePanel("Select JSON file", "", "json");

//         // Read the contents of the file
//         string json = File.ReadAllText(filePath);

//         // Deserialize the JSON into an object
//         MyObject obj = JsonConvert.DeserializeObject<MyObject>(json);

//         // Do something with the object
//         Debug.Log(obj.Property1);
//     }
// }

// public class MyObject
// {
//     public string Property1 { get; set; }
//     public int Property2 { get; set; }
// }