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
using System.Collections.Generic;

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
    private bool m_GeneralSettingsFoldout = true;
    private bool m_ColorSettingsFoldout = true;
    private bool m_OrientationSettingsFoldout = true;
    private bool m_AlphaBlendingSettingsFoldout = true;
    private bool m_LerpingSettingsFoldout = true;

    private UIRadarV2.ColorMode m_PreviousColorMode;

    private GUIContent m_GUIContent;
	//////////////////////////////////////////////////////////////////////////
	#endregion

	#region Handlers
	//////////////////////////////////////////////////////////////////////////
	//HANDLERS
	//////////////////////////////////////////////////////////////////////////
    public override void OnInspectorGUI()
    {
        EditorGUI.indentLevel++;
        UIRadarV2 _Target = target as UIRadarV2;
        m_GUIContent = new GUIContent("", "");

        if (m_GeneralSettingsFoldout = EditorGUILayout.Foldout(m_GeneralSettingsFoldout, "General settings"))
        {
            EditorGUI.indentLevel++;
            m_GUIContent = new GUIContent("MarkerSprite", "");
            _Target.m_MarkerSprite = (Sprite)EditorGUILayout.ObjectField(m_GUIContent, _Target.m_MarkerSprite, typeof(Sprite), true);
            if (!_Target.m_MarkerSprite)
                EditorGUILayout.HelpBox("No sprite currently assigned to your radar.", MessageType.Warning, true);

            m_GUIContent = new GUIContent("Tag", "");
            _Target.m_Tag = EditorGUILayout.TagField(m_GUIContent, _Target.m_Tag);

            m_GUIContent = new GUIContent("MaxMarkerScale", "");
            _Target.m_MaxMarkerScale = EditorGUILayout.Slider(m_GUIContent, _Target.m_MaxMarkerScale, _Target.m_MinMarkerScale, 100.0f);
            m_GUIContent = new GUIContent("MinMarkerScale", "");
            _Target.m_MinMarkerScale = EditorGUILayout.Slider(m_GUIContent, _Target.m_MinMarkerScale, 0.0f, _Target.m_MaxMarkerScale);

            m_GUIContent = new GUIContent("MaxDistance", "");
            _Target.m_MaxDistance = EditorGUILayout.Slider(m_GUIContent, _Target.m_MaxDistance, _Target.m_MinDistance, 1000.0f);
            m_GUIContent = new GUIContent("MinDistance", "");
            _Target.m_MinDistance = EditorGUILayout.Slider(m_GUIContent, _Target.m_MinDistance, 0.0f, _Target.m_MaxDistance);
            EditorGUI.indentLevel--;
        }

        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

        if (m_ColorSettingsFoldout = EditorGUILayout.Foldout(m_ColorSettingsFoldout, "Color settings"))
        {
            EditorGUI.indentLevel++;
            m_GUIContent = new GUIContent("ColorMode", "");
            switch (_Target.m_ColorMode = (UIRadarV2.ColorMode)EditorGUILayout.EnumPopup(m_GUIContent, _Target.m_ColorMode))
            {
                case UIRadarV2.ColorMode.SingleColor :
                    if (m_PreviousColorMode != UIRadarV2.ColorMode.SingleColor)
                    {
                        if (_Target.m_MarkerColors.Count == 0)
                            _Target.m_MarkerColors.Add(Color.white);
                    }

                    m_GUIContent = new GUIContent("MarkerColor", "");
                    _Target.m_MarkerColors[0] = EditorGUILayout.ColorField(m_GUIContent, _Target.m_MarkerColors[0]);

                    m_PreviousColorMode = UIRadarV2.ColorMode.SingleColor;
                    break;

                case UIRadarV2.ColorMode.SimpleGradient:
                    EditorGUI.indentLevel++;
                    if (m_PreviousColorMode != UIRadarV2.ColorMode.SimpleGradient)
                    {
                        if (_Target.m_MarkerColors.Count == 0)
                        {
                            _Target.m_MarkerColors.Add(Color.white);
                            _Target.m_MarkerColors.Add(Color.white);
                        }
                        else if (_Target.m_MarkerColors.Count == 1)
                        {
                            _Target.m_MarkerColors.Add(Color.white);
                        }
                    }

                    m_GUIContent = new GUIContent("MinColor", "Color displayed at minimum distance");
                    _Target.m_MarkerColors[0] = EditorGUILayout.ColorField(m_GUIContent, _Target.m_MarkerColors[0]);
                    m_GUIContent = new GUIContent("MaxColor", "Color displayed at maximum distance");
                    _Target.m_MarkerColors[1] = EditorGUILayout.ColorField(m_GUIContent, _Target.m_MarkerColors[1]);

                    m_PreviousColorMode = UIRadarV2.ColorMode.SimpleGradient;
                    EditorGUI.indentLevel--;
                    break;

                case UIRadarV2.ColorMode.MultipleColors:
                    EditorGUI.indentLevel++;
                    if (m_PreviousColorMode != UIRadarV2.ColorMode.MultipleColors)
                    {
                        if (_Target.m_MarkerColors.Count == 0)
                        {
                            _Target.m_MarkerColors.Add(Color.white);
                            _Target.m_MarkerColors.Add(Color.white);
                        }
                        else if (_Target.m_MarkerColors.Count == 1)
                        {
                            _Target.m_MarkerColors.Add(Color.white);
                        }

                        if (_Target.m_MarkerColors.Count < 2)
                            _Target.m_MarkersColorsPercentages = new List<float>() { 0.0f, 1.0f };
                    }

                    m_GUIContent = new GUIContent("ColorGradient", "If activated, color will go from one to another following a gradient");
                    _Target.m_GradienColorTransition = EditorGUILayout.Toggle(m_GUIContent, _Target.m_GradienColorTransition);

                    for (int i = 0; i < _Target.m_MarkerColors.Count; i ++)
                    {
                        EditorGUILayout.BeginHorizontal();

                        m_GUIContent = new GUIContent((i == 0 ? "MinColor" : (i == _Target.m_MarkerColors.Count - 1 ? "MaxColor" : "Color" + i.ToString())), "Edit color " + i.ToString() + " properties (color - distance %)");
                        EditorGUILayout.LabelField(m_GUIContent, GUILayout.MaxWidth(110.0f));

                        if (i != _Target.m_MarkerColors.Count - 1 || _Target.m_GradienColorTransition)
                            _Target.m_MarkerColors[i] = EditorGUILayout.ColorField(_Target.m_MarkerColors[i]);

                        if (i == 0)
                        {
                            _Target.m_MarkersColorsPercentages[i] = EditorGUILayout.Slider(_Target.m_MarkersColorsPercentages[i] * 100.0f, 0.0f, 0.0f) / 100.0f;
                        }
                        else if (i == _Target.m_MarkerColors.Count - 1)
                        {
                            _Target.m_MarkersColorsPercentages[i] = EditorGUILayout.Slider(_Target.m_MarkersColorsPercentages[i] * 100.0f, 100.0f, 100.0f) / 100.0f;
                        }
                        else
                        {
                            _Target.m_MarkersColorsPercentages[i] = EditorGUILayout.Slider(_Target.m_MarkersColorsPercentages[i] * 100.0f, _Target.m_MarkersColorsPercentages[i - 1] * 100.0f, _Target.m_MarkersColorsPercentages[i + 1] * 100.0f) / 100.0f;
                        }

                        Color _GUIColor = GUI.color;
                        GUI.enabled = !(i == _Target.m_MarkerColors.Count - 1);
                        GUI.color = (i == _Target.m_MarkerColors.Count - 1 ? new Color(0.0f, 0.0f, 0.0f, 0.0f) : _GUIColor);
                        m_GUIContent = new GUIContent("+", "");
                        if (GUILayout.Button(m_GUIContent, EditorStyles.miniButtonLeft))
                        {
                            _Target.m_MarkerColors.Insert(i + 1, Color.white);
                            _Target.m_MarkersColorsPercentages.Insert(i + 1, (_Target.m_MarkersColorsPercentages[i] + _Target.m_MarkersColorsPercentages[i + 1]) * 0.5f);
                        }
                        GUI.color = _GUIColor;
                        GUI.enabled = (_Target.m_MarkerColors.Count > 2);
                        m_GUIContent = new GUIContent("-", "");
                        if (GUILayout.Button(m_GUIContent, EditorStyles.miniButtonRight))
                        {
                            _Target.m_MarkerColors.RemoveAt(i);
                            _Target.m_MarkersColorsPercentages.RemoveAt(i);
                        }
                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();
                    }

                    m_PreviousColorMode = UIRadarV2.ColorMode.MultipleColors;
                    EditorGUI.indentLevel--;
                    break;
            }
            EditorGUI.indentLevel--;
        }

        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

        if (m_OrientationSettingsFoldout = EditorGUILayout.Foldout(m_OrientationSettingsFoldout, "Orientation settings"))
        {
            EditorGUI.indentLevel++;
            m_GUIContent = new GUIContent("RotationSpeedMode", "");
            switch (_Target.m_RotationSpeedMode = (UIRadarV2.RotationSpeedMode)EditorGUILayout.EnumPopup(m_GUIContent, _Target.m_RotationSpeedMode))
            {
                case UIRadarV2.RotationSpeedMode.Constant :
                    m_GUIContent = new GUIContent("RotationSpeed", "");
                    _Target.m_MinRotationSpeed = EditorGUILayout.Slider(m_GUIContent, _Target.m_MinRotationSpeed, -50.0f, 50.0f);
                    break;

                case UIRadarV2.RotationSpeedMode.OverDistance :
                    m_GUIContent = new GUIContent("MinRotationSpeed", "Rotation speed at minimum distance");
                    _Target.m_MinRotationSpeed = EditorGUILayout.Slider(m_GUIContent, _Target.m_MinRotationSpeed, -50.0f, 50.0f);
                    m_GUIContent = new GUIContent("MaxRotationSpeed", "Rotation speed at maximum distance");
                    _Target.m_MaxRotationSpeed = EditorGUILayout.Slider(m_GUIContent, _Target.m_MaxRotationSpeed, -50.0f, 50.0f);
                    break;

                case UIRadarV2.RotationSpeedMode.Random :
                    m_GUIContent = new GUIContent("MinRotationSpeed", "Minimum range of random rotation speed (applied on start)");
                    _Target.m_MinRotationSpeed = EditorGUILayout.Slider(m_GUIContent, _Target.m_MinRotationSpeed, -100.0f, 100.0f);
                    m_GUIContent = new GUIContent("MaxRotationSpeed", "Maximum range of random rotation speed (applied on start)");
                    _Target.m_MaxRotationSpeed = EditorGUILayout.Slider(m_GUIContent, _Target.m_MaxRotationSpeed, -100.0f, 100.0f);
                    break;
            }
            EditorGUI.indentLevel--;
        }

        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

        if (_Target.m_UseAlphaBlending = EditorGUILayout.Toggle("UseAlphaBlending", _Target.m_UseAlphaBlending))
        {
            if (m_AlphaBlendingSettingsFoldout = EditorGUILayout.Foldout(m_AlphaBlendingSettingsFoldout, "Alpha blending settings"))
            {
                EditorGUI.indentLevel++;
                m_GUIContent = new GUIContent("AlphaStartPercentage", "");
                _Target.m_AlphaStartPercentage = EditorGUILayout.Slider(m_GUIContent, _Target.m_AlphaStartPercentage, 0.0f, 0.5f);

                if (_Target.m_UseCustomAlphaLimits = EditorGUILayout.Toggle("UseCustomAlphaLimits", _Target.m_UseCustomAlphaLimits))
                {
                    _Target.m_MinAlpha = EditorGUILayout.Slider("MinAlpha", _Target.m_MinAlpha, 0.0f, _Target.m_MaxAlpha);
                    _Target.m_MaxAlpha = EditorGUILayout.Slider("MaxAlpha", _Target.m_MaxAlpha, _Target.m_MinAlpha, 1.0f);
                }
                EditorGUI.indentLevel--;
            }
        }

        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

        if (_Target.m_DirectViewOnly = EditorGUILayout.Toggle("DirectViewOnly", _Target.m_DirectViewOnly))
            _Target.m_RaycastLayer = LayerMask.LayerToName(EditorGUILayout.LayerField("RaycastLayer", LayerMask.NameToLayer(_Target.m_RaycastLayer)));

        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

        if (_Target.m_UseLerps = EditorGUILayout.Toggle("UseLerps", _Target.m_UseLerps))
        {
            if (m_LerpingSettingsFoldout = EditorGUILayout.Foldout(m_LerpingSettingsFoldout, "Lerp settings"))
            {
                EditorGUI.indentLevel++;
                if (_Target.m_LerpScales = EditorGUILayout.Toggle("LerpScales", _Target.m_LerpScales))
                    _Target.m_ScalingSpeed = EditorGUILayout.Slider("ScalingSpeed", _Target.m_ScalingSpeed, 0.0f, 50.0f);

                if (_Target.m_LerpMoves = EditorGUILayout.Toggle("LerpMoves", _Target.m_LerpMoves))
                    _Target.m_MovingSpeed = EditorGUILayout.Slider("MovingSpeed", _Target.m_MovingSpeed, 0.0f, 50.0f);

                if (_Target.m_UseAlphaBlending)
                {
                    if (_Target.m_LerpAlphaChanges = EditorGUILayout.Toggle("LerpAlphaChanges", _Target.m_LerpAlphaChanges))
                        _Target.m_AlphaChangingSpeed = EditorGUILayout.Slider("AlphaChangingSpeed", _Target.m_AlphaChangingSpeed, 0.0f, 50.0f);
                }

                if (_Target.m_LerpColors = EditorGUILayout.Toggle("LerpColors", _Target.m_LerpColors))
                    _Target.m_ColoringSpeed = EditorGUILayout.Slider("ColoringSpeed", _Target.m_ColoringSpeed, 0.0f, 50.0f);

                if (_Target.m_LerpRotations = EditorGUILayout.Toggle("LerpRotations", _Target.m_LerpRotations))
                    _Target.m_RotatingLerpSpeed = EditorGUILayout.Slider("RotatingSpeed", _Target.m_RotatingLerpSpeed, 0.0f, 50.0f);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
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