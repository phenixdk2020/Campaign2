using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// campaign-map v00.00.02-atlas
/// Grand Tactician / HOI-inspired table map: parchment land, ink water, dark coast.
/// Requires Assets/Resources/CampaignMap/rings.txt
/// No Stockholm. No fake volcanoes.
/// </summary>
[DefaultExecutionOrder(-50)]
public sealed class TheaterAtlasBootstrap : MonoBehaviour
{
    public bool hideExistingMapRenderers = true;
    public int textureWidth = 1024;
    public float reliefMeters = 2.2f;
    public bool showStaffGrid = false;

    static readonly Color WaterDeep = new Color(0.11f, 0.20f, 0.24f);
    static readonly Color WaterShallow = new Color(0.27f, 0.40f, 0.41f);
    static readonly Color LandField = new Color(0.70f, 0.62f, 0.44f);
    static readonly Color LandWood = new Color(0.36f, 0.42f, 0.27f);
    static readonly Color CoastInk = new Color(0.16f, 0.12f, 0.09f);
    static readonly Color GridInk = new Color(0.22f, 0.18f, 0.12f, 0.18f);

    static readonly (string name, float x, float z)[] Places =
    {
        ("Flensborg", -74.142f, -141.704f),
        ("Dybbøl", -56.214f, -127.643f),
        ("Sønderborg", -52.054f, -126.930f),
        ("Danevirke", -66.055f, -171.356f),
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
            Debug.LogError("[campaign-map v00.00.02] Mangler Resources/CampaignMap/rings.txt");
            return;
        }

        if (hideExistingMapRenderers)
        {
            foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if (r == null || r.GetComponentInParent<TheaterAtlasBootstrap>() != null) continue;
                r.enabled = false;
            }
            foreach (var tm in FindObjectsByType<TextMesh>(FindObjectsSortMode.None))
            {
                if (tm.GetComponentInParent<TheaterAtlasBootstrap>() != null) continue;
                tm.gameObject.SetActive(false);
            }
        }

        var root = new GameObject("TheaterAtlas_v0002");
        root.transform.SetParent(transform, false);

        Bounds2 b = Measure(polys);
        var landMask = RasterLand(polys, b, out int tw, out int th);
        var tex = PaintAtlas(landMask, tw, th);
        BuildWater(root.transform, b);
        BuildLandMesh(root.transform, b, landMask, tw, th, tex);
        if (showStaffGrid) BuildGrid(root.transform, b);
        BuildPlaces(root.transform);
        FrameCamera();
        Debug.Log("[campaign-map v00.00.02-atlas] Parchment/ink Denmark. Ingen Stockholm. Relief kun som bord-højde.");
    }

    struct Bounds2
    {
        public float minX, maxX, minZ, maxZ;
        public float W => maxX - minX;
        public float H => maxZ - minZ;
        public Vector3 Center => new Vector3((minX + maxX) * 0.5f, 0f, (minZ + maxZ) * 0.5f);
    }

    static Bounds2 Measure(List<List<Vector2[]>> polys)
    {
        var b = new Bounds2 { minX = 9999, maxX = -9999, minZ = 9999, maxZ = -9999 };
        foreach (var poly in polys)
        foreach (var ring in poly)
        foreach (var p in ring)
        {
            if (p.x < b.minX) b.minX = p.x;
            if (p.x > b.maxX) b.maxX = p.x;
            if (p.y < b.minZ) b.minZ = p.y;
            if (p.y > b.maxZ) b.maxZ = p.y;
        }
        b.minX -= 10f; b.maxX += 10f; b.minZ -= 10f; b.maxZ += 10f;
        return b;
    }

    bool[] RasterLand(List<List<Vector2[]>> polys, Bounds2 b, out int tw, out int th)
    {
        tw = textureWidth;
        th = Mathf.Max(32, Mathf.RoundToInt(tw * (b.H / b.W)));
        var mask = new bool[tw * th];
        var tmp = new Texture2D(tw, th, TextureFormat.RGBA32, false);
        tmp.SetPixels(new Color[tw * th]);
        foreach (var poly in polys)
        {
            Fill(tmp, poly[0], b, Color.white);
            for (int i = 1; i < poly.Count; i++) Fill(tmp, poly[i], b, Color.clear);
        }
        var cols = tmp.GetPixels();
        for (int i = 0; i < cols.Length; i++) mask[i] = cols[i].a > 0.5f;
        Destroy(tmp);
        return mask;
    }

    Texture2D PaintAtlas(bool[] land, int tw, int th)
    {
        var tex = new Texture2D(tw, th, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        var cols = new Color[tw * th];
        for (int y = 0; y < th; y++)
        {
            for (int x = 0; x < tw; x++)
            {
                int i = y * tw + x;
                float n = Hash(x, y);
                float n2 = Hash(x + 19, y + 7);
                if (!land[i])
                {
                    float near = NeighborLand(land, tw, th, x, y, 10);
                    cols[i] = Color.Lerp(WaterDeep, WaterShallow, near * 0.85f + n * 0.04f);
                }
                else
                {
                    float edge = 1f - NeighborLand(land, tw, th, x, y, 8);
                    float wood = Mathf.Clamp01((n2 - 0.55f) * 2.4f);
                    var ground = Color.Lerp(LandField, LandWood, wood * 0.55f);
                    ground *= 1f - edge * 0.28f;
                    ground.r += (n - 0.5f) * 0.03f;
                    ground.g += (n - 0.5f) * 0.025f;
                    cols[i] = ground;
                }
            }
        }

        for (int y = 1; y < th - 1; y++)
        {
            for (int x = 1; x < tw - 1; x++)
            {
                int i = y * tw + x;
                bool c = land[i];
                if (c != land[i - 1] || c != land[i + 1] || c != land[i - tw] || c != land[i + tw])
                    cols[i] = Color.Lerp(cols[i], CoastInk, 0.78f);
            }
        }

        tex.SetPixels(cols);
        tex.Apply();
        return tex;
    }

    static float NeighborLand(bool[] land, int tw, int th, int x, int y, int r)
    {
        int hit = 0, n = 0;
        for (int dy = -r; dy <= r; dy += 2)
        {
            int yy = y + dy;
            if ((uint)yy >= (uint)th) continue;
            for (int dx = -r; dx <= r; dx += 2)
            {
                int xx = x + dx;
                if ((uint)xx >= (uint)tw) continue;
                n++;
                if (land[yy * tw + xx]) hit++;
            }
        }
        return n == 0 ? 0f : hit / (float)n;
    }

    static float Hash(int x, int y)
    {
        uint n = (uint)(x * 374761393 + y * 668265263);
        n = (n ^ (n >> 13)) * 1274126177u;
        return (n & 0xFFFF) / 65535f;
    }

    void BuildWater(Transform parent, Bounds2 b)
    {
        var water = GameObject.CreatePrimitive(PrimitiveType.Quad);
        water.name = "Water";
        water.transform.SetParent(parent, false);
        water.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        water.transform.position = b.Center + new Vector3(0f, -0.15f, 0f);
        water.transform.localScale = new Vector3(b.W + 40f, b.H + 40f, 1f);
        water.GetComponent<MeshRenderer>().sharedMaterial = Unlit(WaterDeep);
        Destroy(water.GetComponent<Collider>());
    }

    void BuildLandMesh(Transform parent, Bounds2 b, bool[] land, int tw, int th, Texture tex)
    {
        int gx = 96;
        int gz = Mathf.Max(16, Mathf.RoundToInt(96f * b.H / b.W));
        var mesh = new Mesh { name = "AtlasLand" };
        var verts = new Vector3[(gx + 1) * (gz + 1)];
        var uvs = new Vector2[verts.Length];
        var tris = new List<int>();
        for (int z = 0; z <= gz; z++)
        {
            float vz = Mathf.Lerp(b.minZ, b.maxZ, z / (float)gz);
            for (int x = 0; x <= gx; x++)
            {
                float vx = Mathf.Lerp(b.minX, b.maxX, x / (float)gx);
                int ix = Mathf.Clamp(Mathf.RoundToInt(x / (float)gx * (tw - 1)), 0, tw - 1);
                int iz = Mathf.Clamp(Mathf.RoundToInt(z / (float)gz * (th - 1)), 0, th - 1);
                bool isLand = land[iz * tw + ix];
                float inland = isLand ? NeighborLand(land, tw, th, ix, iz, 12) : 0f;
                float y = isLand ? 0.08f + inland * reliefMeters : -0.2f;
                int i = z * (gx + 1) + x;
                verts[i] = new Vector3(vx, y, vz);
                uvs[i] = new Vector2(x / (float)gx, z / (float)gz);
            }
        }
        for (int z = 0; z < gz; z++)
        {
            for (int x = 0; x < gx; x++)
            {
                int i = z * (gx + 1) + x;
                tris.Add(i); tris.Add(i + gx + 1); tris.Add(i + 1);
                tris.Add(i + 1); tris.Add(i + gx + 1); tris.Add(i + gx + 2);
            }
        }
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var go = new GameObject("Land");
        go.transform.SetParent(parent, false);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        mat.color = Color.white;
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
    }

    void BuildGrid(Transform parent, Bounds2 b)
    {
        var grid = new GameObject("StaffGrid");
        grid.transform.SetParent(parent, false);
        var lr = grid.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = false;
        lr.widthMultiplier = 0.18f;
        lr.material = Unlit(GridInk);
        lr.startColor = GridInk;
        lr.endColor = GridInk;
        var pts = new List<Vector3>();
        for (float x = Mathf.Ceil(b.minX / 20f) * 20f; x < b.maxX; x += 20f)
        {
            pts.Add(new Vector3(x, 0.35f, b.minZ));
            pts.Add(new Vector3(x, 0.35f, b.maxZ));
        }
        for (float z = Mathf.Ceil(b.minZ / 20f) * 20f; z < b.maxZ; z += 20f)
        {
            pts.Add(new Vector3(b.minX, 0.35f, z));
            pts.Add(new Vector3(b.maxX, 0.35f, z));
        }
        lr.positionCount = pts.Count;
        lr.SetPositions(pts.ToArray());
    }

    void BuildPlaces(Transform parent)
    {
        var holder = new GameObject("Places");
        holder.transform.SetParent(parent, false);
        foreach (var p in Places)
        {
            var go = new GameObject(p.name);
            go.transform.SetParent(holder.transform, false);
            go.transform.position = new Vector3(p.x, 3.2f, p.z);
            var tm = go.AddComponent<TextMesh>();
            tm.text = p.name;
            tm.characterSize = 0.42f;
            tm.fontSize = 36;
            tm.anchor = TextAnchor.LowerLeft;
            tm.color = new Color(0.20f, 0.12f, 0.07f);
            var dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.transform.SetParent(go.transform, false);
            dot.transform.localPosition = new Vector3(-0.7f, 0.1f, 0.15f);
            dot.transform.localScale = Vector3.one * 0.85f;
            dot.GetComponent<MeshRenderer>().sharedMaterial = Unlit(new Color(0.45f, 0.12f, 0.10f));
            Destroy(dot.GetComponent<Collider>());
        }
    }

    static void Fill(Texture2D tex, Vector2[] ring, Bounds2 b, Color color)
    {
        int tw = tex.width, th = tex.height;
        var pts = new Vector2[ring.Length];
        int minPy = th - 1, maxPy = 0;
        for (int i = 0; i < ring.Length; i++)
        {
            pts[i] = new Vector2(
                (ring[i].x - b.minX) / b.W * (tw - 1),
                (ring[i].y - b.minZ) / b.H * (th - 1));
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
                var b2 = pts[(i + 1) % pts.Length];
                if ((a.y <= y && b2.y > y) || (b2.y <= y && a.y > y))
                    xs.Add(a.x + (y - a.y) / (b2.y - a.y) * (b2.x - a.x));
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
                Flush(currentPoly, ref currentRing);
                currentRing = new List<Vector2>();
                continue;
            }
            if (line == "END")
            {
                Flush(currentPoly, ref currentRing);
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

    static void Flush(List<Vector2[]> poly, ref List<Vector2> ring)
    {
        if (poly != null && ring != null && ring.Count >= 3) poly.Add(ring.ToArray());
        ring = null;
    }

    static Material Unlit(Color c)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard"));
        mat.color = c;
        return mat;
    }

    static void FrameCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;
        cam.transform.position = new Vector3(10f, 240f, -90f);
        cam.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
        cam.farClipPlane = 2500f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.16f, 0.15f, 0.13f);
    }
}
