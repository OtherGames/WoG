using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class JsonXyeson : MonoBehaviour
{
    public static void SaveFile(List<SaveData> datas)
    {
        var ebala = new HolderJoson { data = datas };
        var json = JsonUtility.ToJson(ebala);
        File.WriteAllText(Application.dataPath + "/Datasos.json", json);
    }

    public static List<SaveData> LoadData()
    {
        var path = Application.dataPath + "/Datasos.json";
        if (File.Exists(path))
        {
            var file = File.ReadAllText(path);
            return JsonUtility.FromJson<HolderJoson>(file).data;
        }
        else
        {
            return null;
        }
    }
}

[System.Serializable]
public class HolderJoson
{
    public List<SaveData> data;
}

[System.Serializable]
public class SaveData
{
    public List<float> inputishe;
    public float actionIndex;

}