using UnityEngine;

namespace SchoolEscape.Player {
    /// <summary>
    /// All tunable first-person player values. Create via
    /// Create > School Escape > Player > Player Settings.
    /// Treated as read-only at runtime; runtime changes (options menu)
    /// go through the setter methods on the individual components.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerSettings", menuName = "School Escape/Player/Player Settings")]
    public sealed class PlayerSettings : ScriptableObject {
        [Header("Movement")]
        [Tooltip("Walking speed in metres per second.")]
        [SerializeField, Min(0f)] private float walkSpeed = 4f;
        [Tooltip("Sprinting speed in metres per second.")]
        [SerializeField, Min(0f)] private float sprintSpeed = 6.5f;
        [Tooltip("How fast the player speeds up (m/s²).")]
        [SerializeField, Min(0.1f)] private float acceleration = 30f;
        [Tooltip("How fast the player slows down when there is no input (m/s²).")]
        [SerializeField, Min(0.1f)] private float deceleration = 35f;
        [Tooltip("Fraction of acceleration/deceleration available in the air (0 = none, 1 = full).")]
        [SerializeField, Range(0f, 1f)] private float airControl = 0.35f;
        [Tooltip("Jump height in metres.")]
        [SerializeField, Min(0f)] private float jumpHeight = 1.1f;
        [Tooltip("Gravity strength as a POSITIVE value in m/s². Real gravity is 9.81, games feel better with ~20.")]
        [SerializeField, Min(0.1f)] private float gravity = 20f;
        [Tooltip("Maximum falling speed in m/s.")]
        [SerializeField, Min(1f)] private float terminalVelocity = 40f;
        [Tooltip("Small downward speed applied while grounded so the player stays glued to the ground.")]
        [SerializeField, Min(0f)] private float groundStickVelocity = 2f;

        [Header("Ground Check")]
        [Tooltip("Radius of the ground-check sphere. Clamped to the CharacterController radius.")]
        [SerializeField, Min(0.01f)] private float groundCheckRadius = 0.28f;
        [Tooltip("How far below the feet the ground is still detected (metres).")]
        [SerializeField, Min(0f)] private float groundCheckDistance = 0.12f;
        [Tooltip("Layers that count as ground. Do NOT include the Player layer.")]
        [SerializeField] private LayerMask groundLayers = ~0;

        [Header("Mouse Look")]
        [Tooltip("Degrees per mouse pixel, horizontal.")]
        [SerializeField, Min(0f)] private float horizontalSensitivity = 0.12f;
        [Tooltip("Degrees per mouse pixel, vertical.")]
        [SerializeField, Min(0f)] private float verticalSensitivity = 0.12f;
        [Tooltip("Maximum up/down look angle in degrees.")]
        [SerializeField, Range(10f, 89f)] private float maxLookAngle = 85f;
        [SerializeField] private bool invertY = false;

        [Header("Head Bob")]
        [SerializeField] private bool headBobEnabled = true;
        [Tooltip("Steps per second while walking.")]
        [SerializeField, Min(0.1f)] private float walkBobFrequency = 1.9f;
        [Tooltip("Steps per second while sprinting.")]
        [SerializeField, Min(0.1f)] private float sprintBobFrequency = 2.6f;
        [Tooltip("Vertical bob amplitude (metres) while walking.")]
        [SerializeField, Min(0f)] private float walkBobVertical = 0.03f;
        [Tooltip("Vertical bob amplitude (metres) while sprinting.")]
        [SerializeField, Min(0f)] private float sprintBobVertical = 0.05f;
        [Tooltip("Sideways bob amplitude (metres) while walking.")]
        [SerializeField, Min(0f)] private float walkBobHorizontal = 0.015f;
        [Tooltip("Sideways bob amplitude (metres) while sprinting.")]
        [SerializeField, Min(0f)] private float sprintBobHorizontal = 0.03f;
        [Tooltip("How quickly the bob fades in/out and changes between walk and sprint.")]
        [SerializeField, Min(0.1f)] private float bobTransitionSpeed = 8f;

        [Header("Landing Effect")]
        [SerializeField] private bool landingEffectEnabled = true;
        [Tooltip("Camera dip strength. Higher = bigger dip.")]
        [SerializeField, Min(0f)] private float landingIntensity = 0.25f;
        [Tooltip("Falls slower than this (m/s) produce no landing effect.")]
        [SerializeField, Min(0f)] private float minLandingImpactSpeed = 3f;
        [Tooltip("Impact speeds are clamped to this value.")]
        [SerializeField, Min(0.1f)] private float maxLandingImpactSpeed = 18f;
        [SerializeField, Min(1f)] private float landingSpringStiffness = 80f;
        [SerializeField, Min(0f)] private float landingSpringDamping = 12f;

        [Header("Stamina (optional)")]
        [Tooltip("When disabled, the player can sprint forever.")]
        [SerializeField] private bool staminaEnabled = false;
        [SerializeField, Min(1f)] private float maxStamina = 100f;
        [Tooltip("Stamina lost per second while sprinting.")]
        [SerializeField, Min(0f)] private float staminaDrainPerSecond = 25f;
        [Tooltip("Stamina regained per second while not sprinting.")]
        [SerializeField, Min(0f)] private float staminaRegenPerSecond = 18f;
        [Tooltip("Seconds after sprinting before regeneration starts.")]
        [SerializeField, Min(0f)] private float staminaRegenDelay = 0.8f;
        [Tooltip("After running out, stamina must reach this value before sprinting again.")]
        [SerializeField, Min(0f)] private float minStaminaToSprint = 20f;

        // Movement
        public float WalkSpeed => walkSpeed;
        public float SprintSpeed => sprintSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public float AirControl => airControl;
        public float JumpHeight => jumpHeight;
        public float Gravity => gravity;
        public float TerminalVelocity => terminalVelocity;
        public float GroundStickVelocity => groundStickVelocity;
        // Ground
        public float GroundCheckRadius => groundCheckRadius;
        public float GroundCheckDistance => groundCheckDistance;
        public LayerMask GroundLayers => groundLayers;
        // Look
        public float HorizontalSensitivity => horizontalSensitivity;
        public float VerticalSensitivity => verticalSensitivity;
        public float MaxLookAngle => maxLookAngle;
        public bool InvertY => invertY;
        // Bob
        public bool HeadBobEnabled => headBobEnabled;
        public float WalkBobFrequency => walkBobFrequency;
        public float SprintBobFrequency => sprintBobFrequency;
        public float WalkBobVertical => walkBobVertical;
        public float SprintBobVertical => sprintBobVertical;
        public float WalkBobHorizontal => walkBobHorizontal;
        public float SprintBobHorizontal => sprintBobHorizontal;
        public float BobTransitionSpeed => bobTransitionSpeed;
        // Landing
        public bool LandingEffectEnabled => landingEffectEnabled;
        public float LandingIntensity => landingIntensity;
        public float MinLandingImpactSpeed => minLandingImpactSpeed;
        public float MaxLandingImpactSpeed => maxLandingImpactSpeed;
        public float LandingSpringStiffness => landingSpringStiffness;
        public float LandingSpringDamping => landingSpringDamping;
        // Stamina
        public bool StaminaEnabled => staminaEnabled;
        public float MaxStamina => maxStamina;
        public float StaminaDrainPerSecond => staminaDrainPerSecond;
        public float StaminaRegenPerSecond => staminaRegenPerSecond;
        public float StaminaRegenDelay => staminaRegenDelay;
        public float MinStaminaToSprint => minStaminaToSprint;
    }
}