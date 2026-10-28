using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
namespace DragonValley.Editor
{
    public static class AdventureBuilder
    {
        [MenuItem("Adventure Quest/Create AdventureWorld Scene")]
        public static void Prepare()
        {
            PlayerSettings.companyName="Funobotz Discovery"; PlayerSettings.productName="Adventure Quest - The Dragon Valley Rescue";
            PlayerSettings.defaultScreenWidth=1600; PlayerSettings.defaultScreenHeight=900; PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            PlayerSettings.runInBackground=true; PlayerSettings.resizableWindow=true; PlayerSettings.colorSpace=ColorSpace.Linear;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.Standalone,ManagedStrippingLevel.Disabled);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{GraphicsDeviceType.Direct3D11});
            QualitySettings.antiAliasing=4; QualitySettings.shadows=ShadowQuality.All; QualitySettings.shadowDistance=70; QualitySettings.shadowResolution=ShadowResolution.Medium; QualitySettings.pixelLightCount=4; QualitySettings.vSyncCount=1;
            Directory.CreateDirectory("Assets/Resources");
            foreach(var name in new[]{"PlainSurface","GlowSurface"}) {
                string path="Assets/Resources/"+name+".mat";
                if(!AssetDatabase.LoadAssetAtPath<Material>(path)) {
                    var material=new Material(Shader.Find("Standard")); material.enableInstancing=true;
                    if(name=="GlowSurface") { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor",Color.white); }
                    AssetDatabase.CreateAsset(material,path);
                }
                var painted=AssetDatabase.LoadAssetAtPath<Material>(path);
                var paintShader=Shader.Find("DragonValley/StorybookPaint");
                if(paintShader) { painted.shader=paintShader; EditorUtility.SetDirty(painted); }
            }
            // Resource material assets retain Standard variants. Text fonts retain their own shader.
            var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
            var shaders=settings.FindProperty("m_AlwaysIncludedShaders");
            // Unity's GUI/Text Shader is an internal editor resource and must not be forced into a player.
            for(int i=shaders.arraySize-1;i>=0;i--) {
                var included=shaders.GetArrayElementAtIndex(i).objectReferenceValue as Shader;
                if(included && included.name=="GUI/Text Shader") { shaders.GetArrayElementAtIndex(i).objectReferenceValue=null; shaders.DeleteArrayElementAtIndex(i); }
            }
            foreach(string name in new[]{"Standard","DragonValley/KitPrint"}) {
                var shader=Shader.Find(name); bool found=false;
                for(int i=0;i<shaders.arraySize;i++) if(shaders.GetArrayElementAtIndex(i).objectReferenceValue==shader) found=true;
                if(shader && !found) { int index=shaders.arraySize; shaders.InsertArrayElementAtIndex(index); shaders.GetArrayElementAtIndex(index).objectReferenceValue=shader; }
            }
            settings.ApplyModifiedPropertiesWithoutUndo();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            new GameObject("Adventure Quest • Game director").AddComponent<AdventureGame>();
            Directory.CreateDirectory("Assets/Scenes"); EditorSceneManager.SaveScene(scene,"Assets/Scenes/AdventureWorld.unity");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/AdventureWorld.unity",true)};
            AssetDatabase.SaveAssets();
            Debug.Log("QUEST_FOUNDATION_OK Unity="+Application.unityVersion+" Pipeline=Built-in Scene=AdventureWorld");
        }
        [MenuItem("Adventure Quest/Build Windows Prototype")]
        public static void Build()
        {
            Prepare();
            Directory.CreateDirectory("Build/Windows");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=new[]{"Assets/Scenes/AdventureWorld.unity"},locationPathName="Build/Windows/Adventure Quest.exe",target=BuildTarget.StandaloneWindows64, options=BuildOptions.CleanBuildCache });
            var summary="QUEST_BUILD: "+report.summary.result+" | "+report.summary.totalSize+" bytes | "+report.summary.totalErrors+" errors";
            Debug.Log(summary); Directory.CreateDirectory("QA"); File.WriteAllText("QA/build-result.txt",summary);
            if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Adventure Quest Windows build failed.");
        }
    }
}
