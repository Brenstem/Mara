using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour {

    [SerializeField] private float _radius;
    [SerializeField] private float _dashAttackSpeed;

    Animator anim;
    CharacterController characterController;
    MovementController control;
    TargetFinder TargetFinder;

    private bool temp;
    private Vector3 direction;

    GameObject target;
    private void Awake() {
        anim = GetComponent<Animator>();
        characterController = GetComponentInParent<CharacterController>();
        control = GetComponentInParent<MovementController>();
        TargetFinder = GetComponent<TargetFinder>();
    }

    [SerializeField] private HitboxGroup hitboxGroup;

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            hitboxGroup.enabled = true;
            anim.SetTrigger("Attack");
            target = TargetFinder.FindTarget();

            if (!target) {
                FaceEnemy();
            }
        }

        if (temp) {
            if (_radius > Vector3.Distance(target.transform.position, this.transform.position)) {
                temp = false;
                control.enabled = true;
            }
            else {
                characterController.Move(new Vector3(direction.normalized.x, 0, direction.normalized.z) * _dashAttackSpeed * Time.deltaTime);
            }
        }
    }

    public void FaceEnemy() {
        if (target != null) {
            direction = (target.transform.position - this.transform.position);
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            this.transform.rotation = lookRotation;
            if (_radius < direction.magnitude) {
                temp = true;
                control.enabled = false;
            }
        }
    }
}