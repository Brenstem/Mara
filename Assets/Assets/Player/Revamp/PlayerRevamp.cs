using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

// alert kan man gå in i när man nyligen har tagit skada inom en viss tid samt när fienden har aggro på en och man är skadad eller whatever
public class PlayerRevamp : Entity
{
    #region Inspector
    /* === DEBUG === */
    //[Header("Debug")]
    //sköts av global state nu
    //[SerializeField] private bool _lockCursor = true;
    public bool EnabledControls
    {
        get
        {
            return _playerInput.PlayerControls.enabled;
        }
        set
        {
            if (value)
            {
                _playerInput.PlayerControls.Enable();
            }
            else
            {
                _playerInput.PlayerControls.Disable();
            }
        }
    }



    /* === INSPECTOR VARIABLES === */
    [Header("References and logic")]
    public Animator playerAnimator;
    public Animator cameraAnimator;
    [SerializeField] private Transform _groundCheckPosition;
    [SerializeField] private Cinemachine.CinemachineFreeLook _freeLookCam;
    [SerializeField] private Cinemachine.CinemachineFreeLook _lockonCam;
    [SerializeField] private TargetFinder _targetFinder;
    [SerializeField, Range(1, 50)] private int inputBufferSize = 1;

    [Header("Ground Check")]
    [SerializeField] private float _groundDistance = 0.4f;
    [SerializeField] private float _airBorneUntilFallingTime = 0.25f;
    private Vector3 _velocity;
    private bool _isGrounded;
    public bool IsGrounded { get { return _isGrounded; } }

    [Header("Movement"), Space(20)]
    private float _originalMaxSpeed;
    public float maxSpeed = 5f;
    public float rotationSpeed = 15f;
    public float rotationAngleUntilMove = 30;
    public float timeUntilIdle = 5f;

    [Header("Jump")]
    [SerializeField] private float _gravity = -9.82f;
    [SerializeField] private float _jumpHeight = 3f;
    public float airRotationSpeed = 4f;
    public float airSpeed = 5f;

    [Header("Dash Properties")]
    [SerializeField] private float _dashCooldownTime = 0.25f;
    [SerializeField] private int airDashAmount;
    public float dashTime = 0.25f;
    public float dashLag = 0.15f;
    public float dashSpeed = 10.0f;
    public float dashAnimationNumerator = 5;
    public Timer dashCooldownTimer;
    private int _airDashes;
    public bool invulerableWhenDashing;

    [Header("Combat")]
    public float hitstunImmunityTime;
    public float heavyHitstunThreshold = 1.0f;
    public float parryDuration;
    public float parryLag; // borde vara tied till animationen, samt tror inte att cooldown behövs med tanke på att det ska vara få active frames och mycket lag

    [Header("Light Attack 1")]
    public HitboxGroup light1HitboxGroup;
    public float light1StepSpeed;

    [Header("Light Attack 2")]
    public HitboxGroup light2HitboxGroup;
    public float light2StepSpeed;

    [Header("Heavy Attack")]
    public HitboxGroup heavyHitboxGroup;
    public float heavyStepSpeed;
    public float heavyChargeTime = 2.0f;
    public float heavyMaxDamageMultiplier = 1.0f;
    public float heavyMaxHitstopMultiplier = 1.0f;

    [Header("Action Attack")]
    public HitboxGroup actionHitboxGroup;
    public bool actionAttackActive;

    [Header("Insanity events")]
    [SerializeField] float _moveSpeedBuffMultiplier = 1.1f;
    [SerializeField] float _moveSpeedDebuffMultiplier = 0.9f;

    #endregion

    /* === COMPONENT REFERENCES === */
    private PlayerInput _playerInput;

    [HideInInspector] public Transform pointOfInterest;
    [HideInInspector] public StateMachine<PlayerRevamp> stateMachine;
    [HideInInspector] public CharacterController controller;


    /* === INPUT === */
    private Vector2 _input;
    public Vector2 Input { get { return _input; } }
    [HideInInspector] public Collider[] groundHits;
    [HideInInspector] private float _airBorneTimer;

    [HideInInspector] public Vector3 _currentDirection;
    [HideInInspector] public Vector3 CurrentDirection { get { return _currentDirection; } }
    [HideInInspector] public float CurrentSpeed { get { return maxSpeed; } }

    public enum InputType
    {
        Idle = 0,
        Dash = 1,
        Jump = 2,
        AttackLight = 3,
        AttackHeavy = 4,
        AttackHeavyReleased = 5,
        Parry = 6,
    }

    [HideInInspector] public CircularBuffer<InputType> inputBuffer;
    [HideInInspector] public bool dashPerformed;
    [HideInInspector] public bool jumpPerformed;
    [HideInInspector] public bool useGravity = true;

    /* === LOCK ON === */
    private bool _doSnapCamera;
    private bool _lockedOn;

    public bool IsLockedOn { get { return _lockedOn; } }

    private void OnEnable() { EnabledControls = true; }
    private void OnDisable() { EnabledControls = false; }

    /* === COMBAT === */
    private Timer _hitstunImmunityTimer;
    [HideInInspector] public bool interruptable;
    [HideInInspector] public bool attackAnimationOver;
    [HideInInspector] public bool attackStep;
    [HideInInspector] public float hitstunDuration;
    [HideInInspector] public bool isParrying;
    [HideInInspector] public bool walkCancel;

    private void LoadData()
    {
        /* OptionData d = (OptionData)SaveData.Load_Data("controls");
        if (d != null)
        {
            GlobalState.state.language = (GlobalState.LanguageEnum)d.currentLanguage;
            Screen.SetResolution(d.width, d.height, d.isFullscreen);
            QualitySettings.SetQualityLevel(d.qualityLevel);

            if (d.controlKeyArray.Length == 0)
            {
                Dictionary<System.Guid, string> ovr = new Dictionary<System.Guid, string>();
                int i = 0;
                foreach (byte[] key in d.controlKeyArray)
                {
                    ovr.Add(new System.Guid(key), d.valueArray[i]);
                    i++;
                }
                MenuInputResource.LoadOverrides(ref _playerInput, ovr);
            }
        }
    }

    private Timer _alertTimer;
    private bool _isAlerted;
    private float _alertTime;
    private float _alertValue;
    [SerializeField] private float _alertBlendDuration = 0.25f;

    public bool IsAlert
    {
        get { return _isAlerted; }
        set
        {
            if (!value)
            {
                _alertValue = playerAnimator.GetFloat("Alert");
                _alertTime = 0;
            }
            else
            {
                playerAnimator.SetFloat("Alert", 1.0f);
            }
            _alertTimer.Reset();
            _isAlerted = value;
        }
    }

    /* === Unity Functions === */
    private new void Awake()
    {
        base.Awake();

        _alertTimer = new Timer(timeUntilIdle);
        IsAlert = false;
        //sköts av global state nu
        //if (_lockCursor)
        //    Cursor.lockState = CursorLockMode.Locked;

        _originalMaxSpeed = maxSpeed;

        controller = GetComponent<CharacterController>();
        _playerInput = new PlayerInput();
        stateMachine = new StateMachine<PlayerRevamp>(this);
        stateMachine.ChangeState(new IdleState());

        LoadData();

        inputBuffer = new CircularBuffer<InputType>(inputBufferSize);

        // Dash
        dashCooldownTimer = new Timer(_dashCooldownTime);
        dashCooldownTimer.Time += _dashCooldownTime;

        // Hitstun
        _hitstunImmunityTimer = new Timer(hitstunImmunityTime);

        /* === INPUT === */
        _playerInput.PlayerControls.Move.performed += performedInput => _input = performedInput.ReadValue<Vector2>();
        _playerInput.PlayerControls.Dash.performed += performedInput => Action(InputType.Dash);
        _playerInput.PlayerControls.Jump.performed += performedInput => Action(InputType.Jump);
        _playerInput.PlayerControls.AttackLight.performed += performedInput => Action(InputType.AttackLight);
        _playerInput.PlayerControls.AttackHeavy.started += performedInput => Action(InputType.AttackHeavy);
        _playerInput.PlayerControls.AttackHeavy.canceled += performedInput => Action(InputType.AttackHeavyReleased);

        _playerInput.PlayerControls.Parry.performed += performedInput => Action(InputType.Parry);

        /* === PLAYER ANIM EVENTS === */
        PlayerAnimationEventHandler.onAnimationOver += AnimationOver;
        PlayerAnimationEventHandler.onIASA += IASA;
        PlayerAnimationEventHandler.onWalkCancel += WalkCancel;
        PlayerAnimationEventHandler.onAttackStep += AttackStep;
        PlayerAnimationEventHandler.onAttackStepEnd += AttackStepEnd;

        /* === MODIFIER EVENTS === */
        modifier.AttackSpeedMultiplier.onModified += UpdateAttackSpeed;
        modifier.MovementSpeedMultiplier.onModified += UpdateMoveSpeed;

    }

    private void Start()
    {
        //anänds inte när vi har _lockCursorOnStart boolen i global state men när vi tar bort den ska denna rad vara med
        //EnabledControls = false;
    }

    private void Update()
    {
        stateMachine.Update();
        playerAnimator.SetFloat("StrafeDirX", Input.x);
        playerAnimator.SetFloat("StrafeDirY", Input.y);

        //print(stateMachine.currentState);

        Gravity();

        HitstunImmunity();

        SnapCamera();

        dashCooldownTimer += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        GroundCheck();

        Action(InputType.Idle);

        AlertAnimation();
    }

    private void AlertAnimation()
    {
        if (!_isAlerted)
        {
            if (_alertTimer.Expired)
            {
                _alertTime += Time.deltaTime;
                playerAnimator.SetFloat("Alert", Mathf.Lerp(_alertValue, 0, _alertTime / (_alertBlendDuration * _alertValue)));
            }
            else
            {
                _alertTimer += Time.deltaTime;
            }
        }
    }

    /* === INPUT === */
    private void AttackStep() { attackStep = true; }
    private void AttackStepEnd() { attackStep = false; }
    private void IASA() { interruptable = true; }
    private void WalkCancel() { walkCancel = true; }
    private void AnimationOver() { attackAnimationOver = true; }
    private void Action(InputType type) { inputBuffer.Enqueue(type); }

    #region Public Functions
    private bool _hitstunImmunity;
    [HideInInspector] public bool successfulParry;
    public override void TakeDamage(HitboxValues hitbox, Entity attacker = null)
    {
        if (isParrying && hitbox.parryable)
        {
            successfulParry = true;

            if (attacker != null)
                attacker.Parried();

            // parrya alla fiender inom en viss radie
        }
        else
        {
            GlobalState.state.AudioManager.RangedEnemyMeleeAttackHitAudio(this.transform.position);

            if (!_hitstunImmunity)
            {
                hitstunDuration = hitbox.hitstunTime;
                stateMachine.ChangeState(new HitstunState());
                health.Damage(hitbox);
                _hitstunImmunity = true;
                if (actionAttackActive)
                {
                    actionHitboxGroup.enabled = true;
                }
            }
        }
    }

    public override void Parried()
    {
        Debug.LogWarning("Parried implementation missing", this);
    }

    public void FaceDirection(Transform target)
    {
        if (target != null)
        {
            Vector3 _direction = (target.position - transform.position);
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(_direction.x, 0, _direction.z));
            transform.rotation = lookRotation;
        }
    }

    public GameObject FindTarget()
    {
        GameObject _target = _targetFinder.FindTarget(); // Find target to pass to attack function in first frame of attack
        if (_target != null)
        {
            var t = GlobalState.state.Player.gameObject.GetComponent<LockonFunctionality>().Target;

            if (t != null)
            {
                if (Vector3.Distance(transform.position, t.position) <= _targetFinder.trackRadius)
                    _target = t.gameObject;
            }
        }
        return _target;
    }

    public override void KillThis()
    {
        stateMachine.ChangeState(new PlayerDeathState());
        playerAnimator.SetBool("Dead", true);
    }


    public void IncreaseMoveSpeedOverValue(float insValueModStart, float insValueModEnd, float multiplier)
    {
        float currentInsanity = ((PlayerInsanity)health).CurrentHealth;

        float range = insValueModEnd - insValueModStart;

        currentInsanity -= insValueModStart;

        if (currentInsanity > 0 || Modifier.NearlyEquals(currentInsanity, 0))
        {
            float interpolationValue = Mathf.Clamp(currentInsanity / range, 0, 1);

            float finalMultiplier = Mathf.Lerp(1.0f, multiplier, interpolationValue);

            maxSpeed = _originalMaxSpeed;

            modifier.MovementSpeedMultiplier *= finalMultiplier;

            maxSpeed *= modifier.MovementSpeedMultiplier;
        }
        else if (currentInsanity < 0)
        {
            modifier.MovementSpeedMultiplier.Reset();
            maxSpeed *= modifier.MovementSpeedMultiplier;
        }
    }


    public void Dash()
    {
        if (dashCooldownTimer.Expired && _airDashes < airDashAmount)
        {
            _airDashes++;
            //GlobalState.state.Player.ResetAnim();
            stateMachine.ChangeState(new DashingState());
        }
    }

    public void Jump()
    {
        if (IsGrounded && !playerAnimator.GetBool("HasJumped"))
        {
            _velocity.y = Mathf.Sqrt(_jumpHeight) * -_gravity;
            playerAnimator.SetBool("HasJumped", true);
            playerAnimator.SetTrigger("Jump");
            inputBuffer.Clear();
            GlobalState.state.AudioManager.PlayerJumpAudio(this.transform.position);

            StartCoroutine(ActionAttackForOneFrame());
        }
    }
    #endregion

    private IEnumerator ActionAttackForOneFrame()
    {
        if (actionAttackActive)
        {
            actionHitboxGroup.enabled = true;
            yield return new WaitForFixedUpdate();
            actionHitboxGroup.enabled = false;
        }
    }


    #region Private Functions
    private void UpdateMoveSpeed()
    {
        maxSpeed = _originalMaxSpeed;
        maxSpeed *= modifier.MovementSpeedMultiplier;
    }

    private void UpdateAttackSpeed()
    {
        playerAnimator.SetFloat("AttackSpeedModifier", 1.0f * modifier.AttackSpeedMultiplier);
    }

    private void Gravity()
    {
        if (useGravity)
        {
            _velocity.y += _gravity * Time.deltaTime; //Gravity formula
        }
        else
        {
            _velocity.y = 0;
        }

        if (_velocity != Vector3.zero)
        {
            controller.Move(_velocity * Time.deltaTime); // T^2
        }
    }

    private bool _playedLandingSound;
    private void GroundCheck()
    {
        groundHits = Physics.OverlapSphere(_groundCheckPosition.position, _groundDistance, GlobalState.state.GroundMask);
        _isGrounded = groundHits.Length > 0 ? true : false;

        _airBorneTimer += Time.deltaTime;

        if (_isGrounded)
        {
            if (_velocity.y < 0)
                _velocity.y = -2f;
            _airBorneTimer = 0;
            _airDashes = 0;

            playerAnimator.SetBool("HasJumped", false);

            if (playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Landing"))
            {
                if (!_playedLandingSound)
                {
                    GlobalState.state.AudioManager.PlayerLandAudio(_groundCheckPosition.position);
                    _playedLandingSound = true;
                    //StartCoroutine(ActionAttackForOneFrame());
                }
            }
            else
            {
                _playedLandingSound = false;
            }
        }

        if (_airBorneTimer > _airBorneUntilFallingTime && !_isGrounded)
            playerAnimator.SetBool("IsGrounded", false);
        else
            playerAnimator.SetBool("IsGrounded", true);
    }

    private void IncreaseMoveSpeed()
    {
        float currentInsanity = ((PlayerInsanity)health).GetInsanityPercentage();
        currentInsanity -= 75; // Slow starts at 75 insanity or higher
        if (currentInsanity > 0)
        {
            maxSpeed = _originalMaxSpeed;
            if (currentInsanity >= 10)
            {
                maxSpeed *= _moveSpeedBuffMultiplier;
            }
            else
            {
                maxSpeed *= 1 + currentInsanity / 100;
            }
        }
    }

    private void Slow()
    {
        float currentInsanity = ((PlayerInsanity)health).GetInsanityPercentage();
        if (currentInsanity - 50 > 0 && currentInsanity - 75 <= 0)
        {
            currentInsanity -= 50; // Slow starts at 50 insanity or higher
            maxSpeed = _originalMaxSpeed;
            if (currentInsanity >= 10)
            {
                maxSpeed *= _moveSpeedDebuffMultiplier;
            }
            else
            {
                maxSpeed *= 1 - currentInsanity / 100;
            }
        }
    }

    private void SnapCamera()
    {
        if (_doSnapCamera)
        {
            //var free = new Vector2(_freeLookCam.m_XAxis.Value, _freeLookCam.m_YAxis.Value);
            //var loc = new Vector2(_lockonCam.m_XAxis.Value, _lockonCam.m_YAxis.Value);
            _freeLookCam.m_XAxis.Value = transform.eulerAngles.y;
            _doSnapCamera = false;
        }
    }

    private void HitstunImmunity()
    {
        if (_hitstunImmunity)
        {
            _hitstunImmunityTimer += Time.deltaTime;
            if (_hitstunImmunityTimer.Expired)
            {
                _hitstunImmunityTimer.Reset();
                _hitstunImmunity = false;
            }
        }
    }

    public void EnableLockon()
    {
        _lockedOn = true;
        _doSnapCamera = true;
        _lockonCam.LookAt = transform;
        cameraAnimator.SetBool("LockedOn", _lockedOn);
        playerAnimator.SetBool("LockedOn", _lockedOn);
        _lockonCam.LookAt = pointOfInterest;
    }

    public void DisableLockon()
    {
        _lockedOn = false;
        _doSnapCamera = true;
        cameraAnimator.SetBool("LockedOn", _lockedOn);
        playerAnimator.SetBool("LockedOn", _lockedOn);
    }

    public void ToggleLockon()
    {

        if (!pointOfInterest)
        {
            Debug.LogWarning("Trying to toggle lockon without a point of interest!", this);
        }
        else
        {
            _lockedOn = !_lockedOn;
            cameraAnimator.SetBool("LockedOn", IsLockedOn);
            playerAnimator.SetBool("LockedOn", IsLockedOn);

            if (IsLockedOn)
            {
                _lockonCam.LookAt = pointOfInterest;

                FaceDirection(pointOfInterest);
            }
            else
            {
                _doSnapCamera = true;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_groundCheckPosition.position, _groundDistance);
    }

    #endregion 
}

public class IdleState : State<PlayerRevamp>
{
    private float time = 0;
    private float blend;

    public float idleBlendDuration = 0.15f;

    public override void EnterState(PlayerRevamp owner)
    {
        blend = owner.playerAnimator.GetFloat("Blend");
        time = 0;
        owner.IsAlert = false;
    }

    public override void ExitState(PlayerRevamp owner)
    {

    }

    public override void UpdateState(PlayerRevamp owner)
    {
        owner._currentDirection = Vector3.zero;

        if (owner.Input != Vector2.zero)
        {
            owner.stateMachine.ChangeState(new MovementState());
        }

        owner.playerAnimator.SetFloat("Blend", Mathf.Lerp(blend, 0, time / (idleBlendDuration * blend)));

        time += Time.deltaTime;

        /*
        if (owner.IsLockedOn)
        {
            owner.stateMachine.ChangeState(new IdleAlertState());
        }
        */
        bool inputFound = false;
        foreach (PlayerRevamp.InputType item in owner.inputBuffer)
        {
            switch (item)
            {
                case PlayerRevamp.InputType.Dash:
                    if (owner.dashCooldownTimer.Expired)
                    {
                        owner.Dash();
                        return;
                    }
                    break;
                case PlayerRevamp.InputType.Jump:
                    owner.Jump();
                    inputFound = true;
                    break;
                case PlayerRevamp.InputType.Parry:
                    if (owner.IsGrounded)
                    {
                        owner.stateMachine.ChangeState(new ParryState());
                        return;
                    }
                    break;
                case PlayerRevamp.InputType.AttackLight:
                    if (owner.IsGrounded)
                    {
                        owner.stateMachine.ChangeState(new LightAttackOneState());
                        return;
                    }
                    break;
                case PlayerRevamp.InputType.AttackHeavy:
                    if (owner.IsGrounded)
                    {
                        owner.stateMachine.ChangeState(new HeavyAttackState());
                        return;
                    }
                    break;
                default:
                    break;
            }
            if (inputFound)
            {
                break;
            }
        }
    }
}

/*
public class IdleAlertState : State<PlayerRevamp>
{
    public override void EnterState(PlayerRevamp owner)
    {

    }

    public override void ExitState(PlayerRevamp owner)
    {

    }

    public override void UpdateState(PlayerRevamp owner)
    {
        owner._currentDirection = Vector3.zero;

        if (owner.Input != Vector2.zero)
        {
            owner.stateMachine.ChangeState(new MovementState());
        }

        owner.playerAnimator.SetFloat("Blend", owner.Input.magnitude);

        if (!owner.IsLockedOn)
        {
            owner.stateMachine.ChangeState(new IdleState());
        }
        foreach (PlayerRevamp.InputType item in owner.inputBuffer)
        {
            switch (item)
            {
                case PlayerRevamp.InputType.Dash:
                    if (owner.dashCooldownTimer.Expired)
                    {
                        owner.Dash();
                        return;
                    }
                    break;
                case PlayerRevamp.InputType.Jump:
                    owner.Jump();
                    return;
                case PlayerRevamp.InputType.Parry:
                    if (owner.IsGrounded)
                    {
                        owner.stateMachine.ChangeState(new ParryState());
                        return;
                    }
                    break;
                case PlayerRevamp.InputType.AttackLight:
                    if (owner.IsGrounded)
                    {
                        owner.stateMachine.ChangeState(new LightAttackOneState());
                        return;
                    }
                    break;
                case PlayerRevamp.InputType.AttackHeavy:
                    if (owner.IsGrounded)
                    {
                        owner.stateMachine.ChangeState(new HeavyAttackState());
                        return;
                    }
                    break;
            default:
                    break;
            }
        }
    }
}
*/

public class MovementState : State<PlayerRevamp>
{
    private bool _isMoving;
    private float time = 0;
    public float idleBlendDuration = 0.15f;
    private float blend;

    public override void EnterState(PlayerRevamp owner)
    {
        blend = owner.playerAnimator.GetFloat("Blend");
        owner.IsAlert = false;
    }

    public override void ExitState(PlayerRevamp owner)
    {
        _isMoving = false;
        //owner.playerAnimator.SetBool("Running", _isMoving);
    }

    public float movingThreshold = 0.09f;
    //private float completeTurnAroundAngleThreshold = 120;
    public override void UpdateState(PlayerRevamp owner)
    {
        if (owner.Input == Vector2.zero)
        { // Changes state to idle if player is not moving
            owner.stateMachine.ChangeState(new IdleState());
        }
        else
        {
            bool inputFound = false;
            foreach (PlayerRevamp.InputType item in owner.inputBuffer)
            {
                switch (item)
                {
                    case PlayerRevamp.InputType.Dash:
                        if (owner.dashCooldownTimer.Expired)
                        {
                            owner.Dash();
                            return;
                        }
                        break;
                    case PlayerRevamp.InputType.Jump:
                        owner.Jump();
                        inputFound = true;
                        break;
                    case PlayerRevamp.InputType.Parry:
                        if (owner.IsGrounded)
                        {
                            owner.stateMachine.ChangeState(new ParryState());
                            return;
                        }
                        break;
                    case PlayerRevamp.InputType.AttackLight:
                        if (owner.IsGrounded)
                        {
                            owner.stateMachine.ChangeState(new LightAttackOneState());
                            return;
                        }
                        break;
                    case PlayerRevamp.InputType.AttackHeavy:
                        if (owner.IsGrounded)
                        {
                            owner.stateMachine.ChangeState(new HeavyAttackState());
                            return;
                        }
                        break;
                    default:
                        break;
                }
                if (inputFound)
                {
                    break;
                }
            }

            if (owner.IsLockedOn)
            {
                //PointOfInterestIsMousePos(owner);

                owner.transform.LookAt(owner.pointOfInterest);
                owner.transform.eulerAngles = new Vector3(0, owner.transform.eulerAngles.y, 0);

                float x = owner.Input.x;
                float z = owner.Input.y;

                Vector3 move = owner.transform.forward * z;
                move += owner.transform.right * x;

                owner._currentDirection = move;

                owner.controller.Move(move * owner.maxSpeed * Time.deltaTime);
            }
            else if (owner.Input.magnitude >= movingThreshold)
            {
                Vector3 baseInputDirection = Camera.main.transform.right * owner.Input.normalized.x + Camera.main.transform.forward * owner.Input.normalized.y;

                // The angle between baseInputDirection and resultingDirection
                float angle = Vector2.Angle(new Vector2(owner.transform.forward.x, owner.transform.forward.z),
                                            new Vector2(baseInputDirection.x, baseInputDirection.z));

                float rotationSpeed = owner.rotationSpeed * (angle > 135 ? 5 : 1);
                if (!owner.IsGrounded)
                    rotationSpeed = owner.airRotationSpeed * (angle > 135 ? 2 : 1);

                Vector3 resultingDirection = Vector3.RotateTowards(owner.transform.forward, baseInputDirection, rotationSpeed * Time.deltaTime, 0.0f);

                owner.transform.rotation = Quaternion.LookRotation(resultingDirection);
                owner.transform.eulerAngles = new Vector3(0, owner.transform.eulerAngles.y, 0); // Limits rotation to the Y-axis
                Vector3 move = owner.transform.forward * owner.Input.magnitude;                 // Constant forward facing force



                float magnitude = owner.Input.magnitude > 1 ? 1 : owner.Input.magnitude;
                //owner.playerAnimator.SetFloat("Blend", magnitude);
                if (time <= idleBlendDuration)
                {
                    owner.playerAnimator.SetFloat("Blend", Mathf.Lerp(blend, magnitude, time / (idleBlendDuration * blend)));
                    time += Time.deltaTime;
                }
                else
                {
                    owner.playerAnimator.SetFloat("Blend", magnitude);
                }

                //owner.playerAnimator.SetBool("Running", true);

                if (angle < owner.rotationAngleUntilMove)
                {
                    _isMoving = true;
                }

                if (_isMoving)
                {
                    float analogSpeed = move.magnitude;
                    if (analogSpeed > 1)
                        analogSpeed = 1;

                    Vector3 speed = move.normalized * analogSpeed;

                    if (owner.IsGrounded)
                        speed *= owner.maxSpeed;
                    else
                        speed *= owner.airSpeed;

                    owner._currentDirection = move;

                    owner.controller.Move(speed * Time.deltaTime);
                }
            }
        }
    }
}

public class DashingState : State<PlayerRevamp>
{
    private Timer _timer;
    private Timer _lagTimer;
    private Vector3 _dashDirection;

    public override void EnterState(PlayerRevamp owner)
    {
        owner.useGravity = false;
        owner.IsAlert = true;

        if (owner.actionAttackActive)
        {
            owner.actionHitboxGroup.enabled = true;
        }

        owner.playerAnimator.SetFloat("DashSpeed", owner.dashSpeed / owner.dashAnimationNumerator);

        if (owner.invulerableWhenDashing)
        {
            GlobalState.state.Player.invulerable = true;
        }

        _timer = new Timer(owner.dashTime);
        _lagTimer = new Timer(owner.dashLag);

        owner.playerAnimator.SetTrigger("Dash");
        owner.playerAnimator.SetFloat("Alert", 1.0f);
        owner.playerAnimator.SetBool("IsDashing", true);
        GlobalState.state.AudioManager.PlayerDodgeAudio(owner.transform.position);

        _dashDirection += Camera.main.transform.right * owner.Input.x;
        _dashDirection += Camera.main.transform.forward * owner.Input.y;
        if (_dashDirection == Vector3.zero)
            _dashDirection = Camera.main.transform.forward;

        _dashDirection.y = 0;
        if (!owner.IsLockedOn)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(_dashDirection.x, 0, _dashDirection.z));
            owner.transform.rotation = lookRotation;
        }
    }

    public override void ExitState(PlayerRevamp owner)
    {
        owner.useGravity = true;
        owner.dashPerformed = false;

        owner.actionHitboxGroup.enabled = false;

        if (owner.invulerableWhenDashing)
        {
            GlobalState.state.Player.invulerable = false;
        }

        owner.playerAnimator.SetBool("IsDashing", false);
        owner.playerAnimator.SetBool("HasJumped", false);
        owner.dashCooldownTimer.Reset();
    }

    public override void UpdateState(PlayerRevamp owner)
    {
        if (_timer.Expired)
        {
            //owner.playerAnimator.SetTrigger("DashLag");
            owner.playerAnimator.SetBool("IsDashing", false);
            _lagTimer.Time += Time.deltaTime;
            owner.useGravity = true;

            if (_lagTimer.Expired) // input buffer here?
            {
                owner.stateMachine.ChangeState(new IdleState());
            }
        }
        else
        {
            /*
            if (owner.IsGrounded)
            {
                RaycastHit hit;
                if (Physics.Raycast(new Ray(owner.transform.position, Vector3.down), out hit, 2f, GlobalState.state.GroundMask))
                {
                    _dashDirection.y = Mathf.Sin(Vector3.Angle(hit.normal, Vector3.up));
                }
            }
            if (_dashDirection.y > 0)
                _dashDirection.y = 0;
            */

            _timer.Time += Time.deltaTime;
            owner.controller.Move(_dashDirection.normalized * owner.dashSpeed * Time.deltaTime);

            if (owner.IsLockedOn)
            {
                owner.FaceDirection(owner.pointOfInterest);
            }
        }
    }
}

public class LightAttackOneState : State<PlayerRevamp>
{
    private bool _secondAttack = false;
    private GameObject _target;

    public override void EnterState(PlayerRevamp owner)
    {
        if (owner.actionAttackActive)
        {
            owner.actionHitboxGroup.enabled = true;
        }
        owner.walkCancel = false;
        owner.interruptable = false;
        owner.attackAnimationOver = false;
        owner.light1HitboxGroup.enabled = true;
        owner.IsAlert = true;

        owner.playerAnimator.SetTrigger("AttackLight"); // Set animation trigger for first attack
        owner.playerAnimator.SetBool("LightAttackTwo", false); // Reset the double combo animation bool upon entering state
        owner.playerAnimator.SetFloat("Alert", 1.0f);

        GlobalState.state.AudioManager.PlayerSwordSwingAudio(owner.transform.position);

        _target = owner.FindTarget();
    }

    public override void ExitState(PlayerRevamp owner)
    {
        owner.playerAnimator.SetBool("LightAttackTwo", _secondAttack); // Set animation bool
        owner.playerAnimator.SetBool("LightAttackOne", false); // Set animation bool

        _secondAttack = false; // Reset member variable

        owner.actionHitboxGroup.enabled = false;
        owner.light1HitboxGroup.enabled = false;
        owner.interruptable = false;
        owner.attackAnimationOver = false;
        owner.walkCancel = false;
    }

    public override void UpdateState(PlayerRevamp owner)
    {
        if (!owner.IsGrounded)
        {
            owner.stateMachine.ChangeState(new IdleState());
        }
        else
        {
            if (owner.attackStep)
            {
                if (_target != null)
                    owner.FaceDirection(_target.transform);
                owner.controller.Move(owner.transform.forward * owner.light1StepSpeed * Time.deltaTime);
            }

            if (owner.interruptable)
            {
                foreach (PlayerRevamp.InputType item in owner.inputBuffer)
                {
                    switch (item)
                    {
                        case PlayerRevamp.InputType.AttackLight:
                            if (owner.IsGrounded)
                            {
                                _secondAttack = true;
                                owner.stateMachine.ChangeState(new LightAttackTwoState());
                                return;
                            }
                            break;
                        case PlayerRevamp.InputType.AttackHeavy:
                            if (owner.IsGrounded)
                            {
                                owner.stateMachine.ChangeState(new HeavyAttackState());
                                return;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            if (owner.walkCancel && owner.Input != Vector2.zero)
            {
                owner.playerAnimator.SetTrigger("Cancel");
                owner.stateMachine.ChangeState(new MovementState());
            }
            else if (owner.attackAnimationOver)
            {
                owner.stateMachine.ChangeState(new IdleState());
            }
        }
    }
}

public class LightAttackTwoState : State<PlayerRevamp>
{
    private bool _secondAttack;
    private GameObject _target;

    public override void EnterState(PlayerRevamp owner)
    {
        if (owner.actionAttackActive)
        {
            owner.actionHitboxGroup.enabled = true;
        }
        owner.IsAlert = true;
        owner.walkCancel = false;
        owner.interruptable = false;
        owner.attackAnimationOver = false;
        owner.light2HitboxGroup.enabled = true;
        owner.playerAnimator.SetFloat("Alert", 1.0f);
        GlobalState.state.AudioManager.PlayerSwordSwingAudio(owner.transform.position);
        _target = owner.FindTarget();
    }

    public override void ExitState(PlayerRevamp owner)
    {
        owner.playerAnimator.SetBool("LightAttackOne", _secondAttack); // Set animation bool
        owner.playerAnimator.SetBool("LightAttackTwo", false); // Set animation bool

        _secondAttack = false;
        owner.actionHitboxGroup.enabled = false;
        owner.light2HitboxGroup.enabled = false;
        owner.interruptable = false;
        owner.attackAnimationOver = false;
        owner.walkCancel = false;
        _secondAttack = false;
    }

    public override void UpdateState(PlayerRevamp owner)
    {
        if (!owner.IsGrounded)
        {
            owner.stateMachine.ChangeState(new IdleState());
        }
        else
        {
            if (owner.attackStep)
            {
                if (_target != null)
                    owner.FaceDirection(_target.transform);
                owner.controller.Move(owner.transform.forward * owner.light2StepSpeed * Time.deltaTime);
            }

            if (owner.interruptable)
            {
                foreach (PlayerRevamp.InputType item in owner.inputBuffer)
                {
                    switch (item)
                    {
                        case PlayerRevamp.InputType.AttackLight:
                            if (owner.IsGrounded)
                            {
                                _secondAttack = true;
                                owner.stateMachine.ChangeState(new LightAttackOneState());
                                return;
                            }
                            break;
                        case PlayerRevamp.InputType.AttackHeavy:
                            if (owner.IsGrounded)
                            {
                                owner.stateMachine.ChangeState(new HeavyAttackState());
                                return;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            if (owner.walkCancel && owner.Input != Vector2.zero)
            {
                owner.playerAnimator.SetTrigger("Cancel");
                owner.stateMachine.ChangeState(new MovementState());
            }
            else if (owner.attackAnimationOver)
            {
                owner.stateMachine.ChangeState(new IdleState());
            }
        }
    }
}

public class HeavyAttackState : State<PlayerRevamp>
{
    private GameObject _target;
    private bool _playedSound;
    private bool _isCharging;

    private float _previousDamageMultiplier;
    private Timer _chargeTimer;

    public override void EnterState(PlayerRevamp owner)
    {
        _chargeTimer = new Timer(owner.heavyChargeTime);
        _isCharging = true;
        owner.IsAlert = true;

        if (owner.actionAttackActive)
        {
            owner.actionHitboxGroup.enabled = true;
        }

        owner.walkCancel = false;
        owner.interruptable = false;
        owner.attackAnimationOver = false;
        owner.heavyHitboxGroup.enabled = true;
        owner.playerAnimator.SetTrigger("AttackHeavy"); // Set animation trigger for first attack
        owner.playerAnimator.SetBool("HeavyCharge", true);
        owner.playerAnimator.SetFloat("Alert", 1.0f);

        _target = owner.FindTarget();

        GlobalState.state.AudioManager.PlayerHeavyAttackAudio(0, owner.transform); // play sound event?

    }

    public override void ExitState(PlayerRevamp owner)
    {
        _isCharging = false;
        _playedSound = false;
        owner.actionHitboxGroup.enabled = false;
        owner.heavyHitboxGroup.enabled = false;
        owner.interruptable = false;
        owner.attackAnimationOver = false;
        owner.walkCancel = false;
        owner.heavyHitboxGroup.enabled = false;
        owner.playerAnimator.SetBool("HeavyCharge", false);


        if (_previousDamageMultiplier != 0.0f)
            owner.modifier.DamageMultiplier *= _previousDamageMultiplier;
        else
            owner.modifier.DamageMultiplier.Reset();
    }

    public override void UpdateState(PlayerRevamp owner)
    {
        if (!owner.IsGrounded)
        {
            owner.stateMachine.ChangeState(new IdleState());
        }
        else
        {
            if (_isCharging)
            {
                _chargeTimer += Time.deltaTime;

                if (_chargeTimer.Expired || owner.inputBuffer.Contains(PlayerRevamp.InputType.AttackHeavyReleased))
                {
                    //GlobalState.state.AudioManager.PlayerHeavyAttackAudio(1, owner.transform); // play sound event?
                    _isCharging = false;
                    owner.playerAnimator.SetBool("HeavyCharge", false);
                    owner.actionHitboxGroup.enabled = false; // charge state krävs för att synkronisera med animationen (exit time)
                    if (owner.modifier.DamageMultiplier.IsModified)
                        _previousDamageMultiplier = owner.modifier.DamageMultiplier.Multiplier;
                    owner.modifier.DamageMultiplier *= 1f + _chargeTimer.Time / owner.heavyChargeTime * owner.heavyMaxDamageMultiplier;
                }
            }
            else
            {
                if (owner.attackStep)
                {
                    if (!_playedSound)
                    {
                        GlobalState.state.AudioManager.PlayerHeavyAttackAudio(2, owner.transform); // play sound event?
                        _playedSound = true;
                    }

                    if (_target != null)
                        owner.FaceDirection(_target.transform);
                    owner.controller.Move(owner.transform.forward * owner.heavyStepSpeed * Time.deltaTime);
                }

                if (owner.interruptable && owner.inputBuffer.Contains(PlayerRevamp.InputType.AttackLight))
                {
                    owner.playerAnimator.SetTrigger("AttackLight");
                    owner.stateMachine.ChangeState(new LightAttackOneState());
                }

                if (owner.walkCancel && owner.Input != Vector2.zero)
                {
                    owner.playerAnimator.SetTrigger("Cancel");
                    owner.stateMachine.ChangeState(new MovementState());
                }
                else if (owner.attackAnimationOver)
                {
                    owner.stateMachine.ChangeState(new IdleState());
                }
            }
        }
    }
}

public class HitstunState : State<PlayerRevamp>
{
    private Timer _hitstunTimer;
    public override void EnterState(PlayerRevamp owner)
    {
        _hitstunTimer = new Timer(owner.hitstunDuration);
        if (owner.hitstunDuration > owner.heavyHitstunThreshold) // heavy hitstun threshold
        {
            owner.playerAnimator.SetTrigger("HitstunHeavy");
        }
        else
        {
            owner.playerAnimator.SetTrigger("HitstunLight");
        }
        owner.playerAnimator.SetBool("InHitstun", true);
        owner.IsAlert = true;
    }

    public override void ExitState(PlayerRevamp owner)
    {
        owner.playerAnimator.SetBool("InHitstun", false);
        owner.playerAnimator.SetBool("HasJumped", false);
        owner.actionHitboxGroup.enabled = false;
    }

    public override void UpdateState(PlayerRevamp owner)
    {
        _hitstunTimer += Time.deltaTime;
        if (_hitstunTimer.Expired)
        {
            owner.stateMachine.ChangeState(new IdleState());
        }
    }
}

public class ParryState : State<PlayerRevamp>
{
    private Timer _parryTimer;
    private Timer _parryLagTimer;

    public override void EnterState(PlayerRevamp owner)
    {
        _parryTimer = new Timer(owner.parryDuration);
        _parryLagTimer = new Timer(owner.parryLag);
        //owner.isParrying = true; // parry sedan startup
        owner.playerAnimator.SetTrigger("Parry");
        owner.playerAnimator.SetBool("IsParrying", true);
        owner.playerAnimator.SetBool("ParryLag", false);
        owner.IsAlert = true;
        GlobalState.state.AudioManager.ParryindicatorAudio(owner.transform.position);

        if (owner.actionAttackActive)
        {
            owner.actionHitboxGroup.enabled = true;
        }
    }

    public override void ExitState(PlayerRevamp owner)
    {
        owner.actionHitboxGroup.enabled = false;
        owner.successfulParry = false;
        owner.attackAnimationOver = false;
        owner.isParrying = false;
        owner.playerAnimator.SetBool("IsParrying", false);
        owner.playerAnimator.SetBool("ParryLag", false);
        _parryTimer.Reset();
        _parryLagTimer.Reset();
    }

    public override void UpdateState(PlayerRevamp owner)
    {
        if (!owner.IsGrounded)
        {
            owner.stateMachine.ChangeState(new IdleState());
        }
        else
        {
            if (owner.successfulParry)
            {
                owner.playerAnimator.SetBool("ParryLag", false);
                if (_parryTimer.Expired && owner.attackAnimationOver)
                {
                    owner.stateMachine.ChangeState(new SuccessfulParryState());
                }
                else
                {
                    if (owner.Input != Vector2.zero)
                    {
                        owner.stateMachine.ChangeState(new MovementState());
                    }
                    else
                    {
                        foreach (PlayerRevamp.InputType item in owner.inputBuffer)
                        {
                            switch (item)
                            {
                                case PlayerRevamp.InputType.AttackLight:
                                    if (owner.IsGrounded)
                                    {
                                        owner.stateMachine.ChangeState(new LightAttackTwoState());
                                        return;
                                    }
                                    break;
                                case PlayerRevamp.InputType.AttackHeavy:
                                    if (owner.IsGrounded)
                                    {
                                        owner.stateMachine.ChangeState(new HeavyAttackState());
                                        return;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                }
            }
            else if (owner.attackAnimationOver)
            {
                owner.isParrying = true;
                if (_parryTimer.Expired)
                {
                    owner.isParrying = false;
                    owner.actionHitboxGroup.enabled = false;
                    owner.playerAnimator.SetBool("IsParrying", false);
                    owner.playerAnimator.SetBool("ParryLag", true);
                    _parryLagTimer.Time += Time.deltaTime;
                    if (_parryLagTimer.Expired)
                    {
                        owner.stateMachine.ChangeState(new IdleState());
                    }
                }
            }

            _parryTimer.Time += Time.deltaTime;
        }
    }

    private void Reset()
    {

    }
}

public class SuccessfulParryState : State<PlayerRevamp>
{
    public override void EnterState(PlayerRevamp owner)
    {
        if (owner.actionAttackActive)
        {
            owner.actionHitboxGroup.enabled = true;
        }

        owner.attackAnimationOver = true;
        owner.isParrying = true;
        owner.inputBuffer.Clear();
        owner.IsAlert = true;

        GlobalState.state.AudioManager.ParrySuccessAudio(owner.transform.position);
    }

    public override void ExitState(PlayerRevamp owner)
    {
        owner.actionHitboxGroup.enabled = false;
        owner.attackAnimationOver = false;
        owner.playerAnimator.SetBool("IsParrying", false);
        owner.isParrying = false;
    }

    public override void UpdateState(PlayerRevamp owner)
    {
        // parry logic goes here
        Debug.Log("successful parry state!!!");
        if (owner.attackAnimationOver)
            owner.stateMachine.ChangeState(new IdleState());
    }
}

public class PlayerDeathState : State<PlayerRevamp>
{
    public override void EnterState(PlayerRevamp owner)
    {
        owner.invulerable = true;
    }

    public override void ExitState(PlayerRevamp owner)
    {
        owner.invulerable = false;
        owner.playerAnimator.SetBool("Dead", false);
    }

    public override void UpdateState(PlayerRevamp owner) { }
}