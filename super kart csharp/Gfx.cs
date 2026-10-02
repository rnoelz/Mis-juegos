using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SuperKart
{
    // Herramientas de dibujo. Todo se dibuja en coordenadas lógicas de 960x540 y
    // Graphics se encarga de escalar a la resolución real de la ventana (imagen nítida).
    static partial class SK
    {
        public const int W = 960, H = 540;
        public static Graphics g;
        public static double ALPHA = 1;   // transparencia global (como globalAlpha del canvas)

        static Dictionary<int, SolidBrush> brushes = new Dictionary<int, SolidBrush>();
        static Pen pen = new Pen(Color.Black);

        static float F(double v) { return (float)v; }

        static SolidBrush B(Color c)
        {
            int a = (int)(c.A * ALPHA);
            if (a < 0) a = 0; if (a > 255) a = 255;
            int key = (a << 24) | (c.R << 16) | (c.G << 8) | c.B;
            SolidBrush b;
            if (!brushes.TryGetValue(key, out b))
            {
                if (brushes.Count > 3000) { foreach (var x in brushes.Values) x.Dispose(); brushes.Clear(); }
                b = new SolidBrush(Color.FromArgb(a, c.R, c.G, c.B));
                brushes[key] = b;
            }
            return b;
        }
        static Pen P(Color c, double w, bool round = false)
        {
            int a = (int)U.Clamp(c.A * ALPHA, 0, 255);
            pen.Color = Color.FromArgb(a, c.R, c.G, c.B);
            pen.Width = (float)Math.Max(0.1, w);
            pen.LineJoin = LineJoin.Round;
            pen.StartCap = round ? LineCap.Round : LineCap.Flat;
            pen.EndCap = round ? LineCap.Round : LineCap.Flat;
            return pen;
        }

        // ---- figuras ----
        static void circ(double x, double y, double r, Color c)
        {
            if (r < 0.1) r = 0.1;
            g.FillEllipse(B(c), F(x - r), F(y - r), F(2 * r), F(2 * r));
        }
        static void ell(double x, double y, double rx, double ry, Color c, double rot = 0)
        {
            rx = Math.Max(0.1, Math.Abs(rx)); ry = Math.Max(0.1, Math.Abs(ry));
            if (rot == 0) { g.FillEllipse(B(c), F(x - rx), F(y - ry), F(2 * rx), F(2 * ry)); return; }
            var st = g.Save();
            g.TranslateTransform(F(x), F(y));
            g.RotateTransform(F(rot * 180 / Math.PI));
            g.FillEllipse(B(c), F(-rx), F(-ry), F(2 * rx), F(2 * ry));
            g.Restore(st);
        }
        static void ellStroke(double x, double y, double rx, double ry, Color c, double lw)
        {
            rx = Math.Max(0.1, rx); ry = Math.Max(0.1, ry);
            g.DrawEllipse(P(c, lw), F(x - rx), F(y - ry), F(2 * rx), F(2 * ry));
        }
        // media elipse (la parte de arriba)
        static void halfEll(double x, double y, double rx, double ry, Color c)
        {
            rx = Math.Max(0.1, rx); ry = Math.Max(0.1, ry);
            g.FillPie(B(c), F(x - rx), F(y - ry), F(2 * rx), F(2 * ry), 180, 180);
        }
        static void rect(double x, double y, double w, double h, Color c)
        {
            if (w < 0) { x += w; w = -w; }
            if (h < 0) { y += h; h = -h; }
            g.FillRectangle(B(c), F(x), F(y), F(w), F(h));
        }
        static void rectStroke(double x, double y, double w, double h, Color c, double lw)
        {
            g.DrawRectangle(P(c, lw), F(x), F(y), F(w), F(h));
        }
        static PointF[] pts(params double[] xy)
        {
            var p = new PointF[xy.Length / 2];
            for (int i = 0; i < p.Length; i++) p[i] = new PointF(F(xy[i * 2]), F(xy[i * 2 + 1]));
            return p;
        }
        static void fillPts(Color c, params double[] xy) { g.FillPolygon(B(c), pts(xy)); }
        static void lines(Color c, double w, bool round, params double[] xy) { g.DrawLines(P(c, w, round), pts(xy)); }

        static GraphicsPath rrPath(double x, double y, double w, double h, double r)
        {
            var p = new GraphicsPath();
            r = Math.Max(0.01, Math.Min(r, Math.Min(Math.Abs(w) / 2, Math.Abs(h) / 2)));
            if (w <= 0.02 || h <= 0.02) { p.AddRectangle(new RectangleF(F(x), F(y), F(Math.Max(0.02, w)), F(Math.Max(0.02, h)))); return p; }
            float d = F(r * 2);
            p.AddArc(F(x), F(y), d, d, 180, 90);
            p.AddArc(F(x + w - r * 2), F(y), d, d, 270, 90);
            p.AddArc(F(x + w - r * 2), F(y + h - r * 2), d, d, 0, 90);
            p.AddArc(F(x), F(y + h - r * 2), d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
        static void rrFill(double x, double y, double w, double h, double r, Color c)
        {
            using (var p = rrPath(x, y, w, h, r)) g.FillPath(B(c), p);
        }
        static void rrStroke(double x, double y, double w, double h, double r, Color c, double lw)
        {
            using (var p = rrPath(x, y, w, h, r)) g.DrawPath(P(c, lw), p);
        }
        static PointF[] starPts(double x, double y, double R, double r)
        {
            var p = new PointF[10];
            for (int i = 0; i < 10; i++)
            {
                double a = -Math.PI / 2 + i * Math.PI / 5, d = i % 2 == 1 ? r : R;
                p[i] = new PointF(F(x + Math.Cos(a) * d), F(y + Math.Sin(a) * d));
            }
            return p;
        }

        // ---- texto con borde (estilo de letras del juego) ----
        static FontFamily FF;
        static FontStyle FSTYLE = FontStyle.Regular;
        static StringFormat SF_C, SF_L, SF_R;
        static void InitFonts()
        {
            try { FF = new FontFamily("Arial Black"); }
            catch { FF = new FontFamily("Arial"); FSTYLE = FontStyle.Bold; }
            SF_C = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            SF_L = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
            SF_R = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
            foreach (var sf in new[] { SF_C, SF_L, SF_R }) sf.FormatFlags |= StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
        }
        static void txt(string s, double x, double y, double size, Color col) { txt(s, x, y, size, col, "center", Color.Black, -1); }
        static void txt(string s, double x, double y, double size, Color col, string align) { txt(s, x, y, size, col, align, Color.Black, -1); }
        static void txt(string s, double x, double y, double size, Color col, string align, Color outline, double lw)
        {
            if (string.IsNullOrEmpty(s) || size < 1) return;
            if (lw < 0) lw = Math.Max(2, size / 7);
            var sf = align == "left" ? SF_L : align == "right" ? SF_R : SF_C;
            using (var p = new GraphicsPath())
            {
                p.AddString(s, FF, (int)FSTYLE, F(size), new PointF(F(x), F(y + size * 0.06)), sf);
                if (lw > 0) g.DrawPath(P(outline, lw), p);
                g.FillPath(B(col), p);
            }
        }

        // ---- transformaciones (como save/translate/rotate del canvas) ----
        static GraphicsState Save() { return g.Save(); }
        static void Restore(GraphicsState s) { g.Restore(s); }
        static void Translate(double x, double y) { g.TranslateTransform(F(x), F(y)); }
        static void Rotate(double rad) { g.RotateTransform(F(rad * 180 / Math.PI)); }
        static void Scale(double sx, double sy)
        {
            if (Math.Abs(sx) < 0.03) sx = sx < 0 ? -0.03 : 0.03;
            if (Math.Abs(sy) < 0.03) sy = sy < 0 ? -0.03 : 0.03;
            g.ScaleTransform(F(sx), F(sy));
        }
        static void ClipRect(double x, double y, double w, double h) { g.SetClip(new RectangleF(F(x), F(y), F(Math.Max(0, w)), F(Math.Max(0, h))), CombineMode.Intersect); }

        static void Gradient(double x, double y, double w, double h, Color top, Color bottom)
        {
            if (h <= 0 || w <= 0) return;
            using (var lg = new LinearGradientBrush(new PointF(0, F(y - 1)), new PointF(0, F(y + h + 1)), top, bottom))
                g.FillRectangle(lg, F(x), F(y), F(w), F(h));
        }
    }
}
