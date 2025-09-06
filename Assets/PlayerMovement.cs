using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sprintSpeed = 10f; // Швидкість бігу
    public float jumpForce = 5f; // Сила стрибка
    public float sensitivity = 2.0f;
    public float maxYAngle = 80f;
    public Transform cameraMain;
    public LayerMask groundLayer; // Шар, який позначає землю
    public Material selectionMaterial;

    private GameObject selectionO;
    private Material originalMaterial; // Змінено на тип Material
    private float rotationX = 0;
    private Rigidbody rb;

    private bool isGrounded; // Перевірка на знаходження на землі
    private bool isSprinting; // Відстежуємо, чи гравець біжить

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {

        // Перевірка на знаходження на землі за допомогою Raycast
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Логіка, яка вирішує, чи гравець біжить.
        // Це відбувається лише коли гравець знаходиться на землі.
        if (isGrounded)
        {
            isSprinting = Input.GetKey(KeyCode.LeftShift);
        }

        // Вибір швидкості залежить від стану isSprinting
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // Якщо гравець не рухається (inputs == 0), швидкість буде 0,
        // щоб уникнути інерції в повітрі після бігу.
        if (horizontalInput == 0 && verticalInput == 0)
        {
            currentSpeed = 0;
        }

        Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;

        // Рух гравця, використовуючи linearVelocity
        Vector3 newVelocity = new Vector3(moveDirection.x * currentSpeed, rb.linearVelocity.y, moveDirection.z * currentSpeed);
        rb.linearVelocity = newVelocity;
    }

    private void Update()
    {
        selectionObject();
        // Логіка стрибків
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // Використовуємо AddForce, як найкращий спосіб для фізичних стрибків
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Логіка обертання та камери
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Обертаємо тіло гравця по горизонталі
        transform.Rotate(Vector3.up * mouseX * sensitivity);

        // Оновлюємо позицію камери
        Vector3 targetCameraPosition = new Vector3(transform.position.x, transform.position.y + 0.4f, transform.position.z);
        cameraMain.position = targetCameraPosition;

        // Обертаємо камеру по вертикалі
        rotationX -= mouseY * sensitivity;
        rotationX = Mathf.Clamp(rotationX, -maxYAngle, maxYAngle);

        // Обертання камери, яке залежить від обертання гравця по Y та власного X-обертання
        Quaternion playerRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        Quaternion cameraVerticalRotation = Quaternion.Euler(rotationX, 0, 0);

        // Комбінуємо обертання
        cameraMain.rotation = playerRotation * cameraVerticalRotation;
    }

    private void selectionObject()
    {
        // Перевірка, чи не було раніше виділеного об'єкта, якщо так,
        // повертаємо йому оригінальний матеріал
        if (Input.GetMouseButtonDown(0))
        {
            if (selectionO != null)
            {
                selectionO.GetComponent<Renderer>().material = originalMaterial;
                selectionO = null;
            }

            // Випускаємо промінь
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                // Якщо об'єкт існує, зберігаємо його матеріал і змінюємо на матеріал виділення
                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    selectionO = hit.collider.gameObject;
                    originalMaterial = selectionO.GetComponent<Renderer>().material; // Зберігаємо саме матеріал
                    selectionO.GetComponent<Renderer>().material = selectionMaterial;
                }
            }
        }
    }

}