using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions input;
    private PlayerMovement movement;
    private PlayerShooting shooting;
    private PlayerMelee melee;

    private Vector2 moveInput;
    private bool isShooting;
    private bool interacting;
    private bool hasInteracted;
    public event Action interacted;
    public static event Action paused;

    void Awake()
    {
        input = new InputSystem_Actions();
        movement = GetComponent<PlayerMovement>();
        shooting = GetComponent<PlayerShooting>();
        melee = GetComponent<PlayerMelee>();
    }

    private void OnEnable()
    {
        input.Player.Enable();

        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += _ => moveInput = Vector2.zero;

        input.Player.Dash.performed += _ => movement.Dash();

        input.Player.Shoot.performed += _ => isShooting = true;
        input.Player.Shoot.canceled += _ => isShooting = false;
        input.Player.Shoot.canceled += _ => shooting.hasShot = false;

        input.Player.Gun1.performed += _ => shooting.SwitchGun(0);
        input.Player.Gun2.performed += _ => shooting.SwitchGun(1);

        input.Player.Interact.performed += _ => interacting = true;
        input.Player.Interact.canceled += _ => interacting = false;
        input.Player.Interact.canceled += _ => hasInteracted = false;

        input.Player.Pause.performed += _ => paused?.Invoke();

        input.Player.Melee.performed += _ => melee.UseMelee();

        input.Player.UseShockwave.performed += _ => shooting.UseShockwave();
    }

    private void OnDisable()
    {
        input.Player.Disable();

        input.Player.Dash.performed -= _ => movement.Dash();
    }

    private void FixedUpdate()
    {
        movement.moveInput = moveInput;
        if (isShooting)
            shooting.Shoot();
        if (interacting && !hasInteracted) {
            hasInteracted = true;
            interacted?.Invoke();
        }
    }

}
