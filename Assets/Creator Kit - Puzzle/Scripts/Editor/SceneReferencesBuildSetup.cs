using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SceneReferencesBuildSetup
{
    static SceneReferencesBuildSetup ()
    {
        EditorApplication.playModeStateChanged += PlayModeStateChanged;
    }
    
    static void PlayModeStateChanged (PlayModeStateChange playModeStateChange)
    {
        if (playModeStateChange == PlayModeStateChange.ExitingEditMode)
        {
            List<string> uniqueScenePaths = GetScenePathsFromSceneReferences (out SceneReference[] sceneReferences);
            
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[uniqueScenePaths.Count];

            for (int i = 0; i < scenes.Length; i++)
            {
                scenes[i] = new EditorBuildSettingsScene(uniqueScenePaths[i], true);
            }

            EditorBuildSettings.scenes = scenes;

            for (int i = 0; i < sceneReferences.Length; i++)
            {
                CacheInfo (sceneReferences[i]);
            }
        }
    }

    static List<string> GetScenePathsFromSceneReferences (out SceneReference[] sceneReferences)
    {
        List<string> uniqueScenePaths = new List<string> ();

        sceneReferences = GetAllSceneReferences ();
        
        IgnoreNullScenes (sceneReferences);

        for (int i = 0; i < sceneReferences.Length; i++)
        {
            SceneReference sceneReference = sceneReferences[i];
            
            if(sceneReference.ignore)
                continue;
            
            string path = AssetDatabase.GetAssetOrScenePath (sceneReference.menuScene);
            if(!uniqueScenePaths.Contains (path))
                uniqueScenePaths.Add (path);
            path = AssetDatabase.GetAssetOrScenePath (sceneReference.levelScene);
            if(!uniqueScenePaths.Contains (path))
                uniqueScenePaths.Add (path);
        }

        return uniqueScenePaths;
    }

    public static SceneReference[] GetAllSceneReferences ()
    {
        string[] guids = AssetDatabase.FindAssets ("t:SceneReference");
        SceneReference[] sceneReferences = new SceneReference[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath (guids[i]);
            sceneReferences[i] = AssetDatabase.LoadAssetAtPath<SceneReference> (path);
        }

        return sceneReferences;
    }

    static void IgnoreNullScenes (SceneReference[] sceneReferences)
    {
        for (int i = 0; i < sceneReferences.Length; i++)
        {
            if (sceneReferences[i].menuScene == null || sceneReferences[i].levelScene == null)
                sceneReferences[i].ignore = true;
        }
    }
    
    static void CacheInfo (SceneReference sceneReference)
    {
        if(sceneReference.ignore)
            return;
        
        string levelPath = AssetDatabase.GetAssetOrScenePath (sceneReference.levelScene);
            
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            if (EditorBuildSettings.scenes[i].path == levelPath)
                sceneReference.levelBuildIndex = i;
        }

        string menuPath = AssetDatabase.GetAssetOrScenePath (sceneReference.menuScene);
        
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            if (EditorBuildSettings.scenes[i].path == menuPath)
                sceneReference.menuBuildIndex = i;
        }
    }
}
