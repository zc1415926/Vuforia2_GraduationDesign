/*==============================================================================
        Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
        All Rights Reserved.
        Qualcomm Confidential and Proprietary
==============================================================================*/
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// About screen manager.
/// 
/// Draws the UI for the About Screen handling different
/// screen sizes and dpis
/// </summary>
public class AboutScreenManager : MonoBehaviour
{
    #region PUBLIC_MEMBER_VARIABLES

    /// <summary>
    /// A file containing the text to be drawn in the about screen
    /// </summary>
    public TextAsset AboutText;

    #endregion // PUBLIC_MEMBER_VARIABLES



    #region PRIVATE_MEMBER_VARIABLES

    private const float ABOUT_TEXT_MARGIN = 20.0f;
    private const float START_BUTTON_VERTICAL_MARGIN = 10.0f;
    
    // Unity GUI Skin containing settings for font and custom image buttons
    private GUISkin mUISkin;
    
    // dictionary to hold gui styles, fetching them each time a button is drawn is slow
    private Dictionary<string, GUIStyle> mButtonGUIStyles;
    
    private Vector2 mScrollPosition;
    private float mStartButtonAreaHeight = 80.0f;
    private float mAboutTitleHeight = 80.0f;
    private bool mTouchInProgress = false;
    private Vector2 mLastTouchPosition;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PROPERTIES

    /// <summary>
    /// This float returns a resolution dependent scale factor.
    /// Using this, elements can be drawn as if the resolution was 480 (smaller dimension)
    /// on every device.
    /// </summary>
    private static float DeviceDependentScale
    {
         get { if ( Screen.width > Screen.height)
                return Screen.height / 480f;
              else 
                return Screen.width / 480f; 
        }
    }

    #endregion // PROPERTIES



    #region UNITY_MONOBEHAVIOUR_METHODS

    void Start () 
    {
    
        if(Screen.dpi > 260 )
        {
             // load and set gui style
            mUISkin = Resources.Load("UserInterface/ButtonSkinsXHDPI") as GUISkin;
            
        }else
        {
             // load and set gui style
            mUISkin = Resources.Load("UserInterface/ButtonSkins") as GUISkin;
        }
        
        #if UNITY_IPHONE
        if(Screen.height > 1500 )
        {
            // Loads the XHDPI sources for the iPAd 3
            mUISkin = Resources.Load("UserInterface/ButtonSkinsiPad3") as GUISkin;
        }

        #endif
        
        // remember all custom styles in gui skin to avoid constant lookups
        mButtonGUIStyles = new Dictionary<string,GUIStyle>();
        foreach (GUIStyle customStyle in mUISkin.customStyles) mButtonGUIStyles.Add(customStyle.name, customStyle);
        
    }
    

    void Update () 
    {
        if(Input.GetMouseButtonDown(0))
        {
            mTouchInProgress = true;
            mLastTouchPosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0) && mTouchInProgress)
        {
            Vector2 touchPosition = Input.mousePosition;
            mScrollPosition.y += (touchPosition.y - mLastTouchPosition.y);
            mLastTouchPosition = touchPosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            mTouchInProgress = false;
        }
    }
    
    
    void OnGUI()
    {
        float scale = 1*DeviceDependentScale;
        
        mAboutTitleHeight = 80.0f* scale;
        
        // Draws the About title background
        GUIStyle titleStyleBackground;
        if (mButtonGUIStyles.TryGetValue("EmptyGrayBox", out titleStyleBackground))
        {
            GUI.Box(new Rect(0,0,Screen.width,mAboutTitleHeight),string.Empty,titleStyleBackground);
        }
        
        // Draws the About title
        GUIStyle titleStyle;
        if (mButtonGUIStyles.TryGetValue("AboutScreenTitle", out titleStyle))
        {

            GUI.Box(new Rect(ABOUT_TEXT_MARGIN * DeviceDependentScale,0,Screen.width,mAboutTitleHeight),"About",titleStyle);
        }
        
        GUIStyle startButtonStyle;
        if (mButtonGUIStyles.TryGetValue("StartButton", out startButtonStyle))
        {
            float width = Screen.width / 1.5f;
            float height = startButtonStyle.normal.background.height * scale;
            
            mStartButtonAreaHeight = height + 2*(START_BUTTON_VERTICAL_MARGIN * scale);
            float left = Screen.width/2 - width/2;
            float top = Screen.height - mStartButtonAreaHeight + START_BUTTON_VERTICAL_MARGIN * scale;
            
            GUI.skin = mUISkin;
            
            GUILayout.BeginArea(new Rect(ABOUT_TEXT_MARGIN * DeviceDependentScale,
                                     mAboutTitleHeight + 5 * DeviceDependentScale,
                                     Screen.width - (ABOUT_TEXT_MARGIN * DeviceDependentScale),
                                     Screen.height - ( mStartButtonAreaHeight) - mAboutTitleHeight - 5 * DeviceDependentScale));
            
            mScrollPosition = GUILayout.BeginScrollView(mScrollPosition, false, false, GUILayout.Width(Screen.width - (ABOUT_TEXT_MARGIN * DeviceDependentScale)), 
                GUILayout.Height (Screen.height - mStartButtonAreaHeight - mAboutTitleHeight));
        
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            
            GUILayout.Label(AboutText.text);
        
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.EndScrollView();
        
            GUILayout.EndArea();
        
            // if button was pressed, remember to make sure this event is not interpreted as a touch event somewhere else
            if (GUI.Button(new Rect(left, top, width, height), "Start" ,startButtonStyle))
            {
                // Starts the ImageTargets Scene - using the LoadingScene
                Application.LoadLevel("Vuforia-3-LoadingScene");
            }
        }
    }

    #endregion // UNITY_MONOBEHAVIOUR_METHODS
}
