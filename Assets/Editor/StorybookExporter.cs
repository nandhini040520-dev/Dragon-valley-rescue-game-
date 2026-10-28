using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace DragonValley.Editor
{
    public static class StorybookExporter
    {
        static readonly string[] names={"Petalo","Quacky","Tolly","Tiko","Explorer","Aster-Dragon","Delivery-Crate","Signal-Crystals","Brass-Lantern","Forest-Tree","Fairy-Cottage"};
        [MenuItem("Adventure Quest/Export Storybook Models and Images")]
        public static void Export() { ExportModels(false); }
        [MenuItem("Adventure Quest/Update Quacky Model and Images")]
        public static void ExportQuacky() { ExportModels(true); }
        static void ExportModels(bool quackyOnly)
        {
            if(!quackyOnly) AdventureBuilder.Prepare();
            else if(Application.isBatchMode && string.IsNullOrEmpty(SceneManager.GetActiveScene().path)) EditorSceneManager.OpenScene("Assets/Scenes/AdventureWorld.unity");
            Directory.CreateDirectory("Assets/Art/Prefabs"); Directory.CreateDirectory("Assets/Art/Models"); Directory.CreateDirectory("Assets/Art/Materials"); Directory.CreateDirectory("AssetPreviews/Models");
            var original=SceneManager.GetActiveScene(); var preview=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive); SceneManager.SetActiveScene(preview);
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight=new Color(.69f,.72f,.71f); RenderSettings.fog=false;
            var lamp=new GameObject("Portrait key light").AddComponent<Light>(); lamp.type=LightType.Directional; lamp.color=new Color(1,.94f,.8f); lamp.intensity=1.1f; lamp.transform.rotation=Quaternion.Euler(45,215,0); lamp.shadows=LightShadows.Soft;
            var fill=new GameObject("Portrait fill").AddComponent<Light>(); fill.type=LightType.Directional; fill.intensity=.35f; fill.color=new Color(.66f,.78f,1); fill.transform.rotation=Quaternion.Euler(30,40,0);
            var camera=new GameObject("Asset portrait camera").AddComponent<Camera>(); camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.91f,.89f,.82f); camera.orthographic=true; camera.nearClipPlane=.05f; camera.farClipPlane=200; camera.allowHDR=false;
            var models=new List<Transform>();
            for(int i=0;i<names.Length;i++) {
                var model=Create(i); model.name=names[i];
                if(!quackyOnly || i==1) {
                    Persist(model,names[i]);
                    PrefabUtility.SaveAsPrefabAsset(model.gameObject,"Assets/Art/Prefabs/"+names[i]+".prefab");
                    if(quackyOnly) {
                        UnityEngine.Object.DestroyImmediate(model.gameObject);
                        var saved=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Prefabs/Quacky.prefab");
                        model=((GameObject)PrefabUtility.InstantiatePrefab(saved)).transform;
                    }
                }
                var bounds=BoundsOf(model);
                lamp.transform.rotation=Quaternion.Euler(45,i==5?35:215,0);
                lamp.color=i==1?Color.white:new Color(1,.94f,.8f);
                lamp.intensity=i==1?1f:1.1f;
                camera.backgroundColor=i==1?new Color(.81f,.85f,.9f):new Color(.91f,.89f,.82f);
                RenderSettings.ambientLight=i==1?new Color(.65f,.67f,.7f):new Color(.69f,.72f,.71f);
                camera.transform.position=bounds.center+(i==1?new Vector3(.95f,1.9f,3.7f):new Vector3(i==5?-1.65f:1.65f,.8f,i==5?-3:3))*bounds.extents.magnitude;
                camera.transform.LookAt(bounds.center); camera.orthographicSize=Mathf.Max(bounds.extents.y,bounds.extents.x)*1.34f+.12f;
                var floor=i==1?
                    WorldArt.Box("Neutral portrait floor",null,new Vector3(bounds.center.x,bounds.min.y-.025f,bounds.center.z),new Vector3(200,.05f,200),new Color(.74f,.78f,.84f),false):
                    WorldArt.Cylinder("Portrait plinth",null,new Vector3(bounds.center.x,bounds.min.y-.11f,bounds.center.z),new Vector3(bounds.size.x*1.2f+.5f,.1f,bounds.size.z*1.5f+.5f),new Color(.78f,.76f,.65f),false);
                if(!quackyOnly || i==1) Render(camera,"AssetPreviews/Models/"+names[i]+".png",1200,1200);
                if(quackyOnly && i==1) {
                    Directory.CreateDirectory("QA/QuackyViews");
                    var views=new[]{Vector3.forward,Vector3.left,Vector3.back};
                    var viewNames=new[]{"Front","Side","Back"};
                    for(int view=0;view<views.Length;view++) {
                        camera.transform.position=bounds.center+views[view]*5+Vector3.up*.4f; camera.transform.LookAt(bounds.center);
                        Render(camera,"QA/QuackyViews/"+viewNames[view]+".png",1200,1200);
                    }
                }
                UnityEngine.Object.DestroyImmediate(floor); model.gameObject.SetActive(false); models.Add(model);
            }
            for(int i=0;i<6;i++) {
                var model=models[i]; model.gameObject.SetActive(true);
                if(i==5) { model.localScale=Vector3.one*.29f; model.rotation=Quaternion.Euler(0,180,0); }
                var bounds=BoundsOf(model); model.position=new Vector3(-6.25f+i*2.5f,-bounds.min.y,0);
                WorldArt.Label(model,names[i].Replace("Aster-Dragon","ASTER").ToUpperInvariant(),Vector3.zero,.16f,WorldArt.Ink);
                var label=model.GetChild(model.childCount-1); label.SetParent(null,true); label.position=new Vector3(model.position.x,-.3f,0); label.rotation=Quaternion.Euler(0,180,0); label.localScale=Vector3.one;
            }
            lamp.transform.rotation=Quaternion.Euler(45,215,0);
            lamp.color=new Color(1,.94f,.8f); lamp.intensity=1.1f; camera.backgroundColor=new Color(.91f,.89f,.82f); RenderSettings.ambientLight=new Color(.69f,.72f,.71f);
            camera.transform.position=new Vector3(.3f,1.2f,20); camera.transform.LookAt(new Vector3(.3f,1.2f,0)); camera.orthographicSize=2.75f;
            Render(camera,"AssetPreviews/Unity-Character-Lineup.png",2400,800);
            EditorSceneManager.CloseScene(preview,true); SceneManager.SetActiveScene(original); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            if(!quackyOnly) File.WriteAllText("AssetPreviews/asset-manifest.txt","Unity storybook asset exports\n"+string.Join("\n",names)+"\n\n11 reusable prefabs and 11 rendered PNG portraits.\nCharacter concept and illustrated map were generated separately with the built-in image-generation tool.\n");
            Debug.Log(quackyOnly?"QUACKY_EXPORT_OK: Updated Quacky prefab, portrait, character lineup and three inspection views":"STORYBOOK_EXPORT_OK: 11 prefabs, 12 model images");
        }
        static Transform Create(int index)
        {
            if(index<4) return StorybookAssets.Robot(null,(RobotKind)index).transform;
            if(index==4) return StorybookAssets.Explorer(null);
            if(index==5) return StorybookAssets.Dragon(null,Vector3.zero);
            if(index==6) return StorybookAssets.Crate(null,Vector3.zero);
            if(index==7) return StorybookAssets.Crystal(null,Vector3.zero,WorldArt.Cyan);
            if(index==10) return StorybookEnvironment.MushroomHouse(null,Vector3.zero);
            var root=new GameObject(names[index]).transform;
            if(index==8) StorybookAssets.Lantern(root,Vector3.zero,1);
            if(index==9) StorybookEnvironment.Tree(root,Vector3.zero,1,new Color(.25f,.4f,.25f));
            return root;
        }
        static Bounds BoundsOf(Transform t)
        {
            var renderers=t.GetComponentsInChildren<Renderer>(); var bounds=renderers[0].bounds;
            foreach(var renderer in renderers) bounds.Encapsulate(renderer.bounds); return bounds;
        }
        static void Persist(Transform root,string name)
        {
            var cache=new Dictionary<Mesh,Mesh>(); int i=0;
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>()) {
                var mesh=filter.sharedMesh;
                if(!cache.TryGetValue(mesh,out var saved)) {
                    string path="Assets/Art/Models/"+name+"-"+(i++)+".asset";
                    saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if(saved) {
                        // Mesh setters also refresh GPU buffers when topology changes between exports.
                        saved.Clear(); saved.indexFormat=mesh.indexFormat; saved.vertices=mesh.vertices;
                        saved.normals=mesh.normals; saved.tangents=mesh.tangents; saved.uv=mesh.uv;
                        saved.subMeshCount=mesh.subMeshCount;
                        for(int sub=0;sub<mesh.subMeshCount;sub++) saved.SetIndices(mesh.GetIndices(sub),mesh.GetTopology(sub),sub);
                        saved.RecalculateBounds(); saved.name=Path.GetFileNameWithoutExtension(path); EditorUtility.SetDirty(saved);
                    } else { saved=UnityEngine.Object.Instantiate(mesh); saved.name=Path.GetFileNameWithoutExtension(path); AssetDatabase.CreateAsset(saved,path); }
                    cache[mesh]=saved;
                }
                filter.sharedMesh=saved;
                var renderer=filter.GetComponent<MeshRenderer>();
                var material=renderer.sharedMaterial;
                string matPath="Assets/Art/Materials/Paint-"+ColorUtility.ToHtmlStringRGBA(material.color)+"-"+ColorUtility.ToHtmlStringRGBA(material.GetColor("_EmissionColor"))+".mat";
                var persistent=AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if(!persistent) { persistent=new Material(material); persistent.name=Path.GetFileNameWithoutExtension(matPath); AssetDatabase.CreateAsset(persistent,matPath); } else { EditorUtility.CopySerialized(material,persistent); persistent.name=Path.GetFileNameWithoutExtension(matPath); }
                renderer.sharedMaterial=persistent;
            }
        }
        public static void Render(Camera camera,string path,int width,int height)
        {
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32); rt.antiAliasing=4; camera.targetTexture=rt; camera.aspect=(float)width/height; camera.Render();
            var previous=RenderTexture.active; RenderTexture.active=rt; var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,width,height),0,0); texture.Apply(); File.WriteAllBytes(path,texture.EncodeToPNG()); RenderTexture.active=previous; camera.targetTexture=null;
            UnityEngine.Object.DestroyImmediate(texture); rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
        }
        [MenuItem("Adventure Quest/Export Reusable Storybook Asset Package")]
        public static void ExportAssetPackage()
        {
            Directory.CreateDirectory("Release");
            AssetDatabase.ExportPackage(new[]{"Assets/Art","Assets/Scripts/StorybookMotion.cs","Assets/Scripts/FunobotzMotion.cs","Assets/Resources/Storybook/ExpeditionMap.png"},
                "Release/Dragon-Valley-Storybook-Assets.unitypackage",ExportPackageOptions.Recurse|ExportPackageOptions.IncludeDependencies);
            Debug.Log("STORYBOOK_PACKAGE_OK: Release/Dragon-Valley-Storybook-Assets.unitypackage");
        }
        public static void ExportAndBuild() { Export(); AdventureBuilder.Build(); ExportAssetPackage(); }
        public static void ExportQuackyAndBuild() { ExportQuacky(); AdventureBuilder.Build(); ExportAssetPackage(); }
        public static void ExportQuackyAssets() { ExportQuacky(); ExportAssetPackage(); }
    }
}
