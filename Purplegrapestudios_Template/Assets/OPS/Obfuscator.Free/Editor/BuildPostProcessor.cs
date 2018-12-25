#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;

namespace Obfuscator
{
    public class BuildPostProcessor
    {
        //Defines if an Obfuscation Process took place.
        private static bool hasObfuscated = false;

        //The Main Obfuscation Program
        private static ObfuscatorProgram obfuscatorProgram;

        /// <summary>
        /// The PostprocessBuild is called after your Game is build.
        /// It is an important part of the Obfuscation.
        /// Because after building the dll's / your Game, this will get called to Obfuscate your Game.
        /// </summary>
        /// <param name="_Target"></param>
        /// <param name="_PathToBuiltProject"></param>
        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget _Target, string _PathToBuiltProject)
        {
            if(hasObfuscated)
            {
                if (obfuscatorProgram != null)
                {
                    obfuscatorProgram.Finish(_PathToBuiltProject);
                }
            }

            ClearUp();
        }

        [PostProcessScene(1)]
        public static void OnPostProcessScene()
        {
            if(!hasObfuscated)
            {
                if (BuildPipeline.isBuildingPlayer && !EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    //Init
                    obfuscatorProgram = new ObfuscatorProgram();

                    //Check Settings
                    if (!Gui.GuiSettings.SettingsGotReloaded)
                    {
                        Gui.GuiSettings.LoadSettings();
                        Gui.GuiSettings.SettingsGotReloaded = true;
                    }

                    //Apply Settings
                    obfuscatorProgram.PassSettings(Gui.GuiSettings.ObfuscateGlobally,
                        Gui.GuiSettings.ObfuscateNamespace,
                        Gui.GuiSettings.ObfuscateClass, Gui.GuiSettings.ObfuscateClassPrivate, Gui.GuiSettings.ObfuscateClassProtected, Gui.GuiSettings.ObfuscateClassPublic,
                        Gui.GuiSettings.ObfuscateClassGeneric, Gui.GuiSettings.ObfuscateClassAbstract,
                        Gui.GuiSettings.ObfuscateField, Gui.GuiSettings.ObfuscateFieldPrivate, Gui.GuiSettings.ObfuscateFieldProtected, Gui.GuiSettings.ObfuscateFieldPublic,
                        Gui.GuiSettings.ObfuscateProperty,
                        Gui.GuiSettings.ObfuscateEvent,
                        Gui.GuiSettings.ObfuscateMethod, Gui.GuiSettings.ObfuscateMethodPrivate, Gui.GuiSettings.ObfuscateMethodProtected, Gui.GuiSettings.ObfuscateMethodPublic, Gui.GuiSettings.ObfuscateUnityMethod,
                        Gui.GuiSettings.ObfuscateEnumValues,
                        Gui.GuiSettings.ObfuscateUnityClasses, Gui.GuiSettings.ObfuscateUnityPublicFields,
                        Gui.GuiSettings.ObfuscateString, Gui.GuiSettings.StoreObfuscatedStrings,
                        Gui.GuiSettings.AddRandomCode,
                        Gui.GuiSettings.MakeAssemblyTypesUnreadable,
                        Gui.GuiSettings.TryFindGuiMethods,
                        Gui.GuiSettings.TryFindAnimationMethods,
                        Gui.GuiSettings.SaveNamesToPathFile, Gui.GuiSettings.SaveNamePathFile,
                        Gui.GuiSettings.NamespacesToIgnoreList, Gui.GuiSettings.NamespaceViceVersa,
                        Gui.GuiSettings.AttributesBehaveLikeDoNotRenameList);

                    //Obfuscate Assemblies
                    obfuscatorProgram.ObfuscateAssemblies();
                }
                hasObfuscated = true;
            }   
        }

        private static void ClearUp()
        {
            hasObfuscated = false;
            obfuscatorProgram = null;
        }
    }
}
#endif