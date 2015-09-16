using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class MathUtil
{
    public static int Mod(int a, int m)
    {
        return ((a % m) + m) % m;
    }
}
