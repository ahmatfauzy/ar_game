using UnityEngine;

public class ObjectInfoDisplay : MonoBehaviour
{
    [Header("Info Content")]
    [SerializeField] private string objectName = "Nanas (Pineapple)";
    [SerializeField] private string latinName = "Ananas comosus";
    [SerializeField] private string description = "Buah tropis yang kaya vitamin C dan enzim bromelain. Memiliki rasa manis-asam yang segar.";
    [SerializeField] private string origin = "Amerika Selatan";
    [SerializeField] private string nutrition = "Kalori: 50 kcal/100g\nVitamin C: 47.8 mg\nSerat: 1.4 g\nGula: 9.85 g";
    [SerializeField] private string funFact = "Nanas adalah satu-satunya sumber bromelain, enzim yang membantu pencernaan protein!";

    [Header("Panel Settings")]
    [SerializeField] private float panelWidth = 320f;
    [SerializeField] private float panelHeight = 420f;

    private bool showPanel = false;
    private Camera mainCamera;

    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private GUIStyle latinStyle;
    private GUIStyle labelStyle;
    private GUIStyle valueStyle;
    private GUIStyle closeButtonStyle;
    private GUIStyle funFactStyle;
    private bool stylesInitialized = false;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();

        if (mainCamera == null)
            Debug.LogError("[ObjectInfoDisplay] Camera tidak ditemukan!");
        else
            Debug.Log("[ObjectInfoDisplay] Camera ditemukan: " + mainCamera.name);

        // Auto-tambah MeshCollider jika belum ada collider sama sekali
        Collider col = GetComponentInChildren<Collider>();
        if (col == null)
        {
            MeshFilter mf = GetComponentInChildren<MeshFilter>();
            if (mf != null && mf.sharedMesh != null && mf.sharedMesh.vertexCount > 0)
            {
                MeshCollider mc = gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                // JANGAN pakai convex — batas 255 triangle, bikin gagal di HP
                // Non-convex MeshCollider tetap bisa di-raycast
                mc.convex = false;
                Debug.Log("[ObjectInfoDisplay] MeshCollider (" + mf.sharedMesh.vertexCount + " verts) ditambahkan ke " + gameObject.name);
            }
            else
            {
                BoxCollider bc = gameObject.AddComponent<BoxCollider>();
                bc.size = new Vector3(2f, 2f, 2f);
                bc.isTrigger = false;
                Debug.Log("[ObjectInfoDisplay] BoxCollider fallback ditambahkan ke " + gameObject.name);
            }
        }
        else
        {
            Debug.Log("[ObjectInfoDisplay] Collider sudah ada: " + col.GetType().Name);
        }
    }

    bool TryGetInput(out Vector2 pos)
    {
        pos = Vector2.zero;

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                pos = t.position;
                Debug.Log("[ObjectInfoDisplay] Touch di: " + pos);
                return true;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            pos = Input.mousePosition;
            // Jangan pakai mousePosition kalau posisinya (0,0) — itu tanda simulasi touch
            // yang salah di Android. Touch sudah ditangani di atas.
            if (pos.x > 0 || pos.y > 0)
            {
                Debug.Log("[ObjectInfoDisplay] Mouse di: " + pos);
                return true;
            }
        }

        return false;
    }

    void Update()
    {
        if (!TryGetInput(out Vector2 inputPos)) return;
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(inputPos);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Debug.Log("[ObjectInfoDisplay] Raycast HIT: " + hit.transform.name);

            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                showPanel = !showPanel;
                Debug.Log("[ObjectInfoDisplay] Panel: " + (showPanel ? "TAMPIL" : "SEMBUNYI"));
            }
            else
            {
                float px = (Screen.width - panelWidth) / 2f;
                float py = (Screen.height - panelHeight) / 2f;
                Rect panelRect = new Rect(px, py, panelWidth, panelHeight);
                Vector2 guiPos = new Vector2(inputPos.x, Screen.height - inputPos.y);
                if (!panelRect.Contains(guiPos))
                    showPanel = false;
            }
        }
        else
        {
            Debug.Log("[ObjectInfoDisplay] Raycast MISS");

            if (showPanel)
            {
                float px = (Screen.width - panelWidth) / 2f;
                float py = (Screen.height - panelHeight) / 2f;
                Rect panelRect = new Rect(px, py, panelWidth, panelHeight);
                Vector2 guiPos = new Vector2(inputPos.x, Screen.height - inputPos.y);
                if (!panelRect.Contains(guiPos))
                    showPanel = false;
            }
        }
    }

    void InitStyles()
    {
        if (stylesInitialized) return;

        panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.normal.background = MakeRoundedTexture(
            (int)panelWidth, (int)panelHeight,
            new Color(0.05f, 0.08f, 0.05f, 0.96f),
            new Color(0.2f, 0.55f, 0.15f, 1f),
            16, 3
        );
        panelStyle.padding = new RectOffset(20, 20, 20, 20);

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 22;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = new Color(0.4f, 0.95f, 0.3f);
        titleStyle.alignment = TextAnchor.MiddleCenter;

        latinStyle = new GUIStyle(GUI.skin.label);
        latinStyle.fontSize = 13;
        latinStyle.fontStyle = FontStyle.Italic;
        latinStyle.normal.textColor = new Color(0.6f, 0.85f, 0.55f, 0.85f);
        latinStyle.alignment = TextAnchor.MiddleCenter;

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 12;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = new Color(0.45f, 0.9f, 0.35f);

        valueStyle = new GUIStyle(GUI.skin.label);
        valueStyle.fontSize = 12;
        valueStyle.normal.textColor = new Color(0.88f, 0.95f, 0.85f);
        valueStyle.wordWrap = true;

        closeButtonStyle = new GUIStyle(GUI.skin.button);
        closeButtonStyle.fontSize = 14;
        closeButtonStyle.fontStyle = FontStyle.Bold;
        closeButtonStyle.normal.textColor = Color.white;
        closeButtonStyle.normal.background = MakeSolidTexture(new Color(0.75f, 0.15f, 0.1f, 0.9f));
        closeButtonStyle.hover.background = MakeSolidTexture(new Color(0.9f, 0.2f, 0.15f, 1f));
        closeButtonStyle.hover.textColor = Color.white;
        closeButtonStyle.border = new RectOffset(8, 8, 8, 8);

        funFactStyle = new GUIStyle(GUI.skin.label);
        funFactStyle.fontSize = 11;
        funFactStyle.fontStyle = FontStyle.Italic;
        funFactStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);
        funFactStyle.wordWrap = true;
        funFactStyle.alignment = TextAnchor.UpperCenter;

        stylesInitialized = true;
    }

    void OnGUI()
    {
        if (!showPanel) return;

        InitStyles();

        float panelX = (Screen.width - panelWidth) / 2f;
        float panelY = (Screen.height - panelHeight) / 2f;

        GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), GUIContent.none, panelStyle);

        GUILayout.BeginArea(new Rect(panelX + 16, panelY + 16, panelWidth - 32, panelHeight - 32));
        GUILayout.BeginVertical();

        GUILayout.Label("🍍 " + objectName, titleStyle, GUILayout.Height(32));
        GUILayout.Label(latinName, latinStyle, GUILayout.Height(20));

        GUILayout.Space(8);
        DrawHorizontalLine(new Color(0.3f, 0.7f, 0.2f, 0.6f));
        GUILayout.Space(8);

        GUILayout.Label("📋 Deskripsi", labelStyle);
        GUILayout.Label(description, valueStyle);
        GUILayout.Space(6);

        GUILayout.Label("🌍 Asal", labelStyle);
        GUILayout.Label(origin, valueStyle);
        GUILayout.Space(6);

        GUILayout.Label("🥗 Nutrisi (per 100g)", labelStyle);
        GUILayout.Label(nutrition, valueStyle);

        GUILayout.Space(8);
        DrawHorizontalLine(new Color(0.8f, 0.7f, 0.1f, 0.5f));
        GUILayout.Space(6);

        GUILayout.Label("💡 Fakta Unik", labelStyle);
        GUILayout.Label(funFact, funFactStyle);

        GUILayout.Space(10);

        if (GUILayout.Button("✕ Tutup", closeButtonStyle, GUILayout.Height(36)))
            showPanel = false;

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    void DrawHorizontalLine(Color color)
    {
        Texture2D lineTex = MakeSolidTexture(color);
        GUILayout.Box(GUIContent.none,
            new GUIStyle { normal = { background = lineTex }, fixedHeight = 1 },
            GUILayout.ExpandWidth(true), GUILayout.Height(1));
    }

    Texture2D MakeSolidTexture(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }

    Texture2D MakeRoundedTexture(int width, int height, Color fill, Color border, int radius, int borderWidth)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool inRounded = IsInRoundedRect(x, y, width, height, radius);
                bool onBorder = inRounded && !IsInRoundedRect(x, y, width, height, radius,
                    borderWidth, borderWidth, borderWidth, borderWidth);

                if (onBorder)
                    pixels[y * width + x] = border;
                else if (inRounded)
                    pixels[y * width + x] = fill;
                else
                    pixels[y * width + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    bool IsInRoundedRect(int x, int y, int w, int h, int r,
        int padLeft = 0, int padRight = 0, int padTop = 0, int padBottom = 0)
    {
        int x0 = padLeft, x1 = w - 1 - padRight;
        int y0 = padBottom, y1 = h - 1 - padTop;
        if (x < x0 || x > x1 || y < y0 || y > y1) return false;

        if (x < x0 + r && y < y0 + r) return Dist(x, y, x0 + r, y0 + r) <= r;
        if (x > x1 - r && y < y0 + r) return Dist(x, y, x1 - r, y0 + r) <= r;
        if (x < x0 + r && y > y1 - r) return Dist(x, y, x0 + r, y1 - r) <= r;
        if (x > x1 - r && y > y1 - r) return Dist(x, y, x1 - r, y1 - r) <= r;

        return true;
    }

    float Dist(int x1, int y1, int x2, int y2)
    {
        return Mathf.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
    }
}