using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace FutbolToons
{
    // Caras de los personajes de Mario y Dragon Ball (las mismas del Super Kart)
    static partial class G
    {
        static Color W_ = Color.White;
        static Color K_ = Color.FromArgb(255, 17, 17, 17);

        // curva cuadrática (como quadraticCurveTo) convertida a Bézier
        static void quadTo(GraphicsPath p, double x0, double y0, double qx, double qy, double x1, double y1)
        {
            p.AddBezier(F(x0), F(y0), F(x0 + 2.0 / 3 * (qx - x0)), F(y0 + 2.0 / 3 * (qy - y0)), F(x1 + 2.0 / 3 * (qx - x1)), F(y1 + 2.0 / 3 * (qy - y1)), F(x1), F(y1));
        }
        static void quadLine(Color c, double w, double x0, double y0, double qx, double qy, double x1, double y1, bool round = true)
        {
            using (var p = new GraphicsPath()) { quadTo(p, x0, y0, qx, qy, x1, y1); g.DrawPath(P(c, w, round), p); }
        }
        static void arc(Color c, double w, double x, double y, double r, double a0, double a1, bool round = false)
        {
            if (r < 0.1) return;
            g.DrawArc(P(c, w, round), F(x - r), F(y - r), F(2 * r), F(2 * r), F(a0 * 180 / Math.PI), F((a1 - a0) * 180 / Math.PI));
        }

        static void eyes(double x, double y, double r, Color col, double sp = 0.28)
        {
            foreach (int s in new[] { -1, 1 })
            {
                ell(x + s * r * sp, y, r * 0.12, r * 0.19, W_);
                ell(x + s * r * sp, y + r * 0.03, r * 0.075, r * 0.12, col);
                circ(x + s * r * sp + r * 0.02, y - r * 0.03, r * 0.03, W_);
            }
        }
        static void crown(double x, double y, double s, Color gem)
        {
            fillPts(C("#ffd200"), x - s * 0.5, y + s * 0.3, x - s * 0.5, y - s * 0.15, x - s * 0.25, y + s * 0.05, x, y - s * 0.3, x + s * 0.25, y + s * 0.05, x + s * 0.5, y - s * 0.15, x + s * 0.5, y + s * 0.3);
            circ(x, y + s * 0.12, s * 0.1, gem);
        }

        // ===================== retrato de frente =====================
        static void drawHead(Ch ch, double x, double y, double r)
        {
            var st = Save();
            switch (ch.kind)
            {
                case "plumber":
                    {
                        double fw = ch.big ? 1.15 : ch.thin ? 0.85 : 1, fh = ch.thin ? 1.08 : 1;
                        ell(x - r * 0.86 * fw, y + r * 0.12, r * 0.18, r * 0.24, ch.skin); ell(x + r * 0.86 * fw, y + r * 0.12, r * 0.18, r * 0.24, ch.skin);
                        ell(x, y + r * 0.15, r * 0.82 * fw, r * 0.82 * fh, ch.skin);
                        ell(x - r * 0.7 * fw, y - r * 0.02, r * 0.16, r * 0.26, ch.hair); ell(x + r * 0.7 * fw, y - r * 0.02, r * 0.16, r * 0.26, ch.hair);
                        halfEll(x, y - r * 0.2, r * 0.9 * fw, r * 0.72, ch.hat);
                        ell(x, y - r * 0.2, r * 0.98 * fw, r * 0.14, U.Shade(ch.hat, -0.25));
                        circ(x, y - r * 0.5, r * 0.26, W_);
                        var s2 = Save(); Translate(x, y - r * 0.49); if (ch.flip) Rotate(Math.PI);
                        txt(ch.letter, 0, r * 0.0, r * 0.36, ch.hat, "center", Color.Black, 0); Restore(s2);
                        eyes(x, y + r * 0.04, r, (ch.id == "wario" || ch.id == "waluigi") ? C("#3a2a8a") : C("#1a4fd0"));
                        ell(x, y + r * 0.32, r * 0.2 * (ch.big ? 1.35 : 1), r * 0.16 * (ch.thin ? 1.4 : 1), ch.nose.IsEmpty ? U.Shade(ch.skin, -0.12) : ch.nose);
                        if (ch.id == "wario")
                        {
                            ell(x, y + r * 0.66, r * 0.4, r * 0.13, W_);
                            fillPts(C("#2a1406"), x - r * 0.62, y + r * 0.38, x - r * 0.4, y + r * 0.6, x - r * 0.25, y + r * 0.45, x, y + r * 0.58, x + r * 0.25, y + r * 0.45, x + r * 0.4, y + r * 0.6, x + r * 0.62, y + r * 0.38, x, y + r * 0.42);
                        }
                        else if (ch.id == "waluigi")
                        {
                            ell(x - r * 0.22, y + r * 0.5, r * 0.26, r * 0.06, C("#2a1406"), -0.35); ell(x + r * 0.22, y + r * 0.5, r * 0.26, r * 0.06, C("#2a1406"), 0.35);
                        }
                        else
                        {
                            Color stc = ch.stache.IsEmpty ? C("#2a1406") : ch.stache;
                            ell(x - r * 0.24, y + r * 0.5, r * 0.28, r * 0.11, stc, -0.2); ell(x + r * 0.24, y + r * 0.5, r * 0.28, r * 0.11, stc, 0.2);
                        }
                        if (ch.id == "metal") ell(x - r * 0.4, y - r * 0.45, r * 0.2, r * 0.08, U.RGBA(255, 255, 255, 0.5), -0.6);
                        break;
                    }
                case "princess":
                    ell(x, y + r * 0.35, r * 1.0, r * 1.05, ch.hair);
                    ell(x, y + r * 0.12, r * 0.72, r * 0.8, ch.skin);
                    ell(x - r * 0.38, y - r * 0.42, r * 0.48, r * 0.3, ch.hair, 0.4); ell(x + r * 0.38, y - r * 0.42, r * 0.48, r * 0.3, ch.hair, -0.4);
                    crown(x, y - r * 0.82, r * 0.7, ch.gem);
                    eyes(x, y + r * 0.05, r, C("#2a7ad8"), 0.27);
                    circ(x - r * 0.42, y + r * 0.35, r * 0.1, U.RGBA(255, 120, 150, 0.6)); circ(x + r * 0.42, y + r * 0.35, r * 0.1, U.RGBA(255, 120, 150, 0.6));
                    ell(x, y + r * 0.52, r * 0.12, r * 0.06, C("#d8304a"));
                    circ(x - r * 0.76, y + r * 0.38, r * 0.08, ch.gem); circ(x + r * 0.76, y + r * 0.38, r * 0.08, ch.gem);
                    break;
                case "toad":
                    if (ch.pigtails) { ell(x - r * 0.95, y + r * 0.45, r * 0.25, r * 0.38, ch.cap); ell(x + r * 0.95, y + r * 0.45, r * 0.25, r * 0.38, ch.cap); circ(x - r * 0.95, y + r * 0.4, r * 0.08, W_); circ(x + r * 0.95, y + r * 0.4, r * 0.08, W_); }
                    ell(x, y + r * 0.4, r * 0.58, r * 0.55, ch.skin);
                    ell(x, y - r * 0.15, r * 1.12, r * 0.72, ch.cap.IsEmpty ? W_ : ch.cap);
                    circ(x, y - r * 0.45, r * 0.3, ch.spot); circ(x - r * 0.75, y - r * 0.12, r * 0.24, ch.spot); circ(x + r * 0.75, y - r * 0.12, r * 0.24, ch.spot);
                    ell(x - r * 0.2, y + r * 0.36, r * 0.08, r * 0.15, K_); ell(x + r * 0.2, y + r * 0.36, r * 0.08, r * 0.15, K_);
                    ell(x, y + r * 0.64, r * 0.12, r * 0.07, C("#c02a3a"));
                    break;
                case "yoshi":
                    fillPts(ch.spike, x + r * 0.45, y - r * 0.55, x + r * 0.85, y - r * 0.75, x + r * 0.7, y - r * 0.3);
                    ell(x, y - r * 0.05, r * 0.75, r * 0.72, ch.body);
                    ell(x, y + r * 0.38, r * 0.95, r * 0.52, ch.body);
                    ell(x, y + r * 0.7, r * 0.55, r * 0.18, W_);
                    circ(x - r * 0.25, y + r * 0.25, r * 0.06, C("#1a5a1a")); circ(x + r * 0.25, y + r * 0.25, r * 0.06, C("#1a5a1a"));
                    foreach (int s in new[] { -1, 1 }) { ell(x + s * r * 0.28, y - r * 0.5, r * 0.24, r * 0.34, W_); ell(x + s * r * 0.24, y - r * 0.45, r * 0.09, r * 0.16, K_); }
                    break;
                case "koopa":
                    ell(x, y + r * 0.45, r * 1.0, r * 0.75, W_); ell(x, y + r * 0.45, r * 0.92, r * 0.68, ch.shell);
                    ell(x, y - r * 0.05, r * 0.68, r * 0.78, ch.body);
                    ell(x, y + r * 0.38, r * 0.5, r * 0.28, U.Shade(ch.body, -0.12));
                    if (ch.bones) foreach (int s in new[] { -1, 1 }) { ell(x + s * r * 0.24, y - r * 0.28, r * 0.2, r * 0.24, C("#2a2a2a")); circ(x + s * r * 0.24, y - r * 0.26, r * 0.06, W_); }
                    else foreach (int s in new[] { -1, 1 }) { ell(x + s * r * 0.24, y - r * 0.3, r * 0.18, r * 0.27, W_); ell(x + s * r * 0.22, y - r * 0.27, r * 0.08, r * 0.15, K_); }
                    if (ch.bones) lines(C("#5a5a5a"), Math.Max(1, r * 0.04), false, x - r * 0.3, y + r * 0.38, x + r * 0.3, y + r * 0.38);
                    break;
                case "birdo":
                    ell(x, y, r * 0.85, r * 0.85, ch.body);
                    ell(x, y + r * 0.35, r * 0.38, r * 0.36, U.Shade(ch.body, -0.1)); ell(x, y + r * 0.38, r * 0.2, r * 0.2, C("#4a1a2a"));
                    foreach (int s in new[] { -1, 1 })
                    {
                        ell(x + s * r * 0.32, y - r * 0.22, r * 0.16, r * 0.24, W_); ell(x + s * r * 0.3, y - r * 0.2, r * 0.08, r * 0.14, K_);
                        lines(K_, Math.Max(1, r * 0.04), false, x + s * r * 0.22, y - r * 0.45, x + s * r * 0.28, y - r * 0.55);
                        lines(K_, Math.Max(1, r * 0.04), false, x + s * r * 0.36, y - r * 0.46, x + s * r * 0.44, y - r * 0.55);
                    }
                    foreach (int s in new[] { -1, 1 }) fillPts(ch.bow, x, y - r * 0.8, x + s * r * 0.55, y - r * 1.1, x + s * r * 0.55, y - r * 0.6);
                    circ(x, y - r * 0.82, r * 0.15, ch.bow);
                    for (int i = 0; i < 3; i++) circ(x + (i - 1) * r * 0.35, y + r * 0.95, r * 0.1, C("#e52521"));
                    break;
                case "boo":
                    crown(x, y - r * 0.95, r * 0.75, C("#e52521"));
                    circ(x, y, r * 0.92, C("#f4f4ff"));
                    ell(x - r * 0.3, y - r * 0.2, r * 0.12, r * 0.2, K_); ell(x + r * 0.3, y - r * 0.2, r * 0.12, r * 0.2, K_);
                    ell(x, y + r * 0.35, r * 0.48, r * 0.3, C("#3a0a2a")); ell(x, y + r * 0.5, r * 0.25, r * 0.15, C("#a04ad8"));
                    foreach (int s in new[] { -1, 1 }) fillPts(W_, x + s * r * 0.38, y + r * 0.12, x + s * r * 0.28, y + r * 0.3, x + s * r * 0.18, y + r * 0.12);
                    break;
                case "saiyan": drawSaiyanHead(ch, x, y, r); break;
                case "person":
                    {
                        // amigos: cara humana con peinado, estatura y lentes opcionales
                        double fw = ch.thin ? 0.8 : 1, fh = ch.tall > 1 ? 1.1 : ch.tall < 1 ? 0.95 : 1;
                        ell(x - r * 0.8 * fw, y + r * 0.1, r * 0.15, r * 0.22, ch.skin); ell(x + r * 0.8 * fw, y + r * 0.1, r * 0.15, r * 0.22, ch.skin);
                        ell(x, y - r * 0.15, r * 0.86 * fw, r * 0.85 * fh, ch.hair);
                        ell(x, y + r * 0.12, r * 0.74 * fw, r * 0.84 * fh, ch.skin);
                        double top = y + r * 0.12 - r * 0.84 * fh;
                        if (ch.hairStyle == "messy") { for (int i = -2; i <= 2; i++) circ(x + i * r * 0.27 * fw, top + r * 0.18 + Math.Abs(i) * r * 0.06, r * 0.26, ch.hair); }
                        else halfEll(x, top + r * 0.42, r * 0.78 * fw, r * 0.42, ch.hair);
                        if (ch.hairStyle == "tuft") fillPts(ch.hair, x - r * 0.3, top + r * 0.2, x + r * 0.05, top - r * 0.3, x + r * 0.3, top + r * 0.22);
                        eyes(x, y + r * 0.04, r, C("#3a2a1a"), 0.27 * fw);
                        lines(ch.hair, Math.Max(1, r * 0.06), true, x - r * 0.38 * fw, y - r * 0.2, x - r * 0.16 * fw, y - r * 0.22);
                        lines(ch.hair, Math.Max(1, r * 0.06), true, x + r * 0.16 * fw, y - r * 0.22, x + r * 0.38 * fw, y - r * 0.2);
                        ell(x, y + r * 0.3, r * 0.1, r * 0.08, U.Shade(ch.skin, -0.18));
                        arc(C("#5a2a1a"), Math.Max(1, r * 0.05), x, y + r * 0.36, r * 0.22, 0.2 * Math.PI, 0.8 * Math.PI, true);
                        if (ch.glasses)
                        {
                            Color gl = C("#111111"); double gr = r * 0.2, gx = r * 0.27 * fw, lw = Math.Max(1, r * 0.06);
                            ellStroke(x - gx, y + r * 0.04, gr, gr * 0.85, gl, lw); ellStroke(x + gx, y + r * 0.04, gr, gr * 0.85, gl, lw);
                            lines(gl, lw, false, x - gx + gr, y + r * 0.02, x + gx - gr, y + r * 0.02);
                            lines(gl, lw, false, x - gx - gr, y + r * 0.0, x - r * 0.74 * fw, y - r * 0.04);
                            lines(gl, lw, false, x + gx + gr, y + r * 0.0, x + r * 0.74 * fw, y - r * 0.04);
                        }
                        break;
                    }
                case "shyguy":
                    ell(x, y, r * 0.95, r * 1.0, ch.hood);
                    ell(x, y + r * 0.08, r * 0.7, r * 0.75, W_);
                    ell(x - r * 0.24, y - r * 0.1, r * 0.11, r * 0.19, K_); ell(x + r * 0.24, y - r * 0.1, r * 0.11, r * 0.19, K_); ell(x, y + r * 0.38, r * 0.14, r * 0.11, K_);
                    break;
                case "dk":
                    ell(x, y, r * 1.0, r * 0.95, ch.fur);
                    fillPts(C("#5a3410"), x - r * 0.25, y - r * 0.8, x, y - r * 1.15, x + r * 0.1, y - r * 0.8);
                    ell(x - r * 0.27, y - r * 0.15, r * 0.32, r * 0.3, ch.face); ell(x + r * 0.27, y - r * 0.15, r * 0.32, r * 0.3, ch.face); ell(x, y + r * 0.32, r * 0.68, r * 0.5, ch.face);
                    foreach (int s in new[] { -1, 1 }) { ell(x + s * r * 0.25, y - r * 0.15, r * 0.1, r * 0.13, W_); circ(x + s * r * 0.23, y - r * 0.13, r * 0.06, K_); }
                    ell(x - r * 0.13, y + r * 0.18, r * 0.07, r * 0.05, C("#4a2a10")); ell(x + r * 0.13, y + r * 0.18, r * 0.07, r * 0.05, C("#4a2a10"));
                    arc(C("#4a2a10"), Math.Max(1, r * 0.06), x, y + r * 0.3, r * 0.32, 0.25 * Math.PI, 0.75 * Math.PI);
                    if (!ch.cap.IsEmpty) { halfEll(x, y - r * 0.45, r * 0.85, r * 0.55, ch.cap); ell(x, y - r * 0.45, r * 0.9, r * 0.1, U.Shade(ch.cap, -0.25)); circ(x, y - r * 0.72, r * 0.16, W_); }
                    break;
                case "bowser":
                    foreach (int s in new[] { -1, 1 }) fillPts(C("#fff8e0"), x + s * r * 0.4, y - r * 0.45, x + s * r * 0.9, y - r * 1.0, x + s * r * 0.72, y - r * 0.3);
                    ell(x, y - r * 0.45, r * 0.85, r * 0.5, ch.hair);
                    ell(x, y + r * 0.05, r * 0.82, r * 0.78, ch.body);
                    ell(x, y + r * 0.45, r * 0.72, r * 0.4, C("#f8e0a8"));
                    foreach (int s in new[] { -1, 1 }) { ell(x + s * r * 0.28, y - r * 0.12, r * 0.15, r * 0.16, W_); circ(x + s * r * 0.26, y - r * 0.1, r * 0.07, C("#c02020")); ell(x + s * r * 0.3, y - r * 0.32, r * 0.24, r * 0.07, ch.hair, s * 0.35); }
                    ell(x - r * 0.15, y + r * 0.25, r * 0.06, r * 0.05, C("#5a3a10")); ell(x + r * 0.15, y + r * 0.25, r * 0.06, r * 0.05, C("#5a3a10"));
                    foreach (int s in new[] { -1, 1 }) fillPts(W_, x + s * r * 0.35, y + r * 0.55, x + s * r * 0.25, y + r * 0.75, x + s * r * 0.15, y + r * 0.55);
                    if (ch.junior)
                    {
                        fillPts(W_, x - r * 0.7, y + r * 0.6, x + r * 0.7, y + r * 0.6, x, y + r * 1.05);
                        var zz = new List<double> { x - r * 0.4, y + r * 0.68 };
                        for (int i = 1; i <= 6; i++) { zz.Add(x - r * 0.4 + i * r * 0.133); zz.Add(y + r * (i % 2 == 1 ? 0.8 : 0.68)); }
                        lines(K_, Math.Max(1, r * 0.05), false, zz.ToArray());
                    }
                    break;
            }
            Restore(st);
        }

        // ===================== guerreros Z =====================
        static Dictionary<string, Ch> ssjCache = new Dictionary<string, Ch>();
        // el mismo personaje con el pelo dorado (Súper Saiyajin)
        static Ch ssjOf(Ch ch)
        {
            Ch c; if (ssjCache.TryGetValue(ch.id, out c)) return c;
            c = ch.Clone(); if (new[] { "goku", "vegeta", "gohan", "goten", "trunks", "broly" }.Contains(ch.hairStyle)) c.hair = C("#ffe14a");
            ssjCache[ch.id] = c; return c;
        }
        static void drawSaiyanHead(Ch ch, double x, double y, double r)
        {
            string hs = ch.hairStyle; Color hair = ch.hair, skin = ch.skin, dark = C("#1a1a1a");
            double hy = y - r * 0.1; bool spiky = hs == "goku" || hs == "goten";
            // pelo de atrás (y lo que va detrás de la cara)
            if (spiky || hs == "gohan")
            {
                int n = spiky ? 7 : 5; double R = r * (spiky ? 1.32 : 1.15);
                for (int i = 0; i < n; i++)
                {
                    double a = Math.PI + (i + 0.5) * Math.PI / n, a0 = a - 0.34, a1 = a + 0.34;
                    fillPts(hair, x + Math.Cos(a0) * r * 0.7, hy + Math.Sin(a0) * r * 0.7, x + Math.Cos(a) * R, hy + Math.Sin(a) * R, x + Math.Cos(a1) * r * 0.7, hy + Math.Sin(a1) * r * 0.7);
                }
                foreach (int s in new[] { -1, 1 }) fillPts(hair, x + s * r * 0.6, y - r * 0.45, x + s * r * (spiky ? 1.28 : 1.1), y + r * 0.02, x + s * r * 0.7, y + r * 0.1);
                ell(x, y - r * 0.32, r * 0.86, r * 0.62, hair);
            }
            else if (hs == "vegeta")
                fillPts(hair, x - r * 0.8, y - r * 0.05, x - r * 0.98, y - r * 0.85, x - r * 0.58, y - r * 0.72, x - r * 0.66, y - r * 1.45, x - r * 0.22, y - r * 1.02, x, y - r * 1.85,
                    x + r * 0.22, y - r * 1.02, x + r * 0.66, y - r * 1.45, x + r * 0.58, y - r * 0.72, x + r * 0.98, y - r * 0.85, x + r * 0.8, y - r * 0.05);
            else if (hs == "trunks" || hs == "a18") ell(x, y - r * 0.02, r * 0.95, r * (hs == "a18" ? 1.0 : 0.9), hair);
            else if (hs == "bulma") ell(x, y - r * 0.05, r * 0.95, r * 0.88, hair);
            else if (hs == "broly")
            {
                // melena salvaje
                for (int i = 0; i < 11; i++)
                {
                    double a = Math.PI * 0.85 + i * Math.PI * 1.3 / 10, a0 = a - 0.22, a1 = a + 0.22, R = r * 1.45;
                    fillPts(hair, x + Math.Cos(a0) * r * 0.8, hy + Math.Sin(a0) * r * 0.8, x + Math.Cos(a) * R, hy + Math.Sin(a) * R, x + Math.Cos(a1) * r * 0.8, hy + Math.Sin(a1) * r * 0.8);
                }
                ell(x, y - r * 0.2, r * 0.98, r * 0.85, hair);
            }
            else if (hs == "cell")
            {
                fillPts(ch.shirt, x - r * 1.05, y + r * 0.25, x - r * 0.95, y - r * 0.75, x - r * 0.48, y - r * 1.3, x, y - r * 0.98, x + r * 0.48, y - r * 1.3, x + r * 0.95, y - r * 0.75, x + r * 1.05, y + r * 0.25);
                foreach (var d in new[] { new[] { -0.7, -0.6 }, new[] { -0.45, -1.0 }, new[] { 0.45, -1.0 }, new[] { 0.7, -0.6 }, new[] { -0.85, -0.1 }, new[] { 0.85, -0.1 } }) circ(x + d[0] * r, y + d[1] * r, r * 0.07, dark);
            }
            else if (hs == "buu") lines(skin, r * 0.26, true, x - r * 0.05, y - r * 0.6, x + r * 0.12, y - r * 1.05, x + r * 0.42, y - r * 1.28, x + r * 0.62, y - r * 1.22);
            else if (hs == "bills") foreach (int s in new[] { -1, 1 }) { fillPts(skin, x + s * r * 0.3, y - r * 0.55, x + s * r * 0.92, y - r * 1.5, x + s * r * 0.82, y - r * 0.15); fillPts(C("#d888c8"), x + s * r * 0.42, y - r * 0.55, x + s * r * 0.86, y - r * 1.25, x + s * r * 0.78, y - r * 0.32); }
            if (hs == "piccolo") foreach (int s in new[] { -1, 1 }) fillPts(skin, x + s * r * 0.62, y - r * 0.05, x + s * r * 1.22, y - r * 0.5, x + s * r * 0.68, y + r * 0.32);
            else if (hs != "freezer" && hs != "cell" && hs != "bills" && hs != "trunks" && hs != "a18") { ell(x - r * 0.8, y + r * 0.1, r * 0.15, r * 0.22, skin); ell(x + r * 0.8, y + r * 0.1, r * 0.15, r * 0.22, skin); }
            // cara
            ell(x, y + r * 0.12, r * (hs == "buu" || hs == "jiren" ? 0.86 : 0.74), r * 0.84, skin);
            double top = y + r * 0.12 - r * 0.84;
            switch (hs)
            {
                case "goku": case "goten":
                    fillPts(hair, x - r * 0.76, top + r * 0.4, x - r * 0.5, top + r * 0.78, x - r * 0.3, top + r * 0.32, x - r * 0.05, top + r * 0.68, x + r * 0.12, top + r * 0.3,
                        x + r * 0.42, top + r * 0.74, x + r * 0.76, top + r * 0.4, x + r * 0.74, top - r * 0.05, x - r * 0.74, top - r * 0.05);
                    break;
                case "gohan":
                    halfEll(x, top + r * 0.4, r * 0.78, r * 0.42, hair);
                    fillPts(hair, x - r * 0.12, top + r * 0.3, x + r * 0.02, top + r * 0.9, x + r * 0.22, top + r * 0.3);
                    break;
                case "vegeta":
                    fillPts(hair, x - r * 0.76, top + r * 0.62, x - r * 0.72, top - r * 0.02, x + r * 0.72, top - r * 0.02, x + r * 0.76, top + r * 0.62, x + r * 0.34, top + r * 0.2, x, top + r * 0.52, x - r * 0.34, top + r * 0.2);
                    break;
                case "trunks":
                    foreach (int s in new[] { -1, 1 }) fillPts(hair, x + s * r * 0.8, top + r * 1.1, x + s * r * 0.82, top + r * 0.2, x + s * r * 0.3, top - r * 0.02, x + s * r * 0.04, top + r * 0.12, x + s * r * 0.45, top + r * 0.6);
                    break;
                case "a18":
                    halfEll(x, top + r * 0.38, r * 0.8, r * 0.42, hair);
                    fillPts(hair, x - r * 0.8, top + r * 0.35, x + r * 0.55, top + r * 0.22, x - r * 0.15, top + r * 0.72);
                    foreach (int s in new[] { -1, 1 }) fillPts(hair, x + s * r * 0.8, top + r * 0.3, x + s * r * 0.95, top + r * 1.45, x + s * r * 0.6, top + r * 1.2, x + s * r * 0.62, top + r * 0.5);
                    break;
                case "krilin":
                    for (int i = 0; i < 3; i++) for (int j = 0; j < 2; j++) circ(x + (i - 1) * r * 0.17, top + r * 0.22 + j * r * 0.15, r * 0.035, C("#b06a4a"));
                    break;
                case "piccolo":
                    circ(x, top - r * 0.05, r * 0.5, hair); ell(x, top + r * 0.25, r * 0.84, r * 0.42, hair);
                    ell(x, top + r * 0.48, r * 0.8, r * 0.12, ch.shirt);
                    break;
                case "freezer":
                    halfEll(x, top + r * 0.62, r * 0.78, r * 0.66, hair); ell(x - r * 0.25, top + r * 0.25, r * 0.16, r * 0.1, U.Shade(hair, 0.4));
                    foreach (int s in new[] { -1, 1 }) lines(hair, Math.Max(1, r * 0.06), true, x + s * r * 0.27, y + r * 0.18, x + s * r * 0.3, y + r * 0.42);
                    break;
                case "cell":
                    foreach (int s in new[] { -1, 1 }) lines(C("#8a3ab8"), Math.Max(1, r * 0.07), true, x + s * r * 0.3, y + r * 0.2, x + s * r * 0.36, y + r * 0.46);
                    break;
                case "bills":
                    rect(x - r * 0.45, y + r * 0.8, r * 0.9, r * 0.14, C("#ffd23a"));
                    break;
                case "broly":
                    fillPts(hair, x - r * 0.76, top + r * 0.35, x - r * 0.45, top + r * 0.62, x - r * 0.2, top + r * 0.25, x + r * 0.05, top + r * 0.55, x + r * 0.3, top + r * 0.22,
                        x + r * 0.55, top + r * 0.6, x + r * 0.76, top + r * 0.35, x + r * 0.74, top - r * 0.05, x - r * 0.74, top - r * 0.05);
                    rect(x - r * 0.62, top + r * 0.16, r * 1.24, r * 0.09, C("#ffd23a"));   // diadema dorada
                    break;
                case "bulma":
                    halfEll(x, top + r * 0.4, r * 0.8, r * 0.45, hair);
                    fillPts(hair, x - r * 0.8, top + r * 0.35, x - r * 0.1, top + r * 0.25, x - r * 0.55, top + r * 0.7);
                    break;
            }
            // ojos, cejas y boca
            Color eyeC = hs == "freezer" ? C("#c8102e") : hs == "a18" || hs == "bulma" ? C("#2a6ae8") : hs == "cell" ? C("#c83a8a") : hs == "bills" ? C("#d8b020") : dark;
            if (hs == "jiren") foreach (int s in new[] { -1, 1 }) { ell(x + s * r * 0.3, y + r * 0.02, r * 0.17, r * 0.25, dark); circ(x + s * r * 0.3 + r * 0.05, y - r * 0.07, r * 0.05, W_); }
            else eyes(x, y + r * 0.04, r, eyeC, 0.27);
            bool angry = hs == "vegeta" || hs == "piccolo" || hs == "freezer" || hs == "cell" || hs == "bills" || hs == "broly" || hs == "jiren";
            Color bc = hs == "piccolo" ? U.Shade(skin, -0.45) : hs == "roshi" ? hair : hs == "freezer" || hs == "krilin" || hs == "cell" || hs == "bills" || hs == "buu" || hs == "broly" ? dark : U.Shade(hair, -0.2);
            double lw = Math.Max(1, r * (hs == "roshi" ? 0.13 : 0.08)), b0 = angry ? 0.3 : 0.2, b1 = angry ? 0.13 : 0.22;
            if (hs != "jiren")
            {
                lines(bc, lw, true, x - r * 0.42, y - r * b0, x - r * 0.12, y - r * b1);
                lines(bc, lw, true, x + r * 0.12, y - r * b1, x + r * 0.42, y - r * b0);
            }
            if (hs != "freezer" && hs != "piccolo" && hs != "cell" && hs != "jiren") ell(x, y + r * 0.28, r * 0.08, r * 0.07, U.Shade(skin, -0.15));
            if (angry) lines(C("#5a2a1a"), Math.Max(1, r * 0.05), true, x - r * 0.18, y + r * 0.44, x + r * 0.18, y + r * 0.4);
            else arc(C("#5a2a1a"), Math.Max(1, r * 0.05), x, y + r * 0.36, r * 0.22, 0.2 * Math.PI, 0.8 * Math.PI, true);
            if (hs == "roshi")
            {
                // barba, bigote y lentes oscuros del Maestro Roshi
                fillPts(hair, x - r * 0.45, y + r * 0.42, x + r * 0.45, y + r * 0.42, x, y + r * 1.45);
                ell(x - r * 0.2, y + r * 0.4, r * 0.24, r * 0.09, hair); ell(x + r * 0.2, y + r * 0.4, r * 0.24, r * 0.09, hair);
                ell(x - r * 0.27, y + r * 0.04, r * 0.21, r * 0.15, dark); ell(x + r * 0.27, y + r * 0.04, r * 0.21, r * 0.15, dark);
                lines(dark, Math.Max(1, r * 0.05), false, x - r * 0.08, y + r * 0.02, x + r * 0.08, y + r * 0.02);
            }
        }

    }
}
