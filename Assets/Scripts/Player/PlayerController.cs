using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;
    public float jumpForce = 7f;

    private Rigidbody rb;
    private bool isGrounded;
    private Camera playerCamera;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCamera = Camera.main;

        // Bloquear y esconder cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
    }

    void HandleMovement()
    {
        // Movimiento WASD - SOLO EN EJES X y Z
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Obtener dirección relativa a la cámara
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;

        // Ignorar componente Y para movimiento horizontal
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calcular dirección de movimiento
        Vector3 moveDirection = (cameraForward * moveVertical + cameraRight * moveHorizontal).normalized;

        // Aplicar movimiento SOLO en X y Z
        if (moveDirection != Vector3.zero)
        {
            Vector3 moveVelocity = moveDirection * moveSpeed;

            // Mantener la velocidad Y actual (gravedad) y aplicar movimiento en XZ
            rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);

            // Rotación hacia la dirección del movimiento
            if (moveVelocity.magnitude > 0.1f)
            {
                Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Si no hay input, mantener posición Y pero detener movimiento XZ
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Verificar si está en el suelo
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // Verificar continuamente si está en el suelo
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // Cuando deja de tocar el suelo
        isGrounded = false;
    }
}