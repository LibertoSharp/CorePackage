using UnityEngine;

public static class LayerMaskExtensions
{
    public static int ToSingleLayer(this LayerMask mask)
    {
        int value = mask.value;
        if (value == 0) return 0;
        for (int l = 1; l < 32; l++)
            if ((value & (1 << l)) != 0) return l;
        return -1;
    }
}
