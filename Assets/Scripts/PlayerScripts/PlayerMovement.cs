using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Basic Settings")]
    private Rigidbody2D rb;
    public Vector2 moveInput;
    [SerializeField] private float moveSpeed = 5f;
    public bool canMove = false;

    [Header("Dash Settings")]
    public bool hasDash = false;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private int maxCharges = 2;
    [SerializeField] private float rechargeTime = 1f;
    private float rechargeTimer, dashTimeLeft;
    private int charges;
    private bool isDashing;
    private Vector2 lastMoveDirection;

    public static event Action<int> chargesChanged;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        charges = maxCharges;
    }

    private void FixedUpdate()
    {
        if (!canMove)
            return;

        lastMoveDirection = (moveInput != Vector2.zero) ? moveInput.normalized : lastMoveDirection;
        if (isDashing)
        {
            rb.MovePosition(rb.position + lastMoveDirection * dashSpeed * Time.fixedDeltaTime);
            dashTimeLeft -= Time.fixedDeltaTime;
            if (dashTimeLeft <= 0)
                isDashing = false;
        }
        else rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);

        if (charges < maxCharges)
            rechargeTimer += Time.fixedDeltaTime;

        if ((rechargeTimer >= rechargeTime))
        {
            charges++;
            chargesChanged?.Invoke(charges);
            rechargeTimer = 0f;
        }
    }

    public void Dash()
    {
        if (!hasDash || charges == 0) return;
        isDashing = true;
        dashTimeLeft = dashDuration;
        charges--;
        chargesChanged?.Invoke(charges);
        AudioManager.Instance.PlaySFX(1, 0.9f, 1);
    }

}
