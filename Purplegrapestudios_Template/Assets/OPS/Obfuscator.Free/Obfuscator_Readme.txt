Obfuscator Copyright (c) 2017-2018 OrangePearSoftware. All Rights Reserved


Usage
=====

OrangePearSoftwares Obfuscator is developed to increase software and game security especially for
games build with Unity3d. It feature is to obfuscated built dotNet assemblies, like
assembly-csharp and assembly-unityscript for the Platforms Windows/Mac/Linux Stan-
dalone, the embedded Platform Android and IPhone and consoles.
Obfuscator Free considers specific Unity features, like MonoBehaviour, NetworkBe-
haviours, Serialization, Reflection, and so on, to allow a easy and out of the box working
obfuscator. Obfuscator features reachs from simple renaming:
- Classes
- Fields
- Propertys
- Events
- Methods
Up to Method content obfuscation.

With the Pro Version you have access to:
- MonoBehaviour/NetworkBehaviour/ScriptAbleObject obfuscation.
- String obfuscation.
- Protect classes against refactoring!
- And many more features!

Options
=======

From within Unity, select "Window->Obfuscator Setting".

From the Inspector window, you can now see the Obfuscation options available along with descriptions where relevant. The default settings provide a solid configuration that obfuscates the majority of your code, but here you have general control over what is obfuscated.

Code Attributes
===============

Methods often need to be left unobfuscated so that they can be referenced from an external plugin via reflection, or for some other reason. Or maybe you just want a field called "password" to appear as "versionId" when viewed by a decompiler.

You can achieve this by adding Attributes to your code.

The following OrangePearSoftware specific attributes are supported:

[DoNotRenameAttribute]                   - The obfuscator will not rename this class/method/field, but will continue to obfuscate its contents (if relevant).
[DoNotObfuscateClassAttribute]           - The obfuscator will not rename this class, nor will it obfuscate its contents (fields/methods/...).
[DoNotObfuscateMethodBodyAttribute]      - The obfuscator will not rename content in the specific method.
[SharedNameAttribute]                    - Add this to an Class, Field, Method, whatever and it will share the same obfuscated name with all Class, Field, Method, whatever that share the same (original) name.
[DoNotUseClassForFakeCodeAttribute]      - Add this to an Class, to disallow fake code adding! Or to disallow using this class to create new fake classes basing on it!
[DoNotMakeClassUnDecompileAbleAttribute] - Add this to an Class, to disallow making it unreadable by dissassembler!

Troubleshooting F.A.Q
=====================

Q. After obfuscating, my 3rd party plugin has stopped working!

A. The simplest way to fix this is to look at the plugin's script to see what namespace they use. Then, in the inspector window in "Window->Obfuscator Settings" there is an array called "Skip Namespaces". Add the plugin's namespace to this array and the obfuscator will ignore any matching namespaces. Occassionally a plugin will forget to use namespaces for its scripts, in which case you have to : Annotate each class with [Obfuscator.Attribute.DoNotObfuscateClassAttribute].


Q. Button clicks don't work anymore!

A. Check your Options and see if you enabled the "public methods". If you did, then make sure you've added a [Obfuscator.Attribute.DoNotRenameAttribute] attribute to the button click method.
   For a more obfuscated approach you could assign button clicks programatically. For example, here the ButtonMethod can be obfuscated:

     public Button mybutton;

     public void Start()
     {
       mybutton.onClick.AddListener(ButtonMethod);
     }
  
   The same process works for all gui methods.
   But mostly if you check in the inspector window in "Window->Obfuscator Settings": Find Gui Methods, it will find many to all!


Q. Animation events don't work anymore!

A. See "Button clicks don't work anymore!". If a method is being typed into the inspector, you should exclude it from Obfuscation.
   Here works also: if you check in the inspector window in "Window->Obfuscator Settings": Find Animation Methods, it will find many to all!

Q. It's not working for a certain platform.

A. Regardless of the platform, send us an email (orangepearsoftware@gmail.com) with the error and we'll see what we can do.


Q. How can we run obfuscation later in the build process?

A. You can control this in the Assets/ObfuscatorPro/Editor/BuildPostProcessor.cs script. The PostProcessScene attribute on the Obfuscate method has an index number that you can freely change to enable other scripts to be called first.


Q. Can I obfuscate externally created DLLs?

A. Currently no.


Q. How do I obfuscate local variables?

A. Local variable names are not stored anywhere, so there is nothing to obfuscate. A decompiler tries to guess a local variable's name based on the name of its class, or the method that instantiated it.


Q. What can I do to make it more secure?

A. Try enabling obfuscation of MonoBehaviour and their methods. Enable string obfuscation too obfuscate sensitive methods containing strings you'd prefer people not to see in a decompiler. Refactor your code to use smaller methods.


Q. When I build with Jenkins there is an error: Asset obfuscating finished with ERRORS!

A. When calling BuildPlayer by yourself, through Jenkins for example, some Unity intern events will not get called. So you have to call it yourself.

	Obfuscator.BuildPostProcessor.ManualRestoreUnityObjects();
	BuildPlayer(....)
	Obfuscator.BuildPostProcessor.ManualRestoreUnityObjects();

	
Q. Something's still not working, how can I get in touch with you?

A. Please email us at orangepearsoftware@gmail.com giving as much information about the problem as possible.


What is the next plans for the Obfusactor?
=======

- Enable Obfuscation for externally created party dlls.
- Make the Unity 2017.3 feature of "Folder DLLs" work with the Obfuscator.
- Creating an easy to use plugin system to allow Asset Store Creators to create for their assets plugins to make them work with the Obfuscator.
- More platforms to support.
- And surely bugfixes.