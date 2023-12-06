using System;

public static class EasingUtils
{
    public static float EaseInOutQuad(float x)
    {
        return x < 0.5 ? 2 * x * x : 1 - 2 * (x - 1) * (x - 1);
    }

}
