using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveData
{
    public static void DeleteSaveFile()
    {
        BinaryFormatter bf = new BinaryFormatter();

        string path = Application.persistentDataPath + "/player.data";
        
        if (File.Exists(path))
        {
            File.Delete(path);

            #if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
            #endif
        }
    }

    public static void Save(Data data)
    {
        if (data.path == null)
        {
            Debug.LogError("No path given");
        }
        else
        {
            BinaryFormatter bf = new BinaryFormatter();

            string path = Application.persistentDataPath + "/" + data.path + ".data";

            FileStream stream = new FileStream(path, FileMode.Create);

            bf.Serialize(stream, data);
            stream.Close();

            Debug.Log("Saved on \"" + path + "\" with values: " + data.ToString());
        }
    }

    public static Data Load(string datapath)
    {
        string path = Application.persistentDataPath +"/"+ datapath + ".data";
        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            Data data = bf.Deserialize(stream) as Data;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }
}

