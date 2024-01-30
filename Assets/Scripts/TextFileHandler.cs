using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

public static class TextFileHandler
{
    private static readonly string filePath = Path.Combine(Application.persistentDataPath, "playerDatas.json");

    internal static void WritePlayerData(PlayerData playerData)
    {
        try
        {
            string jsonData = JsonConvert.SerializeObject(playerData);
            File.WriteAllText(filePath, jsonData);
        }
        catch (Exception e)
        {
            Debug.LogError("Veriler yazılırken hata oluştu: " + e.Message);
        }
    }

    internal static PlayerData ReadPlayerData()
    {
        try
        {
            if(File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                PlayerData playerData = JsonConvert.DeserializeObject<PlayerData>(jsonData);

                return playerData;
            }
            else
            {
                Debug.LogError("Dosya Bulunamadı");
                return null;
            }
        }
        catch(Exception e)
        {
            Debug.Log("Veriler okunurken hata oluştu: " + e.Message);
            return null;
        }
    }
}