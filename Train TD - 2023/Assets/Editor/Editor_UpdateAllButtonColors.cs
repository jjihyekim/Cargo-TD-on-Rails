using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class Editor_UpdateAllButtonColors : EditorWindow {
    private ColorBlock _colorBlock = ColorBlock.defaultColorBlock;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/UpdateAllButtonColors")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        Editor_UpdateAllButtonColors window = (Editor_UpdateAllButtonColors)EditorWindow.GetWindow(typeof(Editor_UpdateAllButtonColors));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Replace colors of EVERY button", EditorStyles.boldLabel);


        _colorBlock.normalColor = EditorGUILayout.ColorField("normalColor", _colorBlock.normalColor);
        _colorBlock.highlightedColor = EditorGUILayout.ColorField("highlightedColor", _colorBlock.highlightedColor);
        _colorBlock.pressedColor = EditorGUILayout.ColorField("pressedColor", _colorBlock.pressedColor);
        _colorBlock.selectedColor = EditorGUILayout.ColorField("selectedColor", _colorBlock.selectedColor);
        _colorBlock.disabledColor = EditorGUILayout.ColorField("disabledColor", _colorBlock.disabledColor);

        if (GUILayout.Button("Apply to Scene Objects")) {
            var allButtons = FindObjectsOfType<Button>();

            for (int i = 0; i < allButtons.Length; i++) {
                allButtons[i].colors = _colorBlock;
            }
        }
        
        if (GUILayout.Button("Apply to Prefabs in the same folder as the selected prefab")) {
            var path = GetSelectedPathOrFallback();

            if (path.Length > 0) {
                var files = GetFiles(path);
                foreach (var file in files) {
                    if (!file.Contains(".meta")) {
                        using (var editingScope = new PrefabUtility.EditPrefabContentsScope(file)) {
                            var prefabRoot = editingScope.prefabContentsRoot;

                            var allButtons = prefabRoot.GetComponentsInChildren<Button>();

                            for (int i = 0; i < allButtons.Length; i++) {
                                allButtons[i].colors = _colorBlock;
                            }
                            
                            Debug.Log($"{allButtons.Length} buttons changed in {prefabRoot.name}");
                        }
                    }
                }
            }
            
            
        }
    }
    
    /// <summary>
    /// Retrieves selected folder on Project view.
    /// </summary>
    /// <returns></returns>
    public static string GetSelectedPathOrFallback()
    {
        //string path = "Assets";
        string path = "";

        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }

    /// <summary>
    /// Recursively gather all files under the given path including all its subfolders.
    /// </summary>
    static IEnumerable<string> GetFiles(string path)
    {
        Queue<string> queue = new Queue<string>();
        queue.Enqueue(path);
        while (queue.Count > 0)
        {
            path = queue.Dequeue();
            try
            {
                foreach (string subDir in Directory.GetDirectories(path))
                {
                    queue.Enqueue(subDir);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
            }
            string[] files = null;
            try
            {
                files = Directory.GetFiles(path);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
            }
            if (files != null)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    yield return files[i];
                }
            }
        }
    }
}