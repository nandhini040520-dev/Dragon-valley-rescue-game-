using UnityEngine;
namespace DragonValley
{
    [RequireComponent(typeof(CharacterController))]
    public class ExplorerController : MonoBehaviour
    {
        [SerializeField] float speed=6.5f, jumpHeight=1.4f;
        public Transform Visual;
        public ThirdPersonCamera CameraRig;
        CharacterController motor;
        float vertical;
        public bool Grounded => motor && motor.isGrounded;
        void Awake() { motor=GetComponent<CharacterController>(); motor.height=1.8f; motor.radius=.36f; motor.center=Vector3.up*.9f; motor.stepOffset=.35f; }
        void Update()
        {
            var g=AdventureGame.Instance;
            if (!g || !g.IsPlaying || AdventureGame.Autopilot) return;
            Tick(new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")), Input.GetKeyDown(KeyCode.Space),Time.deltaTime);
        }
        public void Tick(Vector2 input,bool jump,float dt)
        {
            var forward=CameraRig ? CameraRig.transform.forward : Vector3.forward; forward.y=0; forward.Normalize();
            var right=new Vector3(forward.z,0,-forward.x);
            var direction=Vector3.ClampMagnitude(right*input.x+forward*input.y,1);
            if (motor.isGrounded && vertical<0) vertical=-2;
            if (jump && motor.isGrounded) vertical=Mathf.Sqrt(jumpHeight*40);
            vertical-=20*dt;
            motor.Move((direction*speed+Vector3.up*vertical)*dt);
            if(direction.sqrMagnitude>.01f && Visual) Visual.rotation=Quaternion.Slerp(Visual.rotation,Quaternion.LookRotation(direction),dt*12);
            if(transform.position.y < -12) { Teleport(AdventureGame.Instance.World.Spawn(AdventureGame.Instance.Missions.Region)); AdventureGame.Instance.Notify("Back on the path","Your expedition continues from this region's checkpoint."); }
        }
        public void Teleport(Vector3 point) { motor.enabled=false; transform.position=point; vertical=0; motor.enabled=true; }
    }
}
