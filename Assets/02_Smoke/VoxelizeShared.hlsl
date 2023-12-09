#ifndef	Include_VoxelizeShared
#define	Include_VoxelizeShared

/// vert, frag shader variables

StructuredBuffer<int> _InstancesBuffer;

/// voxelize scene values
RWStructuredBuffer<int> _StaticVoxels;
StructuredBuffer<float3> _Vertices;
StructuredBuffer<int> _Triangles;

float3 _BoundExtent;
float3 _BoundCenter;
float _VoxelSize;
uint _VoxelCount;
uint3 _VoxelResolution;
float _IntersectionBias;

float4x4 _ObjectToWorld;
int _TrianglesCount;

// smoke values
float3 _SmokeRadius;
float3 _SmokeOrigin;

RWStructuredBuffer<int> _SmokeVoxels;
float _MaxFillSteps;

// filter for flood fill.
static int3 offsets[] = {
    int3(1, 0, 0),
    int3(-1, 0, 0),
    int3(0, 1, 0),
    int3(0, -1, 0),
    int3(0, 0, 1),
    int3(0, 0, -1)
};

// functions

float3 MultiplyByVoxelScale(float3 position, float scale)
{
    float3x3 scaleM = {
        scale, 0, 0,
        0, scale, 0,
        0, 0, scale
    };
    return mul(scaleM, position);
}

uint to1D(uint3 pos)
{
    return pos.x + pos.y * _VoxelResolution.x + pos.z * _VoxelResolution.x * _VoxelResolution.y;
}

uint3 to3D(uint id)
{
    uint x = id % _VoxelResolution.x;
    uint y = id / _VoxelResolution.x % _VoxelResolution.y;
    uint z = id / (_VoxelResolution.x * _VoxelResolution.y);
    
    return uint3(x, y, z);
}

struct AABB
{
    float3 center;
    float3 extents;
};

struct Triangle
{
    float3 a, b, c;
};

bool IntersectsTriangleAabbSat(float3 v0, float3 v1, float3 v2, float3 aabbExtents, float3 axis)
{
    float p0 = dot(v0, axis);
    float p1 = dot(v1, axis);
    float p2 = dot(v2, axis);

    float r = aabbExtents.x * abs(dot(float3(1, 0, 0), axis)) +
            aabbExtents.y * abs(dot(float3(0, 1, 0), axis)) +
            aabbExtents.z * abs(dot(float3(0, 0, 1), axis));

    float maxP = max(p0, max(p1, p2));
    float minP = min(p0, min(p1, p2));

    return !(max(-maxP, minP) > r);
}

bool IntersectsTriangleAabb(Triangle tri, AABB aabb)
{
    // move aabb to ORIGIN.
    tri.a -= aabb.center;
    tri.b -= aabb.center;
    tri.c -= aabb.center;

    // three edge normals (edgeNormal은 변이 아니라 꼭짓점 기준임)
    float3 ab = normalize(tri.b - tri.a);
    float3 bc = normalize(tri.c - tri.a);
    float3 ca = normalize(tri.a - tri.c);

    // cross with (1,0,0)
    float3 a00 = float3(0.0,-ab.z, ab.y);
    float3 a01 = float3(0.0,-bc.z, bc.y);
    float3 a02 = float3(0.0,-ca.z, ca.y);

    // cross with (0,1,0)
    float3 a10 = float3(ab.z, 0.0, -ab.x);
    float3 a11 = float3(bc.z, 0.0, -bc.x);
    float3 a12 = float3(ca.z, 0.0, -ca.x);

    // cross with (0,0,1)
    float3 a20 = float3(-ab.y, ab.x, 0.0);
    float3 a21 = float3(-bc.y, bc.x, 0.0);
    float3 a22 = float3(-ca.y, ca.x, 0.0);

    if(!IntersectsTriangleAabbSat(tri.a, tri.b, tri.c, aabb.extents, a00) ||
        !IntersectsTriangleAabbSat(tri.a, tri.b, tri.c, aabb.extents, a01) ||
        !IntersectsTriangleAabbSat(tri.a, tri.b, tri.c, aabb.extents, a02) ||
        !IntersectsTriangleAabbSat(tri.a, tri.b, tri.c, aabb.extents, a10) ||
        !IntersectsTriangleAabbSat(tri.a, tri.b, tri.c, aabb.extents, a11) ||
        !IntersectsTriangleAabbSat(tri.a, tri.b, tri.c, aabb.extents, a12) ||
        !IntersectsTriangleAabbSat(tri.a, tri.b, tri.c, aabb.extents, a20) ||
        !IntersectsTriangleAabbSat(tri.a, tri.b, tri.c, aabb.extents, a21) ||
        !IntersectsTriangleAabbSat(tri.a, tri.b, tri.c, aabb.extents, a22) ||
        !IntersectsTriangleAabbSat(tri.a, tri.b, tri.c, aabb.extents, float3(1,0,0)) ||
        !IntersectsTriangleAabbSat(tri.a, tri.b, tri.c, aabb.extents, float3(0,1,0)) ||
        !IntersectsTriangleAabbSat(tri.a, tri.b, tri.c, aabb.extents, float3(0,0,1)) ||
        !IntersectsTriangleAabbSat(tri.a, tri.b, tri.c, aabb.extents, cross(ab, bc))
        )
    {
        return false;
    }

    return true;
}

#endif