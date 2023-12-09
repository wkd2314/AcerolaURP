using System;

public static class EasingUtils
{
    public static float EaseInOutCustom(float x)
    {
        return x < 0.5 ? 2 * x * x : 1 - 1 / (10 * x - 3);
    }

}
