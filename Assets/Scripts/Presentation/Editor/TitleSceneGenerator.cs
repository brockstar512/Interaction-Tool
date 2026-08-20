#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IT.Presentation.EditorTools
{
    // 4.6.1 R4 (OQ-A(1) ruled + OQ-G(3)): the Title scene is GENERATED, not
    // authored — one owner click creates Assets/Scenes/Title.unity (camera +
    // TitleScreen; the PresentationRoot/canvas is code-ensured at runtime like
    // every scene) and appends it to Build Settings. Idempotent.
    static class TitleSceneGenerator
    {
        const string ScenePath = "Assets/Scenes/Title.unity";

        [MenuItem("Tools/4.6.1 R4: Generate Title scene (one click)")]
        static void Generate()
        {
            if (!System.IO.File.Exists(ScenePath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
                var titleGo = new GameObject("TitleScreen");
                titleGo.AddComponent<TitleScreen>();
                SceneManager.MoveGameObjectToScene(titleGo, scene);
                EditorSceneManager.SaveScene(scene, ScenePath);
                EditorSceneManager.CloseScene(scene, true);
                Debug.Log($"[Title] generated {ScenePath}");
            }
            else
            {
                Debug.Log("[Title] scene already exists — skipping generation.");
            }

            if (!EditorBuildSettings.scenes.Any(s => s.path == ScenePath))
            {
                EditorBuildSettings.scenes = EditorBuildSettings.scenes
                    .Append(new EditorBuildSettingsScene(ScenePath, true)).ToArray();
                Debug.Log("[Title] added to Build Settings.");
            }
            else
            {
                Debug.Log("[Title] already in Build Settings.");
            }
        }
    }
}
#endif
