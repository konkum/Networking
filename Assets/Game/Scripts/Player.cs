using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;
using EasyCharacterMovement;
using UnityEngine.Pool;
using UnityEngine.Animations.Rigging;

public class Player : NetworkBehaviour
{
    private readonly int Shoot = Animator.StringToHash("Shooting");
    private readonly int Idle = Animator.StringToHash("Idle");
    private readonly int Walk = Animator.StringToHash("Walking");
    private readonly int Gunplay = Animator.StringToHash("Gunplay");

    [SerializeField] private GameObject aimConstraint;
    [SerializeField] private RigBuilder rigBuilder;
    [SerializeField] private Ball prefabBall;
    [SerializeField] private Animator playerAnim;
    [SerializeField] private LayerMask shootingLayer;

    [Networked] private TickTimer delay { get; set; }
    private ObjectPool<Ball> _ballPool;
    private CharacterMovement _cc;
    [Networked] private Vector3 _foward { get; set; }
    [Networked] private Vector3 _hitPosition { get; set; }
    [Networked] private NetworkBool _isFire { get; set; }
    //[Networked] private Ray _shootingRay { get; set; }

    //Shooting
    private bool _isDoneFire = false;

    private float _attactSpeed = 1f;

    //Animation
    private float _lockedTill;

    private int _currentState;
    private float _shootingAnimTime = 0.5f;
    //

    public static Player Local { get; private set; }
    public ObjectPool<Ball> BallPool => _ballPool;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        _cc = GetComponent<CharacterMovement>();

        _ballPool = new ObjectPool<Ball>(SpawnBall, GetBall, ReturnBall);
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
            CameraController.Instance.FreeLook.Follow = Local.transform.GetChild(0);
            CameraController.Instance.FreeLook.LookAt = Local.transform.GetChild(0);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();

            _cc.Move(data.direction * 3, Runner.DeltaTime);
            this.transform.rotation = Quaternion.LookRotation(data.camDir);

            if (data.direction.sqrMagnitude > 0)
            {
                _foward = data.direction;
            }
            else
            {
                _foward = Vector3.zero;
            }
            if (delay.ExpiredOrNotRunning(Runner))
            {
                if ((data.button_0 & NetworkInputData.MOUSEBUTTON0) != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    if (Physics.Raycast(data.firePoint, data.attactDirection, out var hit, 20, shootingLayer))
                    {
                        //Debug.Log(hit.point);
                        _isFire = true;
                        _hitPosition = hit.point;
                    }
                    else
                    {
                        _isFire = true;
                        _hitPosition = data.attactDirection.normalized * 20;
                    }
                }
                else
                {
                    _isFire = false;
                }
            }
            aimConstraint.transform.position = data.attactDirection.normalized * 20;
        }
    }

    public override void Render()
    {
        if (_isFire)
        {
            _isDoneFire = _isFire;
            _attactSpeed = 0f;
        }
        _attactSpeed += Time.deltaTime;
        var state = GetUpperState(_foward, _isDoneFire);
        playerAnim.CrossFade(GetLowerState(_foward), 0, 0);

        if (state != _currentState)
        {
            playerAnim.CrossFade(state, 0.03f, playerAnim.GetLayerIndex("Upper Layer"));
            _currentState = state;
            if (_currentState == Gunplay)
            {
                rigBuilder.layers[1].active = true;
                rigBuilder.layers[2].active = true;
            }
            else
            {
                rigBuilder.layers[1].active = false;
                rigBuilder.layers[2].active = false;
            }
        }

        if (_attactSpeed > 0.05f)
        {
            if (_isDoneFire)
            {
                Debug.Log("Run");
                var ball = _ballPool.Get();
                ball.Destination = _hitPosition;
                _isDoneFire = false;
            }
        }
    }

    private Ball SpawnBall()
    {
        var ball = Instantiate(prefabBall, this.transform.position, Quaternion.identity);
        ball.Pool = _ballPool;
        return ball;
    }

    private void GetBall(Ball ball)
    {
        ball.gameObject.SetActive(true);
        ball.transform.position = this.transform.position;
        ball.transform.rotation = Quaternion.LookRotation(this.transform.forward);
    }

    private void ReturnBall(Ball ball)
    {
        ball.gameObject.SetActive(false);
    }

    private int GetLowerState(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0)
        {
            return Walk;
        }
        else
        {
            return Idle;
        }
    }

    private int GetUpperState(Vector3 direction, bool isFire = false)
    {
        int LockState(int s, float t)
        {
            _lockedTill = Time.time + t;
            return s;
        }
        if (Time.time < _lockedTill) return _currentState;
        if (isFire)
        {
            return LockState(Gunplay, _shootingAnimTime);
        }
        if (direction.sqrMagnitude > 0)
        {
            return Walk;
        }
        else
        {
            return Idle;
        }
    }

    private bool IsAnimationRunning(int animationLayer, string stateName)
    {
        if (playerAnim.GetCurrentAnimatorStateInfo(animationLayer).IsName(stateName) && playerAnim.GetCurrentAnimatorStateInfo(animationLayer).normalizedTime < 1.0f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}