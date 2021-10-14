using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class PostBuildProcessor : IPostprocessBuildWithReport
{
    public void OnPostprocessBuild(BuildReport report)
    {
        Debug.Log(report.summary.outputPath);
        // report.summary.outputPath will lead you to generated .exe file
        // so you can use this path to do whatever you want

        var directoryName = Path.GetDirectoryName(report.summary.outputPath);
        directoryName = Path.Combine(directoryName, "Iron Tracks TD_Data", "Levels");
        Debug.Log(directoryName);
        
        
        Directory.CreateDirectory(directoryName);

        var allSavePaths = LevelDataLoader.GetAllLevelDataPaths();

        for (int i = 0; i < allSavePaths.Length; i++) {
            var dest = Path.Combine(directoryName, Path.GetFileName(allSavePaths[i]));
            File.Copy(allSavePaths[i], dest);
        }
    }

    public int callbackOrder { get; }
}
