using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    public static Player Local { get; set; }


    private NetworkCharacterControllerPrototype _cc;

    [SerializeField]
    private Ball _prefabBall;
    private Vector3 _forward;

    bool isJumpPressed = false;
    Vector2 viewInput = Vector2.zero;


    [Networked]
    private TickTimer delay { get; set; }

    [SerializeField]
    private PhysxBall _prefabPhysxBall;


    private Material _material;
    private Text _message;
    Material material
    {
        get { 
            if (_material == null)
                _material = GetComponentInChildren<MeshRenderer>().material;
            return _material;
        }
    }

    [Networked(OnChanged = nameof(OnBallSpawned))]
    public NetworkBool spawned { get; set; }


    //rotation
    Transform cameraAnchor;
    Camera playerCam;
    float cameraRotationX = 0;
    float cameraRotationY = 0;




    public static void OnBallSpawned(Changed<Player> changed)
    {
        changed.Behaviour.material.color = Color.white;
    }

    public void Awake()
    {
        _cc = GetComponent<NetworkCharacterControllerPrototype>();
        _forward = transform.forward;
        playerCam = GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    public override void FixedUpdateNetwork() 
    {
        if(GetInput(out NetworkInputData data))
        {
            //data.direction.Normalize();
            //_cc.Move(5 * data.direction * Runner.DeltaTime);

            //Debug.Log(data.direction);

            //if (data.direction.sqrMagnitude > 0)
            //{
            //    //_forward = data.direction;
            //    transform.forward = _forward;
            //}


            //sec att
            Vector3 moveDirection = transform.forward * data.movementInput.y + transform.right * data.movementInput.x;
            moveDirection.Normalize();

            _cc.Move(moveDirection * Runner.DeltaTime);

            cameraRotationX += data.lookVector.y * Time.deltaTime * _cc.ViewUpDowanRotationSpeed;
            cameraRotationX = Mathf.Clamp(cameraRotationX, -90, 90);

            cameraRotationY += data.lookVector.x * Time.deltaTime * _cc.rotationSpeed;

            //playerCam.transform.rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0);
            

            _cc.Rotate(data.rotationInput);

            _forward = transform.forward;
            playerCam.transform.rotation = Quaternion.Euler(cameraRotationX, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

            Debug.Log($"camRotX: {cameraRotationX}");
            Debug.Log($"camRotY: {cameraRotationY}");
            Debug.Log($"eulerAnglesY: {transform.rotation.eulerAngles.y}");



            if (delay.ExpiredOrNotRunning(Runner))
            {
                if ((data.buttons & NetworkInputData.MOUSEBUTTON1) != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabBall, transform.position + _forward, playerCam.transform.rotation, Object.InputAuthority, 
                        (runner, o) => { o.GetComponent<Ball>().Init(); });
                    spawned = !spawned;

                }
                else if((data.buttons & NetworkInputData.MOUSEBUTTON2) != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabPhysxBall, transform.position + _forward, playerCam.transform.rotation, Object.InputAuthority,
                        (runner, o) => { o.GetComponent<PhysxBall>().Init(10 * _forward); });
                    spawned = !spawned;
                }
            }

            

            if (data.isJumping)
                _cc.Jump();
        }
    }

    public override void Render()
    {
        material.color = Color.Lerp(material.color, Color.blue, Time.deltaTime);
    }


    private void Update()
    {
        if(Object.HasInputAuthority && Input.GetKeyDown(KeyCode.K))
        {
            RPC_SendMessage("Hey Mate!");
        }




    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        if (_message == null)
            _message = FindObjectOfType<Text>();
        if (info.IsInvokeLocal)
        {
            message = $"You said: {message}\n";
            
        }
        else
        {
            message = $"Some other player said {message} \n";
        }
        _message.text += message;
    }



}
