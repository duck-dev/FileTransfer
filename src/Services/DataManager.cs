using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace FileTransfer.Services;

internal static class DataManager
{
    internal static bool LoadData<T>(string path, out T? data) where T : class
    {
        data = null;
        if (!File.Exists(path))
            return false;

        string content = File.ReadAllText(path);
        var deserializedContent = JsonSerializer.Deserialize<T>(content);
        if(deserializedContent is not null)
            data = deserializedContent;
        return true;
    }

    internal static void SaveData<T>(T data, string path)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(data, options);
        File.WriteAllText(path, jsonString);
    }

    internal static void AddData<T>(T data, ICollection<T> dataCollection, string path)
    {
        dataCollection.Add(data);
        SaveData(data, path);
    }

    internal static bool TestLoadData<T>(string path)
    {
        string content = File.ReadAllText(path);
        try
        {
            JsonSerializer.Deserialize<List<T>>(content);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}