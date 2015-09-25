#region Author
/************************************************************************************************************
Author: BODEREAU Roy
Website: http://roy-bodereau.fr/
GitHub: https://github.com/Kardux
LinkedIn: be.linkedin.com/in/roybodereau
************************************************************************************************************/
#endregion

#region Copyright
/************************************************************************************************************
CC-BY-SA 4.0
http://creativecommons.org/licenses/by-sa/4.0/
Cette oeuvre est mise a disposition selon les termes de la Licence Creative Commons Attribution 4.0 - Partage dans les Memes Conditions 4.0 International.
************************************************************************************************************/
#endregion

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//////////////////////////////////////////////////////////////////////////
//CLASS
//////////////////////////////////////////////////////////////////////////
public class UIRadarDemoController : MonoBehaviour
{
	#region Variables
	//////////////////////////////////////////////////////////////////////////
	//VARIABLES
	//////////////////////////////////////////////////////////////////////////
    [SerializeField]
    private GameObject m_MultipleRadar;

    [SerializeField]
    private GameObject m_GlobalRadar;

    [SerializeField]
    private Text m_FPSDisplay;

    private float m_DTime;
    private float m_FPS;
    private float m_MS;

    private bool m_Started;
    private bool m_RadarStyle;
	//////////////////////////////////////////////////////////////////////////
	#endregion

	#region Handlers
	//////////////////////////////////////////////////////////////////////////
	//HANDLERS
	//////////////////////////////////////////////////////////////////////////
	void Start()
    {
        m_DTime = 0.0f;

        m_Started = false;
        m_RadarStyle = false;
	}
	
	void Update()
	{
        if (!m_Started)
        {
            m_Started = true;
            m_MultipleRadar.SetActive(m_RadarStyle);
            m_GlobalRadar.SetActive(!m_RadarStyle);
        }

	    if (Input.GetKeyDown(KeyCode.M))
        {
            m_RadarStyle = !m_RadarStyle;
            m_MultipleRadar.SetActive(m_RadarStyle);
            m_GlobalRadar.SetActive(!m_RadarStyle);
        }


        m_DTime += (Time.deltaTime - m_DTime) * 0.1f;

        m_FPS = 1.0f / m_DTime;
        m_MS = m_DTime * 1000.0f;

        m_FPSDisplay.text = string.Format("{1:0.} FPS ({0:0.0}ms)", m_MS, m_FPS);
	}
	//////////////////////////////////////////////////////////////////////////
	#endregion

	#region Methods
	//////////////////////////////////////////////////////////////////////////
	//METHODS
	//////////////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////////////
	#endregion
}
//////////////////////////////////////////////////////////////////////////