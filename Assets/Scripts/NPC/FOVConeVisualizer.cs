using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FOVConeVisualizer2D : MonoBehaviour
{
    public NPCVision vision;

    [Header("Visibility")]
    public bool showInGame = true;
    public bool showInEditor = true;

    [Header("Mesh Detail")]
    [Range(3, 120)] public int segments = 40;

    [Header("Look")]
    public bool doubleSided = true;

    Mesh _mesh;
    MeshRenderer _mr;

    void Awake()
    {
        _mesh = new Mesh();
        _mesh.name = "FOVConeMesh2D";
        GetComponent<MeshFilter>().sharedMesh = _mesh;

        _mr = GetComponent<MeshRenderer>();
        ApplyVisibility();
    }

    void OnValidate()
    {
        ApplyVisibility();
        Rebuild();
    }

    void LateUpdate()
    {
        ApplyVisibility();
        Rebuild();
    }

    void ApplyVisibility()
    {
        if (_mr == null) _mr = GetComponent<MeshRenderer>();
        bool visible = Application.isPlaying ? showInGame : showInEditor;
        _mr.enabled = visible;
    }

    void Rebuild()
    {
        if (vision == null) return;
        if (!vision.useFOV) { _mesh.Clear(); return; }

        float radius = vision.detectionRadius;
        float angle = vision.fovAngle;

        int ringCount = segments + 1;

        // Vertices: origin + ring
        Vector3[] verts = new Vector3[1 + ringCount];
        verts[0] = Vector3.zero;

        float half = angle * 0.5f;

        // In 2D: Up is Forward. Rotation around Z.
        for (int i = 0; i < ringCount; i++)
        {
            float t = (float)i / (ringCount - 1);
            float a = Mathf.Lerp(-half, half, t);
            Vector3 dir = Quaternion.Euler(0f, 0f, a) * Vector3.up; // XY plane
            verts[i + 1] = dir * radius;
        }

        // Triangles (fan)
        int baseTriCount = segments * 3;
        int[] tris = new int[doubleSided ? baseTriCount * 2 : baseTriCount];

        int tri = 0;

        for (int i = 0; i < segments; i++)
        {
            // front side
            tris[tri++] = 0;
            tris[tri++] = i + 1;
            tris[tri++] = i + 2;
        }

        if (doubleSided)
        {
            // back side (reversed winding)
            for (int i = 0; i < segments; i++)
            {
                tris[tri++] = 0;
                tris[tri++] = i + 2;
                tris[tri++] = i + 1;
            }
        }

        _mesh.Clear();
        _mesh.vertices = verts;
        _mesh.triangles = tris;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }
}
