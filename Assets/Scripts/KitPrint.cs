using UnityEngine;

namespace DragonValley
{
    // Keep card lettering on the card surface, including after a dynamic font atlas rebuild.
    [ExecuteAlways, RequireComponent(typeof(TextMesh))]
    public sealed class KitPrint : MonoBehaviour
    {
        [SerializeField] Font printFont;
        [SerializeField] Shader printShader;
        Material material;

        public void Configure(Font font,Shader shader)
        {
            printFont=font; printShader=shader; Refresh(font);
        }

        void OnEnable() { Font.textureRebuilt+=Refresh; Refresh(printFont); }
        void OnDisable() { Font.textureRebuilt-=Refresh; }

        void Refresh(Font changedFont)
        {
            if(!printFont || !printShader || changedFont!=printFont) return;
            if(!material) material=new Material(printShader) { name="Depth-tested kit lettering", hideFlags=HideFlags.DontSave };
            material.mainTexture=printFont.material.mainTexture;
            GetComponent<MeshRenderer>().sharedMaterial=material;
        }

        void OnDestroy()
        {
            if(!material) return;
            if(Application.isPlaying) Destroy(material); else DestroyImmediate(material);
        }
    }
}
