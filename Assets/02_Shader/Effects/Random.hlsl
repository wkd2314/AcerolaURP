uint RandomHash(uint s)
{
    s ^= 2747636419u;
    s *= 2654435769u;
    s ^= s >> 16;
    s *= 2654435769u;
    s ^= s >> 16;
    s *= 2654435769u;
    return s;
}

float Random(uint seed)
{
    return float(RandomHash(seed)) / 4294967295.0; // 2^32-1
}

float Random(float a)
{
    uint seed = a * 100.0f;
    return float(RandomHash(seed)) / 4294967295.0; // 2^32-1
}

