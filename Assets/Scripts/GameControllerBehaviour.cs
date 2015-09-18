using UnityEngine;
using System.Collections;

public class GameControllerBehaviour : MonoBehaviour
{
	[SerializeField] private Texture2D[] iconsTextures;

	void Start()
	{
		GameObject.FindGameObjectWithTag("Camera2").GetComponent<UIRadarV2>().enabled = false;
	}

	void OnGUI()
	{
		//Texture
		for (int i = 0; i < iconsTextures.Length; i++)
		{
			if (GUI.Button(new Rect(25f + 100f * i, 25f, 75f, 75f), iconsTextures[i]))
			{
                //GameObject.FindGameObjectWithTag("MainCamera").GetComponent<UIRadarV2>().SetMarkerSprite(iconsTextures[i]);
			}
		}

		//Size
		if (GUI.Button(new Rect(25f, 125f, 75f, 25f), "Small"))
		{
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<UIRadarV2>().SetScales(0f, 100f);
		}
		if (GUI.Button(new Rect(125f, 125f, 75f, 25f), "Medium"))
		{
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<UIRadarV2>().SetScales(0f, 250f);
		}
		if (GUI.Button(new Rect(225f, 125f, 75f, 25f), "Large"))
		{
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<UIRadarV2>().SetScales(0f, 750f);
		}
		if (GUI.Button(new Rect(325f, 125f, 75f, 25f), "XL"))
		{
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<UIRadarV2>().SetScales(0f, 2000f);
		}

		//Speed
		if (GUI.Button(new Rect(25f, 175f, 75f, 25f), "Very Slow"))
		{
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<UIRadarV2>().SetSpeeds(2f, 2f);
		}
		if (GUI.Button(new Rect(125f, 175f, 75f, 25f), "Slow"))
		{
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<UIRadarV2>().SetSpeeds(15f, 15f);
		}
		if (GUI.Button(new Rect(225f, 175f, 75f, 25f), "Fast"))
		{
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<UIRadarV2>().SetSpeeds(50f, 50f);
		}
		if (GUI.Button(new Rect(325f, 175f, 75f, 25f), "Very Fast"))
		{
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<UIRadarV2>().SetSpeeds(100f, 100f);
		}

		//Cameras
		if (GUI.Button(new Rect(25f, 225f, 75f, 25f), "1 camera"))
		{
			GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().rect = new Rect(0f, 0f, 1f, 1f);
			GameObject.FindGameObjectWithTag("MainCamera").GetComponent<UIRadarV2>().SetCameraSpecifications();
			GameObject.FindGameObjectWithTag("Camera2").GetComponent<UIRadarV2>().enabled = false;
		}
		if (GUI.Button(new Rect(125f, 225f, 75f, 25f), "2 cameras"))
		{
			GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().rect = new Rect(0.1f, 0.1f, 0.35f, 0.8f);
			GameObject.FindGameObjectWithTag("MainCamera").GetComponent<UIRadarV2>().SetCameraSpecifications();
			GameObject.FindGameObjectWithTag("Camera2").GetComponent<UIRadarV2>().enabled = true;
		}
	}
}
