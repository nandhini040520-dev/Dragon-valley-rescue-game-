using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace DragonValley.Editor
{
    public static class Phase2Builder
    {
        public static void Build()
        {
            AdventureBuilder.Prepare();
            var args=Environment.GetCommandLineArgs(); int at=Array.IndexOf(args,"-phase2Output");
            string output=at>=0?args[at+1]:Path.GetFullPath("Build/Phase2/Adventure Quest Phase 2.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{"Assets/Scenes/AdventureWorld.unity"},locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(output),"build-result.txt"),report.summary.result+" | "+report.summary.totalErrors+" errors");
            if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Phase 2 build failed");
        }
    }
}
