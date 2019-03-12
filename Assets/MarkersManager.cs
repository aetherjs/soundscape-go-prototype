﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkersManager : MonoBehaviour
{
    private static int playerPrefsCurrentArraySize = 0;
    public int playerPrefsMaxArraySize = 100;
    public static Dictionary<string, Vector2> savedMarkers = new Dictionary<string, Vector2>();
    string [] nameList = new string[playerPrefsCurrentArraySize];
    Vector2 [] coordinateList = new Vector2[playerPrefsCurrentArraySize];


    // Start is called before the first frame update
    void Start()
    {
        ReadPlayerPrefsData();
        DisplayMarkersDictionary();
    }

    private void ReadPlayerPrefsData() 
    {

        nameList = PlayerPrefsX.GetStringArray("nameList", "null", playerPrefsMaxArraySize);
        coordinateList = PlayerPrefsX.GetVector2Array("coordinateList", Vector2.zero, playerPrefsMaxArraySize);

        int j = 0;

        while (nameList[j] != "null")
        {
            j++;
        }

        playerPrefsCurrentArraySize = j;

        for (int i = 0; i < playerPrefsCurrentArraySize; i++)
        {
            savedMarkers.Add(nameList[i], coordinateList[i]);
        }      
    }

    private void SavePlayerPrefsData()
    {
        int i = 0;
        foreach(KeyValuePair<string, Vector2> entry in savedMarkers)
        {
            // do something with entry.Value or entry.Key
            nameList[i] = entry.Key;
            coordinateList[i] = entry.Value;
            i++;
        }
        PlayerPrefsX.SetStringArray("nameList", nameList);
        PlayerPrefsX.SetVector2Array("coordinateList", coordinateList);
    } 

    private void DisplayMarkersDictionary()
    {
        foreach(KeyValuePair<string, Vector2> entry in savedMarkers)
        {
            // do something with entry.Value or entry.Key
            Debug.Log(entry.Key + ": " + entry.Value);
        }
    }

    public void SaveMarker(string name, Vector2 coordinates)
    {
        savedMarkers.Add(name, coordinates);
        playerPrefsCurrentArraySize++;
        DisplayMarkersDictionary();     // Debug 
    }

    public void DeleteMarker(string name)
    {
        savedMarkers.Remove(name);
        playerPrefsCurrentArraySize--;
        DisplayMarkersDictionary();     // Debug
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
