using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class NvrReticleDot : MonoBehaviour, NvrGazePointer
{
    // Number of segments making the reticle circle.
    public int reticleSegments = 20;

    // Growth speed multiplier for the reticle/
    public float reticleGrowthSpeed = 8.0f;

    // Private members
    private Material materialComp;
    private GameObject targetObj;

    // Current angle of the reticle (in degrees).
    private float reticleAngle = 0.5f;
    // Current distance of the reticle (in meters).
    private float reticleDistanceInMeters = 10f;
    
    private float reticleMinAngle = 0f;
    //public float reticleMaxAngle = 3f;

    // Angle at which to expand the reticle when intersecting with an object (in degrees).
    public float reticleGrowthAngle = 4f;

    // Minimum distance of the reticle (in meters).
    public float reticleDistanceMin = 0.45f;
    // Maximum distance of the reticle (in meters).
    public float reticleDistanceMax = 12f;

    public bool hideReticleAfterMaxDistance = false;

    // Current diameter of the reticle before distance multiplication.
    private float reticleDiameter = 0.0f;

    public Color ReticleColor
    {
        get { return materialComp.color; }
        set { materialComp.color = value; }
    }

    void Start()
    {
        CreateReticleVertices();

        Renderer myRenderer = gameObject.GetComponent<Renderer>();
        materialComp = myRenderer.material;

        // this is to ensure we always render the reticle on top of everything else
        // (default for a material is 2000, in the Google example code they had set this to -1 manually)
        myRenderer.sortingLayerName = "Reticle";
        materialComp.renderQueue = -1;
        reticleAngle = reticleMinAngle;
    }

    void OnEnable()
    {
        NvrGazeInputModule.gazePointer = this;
    }

    void OnDisable()
    {
        if ((Object)NvrGazeInputModule.gazePointer == this)
        {
            NvrGazeInputModule.gazePointer = null;
        }
    }

    void Update()
    {
        UpdateDiameters();
    }

    /// This is called when the 'BaseInputModule' system should be enabled.
    public void OnGazeEnabled()
    {

    }

    /// This is called when the 'BaseInputModule' system should be disabled.
    public void OnGazeDisabled()
    {

    }

    public void OnGazeStart(Camera camera, GameObject targetObject, Vector3 intersectionPosition,
                            bool isInteractive)
    {
        SetGazeTarget(intersectionPosition, isInteractive);
    }

    public void OnGazeStay(Camera camera, GameObject targetObject, Vector3 intersectionPosition,
                           bool isInteractive)
    {
        SetGazeTarget(intersectionPosition, isInteractive);
    }

    public void OnGazeExit(Camera camera, GameObject targetObject)
    {
        reticleDistanceInMeters = reticleDistanceMax;
        reticleAngle = reticleMinAngle;
    }

    public void OnGazeTriggerStart(Camera camera)
    {
    }

    public void OnGazeTriggerEnd(Camera camera)
    {
    }

    public void GetPointerRadius(out float innerRadius, out float outerRadius)
    {
        float min_inner_angle_radians = 0;
        float max_inner_angle_radians = Mathf.Deg2Rad * reticleGrowthAngle;

        innerRadius = 2.0f * Mathf.Tan(min_inner_angle_radians);
        outerRadius = 2.0f * Mathf.Tan(max_inner_angle_radians);
    }

    private void CreateReticleVertices()
    {
        Mesh mesh = new Mesh();
        gameObject.AddComponent<MeshFilter>();
        GetComponent<MeshFilter>().mesh = mesh;

        int segments_count = reticleSegments;
        int vertex_count = segments_count + 1;

        #region Vertices

        Vector3[] vertices = new Vector3[vertex_count];

        const float kTwoPi = Mathf.PI * 2.0f;
        int vi = 0;

        // for all verticies the z coordinates must be >= 1 
        // (not 100% sure why, I suspect it prevents the verts being culled)
        vertices[vi++] = new Vector3(0.0f, 0.0f, 1.0f);
        for (int si = 0; si < segments_count; ++si)
        {
            // Add two vertices for every circle segment: one at the beginning of the
            // prism, and one at the end of the prism.
            float angle = (float)si / (float)(segments_count) * kTwoPi;

            float x = Mathf.Sin(angle);
            float y = Mathf.Cos(angle);

            vertices[vi++] = new Vector3(x, y, 1.0f);
        }
        #endregion

        #region Triangles
        int indices_count = vertex_count * 3;
        int[] indices = new int[indices_count];

        int idx = 0;
        for (int si = 0; si < segments_count; ++si)
        {
            indices[idx++] = 0;
            indices[idx++] = si + 1;
            indices[idx++] = ((si + 1) % segments_count) + 1;
        }
        #endregion

        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateBounds();
        ;
    }

    private void UpdateDiameters()
    {
        bool zeroTheDiameter = false;
        if (hideReticleAfterMaxDistance && reticleDistanceInMeters >= reticleDistanceMax)
        {
            zeroTheDiameter = true;
        }
        else
        {
            reticleDistanceInMeters =
                Mathf.Clamp(reticleDistanceInMeters, reticleDistanceMin, reticleDistanceMax);
        }
        float target_diameter = 0;

        if (!zeroTheDiameter)
        {
            float half_angle_radians = Mathf.Deg2Rad * reticleAngle * 0.5f;

            target_diameter = 2.0f * Mathf.Tan(half_angle_radians);
        }

        reticleDiameter =
            Mathf.Lerp(reticleDiameter, target_diameter, Time.deltaTime * reticleGrowthSpeed);

        materialComp.SetFloat("_Diameter", reticleDiameter);
        materialComp.SetFloat("_DistanceInMeters", reticleDistanceInMeters);
    }

    private void SetGazeTarget(Vector3 target, bool interactive)
    {
        Vector3 targetLocalPosition = transform.InverseTransformPoint(target);

        reticleDistanceInMeters =
            Mathf.Clamp(targetLocalPosition.z, reticleDistanceMin, reticleDistanceMax);
        if (interactive)
        {
            reticleAngle = reticleGrowthAngle;
        }
        else
        {
            reticleAngle = reticleMinAngle;
        }
    }
}