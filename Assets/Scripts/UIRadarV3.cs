#region Author
/************************************************************************************************************
Author: BODEREAU Roy
Website: http://roy-bodereau.fr/
GitHub: https://github.com/Kardux
LinkedIn: https://fr.linkedin.com/pub/roy-bodereau/b2/94/82b
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
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

//////////////////////////////////////////////////////////////////////////
//CLASS
//////////////////////////////////////////////////////////////////////////
[RequireComponent(typeof(RectTransform))]
public class UIRadarV3 : MonoBehaviour
{
    #region Variables
    //////////////////////////////////////////////////////////////////////////
    //VARIABLES
    //////////////////////////////////////////////////////////////////////////
#pragma warning disable 0414
    // Camera
    public Camera m_Camera = null;

    // Tag
    public string m_Tag = "";

    // Sprite
    public Sprite m_Sprite = null;
    public Vector2 m_BaseSize = new Vector2(100.0f, 100.0f);

    // Distance
    public float m_CloseDistance = 2.0f;
    public float m_FarDistance = 40.0f;

    // Rotation
    public RotationSpeedMode m_RotationSpeedMode = RotationSpeedMode.Constant;
    public float m_CloseRotateAmount = 20.0f;
    public float m_FarRotateAmount = 0.0f;

    // Scale
    public float m_CloseScale = 1.0f;
    public float m_FarScale = 0.0f;

    // Color
    public bool m_UseGradient = false;
    public Color m_Color = Color.white;
    public Gradient m_Gradient = new Gradient();

    // Lerping
    public bool m_UseLerps = false;
    public bool m_LerpMoves = false;
    public float m_MovingSpeed = 5.0f;
    public bool m_LerpRotations = false;
    public float m_RotatingSpeed = 5.0f;
    public bool m_LerpScales = false;
    public float m_ScalingSpeed = 5.0f;
    public bool m_LerpColor = false;
    public float m_ColoringSpeed = 5.0f;
    public bool m_LerpAlpha = false;
    public float m_AlphaSpeed = 5.0f;

    // Alpha blending
    public bool m_UseAlphaBlending = false;
    public float m_AlphaStartPercentage = 0.2f;
    public bool m_UseCustomAlphaLimits = false;
    public float m_MinAlpha = 0.0f;
    public float m_MaxAlpha = 0.75f;

    // Display only if target can be seen
    public bool m_DirectViewOnly = false;
    public LayerMask m_RaycastLayers = -1;

    public class RadarMarker
    {
        public Transform m_Target;

        public RectTransform m_RectTransform;
        public Vector2 m_CurrentAnchor;
        public Vector2 m_TargetAnchor;
        public Vector3 m_CurrentRotateAmount;
        public Vector3 m_TargetRotateAmount;
        public float m_CurrentLocalScale;
        public float m_TargetLocalScale;

        public Color m_CurrentColor;
        public Color m_TargetColor;
        public float m_CurrentAlpha;
        public float m_TargetAlpha;

        public RadarMarker()
        {
            m_Target = null;
            m_RectTransform = null;
            m_CurrentAnchor = Vector2.zero;
            m_TargetAnchor = Vector2.zero;
            m_CurrentRotateAmount = Vector3.zero;
            m_TargetRotateAmount = Vector3.zero;
            m_CurrentLocalScale = 1.0f;
            m_TargetLocalScale = 0.0f;
            m_CurrentColor = Color.white;
            m_TargetColor = Color.white;
            m_CurrentAlpha = 1.0f;
            m_TargetAlpha = 1.0f;
        }
    }

    public enum RotationSpeedMode { Constant, OverDistance, Random };

    private List<RadarMarker> m_Markers = new List<RadarMarker>();

    private Rect m_RadarRect;
    private Vector2 m_MarkersSizeRatio;

    private float m_TargetDistance;
    private bool m_TargetInDirectView;
    private Vector3 m_WorldToScreenPosition;

    private bool m_RotationEnabled;
#pragma warning restore 0414
    //////////////////////////////////////////////////////////////////////////
    #endregion

    #region Handlers
    //////////////////////////////////////////////////////////////////////////
    //HANDLERS
    //////////////////////////////////////////////////////////////////////////
    protected void Reset()
    {
        m_Camera = Camera.main;
    }

    protected void Awake()
    {
        //Check if the tag exists in the project - May be obsolete in the future
#if UNITY_EDITOR
        bool checkTag = false;
        for(int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
        {
            if(UnityEditorInternal.InternalEditorUtility.tags[i].Equals(m_Tag))
            {
                checkTag = true;
            }
        }

        if(!checkTag)
        {
            Debug.LogWarning("<b>" + m_Tag + "</b> tag not found in the project.");
        }

        if(!m_Sprite)
        {
            Debug.LogWarning("No sprite currently assigned to your radar.");
        }
#endif

        m_Markers = new List<RadarMarker>();
        m_RotationEnabled = CheckForRotationSpeed();
    }

    protected void Start()
    {
        m_RadarRect = GetRectTransformRect(GetComponent<RectTransform>());
        UpdateMarkers();
    }

    protected void Update()
    {
        for(int i = 0; i < m_Markers.Count; i++)
        {
            // Global marker informations
            m_WorldToScreenPosition = m_Camera.WorldToViewportPoint(m_Markers[i].m_Target.position);
            m_TargetDistance = Vector3.Distance(m_Markers[i].m_Target.position, m_Camera.transform.position);

            // Direct view
            m_TargetInDirectView = false;

            if(m_DirectViewOnly && m_TargetDistance <= m_FarDistance)
            {
                RaycastHit hitInfos = new RaycastHit();
                if(Physics.Raycast(m_Camera.transform.position, m_Markers[i].m_Target.position - m_Camera.transform.position, out hitInfos, m_FarDistance, m_RaycastLayers))
                {
                    m_TargetInDirectView = hitInfos.collider.transform.Equals(m_Markers[i].m_Target);
                }
            }
            else
            {
                m_TargetInDirectView = true;
            }

            // Anchor
            bool targetInRange = false;

            if(!m_TargetInDirectView || m_TargetDistance > m_FarDistance || m_WorldToScreenPosition.z < 0.0f)
            {
                if(m_WorldToScreenPosition.x < m_RadarRect.xMin + m_RadarRect.width * 0.5f)
                {
                    m_Markers[i].m_TargetAnchor.x = 0.0f;
                }
                else
                {
                    m_Markers[i].m_TargetAnchor.x = 1.0f;
                }

                if(m_WorldToScreenPosition.y < m_RadarRect.yMin + m_RadarRect.height * 0.5f)
                {
                    m_Markers[i].m_TargetAnchor.y = 0.0f;
                }
                else
                {
                    m_Markers[i].m_TargetAnchor.y = 1.0f;
                }

                m_Markers[i].m_TargetLocalScale = 0.0f;
            }
            else
            {
                m_Markers[i].m_TargetAnchor.x = Mathf.Clamp(m_WorldToScreenPosition.x, 0.0f, 1.0f);
                m_Markers[i].m_TargetAnchor.y = Mathf.Clamp(m_WorldToScreenPosition.y, 0.0f, 1.0f);

                targetInRange = true;
            }

            // Rotation
            if(m_RotationEnabled)
            {
                switch(m_RotationSpeedMode)
                {
                    case RotationSpeedMode.Constant:
                        m_Markers[i].m_TargetRotateAmount.z = m_CloseRotateAmount;
                        break;

                    case RotationSpeedMode.OverDistance:
                        m_Markers[i].m_TargetRotateAmount.z = m_FarRotateAmount + (m_TargetDistance - m_FarDistance) / (m_CloseDistance - m_FarDistance) * (m_CloseRotateAmount - m_FarRotateAmount);
                        break;

                    case RotationSpeedMode.Random:
                        break;

                    default:
                        break;
                }
            }

            // Scale
            if(m_TargetDistance > m_FarDistance || m_TargetDistance < m_CloseDistance || m_WorldToScreenPosition.z < 0.0f)
            {
                m_Markers[i].m_TargetLocalScale = m_FarScale;
            }
            else
            {
                m_Markers[i].m_TargetLocalScale = m_FarScale + (m_TargetDistance - m_FarDistance) / (m_CloseDistance - m_FarDistance) * (m_CloseScale - m_FarScale);
            }

            // Color
            if(!m_UseGradient)
            {
                m_Markers[i].m_TargetColor = m_Color;
            }
            else
            {
                m_Markers[i].m_TargetColor = (targetInRange ? m_Gradient.Evaluate((m_TargetDistance - m_CloseDistance) / (m_FarDistance - m_CloseDistance)) : m_Gradient.Evaluate(1.0f));
            }

            // Alpha blending
            if(!targetInRange || m_WorldToScreenPosition.x < m_RadarRect.xMin || m_WorldToScreenPosition.x > m_RadarRect.xMax || m_WorldToScreenPosition.y < m_RadarRect.yMin || m_WorldToScreenPosition.y > m_RadarRect.yMax)
            {
                m_Markers[i].m_TargetAlpha = m_MinAlpha;
            }
            else if(m_UseAlphaBlending)
            {
                float xAlpha = 1.0f;
                float yAlpha = 1.0f;

                if(m_WorldToScreenPosition.x < m_RadarRect.xMin + (m_RadarRect.width * m_AlphaStartPercentage) + m_MarkersSizeRatio.x * 0.5f)
                {
                    xAlpha = Mathf.Clamp((m_WorldToScreenPosition.x - m_RadarRect.xMin - m_MarkersSizeRatio.x * 0.5f) / (m_AlphaStartPercentage * m_RadarRect.width), 0.0f, 1.0f);
                }
                else if(m_WorldToScreenPosition.x > m_RadarRect.xMax - (m_RadarRect.width * m_AlphaStartPercentage) - m_MarkersSizeRatio.x * 0.5f)
                {
                    xAlpha = Mathf.Clamp((m_RadarRect.xMax - m_WorldToScreenPosition.x - m_MarkersSizeRatio.x * 0.5f) / (m_AlphaStartPercentage * m_RadarRect.width), 0.0f, 1.0f);
                }

                if(m_WorldToScreenPosition.y < m_RadarRect.yMin + (m_RadarRect.height * m_AlphaStartPercentage) + m_MarkersSizeRatio.y * 0.5f)
                {
                    yAlpha = Mathf.Clamp((m_WorldToScreenPosition.y - m_RadarRect.yMin - m_MarkersSizeRatio.y * 0.5f) / (m_AlphaStartPercentage * m_RadarRect.height), 0.0f, 1.0f);
                }
                else if(m_WorldToScreenPosition.y > m_RadarRect.yMax - (m_RadarRect.height * m_AlphaStartPercentage) - m_MarkersSizeRatio.y * 0.5f)
                {
                    yAlpha = Mathf.Clamp((m_RadarRect.yMax - m_WorldToScreenPosition.y - m_MarkersSizeRatio.y * 0.5f) / (m_AlphaStartPercentage * m_RadarRect.height), 0.0f, 1.0f);
                }

                m_Markers[i].m_TargetAlpha = (xAlpha < yAlpha ? m_MinAlpha + (m_MaxAlpha - m_MinAlpha) * xAlpha : m_MinAlpha + (m_MaxAlpha - m_MinAlpha) * yAlpha);
            }
            else
            {
                m_Markers[i].m_TargetAlpha = m_MaxAlpha;
            }

            // Lerps
            if(m_UseLerps)
            {
                if(m_LerpScales)
                {
                    m_Markers[i].m_CurrentLocalScale = Mathf.Lerp(m_Markers[i].m_CurrentLocalScale, m_Markers[i].m_TargetLocalScale, m_ScalingSpeed * Time.deltaTime);
                }
                else
                {
                    m_Markers[i].m_CurrentLocalScale = m_Markers[i].m_TargetLocalScale;
                }

                if(m_LerpMoves)
                {
                    m_Markers[i].m_CurrentAnchor.x = Mathf.Lerp(m_Markers[i].m_CurrentAnchor.x, m_Markers[i].m_TargetAnchor.x, m_MovingSpeed * Time.deltaTime);
                    m_Markers[i].m_CurrentAnchor.y = Mathf.Lerp(m_Markers[i].m_CurrentAnchor.y, m_Markers[i].m_TargetAnchor.y, m_MovingSpeed * Time.deltaTime);
                }
                else
                {
                    m_Markers[i].m_CurrentAnchor.x = m_Markers[i].m_TargetAnchor.x;
                    m_Markers[i].m_CurrentAnchor.y = m_Markers[i].m_TargetAnchor.y;
                }

                if(m_LerpAlpha)
                {
                    m_Markers[i].m_CurrentAlpha = Mathf.Lerp(m_Markers[i].m_CurrentAlpha, m_Markers[i].m_TargetAlpha, m_AlphaSpeed * Time.deltaTime);
                }
                else
                {
                    m_Markers[i].m_CurrentAlpha = m_Markers[i].m_TargetAlpha;
                }

                if(m_LerpColor && m_UseGradient)
                {
                    m_Markers[i].m_CurrentColor = Color.Lerp(m_Markers[i].m_CurrentColor, m_Markers[i].m_TargetColor, m_ColoringSpeed * Time.deltaTime);
                }
                else
                {
                    m_Markers[i].m_CurrentColor = m_Markers[i].m_TargetColor;
                }

                if(m_LerpRotations)
                {
                    m_Markers[i].m_CurrentRotateAmount = Vector3.Lerp(m_Markers[i].m_CurrentRotateAmount, m_Markers[i].m_TargetRotateAmount, m_RotatingSpeed * Time.deltaTime);
                }
                else
                {
                    m_Markers[i].m_CurrentRotateAmount = m_Markers[i].m_TargetRotateAmount;
                }
            }
            else
            {
                m_Markers[i].m_CurrentLocalScale = m_Markers[i].m_TargetLocalScale;

                m_Markers[i].m_CurrentAnchor.x = m_Markers[i].m_TargetAnchor.x;
                m_Markers[i].m_CurrentAnchor.y = m_Markers[i].m_TargetAnchor.y;

                m_Markers[i].m_CurrentAlpha = m_Markers[i].m_TargetAlpha;

                m_Markers[i].m_CurrentColor = m_Markers[i].m_TargetColor;

                m_Markers[i].m_CurrentRotateAmount = m_Markers[i].m_TargetRotateAmount;
            }

            // Apply values
            float tmpX = m_Markers[i].m_CurrentAnchor.x * Screen.width;
            float tmpY = m_Markers[i].m_CurrentAnchor.y * Screen.height;
            m_Markers[i].m_RectTransform.position = new Vector3(m_Markers[i].m_CurrentAnchor.x * Screen.width, m_Markers[i].m_CurrentAnchor.y * Screen.height, 0.0f);
            m_Markers[i].m_RectTransform.transform.Rotate(m_Markers[i].m_CurrentRotateAmount);
            m_Markers[i].m_RectTransform.localScale = new Vector3(m_Markers[i].m_CurrentLocalScale, m_Markers[i].m_CurrentLocalScale, m_Markers[i].m_CurrentLocalScale);

            Color tmpColor = m_Markers[i].m_CurrentColor;
            m_Markers[i].m_RectTransform.GetComponent<Image>().color = new Color(tmpColor.r, tmpColor.g, tmpColor.b, m_Markers[i].m_CurrentAlpha);
        }
    }
    //////////////////////////////////////////////////////////////////////////
    #endregion

    #region Methods
    //////////////////////////////////////////////////////////////////////////
    //METHODS
    //////////////////////////////////////////////////////////////////////////
    //Initialisation
    private Rect GetRectTransformRect(RectTransform a_RectTransform)
    {
        Rect result = new Rect();
        result.width = a_RectTransform.rect.xMax - a_RectTransform.rect.xMin;
        result.height = a_RectTransform.rect.yMax - a_RectTransform.rect.yMin;
        result.x = a_RectTransform.position.x - result.width * 0.5f;
        result.y = a_RectTransform.position.y - result.height * 0.5f;
        result.x /= Screen.width;
        result.y /= Screen.height;
        result.width /= Screen.width;
        result.height /= Screen.height;

        return result;
    }

    public void UpdateMarkers()
    {
        List<GameObject> targetExisting = new List<GameObject>();
        for(int i = 0; i < m_Markers.Count; i++)
        {
            targetExisting.Add(m_Markers[i].m_Target.gameObject);
        }

        m_MarkersSizeRatio = new Vector2(m_BaseSize.x * (1.0f / GetComponent<RectTransform>().lossyScale.x) / Screen.width, m_BaseSize.y * (1.0f / GetComponent<RectTransform>().lossyScale.y) / Screen.height);
        GameObject[] targetsFound = GameObject.FindGameObjectsWithTag(m_Tag);
        for(int i = 0; i < targetsFound.Length; i++)
        {
            if(!targetExisting.Contains(targetsFound[i]))
            {
                RadarMarker marker = new RadarMarker();

                GameObject uiObject = new GameObject(targetsFound[i].name + "_Marker", new System.Type[] { typeof(RectTransform), typeof(Image) });
                uiObject.transform.SetParent(transform, false);
                uiObject.GetComponent<RectTransform>().localScale = Vector3.one;
                uiObject.GetComponent<RectTransform>().sizeDelta = new Vector2(m_MarkersSizeRatio.x * Screen.width, m_MarkersSizeRatio.y * Screen.height);
                uiObject.GetComponent<Image>().sprite = m_Sprite;
                uiObject.GetComponent<Image>().color = m_Color;

                marker.m_Target = targetsFound[i].transform;
                marker.m_RectTransform = uiObject.GetComponent<RectTransform>();
                if(m_RotationEnabled && m_RotationSpeedMode == RotationSpeedMode.Random)
                {
                    marker.m_TargetRotateAmount.z = Random.Range(m_CloseRotateAmount, m_FarRotateAmount);
                }

                m_Markers.Add(marker);
            }
        }
    }

    public void DeleteTarget(GameObject a_Target, bool a_ResetMarkers = false)
    {
        for(int i = 0; i < m_Markers.Count; i++)
        {
            if(m_Markers[i].Equals(a_Target))
            {
                m_Markers.RemoveAt(i);
                a_Target.tag = "Untagged";
                break;
            }
        }

        if(a_ResetMarkers)
        {
            UpdateMarkers();
        }
    }

    //////////////////////////////////////////////////////////////////////////
    //Getter
    public T[] GetRadarMarkersArray<T>()
    {
        if(typeof(T).Equals(typeof(GameObject)))
        {
            T[] _Result = new T[m_Markers.Count];
            for(int i = 0; i < m_Markers.Count; i++)
            {
                _Result[i] = (T)(object)(m_Markers[i].m_RectTransform);
            }
            return _Result;
        }
        else if(typeof(T).Equals(typeof(Image)))
        {
            T[] _Result = new T[m_Markers.Count];
            for(int i = 0; i < m_Markers.Count; i++)
            {
                _Result[i] = (T)(object)(m_Markers[i].m_RectTransform.GetComponent<Image>());
            }
            return _Result;
        }
        else
        {
            return null;
        }
    }

    public List<T> GetRadarMarkersList<T>()
    {
        if(typeof(T).Equals(typeof(GameObject)))
        {
            List<T> _Result = new List<T>();
            for(int i = 0; i < m_Markers.Count; i++)
            {
                _Result.Add((T)(object)(m_Markers[i].m_RectTransform));
            }
            return _Result;
        }
        else if(typeof(T).Equals(typeof(Image)))
        {
            List<T> _Result = new List<T>();
            for(int i = 0; i < m_Markers.Count; i++)
            {
                _Result.Add((T)(object)(m_Markers[i].m_RectTransform.GetComponent<Image>()));
            }
            return _Result;
        }
        else
        {
            return null;
        }
    }

    //Setter
    public void SetMarkerSprite(Sprite a_Sprite)
    {
        m_Sprite = a_Sprite;
        for(int i = 0; i < m_Markers.Count; i++)
        {
            m_Markers[i].m_RectTransform.GetComponent<Image>().sprite = m_Sprite;
        }
    }

    public void SetDistances(float a_CloseDistance, float a_FarDistance)
    {
        m_CloseDistance = a_CloseDistance;
        m_FarDistance = a_FarDistance;
    }

    public void SetScales(float a_CloseScale, float a_FarScale = 0.0f)
    {
        m_CloseScale = a_CloseScale;
        m_FarScale = a_FarScale;
    }

    public void SetColor(Color a_Color)
    {
        m_UseGradient = false;
        m_Color = a_Color;
    }

    public void SetGradient(Gradient a_Gradient)
    {
        m_UseGradient = true;
        m_Gradient = a_Gradient;
    }

    public void SetRotation(float a_CloseRotateAmount, float a_FarRotateAmount, RotationSpeedMode a_RotationMode = RotationSpeedMode.Constant)
    {
        m_CloseRotateAmount = a_CloseRotateAmount;
        m_FarRotateAmount = a_FarRotateAmount;
        m_RotationSpeedMode = a_RotationMode;

        m_RotationEnabled = CheckForRotationSpeed();

        if(m_RotationEnabled && m_RotationSpeedMode == RotationSpeedMode.Random)
        {
            for(int i = 0; i < m_Markers.Count; i++)
            {
                m_Markers[i].m_TargetRotateAmount.z = Random.Range(m_CloseRotateAmount, m_FarRotateAmount);
            }
        }
    }

    public void SetLerpsUsage(bool a_UseLerps)
    {
        m_UseLerps = a_UseLerps;
    }

    public void SetLerpsUsage(bool a_LerpAlpha, bool a_LerpColor, bool a_LerpMoves, bool a_LerpRotations, bool a_LerpScales)
    {
        m_LerpAlpha = a_LerpAlpha;
        m_LerpColor = a_LerpColor;
        m_LerpMoves = a_LerpMoves;
        m_LerpRotations = a_LerpRotations;
        m_LerpScales = a_LerpScales;
    }

    public void SetLerpsSpeeds(float a_MovingSpeed, float a_RotatingSpeed, float a_ScalingSpeed, float a_ColoringSpeed, float a_AlphaSpeed)
    {
        m_MovingSpeed = a_MovingSpeed;
        m_RotatingSpeed = a_RotatingSpeed;
        m_ScalingSpeed = a_ScalingSpeed;
        m_ColoringSpeed = a_ColoringSpeed;
        m_AlphaSpeed = a_AlphaSpeed;
    }

    //////////////////////////////////////////////////////////////////////////
    //Private methods
    private bool CheckForRotationSpeed()
    {
        switch(m_RotationSpeedMode)
        {
            case RotationSpeedMode.Constant:
                return m_CloseRotateAmount != 0.0f;

            case RotationSpeedMode.OverDistance:
                return (m_CloseRotateAmount != 0.0f || m_FarRotateAmount != 0.0f);

            case RotationSpeedMode.Random:
                return (m_CloseRotateAmount != 0.0f || m_FarRotateAmount != 0.0f);

            default:
                return true;
        }
    }
    #endregion
}
//////////////////////////////////////////////////////////////////////////

#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////////
//EDITOR CLASS
//////////////////////////////////////////////////////////////////////////
[CustomEditor(typeof(UIRadarV3))]
public class UIRadarV3_edit : Editor
{
    private UIRadarV3 m_Target;
    private GUIContent m_Content;

    public override void OnInspectorGUI()
    {
        m_Target = (UIRadarV3)target;

        // Camera
        m_Content = new GUIContent("Camera :", "The camera used as reference to compute markers position on UI.");
        m_Target.m_Camera = (Camera)EditorGUILayout.ObjectField(m_Content, m_Target.m_Camera, typeof(Camera), true);

        // Tag
        int tagIndex = 0;
        string[] tags = UnityEditorInternal.InternalEditorUtility.tags;
        for(int i = 0; i < tags.Length; i++)
        {
            if(m_Target.m_Tag.Equals(tags[i]))
            {
                tagIndex = i;
                break;
            }
        }
        m_Target.m_Tag = UnityEditorInternal.InternalEditorUtility.tags[EditorGUILayout.Popup("Tag to follow :", tagIndex, tags)];

        // Sprite
        m_Content = new GUIContent("Sprite :", "The sprite of the markers.");
        m_Target.m_Sprite = (Sprite)EditorGUILayout.ObjectField(m_Content, m_Target.m_Sprite, typeof(Sprite), false);
        m_Content = new GUIContent("Base size :", "The initial size of the markers (in pixels).");
        m_Target.m_BaseSize = EditorGUILayout.Vector2Field(m_Content, m_Target.m_BaseSize);

        EditorGUILayout.Space();

        // Color
        m_Target.m_UseGradient = EditorGUILayout.Toggle("Use gradient ? ", m_Target.m_UseGradient);
        EditorGUILayout.BeginHorizontal();
        if(!m_Target.m_UseGradient)
        {
            m_Target.m_Color = EditorGUILayout.ColorField("Color :", m_Target.m_Color);
        }
        else
        {
            SerializedObject serializedObject = new SerializedObject(m_Target);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Gradient"), new GUIContent("Gradient :"));
            if(EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("CLOSE", EditorStyles.boldLabel, GUILayout.MaxWidth(64.0f));
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("FAR", EditorStyles.boldLabel, GUILayout.MaxWidth(64.0f));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Distance
        m_Content = new GUIContent("Distance :", "The close and far distances of the targets (markers will only be displayed between those values).");
        EditorGUILayout.LabelField(m_Content);
        EditorGUILayout.BeginHorizontal();
        m_Target.m_CloseDistance = EditorGUILayout.FloatField(m_Target.m_CloseDistance, GUILayout.MaxWidth(64.0f));
        GUILayout.FlexibleSpace();
        m_Target.m_FarDistance = EditorGUILayout.FloatField(m_Target.m_FarDistance, GUILayout.MaxWidth(64.0f));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Rotation
        EditorGUILayout.BeginHorizontal();
        m_Content = new GUIContent("Z Rotation :", "The z rotation speed of the markers (can be fixed, linearly evolve between two values or be randomized).");
        EditorGUILayout.LabelField(m_Content);
        GUILayout.FlexibleSpace();
        m_Target.m_RotationSpeedMode = (UIRadarV3.RotationSpeedMode)EditorGUILayout.EnumPopup(m_Target.m_RotationSpeedMode);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        m_Target.m_CloseRotateAmount = EditorGUILayout.FloatField(m_Target.m_CloseRotateAmount, GUILayout.MaxWidth(64.0f));
        GUILayout.FlexibleSpace();
        GUI.enabled = (m_Target.m_RotationSpeedMode != UIRadarV3.RotationSpeedMode.Constant);
        m_Target.m_FarRotateAmount = EditorGUILayout.FloatField(m_Target.m_FarRotateAmount, GUILayout.MaxWidth(64.0f));
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Scale
        m_Content = new GUIContent("Scale :", "The scale value of the markers will evolve linearly between those values.");
        EditorGUILayout.LabelField(m_Content);
        EditorGUILayout.BeginHorizontal();
        m_Target.m_CloseScale = EditorGUILayout.FloatField(m_Target.m_CloseScale, GUILayout.MaxWidth(64.0f));
        GUILayout.FlexibleSpace();
        m_Target.m_FarScale = EditorGUILayout.FloatField(m_Target.m_FarScale, GUILayout.MaxWidth(64.0f));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        EditorGUILayout.Space();

        m_Content = new GUIContent("Use lerps ? ", "Will lerps be used to smooth markers property changes ?");
        m_Target.m_UseLerps = EditorGUILayout.Toggle(m_Content, m_Target.m_UseLerps);

        if(m_Target.m_UseLerps)
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            m_Content = new GUIContent("Lerp moves ? ", "Will the markers moves be lerped ?");
            m_Target.m_LerpMoves = EditorGUILayout.Toggle(m_Content, m_Target.m_LerpMoves);
            if(m_Target.m_LerpMoves)
            {
                m_Content = new GUIContent("Moving speed :", "How fast will the markers moves be lerped ?");
                m_Target.m_MovingSpeed = EditorGUILayout.FloatField(m_Content, m_Target.m_MovingSpeed);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_Content = new GUIContent("Lerp rotations ? ", "Will the markers rotations be lerped ?");
            m_Target.m_LerpRotations = EditorGUILayout.Toggle(m_Content, m_Target.m_LerpRotations);
            if(m_Target.m_LerpRotations)
            {
                m_Content = new GUIContent("Rotating speed :", "How fast will the markers rotations be lerped ?");
                m_Target.m_RotatingSpeed = EditorGUILayout.FloatField(m_Content, m_Target.m_RotatingSpeed);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_Content = new GUIContent("Lerp scales ? ", "Will the markers scales be lerped ?");
            m_Target.m_LerpScales = EditorGUILayout.Toggle(m_Content, m_Target.m_LerpScales);
            if(m_Target.m_LerpScales)
            {
                m_Content = new GUIContent("Scaling speed :", "How fast will the markers scales be lerped ?");
                m_Target.m_ScalingSpeed = EditorGUILayout.FloatField(m_Content, m_Target.m_ScalingSpeed);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_Content = new GUIContent("Lerp colors ? ", "Will the markers colors be lerped ?");
            m_Target.m_LerpColor = EditorGUILayout.Toggle(m_Content, m_Target.m_LerpColor);
            if(m_Target.m_LerpColor)
            {
                m_Content = new GUIContent("Coloring speed :", "How fast will the markers colors be lerped ?");
                m_Target.m_ColoringSpeed = EditorGUILayout.FloatField(m_Content, m_Target.m_ColoringSpeed);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_Content = new GUIContent("Lerp alpha ? ", "Will the markers alpha be lerped ?");
            m_Target.m_LerpAlpha = EditorGUILayout.Toggle(m_Content, m_Target.m_LerpAlpha);
            if(m_Target.m_LerpAlpha)
            {
                m_Content = new GUIContent("Blending speed :", "How fast will the markers alpha be lerped ?");
                m_Target.m_AlphaSpeed = EditorGUILayout.FloatField(m_Content, m_Target.m_AlphaSpeed);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        EditorGUILayout.Space();

        m_Content = new GUIContent("Use alpha blending ? ", "Will the markers blend to alpha when approaching the borders of the radar ?");
        m_Target.m_UseAlphaBlending = EditorGUILayout.Toggle(m_Content, m_Target.m_UseAlphaBlending);

        if(m_Target.m_UseAlphaBlending)
        {
            EditorGUILayout.Space();

            m_Content = new GUIContent("Alpha start distance :", "How far from the edges of the radar the markers will start blending to alpha (in percentage) ?");
            m_Target.m_AlphaStartPercentage = EditorGUILayout.Slider(m_Content, m_Target.m_AlphaStartPercentage, 0.0f, 0.5f);

            m_Content = new GUIContent("Custom alpha values ? ", "Use custom values for alpha values ? (Default ones are 0.0f and 1.0f)");
            m_Target.m_UseCustomAlphaLimits = EditorGUILayout.Toggle(m_Content, m_Target.m_UseCustomAlphaLimits);

            if(m_Target.m_UseCustomAlphaLimits)
            {
                m_Target.m_MinAlpha = EditorGUILayout.FloatField("Min alpha value :", m_Target.m_MinAlpha);
                m_Target.m_MaxAlpha = EditorGUILayout.FloatField("Max alpha value :", m_Target.m_MaxAlpha);
            }
            else
            {
                m_Target.m_MinAlpha = 0.0f;
                m_Target.m_MaxAlpha = 1.0f;
            }
        }
        else
        {
            m_Target.m_MinAlpha = 0.0f;
            m_Target.m_MaxAlpha = 1.0f;
        }

        EditorGUILayout.Space();
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        EditorGUILayout.Space();

        m_Content = new GUIContent("Use direct view only ? ", "Will the markers be displayed only if the target can be seen directly (no obstacles) ?");
        m_Target.m_DirectViewOnly = EditorGUILayout.Toggle(m_Content, m_Target.m_DirectViewOnly);

        if(m_Target.m_DirectViewOnly)
        {
            m_Content = new GUIContent("Raycast layers :", "Select the layers in which the raycast will be performed (don't forget to include the targets layer).");
            List<string> layers = new List<string>();
            int layerCount = LayerMask.NameToLayer(UnityEditorInternal.InternalEditorUtility.layers[UnityEditorInternal.InternalEditorUtility.layers.Length-1]) + 1;
            for(int i = 0; i < layerCount; ++i)
            {
                layers.Add(LayerMask.LayerToName(i));
            }
            m_Target.m_RaycastLayers = EditorGUILayout.MaskField(m_Content, m_Target.m_RaycastLayers, layers.ToArray());
            EditorGUILayout.Space();
        }
    }
}
//////////////////////////////////////////////////////////////////////////
#endif