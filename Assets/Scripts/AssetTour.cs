using System.Collections;
using System.IO;
using UnityEngine;
namespace DragonValley
{
    public class AssetTour : MonoBehaviour
    {
        IEnumerator Start()
        {
            while(!UnityEngine.Rendering.SplashScreen.isFinished) yield return null;
            var g=AdventureGame.Instance; g.StartAdventure(false); g.CloseTutorial(); yield return null;
            g.HUD.enabled=false; g.Player.CameraRig.enabled=false; g.Player.gameObject.SetActive(false);
            foreach(var robot in g.Robots.Members) robot.gameObject.SetActive(false);
            foreach(var gate in g.World.Gates) gate.gameObject.SetActive(false);
            foreach(var label in g.World.GetComponentsInChildren<WorldLabel>()) label.gameObject.SetActive(false);
            string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../AssetPreviews/Regions")); Directory.CreateDirectory(folder);
            for(int i=0;i<7;i++) {
                Vector3 center=new Vector3(0,1.3f,i*AdventureWorld.ZoneLength+1);
                g.Camera.transform.position=center+new Vector3(15,i==4?5:16,-22); g.Camera.transform.LookAt(center+Vector3.forward*4); g.Camera.fieldOfView=61;
                if(i==4) { g.Camera.transform.position=center+new Vector3(0,4,-16); g.Camera.transform.LookAt(center+Vector3.forward*6); }
                if(i==6) { g.Camera.transform.position=center+new Vector3(0,12,-22); g.Camera.transform.LookAt(center+Vector3.forward*6); }
                yield return new WaitForSeconds(.25f);
                Capture(g.Camera,Path.Combine(folder,(i+1)+"-"+g.Missions.Missions[i].zone.Replace(' ','-')+".png"),1920,1080);
            }
            g.Camera.orthographic=true; g.Camera.orthographicSize=163; g.Camera.farClipPlane=550;
            RenderSettings.fog=false;
            g.Camera.transform.position=new Vector3(24,230,132); g.Camera.transform.LookAt(new Vector3(0,0,132),Vector3.forward);
            Capture(g.Camera,Path.GetFullPath(Path.Combine(folder,"../Unity-World-Overview.png")),1400,3600);
            File.WriteAllText(Path.Combine(folder,"export-result.txt"),"EXPORTED: Seven actual Unity region views and the world overview.");
            Application.Quit();
        }
        static void Capture(Camera camera,string path,int width,int height)
        {
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32); rt.antiAliasing=2; camera.targetTexture=rt; camera.aspect=(float)width/height; camera.Render();
            var previous=RenderTexture.active; RenderTexture.active=rt; var texture=new Texture2D(width,height,TextureFormat.RGB24,false); texture.ReadPixels(new Rect(0,0,width,height),0,0); texture.Apply(); File.WriteAllBytes(path,texture.EncodeToPNG());
            RenderTexture.active=previous; camera.targetTexture=null; Destroy(texture); rt.Release(); Destroy(rt);
        }
    }
}
