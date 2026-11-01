using System.Collections.Generic;
using System.Text;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UUP.EditorTools.Editor
{
    /// <summary>
    /// Removes every <see cref="Comment"/> component from the scenes included in a player build.
    /// </summary>
    /// <remarks>
    /// Only the copies of the scenes that go into the build are modified; your scene files are left untouched.
    /// Comments are kept when entering Play Mode in the Editor.
    /// </remarks>
    internal sealed class CommentBuildStripper : IPreprocessBuildWithReport, IProcessSceneWithReport, IPostprocessBuildWithReport
    {
        // Static because Unity may create a separate instance of this class for each callback interface.
        private static readonly List<GameObject> s_RootObjects = new List<GameObject>();
        private static readonly Dictionary<string, List<string>> s_StrippedPaths = new Dictionary<string, List<string>>();

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            s_StrippedPaths.Clear();
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            // The report is null when the scene is loaded for Play Mode: keep the comments in that case.
            if (report == null)
                return;

            scene.GetRootGameObjects(s_RootObjects);
            foreach (GameObject root in s_RootObjects)
            {
                foreach (Comment comment in root.GetComponentsInChildren<Comment>(true))
                {
                    Record(scene.path, comment.transform);
                    Object.DestroyImmediate(comment);
                }
            }

            s_RootObjects.Clear();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (EditorToolsSettings.LogStrippedComments && s_StrippedPaths.Count > 0)
                Debug.Log(BuildMessage());

            s_StrippedPaths.Clear();
        }

        private static void Record(string sceneName, Transform transform)
        {
            if (!s_StrippedPaths.TryGetValue(sceneName, out List<string> paths))
            {
                paths = new List<string>();
                s_StrippedPaths.Add(sceneName, paths);
            }

            paths.Add(GetHierarchyPath(transform));
        }

        private static string BuildMessage()
        {
            var builder = new StringBuilder("[Editor Tools] Comment components removed from the build:\n");
            foreach (KeyValuePair<string, List<string>> scene in s_StrippedPaths)
            {
                builder.AppendLine($"Scene '{scene.Key}' ({scene.Value.Count}):");
                foreach (string path in scene.Value)
                    builder.Append("    ").AppendLine(path);
            }

            builder.Append("You can disable this message in Preferences > Editor Tools.");
            return builder.ToString();
        }

        private static string GetHierarchyPath(Transform transform)
        {
            var builder = new StringBuilder(transform.name);
            for (Transform parent = transform.parent; parent != null; parent = parent.parent)
                builder.Insert(0, '/').Insert(0, parent.name);
            return builder.ToString();
        }
    }
}
