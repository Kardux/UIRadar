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

using UnityEditor;
using UnityEngine;
using System.Collections;

//////////////////////////////////////////////////////////////////////////
//CLASS
//////////////////////////////////////////////////////////////////////////
[CustomEditor(typeof(UIRadarV2))]
public class UIRadarV2_CustomEditor : Editor
{
	#region Variables
	//////////////////////////////////////////////////////////////////////////
	//VARIABLES
	//////////////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////////////
	#endregion

	#region Handlers
	//////////////////////////////////////////////////////////////////////////
	//HANDLERS
	//////////////////////////////////////////////////////////////////////////
	void Start()
	{
	
	}
	
	void Update()
	{
	
	}

    public override void OnInspectorGUI()
    {
        UIRadarV2 _Target = target as UIRadarV2;

        //_Target.m_MarkerSprite = EditorGUILayout.ObjectField( _Target.m_MarkerSprite, Sprite) as Sprite;

        _Target.m_Tag = EditorGUILayout.TagField("Tag", _Target.m_Tag);

        _Target.m_MaxMarkerScale = EditorGUILayout.Slider("MaxMarkerScale", _Target.m_MaxMarkerScale, _Target.m_MinMarkerScale, 100.0f);
        _Target.m_MinMarkerScale = EditorGUILayout.Slider("MinMarkerScale", _Target.m_MinMarkerScale, 0.0f, _Target.m_MaxMarkerScale);

        _Target.m_MaxDistance = EditorGUILayout.Slider("MaxDistance", _Target.m_MaxDistance, _Target.m_MinDistance, 1000.0f);
        _Target.m_MinDistance = EditorGUILayout.Slider("MinDistance", _Target.m_MinDistance, 0.0f, _Target.m_MaxDistance);

        _Target.m_ScalingSpeed = EditorGUILayout.Slider("ScalingSpeed", _Target.m_ScalingSpeed, 0.0f, 50.0f);
        _Target.m_MovingSpeed = EditorGUILayout.Slider("MovingSpeed", _Target.m_MovingSpeed, 0.0f, 50.0f);

        _Target.m_AlphaBlending = EditorGUILayout.Toggle("AlphaBlending", _Target.m_AlphaBlending);
        if (_Target.m_AlphaBlending)
        {
            _Target.m_AlphaStartPercentage = EditorGUILayout.FloatField("AlphaStartPercentage", _Target.m_AlphaStartPercentage);
        }

        _Target.m_DirectViewOnly = EditorGUILayout.Toggle("DirectViewOnly", _Target.m_DirectViewOnly);
        if (_Target.m_DirectViewOnly)
        {
            _Target.m_RaycastLayer = LayerMask.LayerToName(EditorGUILayout.LayerField("RaycastLayer", LayerMask.NameToLayer(_Target.m_RaycastLayer)));
        }

        /*
        TestPropertyManager temp = target as TestPropertyManager;

        temp.TestA = EditorGUILayout.FloatField("Test A", temp.TestA);
        temp.TestB = EditorGUILayout.FloatField("Test B", temp.TestB);

        temp.TestManager.TestC = EditorGUILayout.FloatField("Test C", temp.TestManager.TestC);
        temp.TestManager.TestD = EditorGUILayout.FloatField("Test D", temp.TestManager.TestD);
        */
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