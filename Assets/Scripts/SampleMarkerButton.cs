﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;


public class SampleMarkerButton : MonoBehaviour {
    
    public Button gotoButtonComponent;
    public Button saveButtonComponent;
    public TMP_Text nameLabel;
    public MarkersManager markersManager;
    

    private Vector2 coordinates;
    private MarkerScrollList scrollList;
    private GameObject UIManagerObject;
    private int markerState = 0;

    
    public void Setup(Destination currentDestination, MarkerScrollList currentScrollList, GameObject UIManager, MarkersManager currentMarkersManager) 
    {
        coordinates = currentDestination.coordinates;
        nameLabel.text = currentDestination.destinationName;
        scrollList = currentScrollList;
        UIManagerObject = UIManager;
        markersManager = currentMarkersManager;
    }

    public void SaveButtonClicked() 
    {
        if (markerState == 0)
            SaveMarker();
        else if (markerState == 1)
            DeleteMarker();
    }

    private void SaveMarker()
    {
        markersManager.SaveMarker(nameLabel.text, coordinates);
        Debug.Log("Marker saved");
        markerState = 1;
        //saveButtonComponent.GetComponent<Image>().color = Color.red;
    }

    private void DeleteMarker()
    {
        markersManager.DeleteMarker(nameLabel.text);
        markerState = 0;
        //saveButtonComponent.GetComponent<Image>().color = Color.white;
        Debug.Log("Marker deleted");

    }
    

}