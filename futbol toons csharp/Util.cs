using System;
using System.Collections.Generic;
using System.Drawing;

namespace FutbolToons
{
    // Utilidades generales: números al azar, colores, interpolación, tiempos
    static class U
    {
        public static Random rng = new Random();
        public static double R() { return rng.NextDouble(); }

        public static double Clamp(double v, double a, double b) { return v < a ? a : v > b ? b : v; }
        public static double Lerp(double a, double b, double p) { return a + (b - a) * p; }
        public static double EaseIn(double a, double b, double p) { return a + (b - a) * p * p; }
        public static double EaseInOut(double a, double b, double p) { return a + (b - a) * ((-Math.Cos(p * Math.PI) / 2) + 0.5); }

        // Generador al azar con semilla (para que cada pista se arme siempre igual)
        public static Func<double> Mulberry(uint seed)
        {
            uint a = seed;
            return () =>
            {
                unchecked
                {
                    a += 0x6D2B79F5;
                    uint t = (a ^ (a >> 15)) * (1 | a);
                    t = (t + ((t ^ (t >> 7)) * (61 | t))) ^ t;
                    return (t ^ (t >> 14)) / 4294967296.0;
                }
            };
        }

        public static T Pick<T>(T[] arr, Func<double> rnd)
        {
            return arr[(int)Math.Floor((rnd == null ? R() : rnd()) * arr.Length) % arr.Length];
        }

        public static List<T> Shuffle<T>(IEnumerable<T> src)
        {
            var a = new List<T>(src);
            for (int i = a.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                T tmp = a[i]; a[i] = a[j]; a[j] = tmp;
            }
            return a;
        }

        // ---- colores ----
        static Dictionary<string, Color> cache = new Dictionary<string, Color>();
        public static Color C(string hex)
        {
            Color c;
            if (cache.TryGetValue(hex, out c)) return c;
            string h = hex.TrimStart('#');
            if (h.Length == 3) h = "" + h[0] + h[0] + h[1] + h[1] + h[2] + h[2];
            int v = Convert.ToInt32(h, 16);
            c = Color.FromArgb(255, (v >> 16) & 255, (v >> 8) & 255, v & 255);
            cache[hex] = c;
            return c;
        }
        public static Color RGBA(int r, int g, int b, double a)
        {
            return Color.FromArgb((int)Clamp(a * 255, 0, 255), r, g, b);
        }
        public static Color A(Color c, double a)
        {
            return Color.FromArgb((int)Clamp(a * 255, 0, 255), c.R, c.G, c.B);
        }
        // aclara (f>0) u oscurece (f<0) un color, igual que shade() del juego web
        public static Color Shade(Color c, double f)
        {
            double t = f < 0 ? 0 : 255, p = Math.Abs(f);
            return Color.FromArgb(c.A,
                (int)Math.Round((t - c.R) * p + c.R),
                (int)Math.Round((t - c.G) * p + c.G),
                (int)Math.Round((t - c.B) * p + c.B));
        }
        public static Color Hsl(double h, double s, double l)
        {
            h = (((h % 360) + 360) % 360) / 360.0;
            double r, g, b;
            if (s == 0) { r = g = b = l; }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;
                r = Hue(p, q, h + 1.0 / 3); g = Hue(p, q, h); b = Hue(p, q, h - 1.0 / 3);
            }
            return Color.FromArgb(255, (int)Clamp(r * 255, 0, 255), (int)Clamp(g * 255, 0, 255), (int)Clamp(b * 255, 0, 255));
        }
        static double Hue(double p, double q, double t)
        {
            if (t < 0) t += 1; if (t > 1) t -= 1;
            if (t < 1.0 / 6) return p + (q - p) * 6 * t;
            if (t < 0.5) return q;
            if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
            return p;
        }
        // devuelve el primer color que no esté vacío
        public static Color First(params Color[] cs)
        {
            foreach (var c in cs) if (!c.IsEmpty) return c;
            return C("#888888");
        }

        public static string FmtTime(double s)
        {
            if (s < 0) s = 0;
            int m = (int)Math.Floor(s / 60), ss = (int)Math.Floor(s % 60), cs = (int)Math.Floor((s * 100) % 100);
            return m + ":" + ss.ToString("00") + "." + cs.ToString("00");
        }
    }
}
