using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    private NetworkCharacterControllerPrototype _cc;

    [SerializeField]
    private Ball _prefabBall;
    private Vector3 _forward;

    [Networked]
    private TickTimer delay { get; set; }

    [SerializeField]
    private PhysxBall _prefabPhysxBall;


    private Material _mateial;
    private Text _message;
    Material material
    {
        get { 
            if (_mateial == null)
                _mateial = GetComponentInChildren<MeshRenderer>().material;
            return _mateial;
        }
    }

    [Networked(OnChanged = nameof(OnBallSpawned))]
    public NetworkBool spawned { get; set; }



    public static void OnBallSpawned(Changed<Player> changed)
    {
        changed.Behaviour.material.color = Color.white;
    }

    public void Awake()
    {
        _cc = GetComponent<NetworkCharacterControllerPrototype>();
        _forward = transform.forward;
    }

    public override void FixedUpdateNetwork() 
    {
        if(GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            _cc.Move(5 * data.direction * Runner.DeltaTime);

            if (data.direction.sqrMagnitude > 0)
                _forward = data.direction;


            if (delay.ExpiredOrNotRunning(Runner))
            {
                if ((data.buttons & NetworkInputData.MOUSEBUTTON1) != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabBall, transform.position + _forward, Quaternion.LookRotation(_forward), Object.InputAuthority, 
                        (runner, o) => { o.GetComponent<Ball>().Init(); });
                    spawned = !spawned;

                }
                else if((data.buttons & NetworkInputData.MOUSEBUTTON2) != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabPhysxBall, transform.position + _forward, Quaternion.LookRotation(_forward), Object.InputAuthority,
                        (runner, o) => { o.GetComponent<PhysxBall>().Init(10 * _forward); });
                    spawned = !spawned;
                }
            }


            
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
