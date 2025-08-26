using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;

public class Player : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5.0f; // Movement speed, adjustable in the Inspector
    [SerializeField] float jumpPower = 1;
    public Transform camera;
    Rigidbody rb;
    [SerializeField] Vector3 offset = new Vector3(0, 10, 10);
    private TextMeshProUGUI playerNameInput;
    GameObject serverPanel;
    bool spacebarHit;
    [SerializeField] float mouseSensitivity = 3f;
    private NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public TextMeshPro myName;
    float cameraHeight = 0f;
    float cameraHeightMin = 0;
    float cameraHeightMax = 20;

    void Awake()
    {
        transform.position = new Vector3(Random.Range(-3, 3), 0.5f, Random.Range(-3, 3));

        // Subscribe to network variable changes
        playerName.OnValueChanged += OnPlayerNameChanged;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // Update the name display with current value (important for clients joining after host)
        if (!string.IsNullOrEmpty(playerName.Value.ToString()))
        {
            myName.text = playerName.Value.ToString();
            Debug.Log($"Network spawned with existing name: {playerName.Value}");
        }
    }


    void Start()
    {
        if (!IsOwner) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb = GetComponent<Rigidbody>();

        serverPanel = GameObject.Find("Server info");

        camera = GameObject.Find("Camera").transform;

        // find "Name Input" object
        playerNameInput = GameObject.FindWithTag("PlayerNameText").GetComponent<TextMeshProUGUI>();

        // Request the server to set our player name
        if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text))
        {
            SetPlayerNameServerRpc(playerNameInput.text);
        }
        serverPanel.SetActive(false);
    }

    [ServerRpc]
    void SetPlayerNameServerRpc(string name)
    {
        playerName.Value = name;
        Debug.Log($"Server set player name to: {name}");
    }

    void OnPlayerNameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        // Update the displayed name when the network variable changes
        myName.text = newValue.ToString();
        Debug.Log($"Player name changed to: {newValue}");
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        HandleJump();
        HandleMovement();

        cameraHeight += mouseY * mouseSensitivity;
        cameraHeight = Mathf.Clamp(cameraHeight, cameraHeightMin, cameraHeightMax);
    }

    public override void OnDestroy()
    {
        // Unsubscribe from network variable changes to prevent memory leaks
        playerName.OnValueChanged -= OnPlayerNameChanged;
        base.OnDestroy();
    }

    void LateUpdate()
    {
        // Only execute for the owner of this object (multiplayer safety)
        if (!IsOwner) return;

        HandleCamera();
    }

    void Update()
    {
        // Only execute for the owner of this object (multiplayer safety)
        if (!IsOwner) return;

        HandleCursor();
        HandleJumpInput();
    }

    void HandleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            spacebarHit = true;
        }
    }

    void HandleJump() {
        if (spacebarHit)
        {
            rb.AddForce(0, jumpPower, 0, ForceMode.Impulse);
            spacebarHit = false;
        }
    }

    void HandleMovement()
    {
        // Get input from horizontal and vertical axes (WASD or Arrow keys by default)
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.Rotate(Vector3.up * mouseX * mouseSensitivity);

        // Calculate movement direction based on world coordinates
        Vector3 movement = new Vector3(horizontalInput, 0.0f, verticalInput);

        // Normalize the movement vector to prevent faster diagonal movement
        movement = movement.normalized;

        // Apply movement to the GameObject's position
        // Time.deltaTime ensures movement is frame-rate independent
        transform.Translate(movement * speed * Time.deltaTime, Space.Self);
    }

    void HandleCamera()
    {
        // Get player's current world position components
        float Px = transform.position.x;
        float Py = transform.position.y;
        float Pz = transform.position.z;

        // Get player's Y rotation (how much they've turned left/right)
        // Multiply by -1 to invert direction so camera orbits same way player rotates
        float PlayerYAngle = transform.eulerAngles.y * -1 + 90f;

        // Calculate camera's orbit position using circular math
        // Formula: newPos = center + radius * (cos(angle), sin(angle))
        float x = Px + offset.z * Mathf.Cos(PlayerYAngle * Mathf.Deg2Rad);  // X position on circle
        float z = Pz + offset.z * Mathf.Sin(PlayerYAngle * Mathf.Deg2Rad);  // Z position on circle


        // Apply the calculated position to camera
        // Y uses player's Y + offset.y for height above/below player
        camera.transform.position = new Vector3(x, Py + cameraHeight, z);

        // Make camera always look at the player
        camera.transform.LookAt(transform.position);
    }
}
