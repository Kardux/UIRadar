using UnityEngine;
using System.Collections;

public class IndicatorDisplay : MonoBehaviour
{
	[SerializeField] private Texture2D indicatorTexture;

	private float cameraXSize;
	private float cameraYSize;
	private float cameraXPos;
	private float cameraYPos;

	void Start()
	{
		Rect cameraViewport =  this.GetComponent<Camera>().rect;
		cameraXPos = cameraViewport.x * Screen.width;
		cameraYPos = (1f - cameraViewport.y - cameraViewport.height) * Screen.height;
		cameraXSize = cameraViewport.width * Screen.width;
		cameraYSize = cameraViewport.height * Screen.height;
	}

	void OnGUI()
	{
		GUI.DrawTexture(new Rect(cameraXPos, cameraYPos, cameraXSize, cameraYSize), indicatorTexture);
	}
}
