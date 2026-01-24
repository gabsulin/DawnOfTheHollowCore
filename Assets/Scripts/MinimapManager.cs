using UnityEngine;
using UnityEngine.UI;

public class MinimapManager : MonoBehaviour
{
    [Header("References")]
    public Camera minimapCamera;
    public RenderTexture renderTexture;
    public RawImage minimapUI;
    public Transform mapCore;
    public Transform playerTransform;

    [Header("Camera Settings - 2D Top-Down")]
    [Tooltip("Z pozice kamery (negativní = pøed mapou)")]
    public float cameraZ = -100f;

    [Tooltip("Velikost viditelné oblasti")]
    public float orthographicSize = 50f;

    [Tooltip("Automaticky vypoèítat velikost podle nejvìtší zóny")]
    public bool autoCalculateSize = true;

    [Header("Layers")]
    public LayerMask minimapLayers;

    [Header("Player Icon")]
    public GameObject playerIconPrefab;
    private GameObject playerIcon;

    [Header("Zoom")]
    public bool allowZoom = false;
    [Range(0.5f, 3f)]
    public float zoomLevel = 1f;
    private float baseOrthographicSize;

    void Start()
    {
        SetupMinimapCamera();
        SetupPlayerIcon();

        if (autoCalculateSize)
        {
            AutoCalculateCameraSize();
        }

        baseOrthographicSize = orthographicSize;
    }

    void SetupMinimapCamera()
    {
        if (minimapCamera == null)
        {
            minimapCamera = GameObject.Find("MinimapCamera")?.GetComponent<Camera>();

            if (minimapCamera == null)
            {
                Debug.LogError("MinimapCamera není pøiøazena! Vytvoø GameObject 'MinimapCamera' s Camera komponentou.");
                return;
            }
        }

        // Nastav pozici kamery - 2D top-down (žádná rotace!)
        Vector3 corePosition = mapCore != null ? mapCore.position : Vector3.zero;
        minimapCamera.transform.position = new Vector3(corePosition.x, corePosition.y, cameraZ);
        minimapCamera.transform.rotation = Quaternion.identity; // Žádná rotace!

        // Nastav kameru pro 2D
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = orthographicSize;
        minimapCamera.cullingMask = minimapLayers;

        // Unity 6 - Background Type
        minimapCamera.clearFlags = CameraClearFlags.SolidColor;
        minimapCamera.backgroundColor = Color.black;

        // Nastav near/far clipping
        minimapCamera.nearClipPlane = 0.1f;
        minimapCamera.farClipPlane = Mathf.Abs(cameraZ) + 10f;

        // Pøiøaï RenderTexture
        if (renderTexture != null)
        {
            minimapCamera.targetTexture = renderTexture;

            if (minimapUI != null)
            {
                minimapUI.texture = renderTexture;
            }
        }
        else
        {
            Debug.LogError("RenderTexture není pøiøazena! Vytvoø RenderTexture a pøiøaï ji.");
        }
    }

    void AutoCalculateCameraSize()
    {
        MinimapZoneRenderer zoneRenderer = FindFirstObjectByType<MinimapZoneRenderer>();

        if (zoneRenderer != null && zoneRenderer.zones.Count > 0)
        {
            float maxRadius = 0f;
            foreach (var zone in zoneRenderer.zones)
            {
                if (zone.radius > maxRadius)
                {
                    maxRadius = zone.radius;
                }
            }

            // Pøidej 10% padding
            orthographicSize = maxRadius * 1.1f;

            if (minimapCamera != null)
            {
                minimapCamera.orthographicSize = orthographicSize;
            }

            Debug.Log($"[Minimap] Auto-calculated camera size: {orthographicSize}");
        }
        else
        {
            Debug.LogWarning("[Minimap] MinimapZoneRenderer nebyl nalezen pro auto-calculation!");
        }
    }

    void SetupPlayerIcon()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("[Minimap] Player transform není pøiøazen. Oznaè hráèe tagem 'Player'.");
                return;
            }
        }

        // Smaž všechny existující ikony (pokud tam jsou z pøedchozího bìhu)
        foreach (Transform child in playerTransform)
        {
            if (child.name == "PlayerIcon_Minimap")
            {
                Destroy(child.gameObject);
            }
        }

        if (playerIconPrefab != null)
        {
            playerIcon = Instantiate(playerIconPrefab, playerTransform.position, Quaternion.identity);
            playerIcon.layer = LayerMask.NameToLayer("Player");
            playerIcon.transform.SetParent(playerTransform);
            playerIcon.transform.localPosition = new Vector3(0, 0, -10);
        }
        else
        {
            CreateDefaultPlayerIcon();
        }
    }

    void CreateDefaultPlayerIcon()
    {
        playerIcon = new GameObject("PlayerIcon_Minimap");
        playerIcon.transform.SetParent(playerTransform);

        // DÙLEŽITÉ: Z pozice = -10 (stejnì jako hlavní kamera pro 2D top-down)
        playerIcon.transform.localPosition = new Vector3(0, 0, -10);

        playerIcon.layer = LayerMask.NameToLayer("Player");

        // SpriteRenderer pro ikonu
        SpriteRenderer sr = playerIcon.AddComponent<SpriteRenderer>();

        // Použij vestavìný Unity sprite (Circle)
        // Alternativa: Resources.Load<Sprite>("Circle") nebo vlastní sprite
        Sprite circleSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
        if (circleSprite == null)
        {
            // Fallback: vytvoø sprite programovì
            circleSprite = CreateCircleSprite(64);
        }

        sr.sprite = circleSprite;
        sr.color = Color.yellow; // Žlutá barva pro lepší viditelnost
        sr.sortingOrder = 100;

        // Škálování podle velikosti mapy
        float iconSize = orthographicSize * 0.05f;
        playerIcon.transform.localScale = Vector3.one * iconSize;

        Debug.Log($"[Minimap] Player icon created: Sprite={sr.sprite?.name}, Layer={playerIcon.layer}, LocalPos={playerIcon.transform.localPosition}");
    }

    Sprite CreateCircleSprite(int resolution)
    {
        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[resolution * resolution];

        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float radius = resolution / 2f - 1; // -1 aby nebyl oøíznutý

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                // Vyhlazený okraj
                float alpha = distance <= radius ? 1f : 0f;
                if (distance > radius - 2f && distance <= radius)
                {
                    alpha = (radius - distance) / 2f;
                }
                pixels[y * resolution + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), 100f);
        return sprite;
    }

    void Update()
    {
        if (allowZoom && minimapCamera != null)
        {
            minimapCamera.orthographicSize = baseOrthographicSize / zoomLevel;
        }
    }

    // Volitelné: sledování hráèe (mapa se posune s hráèem)
    public void EnableFollowPlayer(bool follow)
    {
        if (follow)
        {
            InvokeRepeating(nameof(FollowPlayer), 0f, 0.05f);
        }
        else
        {
            CancelInvoke(nameof(FollowPlayer));
        }
    }

    void FollowPlayer()
    {
        if (playerTransform != null && minimapCamera != null)
        {
            Vector3 newPos = new Vector3(
                playerTransform.position.x,
                playerTransform.position.y,
                cameraZ
            );
            minimapCamera.transform.position = newPos;
        }
    }

    // Veøejné metody
    public void SetZoom(float zoom)
    {
        zoomLevel = Mathf.Clamp(zoom, 0.5f, 3f);
    }

    public void SetMinimapVisibility(bool visible)
    {
        if (minimapUI != null)
        {
            minimapUI.gameObject.SetActive(visible);
        }
    }

    void OnDrawGizmos()
    {
        if (minimapCamera != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 camPos = minimapCamera.transform.position;
            Vector3 size = new Vector3(orthographicSize * 2f, orthographicSize * 2f, 0.1f);
            Gizmos.DrawWireCube(new Vector3(camPos.x, camPos.y, 0), size);
        }
    }
}   