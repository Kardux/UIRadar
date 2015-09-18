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
    public Sprite m_MarkerSprite;
    public float m_MaxMarkerScale = 1.0f;
    public float m_MinMarkerScale = 0.0f;
    public float m_MaxDistance = 10.0f;
    public float m_MinDistance = 2.0f;
    public float m_ScalingSpeed = 5.0f;
    public float m_MovingSpeed = 5.0f;
    public string m_Tag = "";
    public bool m_DirectViewOnly = true;
    public bool m_AlphaBlending = false;
    public float m_AlphaStartPercentage = 0.2f;

    public struct RadarMarker
    {
        public GameObject m_TargetObject;
        public float m_TargetDistance;
        public bool m_TargetInDirectView;
        public Vector3 m_WorldToScreenPosition;
        public Vector2 m_CurrentScale;
        public Vector2 m_CurrentPosition;
        public Vector2 m_TargetScale;
        public Vector2 m_TargetPosition;
        public float m_IconAlpha;
    }

    private List<RadarMarker> m_MarkersList = new List<RadarMarker>();
    private List<GameObject> m_Targets;

    private GameObject m_MarkerMockUp;

    private Vector2 m_CanvasSize;

    private float cameraXSize;
    private float cameraYSize;
    private float cameraXPos;
    private float cameraYPos;

#pragma warning restore 0414
    //////////////////////////////////////////////////////////////////////////
    #endregion

    #region Handlers
    //////////////////////////////////////////////////////////////////////////
    //HANDLERS
    //////////////////////////////////////////////////////////////////////////
    void Awake()
    {
        //Check if the tag exists in the project
#if UNITY_EDITOR
        bool checkTag = false;
        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
        {
            if (UnityEditorInternal.InternalEditorUtility.tags[i].Equals(m_Tag))
                checkTag = true;
        }
        if (!checkTag)
            Debug.LogWarning("\"" + m_Tag + "\" tag not found in the project.");
#endif

        m_Targets = new List<GameObject>();
        m_MarkerMockUp = GetComponentInChildren<Image>().gameObject;
        //m_CanvasSize = new Vector2(FindCanvasInParents(gameObject).GetComponent<RectTransform>().rect.width, FindCanvasInParents(gameObject).GetComponent<RectTransform>().rect.height);
        m_CanvasSize = new Vector2(1586.0f, 847.0f);
        Debug.Log(m_CanvasSize);
        //SetCameraSpecifications();

        UpdateTargets();
    }

    void Update()
    {
        for (int i = 0; i < m_Targets.Count; i++)
        {
            RadarMarker _Marker = new RadarMarker();	
            _Marker = m_MarkersList[i];

            if (m_Targets[i])
            {
                _Marker.m_WorldToScreenPosition = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().WorldToViewportPoint(m_Targets[i].transform.position);
                _Marker.m_TargetDistance = Vector3.Distance(m_Targets[i].transform.position, GameObject.FindGameObjectWithTag("Player").transform.position);
            }

            if (_Marker.m_TargetDistance > m_MaxDistance || _Marker.m_TargetDistance < m_MinDistance || _Marker.m_WorldToScreenPosition.z < 0.0f)
            {
                _Marker.m_TargetScale.x = m_MinMarkerScale;
                _Marker.m_TargetScale.y = m_MinMarkerScale;
            }
            else
            {
                _Marker.m_TargetScale.x = m_MaxMarkerScale / _Marker.m_TargetDistance;
                _Marker.m_TargetScale.y = m_MaxMarkerScale / _Marker.m_TargetDistance;
            }

            if (_Marker.m_TargetDistance > m_MaxDistance || _Marker.m_WorldToScreenPosition.z < 0.0f)
            {
                if (_Marker.m_WorldToScreenPosition.x < 0.5f)
                    _Marker.m_TargetPosition.x = 0.0f;
                else
                    _Marker.m_TargetPosition.x = 1.0f;

                if (_Marker.m_WorldToScreenPosition.y < 0.5f)
                    _Marker.m_TargetPosition.y = 0.0f;
                else
                    _Marker.m_TargetPosition.y = 1.0f;
            }
            else
            {
                _Marker.m_TargetPosition.x = Mathf.Clamp(_Marker.m_WorldToScreenPosition.x, 0.0f, 1.0f);
                _Marker.m_TargetPosition.y = Mathf.Clamp(_Marker.m_WorldToScreenPosition.y, 0.0f, 1.0f);
            }

            if (m_AlphaBlending)
            {
                float _XAlpha = 1.0f;
                float _YAlpha = 1.0f;

                if (_Marker.m_WorldToScreenPosition.x < m_AlphaStartPercentage)
                    _XAlpha = Mathf.Clamp(_Marker.m_WorldToScreenPosition.x / m_AlphaStartPercentage, 0f, 1f);
                else if (_Marker.m_WorldToScreenPosition.x > (1f - m_AlphaStartPercentage))
                    _XAlpha = Mathf.Clamp((1.0f - _Marker.m_WorldToScreenPosition.x) / (m_AlphaStartPercentage), 0f, 1f);

                if (_Marker.m_WorldToScreenPosition.y < m_AlphaStartPercentage)
                    _YAlpha = Mathf.Clamp(_Marker.m_WorldToScreenPosition.y / m_AlphaStartPercentage, 0f, 1f);
                else if (_Marker.m_WorldToScreenPosition.y > (1f - m_AlphaStartPercentage))
                    _YAlpha = Mathf.Clamp((1.0f - _Marker.m_WorldToScreenPosition.y) / (m_AlphaStartPercentage), 0f, 1f);

                _Marker.m_IconAlpha = (_XAlpha < _YAlpha ? _XAlpha : _YAlpha);
            }

            _Marker.m_CurrentScale.x = Mathf.Lerp(_Marker.m_CurrentScale.x, _Marker.m_TargetScale.x, m_ScalingSpeed * Time.deltaTime);
            _Marker.m_CurrentScale.y = Mathf.Lerp(_Marker.m_CurrentScale.x, _Marker.m_TargetScale.y, m_ScalingSpeed * Time.deltaTime);

            _Marker.m_CurrentPosition.x = Mathf.Lerp(_Marker.m_CurrentPosition.x, _Marker.m_TargetPosition.x * m_CanvasSize.x, m_MovingSpeed * Time.deltaTime);
            _Marker.m_CurrentPosition.y = Mathf.Lerp(_Marker.m_CurrentPosition.y, _Marker.m_TargetPosition.y * m_CanvasSize.y, m_MovingSpeed * Time.deltaTime);

            /*

            if (m_Targets[i])
            {
                
                RaycastHit hitInfos = new RaycastHit();																	//You create a new variable to stock the information of the coming raycast
                Physics.Raycast(transform.position, target.item.transform.position-transform.position, out hitInfos);	//and you RayCast from the player's position to the item's position
				
                if(hitInfos.collider.gameObject.layer==8)																//HERE IS A BIT TRICKY : you have to creat new layers (I called them "Interactive Items" and "Obstacles") and to apply them to your various items.
                    target.directView=true;																				//Then you select EVERY items in your scene and set their layer to "Ignore Raycast". After that you select your interactive items biggest shape (if you have big trigger colliders on them select the item that hold it),
                else 																									//and set their layers to "Interactive Items". Last part is setting every potential obstacle item layer to "Obstacles".
                    target.directView=false;																			//NOTE : Here my "Interactive Items" layer number is 8
				
                
                m_MarkersList[i] = _Marker;																	//You apply all the variables to your index-i icon in the ICONS LIST
            }
             * 
             * */


            _Marker.m_TargetObject.GetComponent<RectTransform>().position = new Vector3(_Marker.m_CurrentPosition.x, _Marker.m_CurrentPosition.y, 0.0f);
            _Marker.m_TargetObject.GetComponent<RectTransform>().localScale = new Vector3(_Marker.m_CurrentScale.x, _Marker.m_CurrentScale.y, 1.0f);

            Color _TmpColor = _Marker.m_TargetObject.GetComponent<Image>().color;
            _Marker.m_TargetObject.GetComponent<Image>().color = new Color(_TmpColor.r, _TmpColor.g, _TmpColor.b, _Marker.m_IconAlpha);

            m_MarkersList[i] = _Marker;
            
            //Debug.Log("World to screen : " + _Marker.m_WorldToScreenPosition.ToString() + " // Current position : " + _Marker.m_CurrentPosition.ToString() + " // Target position : " + _Marker.m_TargetPosition.ToString());

        }
    }
    /*
    void OnGUI()
    {
        GUI.color = guiGolor;
        for (int i = 0; i < m_Targets.Count; i++)																						//For every icon
        {
            if (m_MarkersList[i].m_WorldToScreenPosition.z > 0 && (!m_DirectViewOnly || (m_DirectViewOnly && m_MarkersList[i].m_TargetInDirectView)))					//If the icon is in front of you and all the required conditions are okay
            {
                if (m_AlphaBlending)
                {
                    guiGolor.a = m_MarkersList[i].m_IconAlpha;
                    GUI.color = guiGolor;
                }
                GUI.DrawTexture(new Rect(m_MarkersList[i].m_CurrentPosition.x, m_MarkersList[i].m_CurrentPosition.y, m_MarkersList[i].m_CurrentScale.x, m_MarkersList[i].m_CurrentScale.y), m_MarkerSprite);//you display the icon with it's size and position
            }
        }
    }
    */
    //////////////////////////////////////////////////////////////////////////
    #endregion

    #region Methods
    //////////////////////////////////////////////////////////////////////////
    //METHODS
    //////////////////////////////////////////////////////////////////////////
    public void SetCameraSpecifications()
    {
        Rect cameraViewport = this.GetComponent<Camera>().rect;
        cameraXPos = cameraViewport.x * Screen.width;
        cameraYPos = (1f - cameraViewport.y - cameraViewport.height) * Screen.height;
        cameraXSize = cameraViewport.width * Screen.width;
        cameraYSize = cameraViewport.height * Screen.height;
    }

    public void UpdateTargets()
    {
        m_Targets.Clear();
        m_MarkerMockUp.SetActive(true);
        foreach (Transform _Child in gameObject.transform)
        {
            if (!_Child.gameObject.Equals(m_MarkerMockUp))
                Destroy(_Child);
        }

        GameObject[] _TmpTargets = new GameObject[GameObject.FindGameObjectsWithTag(m_Tag).Length];
        _TmpTargets = GameObject.FindGameObjectsWithTag(m_Tag);
        for (int i = 0; i < _TmpTargets.Length; i++)
        {
            m_Targets.Add(_TmpTargets[i]);

            RadarMarker _NewTarget = new RadarMarker();
            GameObject _Icon = Instantiate(m_MarkerMockUp);
            _Icon.name = _TmpTargets[i].name + "_Marker";
            _Icon.transform.SetParent(transform);
            _Icon.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
            _NewTarget.m_TargetObject = _Icon;
            _NewTarget.m_IconAlpha = 1.0f;
            m_MarkersList.Add(_NewTarget);
        }

        m_MarkerMockUp.SetActive(false);
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

    public void SetMarkerSprite(Sprite Sprite)
    {
        m_MarkerSprite = Sprite;
    }

    public void SetScales(float MinScale, float MaxScale)
    {
        m_MinMarkerScale = MinScale;
        m_MaxMarkerScale = MaxScale;
    }

    public void SetDistances(float MinDistance, float MaxDistance)
    {
        m_MinDistance = MinDistance;
        m_MaxDistance = MaxDistance;
    }

    public void SetSpeeds(float MovingSpeed, float ScalingSpeed)
    {
        m_MovingSpeed = MovingSpeed;
        m_ScalingSpeed = ScalingSpeed;
    }

    public void SetAlphaStartPercentage(float AlphaStartPercentage)
    {
        m_AlphaStartPercentage = AlphaStartPercentage;
    }
    //////////////////////////////////////////////////////////////////////////
    private Canvas FindCanvasInParents(GameObject Container)
    {
        Canvas _Result = Container.GetComponent<Canvas>();
        if (_Result)
        {
            return _Result;
        }
        else
        {
            if (Container.transform.parent)
            {
                return FindCanvasInParents(Container.transform.parent.gameObject);
            }
            else
            {
                return null;
            }
        }
    }
    #endregion
}
//////////////////////////////////////////////////////////////////////////