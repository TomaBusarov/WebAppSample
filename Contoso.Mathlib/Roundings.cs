using System;

namespace Contoso.Mathlib
{
    public static class Roundings
    {
        public static int Floor(double d)
        {
            return (int)d;
        }

        public static int Ceiling(double d)
        {
            return (int)(d + 1);
        }

        public static void Dummy()
        {
            //TODO: Add body
        }
    }

}
