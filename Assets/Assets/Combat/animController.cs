using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animController : MonoBehaviour
{

    [SerializeField] float radius;
    [SerializeField] float _dashAttackSpeed;

    Animator anim;
    CharacterController characterController;
    PlayerController control;
    FindTargets TargetFinder;

    private bool temp;
    private Vector3 direction ;

    GameObject target;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        characterController = GetComponentInParent<CharacterController>();
        control = GetComponentInParent<PlayerController>();
        TargetFinder = GetComponent<FindTargets>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            
            anim.SetTrigger("Attack");
            target = TargetFinder.FindTarget();

            if (!target)
            {
                FaceEnemy();
            }
        }

        if (temp)
        {
            if (radius > Vector3.Distance(target.transform.position, this.transform.position))
            {
                temp = false;
                control.enabled = true;
            }
            else
            {
                characterController.Move(new Vector3(direction.normalized.x, 0, direction.normalized.z) * _dashAttackSpeed * Time.deltaTime);           
            }
        }
    }

    public void FaceEnemy()
    {
        direction = (target.transform.position - this.transform.position);
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        this.transform.rotation = lookRotation;
        if (radius < direction.magnitude)
        {
            temp = true;
            control.enabled = false;
        }
    }
}
