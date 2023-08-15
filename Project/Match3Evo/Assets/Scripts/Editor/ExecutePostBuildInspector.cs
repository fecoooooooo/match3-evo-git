using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ExecutePostBuild))]
public class ExecutePostBuildInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("ExecutePostBuild"))
        {
	    // SkillzSDK.Internal.Build.iOS.SkillzPostProcessBuild.OnPostProcessBuild(BuildTarget.iOS, EditorUtility.OpenFolderPanel("Build Folder Path", "", ""));
        }
    }
}
