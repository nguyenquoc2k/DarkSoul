using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace SG
{
    public class PlayerLocomotion : MonoBehaviour
    {
        private PlayerManager playerManager;
        private Transform cameraObject;
        private InputHandler inputHandler;
        public Vector3 moveDirection;
        [HideInInspector] public Transform myTransform;
        [HideInInspector] public AnimatorHandler animatorHandler;
        public new Rigidbody rigidbody;
        public GameObject normalCamera;

        [Header("Ground & Air Detection Stats")] [SerializeField]
        private float groundDectectionRayStartPoint = 0.5f;

        [SerializeField] private float minimunDistanceNeedToBeginFall = 1f;
        [SerializeField] private float groundDectectionRayDistance = 0.2f;
        private LayerMask ignorForGroundCheck;
        public float inAirTimer;

        [Header("Movement Stats")] [SerializeField]
        private float movementSpeed = 5;

        [SerializeField] private float sprintSpeed = 7;
        [SerializeField] private float rotationSpeed = 10;
        [SerializeField] private float fallingSpeed = 10;
        [SerializeField] private float walkingSpeed = 1;

        void Start()
        {
            playerManager = GetComponent<PlayerManager>();
            rigidbody = GetComponent<Rigidbody>();
            inputHandler = GetComponent<InputHandler>();
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
            cameraObject = Camera.main.transform;
            myTransform = transform;
            animatorHandler.Initialize();
            playerManager.isGrounded = true;
            ignorForGroundCheck = ~(1 << 8 | 1 << 11);
        }

        #region Movement

        private Vector3 targetDir;
        private Vector3 normalVector;

        private void HandleRotation(float delta)
        {
            Vector3 targetDir = Vector3.zero;
            float moveOverride = inputHandler.moveAmount;
            targetDir = cameraObject.forward * inputHandler.vertical;
            targetDir += cameraObject.right * inputHandler.horizontal;
            targetDir.Normalize();
            targetDir.y = 0;
            if (targetDir == Vector3.zero) targetDir = myTransform.forward;
            float rs = rotationSpeed;
            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);
            myTransform.rotation = targetRotation;
        }

        #endregion

        private Vector3 targetPosition;
        // Update is called once per frame


        public void HandleMovement(float delta)
        {
            if (inputHandler.rollFlag) return;
            if(playerManager.isInteracting) return;
            moveDirection = cameraObject.forward * inputHandler.vertical;
            moveDirection += cameraObject.right * inputHandler.horizontal;
            moveDirection.Normalize();
            moveDirection.y = 0;
            float speed = movementSpeed;
            if (inputHandler.sprintFlag&&inputHandler.moveAmount > 0.5f)
            {
                speed = sprintSpeed;
                playerManager.isSprinting = true;
                moveDirection *= speed;
            }
            else
            {
                if (inputHandler.moveAmount < 0.5f)
                {
                    moveDirection *= walkingSpeed;
                    playerManager.isSprinting = false;
                }
                else
                {
                    moveDirection *= speed;
                    playerManager.isSprinting = false;
                }
               
            }

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);
            rigidbody.velocity = projectedVelocity;
            animatorHandler.UpdateAnimatorValues(inputHandler.moveAmount, 0, playerManager.isSprinting);
            if (animatorHandler.canRotate)
            {
                HandleRotation(delta);
            }
        }

        public void HandleRollingAndSprinting(float delta)
        {
            if (animatorHandler.anim.GetBool("isInteracting")) return;
            if (inputHandler.rollFlag)
            {
                moveDirection = cameraObject.forward * inputHandler.vertical;
                moveDirection += cameraObject.right * inputHandler.horizontal;
                if (inputHandler.moveAmount > 0)
                {
                    animatorHandler.PlayTargetAnimation("Roll", true);
                    Debug.Log("roll");
                    moveDirection.y = 0;
                    Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                    myTransform.rotation = rollRotation;
                }
                else
                {
                    animatorHandler.PlayTargetAnimation("Backstep", true);
                }
            }
        }

        public void HandleFalling(float delta, Vector3 moveDirection)
        {
            playerManager.isGrounded = false;
            RaycastHit hit;
            Vector3 origin = myTransform.position;
            origin.y += groundDectectionRayStartPoint;
            if (Physics.Raycast(origin, myTransform.forward, out hit, 0.4f))
            {
                moveDirection = Vector3.zero;
            }

            if (playerManager.isInAir)
            {
                rigidbody.AddForce(-Vector3.up * fallingSpeed);
                rigidbody.AddForce(moveDirection * fallingSpeed / 10f);
            }

            Vector3 dir = moveDirection;
            moveDirection.Normalize();
            origin = origin + dir * groundDectectionRayDistance;
            targetPosition = myTransform.position;
            Debug.DrawRay(origin, -Vector3.up * minimunDistanceNeedToBeginFall, Color.red, 0.1f, false);
            if (Physics.Raycast(origin, -Vector3.up, out hit, minimunDistanceNeedToBeginFall, ignorForGroundCheck))
            {
                normalVector = hit.normal;
                Vector3 tp = hit.point;
                playerManager.isGrounded = true;
                targetPosition.y = tp.y;
                if (playerManager.isInAir)
                {
                    if (inAirTimer > 0.5f)
                    {
                        animatorHandler.PlayTargetAnimation("Land", true);
                        inAirTimer = 0;
                    }
                    else
                    {
                        Debug.Log("Empty");
                        animatorHandler.PlayTargetAnimation("Empty", false);
                        inAirTimer = 0;
                    }

                    playerManager.isInAir = false;
                }
            }
            else
            {
                if (playerManager.isGrounded)
                {
                    playerManager.isGrounded = false;
                }

                if (playerManager.isInAir == false)
                {
                    if (playerManager.isInteracting == false)
                    {
                        animatorHandler.PlayTargetAnimation("Falling", true);
                    }

                    Vector3 vel = rigidbody.velocity;
                    vel.Normalize();
                    rigidbody.velocity = vel * (movementSpeed / 2);
                    playerManager.isInAir = true;
                }
            }

            if (playerManager.isGrounded)
            {
                if (playerManager.isInteracting || inputHandler.moveAmount > 0)
                {
                    myTransform.position = Vector3.Lerp(myTransform.position, targetPosition, Time.deltaTime);
                }
                else
                {
                    myTransform.position = targetPosition;
                }
            }
        }
    }
}