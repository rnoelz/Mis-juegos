using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SuperKart
{
    // Dibujos: personajes, karts, objetos, decorados y peligros
    static partial class SK
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

        // ===================== piloto visto de espaldas =====================
        static void drawDriverBack(Ch ch, double x, double y, double w)
        {
            double big = ((ch.kind == "bowser" && !ch.junior) || (ch.kind == "dk" && ch.cap.IsEmpty) || ch.big || ch.kind == "boo") ? 1.2 : 1;
            double tw = w * 0.32 * big, th = w * 0.22, hr = w * 0.15 * big, hy = y - th - hr * 0.7;
            Color body = U.First(ch.over, ch.dress, ch.vest, ch.body, ch.hood, ch.fur);
            Color arm = !ch.shirt.IsEmpty ? ch.shirt : ((ch.kind == "koopa" || ch.kind == "bowser") ? ch.body : body);
            circ(x - tw * 0.62, y - th * 0.45, w * 0.06, arm); circ(x + tw * 0.62, y - th * 0.45, w * 0.06, arm);
            switch (ch.kind)
            {
                case "plumber":
                    rrFill(x - tw / 2, y - th, tw, th, tw * 0.25, ch.shirt);
                    rect(x - tw * 0.42, y - th * 0.55, tw * 0.84, th * 0.55, ch.over); rect(x - tw * 0.3, y - th, tw * 0.12, th * 0.5, ch.over); rect(x + tw * 0.18, y - th, tw * 0.12, th * 0.5, ch.over);
                    circ(x, hy, hr, ch.hair); circ(x - hr * 0.95, hy + hr * 0.1, hr * 0.25, ch.skin); circ(x + hr * 0.95, hy + hr * 0.1, hr * 0.25, ch.skin);
                    halfEll(x, hy - hr * 0.05, hr * 1.05, hr * 1.0, ch.hat); ell(x, hy - hr * 0.05, hr * 1.08, hr * 0.15, U.Shade(ch.hat, -0.25));
                    break;
                case "saiyan":
                    {
                        string hs = ch.hairStyle; double tf = ch.tall, th2 = th * tf, tw2 = tw * (tf < 1 ? 0.9 : 1), hr2 = hr * (tf < 1 ? 0.92 : 1), hy2 = y - th2 - hr2 * 0.7;
                        if (hs == "piccolo") fillPts(ch.vest, x - tw2 * 0.75, y - th2 * 0.95, x + tw2 * 0.75, y - th2 * 0.95, x + tw2 * 0.85, y, x - tw2 * 0.85, y);   // capa
                        rrFill(x - tw2 / 2, y - th2, tw2, th2, tw2 * 0.28, ch.shirt);
                        if (hs == "vegeta") { rrFill(x - tw2 * 0.5, y - th2, tw2, th2 * 0.62, tw2 * 0.2, ch.vest); ell(x - tw2 * 0.45, y - th2 * 0.95, tw2 * 0.2, th2 * 0.14, C("#ffd23a")); ell(x + tw2 * 0.45, y - th2 * 0.95, tw2 * 0.2, th2 * 0.14, C("#ffd23a")); }
                        else if (hs == "freezer") { ell(x, y - th2 * 0.75, tw2 * 0.4, th2 * 0.22, ch.vest); fillPts(ch.skin, x + tw2 * 0.3, y - th2 * 0.2, x + tw2 * 1.2, y - th2 * 0.7, x + tw2 * 0.4, y); }
                        else if (hs == "roshi") { ell(x, y - th2 * 0.5, tw2 * 0.62, th2 * 0.62, ch.vest); ellStroke(x, y - th2 * 0.5, tw2 * 0.4, th2 * 0.4, U.Shade(ch.vest, -0.3), Math.Max(1, w * 0.012)); }   // caparazón de tortuga
                        else if (hs != "piccolo") { rect(x - tw2 / 2, y - th2 * 0.32, tw2, th2 * 0.12, ch.vest); rect(x - tw2 * 0.12, y - th2, tw2 * 0.24, th2 * 0.2, ch.vest); }
                        circ(x, hy2, hr2, ch.skin);
                        if (hs == "trunks" || hs == "a18" || hs == "bulma") ell(x, hy2 + hr2 * (hs == "a18" ? 0.2 : 0.05), hr2 * 1.1, hr2 * (hs == "a18" ? 1.0 : 0.95), ch.hair);
                        else if (hs == "broly")
                        {
                            for (int i = 0; i < 12; i++) { double a = i * Math.PI * 2 / 12, a0 = a - 0.25, a1 = a + 0.25, R = hr2 * 1.7; fillPts(ch.hair, x + Math.Cos(a0) * hr2 * 0.9, hy2 + Math.Sin(a0) * hr2 * 0.9, x + Math.Cos(a) * R, hy2 + Math.Sin(a) * R * (Math.Sin(a) > 0 ? 1.2 : 1), x + Math.Cos(a1) * hr2 * 0.9, hy2 + Math.Sin(a1) * hr2 * 0.9); }
                            circ(x, hy2, hr2 * 1.1, ch.hair);
                        }
                        else if (hs == "cell")
                        {
                            fillPts(ch.shirt, x - hr2 * 1.15, hy2 + hr2 * 0.3, x - hr2 * 1.0, hy2 - hr2 * 0.8, x - hr2 * 0.5, hy2 - hr2 * 1.4, x, hy2 - hr2 * 1.05, x + hr2 * 0.5, hy2 - hr2 * 1.4, x + hr2 * 1.0, hy2 - hr2 * 0.8, x + hr2 * 1.15, hy2 + hr2 * 0.3);
                            foreach (var d in new[] { new[] { -0.6, -0.6 }, new[] { 0.6, -0.6 }, new[] { 0.0, -0.3 }, new[] { -0.3, 0.1 }, new[] { 0.35, 0.05 } }) circ(x + d[0] * hr2, hy2 + d[1] * hr2, hr2 * 0.1, C("#1a1a1a"));
                        }
                        else if (hs == "buu") lines(ch.skin, hr2 * 0.3, true, x, hy2 - hr2 * 0.7, x + hr2 * 0.2, hy2 - hr2 * 1.25, x + hr2 * 0.6, hy2 - hr2 * 1.4);
                        else if (hs == "bills") foreach (int s in new[] { -1, 1 }) fillPts(ch.skin, x + s * hr2 * 0.3, hy2 - hr2 * 0.6, x + s * hr2 * 0.95, hy2 - hr2 * 1.65, x + s * hr2 * 0.9, hy2 - hr2 * 0.1);
                        if (hs == "goku" || hs == "goten" || hs == "gohan")
                        {
                            int n = hs != "gohan" ? 7 : 5; double R = hr2 * (hs != "gohan" ? 1.65 : 1.4);
                            for (int i = 0; i < n; i++) { double a = Math.PI * 0.95 + (i + 0.5) * Math.PI * 1.1 / n, a0 = a - 0.3, a1 = a + 0.3; fillPts(ch.hair, x + Math.Cos(a0) * hr2 * 0.8, hy2 + Math.Sin(a0) * hr2 * 0.8, x + Math.Cos(a) * R, hy2 + Math.Sin(a) * R, x + Math.Cos(a1) * hr2 * 0.8, hy2 + Math.Sin(a1) * hr2 * 0.8); }
                            circ(x, hy2, hr2 * 1.02, ch.hair);
                        }
                        else if (hs == "vegeta") { fillPts(ch.hair, x - hr2 * 0.95, hy2 + hr2 * 0.3, x - hr2 * 0.7, hy2 - hr2 * 1.3, x, hy2 - hr2 * 2.1, x + hr2 * 0.7, hy2 - hr2 * 1.3, x + hr2 * 0.95, hy2 + hr2 * 0.3); }
                        else if (hs == "piccolo") { circ(x, hy2 - hr2 * 0.2, hr2 * 1.1, ch.hair); circ(x, hy2 - hr2 * 0.8, hr2 * 0.6, ch.hair); ell(x, hy2 + hr2 * 0.2, hr2 * 1.05, hr2 * 0.2, ch.shirt); }
                        else if (hs == "freezer") halfEll(x, hy2 + hr2 * 0.2, hr2 * 1.02, hr2 * 1.15, ch.hair);
                        circ(x - hr2 * 0.97, hy2 + hr2 * 0.1, hr2 * 0.24, ch.skin); circ(x + hr2 * 0.97, hy2 + hr2 * 0.1, hr2 * 0.24, ch.skin);
                        break;
                    }
                case "person":
                    {
                        double tf = ch.tall, th2 = th * tf, tw2 = tw * (ch.thin ? 0.88 : 1) * (tf < 1 ? 0.9 : 1), hr2 = hr * (tf < 1 ? 0.9 : 1), hy2 = y - th2 - hr2 * 0.7;
                        rrFill(x - tw2 / 2, y - th2, tw2, th2, tw2 * 0.28, ch.shirt);
                        rect(x - tw2 * 0.12, y - th2, tw2 * 0.24, th2 * 0.12, U.Shade(ch.shirt, -0.25));
                        circ(x - hr2 * 0.95, hy2 + hr2 * 0.1, hr2 * 0.25, ch.skin); circ(x + hr2 * 0.95, hy2 + hr2 * 0.1, hr2 * 0.25, ch.skin);
                        circ(x, hy2, hr2, ch.skin);
                        ell(x, hy2 - hr2 * 0.12, hr2 * 1.02, hr2 * 0.92, ch.hair);
                        if (ch.hairStyle == "tuft") fillPts(ch.hair, x - hr2 * 0.35, hy2 - hr2 * 0.85, x + hr2 * 0.05, hy2 - hr2 * 1.45, x + hr2 * 0.35, hy2 - hr2 * 0.8);
                        if (ch.hairStyle == "messy") for (int i = -1; i <= 1; i++) circ(x + i * hr2 * 0.55, hy2 - hr2 * 0.85, hr2 * 0.35, ch.hair);
                        if (ch.glasses) { lines(C("#111111"), Math.Max(1, hr2 * 0.12), false, x - hr2 * 1.05, hy2 + hr2 * 0.05, x - hr2 * 0.75, hy2 + hr2 * 0.1); lines(C("#111111"), Math.Max(1, hr2 * 0.12), false, x + hr2 * 1.05, hy2 + hr2 * 0.05, x + hr2 * 0.75, hy2 + hr2 * 0.1); }
                        break;
                    }
                case "princess":
                    rrFill(x - tw / 2, y - th, tw, th, tw * 0.3, ch.dress);
                    ell(x, hy + hr * 0.35, hr * 1.2, hr * 1.35, ch.hair);
                    crown(x, hy - hr * 0.95, hr * 0.9, ch.gem);
                    break;
                case "toad":
                    rrFill(x - tw / 2, y - th, tw, th, tw * 0.3, ch.vest);
                    if (ch.pigtails) { ell(x - hr * 1.3, hy + hr * 0.9, hr * 0.35, hr * 0.55, ch.cap); ell(x + hr * 1.3, hy + hr * 0.9, hr * 0.35, hr * 0.55, ch.cap); }
                    ell(x, hy, hr * 1.55, hr * 1.15, ch.cap.IsEmpty ? W_ : ch.cap); circ(x, hy - hr * 0.3, hr * 0.45, ch.spot); circ(x - hr * 1.0, hy + hr * 0.1, hr * 0.35, ch.spot); circ(x + hr * 1.0, hy + hr * 0.1, hr * 0.35, ch.spot);
                    break;
                case "yoshi":
                    rrFill(x - tw / 2, y - th, tw, th, tw * 0.3, ch.body);
                    rect(x - tw * 0.4, y - th * 0.5, tw * 0.8, th * 0.3, C("#e52521"));
                    circ(x, hy, hr * 1.05, ch.body);
                    for (int i = 0; i < 3; i++) { double sy = hy - hr * 0.9 + i * hr * 0.6; fillPts(ch.spike, x - hr * 0.22, sy + hr * 0.3, x, sy - hr * 0.1, x + hr * 0.22, sy + hr * 0.3); }
                    break;
                case "koopa":
                    circ(x, hy, hr, ch.body);
                    ell(x, y - th * 0.55, tw * 0.65, th * 0.75, W_); ell(x, y - th * 0.6, tw * 0.58, th * 0.68, ch.shell);
                    lines(U.Shade(ch.shell, -0.3), Math.Max(1, w * 0.012), false, x, y - th * 1.25, x, y);
                    lines(U.Shade(ch.shell, -0.3), Math.Max(1, w * 0.012), false, x - tw * 0.5, y - th * 0.6, x + tw * 0.5, y - th * 0.6);
                    break;
                case "shyguy":
                    rrFill(x - tw / 2, y - th, tw, th, tw * 0.3, ch.hood); circ(x, hy, hr * 1.05, ch.hood);
                    rect(x - tw / 2, y - th * 0.35, tw, th * 0.12, C("#222222"));
                    break;
                case "dk":
                    rrFill(x - tw / 2, y - th, tw, th, tw * 0.3, ch.fur); circ(x, hy, hr * 1.05, ch.fur);
                    fillPts(C("#5a3410"), x - hr * 0.3, hy - hr * 0.8, x, hy - hr * 1.4, x + hr * 0.2, hy - hr * 0.8);
                    if (!ch.cap.IsEmpty) { halfEll(x, hy - hr * 0.1, hr * 1.05, hr * 0.95, ch.cap); ell(x, hy - hr * 0.1, hr * 1.08, hr * 0.14, U.Shade(ch.cap, -0.25)); }
                    else rect(x - tw * 0.08, y - th, tw * 0.16, th * 0.3, C("#e52521"));
                    break;
                case "birdo":
                    rrFill(x - tw / 2, y - th, tw, th, tw * 0.3, ch.body);
                    circ(x, hy, hr * 1.05, ch.body);
                    foreach (int s in new[] { -1, 1 }) fillPts(ch.bow, x, hy - hr * 0.9, x + s * hr * 0.8, hy - hr * 1.4, x + s * hr * 0.8, hy - hr * 0.6);
                    circ(x, hy - hr * 0.9, hr * 0.22, ch.bow);
                    break;
                case "boo":
                    ell(x, y - th * 0.6, tw * 0.6, th * 0.9, C("#f4f4ff"));
                    circ(x, hy + hr * 0.2, hr * 1.1, C("#f4f4ff"));
                    crown(x, hy - hr * 1.1, hr * 1.0, C("#e52521"));
                    break;
                case "bowser":
                    foreach (int s in new[] { -1, 1 }) fillPts(C("#fff8e0"), x + s * hr * 0.5, hy - hr * 0.5, x + s * hr * 1.1, hy - hr * 1.3, x + s * hr * 0.9, hy - hr * 0.2);
                    ell(x, hy + hr * 0.1, hr, hr * 0.95, ch.body);
                    ell(x, hy - hr * 0.5, hr * 0.8, hr * 0.55, ch.hair);
                    ell(x, y - th * 0.55, tw * 0.68, th * 0.8, W_); ell(x, y - th * 0.6, tw * 0.6, th * 0.72, ch.shell);
                    foreach (var d in new[] { new[] { 0, -0.95 }, new[] { -0.35, -0.55 }, new[] { 0.35, -0.55 } })
                    { double sx = x + d[0] * tw, sy = y + d[1] * th; fillPts(W_, sx - w * 0.03, sy + w * 0.02, sx, sy - w * 0.05, sx + w * 0.03, sy + w * 0.02); }
                    break;
            }
        }

        // ===================== objetos =====================
        static void drawMushroom(double x, double y, double s, bool gold = false)
        {
            rrFill(x - s * 0.24, y, s * 0.48, s * 0.42, s * 0.12, C("#ffe8c4"));
            ell(x - s * 0.09, y + s * 0.2, s * 0.04, s * 0.08, K_); ell(x + s * 0.09, y + s * 0.2, s * 0.04, s * 0.08, K_);
            halfEll(x, y + s * 0.05, s * 0.52, s * 0.5, gold ? C("#ffc800") : C("#e52521"));
            Color sp = gold ? C("#fff4a0") : W_;
            circ(x, y - s * 0.22, s * 0.14, sp); circ(x - s * 0.33, y - s * 0.04, s * 0.1, sp); circ(x + s * 0.33, y - s * 0.04, s * 0.1, sp);
        }
        static void drawShell(double x, double y, double s, Color col, bool wings = false)
        {
            if (wings) { ell(x - s * 0.5, y - s * 0.5, s * 0.32, s * 0.13, W_, -0.5); ell(x + s * 0.5, y - s * 0.5, s * 0.32, s * 0.13, W_, 0.5); }
            ell(x, y - s * 0.13, s * 0.5, s * 0.14, W_);
            halfEll(x, y - s * 0.15, s * 0.46, s * 0.44, col);
            Color lt = U.Shade(col, 0.35);
            circ(x, y - s * 0.38, s * 0.11, lt); circ(x - s * 0.25, y - s * 0.27, s * 0.08, lt); circ(x + s * 0.25, y - s * 0.27, s * 0.08, lt);
        }
        static void drawBanana(double x, double y, double s)
        {
            arc(C("#b88a00"), Math.Max(1, s * 0.26), x, y - s * 0.75, s * 0.5, Math.PI * 0.2, Math.PI * 0.8, true);
            arc(C("#ffe23a"), Math.Max(1, s * 0.17), x, y - s * 0.75, s * 0.5, Math.PI * 0.2, Math.PI * 0.8, true);
        }
        static void drawStar(double x, double y, double R)
        {
            var p = starPts(x, y, R, R * 0.45);
            g.FillPolygon(B(C("#ffd800")), p); g.DrawPolygon(P(C("#c88a00"), Math.Max(1, R * 0.08)), p);
            ell(x - R * 0.15, y - R * 0.05, R * 0.06, R * 0.14, K_); ell(x + R * 0.15, y - R * 0.05, R * 0.06, R * 0.14, K_);
        }
        static void drawCoinIcon(double x, double y, double s)
        {
            ell(x, y, s * 0.38, s * 0.5, C("#c88a00")); ell(x, y, s * 0.3, s * 0.42, C("#ffd23a"));
            rect(x - s * 0.06, y - s * 0.22, s * 0.12, s * 0.44, C("#e8a800"));
        }
        static void drawBolt(double x, double y, double s)
        {
            double[] q = { 0.15, -0.5, -0.25, 0.05, 0, 0.05, -0.15, 0.5, 0.3, -0.1, 0.05, -0.1, 0.25, -0.5 };
            var xy = new double[q.Length];
            for (int i = 0; i < q.Length; i += 2) { xy[i] = x + q[i] * s; xy[i + 1] = y + q[i + 1] * s; }
            var p = pts(xy);
            g.FillPolygon(B(C("#ffe600")), p); g.DrawPolygon(P(C("#a88000"), Math.Max(1, s * 0.04)), p);
        }
        static void drawBulletIcon(double x, double y, double s)
        {
            rrFill(x - s * 0.42, y - s * 0.25, s * 0.6, s * 0.5, s * 0.05, C("#222222")); circ(x + s * 0.18, y, s * 0.25, C("#222222"));
            rect(x - s * 0.36, y - s * 0.25, s * 0.06, s * 0.5, C("#dddddd")); ell(x + s * 0.12, y - s * 0.06, s * 0.07, s * 0.1, W_);
            rect(x - s * 0.5, y - s * 0.35, s * 0.12, s * 0.2, K_); rect(x - s * 0.5, y + s * 0.15, s * 0.12, s * 0.2, K_);
        }
        static void drawBombIcon(double x, double y, double s)
        {
            circ(x, y + s * 0.05, s * 0.38, C("#1a1a1a")); circ(x - s * 0.12, y - s * 0.08, s * 0.08, U.RGBA(255, 255, 255, 0.35));
            ell(x - s * 0.12, y, s * 0.05, s * 0.1, W_); ell(x + s * 0.12, y, s * 0.05, s * 0.1, W_);
            rect(x - s * 0.06, y - s * 0.42, s * 0.12, s * 0.1, C("#c8c8c8"));
            quadLine(C("#8a6a3a"), Math.Max(1, s * 0.05), x, y - s * 0.42, x + s * 0.15, y - s * 0.55, x + s * 0.1, y - s * 0.6);
            circ(x + s * 0.1, y - s * 0.62, s * (0.06 + U.R() * 0.03), C("#ffb02a"));
            ell(x - s * 0.2, y + s * 0.45, s * 0.14, s * 0.06, C("#ffb02a")); ell(x + s * 0.2, y + s * 0.45, s * 0.14, s * 0.06, C("#ffb02a"));
        }
        static void drawBlooperIcon(double x, double y, double s)
        {
            using (var p = new GraphicsPath())
            {
                quadTo(p, x, y - s * 0.5, x + s * 0.38, y - s * 0.1, x + s * 0.25, y + s * 0.15);
                p.AddLine(F(x + s * 0.25), F(y + s * 0.15), F(x - s * 0.25), F(y + s * 0.15));
                quadTo(p, x - s * 0.25, y + s * 0.15, x - s * 0.38, y - s * 0.1, x, y - s * 0.5);
                g.FillPath(B(C("#f4f4ff")), p);
            }
            foreach (double o in new[] { -0.18, -0.06, 0.06, 0.18 }) quadLine(C("#f4f4ff"), Math.Max(1, s * 0.08), x + o * s, y + s * 0.12, x + o * s * 1.6, y + s * 0.3, x + o * s, y + s * 0.48);
            ell(x - s * 0.1, y - s * 0.12, s * 0.05, s * 0.09, K_); ell(x + s * 0.1, y - s * 0.12, s * 0.05, s * 0.09, K_);
        }
        static void drawBooIcon(double x, double y, double s)
        {
            circ(x, y, s * 0.4, C("#f4f4ff")); fillPts(C("#f4f4ff"), x + s * 0.25, y + s * 0.28, x + s * 0.5, y + s * 0.45, x + s * 0.38, y + s * 0.12);
            ell(x - s * 0.13, y - s * 0.1, s * 0.05, s * 0.1, K_); ell(x + s * 0.13, y - s * 0.1, s * 0.05, s * 0.1, K_);
            ell(x, y + s * 0.15, s * 0.18, s * 0.12, C("#3a0a1a")); ell(x, y + s * 0.21, s * 0.09, s * 0.05, C("#ff5a7a"));
        }
        static void drawHornIcon(double x, double y, double s)
        {
            fillPts(C("#ffc800"), x - s * 0.4, y - s * 0.08, x + s * 0.15, y - s * 0.35, x + s * 0.15, y + s * 0.35, x - s * 0.4, y + s * 0.08);
            ell(x + s * 0.18, y, s * 0.12, s * 0.38, C("#e8a000")); ell(x + s * 0.18, y, s * 0.07, s * 0.28, C("#5a3a00"));
            foreach (double r2 in new[] { 0.45, 0.6 }) arc(C("#ffc800"), Math.Max(1, s * 0.05), x + s * 0.2, y, s * r2, -0.6, 0.6);
            rect(x - s * 0.48, y - s * 0.12, s * 0.1, s * 0.24, C("#d8d8e8"));
        }
        static void drawFlower(double x, double y, double R)
        {
            lines(C("#2fa84a"), R * 0.18, false, x, y, x, y + R * 1.3);
            ell(x - R * 0.4, y + R * 1.0, R * 0.4, R * 0.16, C("#4ad86a"), -0.4); ell(x + R * 0.4, y + R * 1.0, R * 0.4, R * 0.16, C("#4ad86a"), 0.4);
            ell(x, y, R, R * 0.7, C("#ff5a1a")); ell(x, y, R * 0.72, R * 0.48, C("#ffd84a")); ell(x, y, R * 0.45, R * 0.28, W_);
            ell(x - R * 0.14, y, R * 0.06, R * 0.14, K_); ell(x + R * 0.14, y, R * 0.06, R * 0.14, K_);
        }
        static void drawItemIcon(string type, double x, double y, double s)
        {
            switch (type)
            {
                case "mushroom": drawMushroom(x, y - s * 0.05, s * 0.8); break;
                case "triple": drawMushroom(x - s * 0.22, y + s * 0.08, s * 0.45); drawMushroom(x + s * 0.22, y + s * 0.08, s * 0.45); drawMushroom(x, y - s * 0.22, s * 0.45); break;
                case "golden": drawMushroom(x, y - s * 0.05, s * 0.8, true); break;
                case "banana": drawBanana(x, y + s * 0.38, s * 0.95); break;
                case "triple_banana": drawBanana(x - s * 0.2, y + s * 0.3, s * 0.55); drawBanana(x + s * 0.2, y + s * 0.3, s * 0.55); drawBanana(x, y + s * 0.05, s * 0.55); break;
                case "green": drawShell(x, y + s * 0.3, s * 0.85, C("#2fb44a")); break;
                case "triple_green": drawShell(x - s * 0.22, y + s * 0.32, s * 0.48, C("#2fb44a")); drawShell(x + s * 0.22, y + s * 0.32, s * 0.48, C("#2fb44a")); drawShell(x, y + s * 0.02, s * 0.48, C("#2fb44a")); break;
                case "red": drawShell(x, y + s * 0.3, s * 0.85, C("#e52521")); break;
                case "blue": drawShell(x, y + s * 0.32, s * 0.75, C("#2a6ae8"), true); break;
                case "star": drawStar(x, y, s * 0.45); break;
                case "lightning": drawBolt(x, y, s * 0.9); break;
                case "bullet": drawBulletIcon(x, y, s * 0.9); break;
                case "coin": drawCoinIcon(x - s * 0.14, y + s * 0.05, s * 0.55); drawCoinIcon(x + s * 0.14, y - s * 0.05, s * 0.55); break;
                case "bomb": drawBombIcon(x, y, s * 0.85); break;
                case "blooper": drawBlooperIcon(x, y, s * 0.9); break;
                case "boo": drawBooIcon(x, y, s * 0.85); break;
                case "horn": drawHornIcon(x, y, s * 0.9); break;
            }
        }
        static void drawCoin(double x, double y, double s)
        {
            double sx = Math.Abs(Math.Cos(T * 4)) * s * 0.38 + s * 0.05;
            ell(x, y, sx, s / 2, C("#c88a00")); ell(x, y, sx * 0.8, s * 0.42, C("#ffd23a"));
            if (sx > 3) rect(x - sx * 0.15, y - s * 0.22, sx * 0.3, s * 0.44, C("#e8a800"));
        }
        static void drawBox(double x, double y, double s)
        {
            double by = y - s * 0.75 - Math.Sin(T * 3) * s * 0.08;
            var st = Save(); Translate(x, by); Rotate(Math.Sin(T * 1.5) * 0.25);
            rrFill(-s / 2, -s / 2, s, s, s * 0.15, U.A(U.Hsl(T * 90, 0.85, 0.68), 0.8));
            rrFill(-s * 0.4, -s * 0.4, s * 0.8, s * 0.35, s * 0.1, U.RGBA(255, 255, 255, 0.25));
            rrStroke(-s / 2, -s / 2, s, s, s * 0.15, U.RGBA(255, 255, 255, 0.9), Math.Max(1, s * 0.06));
            if (s > 8) txt("?", 0, s * 0.03, s * 0.6, W_, "center", Color.Black, 0);
            Restore(st);
        }
        static void drawProj(Proj p, double x, double y, double k)
        {
            double s = 170 * k;
            switch (p.type)
            {
                case "banana": drawBanana(x, y, s); break;
                case "fire": { double fy = y - 140 * k; circ(x, fy + 40 * k, s * 0.6, U.RGBA(255, 90, 20, 0.35)); circ(x, fy, s * 0.5, C("#ff5a1a")); circ(x, fy - s * 0.08, s * 0.32, C("#ffb02a")); circ(x, fy - s * 0.12, s * 0.16, C("#fff2a0")); break; }
                case "egg": { double fy = y - 120 * k; ell(x, fy, s * 0.36, s * 0.46, W_); circ(x - s * 0.12, fy - s * 0.1, s * 0.1, C("#4cc84c")); circ(x + s * 0.14, fy + s * 0.12, s * 0.12, C("#4cc84c")); break; }
                case "green": drawShell(x, y, s, C("#2fb44a")); break;
                case "red": drawShell(x, y, s, C("#e52521")); break;
                case "blue": drawShell(x, y - 300 * k, s * 1.3, C("#2a6ae8"), true); break;
                case "bomb":
                    {
                        double hop = p.speed > 0 ? Math.Abs(Math.Sin(p.age * 9)) * 200 * k : 0;
                        ell(x, y, 110 * k, 25 * k, U.RGBA(0, 0, 0, 0.3)); drawBombIcon(x, y - 150 * k - hop, 300 * k);
                        if (p.life < 1 && ((int)Math.Floor(T * 12)) % 2 == 1) circ(x, y - 150 * k - hop, 140 * k, U.RGBA(255, 60, 30, 0.45));
                        break;
                    }
                case "boom":
                    {
                        double pr = 1 - p.life / 0.7;
                        Color bc = p.col.IsEmpty ? U.C("#50a0ff") : p.col;
                        circ(x, y - 300 * k, (600 + 1600 * pr) * k, U.A(bc, 0.7 * (1 - pr)));
                        circ(x, y - 300 * k, (300 + 900 * pr) * k, U.RGBA(255, 255, 255, 0.8 * (1 - pr)));
                        break;
                    }
            }
        }

        // ===================== peligros =====================
        static void drawHazard(Hazard h, double x, double y, double s)
        {
            switch (h.type)
            {
                case "goomba":
                    {
                        double b = Math.Abs(Math.Sin(T * 8 + h.ph)) * 30 * s;
                        ell(x, y, 170 * s, 35 * s, U.RGBA(0, 0, 0, 0.3));
                        ell(x - 90 * s, y - 30 * s - b * 0.3, 90 * s, 45 * s, C("#2a1a10")); ell(x + 90 * s, y - 30 * s - (30 * s - b) * 0.3, 90 * s, 45 * s, C("#2a1a10"));
                        ell(x, y - 150 * s - b, 140 * s, 110 * s, C("#f0d8a8"));
                        halfEll(x, y - 200 * s - b, 260 * s, 230 * s, C("#8a4a1a")); ell(x, y - 200 * s - b, 260 * s, 60 * s, C("#8a4a1a"));
                        ell(x - 70 * s, y - 250 * s - b, 40 * s, 65 * s, W_); ell(x + 70 * s, y - 250 * s - b, 40 * s, 65 * s, W_);
                        circ(x - 62 * s, y - 240 * s - b, 24 * s, K_); circ(x + 62 * s, y - 240 * s - b, 24 * s, K_);
                        fillPts(C("#2a1a10"), x - 130 * s, y - 330 * s - b, x - 20 * s, y - 300 * s - b, x - 25 * s, y - 315 * s - b);
                        fillPts(C("#2a1a10"), x + 130 * s, y - 330 * s - b, x + 20 * s, y - 300 * s - b, x + 25 * s, y - 315 * s - b);
                        break;
                    }
                case "crab":
                    {
                        double cl = Math.Sin(T * 6 + h.ph) * 0.3;
                        ell(x, y, 200 * s, 35 * s, U.RGBA(0, 0, 0, 0.3));
                        foreach (int d in new[] { -1, 1 }) for (int i = 0; i < 3; i++) lines(C("#c8301a"), Math.Max(1, 30 * s), false, x + d * 80 * s, y - 90 * s, x + d * (170 + i * 30) * s, y - (20 + i * 10) * s);
                        ell(x, y - 130 * s, 190 * s, 110 * s, C("#e8402a"));
                        foreach (int d in new[] { -1, 1 }) { circ(x + d * 260 * s, y - 220 * s, 80 * s, C("#e8402a")); ell(x + d * 290 * s, y - 260 * s, 40 * s, 25 * s, C("#ff7a5a"), d * cl); }
                        rect(x - 70 * s, y - 300 * s, 25 * s, 90 * s, C("#e8402a")); rect(x + 45 * s, y - 300 * s, 25 * s, 90 * s, C("#e8402a"));
                        circ(x - 58 * s, y - 310 * s, 40 * s, W_); circ(x + 58 * s, y - 310 * s, 40 * s, W_); circ(x - 58 * s, y - 310 * s, 20 * s, K_); circ(x + 58 * s, y - 310 * s, 20 * s, K_);
                        break;
                    }
                case "car":
                    {
                        double w = 560 * s, hh = 330 * s;
                        ell(x, y, w * 0.55, w * 0.08, U.RGBA(0, 0, 0, 0.35));
                        rrFill(x - w * 0.48, y - w * 0.18, w * 0.18, w * 0.18, w * 0.04, C("#1a1a1a")); rrFill(x + w * 0.3, y - w * 0.18, w * 0.18, w * 0.18, w * 0.04, C("#1a1a1a"));
                        rrFill(x - w / 2, y - hh, w, hh * 0.62, w * 0.06, h.col); rrFill(x - w * 0.36, y - hh * 1.35, w * 0.72, hh * 0.5, w * 0.1, h.col);
                        rrFill(x - w * 0.3, y - hh * 1.28, w * 0.6, hh * 0.36, w * 0.06, C("#2a3a5a"));
                        rect(x - w * 0.46, y - hh * 0.88, w * 0.14, hh * 0.14, C("#ff2a2a")); rect(x + w * 0.32, y - hh * 0.88, w * 0.14, hh * 0.14, C("#ff2a2a"));
                        rect(x - w * 0.12, y - hh * 0.62, w * 0.24, hh * 0.12, C("#eeeeee"));
                        break;
                    }
                case "podoboo":
                    {
                        double hy = y - 120 * s - h.h * s;
                        ell(x, y, 140 * s * (1 - h.h / 1400), 30 * s, U.RGBA(0, 0, 0, 0.35));
                        circ(x, hy + 60 * s, 120 * s, U.A(C("#ff7a1a"), 0.5));
                        circ(x, hy, 130 * s, C("#ff5a1a")); circ(x, hy - 15 * s, 95 * s, C("#ffb02a")); circ(x, hy - 25 * s, 55 * s, C("#fff2a0"));
                        ell(x - 40 * s, hy - 10 * s, 18 * s, 30 * s, K_); ell(x + 40 * s, hy - 10 * s, 18 * s, 30 * s, K_);
                        break;
                    }
            }
        }

        // ===================== decorados =====================
        static void drawDecor(Decor d, double x, double y, double k)
        {
            switch (d.t)
            {
                case "pipe":
                    {
                        double w = 520 * k, h = 600 * k, lw = Math.Max(1, w * 0.04);
                        rect(x - w * 0.4, y - h, w * 0.8, h, C("#22a838")); rectStroke(x - w * 0.4, y - h, w * 0.8, h, C("#0b4a15"), lw);
                        rect(x - w * 0.3, y - h, w * 0.09, h, C("#7ae888"));
                        rect(x - w * 0.5, y - h - w * 0.32, w, w * 0.32, C("#22a838")); rectStroke(x - w * 0.5, y - h - w * 0.32, w, w * 0.32, C("#0b4a15"), lw);
                        rect(x - w * 0.4, y - h - w * 0.32, w * 0.09, w * 0.32, C("#7ae888"));
                        break;
                    }
                case "tree":
                    rect(x - 70 * k, y - 700 * k, 140 * k, 700 * k, C("#7a4a22"));
                    circ(x - 250 * k, y - 820 * k, 300 * k, C("#24823a")); circ(x + 250 * k, y - 840 * k, 300 * k, C("#24823a")); circ(x, y - 1050 * k, 400 * k, C("#2e9a40")); circ(x - 90 * k, y - 1170 * k, 220 * k, C("#45b856"));
                    break;
                case "bush":
                    circ(x - 190 * k, y - 120 * k, 160 * k, C("#2aa83a")); circ(x + 190 * k, y - 120 * k, 160 * k, C("#2aa83a")); circ(x, y - 190 * k, 220 * k, C("#3cc84c"));
                    rect(x - 60 * k, y - 260 * k, 24 * k, 60 * k, K_); rect(x + 36 * k, y - 260 * k, 24 * k, 60 * k, K_);
                    break;
                case "qblock":
                    {
                        double s = 380 * k, by = y - 650 * k - Math.Sin(T * 2 + d.x * 3) * 60 * k;
                        ell(x, y, s * 0.4, s * 0.08, U.RGBA(0, 0, 0, 0.2));
                        rrFill(x - s / 2, by - s, s, s, s * 0.1, C("#f8b800")); rrStroke(x - s / 2, by - s, s, s, s * 0.1, C("#8a4a00"), Math.Max(1, s * 0.06));
                        if (s > 6) txt("?", x, by - s * 0.48, s * 0.7, W_, "center", Color.Black, 0);
                        break;
                    }
                case "mushroom":
                    rect(x - 130 * k, y - 650 * k, 260 * k, 650 * k, C("#ffe8c4")); halfEll(x, y - 600 * k, 480 * k, 420 * k, C("#e52521"));
                    circ(x, y - 850 * k, 100 * k, W_); circ(x - 280 * k, y - 700 * k, 80 * k, W_); circ(x + 280 * k, y - 700 * k, 80 * k, W_);
                    break;
                case "palm":
                    {
                        int sg = d.x > 0 ? 1 : -1;
                        quadLine(C("#9a6a3a"), Math.Max(1, 110 * k), x, y, x + 150 * k * sg, y - 900 * k, x + 350 * k * sg, y - 1700 * k, false);
                        double tx = x + 350 * k * sg, ty = y - 1700 * k;
                        for (int i = 0; i < 6; i++) ell(tx + Math.Cos(i * 1.05) * 300 * k, ty + Math.Sin(i * 1.05) * 130 * k + 60 * k, 330 * k, 90 * k, i % 2 == 1 ? C("#2a9a3a") : C("#3cb84a"), i * 1.05);
                        circ(tx - 40 * k, ty + 60 * k, 60 * k, C("#6a4a1a")); circ(tx + 50 * k, ty + 70 * k, 60 * k, C("#6a4a1a"));
                        break;
                    }
                case "umbrella":
                    rect(x - 20 * k, y - 750 * k, 40 * k, 750 * k, C("#eeeeee"));
                    for (int i = 0; i < 6; i++)
                    {
                        double r = 450 * k; if (r < 0.2) break;
                        g.FillPie(B(i % 2 == 1 ? W_ : C("#ff3a3a")), F(x - r), F(y - 650 * k - r), F(2 * r), F(2 * r), 180 + i * 30, 30);
                    }
                    break;
                case "rock":
                    ell(x, y - 150 * k, 320 * k, 200 * k, theme.bg == "dunes" ? C("#a8703a") : C("#8a7a6a")); ell(x - 80 * k, y - 220 * k, 140 * k, 80 * k, U.RGBA(255, 255, 255, 0.18));
                    break;
                case "cactus":
                    rrFill(x - 90 * k, y - 1100 * k, 180 * k, 1100 * k, 90 * k, C("#2f9a4a")); rrFill(x - 330 * k, y - 750 * k, 100 * k, 380 * k, 50 * k, C("#2f9a4a"));
                    rect(x - 330 * k, y - 420 * k, 260 * k, 90 * k, C("#2f9a4a")); rrFill(x + 230 * k, y - 900 * k, 100 * k, 360 * k, 50 * k, C("#2f9a4a")); rect(x + 70 * k, y - 600 * k, 260 * k, 90 * k, C("#2f9a4a"));
                    break;
                case "pyramid":
                    fillPts(C("#e0b060"), x - 1400 * k, y, x, y - 1500 * k, x + 1400 * k, y);
                    fillPts(C("#b88a40"), x, y - 1500 * k, x + 1400 * k, y, x + 300 * k, y);
                    break;
                case "pine":
                    rect(x - 60 * k, y - 350 * k, 120 * k, 350 * k, C("#6a4a2a"));
                    for (int i = 0; i < 3; i++)
                    {
                        double ty = y - 300 * k - i * 420 * k, w = (650 - i * 170) * k;
                        fillPts(C("#1f6a3a"), x - w, ty, x, ty - 600 * k, x + w, ty);
                        fillPts(W_, x - w * 0.34, ty - 400 * k, x, ty - 600 * k, x + w * 0.34, ty - 400 * k);
                    }
                    break;
                case "snowman":
                    circ(x, y - 260 * k, 260 * k, W_); circ(x, y - 640 * k, 190 * k, W_); circ(x, y - 930 * k, 140 * k, W_);
                    circ(x - 50 * k, y - 960 * k, 20 * k, K_); circ(x + 50 * k, y - 960 * k, 20 * k, K_);
                    fillPts(C("#ff8a1a"), x, y - 930 * k, x + 150 * k, y - 910 * k, x, y - 900 * k);
                    rect(x - 130 * k, y - 1080 * k, 260 * k, 40 * k, C("#222222")); rect(x - 90 * k, y - 1260 * k, 180 * k, 190 * k, C("#222222")); rect(x - 130 * k, y - 800 * k, 260 * k, 50 * k, C("#e52521"));
                    break;
                case "ice":
                    fillPts(U.RGBA(170, 220, 255, 0.85), x - 300 * k, y, x - 200 * k, y - 700 * k, x, y - 1000 * k, x + 150 * k, y - 650 * k, x + 300 * k, y);
                    fillPts(U.RGBA(255, 255, 255, 0.6), x - 150 * k, y - 100 * k, x - 100 * k, y - 650 * k, x, y - 900 * k, x - 30 * k, y - 100 * k);
                    break;
                case "torch":
                    {
                        rect(x - 120 * k, y - 900 * k, 240 * k, 900 * k, C("#5a5a62")); rect(x - 160 * k, y - 980 * k, 320 * k, 100 * k, C("#46464e"));
                        double f = 1 + Math.Sin(T * 15 + d.x * 7) * 0.15;
                        ell(x, y - 1120 * k * f, 130 * k, 200 * k * f, C("#ff7a1a")); ell(x, y - 1080 * k, 80 * k, 120 * k, C("#ffe14a"));
                        break;
                    }
                case "thwomp":
                    {
                        double s = 650 * k, by = y - 200 * k - Math.Max(0, Math.Sin(T * 1.6 + d.x * 5)) * 700 * k;
                        ell(x, y, s * 0.5, s * 0.1, U.RGBA(0, 0, 0, 0.3));
                        rect(x - s / 2, by - s, s, s, C("#8a94a8")); rectStroke(x - s / 2, by - s, s, s, C("#4a5468"), Math.Max(1, s * 0.04));
                        rect(x - s * 0.32, by - s * 0.68, s * 0.22, s * 0.16, W_); rect(x + s * 0.1, by - s * 0.68, s * 0.22, s * 0.16, W_);
                        rect(x - s * 0.24, by - s * 0.65, s * 0.08, s * 0.1, K_); rect(x + s * 0.16, by - s * 0.65, s * 0.08, s * 0.1, K_);
                        fillPts(C("#4a5468"), x - s * 0.36, by - s * 0.82, x - s * 0.08, by - s * 0.7, x - s * 0.08, by - s * 0.76);
                        fillPts(C("#4a5468"), x + s * 0.36, by - s * 0.82, x + s * 0.08, by - s * 0.7, x + s * 0.08, by - s * 0.76);
                        rect(x - s * 0.3, by - s * 0.35, s * 0.6, s * 0.14, W_);
                        for (int i = 1; i < 4; i++) rect(x - s * 0.3 + i * s * 0.15, by - s * 0.35, s * 0.02, s * 0.14, C("#4a5468"));
                        break;
                    }
                case "lavarock":
                    ell(x, y - 200 * k, 420 * k, 260 * k, C("#2a1a1a"));
                    lines(C("#ff6a1a"), Math.Max(1, 25 * k), false, x - 250 * k, y - 200 * k, x - 80 * k, y - 280 * k, x + 60 * k, y - 180 * k, x + 220 * k, y - 260 * k);
                    break;
                case "star":
                    {
                        double by = y - 900 * k - Math.Sin(T * 2 + d.x * 4) * 150 * k, R = 260 * k;
                        var st = Save(); Translate(x, by); Rotate(T * 1.5 + d.x);
                        g.FillPolygon(B(C("#ffd800")), starPts(0, 0, R, R * 0.45)); Restore(st);
                        break;
                    }
                case "ring":
                    ellStroke(x, y - 900 * k, 300 * k, 600 * k, U.Hsl((T * 80 + d.x * 90) % 360, 0.9, 0.6), Math.Max(1, 70 * k));
                    break;
                case "jtree":
                    rect(x - 90 * k, y - 1600 * k, 180 * k, 1600 * k, C("#5a3a1a"));
                    ell(x, y - 1700 * k, 650 * k, 330 * k, C("#1f6a2a")); ell(x - 300 * k, y - 1550 * k, 380 * k, 200 * k, C("#2a8a35")); ell(x + 320 * k, y - 1580 * k, 380 * k, 200 * k, C("#2a8a35"));
                    foreach (int vx in new[] { -380, -120, 200, 420 }) quadLine(C("#2a7a2a"), Math.Max(1, 30 * k), x + vx * k, y - 1500 * k, x + (vx + 60) * k, y - 1100 * k, x + vx * k, y - (700 + Math.Abs(vx)) * k);
                    break;
                case "barrel":
                    {
                        double w = 340 * k, h = 420 * k;
                        rrFill(x - w / 2, y - h, w, h, w * 0.2, C("#9a5a22"));
                        rect(x - w / 2, y - h * 0.8, w, h * 0.07, C("#4a2a10")); rect(x - w / 2, y - h * 0.27, w, h * 0.07, C("#4a2a10"));
                        if (w > 10) txt("DK", x, y - h * 0.53, w * 0.32, C("#ffd84a"), "center", Color.Black, 0);
                        break;
                    }
                case "penguin":
                    {
                        double b = Math.Abs(Math.Sin(T * 4 + d.x * 3)) * 60 * k;
                        ell(x, y - 380 * k - b, 220 * k, 330 * k, C("#1a1a2a")); ell(x, y - 340 * k - b, 150 * k, 250 * k, W_);
                        circ(x, y - 700 * k - b, 170 * k, C("#1a1a2a")); circ(x - 60 * k, y - 720 * k - b, 30 * k, W_); circ(x + 60 * k, y - 720 * k - b, 30 * k, W_);
                        ell(x, y - 650 * k - b, 60 * k, 30 * k, C("#ff9a1a")); ell(x - 90 * k, y - 40 * k, 80 * k, 30 * k, C("#ff9a1a")); ell(x + 90 * k, y - 40 * k, 80 * k, 30 * k, C("#ff9a1a"));
                        break;
                    }
                case "beam":
                    rect(x - 420 * k, y - 1300 * k, 110 * k, 1300 * k, C("#7a5228")); rect(x + 310 * k, y - 1300 * k, 110 * k, 1300 * k, C("#7a5228"));
                    rect(x - 480 * k, y - 1400 * k, 960 * k, 130 * k, C("#8a5e30")); rectStroke(x - 480 * k, y - 1400 * k, 960 * k, 130 * k, C("#4a2e14"), Math.Max(1, 20 * k));
                    break;
                case "crystal":
                    {
                        Color col = new[] { C("#b06aff"), C("#4af0ff"), C("#ff6ad8") }[(int)Math.Floor(d.h * 3) % 3];
                        circ(x, y - 400 * k, 500 * k, U.A(col, 0.35));
                        foreach (var q in new[] { new[] { -150.0, 700, 120 }, new[] { 0.0, 1000, 150 }, new[] { 160.0, 600, 110 } })
                            fillPts(col, x + (q[0] - q[2]) * k, y, x + q[0] * k, y - q[1] * k, x + (q[0] + q[2]) * k, y);
                        fillPts(U.RGBA(255, 255, 255, 0.5), x - 60 * k, y, x, y - 1000 * k, x + 20 * k, y);
                        break;
                    }
                case "lantern":
                    rect(x - 25 * k, y - 900 * k, 50 * k, 900 * k, C("#3a2a1a"));
                    circ(x, y - 950 * k, 280 * k, U.A(C("#ffc84a"), 0.3));
                    rect(x - 90 * k, y - 1050 * k, 180 * k, 30 * k, C("#222222")); rect(x - 70 * k, y - 1020 * k, 140 * k, 150 * k, C("#ffd84a"));
                    break;
                case "minecart":
                    fillPts(C("#5a5a62"), x - 380 * k, y - 500 * k, x + 380 * k, y - 500 * k, x + 300 * k, y - 130 * k, x - 300 * k, y - 130 * k);
                    for (int i = 0; i < 5; i++) circ(x + (i - 2) * 120 * k, y - 520 * k - (i % 2) * 60 * k, 90 * k, C("#ffd23a"));
                    circ(x - 200 * k, y - 110 * k, 100 * k, C("#222222")); circ(x + 200 * k, y - 110 * k, 100 * k, C("#222222"));
                    break;
                case "boo":
                    {
                        double by = y - 800 * k - Math.Sin(T * 2 + d.x * 5) * 200 * k, r = 280 * k;
                        bool shy = Math.Sin(T * 0.8 + d.x * 7) > 0.4;
                        circ(x, by, r, U.RGBA(255, 255, 255, 0.92));
                        fillPts(U.RGBA(255, 255, 255, 0.92), x + r * 0.6, by + r * 0.7, x + r * 1.2, by + r * 1.1, x + r * 0.9, by + r * 0.4);
                        if (shy) { ell(x - r * 0.55, by, r * 0.3, r * 0.22, C("#f0f0f0")); ell(x + r * 0.55, by, r * 0.3, r * 0.22, C("#f0f0f0")); ell(x, by + r * 0.3, r * 0.25, r * 0.08, C("#ff8aa8")); }
                        else { ell(x - r * 0.3, by - r * 0.25, r * 0.1, r * 0.2, K_); ell(x + r * 0.3, by - r * 0.25, r * 0.1, r * 0.2, K_); ell(x, by + r * 0.3, r * 0.45, r * 0.3, C("#3a0a1a")); ell(x, by + r * 0.45, r * 0.22, r * 0.14, C("#ff5a7a")); }
                        break;
                    }
                case "deadtree":
                    lines(C("#1a0f1a"), Math.Max(1, 140 * k), true, x, y, x, y - 1200 * k);
                    lines(C("#1a0f1a"), Math.Max(1, 60 * k), true, x, y - 800 * k, x - 400 * k, y - 1300 * k);
                    lines(C("#1a0f1a"), Math.Max(1, 60 * k), true, x, y - 1000 * k, x + 350 * k, y - 1450 * k);
                    lines(C("#1a0f1a"), Math.Max(1, 60 * k), true, x, y - 1200 * k, x - 150 * k, y - 1600 * k);
                    break;
                case "grave":
                    rrFill(x - 180 * k, y - 480 * k, 360 * k, 480 * k, 170 * k, C("#6a6a7a"));
                    rect(x - 25 * k, y - 400 * k, 50 * k, 220 * k, C("#4a4a5a")); rect(x - 100 * k, y - 340 * k, 200 * k, 50 * k, C("#4a4a5a"));
                    break;
                case "candle":
                    rect(x - 30 * k, y - 900 * k, 60 * k, 900 * k, C("#8a7a3a")); rect(x - 300 * k, y - 900 * k, 600 * k, 50 * k, C("#8a7a3a"));
                    foreach (int ox in new[] { -280, 0, 280 })
                    {
                        rect(x + (ox - 35) * k, y - 1080 * k, 70 * k, 180 * k, C("#f0e8d0"));
                        double fl = 1 + Math.Sin(T * 12 + ox) * 0.2;
                        ell(x + ox * k, y - 1130 * k, 35 * k, 60 * k * fl, C("#ffb02a"));
                    }
                    break;
                case "building":
                    {
                        double bw = 1000 * k, bh = (2600 + d.h * 2600) * k;
                        rect(x - bw / 2, y - bh, bw, bh, d.h > 0.5 ? C("#1e1e3a") : C("#24203a"));
                        if (bw > 20)
                        {
                            Color[] cols = { C("#ffe14a"), C("#3affff"), C("#ff3ad8") };
                            for (double wy = y - bh + bw * 0.08; wy < y - bw * 0.1; wy += bw * 0.14)
                                for (int i = 0; i < 5; i++)
                                {
                                    if ((((int)Math.Floor(wy / 7)) + i + (int)Math.Floor(d.h * 10)) % 3 == 0) continue;
                                    rect(x - bw / 2 + bw * 0.08 + i * bw * 0.18, wy, bw * 0.1, bw * 0.07, cols[(i + (int)Math.Floor(d.h * 9)) % 3]);
                                }
                        }
                        break;
                    }
                case "lamp":
                    {
                        rect(x - 25 * k, y - 1300 * k, 50 * k, 1300 * k, C("#444a5a"));
                        double lx = x - Math.Sign(d.x) * 250 * k;
                        rect(Math.Min(x, lx), y - 1300 * k, 250 * k, 40 * k, C("#444a5a"));
                        circ(lx, y - 1230 * k, 260 * k, U.A(C("#fff4b0"), 0.35));
                        ell(lx, y - 1250 * k, 90 * k, 40 * k, C("#fff4b0"));
                        break;
                    }
                case "neon":
                    {
                        Color col = U.Hsl((T * 90 + d.h * 360) % 360, 1, 0.6);
                        rect(x - 30 * k, y - 800 * k, 60 * k, 800 * k, C("#333333"));
                        rect(x - 450 * k, y - 1300 * k, 900 * k, 500 * k, C("#111111"));
                        rectStroke(x - 420 * k, y - 1270 * k, 840 * k, 440 * k, col, Math.Max(1, 40 * k));
                        if (k > 0.02) txt(d.h > 0.5 ? "KART" : "GO!", x, y - 1050 * k, 260 * k, col, "center", Color.Black, 0);
                        break;
                    }
                case "cloudpuff":
                    {
                        double by = y - 300 * k - Math.Sin(T * 1.5 + d.x) * 80 * k;
                        foreach (var q in new[] { new[] { -300.0, 0, 260 }, new[] { 300.0, 0, 260 }, new[] { 0.0, -120, 340 }, new[] { -120.0, 60, 240 }, new[] { 150.0, 60, 240 } })
                            circ(x + q[0] * k, by + q[1] * k, q[2] * k, W_);
                        ell(x - 90 * k, by - 100 * k, 30 * k, 60 * k, C("#333333")); ell(x + 90 * k, by - 100 * k, 30 * k, 60 * k, C("#333333"));
                        break;
                    }
                case "beanstalk":
                    {
                        var xy = new List<double> { x, y };
                        for (int i = 1; i <= 6; i++) { xy.Add(x + Math.Sin(i * 1.3) * 150 * k); xy.Add(y - i * 350 * k); }
                        lines(C("#2fa84a"), Math.Max(1, 130 * k), true, xy.ToArray());
                        for (int i = 1; i < 6; i++) ell(x + Math.Sin(i * 1.3) * 150 * k + (i % 2 == 1 ? 160 : -160) * k, y - i * 350 * k, 170 * k, 70 * k, C("#4ad86a"), i % 2 == 1 ? -0.4 : 0.4);
                        break;
                    }
                case "sign":
                    {
                        rect(x - 25 * k, y - 500 * k, 50 * k, 500 * k, C("#555555"));
                        rect(x - 260 * k, y - 820 * k, 520 * k, 330 * k, C("#ffd800")); rectStroke(x - 260 * k, y - 820 * k, 520 * k, 330 * k, K_, Math.Max(1, 30 * k));
                        for (int i = 0; i < 2; i++)
                        {
                            double cx = x + (i - 0.5) * 200 * k; int dr = d.dir;
                            fillPts(K_, cx - 60 * k * dr, y - 780 * k, cx + 70 * k * dr, y - 655 * k, cx - 60 * k * dr, y - 530 * k, cx, y - 655 * k);
                        }
                        break;
                    }
            }
        }

        // ===================== karts =====================
        static double[][] drawKartBody(Racer r, double w)
        {
            Ch ch = r.ch; Color col = ch.kart, dk = C("#1d1d1d");
            double[][] ex = null;
            Action<double, double, double> wheel = (wx, ww, wh) =>
            {
                rrFill(wx - ww / 2, -wh, ww, wh, ww * 0.3, dk);
                rect(wx - ww * 0.35, -wh * 0.7, ww * 0.7, wh * 0.1, C("#4a4a4a")); rect(wx - ww * 0.35, -wh * 0.4, ww * 0.7, wh * 0.1, C("#4a4a4a"));
            };
            switch (r.kt.shape)
            {
                case "kart":
                    wheel(-w * 0.4, w * 0.2, w * 0.28); wheel(w * 0.4, w * 0.2, w * 0.28);
                    drawDriverBack(ch, 0, -w * 0.24, w);
                    rrFill(-w * 0.33, -w * 0.3, w * 0.66, w * 0.21, w * 0.06, col); rrFill(-w * 0.33, -w * 0.15, w * 0.66, w * 0.06, w * 0.02, U.Shade(col, -0.35)); rrFill(-w * 0.12, -w * 0.27, w * 0.24, w * 0.09, w * 0.02, C("#666666"));
                    ex = new[] { new[] { -w * 0.08, -w * 0.12 }, new[] { w * 0.08, -w * 0.12 } }; break;
                case "sport":
                    wheel(-w * 0.41, w * 0.22, w * 0.22); wheel(w * 0.41, w * 0.22, w * 0.22);
                    drawDriverBack(ch, 0, -w * 0.2, w);
                    rrFill(-w * 0.38, -w * 0.24, w * 0.76, w * 0.16, w * 0.07, col); rrFill(-w * 0.38, -w * 0.12, w * 0.76, w * 0.04, w * 0.01, C("#222222"));
                    rect(-w * 0.3, -w * 0.42, w * 0.04, w * 0.2, C("#222222")); rect(w * 0.26, -w * 0.42, w * 0.04, w * 0.2, C("#222222"));
                    rrFill(-w * 0.44, -w * 0.47, w * 0.88, w * 0.07, w * 0.02, U.Shade(col, -0.2));
                    ex = new[] { new[] { -w * 0.18, -w * 0.1 }, new[] { w * 0.18, -w * 0.1 } }; break;
                case "mini":
                    wheel(-w * 0.31, w * 0.16, w * 0.2); wheel(w * 0.31, w * 0.16, w * 0.2);
                    drawDriverBack(ch, 0, -w * 0.22, w * 0.92);
                    rrFill(-w * 0.26, -w * 0.28, w * 0.52, w * 0.2, w * 0.09, col);
                    circ(-w * 0.15, -w * 0.2, w * 0.035, C("#ff4040")); circ(w * 0.15, -w * 0.2, w * 0.035, C("#ff4040"));
                    ex = new[] { new[] { 0, -w * 0.12 } }; break;
                case "buggy":
                    wheel(-w * 0.43, w * 0.25, w * 0.36); wheel(w * 0.43, w * 0.25, w * 0.36);
                    drawDriverBack(ch, 0, -w * 0.28, w);
                    lines(C("#333333"), w * 0.035, false, -w * 0.28, -w * 0.28, -w * 0.24, -w * 0.68, w * 0.24, -w * 0.68, w * 0.28, -w * 0.28);
                    rrFill(-w * 0.3, -w * 0.32, w * 0.6, w * 0.17, w * 0.04, col);
                    ex = new[] { new[] { -w * 0.1, -w * 0.17 }, new[] { w * 0.1, -w * 0.17 } }; break;
                case "bike":
                    wheel(0, w * 0.15, w * 0.3);
                    drawDriverBack(ch, 0, -w * 0.36, w);
                    rrFill(-w * 0.18, -w * 0.42, w * 0.36, w * 0.16, w * 0.07, col); circ(0, -w * 0.32, w * 0.04, C("#ff3030"));
                    ex = new[] { new[] { -w * 0.13, -w * 0.28 }, new[] { w * 0.13, -w * 0.28 } }; break;
                case "heavy":
                    wheel(-w * 0.44, w * 0.25, w * 0.3); wheel(w * 0.44, w * 0.25, w * 0.3);
                    drawDriverBack(ch, 0, -w * 0.3, w);
                    rrFill(-w * 0.42, -w * 0.38, w * 0.84, w * 0.28, w * 0.05, col);
                    for (int i = 0; i < 4; i++) rect(-w * 0.36 + i * w * 0.2, -w * 0.34, w * 0.1, w * 0.05, U.Shade(col, -0.4));
                    for (int i = 0; i < 5; i++) { double sx = -w * 0.34 + i * w * 0.17; fillPts(C("#dddddd"), sx - w * 0.04, -w * 0.38, sx, -w * 0.45, sx + w * 0.04, -w * 0.38); }
                    ex = new[] { new[] { -w * 0.26, -w * 0.17 }, new[] { w * 0.26, -w * 0.17 } }; break;
                case "pipe":
                    wheel(-w * 0.38, w * 0.18, w * 0.25); wheel(w * 0.38, w * 0.18, w * 0.25);
                    drawDriverBack(ch, 0, -w * 0.22, w);
                    rectStroke(-w * 0.3, -w * 0.3, w * 0.6, w * 0.15, C("#1f9a30"), w * 0.06);
                    rect(-w * 0.1, -w * 0.27, w * 0.2, w * 0.09, col);
                    ex = new[] { new[] { 0, -w * 0.13 } }; break;
                default: // rocket
                    wheel(-w * 0.36, w * 0.17, w * 0.22); wheel(w * 0.36, w * 0.17, w * 0.22);
                    drawDriverBack(ch, 0, -w * 0.26, w);
                    foreach (int s in new[] { -1, 1 }) fillPts(U.Shade(col, -0.3), s * w * 0.25, -w * 0.3, s * w * 0.42, -w * 0.52, s * w * 0.32, -w * 0.14);
                    rrFill(-w * 0.3, -w * 0.34, w * 0.6, w * 0.24, w * 0.12, col);
                    circ(0, -w * 0.22, w * 0.1, C("#555555")); circ(0, -w * 0.22, w * 0.065, C("#222222"));
                    ex = new[] { new[] { 0, -w * 0.22 } }; break;
            }
            return ex;
        }
        static void drawBulletBack(double w)
        {
            circ(0, -w * 0.42, w * 0.42, C("#1a1a1a")); circ(0, -w * 0.42, w * 0.32, C("#3a3a3a"));
            rect(-w * 0.6, -w * 0.46, w * 0.2, w * 0.08, K_); rect(w * 0.4, -w * 0.46, w * 0.2, w * 0.08, K_); rect(-w * 0.04, -w * 0.95, w * 0.08, w * 0.2, K_);
            circ(0, -w * 0.42, w * (0.18 + U.R() * 0.04), C("#ff8a1a")); circ(0, -w * 0.42, w * 0.1, C("#ffe14a"));
        }
        static void drawKart(Racer r, double x, double y, double w)
        {
            var st = Save(); double oldA = ALPHA;
            Translate(x, y);
            ell(0, 0, w * 0.5, w * 0.08, U.RGBA(0, 0, 0, 0.3));
            if (r.star > 0) { circ(0, -w * 0.35, w * 0.75, U.A(U.Hsl(T * 720, 1, 0.6), 0.35)); circ(0, -w * 0.35, w * 0.5, U.A(U.Hsl(T * 720 + 120, 1, 0.6), 0.45)); }
            if (r.hornT > 0) { double pr = 1 - r.hornT / 0.5; ellStroke(0, -w * 0.3, w * (0.6 + pr * 1.6), w * (0.3 + pr * 0.8), U.RGBA(255, 220, 80, 1 - pr), Math.Max(2, w * 0.06)); }
            if (r.rescue > 0) Translate(0, -w * 1.1 * Math.Sin(U.Clamp((1.6 - r.rescue) / 1.6, 0, 1) * Math.PI));
            // ---- efectos de súper poder ----
            bool pwOn = r.pwT > 0; string pwt = r.pwType;
            if (pwOn && pwt == "shield") { circ(0, -w * 0.35, w * 0.75, U.A(r.ch.kart, 0.3)); ellStroke(0, -w * 0.35, w * 0.75, w * 0.75, U.RGBA(255, 255, 255, 0.85), Math.Max(2, w * 0.04)); }
            if (pwOn && pwt == "giant") circ(0, -w * 0.35, w * 0.7, U.RGBA(255, 255, 255, 0.25 + Math.Sin(T * 12) * 0.1));
            if (r.teleT > 0) { double pr = 1 - r.teleT / 0.6; ellStroke(0, -w * 0.3, w * (0.4 + pr * 1.4), w * (0.6 + pr * 1.8), U.RGBA(200, 106, 255, 1 - pr), Math.Max(2, w * 0.08)); }
            if (r.laserT > 0)
            {
                double la = Math.Min(1, r.laserT * 3);
                if (PowerOf(r.ch)[0] == "kame")
                {
                    // Kamehameha: rayo azul gordo con bola de energía
                    lines(U.RGBA(60, 160, 255, la * 0.45), w * 0.42, true, 0, -w * 0.62, 0, -w * 9);
                    lines(U.RGBA(170, 230, 255, la * 0.8), w * 0.2, true, 0, -w * 0.62, 0, -w * 9);
                    lines(U.RGBA(255, 255, 255, la), w * 0.08, true, 0, -w * 0.62, 0, -w * 9);
                    circ(0, -w * 0.7, w * 0.26, U.RGBA(120, 200, 255, la * 0.8)); circ(0, -w * 0.7, w * 0.14, U.RGBA(255, 255, 255, la));
                }
                else
                {
                    lines(U.RGBA(255, 40, 40, la * 0.5), w * 0.22, false, 0, -w * 0.62, 0, -w * 9);
                    lines(U.RGBA(255, 240, 240, la), w * 0.07, false, 0, -w * 0.62, 0, -w * 9);
                }
            }
            if (pwOn && pwt == "ssj")
            {
                // aura dorada de Súper Saiyajin
                double f = Math.Sin(T * 30) * 0.08;
                circ(0, -w * 0.45, w * (0.72 + f), U.RGBA(255, 220, 40, 0.22));
                for (int i = 0; i < 7; i++) { double a = -Math.PI / 2 + (i - 3) * 0.32, len = w * (0.95 + 0.25 * Math.Sin(T * 25 + i * 1.7)); fillPts(U.RGBA(255, 236, 90, 0.55), Math.Cos(a - 0.12) * w * 0.4, -w * 0.45 + Math.Sin(a - 0.12) * w * 0.4, Math.Cos(a) * len, -w * 0.45 + Math.Sin(a) * len, Math.Cos(a + 0.12) * w * 0.4, -w * 0.45 + Math.Sin(a + 0.12) * w * 0.4); }
            }
            double flyY = 0;
            if (flying(r)) { double dur = PW_DUR[pwt]; flyY = w * 0.85 * Math.Min(1, Math.Min((dur - r.pwT) * 3, r.pwT * 3)); }
            double hopY = r.hop > 0 ? Math.Sin((1 - r.hop / 0.28) * Math.PI) * w * 0.16 : 0;
            double airY = r.air > 0 ? Math.Sin(U.Clamp(1 - r.air / r.airMax, 0, 1) * Math.PI) * w * 0.7 : 0;
            Translate(0, -hopY - airY - flyY);
            if (flying(r))
            {
                double fl = Math.Sin(T * 22) * 0.5;
                foreach (int s in new[] { -1, 1 }) { ell(s * w * 0.5, -w * 0.45, w * 0.34, w * 0.13, W_, s * (0.4 + fl)); ell(s * w * 0.62, -w * 0.5, w * 0.2, w * 0.08, C("#e8f4ff"), s * (0.6 + fl)); }
            }
            if (pwOn && pwt == "acro") Rotate(T * 9);
            if (pwOn && pwt == "spin") Scale(Math.Cos(T * 20), 1);
            Rotate(r.drift != 0 ? r.drift * 0.12 : r.steerVis * 0.05);
            if (r.trickT > 0) Rotate((1 - r.trickT / 0.4) * Math.PI * 2);
            if (r.spin > 0) Scale(Math.Cos(r.spin * 14), 1);
            if (r.invul > 0 && r.spin <= 0 && ((int)Math.Floor(T * 20)) % 2 == 1) ALPHA = 0.45;
            if (r.ghostT > 0) ALPHA = r.isPlayer ? 0.4 : 0.15;
            if (r.bullet > 0) { drawBulletBack(w); ALPHA = oldA; Restore(st); return; }
            var och = r.ch; if (pwOn && pwt == "ssj") r.ch = ssjOf(och);
            var ex = drawKartBody(r, w);
            r.ch = och;
            foreach (var e in ex)
            {
                if (r.boost > 0) { double fr = w * (0.06 + U.R() * 0.05); circ(e[0], e[1], fr * 1.5, U.RGBA(255, 120, 20, 0.7)); circ(e[0], e[1], fr, C("#ffe14a")); }
                else circ(e[0], e[1], w * 0.03, C("#333333"));
            }
            if (r.drift != 0 && r.driftLvl > 0)
            {
                Color col = new[] { Color.White, C("#4ab8ff"), C("#ff9a1a"), C("#d05aff") }[r.driftLvl];
                foreach (int s in new[] { -1, 1 }) for (int i = 0; i < 4; i++) { double a = U.R() * Math.PI; circ(s * w * 0.4 + Math.Cos(a) * w * 0.09, -U.R() * w * 0.08, w * (0.015 + U.R() * 0.02), col); }
            }
            if (r.frozenT > 0) { rrFill(-w * 0.58, -w * 0.95, w * 1.16, w * 0.98, w * 0.12, U.A(C("#9af0ff"), 0.5)); rrStroke(-w * 0.58, -w * 0.95, w * 1.16, w * 0.98, w * 0.12, W_, Math.Max(1, w * 0.02)); }
            if (r.rescue > 0)
            {
                lines(C("#222222"), Math.Max(1, w * 0.015), false, 0, -w * 0.55, 0, -w * 1.05);
                circ(-w * 0.25, -w * 1.15, w * 0.18, W_); circ(w * 0.25, -w * 1.15, w * 0.18, W_); circ(0, -w * 1.12, w * 0.24, W_); circ(0, -w * 1.32, w * 0.12, C("#f5c84a"));
            }
            ALPHA = oldA; Restore(st);
        }

        // ---- Lakitu con semáforo ----
        static void drawLakitu(double x, double y, double s, int stage)
        {
            lines(C("#5a3a1a"), Math.Max(2, s * 0.06), false, x + s * 0.3, y - s * 0.2, x + s * 1.1, y - s * 0.9);
            lines(C("#222222"), 1.5, false, x + s * 1.1, y - s * 0.9, x + s * 1.1, y + s * 0.2);
            rrFill(x + s * 0.85, y + s * 0.2, s * 0.5, s * 1.35, s * 0.12, C("#222222"));
            for (int i = 0; i < 3; i++) circ(x + s * 1.1, y + s * 0.45 + i * s * 0.42, s * 0.16, stage == 4 ? C("#3aff6a") : (i < stage ? C("#ff2a2a") : C("#4a1a1a")));
            halfEll(x, y - s * 0.05, s * 0.45, s * 0.42, C("#2fa84a"));
            circ(x, y - s * 0.35, s * 0.32, C("#f5c84a"));
            ell(x - s * 0.12, y - s * 0.42, s * 0.11, s * 0.13, W_); ell(x + s * 0.12, y - s * 0.42, s * 0.11, s * 0.13, W_);
            circ(x - s * 0.1, y - s * 0.4, s * 0.05, K_); circ(x + s * 0.14, y - s * 0.4, s * 0.05, K_);
            ellStroke(x, y - s * 0.42, s * 0.3, s * 0.15, C("#222222"), Math.Max(1, s * 0.04));
            ell(x, y - s * 0.22, s * 0.15, s * 0.08, C("#e8a030"));
            foreach (var q in new[] { new[] { -0.45, 0.05, 0.32 }, new[] { 0.45, 0.05, 0.32 }, new[] { 0, 0.1, 0.42 }, new[] { -0.2, -0.05, 0.3 }, new[] { 0.2, -0.05, 0.3 } })
                circ(x + q[0] * s, y + q[1] * s, q[2] * s, W_);
            ell(x - s * 0.15, y + s * 0.08, s * 0.04, s * 0.07, C("#333333")); ell(x + s * 0.15, y + s * 0.08, s * 0.04, s * 0.07, C("#333333"));
        }
        static void drawTrophy(double x, double y, double s, Color col)
        {
            using (var p = new GraphicsPath())
            {
                p.AddLine(F(x - s * 0.4), F(y - s), F(x + s * 0.4), F(y - s));
                quadTo(p, x + s * 0.4, y - s, x + s * 0.4, y - s * 0.35, x, y - s * 0.3);
                quadTo(p, x, y - s * 0.3, x - s * 0.4, y - s * 0.35, x - s * 0.4, y - s);
                g.FillPath(B(col), p);
            }
            arc(col, s * 0.08, x - s * 0.42, y - s * 0.78, s * 0.15, Math.PI * 0.5, Math.PI * 1.5);
            arc(col, s * 0.08, x + s * 0.42, y - s * 0.78, s * 0.15, -Math.PI * 0.5, Math.PI * 0.5);
            rect(x - s * 0.06, y - s * 0.32, s * 0.12, s * 0.2, col); rect(x - s * 0.25, y - s * 0.14, s * 0.5, s * 0.14, col);
        }
    }
}
