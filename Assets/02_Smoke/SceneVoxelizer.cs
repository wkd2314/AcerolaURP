using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using static System.Runtime.InteropServices.Marshal;

[ExecuteInEditMode]
public class SceneVoxelizer : MonoBehaviour
{
    [SerializeField] private ComputeShader initializeShader;
    [SerializeField] private Mesh instancedMesh;
    
    [Header("Scene Settings")]
    [SerializeField] private Vector3 boundExtent = new Vector3(10, 5, 10);
    [SerializeField] private GameObject objectsToVoxelize;
    [SerializeField][Min(0.1f)] private float voxelSize;
    [SerializeField][Range(-0.5f, 1f)] private float intersectionBias;
    
    [Header("Smoke Settings")]
    [SerializeField] private Vector3 maxSmokeRadius = Vector3.one;
    [SerializeField] private float growthSpeed = 1f;
    [SerializeField][Range(0, 128)] private int maxFillSteps = 16;
    private Vector3 smokeOrigin;
    
    private int voxelsCount;
    private Vector3Int voxelResolution;
    
    private Bounds bounds;
    private Material instancedMaterial;

    private ComputeBuffer staticVoxelsBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    private ComputeBuffer trianglesBuffer;
    private ComputeBuffer verticesBuffer;

    private ComputeBuffer smokeBuffer;
    
    private static readonly int StaticVoxels = Shader.PropertyToID("_StaticVoxels");
    private static readonly int InstancesBuffer = Shader.PropertyToID("_InstancesBuffer");
    private static readonly int BoundExtent = Shader.PropertyToID("_BoundExtent");
    private static readonly int VoxelSize = Shader.PropertyToID("_VoxelSize");
    private static readonly int VoxelCount = Shader.PropertyToID("_VoxelCount");
    private static readonly int VoxelResolution = Shader.PropertyToID("_VoxelResolution");
    private static readonly int Vertices = Shader.PropertyToID("_Vertices");
    private static readonly int Triangles = Shader.PropertyToID("_Triangles");
    private static readonly int ObjectToWorld = Shader.PropertyToID("_ObjectToWorld");
    private static readonly int TrianglesCount = Shader.PropertyToID("_TrianglesCount");
    private static readonly int BoundCenter = Shader.PropertyToID("_BoundCenter");
    private static readonly int IntersectionBias = Shader.PropertyToID("_IntersectionBias");
    private Camera cam;
    private float radiusLerpValue;
    private static readonly int SmokeRadius = Shader.PropertyToID("_SmokeRadius");
    private static readonly int SmokeOrigin = Shader.PropertyToID("_SmokeOrigin");
    private static readonly int SmokeVoxels = Shader.PropertyToID("_SmokeVoxels");

    private void Start()
    {
        cam = Camera.main;
    }
    public Vector3 GetBoundExtent() => boundExtent;

    Vector3Int GetVoxelResolution()
    {
        Vector3 resolution = boundExtent * 2 / voxelSize;
        return Vector3Int.CeilToInt(resolution);
    }

    int GetVoxelsCount(Vector3Int resolution)
    {
        return resolution.x * resolution.y * resolution.z;
    }

    private void OnEnable()
    {
        if (!instancedMaterial) instancedMaterial = CoreUtils.CreateEngineMaterial("Hidden/SceneVoxelizer");

        voxelResolution = GetVoxelResolution();
        voxelsCount = GetVoxelsCount(voxelResolution);
        bounds = new Bounds(transform.position, Vector3.one * 100f);

        // instance setting
        staticVoxelsBuffer = new ComputeBuffer(voxelsCount, sizeof(int));
        
        initializeShader.SetVector(VoxelResolution, (Vector3)voxelResolution);
        initializeShader.SetInt(VoxelCount, voxelsCount);
        initializeShader.SetVector(BoundExtent, boundExtent);
        initializeShader.SetVector(BoundCenter, transform.localPosition);
        initializeShader.SetFloat(VoxelSize, voxelSize);

        // set to 0 by default
        initializeShader.SetBuffer(0, StaticVoxels, staticVoxelsBuffer);
        initializeShader.Dispatch(0, Mathf.CeilToInt(voxelsCount / 256.0f), 1, 1);
        
        initializeShader.SetBuffer(1, StaticVoxels, staticVoxelsBuffer);
        initializeShader.SetFloat(IntersectionBias, intersectionBias);
        
        // scene settings
        foreach (Transform t in objectsToVoxelize.GetComponentsInChildren<Transform>())
        {
            var m = t.GetComponent<MeshFilter>();
            
            if(!m) continue;

            var sharedMesh = m.sharedMesh;

            trianglesBuffer = new ComputeBuffer(sharedMesh.triangles.Length, sizeof(int)); // length multiple of 3
            verticesBuffer = new ComputeBuffer(sharedMesh.vertices.Length, sizeof(float) * 3);
            
            trianglesBuffer.SetData(sharedMesh.triangles);
            verticesBuffer.SetData(sharedMesh.vertices);
            
            
            initializeShader.SetBuffer(1, Triangles, trianglesBuffer);
            initializeShader.SetBuffer(1, Vertices, verticesBuffer);
            initializeShader.SetInt(TrianglesCount, sharedMesh.triangles.Length);
            initializeShader.SetMatrix(ObjectToWorld, t.localToWorldMatrix);
            
            initializeShader.Dispatch(1, Mathf.CeilToInt(voxelsCount / 256.0f), 1, 1);
            
            trianglesBuffer.Release();
            verticesBuffer.Release();
        }
        
        // smoke default settings (initialize compute shader)
        smokeBuffer = new ComputeBuffer(voxelsCount, sizeof(int));
        initializeShader.SetBuffer(2, SmokeVoxels, smokeBuffer);
        initializeShader.SetInt("_MaxFillSteps", maxFillSteps);
        
        initializeShader.SetBuffer(3, SmokeVoxels, smokeBuffer);
        initializeShader.SetBuffer(3, StaticVoxels, staticVoxelsBuffer);
        
        // args setting
        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        args[0] = (uint)instancedMesh.GetIndexCount(0);
        args[1] = (uint)voxelsCount;
        args[2] = (uint)instancedMesh.GetIndexStart(0);
        args[3] = (uint)instancedMesh.GetBaseVertex(0);
        argsBuffer.SetData(args);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out var hitInfo))
            {
                smokeOrigin = hitInfo.point;
                
                initializeShader.SetVector(SmokeOrigin, smokeOrigin);
                radiusLerpValue = 0f;
                initializeShader.Dispatch(2, 1, 1, 1);
            }
        }
        
        initializeShader.SetVector(SmokeRadius, Vector3.Lerp(Vector3.zero, maxSmokeRadius,
            EasingUtils.EaseInOutCustom(radiusLerpValue)));
        initializeShader.Dispatch(3, Mathf.CeilToInt(voxelsCount / 256.0f), 1, 1);

        radiusLerpValue += growthSpeed * Time.deltaTime;
        
        // final shader setting
        // instanceID to position, get smokeVoxels value -> draw
        instancedMaterial.SetBuffer(SmokeVoxels, smokeBuffer);
        instancedMaterial.SetVector(VoxelResolution, (Vector3)voxelResolution);
        instancedMaterial.SetFloat(VoxelSize, voxelSize);
        
        Graphics.DrawMeshInstancedIndirect(instancedMesh, 0, instancedMaterial, bounds, argsBuffer);
    }

    private void OnDisable()
    {
        argsBuffer?.Release();
        staticVoxelsBuffer?.Release();
        if(instancedMaterial) CoreUtils.Destroy(instancedMaterial);
    }
} 
