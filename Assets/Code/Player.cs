using UnityEngine;

public class Player : MonoBehaviour
{
    public float Sensitivity = 0.132f;
    public float MoveSpeed = 3.0f;
    public float SprintMultiplier = 1.5f;

    private MazeGenerator mg;
    private Transform goal;
    private UIManager ui;
    private float pitch, yaw;
    private bool paused;
    private bool celebrating = false;
    private Vector3 move;
    private Rigidbody rb;

    void Awake()
    {
        mg = GameObject.FindGameObjectWithTag("GameController").GetComponent<MazeGenerator>();
        ui = GameObject.FindGameObjectWithTag("UI").GetComponent<UIManager>();
        goal = GameObject.FindGameObjectWithTag("Finish").transform;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        paused = false;
        mg.Size = new MazeGenerator.Int3(3, 3, 3);
        mg.Loops = 0;
        move = new Vector3();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        move = new();

        if (Input.GetButtonDown("Cancel") && !celebrating)
        {
            if (ui.IsScoreToggled())
            {
                CancelInvoke();
                ui.ToggleScoreText();
            }
            paused = !paused;
            Cursor.visible = !Cursor.visible;
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                paused = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                paused = false;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            ui.TogglePause();
        }
        if (paused)
            return;

        yaw += Input.GetAxisRaw("Mouse X") * Sensitivity;
        pitch -= Input.GetAxisRaw("Mouse Y") * Sensitivity;
        pitch = Mathf.Max(Mathf.Min(pitch, 90.0f), -90.0f);
        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);

        if (celebrating)
            return;

        move = transform.right * Input.GetAxisRaw("Horizontal") + transform.up * Input.GetAxisRaw("Vertical") + transform.forward * Input.GetAxisRaw("Forward");

        float multiplier = 1.0f;
        if (Input.GetButton("Sprint"))
            multiplier = SprintMultiplier;

        move = MoveSpeed * multiplier * move.normalized;

        if ((rb.position - goal.position).sqrMagnitude < 1.0f && !celebrating)
        {
            celebrating = true;
            ui.PlayParticles();
            ui.ToggleScoreText();
            ui.ChangeScoreText(mg.GetTime());
            Invoke("StopParticles", ui.ParticleDuration());
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = move;
    }

    private void StopParticles()
    {
        celebrating = false;
        mg.GenerateMaze();
        ui.ToggleScoreText();
    }

    public void ChangeScoreText(string text)
    {
        ui.ChangeScoreText(text);
    }

    public void ToggleScoreText()
    {
        ui.ToggleScoreText();
    }

    public void Respawn()
    {
        rb.position = new();
        pitch = yaw = 0;
        if (paused)
        {
            paused = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            ui.TogglePause();
        }
    }

    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
