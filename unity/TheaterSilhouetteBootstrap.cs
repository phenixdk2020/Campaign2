using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// campaign-map v00.00.01-silhouette
/// Requires Assets/Resources/CampaignMap/rings.txt
/// </summary>
[DefaultExecutionOrder(-50)]
public sealed class TheaterSilhouetteBootstrap : MonoBehaviour
{
    public bool hideExistingMapRenderers = true;
    public Color landColor = new Color(0.44f, 0.50f, 0.34f);
    public Color waterColor = new Color(0.11f, 0.19f, 0.24f);
    public int textureWidth = 1024;

    static readonly (string name, float x, float z)[] Places =
    {
        ("Flensborg", -74.142f, -141.704f),
        ("Dybbøl", -56.214f, -127.643f),
        ("Sønderborg", -52.054f, -126.930f),
        ("Slesvig", -66.055f, -171.356f),
        ("Haderslev", -71.935f, -89.163f),
        ("Kolding", -73.004f, -62.795f),
        ("Fredericia", -55.339f, -54.403f),
        ("Vejle", -69.211f, -38.505f),
        ("Odense", -14.918f, -72.591f),
        ("Aarhus", -28.178f, 12.433f),
        ("Aalborg", -46.954f, 110.778f),
        ("København", 121.443f, -36.536f),
    };

    void Start()
    {
        var polys = LoadRings();
        if (polys == null || polys.Count == 0)
        {
            Debug.LogError("[campaign-map v00.00.01] Mangler Resources/CampaignMap/rings.txt");
            return;
        }

        if (hideExistingMapRenderers)
        {
            foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if (r == null || r.GetComponentInParent<TheaterSilhouetteBootstrap>() != null) continue;
                r.enabled = false;
            }
        }

        var root = new GameObject("TheaterSilhouette_v0001");
        root.transform.SetParent(transform, false);
        BuildWater(root.transform);
        BuildLand(root.transform, polys);
        BuildPlaces(root.transform);
        FrameCamera();
        Debug.Log("[campaign-map v00.00.01] Denmark+Slesvig silhouette klar. Forvent Jylland, Fyn, Sjælland, Als. Ikke Stockholm.");
    }

    static List<List<Vector2[]>> LoadRings()
    {
        var asset = Resources.Load<TextAsset>("CampaignMap/rings");
        if (asset == null) return null;

        var polys = new List<List<Vector2[]>>();
        List<Vector2[]> currentPoly = null;
        List<Vector2> currentRing = null;

        foreach (var raw in asset.text.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;
            if (line == "POLY")
            {
                currentPoly = new List<Vector2[]>();
                currentRing = new List<Vector2>();
                continue;
            }
            if (line == "HOLE")
            {
                FlushRing(currentPoly, ref currentRing);
                currentRing = new List<Vector2>();
                continue;
            }
            if (line == "END")
            {
                FlushRing(currentPoly, ref currentRing);
                if (currentPoly != null && currentPoly.Count > 0) polys.Add(currentPoly);
                currentPoly = null;
                continue;
            }
            var parts = line.Split(' ');
            if (parts.Length < 2 || currentRing == null) continue;
            if (float.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var x) &&
                float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var z))
                currentRing.Add(new Vector2(x, z));
        }
        return polys;
    }

    static void FlushRing(List<Vector2[]> poly, ref List<Vector2> ring)
    {
        if (poly != null && ring != null && ring.Count >= 3) poly.Add(ring.ToArray());
        ring = null;
    }

    void BuildWater(Transform parent)
    {
        var water = GameObject.CreatePrimitive(PrimitiveType.Quad);
        water.name = "Water";
        water.transform.SetParent(parent, false);
        water.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        water.transform.localScale = new Vector3(380f, 430f, 1f);
        var mr = water.GetComponent<MeshRenderer>();
        mr.sharedMaterial = NewColor(waterColor);
        Object.Destroy(water.GetComponent<Collider>());
    }

    void BuildLand(Transform parent, List<List<Vector2[]>> polys)
    {
        float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
        foreach (var poly in polys)
        {
            foreach (var ring in poly)
            {
                foreach (var p in ring)
                {
                    if (p.x < minX) minX = p.x;
                    if (p.x > maxX) maxX = p.x;
                    if (p.y < minZ) minZ = p.y;
                    if (p.y > maxZ) maxZ = p.y;
                }
            }
        }

        minX -= 8f; maxX += 8f; minZ -= 8f; maxZ += 8f;
        float worldW = maxX - minX;
        float worldH = maxZ - minZ;
        int tw = textureWidth;
        int th = Mathf.Max(32, Mathf.RoundToInt(tw * (worldH / worldW)));
        var tex = new Texture2D(tw, th, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        tex.SetPixels(new Color[tw * th]);

        foreach (var poly in polys)
        {
            Rasterize(tex, poly[0], minX, minZ, worldW, worldH, landColor);
            for (int i = 1; i < poly.Count; i++)
                Rasterize(tex, poly[i], minX, minZ, worldW, worldH, Color.clear);
        }
        tex.Apply();

        var land = GameObject.CreatePrimitive(PrimitiveType.Quad);
        land.name = "Land";
        land.transform.SetParent(parent, false);
        land.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        land.transform.position = new Vector3((minX + maxX) * 0.5f, 0.05f, (minZ + maxZ) * 0.5f);
        land.transform.localScale = new Vector3(worldW, worldH, 1f);
        var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Transparent") ?? Shader.Find("Unlit/Texture") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        mat.color = Color.white;
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        land.GetComponent<MeshRenderer>().sharedMaterial = mat;
        Object.Destroy(land.GetComponent<Collider>());
    }

    static void Rasterize(Texture2D tex, Vector2[] ring, float minX, float minZ, float worldW, float worldH, Color color)
    {
        int tw = tex.width, th = tex.height;
        var pts = new Vector2[ring.Length];
        int minPy = th - 1, maxPy = 0;
        for (int i = 0; i < ring.Length; i++)
        {
            pts[i] = new Vector2(
                (ring[i].x - minX) / worldW * (tw - 1),
                (ring[i].y - minZ) / worldH * (th - 1));
            int py = Mathf.Clamp(Mathf.RoundToInt(pts[i].y), 0, th - 1);
            if (py < minPy) minPy = py;
            if (py > maxPy) maxPy = py;
        }

        for (int y = minPy; y <= maxPy; y++)
        {
            var xs = new List<float>();
            for (int i = 0; i < pts.Length; i++)
            {
                var a = pts[i];
                var b = pts[(i + 1) % pts.Length];
                if ((a.y <= y && b.y > y) || (b.y <= y && a.y > y))
                    xs.Add(a.x + (y - a.y) / (b.y - a.y) * (b.x - a.x));
            }
            xs.Sort();
            for (int i = 0; i + 1 < xs.Count; i += 2)
            {
                int x0 = Mathf.Clamp(Mathf.FloorToInt(xs[i]), 0, tw - 1);
                int x1 = Mathf.Clamp(Mathf.CeilToInt(xs[i + 1]), 0, tw - 1);
                for (int x = x0; x <= x1; x++) tex.SetPixel(x, y, color);
            }
        }
    }

    void BuildPlaces(Transform parent)
    {
        var holder = new GameObject("Places");
        holder.transform.SetParent(parent, false);
        foreach (var p in Places)
        {
            var go = new GameObject(p.name);
            go.transform.SetParent(holder.transform, false);
            go.transform.position = new Vector3(p.x, 0.4f, p.z);
            var tm = go.AddComponent<TextMesh>();
            tm.text = p.name;
            tm.characterSize = 0.35f;
            tm.fontSize = 32;
            tm.anchor = TextAnchor.LowerLeft;
            tm.color = new Color(0.25f, 0.08f, 0.08f);
            var dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.transform.SetParent(go.transform, false);
            dot.transform.localPosition = new Vector3(-0.6f, 0f, 0.2f);
            dot.transform.localScale = Vector3.one * 0.7f;
            dot.GetComponent<MeshRenderer>().sharedMaterial = NewColor(new Color(0.55f, 0.12f, 0.12f));
            Object.Destroy(dot.GetComponent<Collider>());
        }
    }

    static Material NewColor(Color c)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard"));
        mat.color = c;
        return mat;
    }

    static void FrameCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;
        cam.transform.position = new Vector3(0f, 280f, -40f);
        cam.transform.rotation = Quaternion.Euler(72f, 0f, 0f);
        cam.farClipPlane = 2000f;
    }
}
