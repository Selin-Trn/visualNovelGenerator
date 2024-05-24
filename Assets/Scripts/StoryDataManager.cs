using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class StoryDataManager : MonoBehaviour
{
    public static void SaveStoryData(StoryData data, string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        string json = JsonConvert.SerializeObject(data);
        File.WriteAllText(filePath, json);
    }
}