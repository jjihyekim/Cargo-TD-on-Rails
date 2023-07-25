using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// I recommend dropping this script in an Editor folder.
// You should have two audio clips somewhere in the project.
// You'll need to edit-in the paths of those clips (from your project root folder) in the static initializer below.
// Example path: "Assets/Editor/CompileIndicator/start.mp3"


/// <summary>
/// Plays a sound effect when script compiling starts and ends. 
/// </summary>
[InitializeOnLoad]
public static class CompileIndicator {

	private const string CompileStatePrefsKey = "CompileIndicator.WasCompiling";
	private const string DoPlaySoundKey = "CompileIndicator.PlaySound";
	private static AudioClip StartClip;
	private static AudioClip EndClip;

	private static bool doPlay = false;

	static CompileIndicator() {
		EditorApplication.update += OnUpdate;
		doPlay = EditorPrefs.GetBool(DoPlaySoundKey);
		CacheSounds();
	}

	[MenuItem("CompileSounds/CacheSounds")]
	private static void CacheSounds() {
		string guid = AssetDatabase.FindAssets("confirmation_001")[0];
		StartClip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));
		guid = AssetDatabase.FindAssets("Train Whistle-1")[0];
		EndClip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));
		Debug.Log($"Sounds cached {StartClip} - {EndClip}");
	}
	
	[MenuItem("CompileSounds/ToggleSounds")]
	private static void ToggleSounds() {
		doPlay = EditorPrefs.GetBool(DoPlaySoundKey);
		doPlay = !doPlay;
		EditorPrefs.SetBool(DoPlaySoundKey, doPlay);
		Debug.Log($"Sound toggled: {doPlay}");
	}

	private static void OnUpdate() {
		if (doPlay) {
			var wasCompiling = EditorPrefs.GetBool(CompileStatePrefsKey);
			var isCompiling = EditorApplication.isCompiling;

			// Return early if compile status hasn't changed.
			if (wasCompiling == isCompiling)
				return;

			if (isCompiling)
				OnStartCompiling();
			else
				OnEndCompiling();

			EditorPrefs.SetBool(CompileStatePrefsKey, isCompiling);
		}
	}

	private static void OnStartCompiling() {
		PlayClip(StartClip);
	}

	private static void OnEndCompiling() {
		PlayClip(EndClip);
	}

	/*private static void PlayClip(AudioClip clip) {
		Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
		Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
		MethodInfo method = audioUtilClass.GetMethod(
			"PlayClip",
			BindingFlags.Static | BindingFlags.Public,
			null,
			new[] { typeof(AudioClip) },
			null
		);
		method.Invoke(null, new object[] { clip });
	}*/
	
	public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false)
	{
		Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
     
		Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
		MethodInfo method = audioUtilClass.GetMethod(
			"PlayPreviewClip",
			BindingFlags.Static | BindingFlags.Public,
			null,
			new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
			null
		);
 
		//Debug.Log(method);
		method.Invoke(
			null,
			new object[] { clip, startSample, loop }
		);
	}

}
