using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class RecentAssetsWindow : EditorWindow
{
    private static List<Object> recentAssets = new List<Object>();
    private const int MaxAssets = 10;
    private const string RecentAssetsKey = "RecentAssetsWindow_RecentAssets";

    [MenuItem("Window/Recent Assets")]
    public static void ShowWindow()
    {
        GetWindow<RecentAssetsWindow>("Recent Assets");
    }

    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChanged;
        LoadRecentAssets();
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
        SaveRecentAssets();
    }

    private void OnSelectionChanged()
    {
        if (Selection.activeObject != null && !recentAssets.Contains(Selection.activeObject))
        {
            recentAssets.Insert(0, Selection.activeObject);

            if (recentAssets.Count > MaxAssets)
            {
                recentAssets.RemoveAt(MaxAssets);
            }
        }

        Repaint();
    }

    private void OnGUI()
    {
        GUILayout.Label("Recent Assets", EditorStyles.boldLabel);

        if (recentAssets.Count == 0)
        {
            GUILayout.Label("No assets selected yet.");
            return;
        }

        for (int i = 0; i < recentAssets.Count; i++)
        {
            Object asset = recentAssets[i];
            if (asset == null) continue;

            GUILayout.BeginHorizontal();

            // Get the icon for the asset
            Texture icon = GetIconForAsset(asset);
            
            GUIContent content = new GUIContent(asset.name, icon);
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 25,
                alignment = TextAnchor.MiddleLeft,
                imagePosition = ImagePosition.ImageLeft
            };

            if (GUILayout.Button(content, buttonStyle, GUILayout.Height(25)))
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }

            GUILayout.EndHorizontal();
        }
    }

    private Texture GetIconForAsset(Object asset)
    {
        string assetPath = AssetDatabase.GetAssetPath(asset);
        if (string.IsNullOrEmpty(assetPath))
            return null;
        
        if (AssetDatabase.IsValidFolder(assetPath))
        {
            return EditorGUIUtility.IconContent("Folder Icon").image;
        }
        
        switch (asset)
        {
            case GameObject _ when PrefabUtility.IsPartOfAnyPrefab(asset):
                return EditorGUIUtility.IconContent("Prefab Icon").image;

            case MonoScript _:
                return EditorGUIUtility.IconContent("cs Script Icon").image;

            case SceneAsset _:
                return EditorGUIUtility.IconContent("SceneAsset Icon").image;

            case Material _:
                return EditorGUIUtility.IconContent("Material Icon").image;

            case Texture _:
                return EditorGUIUtility.IconContent("Texture Icon").image;

            case AudioClip _:
                return EditorGUIUtility.IconContent("AudioClip Icon").image;

            default:
                return AssetPreview.GetMiniThumbnail(asset);
        }
    }

    private void SaveRecentAssets()
    {
        List<string> assetPaths = new List<string>();

        foreach (Object asset in recentAssets)
        {
            if (asset != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(asset);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    assetPaths.Add(assetPath);
                }
            }
        }

        string json = JsonUtility.ToJson(new AssetPathList { Paths = assetPaths });
        EditorPrefs.SetString(RecentAssetsKey, json);
    }

    private void LoadRecentAssets()
    {
        recentAssets.Clear();

        if (EditorPrefs.HasKey(RecentAssetsKey))
        {
            string json = EditorPrefs.GetString(RecentAssetsKey);
            AssetPathList assetPathList = JsonUtility.FromJson<AssetPathList>(json);

            if (assetPathList != null && assetPathList.Paths != null)
            {
                foreach (string assetPath in assetPathList.Paths)
                {
                    Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    if (asset != null)
                    {
                        recentAssets.Add(asset);
                    }
                }
            }
        }
    }

    [System.Serializable]
    private class AssetPathList
    {
        public List<string> Paths;
    }
}
