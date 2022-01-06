// C# example.
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using UnityEngine;

public class ScriptBatch 
{

    [MenuItem("Design Demolish/Tools/Build Client")]
    private static void BuildClientForWindows()
    {
        string buildFolder = Path.Combine("Build");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/World.unity" },
            locationPathName = Path.Combine(buildFolder, "Design Demolish.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.ShowBuiltPlayer | BuildOptions.Development
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Client build succeeded: " + (summary.totalSize / 1024) + " kb");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Client build failed");
        }

        // Copy a file from the project folder to the build folder, alongside the built game.
        FileUtil.CopyFileOrDirectory("Assets/Resources/Data", buildFolder + "/Design Demolish_Data/Resources/Data");
    }

    [MenuItem("Design Demolish/Tools/Copy Resources To Folder")]
    private static void CopyResourcesToFolder()
    {
        string buildFolder = Path.Combine("Build");

        // Copy a file from the project folder to the build folder, alongside the built game.
        FileUtil.CopyFileOrDirectory("Assets/Resources/Data", buildFolder + "/Design Demolish_Data/Resources/Data");
    }
}

