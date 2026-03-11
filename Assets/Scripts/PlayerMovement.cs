using UnityEngine;
using System.Globalization;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    private Rigidbody2D rb;
    private Vector2 movement;

    [Header("Sprite")]
    private SpriteRenderer spriteRenderer;
    public Sprite fallbackSprite; // white square or test-sprite

    private Sprite[] idleSprites = new Sprite[16];
    private Sprite[,] walkSprites = new Sprite[16, 8];
    private readonly string[] directions = new string[] { "E", "NEE", "NE", "NNE", "N", "NNW", "NW", "NWW", "W", "SWW", "SW", "SSW", "S", "SSE", "SE", "SEE" };

    private int directionIndex = 0;

    private float animTimer = 0f;
    private float animInterval = 0.1f; // 100ms per frame
    private int currentFrame = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on the player!");
            return;
        }

        if (fallbackSprite == null)
        {
            Debug.LogError("Fallback sprite missing! Assign a white square or placeholder sprite in the Inspector.");
            return;
        }

        spriteRenderer.sprite = fallbackSprite;

        // Initialize all idle sprites with fallback sprite
        for (int i = 0; i < idleSprites.Length; i++)
        {
            idleSprites[i] = fallbackSprite;
        }

        // Initialize all walk sprites with fallback sprite
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                walkSprites[i, j] = fallbackSprite;
            }
        }

        // Load all sprites from the folder
        Sprite[] loadedSprites = Resources.LoadAll<Sprite>("warrior_armed_idle");
        Debug.Log($"Loaded {loadedSprites.Length} idle sprites from Resources/warrior_armed_idle");

        // Assign idle sprites based on the direction key from the name
        foreach (Sprite sprite in loadedSprites)
        {
            string[] parts = sprite.name.Split('_');

            // Expected format for idle: warrior_armed_idle_DIRECTION_ANGLE or warrior_armed_idle_DIRECTION
            if (parts.Length >= 4 && parts[2] == "idle")
            {
                string directionKey = parts[3];
                int index = -1;
                for (int i = 0; i < directions.Length; i++)
                {
                    if (directions[i] == directionKey)
                    {
                        index = i;
                        break;
                    }
                }
                if (index >= 0)
                {
                    idleSprites[index] = sprite;
                }
            }
        }

        Sprite firstLoadedIdleSprite = null;
        for (int i = 0; i < idleSprites.Length; i++)
        {
            if (idleSprites[i] != fallbackSprite)
            {
                firstLoadedIdleSprite = idleSprites[i];
                break;
            }
        }
        if (firstLoadedIdleSprite != null)
        {
            spriteRenderer.sprite = firstLoadedIdleSprite;
        }
        else
        {
            spriteRenderer.sprite = fallbackSprite;
        }

        // Load walk sprites from the folder "warrior_armed_walk"
        Sprite[] loadedWalkSprites = Resources.LoadAll<Sprite>("warrior_armed_walk");
        Debug.Log($"Loaded {loadedWalkSprites.Length} walk sprites from Resources/warrior_armed_walk");

        int[] framesLoadedPerDirection = new int[directions.Length];

        foreach (Sprite sprite in loadedWalkSprites)
        {
            string[] parts = sprite.name.Split('_');

            // Expected format: warrior_armed_walk_DIRECTION_... (ignore frame index parsing)
            if (parts.Length >= 4 && parts[2] == "walk")
            {
                string directionKey = parts[3];

                int dirIndex = -1;
                for (int i = 0; i < directions.Length; i++)
                {
                    if (directions[i] == directionKey)
                    {
                        dirIndex = i;
                        break;
                    }
                }

                if (dirIndex < 0)
                {
                    Debug.LogWarning($"Walk sprite '{sprite.name}' direction '{directionKey}' does not match any known direction.");
                    continue;
                }

                int frameIndex = framesLoadedPerDirection[dirIndex];
                if (frameIndex < 8)
                {
                    walkSprites[dirIndex, frameIndex] = sprite;
                    framesLoadedPerDirection[dirIndex]++;
                }
            }
        }

        for (int i = 0; i < directions.Length; i++)
        {
            Debug.Log($"Direction {directions[i]}: Loaded {framesLoadedPerDirection[i]} walk frames");
        }

        // Place the player at (0,0) and ensure it has a visible scale
        transform.position = Vector3.zero;
        transform.localScale = Vector3.one;

        // Ensure the sprite is visible
        spriteRenderer.sortingLayerName = "Default";
        spriteRenderer.sortingOrder = 100;
    }

    void Update()
    {
        float hInput = Input.GetAxisRaw("Horizontal");
        float vInput = Input.GetAxisRaw("Vertical");

        movement = new Vector2(hInput, vInput);

        if (movement != Vector2.zero)
        {
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            // Adjust angle by 11.25 degrees to center each direction sector, then divide by 22.5 degrees
            float adjustedAngle = (angle + 11.25f) % 360f;
            directionIndex = Mathf.FloorToInt(adjustedAngle / 22.5f);

            animTimer += Time.deltaTime;
            if (animTimer >= animInterval)
            {
                animTimer -= animInterval;
                currentFrame = (currentFrame + 1) % 8;
            }

            spriteRenderer.sprite = walkSprites[directionIndex, currentFrame];
        }
        else
        {
            animTimer = 0f;
            currentFrame = 0;

            spriteRenderer.sprite = idleSprites[directionIndex];
        }
    }

    void FixedUpdate()
    {
        // Move the player
        if (movement != Vector2.zero)
        {
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
    }
}