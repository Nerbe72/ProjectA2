using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class TableBase
{
    string GetTablePath()
    {
#if UNITY_EDITOR
        return Application.dataPath;
#else
         return Application.persistentDataPath + "/Assets";
#endif
    }

    protected void Load_Binary<T>(string _Name, ref T _Obj)
    {
        var b = new BinaryFormatter();

        b.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;

        string path = Path.Combine("Table", "Table_" + _Name);
        TextAsset asset = Resources.Load(path) as TextAsset;
        Stream stream = new MemoryStream(asset.bytes);

        _Obj = (T)b.Deserialize(stream);

        stream.Close();
    }

    protected void Save_Binary(string _Name, object _Obj)
    {
        string strpath = Path.Combine(GetTablePath(), "Resources", "Table", "Table_" + _Name + ".txt");

        var b = new BinaryFormatter();

        string directoryPath = Path.GetDirectoryName(strpath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        Stream stream = File.Open(strpath, FileMode.OpenOrCreate, FileAccess.Write);
        b.Serialize(stream, _Obj);
        stream.Close();
    }

    protected CSVReader GetCSVReader(string _Name)
    {
        string ext = ".csv";

        FileStream file = new FileStream("./Document/" + _Name + ext, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        StreamReader stream = new StreamReader(file, System.Text.Encoding.UTF8);

        CSVReader reader = new CSVReader();

        reader.parse(stream.ReadToEnd(), false, 1);

        stream.Close();

        return reader;
    }
}
