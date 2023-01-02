using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SG
{
    public class InputHandler : MonoBehaviour
    {
        public float horizontal;
        public float vertical;
        public float moveAmount;
        public float mouseX;
        public float mouseY;
        private PlayerControl inputActions;
        private PlayerAttacker playerAttacker;
        private PlayerInventory playerInventory;
        private Vector2 movementInput;
        private Vector2 CameraInput;
        public bool b_Input;
        public bool rb_Input;
        public bool rt_Input;
        public bool rollFlag;
        public bool sprintFlag;
        public float rollInputTimer;


        private void Awake()
        {
            playerAttacker = GetComponent<PlayerAttacker>();
            playerInventory = GetComponent<PlayerInventory>();
        }

        private void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerControl();
                inputActions.PlayerMovement.Movement.performed +=
                    inputActions => movementInput = inputActions.ReadValue<Vector2>();
                inputActions.PlayerMovement.Camera.performed += i => CameraInput = i.ReadValue<Vector2>();
            }

            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        public void TickInput(float delta)
        {
            MoveInput(delta);
            HandleRollInput(delta);
            HandleAttackInput(delta);
        }

        public void MoveInput(float delta)
        {
            horizontal = movementInput.x;
            vertical = movementInput.y;
            moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
            mouseX = CameraInput.x;
            mouseY = CameraInput.y;
        }

        private void HandleRollInput(float delta)
        {
            b_Input = inputActions.PlayerActions.Roll.phase == UnityEngine.InputSystem.InputActionPhase.Performed;
            if (b_Input)
            {
                rollInputTimer += delta;
                sprintFlag = true;
            }
            else
            {
                if (rollInputTimer > 0 && rollInputTimer < 0.5f)
                {
                    sprintFlag = false;
                    rollFlag = true;
                }

                rollInputTimer = 0;
            }
        }

        public void HandleAttackInput(float delta)
        {
            inputActions.PlayerActions.RB.performed += i => rb_Input = true;
            inputActions.PlayerActions.RT.performed += i => rt_Input = true;
            //RB Input handles the RIGHT hand weapon's light attack - Đầu vào RB xử lý đòn tấn công nhẹ của vũ khí tay PHẢI
            if (rb_Input)
            {
                playerAttacker.HandleLightAttack(playerInventory.rigtWeapon);
            }

            if (rt_Input)
            {
                playerAttacker.HandleHeavyAttack(playerInventory.rigtWeapon);
            }
        }
    }
}