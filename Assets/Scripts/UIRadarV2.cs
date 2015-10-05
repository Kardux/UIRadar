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

//////////////////////////////////////////////////////////////////////////
//CLASS
//////////////////////////////////////////////////////////////////////////
public class UIRadarV2 : MonoBehaviour
{
    #region Variables
    //////////////////////////////////////////////////////////////////////////
    //VARIABLES
    //////////////////////////////////////////////////////////////////////////
#pragma warning disable 0414
    //Sprite
    public Sprite m_MarkerSprite;

    //Scale
    public float m_MaxMarkerScale = 1.0f;
    public float m_MinMarkerScale = 0.0f;

    //Distance
    public float m_MaxDistance = 10.0f;
    public float m_MinDistance = 2.0f;

    //Color
    public ColorMode m_ColorMode = ColorMode.SingleColor;
    public bool m_GradienColorTransition = true;
    public List<Color> m_MarkerColors = new List<Color> { Color.white };
    public List<float> m_MarkersColorsPercentages = new List<float> { 0.0f, 1.0f };

    //Rotation
    public RotationSpeedMode m_RotationSpeedMode = RotationSpeedMode.Constant;
    public float m_MinRotationSpeed = 20.0f;
    public float m_MaxRotationSpeed = 0.0f;

    //Lerping
    public bool m_UseLerps = true;
    public bool m_LerpScales = true;
    public bool m_LerpMoves = true;
    public bool m_LerpAlphaChanges = true;
    public bool m_LerpColors = true;
    public bool m_LerpRotations = true;
    public float m_ScalingSpeed = 5.0f;
    public float m_MovingSpeed = 5.0f;
    public float m_AlphaChangingSpeed = 5.0f;
    public float m_ColoringSpeed = 5.0f;
    public float m_RotatingLerpSpeed = 5.0f;

    //Alpha blending
    public bool m_UseAlphaBlending = true;
    public float m_AlphaStartPercentage = 0.2f;
    public bool m_UseCustomAlphaLimits = true;
    public float m_MinAlpha = 0.0f;
    public float m_MaxAlpha = 0.75f;

    //Tag to follow
    public string m_Tag = "";

    //Display only if target can be seen
    public bool m_DirectViewOnly = false;
    public string m_RaycastLayer = "Default";

    public struct RadarMarker
    {
        public GameObject m_TargetObject;
        public float m_TargetDistance;
        public bool m_TargetInDirectView;
        public Vector3 m_WorldToScreenPosition;
        public Vector2 m_CurrentScale;
        public Vector2 m_CurrentPosition;
        public Vector2 m_TargetLerpScale;
        public Vector2 m_TargetLerpPosition;
        public float m_CurrentAlpha;
        public float m_TargetLerpAlpha;
        public Color m_CurrentColor;
        public Color m_TargetLerpColor;
        public Vector3 m_CurrentRotation;
        public Vector3 m_TargetLerpRotation;
    }

    public enum ColorMode { SingleColor, SimpleGradient, MultipleColors };
    public enum RotationSpeedMode { Constant, OverDistance, Random };

    private List<RadarMarker> m_MarkersList = new List<RadarMarker>();
    private List<GameObject> m_Targets;

    private GameObject m_MainCamera;

    private Vector2 m_CanvasSize;
    private Vector2 m_RadarRatio;
    private Rect m_RadarRect;

    private bool m_RotationSpeedEnbaled;
#pragma warning restore 0414
    //////////////////////////////////////////////////////////////////////////
    #endregion

    #region Handlers
    //////////////////////////////////////////////////////////////////////////
    //HANDLERS
    //////////////////////////////////////////////////////////////////////////
    void Awake()
    {
        //Check if the tag exists in the project - May be obsolete in the future
#if UNITY_EDITOR
        bool checkTag = false;
        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
        {
            if (UnityEditorInternal.InternalEditorUtility.tags[i].Equals(m_Tag))
                checkTag = true;
        }
        if (!checkTag)
            Debug.LogWarning("\"" + m_Tag + "\" tag not found in the project.");

        if (!m_MarkerSprite)
            Debug.LogWarning("No sprite currently assigned to your radar.");
#endif

        m_Targets = new List<GameObject>();
        m_MainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        SetRadarSpecifications();

        m_RotationSpeedEnbaled = CheckForRotationSpeed();

        UpdateTargets();
    }

    void Update()
    {
        for (int i = 0; i < m_Targets.Count; i++)
        {
            RadarMarker _Marker = new RadarMarker();
            _Marker = m_MarkersList[i];

            //Global marker informations
            if (m_Targets[i])
            {
                _Marker.m_WorldToScreenPosition = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().WorldToViewportPoint(m_Targets[i].transform.position);
                _Marker.m_TargetDistance = Vector3.Distance(m_Targets[i].transform.position, GameObject.FindGameObjectWithTag("MainCamera").transform.position);
            }

            //Scale
            if (_Marker.m_TargetDistance > m_MaxDistance || _Marker.m_TargetDistance < m_MinDistance || _Marker.m_WorldToScreenPosition.z < 0.0f)
            {
                _Marker.m_TargetLerpScale.x = m_MinMarkerScale;
                _Marker.m_TargetLerpScale.y = m_MinMarkerScale;
            }
            else
            {
                _Marker.m_TargetLerpScale.x = m_MaxMarkerScale / _Marker.m_TargetDistance;
                _Marker.m_TargetLerpScale.y = m_MaxMarkerScale / _Marker.m_TargetDistance;
            }

            //Direct view
            bool _DirectView = false;

            if (m_DirectViewOnly)
            {
                RaycastHit _HitInfos = new RaycastHit();
                if (Physics.Raycast(m_MainCamera.transform.position, m_Targets[i].transform.position - m_MainCamera.transform.position, out _HitInfos, m_MaxDistance, ~LayerMask.NameToLayer(m_RaycastLayer)))
                {
                    if (_HitInfos.collider.gameObject.Equals(m_Targets[i]))
                        _DirectView = true;
                }
            }
            else
            {
                _DirectView = true;
            }

            //Position
            bool _TargetInRange = false;

            if (!_DirectView || _Marker.m_TargetDistance > m_MaxDistance || _Marker.m_WorldToScreenPosition.z < 0.0f)
            {
                if (_Marker.m_WorldToScreenPosition.x < m_RadarRect.xMin + m_RadarRect.width * 0.5f)
                    _Marker.m_TargetLerpPosition.x = 0.0f;
                else
                    _Marker.m_TargetLerpPosition.x = 1.0f;

                if (_Marker.m_WorldToScreenPosition.y < m_RadarRect.yMin + m_RadarRect.height * 0.5f)
                    _Marker.m_TargetLerpPosition.y = 0.0f;
                else
                    _Marker.m_TargetLerpPosition.y = 1.0f;

                _Marker.m_TargetLerpScale = Vector2.zero;
            }
            else
            {
                _Marker.m_TargetLerpPosition.x = Mathf.Clamp(_Marker.m_WorldToScreenPosition.x, 0.0f, 1.0f);
                _Marker.m_TargetLerpPosition.y = Mathf.Clamp(_Marker.m_WorldToScreenPosition.y, 0.0f, 1.0f);

                _TargetInRange = true;
            }

            //Color
            switch (m_ColorMode)
            {
                case ColorMode.SingleColor:
                    _Marker.m_TargetLerpColor = m_MarkerColors[0];
                    break;

                case ColorMode.SimpleGradient :
                    if (_TargetInRange)
                        _Marker.m_TargetLerpColor = ColorGradient(m_MarkerColors[0], m_MarkerColors[1], (_Marker.m_TargetDistance - m_MinDistance) / (m_MaxDistance - m_MinDistance));
                    else
                        _Marker.m_TargetLerpColor = m_MarkerColors[1];
                    break;

                case ColorMode.MultipleColors :
                    if (_TargetInRange)
                    {
                        for (int j = 0; j < m_MarkersColorsPercentages.Count; j++)
                        {
                            if (m_MarkersColorsPercentages[j] > (_Marker.m_TargetDistance - m_MinDistance) / (m_MaxDistance - m_MinDistance))
                            {
                                if (j == 0)
                                {
                                    _Marker.m_TargetLerpColor = m_MarkerColors[0];
                                    break;
                                }

                                if (m_GradienColorTransition)
                                    _Marker.m_TargetLerpColor = ColorGradient(m_MarkerColors[j - 1], m_MarkerColors[j], ((_Marker.m_TargetDistance - m_MinDistance) / (m_MaxDistance - m_MinDistance) - m_MarkersColorsPercentages[j - 1]) / (m_MarkersColorsPercentages[j] - m_MarkersColorsPercentages[j - 1]));
                                else
                                    _Marker.m_TargetLerpColor = m_MarkerColors[j - 1];
                                break;
                            }
                        }
                    }
                    else
                    {
                        _Marker.m_TargetLerpColor = m_MarkerColors[0];
                    }
                    break;
            }

            //Rotation speed
            if (m_RotationSpeedEnbaled)
            {
                switch (m_RotationSpeedMode)
                {
                    case RotationSpeedMode.Constant :
                        _Marker.m_TargetLerpRotation.z = m_MinRotationSpeed;
                        break;

                    case RotationSpeedMode.OverDistance :
                        _Marker.m_TargetLerpRotation.z = m_MaxRotationSpeed + (_Marker.m_TargetDistance - m_MaxDistance) / (m_MinDistance - m_MaxDistance) * (m_MinRotationSpeed - m_MaxRotationSpeed);
                        break;

                    case RotationSpeedMode.Random :
                        break;

                    default :
                        break;
                }
            }

            //Alpha blending
            if (!_TargetInRange || _Marker.m_WorldToScreenPosition.x < m_RadarRect.xMin || _Marker.m_WorldToScreenPosition.x > m_RadarRect.xMax || _Marker.m_WorldToScreenPosition.y < m_RadarRect.yMin || _Marker.m_WorldToScreenPosition.y > m_RadarRect.yMax)
            {
                _Marker.m_TargetLerpAlpha = m_MinAlpha;
            }
            else if (m_UseAlphaBlending)
            {
                float _XAlpha = 1.0f;
                float _YAlpha = 1.0f;

                if (_Marker.m_WorldToScreenPosition.x < m_RadarRect.xMin + m_RadarRect.width * m_AlphaStartPercentage)
                    _XAlpha = Mathf.Clamp((_Marker.m_WorldToScreenPosition.x - m_RadarRect.xMin) / m_AlphaStartPercentage / m_RadarRect.width, 0f, 1f);
                else if (_Marker.m_WorldToScreenPosition.x > m_RadarRect.xMax - m_RadarRect.width * m_AlphaStartPercentage)
                    _XAlpha = Mathf.Clamp((m_RadarRect.xMax - _Marker.m_WorldToScreenPosition.x) / m_AlphaStartPercentage / m_RadarRect.width, 0f, 1f);

                if (_Marker.m_WorldToScreenPosition.y < m_RadarRect.yMin + m_RadarRect.height * m_AlphaStartPercentage)
                    _YAlpha = Mathf.Clamp((_Marker.m_WorldToScreenPosition.y - m_RadarRect.yMin) / m_AlphaStartPercentage / m_RadarRect.height, 0f, 1f);
                else if (_Marker.m_WorldToScreenPosition.y > m_RadarRect.yMax - m_RadarRect.height * m_AlphaStartPercentage)
                    _YAlpha = Mathf.Clamp((m_RadarRect.yMax - _Marker.m_WorldToScreenPosition.y) / m_AlphaStartPercentage / m_RadarRect.height, 0f, 1f);


                _Marker.m_TargetLerpAlpha = (_XAlpha < _YAlpha ? m_MinAlpha + (m_MaxAlpha - m_MinAlpha) * _XAlpha : m_MinAlpha + (m_MaxAlpha - m_MinAlpha) * _YAlpha);
            }
            else
            {
                _Marker.m_TargetLerpAlpha = m_MaxAlpha;
            }

            //Lerps
            if (m_UseLerps)
            {
                if (m_LerpScales)
                    _Marker.m_CurrentScale = Vector2.Lerp(_Marker.m_CurrentScale, _Marker.m_TargetLerpScale, m_ScalingSpeed * Time.deltaTime);
                else
                    _Marker.m_CurrentScale = _Marker.m_TargetLerpScale;

                if (m_LerpMoves)
                {
                    _Marker.m_CurrentPosition.x = Mathf.Lerp(_Marker.m_CurrentPosition.x, _Marker.m_TargetLerpPosition.x * m_CanvasSize.x * m_RadarRatio.x, m_MovingSpeed * Time.deltaTime);
                    _Marker.m_CurrentPosition.y = Mathf.Lerp(_Marker.m_CurrentPosition.y, _Marker.m_TargetLerpPosition.y * m_CanvasSize.y * m_RadarRatio.y, m_MovingSpeed * Time.deltaTime);
                }
                else
                {
                    _Marker.m_CurrentPosition.x = _Marker.m_TargetLerpPosition.x * m_CanvasSize.x * m_RadarRatio.x;
                    _Marker.m_CurrentPosition.y = _Marker.m_TargetLerpPosition.y * m_CanvasSize.y * m_RadarRatio.y;
                }

                if (m_LerpAlphaChanges)
                    _Marker.m_CurrentAlpha = Mathf.Lerp(_Marker.m_CurrentAlpha, _Marker.m_TargetLerpAlpha, m_AlphaChangingSpeed * Time.deltaTime);
                else
                    _Marker.m_CurrentAlpha = _Marker.m_TargetLerpAlpha;

                if (m_LerpColors && !(m_ColorMode == ColorMode.MultipleColors && !m_GradienColorTransition))
                    _Marker.m_CurrentColor = Color.Lerp(_Marker.m_CurrentColor, _Marker.m_TargetLerpColor, m_ColoringSpeed * Time.deltaTime);
                else
                    _Marker.m_CurrentColor = _Marker.m_TargetLerpColor;

                if (m_LerpRotations)
                    _Marker.m_CurrentRotation = Vector3.Lerp(_Marker.m_CurrentRotation, _Marker.m_TargetLerpRotation, m_RotatingLerpSpeed * Time.deltaTime);
                else
                    _Marker.m_CurrentRotation = _Marker.m_TargetLerpRotation;
            }
            else
            {
                _Marker.m_CurrentScale = _Marker.m_TargetLerpScale;

                _Marker.m_CurrentPosition.x = _Marker.m_TargetLerpPosition.x * m_CanvasSize.x * m_RadarRatio.x;
                _Marker.m_CurrentPosition.y = _Marker.m_TargetLerpPosition.y * m_CanvasSize.y * m_RadarRatio.y;

                _Marker.m_CurrentAlpha = _Marker.m_TargetLerpAlpha;

                _Marker.m_CurrentColor = _Marker.m_TargetLerpColor;

                _Marker.m_CurrentRotation = _Marker.m_TargetLerpRotation;
            }

            //Apply values
            _Marker.m_TargetObject.GetComponent<RectTransform>().position = new Vector3(_Marker.m_CurrentPosition.x, _Marker.m_CurrentPosition.y, 0.0f);
            _Marker.m_TargetObject.GetComponent<RectTransform>().localScale = new Vector3(_Marker.m_CurrentScale.x, _Marker.m_CurrentScale.y, 1.0f);

            Color _TmpColor = _Marker.m_CurrentColor;
            _Marker.m_TargetObject.GetComponent<Image>().color = new Color(_TmpColor.r, _TmpColor.g, _TmpColor.b, _Marker.m_CurrentAlpha);

            _Marker.m_TargetObject.transform.Rotate(_Marker.m_CurrentRotation);

            //Apply marker
            m_MarkersList[i] = _Marker;

            //Debug.Log("World to screen : " + _Marker.m_WorldToScreenPosition.ToString() + " // Current position : " + _Marker.m_CurrentPosition.ToString() + " // Target position : " + _Marker.m_TargetLerpPosition.ToString());

        }
    }
    //////////////////////////////////////////////////////////////////////////
    #endregion

    #region Methods
    //////////////////////////////////////////////////////////////////////////
    //METHODS
    //////////////////////////////////////////////////////////////////////////
    //Initialisation
    public void SetRadarSpecifications()
    {
        RectTransform _RadarRect = GetComponent<RectTransform>();
        m_CanvasSize = new Vector2((_RadarRect.anchorMax.x - _RadarRect.anchorMin.x) * Screen.width, (_RadarRect.anchorMax.y - _RadarRect.anchorMin.y) * Screen.height);
        m_RadarRatio = new Vector2(Screen.width / m_CanvasSize.x, Screen.height / m_CanvasSize.y);

        m_RadarRect = new Rect();
        m_RadarRect.min = new Vector2(_RadarRect.anchorMin.x, _RadarRect.anchorMin.y);
        m_RadarRect.max = new Vector2(_RadarRect.anchorMax.x, _RadarRect.anchorMax.y);

        if (m_UseAlphaBlending && !m_UseCustomAlphaLimits)
        {
            m_MinAlpha = 0.0f;
            m_MaxAlpha = 1.0f;
        }
    }

    public void UpdateTargets()
    {
        m_Targets.Clear();
        foreach (Transform _Child in gameObject.transform)
        {
            Destroy(_Child);
        }

        GameObject[] _TmpTargets = new GameObject[GameObject.FindGameObjectsWithTag(m_Tag).Length];
        _TmpTargets = GameObject.FindGameObjectsWithTag(m_Tag);
        for (int i = 0; i < _TmpTargets.Length; i++)
        {
            m_Targets.Add(_TmpTargets[i]);

            RadarMarker _NewMarker = new RadarMarker();
            GameObject _Icon = new GameObject();
            _Icon.AddComponent<Image>();
            _Icon.GetComponent<Image>().sprite = m_MarkerSprite;
            _Icon.GetComponent<Image>().color = m_MarkerColors[0];
            _Icon.name = _TmpTargets[i].name + "_Marker";
            _Icon.transform.SetParent(transform);
            _Icon.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
            _NewMarker.m_TargetObject = _Icon;
            _NewMarker.m_CurrentAlpha = 1.0f;
            if (m_RotationSpeedEnbaled && m_RotationSpeedMode == RotationSpeedMode.Random)
                _NewMarker.m_TargetLerpRotation.z = Random.Range(m_MinRotationSpeed, m_MaxRotationSpeed);

            m_MarkersList.Add(_NewMarker);
        }
    }

    public void DeleteTarget(GameObject Obj, bool ResetMarkers = false)
    {
        for (int i = 0; i < m_Targets.Count; i++)
        {
            if (m_Targets[i].Equals(Obj))
            {
                m_MarkersList.RemoveAt(i);
                m_Targets.RemoveAt(i);
                Obj.tag = "Untagged";
                break;
            }
        }

        if (ResetMarkers)
            UpdateTargets();
    }

    //////////////////////////////////////////////////////////////////////////
    //Getter
    public T[] GetRadarMarkersArray<T>()
    {
        if (typeof(T).Equals(typeof(GameObject)))
        {
            T[] _Result = new T[m_MarkersList.Count];
            for (int i = 0; i < m_MarkersList.Count; i++)
            {
                _Result[i] = (T)(object)(m_MarkersList[i].m_TargetObject);
            }
            return _Result;
        }
        else if (typeof(T).Equals(typeof(Image)))
        {
            T[] _Result = new T[m_MarkersList.Count];
            for (int i = 0; i < m_MarkersList.Count; i++)
            {
                _Result[i] = (T)(object)(m_MarkersList[i].m_TargetObject.GetComponent<Image>());
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
        if (typeof(T).Equals(typeof(GameObject)))
        {
            List<T> _Result = new List<T>();
            for (int i = 0; i < m_MarkersList.Count; i++)
            {
                _Result.Add((T)(object)(m_MarkersList[i].m_TargetObject));
            }
            return _Result;
        }
        else if (typeof(T).Equals(typeof(Image)))
        {
            List<T> _Result = new List<T>();
            for (int i = 0; i < m_MarkersList.Count; i++)
            {
                _Result.Add((T)(object)(m_MarkersList[i].m_TargetObject.GetComponent<Image>()));
            }
            return _Result;
        }
        else
        {
            return null;
        }
    }
    
    //Setter
    public void SetMarkerSprite(Sprite Sprite)
    {
        m_MarkerSprite = Sprite;
        for (int i = 0; i < m_MarkersList.Count; i ++)
        {
            m_MarkersList[i].m_TargetObject.GetComponent<Image>().sprite = m_MarkerSprite;
        }
    }

    public void SetDistances(float MaxDistance, float MinDistance)
    {
        m_MaxDistance = MaxDistance;
        m_MinDistance = MinDistance;
    }

    public void SetScales(float MaxScale, float MinScale = 0.0f)
    {
        m_MaxMarkerScale = MaxScale;
        m_MinMarkerScale = MinScale;
    }

    public void SetColors(List<Color> MarkerColors, ColorMode Mode = ColorMode.SingleColor, bool GradienColorTransition = false)
    {
        m_ColorMode = Mode;
        switch (m_ColorMode)
        {
            case ColorMode.SingleColor :
                m_MarkerColors[0] = MarkerColors[0];
                break;

            case ColorMode.SimpleGradient :
                m_MarkerColors[0] = MarkerColors[0];
                m_MarkerColors[1] = MarkerColors[1];
                break;

            case ColorMode.MultipleColors :
                m_MarkerColors = MarkerColors;
                break;
        }
    }

    public void SetRotation(float MinRotationSpeed, float MaxRotationSpeed, RotationSpeedMode RotationMode = RotationSpeedMode.Constant)
    {
        m_RotationSpeedMode = RotationMode;
        m_MinRotationSpeed = MinRotationSpeed;
        m_MaxRotationSpeed = MaxRotationSpeed;

        m_RotationSpeedEnbaled = CheckForRotationSpeed();

        if (m_RotationSpeedEnbaled && m_RotationSpeedMode == RotationSpeedMode.Random)
        {
            for (int i = 0; i < m_MarkersList.Count; i ++)
            {
                RadarMarker _TmpRadarMarker = m_MarkersList[i];
                _TmpRadarMarker.m_TargetLerpRotation.z = Random.Range(m_MinRotationSpeed, m_MaxRotationSpeed);
                m_MarkersList[i] = _TmpRadarMarker;
            }
        }
    }

    public void SetLerpsUsage(bool UseLerps)
    {
        m_UseLerps = UseLerps;
    }

    public void SetLerpsUsage(bool LerpAlphaChanges, bool LerpColors, bool LerpMoves, bool LerpRotations, bool LerpScales)
    {
        m_LerpAlphaChanges = LerpAlphaChanges;
        m_LerpColors = LerpColors;
        m_LerpMoves = LerpMoves;
        m_LerpRotations = LerpRotations;
        m_LerpScales = LerpScales;
    }

    public void SetLerpsSpeeds(float AlphaChangingSpeed, float ColoringSpeed, float MovingSpeed, float RotatingLerpSpeed, float ScalingSpeed)
    {
        m_AlphaChangingSpeed = AlphaChangingSpeed;
        m_ColoringSpeed = ColoringSpeed;
        m_MovingSpeed = MovingSpeed;
        m_RotatingLerpSpeed = RotatingLerpSpeed;
        m_ScalingSpeed = ScalingSpeed;
    }

    public void SetLerpsSpeeds(float MovingSpeed, float ScalingSpeed)
    {
        m_MovingSpeed = MovingSpeed;
        m_ScalingSpeed = ScalingSpeed;
    }

    //////////////////////////////////////////////////////////////////////////
    //Private methods
    private Color ColorGradient(Color ColorA, Color ColorB, float Percentage)
    {
        float _R = (ColorA.r + (ColorB.r - ColorA.r) * Percentage);
        float _G = (ColorA.g + (ColorB.g - ColorA.g) * Percentage);
        float _B = (ColorA.b + (ColorB.b - ColorA.b) * Percentage);

        return new Color(_R, _G, _B);
    }

    private bool CheckForRotationSpeed()
    {
        switch (m_RotationSpeedMode)
        {
            case RotationSpeedMode.Constant :
                return m_MinRotationSpeed != 0.0f;

            case RotationSpeedMode.OverDistance :
                return (m_MinRotationSpeed != 0.0f || m_MaxRotationSpeed != 0.0f);

            case RotationSpeedMode.Random :
                return (m_MinRotationSpeed != 0.0f || m_MaxRotationSpeed != 0.0f);

            default:
                return true;
        }
    }
    #endregion
}
//////////////////////////////////////////////////////////////////////////