using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace Obfuscator.Gui
{
    public class SettingsWindow : EditorWindow
    {
        private class Fonts
        {
            private static Font candara;
            public static Font Candara
            {
                get
                {
                    if(candara == null)
                    {
                        try
                        {
                            candara = (Font)EditorGUIUtility.Load("Assets/OPS/Obfuscator.Free/Editor/Gui/Font/Candara.ttf");
                        }
                        catch(Exception e)
                        {
                            Debug.LogError(e.ToString());
                        }
                    }
                    return candara;
                }
            }
            private static Font candaraBold;
            public static Font CandaraBold
            {
                get
                {
                    if (candaraBold == null)
                    {
                        try
                        {
                            candaraBold = (Font)EditorGUIUtility.Load("Assets/OPS/Obfuscator.Free/Editor/Gui/Font/Candara_Bold.ttf");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.ToString());
                        }
                    }
                    return candaraBold;
                }
            }
        }

        private class Text
        {
            private static GUIStyle styleMiddleLeft;
            private static GUIStyle styleMiddleLeftBold;

            private static GUIStyle styleMiddleCenter;
            private static GUIStyle styleMiddleCenterBold;

            public static void Gui(String _Text, int _Width = 200, int _Height = 24)
            {
                if (styleMiddleLeft == null)
                {
                    styleMiddleLeft = new GUIStyle("label");
                    styleMiddleLeft.alignment = TextAnchor.MiddleLeft;
                    styleMiddleLeft.font = Fonts.Candara;
                    styleMiddleLeft.fontSize = 15;
                }
                GUILayout.Label(_Text, styleMiddleLeft, GUILayout.Width(_Width), GUILayout.Height(_Height));
            }
            public static void GuiBold(String _Text, int _Width = 200, int _Height = 24)
            {
                if (styleMiddleLeftBold == null)
                {
                    styleMiddleLeftBold = new GUIStyle("label");
                    styleMiddleLeftBold.alignment = TextAnchor.MiddleLeft;
                    styleMiddleLeftBold.font = Fonts.CandaraBold;
                    styleMiddleLeftBold.fontSize = 16;
                }
                GUILayout.Label(_Text, styleMiddleLeftBold, GUILayout.Width(_Width), GUILayout.Height(_Height));
            }

            public static void GuiCenter(String _Text, int _Width = 200, int _Height = 24)
            {
                if (styleMiddleCenter == null)
                {
                    styleMiddleCenter = new GUIStyle("label");
                    styleMiddleCenter.alignment = TextAnchor.MiddleCenter;
                    styleMiddleCenter.font = Fonts.Candara;
                    styleMiddleCenter.fontSize = 15;
                }

                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();
                GUILayout.Label(_Text, styleMiddleCenter, GUILayout.Width(_Width), GUILayout.Height(_Height));
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
               
            }
            public static void GuiCenterBold(String _Text, int _Width = 200, int _Height = 24)
            {
                if (styleMiddleCenterBold == null)
                {
                    styleMiddleCenterBold = new GUIStyle("label");
                    styleMiddleCenterBold.alignment = TextAnchor.MiddleCenter;
                    styleMiddleCenterBold.font = Fonts.CandaraBold;
                    styleMiddleCenterBold.fontSize = 16;
                }

                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();
                GUILayout.Label(_Text, styleMiddleLeftBold, GUILayout.Width(_Width), GUILayout.Height(_Height));
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
            }
        }

        private class Switch
        {
            private static Texture onTexture;
            private static Texture offTexture;
            private static Texture onGoldTexture;
            private static Texture offGoldTexture;
            private static GUIStyle style;

            public static void Gui(ref bool _Value)
            {
                try
                {
                    if (onTexture == null)
                    {
                        onTexture = (Texture)EditorGUIUtility.Load("Assets/OPS/Obfuscator.Free/Editor/Gui/Button_On_24x.png");
                    }
                    if (offTexture == null)
                    {
                        offTexture = (Texture)EditorGUIUtility.Load("Assets/OPS/Obfuscator.Free/Editor/Gui/Button_Off_24x.png");
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError(e.ToString());
                }
                if (style == null)
                {
                    style = new GUIStyle("button");
                    style.normal.background = null;
                    style.active.background = null;
                }
                if(_Value)
                {
                    if (GUILayout.Button(onTexture, style,  GUILayout.Width(64), GUILayout.Height(24)))
                    {
                        _Value = false;
                    }
                }
                else
                {
                    if (GUILayout.Button(offTexture, style, GUILayout.Width(64), GUILayout.Height(24)))
                    {
                        _Value = true;
                    }
                }
            }

            public static void GuiGold(ref bool _Value)
            {
                try
                {
                    if (onGoldTexture == null)
                    {
                        onGoldTexture = (Texture)EditorGUIUtility.Load("Assets/OPS/Obfuscator.Free/Editor/Gui/Button_On_Gold_24x.png");
                    }
                    if (offGoldTexture == null)
                    {
                        offGoldTexture = (Texture)EditorGUIUtility.Load("Assets/OPS/Obfuscator.Free/Editor/Gui/Button_Off_Gold_24x.png");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
                if (style == null)
                {
                    style = new GUIStyle("button");
                    style.normal.background = null;
                    style.active.background = null;
                }
                if (_Value)
                {
                    if (GUILayout.Button(onGoldTexture, style, GUILayout.Width(64), GUILayout.Height(24)))
                    {
                        _Value = false;
                    }
                }
                else
                {
                    if (GUILayout.Button(offGoldTexture, style, GUILayout.Width(64), GUILayout.Height(24)))
                    {
                        _Value = true;
                    }
                }
            }

            public static void GuiCenter(ref bool _Value)
            {
                try
                { 
                    if (onTexture == null)
                    {
                        onTexture = (Texture)EditorGUIUtility.Load("Assets/OPS/Obfuscator.Free/Editor/Gui/Button_On_24x.png");
                    }
                    if (offTexture == null)
                    {
                        offTexture = (Texture)EditorGUIUtility.Load("Assets/OPS/Obfuscator.Free/Editor/Gui/Button_Off_24x.png");
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError(e.ToString());
                }
                if (style == null)
                {
                    style = new GUIStyle("button");
                    style.normal.background = null;
                    style.active.background = null;
                }
                if (_Value)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(onTexture, style, GUILayout.Width(64), GUILayout.Height(24)))
                    {
                        _Value = false;
                    }
                    GUILayout.FlexibleSpace();

                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(offTexture, style, GUILayout.Width(64), GUILayout.Height(24)))
                    {
                        _Value = true;
                    }
                    GUILayout.FlexibleSpace();

                    GUILayout.EndHorizontal();
                }
            }

            public static void Gui(ref bool _Value, String _TextureOn, String _TextureOff, int _Width = 128, int _Height = 32)
            {
                Texture var_OnTexture = null;
                Texture var_OffTexture = null;
                try
                { 
                    var_OnTexture = (Texture)EditorGUIUtility.Load(_TextureOn);
                    var_OffTexture = (Texture)EditorGUIUtility.Load(_TextureOff);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
                if (style == null)
                {
                    style = new GUIStyle("button");
                    style.normal.background = null;
                    style.active.background = null;
                }
                if (_Value)
                {
                    if (GUILayout.Button(var_OnTexture, style, GUILayout.Width(_Width), GUILayout.Height(_Height)))
                    {
                        _Value = false;
                    }
                }
                else
                {
                    if (GUILayout.Button(var_OffTexture, style, GUILayout.Width(_Width), GUILayout.Height(_Height)))
                    {
                        _Value = true;
                    }
                }
            }

            public static bool GuiNoRef(bool _Value, String _TextureOn, String _TextureOff, int _Width = 128, int _Height = 32)
            {
                Texture var_OnTexture = null;
                Texture var_OffTexture = null;
                try
                {
                    var_OnTexture = (Texture)EditorGUIUtility.Load(_TextureOn);
                    var_OffTexture = (Texture)EditorGUIUtility.Load(_TextureOff);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
                if (style == null)
                {
                    style = new GUIStyle("button");
                    style.normal.background = null;
                    style.active.background = null;
                }
                if (_Value)
                {
                    if (GUILayout.Button(var_OnTexture, style, GUILayout.Width(_Width), GUILayout.Height(_Height)))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (GUILayout.Button(var_OffTexture, style, GUILayout.Width(_Width), GUILayout.Height(_Height)))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        private class Row
        {
            public static void Gui(String _Text, ref bool _Value)
            {
                GUILayout.BeginHorizontal();
                Text.Gui(_Text);
                Switch.Gui(ref _Value);
                GUILayout.EndHorizontal();
            }
            public static void GuiGold(String _Text, ref bool _Value)
            {
                GUILayout.BeginHorizontal();
                Text.Gui(_Text);
                Switch.GuiGold(ref _Value);
                GUILayout.EndHorizontal();
            }
            public static void GuiBold(String _Text, ref bool _Value)
            {
                GUILayout.BeginHorizontal();
                Text.GuiBold(_Text);
                Switch.Gui(ref _Value);
                GUILayout.EndHorizontal();
            }
        }

        private class Table
        {
            public static void Gui(String _TH, String _T1, String _T2, String _T3, ref bool _B1, ref bool _B2, ref bool _B3)
            {
                int var_Width = 72;
                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical();
                Text.Gui(" ", var_Width);
                Text.Gui(_TH, var_Width);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                Text.GuiCenter(_T1, var_Width);
                Switch.GuiCenter(ref _B1);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                Text.GuiCenter(_T2, var_Width);
                Switch.GuiCenter(ref _B2);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                Text.GuiCenter(_T3, var_Width);
                Switch.GuiCenter(ref _B3);
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }

            public static void Gui(String _TH, String _T1, String _T2, String _T3, String _T4, String _T5, ref bool _B1, ref bool _B2, ref bool _B3, ref bool _B4, ref bool _B5)
            {
                int var_Width = 72;
                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical();
                Text.Gui(" ", var_Width);
                Text.Gui(_TH, var_Width);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                Text.GuiCenter(_T1, var_Width);
                Switch.GuiCenter(ref _B1);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                Text.GuiCenter(_T2, var_Width);
                Switch.GuiCenter(ref _B2);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                Text.GuiCenter(_T3, var_Width);
                Switch.GuiCenter(ref _B3);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                Text.GuiCenter(_T4, var_Width);
                Switch.GuiCenter(ref _B4);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                Text.GuiCenter(_T5, var_Width);
                Switch.GuiCenter(ref _B5);
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }
        }

        //
        private int activeOptions
        {
            get
            {
                return (GuiSettings.AddRandomCode ? 1 : 0)
                    + (GuiSettings.MakeAssemblyTypesUnreadable ? 1 : 0)
                    + (GuiSettings.ObfuscateClass ? 1 : 0)
                    + (GuiSettings.ObfuscateClassAbstract ? 1 : 0)
                    + (GuiSettings.ObfuscateClassGeneric ? 1 : 0)
                    + (GuiSettings.ObfuscateClassPrivate ? 1 : 0)
                    + (GuiSettings.ObfuscateClassProtected ? 1 : 0)
                    + (GuiSettings.ObfuscateClassPublic ? 1 : 0)
                    + (GuiSettings.ObfuscateEnumValues ? 1 : 0)
                    + (GuiSettings.ObfuscateEvent ? 1 : 0)
                    + (GuiSettings.ObfuscateField ? 1 : 0)
                    + (GuiSettings.ObfuscateFieldPrivate ? 1 : 0)
                    + (GuiSettings.ObfuscateFieldProtected ? 1 : 0)
                    + (GuiSettings.ObfuscateFieldPublic ? 1 : 0)
                    + (GuiSettings.ObfuscateMethod ? 1 : 0)
                    + (GuiSettings.ObfuscateMethodPrivate ? 1 : 0)
                    + (GuiSettings.ObfuscateMethodProtected ? 1 : 0)
                    + (GuiSettings.ObfuscateMethodPublic ? 1 : 0)
                    + (GuiSettings.ObfuscateNamespace ? 1 : 0)
                    + (GuiSettings.ObfuscateProperty ? 1 : 0)
                    + (GuiSettings.ObfuscateString ? 1 : 0)
                    + (GuiSettings.ObfuscateUnityClasses ? 1 : 0)
                    + (GuiSettings.ObfuscateUnityPublicFields ? 1 : 0);
            }
        }

        private int securityLevel
        {
            get
            {
                if (activeOptions > 18)
                {
                    return 3;
                }
                if (activeOptions > 13)
                {
                    return 2;
                }
                return 1;
            }
            set
            {
                switch(value)
                {
                    case 1:
                        {
                            GuiSettings.ObfuscateNamespace = false;

                            GuiSettings.ObfuscateClass = true;
                            GuiSettings.ObfuscateClassPublic = false;
                            GuiSettings.ObfuscateClassProtected = true;
                            GuiSettings.ObfuscateClassPrivate = true;

                            GuiSettings.ObfuscateField = true;
                            GuiSettings.ObfuscateFieldPublic = false;
                            GuiSettings.ObfuscateFieldProtected = true;
                            GuiSettings.ObfuscateFieldPrivate = true;

                            GuiSettings.ObfuscateProperty = false;

                            GuiSettings.ObfuscateEvent = false;

                            GuiSettings.ObfuscateMethod = true;
                            GuiSettings.ObfuscateMethodPublic = false;
                            GuiSettings.ObfuscateMethodProtected = true;
                            GuiSettings.ObfuscateMethodPrivate = true;

                            //

                            GuiSettings.ObfuscateClassGeneric = false;
                            GuiSettings.ObfuscateClassAbstract = false;
                            GuiSettings.ObfuscateUnityClasses = false;

                            GuiSettings.ObfuscateUnityPublicFields = false;
                            GuiSettings.ObfuscateEnumValues = false;

                            GuiSettings.TryFindGuiMethods = true;
                            GuiSettings.TryFindAnimationMethods = true;

                            //

                            GuiSettings.AddRandomCode = false;
                            GuiSettings.MakeAssemblyTypesUnreadable = false;
                            GuiSettings.ObfuscateString = false;
                            GuiSettings.StoreObfuscatedStrings = false;

                            break;
                        }
                    case 2:
                        {
                            if (Obfuscator.Setting.InternalSettings.ObfuscatorType == Setting.EObfuscatorType.Pro)
                            {
                                GuiSettings.ObfuscateNamespace = true;
                            }
                            else
                            {
                                GuiSettings.ObfuscateNamespace = false;
                            }

                            GuiSettings.ObfuscateClass = true;
                            GuiSettings.ObfuscateClassPublic = true;
                            GuiSettings.ObfuscateClassProtected = true;
                            GuiSettings.ObfuscateClassPrivate = true;

                            GuiSettings.ObfuscateField = true;
                            GuiSettings.ObfuscateFieldPublic = true;
                            GuiSettings.ObfuscateFieldProtected = true;
                            GuiSettings.ObfuscateFieldPrivate = true;

                            GuiSettings.ObfuscateProperty = true;

                            GuiSettings.ObfuscateEvent = true;

                            GuiSettings.ObfuscateMethod = true;
                            GuiSettings.ObfuscateMethodPublic = true;
                            GuiSettings.ObfuscateMethodProtected = true;
                            GuiSettings.ObfuscateMethodPrivate = true;

                            //

                            GuiSettings.ObfuscateClassGeneric = false;
                            GuiSettings.ObfuscateClassAbstract = false;
                            GuiSettings.ObfuscateUnityClasses = false;

                            GuiSettings.ObfuscateUnityPublicFields = false;
                            GuiSettings.ObfuscateEnumValues = false;

                            GuiSettings.TryFindGuiMethods = true;
                            GuiSettings.TryFindAnimationMethods = true;

                            //

                            GuiSettings.AddRandomCode = false;
                            GuiSettings.MakeAssemblyTypesUnreadable = false;
                            GuiSettings.ObfuscateString = false;
                            GuiSettings.StoreObfuscatedStrings = false;

                            break;
                        }
                    case 3:
                        {
                            if (Obfuscator.Setting.InternalSettings.ObfuscatorType == Setting.EObfuscatorType.Pro)
                            {
                                GuiSettings.ObfuscateNamespace = true;
                            }
                            else
                            {
                                GuiSettings.ObfuscateNamespace = false;
                            }

                            GuiSettings.ObfuscateClass = true;
                            GuiSettings.ObfuscateClassPublic = true;
                            GuiSettings.ObfuscateClassProtected = true;
                            GuiSettings.ObfuscateClassPrivate = true;

                            GuiSettings.ObfuscateField = true;
                            GuiSettings.ObfuscateFieldPublic = true;
                            GuiSettings.ObfuscateFieldProtected = true;
                            GuiSettings.ObfuscateFieldPrivate = true;

                            GuiSettings.ObfuscateProperty = true;

                            GuiSettings.ObfuscateEvent = true;

                            GuiSettings.ObfuscateMethod = true;
                            GuiSettings.ObfuscateMethodPublic = true;
                            GuiSettings.ObfuscateMethodProtected = true;
                            GuiSettings.ObfuscateMethodPrivate = true;

                            //

                            GuiSettings.ObfuscateClassGeneric = true;
                            GuiSettings.ObfuscateClassAbstract = true;
                            if (Obfuscator.Setting.InternalSettings.ObfuscatorType == Setting.EObfuscatorType.Pro)
                            {
                                GuiSettings.ObfuscateUnityClasses = true;
                                GuiSettings.ObfuscateUnityPublicFields = true;
                            }
                            else
                            {
                                GuiSettings.ObfuscateUnityClasses = false;
                                GuiSettings.ObfuscateUnityPublicFields = false;
                            }
          
                            GuiSettings.ObfuscateEnumValues = true;

                            GuiSettings.TryFindGuiMethods = true;
                            GuiSettings.TryFindAnimationMethods = true;

                            //

                            if (Obfuscator.Setting.InternalSettings.ObfuscatorType == Setting.EObfuscatorType.Pro)
                            {
                                GuiSettings.AddRandomCode = true;
                                GuiSettings.MakeAssemblyTypesUnreadable = true;
                                GuiSettings.ObfuscateString = true;
                                GuiSettings.StoreObfuscatedStrings = true;
                            }
                            else
                            {
                                GuiSettings.AddRandomCode = false;
                                GuiSettings.MakeAssemblyTypesUnreadable = false;
                                GuiSettings.ObfuscateString = false;
                                GuiSettings.StoreObfuscatedStrings = false;
                            }        

                            break;
                        }
                }
            }
        }

        //Tabs
        private int tabIndex;

        //Namespace
        public string[] NamespaceArray = { "" };

        //Attribute
        public string[] AttributeArray = { "" };

        //Scroll
        private Vector2 scrollPosition = Vector2.zero;

        // Add menu item named "My Window" to the Window menu
        [MenuItem("Window/Obfuscator Free Settings")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow<SettingsWindow>("Obfuscator");
        }

        void OnEnable()
        {
            GuiSettings.LoadSettings();
            GuiSettings.SettingsGotReloaded = true;
            NamespaceArray = GuiSettings.NamespacesToIgnoreList == null ? null : GuiSettings.NamespacesToIgnoreList.ToArray();
            AttributeArray = GuiSettings.AttributesBehaveLikeDoNotRenameList == null ? null : GuiSettings.AttributesBehaveLikeDoNotRenameList.ToArray();
        }

        void OnDisable()
        {
            GuiSettings.SaveSettings();
        }

        void OnGUI()
        {
            try
            {
                this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition, false, false, GUILayout.Width(position.width), GUILayout.Height(position.height));

                GUIStyle var_Style = new GUIStyle("button");
                var_Style.normal.background = null;
                var_Style.active.background = null;

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if(GUILayout.Button((Texture)EditorGUIUtility.Load("Assets/OPS/Obfuscator.Free/Editor/Gui/Rate.png"), var_Style, GUILayout.MaxWidth(100), GUILayout.MaxHeight(26)))
                {
                    Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/obfuscator-free-89420");
                }
                if(GUILayout.Button((Texture)EditorGUIUtility.Load("Assets/OPS/Obfuscator.Free/Editor/Gui/BugQuestion.png"), var_Style, GUILayout.MaxWidth(200), GUILayout.MaxHeight(26)))
                {
                    Application.OpenURL("mailto:orangepearsoftware@gmail.com?subject=OPS/Obfuscator.Free_Bug");
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label((Texture)EditorGUIUtility.Load("Assets/OPS/Obfuscator.Free/Editor/Gui/Header_Icon.png"), GUILayout.MaxWidth(24), GUILayout.MaxHeight(24), GUILayout.MinWidth(24), GUILayout.MinHeight(24));
                Text.GuiBold("Obfuscator", 150, 24);
                GUILayout.EndHorizontal();

                EditorGUILayout.HelpBox("De-/Activate Obfuscator here.", MessageType.Info);

                Row.GuiBold("Obfuscate Globally: ", ref Gui.GuiSettings.ObfuscateGlobally);

                GUILayout.Space(10);

                GUI.enabled = Gui.GuiSettings.ObfuscateGlobally;

                this.tabIndex = GUILayout.Toolbar(this.tabIndex, new Texture[] { (Texture)EditorGUIUtility.Load("Assets/OPS/Obfuscator.Free/Editor/Gui/General_T32x.png"), (Texture)EditorGUIUtility.Load("Assets/OPS/Obfuscator.Free/Editor/Gui/Advanced_T32x.png"), (Texture)EditorGUIUtility.Load("Assets/OPS/Obfuscator.Free/Editor/Gui/Security_T32x.png") });
                switch (this.tabIndex)
                {
                    case 0:
                        {
                            this.GeneralTab();
                            break;
                        }
                    case 1:
                        {
                            this.AdvancedTab();
                            break;
                        }
                    case 2:
                        {
                            this.SecurityTab();
                            break;
                        }
                }

                GUI.enabled = true;

                GUILayout.EndScrollView();

                if (GUI.changed)
                {
                    GuiSettings.SaveSettings();
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e.ToString());
                this.Close();
            }
        }

        private void GeneralTab()
        {
            Text.GuiBold("Profile");

            EditorGUILayout.HelpBox("The three following buttons show your current level of obfuscation.\nAdditionally you can press on of those to activate a predefined obfuscation profile.", MessageType.Info);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            int var_SecurityLevel = this.securityLevel;
            bool var_Profile_Simple = false;
            bool var_Profile_Standard = false;
            bool var_Profile_Optimal = false;
            if (var_SecurityLevel == 1)
            {
                var_Profile_Simple = true;
            }
            if (var_SecurityLevel == 2)
            {
                var_Profile_Standard = true;
            }
            if (var_SecurityLevel == 3)
            {
                var_Profile_Optimal = true;
            }

            if(Switch.GuiNoRef(var_Profile_Simple, "Assets/OPS/Obfuscator.Free/Editor/Gui/Profile_Simple.png", "Assets/OPS/Obfuscator.Free/Editor/Gui/Profile_Simple_Unselect.png"))
            {
                this.securityLevel = 1;
            }
            if(Switch.GuiNoRef(var_Profile_Standard, "Assets/OPS/Obfuscator.Free/Editor/Gui/Profile_Standard.png", "Assets/OPS/Obfuscator.Free/Editor/Gui/Profile_Standard_Unselect.png"))
            {
                this.securityLevel = 2;
            }
            if (Obfuscator.Setting.InternalSettings.ObfuscatorType != Setting.EObfuscatorType.Pro)
            {
                GUI.enabled = false;
            }
            if (Switch.GuiNoRef(var_Profile_Optimal, "Assets/OPS/Obfuscator.Free/Editor/Gui/Profile_Optimal.png", "Assets/OPS/Obfuscator.Free/Editor/Gui/Profile_Optimal_Unselect.png"))
            {
                this.securityLevel = 3;
            }
            GUI.enabled = Gui.GuiSettings.ObfuscateGlobally;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            Text.GuiBold("Obfuscation");

            EditorGUILayout.HelpBox("Define here generally what names shall get obfuscated!", MessageType.Info);

            Table.Gui("Obfuscate:", "Class", "Field", "Property", "Event", "Method", ref Gui.GuiSettings.ObfuscateClass, ref Gui.GuiSettings.ObfuscateField, ref Gui.GuiSettings.ObfuscateProperty, ref Gui.GuiSettings.ObfuscateEvent, ref Gui.GuiSettings.ObfuscateMethod);

            Text.GuiBold("Namespace");
            
            if (Obfuscator.Setting.InternalSettings.ObfuscatorType != Setting.EObfuscatorType.Pro)
            {
                GUI.enabled = false;
            }
            EditorGUILayout.HelpBox("Define here generally if namespace names shall get obfuscated.", MessageType.Info);
            Row.GuiGold("Namespace:", ref GuiSettings.ObfuscateNamespace);
            GUI.enabled = Gui.GuiSettings.ObfuscateGlobally;

            EditorGUILayout.HelpBox("To make Obfuscator Free ignore easily some Namespaces, enter it here. All Namespaces beginning like your entered one will get ignored. (Example the entry: 'UnityStandardAssets'. All Namespaces beginning with 'UnityStandardAssets' will get ignored (and so it's Content)).", MessageType.Info);

            //
            ScriptableObject var_Target = this;
            SerializedObject var_SerializedObject = new SerializedObject(var_Target);
            SerializedProperty var_StringsProperty = var_SerializedObject.FindProperty("NamespaceArray");

            EditorGUILayout.PropertyField(var_StringsProperty, new GUIContent("Namespaces"), true);
            var_SerializedObject.ApplyModifiedProperties();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Line"))
            {
                List<String> var_TempList = new List<string>(this.NamespaceArray);
                var_TempList.Add("");
                this.NamespaceArray = var_TempList.ToArray();
            }
            if (GUILayout.Button("Remove Line"))
            {
                List<String> var_TempList = new List<string>(this.NamespaceArray);
                if (var_TempList.Count > 0)
                {
                    var_TempList.RemoveAt(var_TempList.Count - 1);
                }
                this.NamespaceArray = var_TempList.ToArray();
            }
            GUILayout.EndHorizontal();

            GuiSettings.NamespacesToIgnoreList = new List<string>(this.NamespaceArray);
            //

            EditorGUILayout.HelpBox("When you activate 'Vice Versa Namespace skipping', every content (class/methods/..) belonging to a namespace that is in the bottom list gets obfuscated. Every namespace that is not in the list will get skipped. The advantage is, if you use many external plugins that shall get skipped while obfuscation and you only want your namespaces to get obfuscated, it reduces your administration effort.", MessageType.Info);
            Row.Gui("Vice Versa Namespace skipping:", ref GuiSettings.NamespaceViceVersa);
        }

        private void AdvancedTab()
        {
            Text.GuiBold("Class");

            Table.Gui("Obfuscate:", "Private", "Protected", "Public", ref Gui.GuiSettings.ObfuscateClassPrivate, ref Gui.GuiSettings.ObfuscateClassProtected, ref Gui.GuiSettings.ObfuscateClassPublic);

            EditorGUILayout.HelpBox("Obfuscation of Generic or Abstract classes can lead to warning messages (like 'XYZ can not be an abstract class' or 'Serialization XYZ error') in the build log. This does not harm. But if you recognize any problems or limitations deactivate it.", MessageType.Warning);

            Row.Gui("Obfuscate generic classes:", ref GuiSettings.ObfuscateClassGeneric);
            Row.Gui("Obfuscate abstract classes:", ref GuiSettings.ObfuscateClassAbstract);

            if (Obfuscator.Setting.InternalSettings.ObfuscatorType != Setting.EObfuscatorType.Pro)
            {
                GUI.enabled = false;
            }
            EditorGUILayout.HelpBox("Define here if classes that inherite from MonoBehaviour/NetworkBehaviour or ScriptableObject should get Obfuscated. For more security, they will automatically be placed in the same namespace.\nWhile obfuscation and after, 'Reloading Assets' appears. Thats normal, do not worry.", MessageType.Info);
            Row.GuiGold("Obfuscate unity class names:", ref GuiSettings.ObfuscateUnityClasses);
            GUI.enabled = Gui.GuiSettings.ObfuscateGlobally;

            Text.GuiBold("Field");

            Table.Gui("Obfuscate:", "Private", "Protected", "Public", ref Gui.GuiSettings.ObfuscateFieldPrivate, ref Gui.GuiSettings.ObfuscateFieldProtected, ref Gui.GuiSettings.ObfuscateFieldPublic);

            EditorGUILayout.HelpBox("Define here if values of enums shall get Obfuscated. Deactivate it, if you use 'ToString()' with enums.", MessageType.Warning);
            Row.Gui("Obfuscate enum values:", ref GuiSettings.ObfuscateEnumValues);

            if (Obfuscator.Setting.InternalSettings.ObfuscatorType != Setting.EObfuscatorType.Pro)
            {
                GUI.enabled = false;
            }
            EditorGUILayout.HelpBox("If you use GameObjects in your Scene, which use Editor editable fields, deactivate this.", MessageType.Warning);
            Row.GuiGold("Obfuscate unity public fields:", ref GuiSettings.ObfuscateUnityPublicFields);
            GUI.enabled = Gui.GuiSettings.ObfuscateGlobally;

            Text.GuiBold("Method");

            Table.Gui("Obfuscate:", "Private", "Protected", "Public", ref Gui.GuiSettings.ObfuscateMethodPrivate, ref Gui.GuiSettings.ObfuscateMethodProtected, ref Gui.GuiSettings.ObfuscateMethodPublic);

            EditorGUILayout.HelpBox("Try to auto find used Gui methods. Mostly it will not find all methods. So add to methods, that appear not to get called in game, the 'Obfuscator.DoNotRenameAttribute'.", MessageType.Info);
            Row.Gui("Auto find Gui methods: ", ref GuiSettings.TryFindGuiMethods);

            EditorGUILayout.HelpBox("Try to auto find used Animation methods. Mostly it will not find all methods. So add to methods, that appear not to get called in game, the 'Obfuscator.DoNotRenameAttribute'.", MessageType.Info);
            Row.Gui("Auto find Animation methods: ", ref GuiSettings.TryFindAnimationMethods);

            if (Obfuscator.Setting.InternalSettings.ObfuscatorType != Setting.EObfuscatorType.Pro)
            {
                GUI.enabled = false;
            }
            EditorGUILayout.HelpBox("BETA: Obfuscate Unity methods like: Awake, Start, Update, ...", MessageType.Info);
            Row.GuiGold("Obfuscate Unity methods: ", ref GuiSettings.ObfuscateUnityMethod);
            GUI.enabled = Gui.GuiSettings.ObfuscateGlobally;

            Text.GuiBold("Attribute");
            EditorGUILayout.HelpBox("Add here Attributes you want to behave like the Attribute DoNotRename. These Attribute must entered with their fullname. For example, if you want the Unity NetworkBehaviour Attribute ClientRpc to behave like DoNotRename you have to enter ClientRpcAttribute. So don't forget the 'Attribute' at the ending if there is one.", MessageType.Info);
            
            //
            ScriptableObject var_Target = this;
            SerializedObject var_SerializedObject = new SerializedObject(var_Target);
            SerializedProperty var_StringsProperty = var_SerializedObject.FindProperty("AttributeArray");

            EditorGUILayout.PropertyField(var_StringsProperty, new GUIContent("Attributes"), true);
            var_SerializedObject.ApplyModifiedProperties();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Line"))
            {
                List<String> var_TempList = new List<string>(this.AttributeArray);
                var_TempList.Add("");
                this.AttributeArray = var_TempList.ToArray();
            }
            if (GUILayout.Button("Remove Line"))
            {
                List<String> var_TempList = new List<string>(this.AttributeArray);
                if (var_TempList.Count > 0)
                {
                    var_TempList.RemoveAt(var_TempList.Count - 1);
                }
                this.AttributeArray = var_TempList.ToArray();
            }
            GUILayout.EndHorizontal();

            GuiSettings.AttributesBehaveLikeDoNotRenameList = new List<string>(this.AttributeArray);
            //
        }

        private void SecurityTab()
        {
            if (Obfuscator.Setting.InternalSettings.ObfuscatorType != Setting.EObfuscatorType.Pro)
            {
                GUI.enabled = false;
            }
            Text.GuiBold("String");
            EditorGUILayout.HelpBox("Enable RSA String Encryption. The Obfuscation of Strings is good to have. But even strong String Obfuscation can get broken. So do never store sensitive data in your Game. But keep in mind, String Obfuscation costs performance while the Game is running.", MessageType.Warning);

            Row.GuiGold("Obfuscate Strings: ", ref Gui.GuiSettings.ObfuscateString);

            EditorGUILayout.HelpBox("To increase the runtime performance while using String Obfuscation activate: 'Store Obfuscated Strings'. 'Store Obfuscated Strings' will make the Obfuscator store the Obfuscated String and its decrypted in the RAM. Gives greate Performance boost. But the decrypted String is stored in the RAM, so you have weigh it by yourself if you want to use it. (I personally recommend to activate it.)", MessageType.Warning);
            Row.GuiGold("Store Obfuscated Strings: ", ref Gui.GuiSettings.StoreObfuscatedStrings);

            EditorGUILayout.HelpBox("When you want to use String obfuscation in an Windows Metro Project. Please switch the build platform at first to a standalone platform, close and reopen Window->Obfuscator Free Settings and press here in the Obfuscator settings the \"Generate\" button, then switch back to our old build project.", MessageType.Warning);
#if UNITY_WINRT
            GUI.enabled = false;
#endif
            GUILayout.BeginHorizontal();
            Text.Gui("Generate new RSA keys: ");
            if (GUILayout.Button("Generate"))
            {
                
            }

            GUILayout.EndHorizontal();
#if UNITY_WINRT
            if (Obfuscator.Setting.InternalSettings.ObfuscatorType == Setting.EObfuscatorType.Pro)
            {
                GUI.enabled = Gui.GuiSettings.ObfuscateGlobally;
            }
#endif
            Text.GuiBold("Assembly");

            EditorGUILayout.HelpBox("Add Random Source Code based on existing Methods and Classes.", MessageType.Info);
            Row.GuiGold("Add Random Code: ", ref Gui.GuiSettings.AddRandomCode);

            EditorGUILayout.HelpBox("When your Game Assembly will get opened by an Disassembler like ILSpy, this Option will make your Classes not direct viewable. The Reverse Engineer will get an Null Pointer Exception.", MessageType.Info);
            Row.GuiGold("Unreadability for Decompilers: ", ref Gui.GuiSettings.MakeAssemblyTypesUnreadable);

            GUI.enabled = Gui.GuiSettings.ObfuscateGlobally;
        }
    }
}
#endif