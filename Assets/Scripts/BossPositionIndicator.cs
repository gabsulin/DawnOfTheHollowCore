using UnityEngine;

public class BossPositionIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform boss;
    [SerializeField] private Transform player;
    [SerializeField] private RectTransform indicatorArrow;

    [Header("Settings")]
    [SerializeField] private float edgeOffset = 50f;
    [SerializeField] private bool onlyShowWhenOffscreen = true;
    [SerializeField] private float screenPadding = 100f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        indicatorArrow.gameObject.SetActive(false);
    }

    public void SetBoss(Transform newBoss)
    {
        boss = newBoss;
        indicatorArrow.gameObject.SetActive(boss != null);

        if (enableDebugLogs)
            Debug.Log(boss != null
                ? "<color=green>Boss assigned to indicator</color>"
                : "<color=orange>Boss cleared from indicator</color>");
    }

    void Update()
    {
        if (boss == null || player == null || indicatorArrow == null)
            return;

        bool isOffScreen = IsBossOffScreen();

        if (onlyShowWhenOffscreen)
            indicatorArrow.gameObject.SetActive(isOffScreen);

        if (indicatorArrow.gameObject.activeSelf)
            UpdateIndicatorPositionAndRotation();
    }

    bool IsBossOffScreen()
    {
        Vector3 vp = mainCamera.WorldToViewportPoint(boss.position);

        if (vp.z < 0) return true;

        float padX = screenPadding / Screen.width;
        float padY = screenPadding / Screen.height;

        return vp.x < -padX || vp.x > 1 + padX ||
               vp.y < -padY || vp.y > 1 + padY;
    }

    void UpdateIndicatorPositionAndRotation()
    {
        Vector3 dir = (boss.position - player.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        indicatorArrow.rotation = Quaternion.Euler(0, 0, angle - 90f);

        Vector2 center = new(Screen.width / 2f, Screen.height / 2f);
        Vector2 bounds = center - Vector2.one * edgeOffset;

        Vector2 screenDir = new(dir.x, dir.y);

        float scaleX = Mathf.Abs(screenDir.x) > 0.01f ? bounds.x / Mathf.Abs(screenDir.x) : float.MaxValue;
        float scaleY = Mathf.Abs(screenDir.y) > 0.01f ? bounds.y / Mathf.Abs(screenDir.y) : float.MaxValue;

        float scale = Mathf.Min(scaleX, scaleY);
        indicatorArrow.position = center + screenDir * scale;
    }
}
