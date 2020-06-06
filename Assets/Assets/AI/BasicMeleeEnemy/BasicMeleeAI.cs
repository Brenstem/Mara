using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class BasicMeleeAI : BaseAIMovementController
{
    [Header("Parry")]
    [SerializeField] private float _hitstunOnParry;

    [Header("Hitstun")]
    [SerializeField] public float _attackDelayAfterHitstun;

    [Header("Shader")]
    [SerializeField] float shaderFadeMultiplier = 1f;
    [SerializeField] GameObject _mesh;
    
    private Material _shader;
    private Timer _shaderTimer;
    private float _shaderFadeTime = -1f;

    [Header("References")]
    [SerializeField] public GameObject _healthBar;
    [SerializeField] public HitboxGroup hitboxGroup;
    [SerializeField] private GameObject _hurtVFX;

    // [SerializeField] public float _attackDelayAfterHitstun;

    [HideInInspector] public Timer _hitStunTimer;

    /* === UNITY FUNCTIONS === */
    void Start()
    {
        _mesh.GetComponent<Renderer>().materials[0] = Instantiate<Material>(_mesh.GetComponent<Renderer>().materials[0]);

        _shader = _mesh.GetComponent<Renderer>().materials[0];

        _shader.SetFloat("Vector1_5443722F", -1);

        for (int i = 0; i < _healthBar.transform.childCount; i++)
        {
            _healthBar.transform.GetChild(i).gameObject.SetActive(false);
        }

        stateMachine.ChangeState(new BasicMeleeIdleState());
        meleeEnemy = this;
        GenerateNewAttackTimer();
    }

    protected override void Update()
    {
        base.Update();

        if (_shaderTimer != null)
        {
            _shaderFadeTime += Time.deltaTime * shaderFadeMultiplier;
            Mathf.Clamp(_shaderFadeTime, -1, 1);
            _shader.SetFloat("Vector1_5443722F", _shaderFadeTime);
        }

        _anim.SetFloat("Blend", _agent.velocity.magnitude);
    }

    /* === PUBLIC FUNCTIONS === */
    public override void KillThis()
    {
        GlobalState.state.AudioManager.BasicEnemyDies(this.transform.position);
        stateMachine.ChangeState(new DeadState());
        _anim.SetBool("Dead", true);
        _agent.SetDestination(transform.position);
        transform.tag = "Untagged";
        _shaderTimer = new Timer(_shaderFadeTime);
    }

    public override void TakeDamage(HitboxValues hitbox, Entity attacker)
    {
        EnableHitstun(hitbox.hitstunTime, hitbox.ignoreArmor);
        base.TakeDamage(hitbox, attacker);

        // Instantiate(_hurtVFX, this.transform.position + new Vector3(0, 1.5f, 0), this.transform.rotation); ;

        if (hitbox.damageValue > 5)
        {
            GlobalState.state.AudioManager.FloatingEnemyHurtAudio(this.transform.position);
        }

        for (int i = 0; i < _healthBar.transform.childCount; i++)
        {
            _healthBar.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    public override void Parried()
    {
        EnableHitstun(_hitstunOnParry, true);
    }

    public void EnableHitstun(float duration, bool ignoreArmor)
    {
        if (duration > 0.0f && ignoreArmor)
        {
            _hitStunTimer = new Timer(duration);
            stateMachine.ChangeState(new MeleeAIHitstunState());
            GenerateNewAttackTimer(duration);
        }
        else if (duration > 0.0f && _canEnterHitStun)
        {
            _hitStunTimer = new Timer(duration);
            stateMachine.ChangeState(new MeleeAIHitstunState());
            AddToAttackTimer(_attackDelayAfterHitstun);
        }
        else if (duration > 0.0f && !_canEnterHitStun)
        {
            AddToAttackTimer(_attackDelayAfterHitstun);
        }
    }
}

/* === IDLE STATE === */
public class BasicMeleeIdleState : BaseIdleState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new BasicMeleeChasingState();
        base.EnterState(owner);
    }
    public override void UpdateState(BaseAIMovementController owner)
    {
        base.UpdateState(owner);
        owner._attackRateTimer += Time.deltaTime;
    }
}

/* === CHASING STATE === */ 
public class BasicMeleeChasingState : BaseChasingState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _attackingState = new BasicMeleeAttackingState();
        _returnToIdleState = new BasicMeleeReturnToIdleState();
        GlobalState.state.AudioManager.BasicEnemyAlerted(owner.transform.position);
        
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        base.UpdateState(owner);
        owner._attackRateTimer += Time.deltaTime;
    }
}

/* === ATTACKING STATE === */
public class BasicMeleeAttackingState : BaseAttackingState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new BasicMeleeChasingState();
    }
    
    public override void UpdateState(BaseAIMovementController owner)
    {
        owner._attackRateTimer += Time.deltaTime;
        
        //float range = owner._attackRange - owner._agent.stoppingDistance;
        float range = owner._attackRange;
        
        owner.FacePlayer();

        if (range < Vector3.Distance(owner._target.transform.position, owner.transform.position))
        {
            owner.stateMachine.ChangeState(_chasingState);
        }
        else if (range > Vector3.Distance(owner._target.transform.position, owner.transform.position))
        {
            if (owner._attackRateTimer.Expired)
            {
                owner.stateMachine.ChangeState(new BasicMeleeSwingState());
            }
        }
    }
}

/* === SWING STATE === */
public class BasicMeleeSwingState : State<BaseAIMovementController>
{
    public override void EnterState(BaseAIMovementController owner)
    {
        owner.meleeEnemy.hitboxGroup.enabled = true;
        owner._anim.SetTrigger("Attack");
        owner._canEnterHitStun = false;
    }

    public override void ExitState(BaseAIMovementController owner)
    {
        owner.meleeEnemy.hitboxGroup.enabled = false;
        owner._animationOver = false;
        owner._canEnterHitStun = owner._usesHitStun;
        owner.GenerateNewAttackTimer();
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        owner._attackRateTimer += Time.deltaTime;

        owner.FacePlayer();

        if (owner._animationOver)
        {
            owner.stateMachine.ChangeState(new BasicMeleeAttackingState());
        }
    }
}

/* === RETURN TO IDLE STATE === */
public class BasicMeleeReturnToIdleState : BaseReturnToIdlePosState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new BasicMeleeChasingState();
        _idleState = new BasicMeleeIdleState();
        base.EnterState(owner);
    }

    public override void ExitState(BaseAIMovementController owner)
    {
        for (int i = 0; i < owner.meleeEnemy._healthBar.transform.childCount; i++)
        {
            owner.meleeEnemy._healthBar.transform.GetChild(i).gameObject.SetActive(false);
        }

        base.ExitState(owner);
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        base.UpdateState(owner);
        owner._attackRateTimer += Time.deltaTime;
    }
}

/* === HITSTUN STATE === */
public class MeleeAIHitstunState : State<BaseAIMovementController>
{
    public override void EnterState(BaseAIMovementController owner) 
    {
        GlobalState.state.AudioManager.FloatingEnemyHurtAudio(owner.transform.position);
        owner._anim.SetTrigger("Hurt");
        owner._anim.SetBool("InHitstun", true);
    }

    public override void ExitState(BaseAIMovementController owner) 
    {
        owner._anim.SetBool("InHitstun", false);
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        owner.meleeEnemy._hitStunTimer.Time += Time.deltaTime;

        if (owner.meleeEnemy._hitStunTimer.Expired)
        {
            owner.meleeEnemy._hitStunTimer.Reset();
            owner.stateMachine.ChangeState(new BasicMeleeAttackingState());
        }
    }
}