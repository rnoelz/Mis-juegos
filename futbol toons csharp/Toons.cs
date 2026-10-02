using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace FutbolToons
{
    // Dibujos nuevos: caras de las caricaturas, cuerpos con uniforme, escudos y trofeos
    static partial class G
    {
        static readonly Color DARK = U.C("#1a1a1a");
        static void ln(Color c, double w, params double[] xy) { lines(c, Math.Max(0.5, w), true, xy); }
        static void dotEyes(double x, double y, double r, double sp, double er)
        {
            foreach (int s in new[] { -1, 1 }) { circ(x + s * r * sp, y, r * er, W_); ellStroke(x + s * r * sp, y, r * er, r * er, DARK, Math.Max(0.6, r * 0.04)); circ(x + s * r * sp + r * 0.02, y + r * 0.01, r * er * 0.32, DARK); }
        }
        static void smile(double x, double y, double r, double w = 0.22) { arc(U.C("#5a2a1a"), Math.Max(1, r * 0.05), x, y, r * w, 0.15 * Math.PI, 0.85 * Math.PI, true); }

        // ===================== cara de cualquier personaje =====================
        public static void drawFace(Ch ch, double x, double y, double r)
        {
            var st = Save();
            switch (ch.kind)
            {
                case "bob":
                    {
                        Color c = ch.skin;
                        rrFill(x - r * 0.92, y - r * 1.0, r * 1.84, r * 1.95, r * 0.22, c);
                        foreach (var h in new[] { new[] { -0.6, -0.7, 0.12 }, new[] { 0.65, -0.55, 0.09 }, new[] { -0.7, 0.55, 0.1 }, new[] { 0.6, 0.65, 0.13 }, new[] { 0.05, -0.85, 0.07 } })
                            ell(x + h[0] * r, y + h[1] * r, h[2] * r, h[2] * r * 0.8, U.C("#c8b42a"));
                        foreach (int s in new[] { -1, 1 })
                        {
                            circ(x + s * r * 0.36, y - r * 0.22, r * 0.34, W_); ellStroke(x + s * r * 0.36, y - r * 0.22, r * 0.34, r * 0.34, DARK, Math.Max(1, r * 0.04));
                            circ(x + s * r * 0.33, y - r * 0.2, r * 0.15, U.C("#3a8ad8")); circ(x + s * r * 0.33, y - r * 0.2, r * 0.07, DARK);
                            for (int i = -1; i <= 1; i++) ln(DARK, r * 0.05, x + s * r * 0.36 + i * r * 0.12, y - r * 0.56, x + s * r * 0.36 + i * r * 0.16, y - r * 0.7);
                            circ(x + s * r * 0.62, y + r * 0.25, r * 0.13, U.C("#ff8a6a"));
                        }
                        ell(x, y + r * 0.02, r * 0.1, r * 0.16, U.Shade(c, -0.1));
                        arc(DARK, Math.Max(1, r * 0.06), x, y + r * 0.05, r * 0.55, 0.15 * Math.PI, 0.85 * Math.PI, true);
                        rect(x - r * 0.17, y + r * 0.55, r * 0.15, r * 0.18, W_); rect(x + r * 0.02, y + r * 0.55, r * 0.15, r * 0.18, W_);
                        break;
                    }
                case "patricio":
                    {
                        Color c = ch.skin;
                        fillPts(c, x, y - r * 1.45, x + r * 0.9, y + r * 0.8, x - r * 0.9, y + r * 0.8);
                        ell(x, y + r * 0.45, r * 0.9, r * 0.55, c);
                        foreach (int s in new[] { -1, 1 })
                        {
                            ell(x + s * r * 0.16, y - r * 0.2, r * 0.15, r * 0.24, W_); circ(x + s * r * 0.13, y - r * 0.15, r * 0.07, DARK);
                            ln(DARK, r * 0.07, x + s * r * 0.05, y - r * 0.5, x + s * r * 0.3, y - r * 0.56);
                        }
                        halfEll(x, y + r * 0.3, r * 0.42, r * 0.35, U.C("#b02a3a"));
                        fillPts(U.C("#b02a3a"), x - r * 0.42, y + r * 0.3, x + r * 0.42, y + r * 0.3, x, y + r * 0.62);
                        break;
                    }
                case "calamardo":
                    {
                        Color c = ch.skin;
                        ell(x, y - r * 0.15, r * 0.82, r * 1.05, c);
                        foreach (int s in new[] { -1, 1 })
                        {
                            ell(x + s * r * 0.3, y - r * 0.05, r * 0.24, r * 0.2, U.C("#fff8d0"));
                            circ(x + s * r * 0.3, y + r * 0.02, r * 0.07, U.C("#c83a2a"));
                            halfEll(x + s * r * 0.3, y - r * 0.02, r * 0.26, r * 0.2, U.Shade(c, -0.12));
                        }
                        ell(x, y + r * 0.45, r * 0.18, r * 0.5, U.Shade(c, -0.05)); ell(x + r * 0.04, y + r * 0.3, r * 0.06, r * 0.25, U.Shade(c, 0.2));
                        ln(U.Shade(c, -0.4), r * 0.05, x - r * 0.3, y + r * 0.62, x - r * 0.08, y + r * 0.58);
                        break;
                    }
                case "cangrejo":
                    {
                        Color c = ch.skin;
                        foreach (int s in new[] { -1, 1 }) { ln(c, r * 0.14, x + s * r * 0.25, y - r * 0.2, x + s * r * 0.3, y - r * 1.05); circ(x + s * r * 0.3, y - r * 1.15, r * 0.24, W_); circ(x + s * r * 0.3, y - r * 1.12, r * 0.1, DARK); }
                        ell(x, y + r * 0.2, r * 0.95, r * 0.75, c);
                        ell(x, y + r * 0.12, r * 0.12, r * 0.08, U.Shade(c, -0.25));
                        halfEll(x, y + r * 0.42, r * 0.38, r * 0.05, U.Shade(c, -0.4));
                        arc(U.Shade(c, -0.45), Math.Max(1, r * 0.06), x, y + r * 0.3, r * 0.38, 0.15 * Math.PI, 0.85 * Math.PI, true);
                        rect(x - r * 0.3, y + r * 0.52, r * 0.6, r * 0.1, W_);
                        break;
                    }
                case "arenita":
                    {
                        Color c = ch.skin;
                        foreach (int s in new[] { -1, 1 }) ell(x + s * r * 0.55, y - r * 0.55, r * 0.18, r * 0.24, c);
                        ell(x, y + r * 0.05, r * 0.72, r * 0.75, c);
                        ell(x, y + r * 0.3, r * 0.48, r * 0.38, U.C("#fff0dc"));
                        foreach (int s in new[] { -1, 1 }) { ell(x + s * r * 0.25, y - r * 0.12, r * 0.13, r * 0.2, W_); circ(x + s * r * 0.24, y - r * 0.08, r * 0.08, DARK); }
                        ell(x, y + r * 0.12, r * 0.08, r * 0.06, DARK); smile(x, y + r * 0.22, r, 0.18);
                        rect(x - r * 0.08, y + r * 0.38, r * 0.16, r * 0.12, W_);
                        // casco de buzo con flor
                        circ(x, y, r * 1.05, U.RGBA(200, 240, 255, 0.28)); ellStroke(x, y, r * 1.05, r * 1.05, U.RGBA(255, 255, 255, 0.9), Math.Max(1, r * 0.06));
                        ell(x - r * 0.45, y - r * 0.55, r * 0.15, r * 0.3, U.RGBA(255, 255, 255, 0.6), -0.6);
                        for (int i = 0; i < 5; i++) { double a = i * Math.PI * 2 / 5; circ(x + r * 0.55 + Math.Cos(a) * r * 0.16, y - r * 1.0 + Math.Sin(a) * r * 0.16, r * 0.12, U.C("#c86ad8")); }
                        circ(x + r * 0.55, y - r * 1.0, r * 0.09, U.C("#ffd23a"));
                        break;
                    }
                case "plankton":
                    {
                        Color c = ch.skin;
                        foreach (int s in new[] { -1, 1 }) ln(c, r * 0.06, x + s * r * 0.15, y - r * 0.85, x + s * r * 0.3, y - r * 1.35);
                        ell(x, y + r * 0.05, r * 0.6, r * 0.95, c);
                        circ(x, y - r * 0.3, r * 0.38, W_); circ(x, y - r * 0.3, r * 0.2, U.C("#e8c020")); circ(x, y - r * 0.3, r * 0.1, U.C("#c8102e"));
                        ln(DARK, r * 0.1, x - r * 0.42, y - r * 0.82, x + r * 0.42, y - r * 0.62);
                        ln(DARK, r * 0.05, x - r * 0.2, y + r * 0.45, x + r * 0.2, y + r * 0.4);
                        break;
                    }
                case "pikachu":
                    {
                        Color c = ch.skin;
                        foreach (int s in new[] { -1, 1 })
                        {
                            fillPts(c, x + s * r * 0.35, y - r * 0.5, x + s * r * 1.05, y - r * 1.45, x + s * r * 0.7, y - r * 0.35);
                            fillPts(DARK, x + s * r * 0.86, y - r * 1.13, x + s * r * 1.05, y - r * 1.45, x + s * r * 0.95, y - r * 1.07);
                        }
                        ell(x, y, r * 0.95, r * 0.82, c);
                        foreach (int s in new[] { -1, 1 }) { circ(x + s * r * 0.4, y - r * 0.12, r * 0.15, DARK); circ(x + s * r * 0.37, y - r * 0.17, r * 0.06, W_); circ(x + s * r * 0.68, y + r * 0.25, r * 0.17, U.C("#e8302a")); }
                        circ(x, y + r * 0.07, r * 0.04, DARK);
                        arc(DARK, Math.Max(1, r * 0.05), x - r * 0.08, y + r * 0.18, r * 0.09, 0.1 * Math.PI, 0.9 * Math.PI, true); arc(DARK, Math.Max(1, r * 0.05), x + r * 0.08, y + r * 0.18, r * 0.09, 0.1 * Math.PI, 0.9 * Math.PI, true);
                        break;
                    }
                case "charmander":
                case "squirtle":
                    {
                        Color c = ch.skin; bool sq = ch.kind == "squirtle";
                        ell(x, y - r * 0.05, r * 0.88, r * 0.9, c);
                        ell(x, y + r * 0.4, r * 0.55, r * 0.35, U.Shade(c, 0.25));
                        foreach (int s in new[] { -1, 1 }) { ell(x + s * r * 0.38, y - r * 0.18, r * 0.2, r * 0.28, W_); circ(x + s * r * 0.36, y - r * 0.12, r * 0.14, sq ? U.C("#8a3a2a") : U.C("#2a5aa8")); circ(x + s * r * 0.36, y - r * 0.12, r * 0.07, DARK); circ(x + s * r * 0.33, y - r * 0.2, r * 0.05, W_); }
                        arc(DARK, Math.Max(1, r * 0.06), x, y + r * 0.2, r * 0.36, 0.15 * Math.PI, 0.85 * Math.PI, true);
                        if (!sq) ell(x, y + r * 0.42, r * 0.18, r * 0.1, U.C("#d83a3a"));
                        break;
                    }
                case "bulbasaur":
                    {
                        Color c = ch.skin;
                        circ(x, y - r * 0.85, r * 0.6, U.C("#3a9a4a")); ln(U.C("#2a7a3a"), r * 0.06, x, y - r * 1.4, x, y - r * 0.6);
                        foreach (int s in new[] { -1, 1 }) fillPts(c, x + s * r * 0.45, y - r * 0.5, x + s * r * 0.9, y - r * 0.85, x + s * r * 0.85, y - r * 0.25);
                        ell(x, y, r * 1.0, r * 0.75, c);
                        foreach (var d in new[] { new[] { -0.5, -0.45 }, new[] { 0.1, -0.55 }, new[] { 0.6, -0.3 } }) ell(x + d[0] * r, y + d[1] * r, r * 0.14, r * 0.1, U.Shade(c, -0.25));
                        foreach (int s in new[] { -1, 1 }) { ell(x + s * r * 0.45, y - r * 0.05, r * 0.2, r * 0.18, W_); circ(x + s * r * 0.48, y - r * 0.03, r * 0.11, U.C("#c8302a")); circ(x + s * r * 0.48, y - r * 0.03, r * 0.05, DARK); }
                        arc(DARK, Math.Max(1, r * 0.05), x, y + r * 0.15, r * 0.4, 0.15 * Math.PI, 0.85 * Math.PI, true);
                        break;
                    }
                case "jigglypuff":
                    {
                        Color c = ch.skin;
                        foreach (int s in new[] { -1, 1 }) { fillPts(c, x + s * r * 0.45, y - r * 0.6, x + s * r * 0.95, y - r * 1.05, x + s * r * 0.85, y - r * 0.35); fillPts(DARK, x + s * r * 0.62, y - r * 0.68, x + s * r * 0.88, y - r * 0.92, x + s * r * 0.82, y - r * 0.55); }
                        circ(x, y, r * 0.98, c);
                        quadLine(U.Shade(c, -0.1), r * 0.22, x - r * 0.05, y - r * 0.85, x + r * 0.45, y - r * 0.75, x + r * 0.1, y - r * 0.45);
                        foreach (int s in new[] { -1, 1 }) { circ(x + s * r * 0.36, y - r * 0.05, r * 0.27, W_); circ(x + s * r * 0.36, y - r * 0.02, r * 0.2, U.C("#2a9a9a")); circ(x + s * r * 0.36, y - r * 0.02, r * 0.1, DARK); circ(x + s * r * 0.3, y - r * 0.12, r * 0.07, W_); }
                        smile(x, y + r * 0.3, r, 0.12);
                        break;
                    }
                case "homero":
                case "bart":
                case "lisa":
                case "marge":
                case "milhouse":
                    drawSimpson(ch, x, y, r); break;
                case "scooby":
                    {
                        Color c = ch.skin, d = U.Shade(c, -0.3);
                        foreach (int s in new[] { -1, 1 }) ell(x + s * r * 0.72, y - r * 0.1, r * 0.2, r * 0.5, d, s * 0.3);
                        ell(x, y - r * 0.1, r * 0.72, r * 0.78, c);
                        ell(x + r * 0.15, y + r * 0.4, r * 0.62, r * 0.38, c);
                        foreach (var p in new[] { new[] { -0.4, -0.55 }, new[] { 0.35, -0.65 }, new[] { -0.2, 0.15 } }) circ(x + p[0] * r, y + p[1] * r, r * 0.09, DARK);
                        foreach (int s in new[] { -1, 1 }) { ell(x + s * r * 0.25, y - r * 0.2, r * 0.17, r * 0.2, W_); circ(x + s * r * 0.25, y - r * 0.17, r * 0.08, DARK); }
                        ell(x + r * 0.62, y + r * 0.25, r * 0.17, r * 0.13, DARK);
                        arc(DARK, Math.Max(1, r * 0.05), x + r * 0.2, y + r * 0.42, r * 0.32, 0.1 * Math.PI, 0.8 * Math.PI, true);
                        break;
                    }
                case "shaggy":
                    {
                        Color c = ch.skin, h = ch.hair;
                        ell(x, y - r * 0.35, r * 0.82, r * 0.7, h);
                        ell(x, y + r * 0.15, r * 0.66, r * 0.9, c);
                        fillPts(h, x - r * 0.7, y - r * 0.25, x - r * 0.4, y - r * 0.75, x + r * 0.5, y - r * 0.78, x + r * 0.72, y - r * 0.25, x + r * 0.35, y - r * 0.45, x, y - r * 0.32, x - r * 0.35, y - r * 0.45);
                        dotEyes(x, y - r * 0.05, r, 0.24, 0.14);
                        ell(x, y + r * 0.2, r * 0.1, r * 0.08, U.Shade(c, -0.18));
                        smile(x, y + r * 0.38, r, 0.18);
                        for (int i = -2; i <= 2; i++) ln(h, r * 0.06, x + i * r * 0.08, y + r * 0.82, x + i * r * 0.1, y + r * 0.98);
                        break;
                    }
                case "finn":
                    {
                        ell(x, y, r * 0.9, r * 0.92, W_);
                        foreach (int s in new[] { -1, 1 }) circ(x + s * r * 0.55, y - r * 0.85, r * 0.2, W_);
                        ell(x, y + r * 0.12, r * 0.62, r * 0.55, ch.skin);
                        foreach (int s in new[] { -1, 1 }) circ(x + s * r * 0.22, y + r * 0.02, r * 0.06, DARK);
                        halfEll(x, y + r * 0.25, r * 0.22, r * 0.1, W_); ell(x, y + r * 0.3, r * 0.22, r * 0.12, U.C("#5a2a1a")); ell(x, y + r * 0.26, r * 0.2, r * 0.05, W_);
                        break;
                    }
                case "jake":
                    {
                        Color c = ch.skin;
                        foreach (int s in new[] { -1, 1 }) ell(x + s * r * 0.82, y - r * 0.05, r * 0.2, r * 0.35, U.Shade(c, -0.1));
                        ell(x, y, r * 0.92, r * 0.85, c);
                        foreach (int s in new[] { -1, 1 }) { circ(x + s * r * 0.3, y - r * 0.2, r * 0.24, W_); ellStroke(x + s * r * 0.3, y - r * 0.2, r * 0.24, r * 0.24, DARK, Math.Max(1, r * 0.06)); circ(x + s * r * 0.3, y - r * 0.18, r * 0.07, DARK); }
                        ell(x, y + r * 0.15, r * 0.4, r * 0.28, U.Shade(c, 0.15));
                        ell(x, y + r * 0.05, r * 0.12, r * 0.08, DARK);
                        arc(DARK, Math.Max(1, r * 0.05), x - r * 0.13, y + r * 0.22, r * 0.13, 0.1 * Math.PI, 0.9 * Math.PI, true); arc(DARK, Math.Max(1, r * 0.05), x + r * 0.13, y + r * 0.22, r * 0.13, 0.1 * Math.PI, 0.9 * Math.PI, true);
                        break;
                    }
                case "tom":
                case "jerry":
                    {
                        Color c = ch.skin; bool tom = ch.kind == "tom";
                        if (tom) foreach (int s in new[] { -1, 1 }) { fillPts(c, x + s * r * 0.3, y - r * 0.55, x + s * r * 0.85, y - r * 1.25, x + s * r * 0.82, y - r * 0.25); fillPts(U.C("#ffb0b8"), x + s * r * 0.42, y - r * 0.55, x + s * r * 0.8, y - r * 1.05, x + s * r * 0.75, y - r * 0.38); }
                        else foreach (int s in new[] { -1, 1 }) { circ(x + s * r * 0.7, y - r * 0.6, r * 0.45, c); circ(x + s * r * 0.7, y - r * 0.6, r * 0.3, U.C("#ffb0b8")); }
                        ell(x, y, r * 0.85, r * 0.8, c);
                        ell(x, y + r * 0.32, r * 0.55, r * 0.38, tom ? W_ : U.C("#f0d8b0"));
                        foreach (int s in new[] { -1, 1 })
                        {
                            ell(x + s * r * 0.25, y - r * 0.15, r * 0.17, r * 0.24, tom ? U.C("#f0e060") : W_); circ(x + s * r * 0.23, y - r * 0.12, r * 0.08, DARK);
                            if (tom) for (int i = 0; i < 3; i++) ln(DARK, r * 0.025, x + s * r * 0.3, y + r * 0.3 + i * r * 0.08, x + s * r * 0.95, y + r * 0.22 + i * r * 0.12);
                        }
                        ell(x, y + r * 0.15, r * 0.12, r * 0.09, tom ? U.C("#2a2a3a") : DARK);
                        arc(DARK, Math.Max(1, r * 0.05), x, y + r * 0.25, r * 0.25, 0.15 * Math.PI, 0.85 * Math.PI, true);
                        break;
                    }
                case "bugs":
                    {
                        Color c = ch.skin;
                        foreach (int s in new[] { -1, 1 }) { ell(x + s * r * 0.32, y - r * 1.25, r * 0.22, r * 0.75, c, s * 0.15); ell(x + s * r * 0.32, y - r * 1.2, r * 0.11, r * 0.55, U.C("#ffb0c0"), s * 0.15); }
                        ell(x, y, r * 0.8, r * 0.82, c);
                        ell(x, y + r * 0.35, r * 0.55, r * 0.4, W_);
                        foreach (int s in new[] { -1, 1 }) { ell(x + s * r * 0.22, y - r * 0.22, r * 0.17, r * 0.27, W_); circ(x + s * r * 0.2, y - r * 0.17, r * 0.08, DARK); for (int i = 0; i < 2; i++) ln(DARK, r * 0.025, x + s * r * 0.2, y + r * 0.22 + i * r * 0.1, x + s * r * 0.85, y + r * 0.15 + i * r * 0.14); }
                        ell(x, y + r * 0.15, r * 0.12, r * 0.08, U.C("#ff8ab0"));
                        smile(x, y + r * 0.25, r, 0.25);
                        rect(x - r * 0.13, y + r * 0.45, r * 0.12, r * 0.18, W_); rect(x + r * 0.01, y + r * 0.45, r * 0.12, r * 0.18, W_);
                        rectStroke(x - r * 0.13, y + r * 0.45, r * 0.26, r * 0.18, U.C("#888888"), Math.Max(0.5, r * 0.02));
                        break;
                    }
                case "lucas":
                    {
                        Color c = ch.skin;
                        ell(x, y - r * 0.05, r * 0.78, r * 0.88, c);
                        foreach (int s in new[] { -1, 1 }) { ell(x + s * r * 0.24, y - r * 0.25, r * 0.22, r * 0.3, W_); circ(x + s * r * 0.2, y - r * 0.2, r * 0.09, DARK); }
                        ln(DARK, r * 0.08, x - r * 0.45, y - r * 0.6, x - r * 0.05, y - r * 0.5); ln(DARK, r * 0.08, x + r * 0.05, y - r * 0.5, x + r * 0.45, y - r * 0.6);
                        ell(x, y + r * 0.3, r * 0.62, r * 0.18, U.C("#ff9a1a")); ell(x, y + r * 0.45, r * 0.55, r * 0.14, U.C("#e8781a"));
                        ell(x, y + r * 0.8, r * 0.5, r * 0.12, W_);
                        break;
                    }
                default:
                    drawHead(ch, x, y, r); break;
            }
            Restore(st);
        }

        // Los Simpson: cara amarilla, ojos grandes y peinados de cada uno
        static void drawSimpson(Ch ch, double x, double y, double r)
        {
            Color c = ch.skin; string k = ch.kind;
            if (k == "marge") { rrFill(x - r * 0.6, y - r * 2.4, r * 1.2, r * 2.0, r * 0.5, ch.hair); for (int i = 0; i < 4; i++) arc(U.Shade(ch.hair, -0.25), Math.Max(1, r * 0.05), x, y - r * 1.9 + i * r * 0.4, r * 0.4, 0.2 * Math.PI, 0.8 * Math.PI, true); }
            if (k == "lisa") for (int i = 0; i < 8; i++) { double a = Math.PI * (1.05 + i * 0.9 / 7); fillPts(c, x + Math.Cos(a - 0.15) * r * 0.6, y - r * 0.1 + Math.Sin(a - 0.15) * r * 0.6, x + Math.Cos(a) * r * 1.25, y - r * 0.1 + Math.Sin(a) * r * 1.25, x + Math.Cos(a + 0.15) * r * 0.6, y - r * 0.1 + Math.Sin(a + 0.15) * r * 0.6); }
            if (k == "milhouse") ell(x - r * 0.1, y - r * 0.6, r * 0.75, r * 0.45, ch.hair);
            // cabeza (Bart y Lisa con forma más alta)
            if (k == "bart")
            {
                rrFill(x - r * 0.62, y - r * 1.0, r * 1.24, r * 1.7, r * 0.3, c);
                for (int i = 0; i < 9; i++) { double px = x - r * 0.62 + i * r * 1.24 / 8; fillPts(c, px - r * 0.08, y - r * 0.95, px, y - r * 1.2, px + r * 0.08, y - r * 0.95); }
            }
            else ell(x, y - r * 0.05, r * 0.72, r * 0.88, c);
            if (k == "homero")
            {
                arc(DARK, Math.Max(1, r * 0.04), x - r * 0.12, y - r * 0.85, r * 0.15, Math.PI, 2 * Math.PI); arc(DARK, Math.Max(1, r * 0.04), x + r * 0.12, y - r * 0.85, r * 0.15, Math.PI, 2 * Math.PI);
                ell(x, y + r * 0.45, r * 0.55, r * 0.32, U.C("#c8a878"));
                foreach (int s in new[] { -1, 1 }) { ell(x + s * r * 0.66, y + r * 0.0, r * 0.12, r * 0.18, c); }
            }
            // ojos grandes de los Simpson
            foreach (int s in new[] { -1, 1 }) { circ(x + s * r * 0.24, y - r * 0.2, r * 0.25, W_); ellStroke(x + s * r * 0.24, y - r * 0.2, r * 0.25, r * 0.25, DARK, Math.Max(1, r * 0.04)); circ(x + s * r * 0.22, y - r * 0.18, r * 0.06, DARK); }
            if (k == "milhouse") foreach (int s in new[] { -1, 1 }) ellStroke(x + s * r * 0.24, y - r * 0.2, r * 0.32, r * 0.3, U.C("#c8302a"), Math.Max(1.5, r * 0.08));
            ell(x, y + r * 0.08, r * 0.12, r * 0.2, c); ellStroke(x, y + r * 0.08, r * 0.12, r * 0.2, U.Shade(c, -0.3), Math.Max(0.6, r * 0.03));
            if (k == "homero") arc(DARK, Math.Max(1, r * 0.05), x, y + r * 0.35, r * 0.28, 0.15 * Math.PI, 0.85 * Math.PI, true);
            else smile(x, y + r * 0.35, r, 0.24);
            if (k == "marge" || k == "lisa") for (int i = -2; i <= 2; i++) circ(x + i * r * 0.14, y + r * 0.8, r * 0.07, k == "marge" ? U.C("#e83a3a") : W_);
        }

        // ===================== uniformes =====================
        // dibuja el diseño de la camiseta dentro de un rectángulo (lo que se sale se recorta)
        static void kitPattern(Kit k, double x, double y, double w, double h)
        {
            rect(x, y, w, h, k.shirt);
            switch (k.pat)
            {
                case "stripes": for (int i = 1; i < 6; i += 2) rect(x + i * w / 6, y, w / 6, h, k.shirt2); break;
                case "hoops": for (int i = 1; i < 6; i += 2) rect(x, y + i * h / 6, w, h / 6, k.shirt2); break;
                case "vband": rect(x + w * 0.36, y, w * 0.28, h, k.shirt2); break;
                case "sash": fillPts(k.shirt2, x, y + h * 0.05, x + w * 0.28, y, x + w, y + h * 0.8, x + w, y + h, x + w * 0.72, y + h); break;
                case "chest": rect(x, y, w, h * 0.1, k.shirt2); break;
            }
        }
        static void clipped(GraphicsPath p, Action a)
        {
            var st = Save(); g.SetClip(p, CombineMode.Intersect); a(); Restore(st);
        }
        // camiseta como ícono (menús y tarjetas de equipo)
        public static void drawShirt(Kit k, double x, double y, double s, string num = null)
        {
            using (var p = new GraphicsPath())
            {
                p.AddPolygon(pts(x - s * 0.3, y - s * 0.5, x - s * 0.12, y - s * 0.5, x, y - s * 0.42, x + s * 0.12, y - s * 0.5, x + s * 0.3, y - s * 0.5, x + s * 0.55, y - s * 0.3,
                    x + s * 0.45, y - s * 0.12, x + s * 0.32, y - s * 0.2, x + s * 0.32, y + s * 0.5, x - s * 0.32, y + s * 0.5, x - s * 0.32, y - s * 0.2, x - s * 0.45, y - s * 0.12, x - s * 0.55, y - s * 0.3));
                clipped(p, () =>
                {
                    kitPattern(k, x - s * 0.32, y - s * 0.5, s * 0.64, s);
                    Color sl = k.pat == "sleeves" ? k.shirt2 : k.pat == "plain" || k.pat == "chest" ? k.shirt : k.shirt2;
                    fillPts(sl, x - s * 0.3, y - s * 0.5, x - s * 0.55, y - s * 0.3, x - s * 0.45, y - s * 0.12, x - s * 0.32, y - s * 0.2, x - s * 0.32, y - s * 0.5);
                    fillPts(sl, x + s * 0.3, y - s * 0.5, x + s * 0.55, y - s * 0.3, x + s * 0.45, y - s * 0.12, x + s * 0.32, y - s * 0.2, x + s * 0.32, y - s * 0.5);
                    ell(x, y - s * 0.5, s * 0.13, s * 0.09, U.Shade(k.shirt2, -0.1));
                });
                g.DrawPath(P(U.RGBA(0, 0, 0, 0.55), Math.Max(1, s * 0.03)), p);
            }
            if (num != null) txt(num, x, y + s * 0.08, s * 0.32, k.pat == "plain" || k.pat == "chest" ? k.shirt2 : W_, "center", Color.Black, Math.Max(1, s * 0.03));
        }
        // escudo del equipo (forma de escudo con sus colores y sus siglas)
        public static void drawCrest(Team t, double x, double y, double s)
        {
            using (var p = new GraphicsPath())
            {
                p.AddPolygon(pts(x - s * 0.45, y - s * 0.5, x + s * 0.45, y - s * 0.5, x + s * 0.45, y + s * 0.05, x, y + s * 0.55, x - s * 0.45, y + s * 0.05));
                clipped(p, () => { kitPattern(t.home, x - s * 0.45, y - s * 0.5, s * 0.9, s * 1.05); rect(x - s * 0.45, y - s * 0.22, s * 0.9, s * 0.28, U.RGBA(0, 0, 0, 0.55)); });
                g.DrawPath(P(U.C("#ffd23a"), Math.Max(1.2, s * 0.06)), p);
            }
            txt(t.abbr, x, y - s * 0.08, s * 0.25, W_, "center", Color.Black, Math.Max(1, s * 0.03));
        }

        // ===================== cuerpo con uniforme =====================
        // (x, y) = los pies; k = tamaño; el jugador mira a dir (1 derecha, -1 izquierda)
        public static void drawBody(Ch ch, Kit kit, Color gkCol, bool gk, double x, double y, double k, int dir, double ph, bool moving, double kick, int num, double armUp = 0)
        {
            double sc = k * (ch.big ? 1.15 : 1) * ch.tall;
            Color shirt = gk ? gkCol : kit.shirt, shorts = gk ? U.Shade(gkCol, -0.35) : kit.shorts, socks = gk ? gkCol : kit.socks;
            Color skin = U.First(ch.skin, ch.body, ch.fur);
            double hip = y - 22 * sc;
            for (int l = -1; l <= 1; l += 2)
            {
                double a = ph + (l > 0 ? Math.PI : 0), sw = moving ? Math.Sin(a) * 9 * sc : 0;
                if (kick > 0 && l == 1) sw = 15 * sc;
                double fx = x + l * 5 * sc + sw * dir, fy = y - (moving ? Math.Max(0, -Math.Cos(a)) * 6 * sc : 0);
                lines(skin, 6.5 * sc, true, x + l * 5 * sc, hip, fx, fy - 9 * sc);
                lines(socks, 7.5 * sc, true, x + l * 5 * sc + (fx - x - l * 5 * sc) * 0.5, (hip + fy) / 2, fx, fy - 4 * sc);
                ell(fx + dir * 3 * sc, fy - 3 * sc, 7.5 * sc, 4.5 * sc, U.C("#1a1a1a")); ell(fx + dir * 4 * sc, fy - 4 * sc, 3 * sc, 1.5 * sc, W_);
            }
            double bw = 28 * sc, bh = 26 * sc, by = hip - bh + 4 * sc;
            rrFill(x - bw * 0.52, hip - 9 * sc, bw * 1.04, 11 * sc, 3 * sc, shorts);
            using (var p = rrPath(x - bw / 2, by, bw, bh, bw * 0.3))
            {
                if (gk) g.FillPath(B(shirt), p);
                else clipped(p, () => kitPattern(kit, x - bw / 2, by, bw, bh));
                g.DrawPath(P(U.RGBA(0, 0, 0, 0.35), Math.Max(0.6, sc)), p);
            }
            ell(x, by + 1.5 * sc, bw * 0.22, 3 * sc, gk ? U.Shade(shirt, -0.3) : U.Shade(kit.shirt2, -0.1));
            if (num > 0 && sc > 0.35) txt(num.ToString(), x, by + bh * 0.55, 11 * sc, gk ? DARK : (kit.pat == "plain" || kit.pat == "chest" ? kit.shirt2 : W_), "center", Color.Black, Math.Max(0.5, sc * 0.8));
            // brazos con mangas (los arqueros con guantes)
            Color sleeve = gk ? shirt : kit.pat == "sleeves" ? kit.shirt2 : kit.shirt;
            double asw = moving ? Math.Sin(ph) * 7 * sc : 0, ay = by + bh * 0.4 - armUp * 14 * sc;
            foreach (int s in new[] { -1, 1 })
            {
                double ax = x + s * (bw * 0.58 + (s < 0 ? -asw : asw) * dir * 0.3);
                circ(x + s * bw * 0.48, by + 6 * sc, 5.5 * sc, sleeve);
                circ(ax, ay, (gk ? 6.5 : 4.5) * sc, gk ? W_ : skin);
            }
        }

        // ===================== trofeos =====================
        public static void drawTrophy(string comp, double x, double y, double s, bool won = true)
        {
            double oa = ALPHA; if (!won) ALPHA = oa * 0.3;
            Color gold = C("#ffd23a"), gold2 = C("#c89a1a"), silver = C("#dfe6ee"), silver2 = C("#9aa6b4");
            switch (comp)
            {
                case "champions":   // la "orejona": copa plateada con asas enormes
                    rrFill(x - s * 0.3, y - s * 0.12, s * 0.6, s * 0.14, s * 0.03, C("#2a2a3a"));
                    fillPts(silver2, x - s * 0.08, y - s * 0.12, x + s * 0.08, y - s * 0.12, x + s * 0.05, y - s * 0.4, x - s * 0.05, y - s * 0.4);
                    fillPts(silver, x - s * 0.32, y - s * 0.95, x + s * 0.32, y - s * 0.95, x + s * 0.14, y - s * 0.4, x - s * 0.14, y - s * 0.4);
                    foreach (int sd in new[] { -1, 1 }) { ellStroke(x + sd * s * 0.36, y - s * 0.7, s * 0.2, s * 0.26, silver, Math.Max(2, s * 0.07)); }
                    ell(x, y - s * 0.95, s * 0.32, s * 0.06, silver2); ell(x - s * 0.1, y - s * 0.7, s * 0.05, s * 0.18, W_);
                    break;
                case "premier":     // copa con corona y base morada
                    rrFill(x - s * 0.28, y - s * 0.16, s * 0.56, s * 0.18, s * 0.03, C("#3d195b"));
                    fillPts(gold2, x - s * 0.08, y - s * 0.16, x + s * 0.08, y - s * 0.16, x + s * 0.06, y - s * 0.38, x - s * 0.06, y - s * 0.38);
                    fillPts(silver, x - s * 0.3, y - s * 0.85, x + s * 0.3, y - s * 0.85, x + s * 0.2, y - s * 0.42, x, y - s * 0.36, x - s * 0.2, y - s * 0.42);
                    foreach (int sd in new[] { -1, 1 }) ellStroke(x + sd * s * 0.32, y - s * 0.68, s * 0.1, s * 0.13, silver, Math.Max(1.5, s * 0.04));
                    fillPts(gold, x - s * 0.16, y - s * 0.85, x - s * 0.16, y - s * 1.0, x - s * 0.08, y - s * 0.92, x, y - s * 1.05, x + s * 0.08, y - s * 0.92, x + s * 0.16, y - s * 1.0, x + s * 0.16, y - s * 0.85);
                    ell(x - s * 0.1, y - s * 0.65, s * 0.04, s * 0.14, W_);
                    break;
                case "laliga":      // copa alta plateada con detalles rojos
                    rrFill(x - s * 0.24, y - s * 0.14, s * 0.48, s * 0.16, s * 0.03, C("#ee2737"));
                    fillPts(silver2, x - s * 0.06, y - s * 0.14, x + s * 0.06, y - s * 0.14, x + s * 0.05, y - s * 0.5, x - s * 0.05, y - s * 0.5);
                    fillPts(silver, x - s * 0.22, y - s * 1.05, x + s * 0.22, y - s * 1.05, x + s * 0.18, y - s * 0.55, x, y - s * 0.48, x - s * 0.18, y - s * 0.55);
                    rect(x - s * 0.21, y - s * 0.85, s * 0.42, s * 0.06, C("#ee2737"));
                    ell(x, y - s * 1.05, s * 0.22, s * 0.05, silver2); ell(x - s * 0.08, y - s * 0.7, s * 0.03, s * 0.14, W_);
                    break;
                case "supercopa":   // copa plateada con base azul
                    rrFill(x - s * 0.26, y - s * 0.18, s * 0.52, s * 0.2, s * 0.04, C("#1a4a8a"));
                    fillPts(silver2, x - s * 0.07, y - s * 0.18, x + s * 0.07, y - s * 0.18, x + s * 0.05, y - s * 0.4, x - s * 0.05, y - s * 0.4);
                    halfEll(x, y - s * 0.62, s * 0.3, s * 0.3, silver); rect(x - s * 0.3, y - s * 0.66, s * 0.6, s * 0.05, silver2);
                    fillPts(silver, x - s * 0.3, y - s * 0.62, x + s * 0.3, y - s * 0.62, x + s * 0.1, y - s * 0.4, x - s * 0.1, y - s * 0.4);
                    foreach (int sd in new[] { -1, 1 }) ellStroke(x + sd * s * 0.33, y - s * 0.6, s * 0.1, s * 0.12, silver, Math.Max(1.5, s * 0.04));
                    break;
                case "mundial":     // figura dorada sosteniendo el mundo
                    rrFill(x - s * 0.2, y - s * 0.18, s * 0.4, s * 0.2, s * 0.04, gold2);
                    rect(x - s * 0.2, y - s * 0.12, s * 0.4, s * 0.04, C("#1a8a4a")); rect(x - s * 0.2, y - s * 0.05, s * 0.4, s * 0.03, C("#1a8a4a"));
                    fillPts(gold, x - s * 0.17, y - s * 0.18, x + s * 0.17, y - s * 0.18, x + s * 0.08, y - s * 0.55, x + s * 0.22, y - s * 0.75, x - s * 0.22, y - s * 0.75, x - s * 0.08, y - s * 0.55);
                    circ(x, y - s * 0.92, s * 0.22, gold);
                    foreach (int sd in new[] { -1, 1 }) quadLine(gold2, Math.Max(1, s * 0.03), x + sd * s * 0.1, y - s * 0.3, x + sd * s * 0.3, y - s * 0.65, x + sd * s * 0.12, y - s * 0.85);
                    ellStroke(x, y - s * 0.92, s * 0.22, s * 0.08, gold2, Math.Max(1, s * 0.025)); ellStroke(x, y - s * 0.92, s * 0.08, s * 0.22, gold2, Math.Max(1, s * 0.025));
                    circ(x - s * 0.08, y - s * 1.0, s * 0.05, W_);
                    break;
                default:            // amistoso: copa dorada sencilla
                    rrFill(x - s * 0.22, y - s * 0.14, s * 0.44, s * 0.16, s * 0.03, C("#5a3a1a"));
                    fillPts(gold2, x - s * 0.06, y - s * 0.14, x + s * 0.06, y - s * 0.14, x + s * 0.05, y - s * 0.38, x - s * 0.05, y - s * 0.38);
                    halfEll(x, y - s * 0.6, s * 0.28, s * 0.28, gold); rect(x - s * 0.28, y - s * 0.64, s * 0.56, s * 0.05, gold2);
                    fillPts(gold, x - s * 0.28, y - s * 0.6, x + s * 0.28, y - s * 0.6, x + s * 0.08, y - s * 0.38, x - s * 0.08, y - s * 0.38);
                    break;
            }
            ALPHA = oa;
        }
        public static void drawBallIcon(double x, double y, double r)
        {
            circ(x, y, r, Color.White);
            for (int i = 0; i < 5; i++) { double a = i * Math.PI * 2 / 5 - Math.PI / 2; circ(x + Math.Cos(a) * r * 0.62, y + Math.Sin(a) * r * 0.62, r * 0.2, DARK); }
            circ(x, y, r * 0.28, DARK); ellStroke(x, y, r, r, U.C("#333333"), Math.Max(1, r * 0.08));
        }
    }
}
