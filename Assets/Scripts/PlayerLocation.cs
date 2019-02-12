﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using Mapbox.Unity.Location;
using Scripts.BingMapClasses;
using Scripts.DistanceCalc;
using UnityEngine.UI;
using TMPro;
using BingSearch;
using BingMapsEntities;
using TTS;

public class PlayerLocation : MonoBehaviour {
	
	private AbstractMap abstractMap; 
	private Vector2d mapCenter;
	public TextMeshProUGUI infoTextMesh;
	public TextMeshProUGUI infoHeaderTextMesh;
	private List<BingMapsClasses.Result> resultsList;
	private TextToSpeechHandler ttsHandler;
	private ILocationProvider locationProvider;
	private BingSearchHandler bsHandler;

	// Use this for initialization
	IEnumerator Start () {
		locationProvider = GameObject.Find ("LocationProviderFactoryObject").GetComponent<LocationProviderFactory> ().TransformLocationProvider;
		Debug.Log (locationProvider);
		abstractMap = GameObject.Find("Map").GetComponent<AbstractMap>();
		ttsHandler = GameObject.Find ("TextToSpeechHandler").GetComponent<TextToSpeechHandler> ();
		bsHandler = GameObject.Find ("BingSearchHandler").GetComponent<BingSearchHandler> ();
		yield return new WaitUntil(() => abstractMap.isReady == true);

		mapCenter = abstractMap.getCenterLongLat ();
		Debug.Log (mapCenter.ToString ());

	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log("CurrentPosition is: " + getLongLat().ToString());
	}

	public Vector2d getLongLat() {
		double newX, newY;
		Vector2d newLatLong;
		/*Vector2d longLatChange = VectorExtensions.GetGeoPosition(transform, mapCenter,  (abstractMap.WorldRelativeScale)); //* 1.5f
		Debug.Log(mapCenter.ToString() + " from getLongLat");
		newX = mapCenter.x + longLatChange.x;
		newY = mapCenter.y + longLatChange.y;*/

		newLatLong = locationProvider.CurrentLocation.LatitudeLongitude;
		return newLatLong;
		//return new Vector2d (newX, newY);


	}

	public Vector3 getUnityPos(double lat, double lon) {
		Vector2 latlon = new Vector2((float)(lat), (float)(lon));
		return VectorExtensions.AsUnityPosition (latlon, abstractMap.CenterMercator, (abstractMap.WorldRelativeScale)); //* 1.5f
	}

	public void getNearbyFeatures() {

		Vector2d currentLocation = getLongLat ();
		double latitude = currentLocation.x;
		double longitude = currentLocation.y;


		infoTextMesh.transform.parent.gameObject.SetActive (true);
		string infoPanelString = "";
		string readableString = "";
		string singleReadableString = "";
		int x = 1;
		List<BingMapsClasses.Result> newResultsList = BingMapsClasses.requestPoiFromBingMaps (latitude, longitude, 5.0, 3); //limitation due to Bing Search API : Allows only 3 calls per second
		foreach (BingMapsClasses.Result result in newResultsList) {
			double resultLong = result.Longitude;
			double resultLat = result.Latitude;
			string displayName = result.DisplayName;
			string additionalInfoUrl = bsHandler.getLinkResult(displayName);
			Vector3 unityPos = getUnityPos (resultLat, resultLong);
			double distanceFromPlayerM = DistanceCalculator.getDistanceBetweenPlaces (longitude, latitude, resultLong, resultLat) * 1000;
			float relativeAngle = getRelativeDirection (unityPos);
			string relativeDirectionString = DistanceCalculator.getRelativeDirectionString (relativeAngle);
			infoPanelString = (infoPanelString + x + ". " + displayName + " , " + result.AddressLine + " , " + BingMapsEntityId.getEntityNameFromId(result.EntityTypeID) + " " + additionalInfoUrl + "\n\n");
			readableString = (readableString + x + ". " + displayName + " , " + result.AddressLine + " , " + BingMapsEntityId.getEntityNameFromId(result.EntityTypeID) + " , " + relativeDirectionString + "\n\n");
			singleReadableString = x + ". " + displayName + " , " + result.AddressLine + " , " + BingMapsEntityId.getEntityNameFromId (result.EntityTypeID) + " , " + relativeDirectionString;
			Debug.Log ("Distance: " + distanceFromPlayerM + " Direction: " + relativeAngle + " which is " + relativeDirectionString);
			StartCoroutine (ttsHandler.GetTextToSpeech (singleReadableString, (x-1)));
			ttsHandler.addAudioDir (unityPos);
			x++;
		}

		infoTextMesh.text = infoPanelString;
		Debug.Log ("THIS PLAYER IS AT: " + getUnityPos (currentLocation.x, currentLocation.y).ToString());
		ttsHandler.StartCoroutine (ttsHandler.playDirAudioQueue ());
		Debug.Log (infoPanelString);




	}

	public void getAheadOfMe() {
		int totalFeatureCount = 5;
		Vector2d currentLocation = getLongLat ();
		double latitude = currentLocation.x;
		double longitude = currentLocation.y;


		infoTextMesh.transform.parent.gameObject.SetActive (true);
		string infoPanelString = "";
		string readableString = "";
		int x = 1;
		int featureCount = 0;
		List<BingMapsClasses.Result> newResultsList = BingMapsClasses.requestPoiFromBingMaps (latitude, longitude, 5.0, 20);
		foreach (BingMapsClasses.Result result in newResultsList) {
			double resultLong = result.Longitude;
			double resultLat = result.Latitude;
			Vector3 unityPos = getUnityPos (resultLat, resultLong);
			float relativeAngle = getRelativeDirection (unityPos);
			if (relativeAngle > -60.0f && relativeAngle < 60.0f) {
				double distanceFromPlayerM = DistanceCalculator.getDistanceBetweenPlaces (longitude, latitude, resultLong, resultLat) * 1000;

				string relativeDirectionString = DistanceCalculator.getRelativeDirectionString (relativeAngle);
				infoPanelString = (infoPanelString + x + ",, " + result.DisplayName + " , " + result.AddressLine + " , " + BingMapsEntityId.getEntityNameFromId (result.EntityTypeID) + "\n\n");
				readableString = (readableString + x + ",, " + result.DisplayName + " , " + result.AddressLine + " , " + BingMapsEntityId.getEntityNameFromId (result.EntityTypeID) + " , " + relativeDirectionString + "\n\n");
				Debug.Log ("Distance: " + distanceFromPlayerM + " Direction: " + relativeAngle + " which is " + relativeDirectionString);
				x++;
				featureCount++;
			}
			if (featureCount == totalFeatureCount) {
				break;
			}
		}
		infoHeaderTextMesh.text = "Ahead of Me";
		infoTextMesh.text = infoPanelString;
		StartCoroutine (ttsHandler.GetTextToSpeech (readableString, 0));
		Debug.Log (infoPanelString);




	}

	 /*IEnumerator requestPoiFromBingMaps(double latitude, double longitude, double distance) {
		string bingMapsApiKey = "Aul2Lj8luxSAtsuBPTb0qlqhXc6kwdTZvQGvGkOc_h_Jg3HI_2F-V6BeeHwHZZ4E";
		string dataAccessId = "c2ae584bbccc4916a0acf75d1e6947b4";
		string dataSourceName = "NavteqEU";
		string entityTypeName = "NavteqPOIs";
		string[] returnParams = { "DisplayName", "Name", "AddressLine", "EntityTypeID", "Latitude", "Longitude" };
		int poiCount = 3;
		string dataFormat = "json";

		
		string queryURL = generateQueryUrlNearby(dataAccessId, dataSourceName, entityTypeName, latitude, longitude, distance, returnParams, poiCount, dataFormat, bingMapsApiKey);
		WWW request = new WWW (queryURL);
		float startTime = Time.time;
		while (request.isDone == false) {
			if (Time.time - startTime > 10) {
				Debug.Log ("API TIMEOUT");
				break;
			}
			Debug.Log ("Waiting : " + (Time.time - startTime) + " seconds elapsed");
		}
		if (request.isDone) {
			string jsonData = System.Text.Encoding.UTF8.GetString (request.bytes, 0, request.bytes.Length);

			BingMapsClasses.RootObject rootObject = BingMapsClasses.getRootObject (jsonData);

			resultsList = rootObject.d.results;
			yield return null;
		}
		resultsList = new List<BingMapsClasses.Result> ();




	}

	private string generateQueryUrlNearby(string dataAccessId, string dataSourceName, string entityTypeName, double latitude, double longitude, double distance, string[] returnParams, int poiCount, string format, string apiKey) {
		string staticEndpoint = "http://spatial.virtualearth.net/REST/v1/data";
		string returnParamsString = "";
		foreach (string param in returnParams) {
			returnParamsString = returnParamsString + param + ",";
		}
		Debug.Log (returnParamsString);
		returnParamsString = returnParamsString.Remove(returnParamsString.Length - 1);
		Debug.Log (returnParamsString);
		string queryURL = (string.Format("{0}/{1}/{2}/{3}?spatialFilter=nearby({4},{5},{6})&$select={7}&$top={8}&$format={9}&key={10}",staticEndpoint, dataAccessId, dataSourceName, entityTypeName, latitude, longitude, distance, returnParamsString, poiCount, format, apiKey));
		Debug.Log (queryURL);
		return queryURL;
	}
	*/


	void updateInfoPanel(Text textBox, string content) {
		textBox.text = content;
	}

	float getRelativeDirection(Vector3 targetPosition) {
		float relativeAngle = 0.0f;
		Vector3 yaxis = new Vector3 (0, 1, 0);
		Vector2d thisLongLat = getLongLat ();
		double thisLat = thisLongLat.x;
		double thisLong = thisLongLat.y;
		Vector3 targetDir = targetPosition - getUnityPos (thisLat, thisLong);
		relativeAngle = Vector3.SignedAngle (transform.forward, targetDir, yaxis);
		return relativeAngle;
	}
}
