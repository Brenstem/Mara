using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Interactions;
using Random = UnityEngine.Random;

public class RangedEnemyAI : BaseAIMovementController
{
    // Variable default values need to be declared in start after "rangedAI = this;" because code dumb >:(
    [Header("References")]
    [SerializeField] private GameObject _projectile;
    [SerializeField] private Transform _projectileSpawnPos;
    [SerializeField] public GameObject _healthBar;

    [Header("Ranged Attack")]
    [SerializeField] private float _firerate;
    [SerializeField] private float _aimAssistDistanceCutOff;

    [Header("Melee Attack")]
    [SerializeField] public float _meleeRange;
    [SerializeField] public HitboxGroup _meleeHitBoxGroup;
    [SerializeField] public float _minMeleeAttackSpeed;
    [SerializeField] public float _maxMeleeAttackSpeed;

    [Header("Parry")]
    [SerializeField] private float _hitstunOnParry;

    [HideInInspector] public Timer _hitStunTimer;
    [HideInInspector] public bool _canTurn;

    /* === UNITY FUNCTIONS === */
    private void Start()
    {
        for (int i = 0; i < _healthBar.transform.childCount; i++)
        {
            _healthBar.transform.GetChild(i).gameObject.SetActive(false);
        }

        stateMachine.ChangeState(new RangedEnemyIdleState());
        rangedAI = this;
        _canTurn = true; // Declare variable default values like this
    }

    protected override void Update()
    {
        base.Update();

        print(stateMachine.currentState);

        _anim.SetFloat("Blend", _agent.velocity.magnitude);
    }

    /* === PUBLIC FUNCTIONS === */
    public override void KillThis()
    {
        stateMachine.ChangeState(new DeadState());
        _anim.SetBool("Dead", true);
        _agent.SetDestination(transform.position);
        transform.tag = "Untagged";
    }

    public override void TakeDamage(HitboxValues hitbox, Entity attacker)
    {
        EnableHitstun(hitbox.hitstunTime, hitbox.ignoreArmor);
        GlobalState.state.AudioManager.FloatingEnemyHurtAudio(this.transform.position);
        base.TakeDamage(hitbox, attacker);

        for (int i = 0; i < _healthBar.transform.childCount; i++)
        {
            _healthBar.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    public override void Parried()
    {
        EnableHitstun(_hitstunOnParry, true);
        UnityEngine.Debug.LogWarning("Parried implementation missing", this);
    }

    public void EnableHitstun(float duration, bool ignoreArmor)
    {
        if (duration > 0.0f && ignoreArmor)
        {
            stateMachine.ChangeState(new RangedAIHitStunState());
            _hitStunTimer = new Timer(duration);
        }
        else if (duration > 0.0f && _canEnterHitStun)
        {
            stateMachine.ChangeState(new RangedAIHitStunState());
            _hitStunTimer = new Timer(duration);
        }
    }

    public void Fire()
    {
        GlobalState.state.AudioManager.RangedEnemyFireAudio(this.transform.position);
        Instantiate(_projectile, _projectileSpawnPos.position, this.transform.rotation);
        _canTurn = true;
    }

    public override void FacePlayer()
    {
        // Only use autoaim if target is a bit away, this prevents the dude from spazzing tf out when player comes too close

        if (Vector3.Distance(_target.transform.position, transform.position) > _aimAssistDistanceCutOff)
        {
            Vector3 targetVelocity = _target.GetComponent<PlayerRevamp>().CurrentDirection * _target.GetComponent<PlayerRevamp>().CurrentSpeed;
            Vector3 direction;

            PredictiveAim(_projectileSpawnPos.position, _projectile.GetComponent<ProjectileBehaviour>().Speed, _target.transform.position, targetVelocity, 0, out direction);

            direction = direction.normalized;

            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, lookRotation, Time.deltaTime * _turnSpeed); //ändrat från Slerp till Lerp MVH Måns
        }
        else
        {
            Vector3 direction = (_target.transform.position - this.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookRotation, Time.deltaTime * _turnSpeed);
        }
    }

    // Full derivation by Kain Shin exists here:
    // http://www.gamasutra.com/blogs/KainShin/20090515/83954/Predictive_Aim_Mathematics_for_AI_Targeting.php

    static public bool PredictiveAim(Vector3 muzzlePosition, float projectileSpeed, Vector3 targetPosition, Vector3 targetVelocity, float gravity, out Vector3 projectileVelocity)
    {
        System.Diagnostics.Debug.Assert(projectileSpeed > 0, "What are you doing shooting at something with a projectile that doesn't move?");

        if (muzzlePosition == targetPosition)
        {
            // Why dost thou hate thyself so?
            // Do something smart here. I dunno... whatever.
            projectileVelocity = projectileSpeed * (Random.rotation * Vector3.forward);
            return true;
        }

        // Much of this is geared towards reducing floating point precision errors
        float projectileSpeedSq = projectileSpeed * projectileSpeed;
        float targetSpeedSq = targetVelocity.sqrMagnitude; // doing this instead of self-multiply for maximum accuracy
        float targetSpeed = Mathf.Sqrt(targetSpeedSq);
        Vector3 targetToMuzzle = muzzlePosition - targetPosition;
        float targetToMuzzleDistSq = targetToMuzzle.sqrMagnitude; // doing this instead of self-multiply for maximum accuracy
        float targetToMuzzleDist = Mathf.Sqrt(targetToMuzzleDistSq);
        Vector3 targetToMuzzleDir = targetToMuzzle;
        targetToMuzzleDir.Normalize();

        // Law of Cosines: A*A + B*B - 2*A*B*cos(theta) = C*C
        // A is distance from muzzle to target (known value: targetToMuzzleDist)
        // B is distance traveled by target until impact (targetSpeed * t)
        // C is distance traveled by projectile until impact (projectileSpeed * t)
        float cosTheta = (targetSpeedSq > 0)
            ? Vector3.Dot(targetToMuzzleDir, targetVelocity.normalized)
            : 1.0f;

        bool validSolutionFound = true;
        float t;

        if (Mathf.Approximately(projectileSpeedSq, targetSpeedSq))
        {
            // a = projectileSpeedSq - targetSpeedSq = 0
            // We want to avoid div/0 that can result from target and projectile traveling at the same speed
            // We know that C and B are the same length because the target and projectile will travel the same distance to impact
            // Law of Cosines: A*A + B*B - 2*A*B*cos(theta) = C*C
            // Law of Cosines: A*A + B*B - 2*A*B*cos(theta) = B*B
            // Law of Cosines: A*A - 2*A*B*cos(theta) = 0
            // Law of Cosines: A*A = 2*A*B*cos(theta)
            // Law of Cosines: A = 2*B*cos(theta)
            // Law of Cosines: A/(2*cos(theta)) = B
            // Law of Cosines: 0.5f*A/cos(theta) = B
            // Law of Cosines: 0.5f * targetToMuzzleDist / cos(theta) = targetSpeed * t
            // We know that cos(theta) of zero or less means there is no solution, since that would mean B goes backwards or leads to div/0 (infinity)
            if (cosTheta > 0)
            {
                t = (0.5f * targetToMuzzleDist / (targetSpeed * cosTheta)) ;
            }
            else
            {
                validSolutionFound = false;
                t = PredictiveAimWildGuessAtImpactTime();
            }
        }
        else
        {
            // Quadratic formula: Note that lower case 'a' is a completely different derived variable from capital 'A' used in Law of Cosines (sorry):
            // t = [ -b � Sqrt( b*b - 4*a*c ) ] / (2*a)
            float a = projectileSpeedSq - targetSpeedSq;
            float b = 2.0f * targetToMuzzleDist * targetSpeed * cosTheta;
            float c = -targetToMuzzleDistSq;
            float discriminant = b * b - 4.0f * a * c;

            if (discriminant < 0)
            {
                // Square root of a negative number is an imaginary number (NaN)
                // Special thanks to Rupert Key (Twitter: @Arakade) for exposing NaN values that occur when target speed is faster than or equal to projectile speed
                validSolutionFound = false;
                t = PredictiveAimWildGuessAtImpactTime();
            }
            else
            {
                // a will never be zero because we protect against that with "if (Mathf.Approximately(projectileSpeedSq, targetSpeedSq))" above
                float uglyNumber = Mathf.Sqrt(discriminant);
                float t0 = 0.5f * (-b + uglyNumber) / a;
                float t1 = 0.5f * (-b - uglyNumber) / a;

                // Assign the lowest positive time to t to aim at the earliest hit
                t = Mathf.Min(t0, t1);
                if (t < Mathf.Epsilon)
                {
                    t = Mathf.Max(t0, t1) + (Mathf.Acos(cosTheta) * (Time.deltaTime * 5)) + (9.0f / 30.0f); /*CHANGE "5" TO TURNSPEED!!!!!!!!!!!!!!!!1! (9 = frames mellan turnlock och fire, 30 = animationFPS) */
                }

                if (t < Mathf.Epsilon)
                {
                    // Time can't flow backwards when it comes to aiming.
                    // No real solution was found, take a wild shot at the target's future location
                    validSolutionFound = false;
                    t = PredictiveAimWildGuessAtImpactTime();
                }
            }
        }

        // Vb = Vt - 0.5*Ab*t + [(Pti - Pbi) / t]
        projectileVelocity = targetVelocity + (-targetToMuzzle / t);
        if (!validSolutionFound)
        {
            // PredictiveAimWildGuessAtImpactTime gives you a t that will not result in impact
            // Which means that all that math that assumes projectileSpeed is enough to impact at time t breaks down
            // In this case, we simply want the direction to shoot to make sure we
            // don't break the gameplay rules of the cannon's capabilities aside from gravity compensation
            projectileVelocity = projectileSpeed * projectileVelocity.normalized;
        }

        if (!Mathf.Approximately(gravity, 0))
        {
            // projectileSpeed passed in is a constant that assumes zero gravity.
            // By adding gravity as projectile acceleration, we are essentially breaking real world rules by saying that the projectile
            // gets additional gravity compensation velocity for free
            // We want netFallDistance to match the net travel distance caused by gravity (whichever direction gravity flows)
            float netFallDistance = (t * projectileVelocity).z;
            // d = Vi*t + 0.5*a*t^2
            // Vi*t = d - 0.5*a*t^2
            // Vi = (d - 0.5*a*t^2)/t
            // Remember that gravity is a positive number in the down direction, the stronger the gravity, the larger gravityCompensationSpeed becomes
            float gravityCompensationSpeed = (netFallDistance + 0.5f * gravity * t * t) / t;
            projectileVelocity.z = gravityCompensationSpeed;
        }

        // FOR CHECKING ONLY (valid only if gravity is 0)...
        // float calculatedprojectilespeed = projectileVelocity.magnitude;
        // bool projectilespeedmatchesexpectations = (projectileSpeed == calculatedprojectilespeed);
        // ...FOR CHECKING ONLY

        return validSolutionFound;
    }

    static float PredictiveAimWildGuessAtImpactTime()
    {
        return Random.Range(0.01f, 0.1f);
    }
}

/* === IDLE STATE === */
public class RangedEnemyIdleState : BaseIdleState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        owner.GenerateNewAttackTimer();
        _chasingState = new RangedEnemyChasingState();
        base.EnterState(owner);
    }
}

/* === CHASING STATE === */ 
public class RangedEnemyChasingState : BaseChasingState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        base.EnterState(owner);

        _attackingState = new RangedEnemyAttackingState();
        _returnToIdleState = new RangedEnemyReturnToIdleState();

        GlobalState.state.AudioManager.RangedEnemyAlertAudio(owner.rangedAI.transform.position);
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        base.UpdateState(owner);
    }
}

/* === ATTACKING STATE === */
public class RangedEnemyAttackingState : BaseAttackingState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new RangedEnemyChasingState();
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        if (owner._attackRange < Vector3.Distance(owner._target.transform.position, owner.transform.position))
        {
            owner.stateMachine.ChangeState(_chasingState);
        }
        else if (Vector3.Distance(owner._target.transform.position, owner.transform.position) < owner.rangedAI._meleeRange)
        {
            if (owner._attackRateTimer.Expired)
            {
                owner.stateMachine.ChangeState(new RangedEnemySwingState());
            }
        }

        owner._attackRateTimer += Time.deltaTime;

        owner.rangedAI.FacePlayer();

        if (owner._attackRateTimer.Expired && Vector3.Distance(owner._target.transform.position, owner.transform.position) > owner.rangedAI._meleeRange)
        {
            owner.stateMachine.ChangeState(new RangedEnemyFiringState());
        }
    }
}

/* === FIRING STATE === */
public class RangedEnemyFiringState : State<BaseAIMovementController>
{
    public override void EnterState(BaseAIMovementController owner)
    {
        owner._canEnterHitStun = false;
        owner._anim.SetTrigger("Attack");
    }

    public override void ExitState(BaseAIMovementController owner)
    {
        owner._animationOver = false;
        owner._canEnterHitStun = owner._usesHitStun;
        owner.GenerateNewAttackTimer();
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        if (owner.rangedAI._canTurn)
        {
            owner.rangedAI.FacePlayer();
        }

        if (owner._animationOver)
        {
            owner.stateMachine.ChangeState(owner.stateMachine.previousState);
        }
    }
}

/* === SWING STATE === */
public class RangedEnemySwingState : State<BaseAIMovementController>
{
    public override void EnterState(BaseAIMovementController owner)
    {
        owner._anim.SetTrigger("Swing");
        owner._canEnterHitStun = false;
        owner.rangedAI._meleeHitBoxGroup.enabled = true;
    }

    public override void ExitState(BaseAIMovementController owner)
    {
        owner._animationOver = false;
        owner._canEnterHitStun = owner._usesHitStun;
        owner.GenerateNewAttackTimer(owner.rangedAI._minMeleeAttackSpeed, owner.rangedAI._maxMeleeAttackSpeed);
        owner.rangedAI._meleeHitBoxGroup.enabled = false;
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        owner.FacePlayer();

        if (owner._animationOver)
        {
            owner.stateMachine.ChangeState(new RangedEnemyAttackingState());
        }
    }
}

/* === RETURN TO IDLE STATE === */
public class RangedEnemyReturnToIdleState : BaseReturnToIdlePosState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new RangedEnemyChasingState();
        _idleState = new RangedEnemyIdleState();
        base.EnterState(owner);
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        base.UpdateState(owner);
    }

    public override void ExitState(BaseAIMovementController owner)
    {
        for (int i = 0; i < owner.rangedAI._healthBar.transform.childCount; i++)
        {
            owner.rangedAI._healthBar.transform.GetChild(i).gameObject.SetActive(false);
        }

        base.ExitState(owner);
    }
}

/* === HITSTUN STATE === */ 
public class RangedAIHitStunState : State<BaseAIMovementController>
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
        owner.rangedAI._hitStunTimer.Time += Time.deltaTime;
        if (owner.rangedAI._hitStunTimer.Expired)
        {
            owner.rangedAI._hitStunTimer.Reset();
            owner.stateMachine.ChangeState(owner.stateMachine.previousState);
        }
    }
}
