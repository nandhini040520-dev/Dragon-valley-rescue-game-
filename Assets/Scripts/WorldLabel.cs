using UnityEngine;
namespace DragonValley
{
    public class WorldLabel : MonoBehaviour
    {
        Renderer textRenderer;
        InteractableBase task;
        Vector3 authoredScale;
        void Awake() { textRenderer=GetComponent<Renderer>(); task=GetComponentInParent<InteractableBase>(); authoredScale=transform.localScale; }
        void LateUpdate()
        {
            var g=AdventureGame.Instance; if(!g || !g.Camera) return;
            bool show=g.IsPlaying && Vector3.Distance(transform.position,g.Player.transform.position)<15;
            if(task) show &= task.Relevant && (g.Missions.IsCurrent(task.objectiveId) || (task.action==ActionKind.UnsafePlace && g.Interactions.Carried!=null));
            textRenderer.enabled=show;
            if(show) {
                transform.rotation=g.Camera.transform.rotation;
                // Preserve authored proportions, but prevent labels filling the view up close.
                float cameraDistance=Vector3.Distance(transform.position,g.Camera.transform.position);
                transform.localScale=authoredScale*Mathf.Clamp(cameraDistance/7f,.25f,1f);
                var viewport=g.Camera.WorldToViewportPoint(transform.position);
                textRenderer.enabled=viewport.z>g.Camera.nearClipPlane && viewport.x>.06f && viewport.x<.94f && viewport.y>.16f && viewport.y<.78f;
            }
        }
    }
}
