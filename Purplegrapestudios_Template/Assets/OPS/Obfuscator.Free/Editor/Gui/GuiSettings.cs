using System;
using System.Collections.Generic;

namespace Obfuscator.Gui
{
    public static class GuiSettings
    {
        //Reload
        public static bool SettingsGotReloaded;

        private static Dictionary<String, String> SettingDictionary;

        //Global
        public static bool ObfuscateGlobally;

        //Namespace
        public static bool ObfuscateNamespace;

        //Class
        public static bool ObfuscateClass;
        public static bool ObfuscateClassPrivate;
        public static bool ObfuscateClassProtected;
        public static bool ObfuscateClassPublic;

        public static bool ObfuscateClassGeneric;
        public static bool ObfuscateClassAbstract;

        //Field
        public static bool ObfuscateField;
        public static bool ObfuscateFieldPrivate;
        public static bool ObfuscateFieldProtected;
        public static bool ObfuscateFieldPublic;

        //Property
        public static bool ObfuscateProperty;

        //Event
        public static bool ObfuscateEvent;

        //Method
        public static bool ObfuscateMethod;
        public static bool ObfuscateMethodPrivate;
        public static bool ObfuscateMethodProtected;
        public static bool ObfuscateMethodPublic;
        public static bool ObfuscateUnityMethod;

        //Enum
        public static bool ObfuscateEnumValues;

        //Unity Objects
        public static bool ObfuscateUnityClasses;
        public static bool ObfuscateUnityPublicFields;

        //String Obfuscation
        public static bool ObfuscateString;
        public static bool StoreObfuscatedStrings;

        //Add Random Code
        public static bool AddRandomCode;

        //Make Assembly Types Unreadable
        public static bool MakeAssemblyTypesUnreadable;

        //Find Gui Methods in File
        public static bool TryFindGuiMethods;

        //Find Animation Methods in File
        public static bool TryFindAnimationMethods;

        //Name Obfuscated Name and Length File Path
        public static bool SaveNamesToPathFile;
        public static String SaveNamePathFile;

        //Ignore Namespaces
        public static List<String> NamespacesToIgnoreList;
        public static bool NamespaceViceVersa;

        //Attributes behave like DoNotRename
        public static List<String> AttributesBehaveLikeDoNotRenameList;

        private static bool Parse(String _Value)
        {
            return (_Value == "1");
        }

        private static String Parse(bool _Value)
        {
            return (_Value ? "1" : "0");
        }

        public static void LoadSettings()
        {
            try
            {
                String var_FileName = UnityEngine.Application.dataPath + "\\OPS\\Obfuscator.Free\\Obfuscator_Settings.txt";
                if (!System.IO.File.Exists(var_FileName))
                {
                    return;
                }

                ReadDictionary(var_FileName);

                //Global
                ObfuscateGlobally = Parse(GetDictionaryValue("ObfuscateGlobally"));

                //Namespace
                ObfuscateNamespace = Parse(GetDictionaryValue("ObfuscateNamespace"));

                //Class
                ObfuscateClass = Parse(GetDictionaryValue("ObfuscateClass"));
                ObfuscateClassPrivate = Parse(GetDictionaryValue("ObfuscateClassPrivate"));
                ObfuscateClassProtected = Parse(GetDictionaryValue("ObfuscateClassProtected"));
                ObfuscateClassPublic = Parse(GetDictionaryValue("ObfuscateClassPublic"));

                //Field
                ObfuscateField = Parse(GetDictionaryValue("ObfuscateField"));
                ObfuscateFieldPrivate = Parse(GetDictionaryValue("ObfuscateFieldPrivate"));
                ObfuscateFieldProtected = Parse(GetDictionaryValue("ObfuscateFieldProtected"));
                ObfuscateFieldPublic = Parse(GetDictionaryValue("ObfuscateFieldPublic"));

                //Property
                ObfuscateProperty = Parse(GetDictionaryValue("ObfuscateProperty"));

                //Event
                ObfuscateEvent = Parse(GetDictionaryValue("ObfuscateEvent"));

                //Method
                ObfuscateMethod = Parse(GetDictionaryValue("ObfuscateMethod"));
                ObfuscateMethodPrivate = Parse(GetDictionaryValue("ObfuscateMethodPrivate"));
                ObfuscateMethodProtected = Parse(GetDictionaryValue("ObfuscateMethodProtected"));
                ObfuscateMethodPublic = Parse(GetDictionaryValue("ObfuscateMethodPublic"));
                ObfuscateUnityMethod = Parse(GetDictionaryValue("ObfuscateUnityMethod"));

                //Enum
                ObfuscateEnumValues = Parse(GetDictionaryValue("ObfuscateEnumValues"));

                //MonoBehaviour
                ObfuscateUnityClasses = Parse(GetDictionaryValue("ObfuscateUnityClasses"));
                ObfuscateUnityPublicFields = Parse(GetDictionaryValue("ObfuscateUnityPublicFields"));
                ObfuscateClassGeneric = Parse(GetDictionaryValue("ObfuscateClassGeneric"));
                ObfuscateClassAbstract = Parse(GetDictionaryValue("ObfuscateClassAbstract"));

                //String Obfuscation
                ObfuscateString = Parse(GetDictionaryValue("ObfuscateString"));
                StoreObfuscatedStrings = Parse(GetDictionaryValue("StoreObfuscatedStrings"));

                //Add Random Code
                AddRandomCode = Parse(GetDictionaryValue("AddRandomCode"));

                //Add Random Code
                MakeAssemblyTypesUnreadable = Parse(GetDictionaryValue("MakeAssemblyTypesUnreadable"));

                //Gui
                TryFindGuiMethods = Parse(GetDictionaryValue("TryFindGuiMethods"));

                //Animation
                TryFindAnimationMethods = Parse(GetDictionaryValue("TryFindAnimationMethods"));

                //SaveNamesToPathFile
                SaveNamesToPathFile = Parse(GetDictionaryValue("SaveNamesToPathFile"));
                SaveNamePathFile = GetDictionaryValue("SaveNamePathFile");

                //Namespaces
                String[] var_NamespaceToIgnoreArray = GetDictionaryValue("NamespacesToIgnoreList").Split('|');
                NamespacesToIgnoreList = new List<string>();
                for (int i = 0; i < var_NamespaceToIgnoreArray.Length - 1; i++)
                {
                    NamespacesToIgnoreList.Add(var_NamespaceToIgnoreArray[i]);
                }

                NamespaceViceVersa = Parse(GetDictionaryValue("NamespaceViceVersa"));

                //Attributes
                String[] var_AttributesBehaveLikeDoNotRenameList = GetDictionaryValue("AttributesBehaveLikeDoNotRenameList").Split('|');
                AttributesBehaveLikeDoNotRenameList = new List<string>();
                for (int i = 0; i < var_AttributesBehaveLikeDoNotRenameList.Length - 1; i++)
                {
                    AttributesBehaveLikeDoNotRenameList.Add(var_AttributesBehaveLikeDoNotRenameList[i]);
                }
            }
            catch
            {
            }
        }

        private static void ReadDictionary(String _FileName)
        {
            SettingDictionary = new Dictionary<string, string>();

            if (System.IO.File.Exists(_FileName))
            {
                System.IO.StreamReader var_StreamReader = new System.IO.StreamReader(_FileName);

                String var_NameLine;

                while ((var_NameLine = var_StreamReader.ReadLine()) != null)
                {
                    String var_ValueLine = var_StreamReader.ReadLine();
                    SettingDictionary.Add(var_NameLine, var_ValueLine);
                }

                var_StreamReader.Close();
            }
        }

        private static String GetDictionaryValue(String _Key)
        {
            String var_Value;
            if (SettingDictionary.TryGetValue(_Key, out var_Value))
            {
                return var_Value;
            }
            return "";
        }

        public static void SaveSettings()
        {
            try
            {
                String var_FileName = UnityEngine.Application.dataPath + "\\OPS\\Obfuscator.Free\\Obfuscator_Settings.txt";
                if (!System.IO.File.Exists(var_FileName))
                {
                    System.IO.File.Create(var_FileName);
                }

                System.IO.StreamWriter var_Writer = new System.IO.StreamWriter(var_FileName);

                //Global
                WriteDictionaryPair(var_Writer, "ObfuscateGlobally", Parse(ObfuscateGlobally));

                //Namespace
                WriteDictionaryPair(var_Writer, "ObfuscateNamespace", Parse(ObfuscateNamespace));

                //Class
                WriteDictionaryPair(var_Writer, "ObfuscateClass", Parse(ObfuscateClass));
                WriteDictionaryPair(var_Writer, "ObfuscateClassPrivate", Parse(ObfuscateClassPrivate));
                WriteDictionaryPair(var_Writer, "ObfuscateClassProtected", Parse(ObfuscateClassProtected));
                WriteDictionaryPair(var_Writer, "ObfuscateClassPublic", Parse(ObfuscateClassPublic));

                //Field
                WriteDictionaryPair(var_Writer, "ObfuscateField", Parse(ObfuscateField));
                WriteDictionaryPair(var_Writer, "ObfuscateFieldPrivate", Parse(ObfuscateFieldPrivate));
                WriteDictionaryPair(var_Writer, "ObfuscateFieldProtected", Parse(ObfuscateFieldProtected));
                WriteDictionaryPair(var_Writer, "ObfuscateFieldPublic", Parse(ObfuscateFieldPublic));

                //Property
                WriteDictionaryPair(var_Writer, "ObfuscateProperty", Parse(ObfuscateProperty));

                //Event
                WriteDictionaryPair(var_Writer, "ObfuscateEvent", Parse(ObfuscateEvent));

                //Method
                WriteDictionaryPair(var_Writer, "ObfuscateMethod", Parse(ObfuscateMethod));
                WriteDictionaryPair(var_Writer, "ObfuscateMethodPrivate", Parse(ObfuscateMethodPrivate));
                WriteDictionaryPair(var_Writer, "ObfuscateMethodProtected", Parse(ObfuscateMethodProtected));
                WriteDictionaryPair(var_Writer, "ObfuscateMethodPublic", Parse(ObfuscateMethodPublic));
                WriteDictionaryPair(var_Writer, "ObfuscateUnityMethod", Parse(ObfuscateUnityMethod));

                //Enum
                WriteDictionaryPair(var_Writer, "ObfuscateEnumValues", Parse(ObfuscateEnumValues));

                //MonoBehaviour
                WriteDictionaryPair(var_Writer, "ObfuscateUnityClasses", Parse(ObfuscateUnityClasses));
                WriteDictionaryPair(var_Writer, "ObfuscateUnityPublicFields", Parse(ObfuscateUnityPublicFields));
                WriteDictionaryPair(var_Writer, "ObfuscateClassGeneric", Parse(ObfuscateClassGeneric));
                WriteDictionaryPair(var_Writer, "ObfuscateClassAbstract", Parse(ObfuscateClassAbstract));

                //String Obfuscation
                WriteDictionaryPair(var_Writer, "ObfuscateString", Parse(ObfuscateString));
                WriteDictionaryPair(var_Writer, "StoreObfuscatedStrings", Parse(StoreObfuscatedStrings));

                //Add Random Code
                WriteDictionaryPair(var_Writer, "AddRandomCode", Parse(AddRandomCode));

                //Add Random Code
                WriteDictionaryPair(var_Writer, "MakeAssemblyTypesUnreadable", Parse(MakeAssemblyTypesUnreadable));

                //Gui
                WriteDictionaryPair(var_Writer, "TryFindGuiMethods", Parse(TryFindGuiMethods));

                //Animation
                WriteDictionaryPair(var_Writer, "TryFindAnimationMethods", Parse(TryFindAnimationMethods));

                //SaveNamesToPathFile
                WriteDictionaryPair(var_Writer, "SaveNamesToPathFile", Parse(SaveNamesToPathFile));
                WriteDictionaryPair(var_Writer, "SaveNamePathFile", SaveNamePathFile);

                //Namespaces
                String var_NamespaceValue = "";
                for (int i = 0; i < NamespacesToIgnoreList.Count; i++)
                {
                    var_NamespaceValue += NamespacesToIgnoreList[i];
                    var_NamespaceValue += "|";
                    /*if(i != NamespacesToIgnoreList.Count - 1)
                    {
                        var_NamespaceValue += "|";
                    }*/
                }
                WriteDictionaryPair(var_Writer, "NamespacesToIgnoreList", var_NamespaceValue);

                WriteDictionaryPair(var_Writer, "NamespaceViceVersa", Parse(NamespaceViceVersa));

                //Attributes
                String var_AttributeValue = "";
                for (int i = 0; i < AttributesBehaveLikeDoNotRenameList.Count; i++)
                {
                    var_AttributeValue += AttributesBehaveLikeDoNotRenameList[i];
                    var_AttributeValue += "|";
                }
                WriteDictionaryPair(var_Writer, "AttributesBehaveLikeDoNotRenameList", var_NamespaceValue);

                var_Writer.Close();
            }
            catch
            {
            }
        }

        private static void WriteDictionaryPair(System.IO.StreamWriter _Writer, String _Key, String _Value)
        {
            _Writer.WriteLine(_Key);
            _Writer.WriteLine(_Value);
        }
    }
}
