using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FutbolToons
{
    // Dibujo del partido: estadio, cancha, jugadores, pelota y marcador
    static partial class G
    {
        static double socK(double y) { return FOC / (FW + CAMD - y); }
        static double SX(double x, double y) { return W / 2.0 + (x - M.camX) * socK(y); }
        static double SY(double y, double z) { return HZ + (CAMZ - z) * socK(y); }
        static void quadW(Color c, double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
            fillPts(c, SX(x1, y1), SY(y1, 0), SX(x2, y2), SY(y2, 0), SX(x3, y3), SY(y3, 0), SX(x4, y4), SY(y4, 0));
        }
        static void lineW(Color c, double lw, double x1, double y1, double x2, double y2) { lines(c, lw, true, SX(x1, y1), SY(y1, 0), SX(x2, y2), SY(y2, 0)); }
        static void wallW(Color c, double x1, double y1, double x2, double y2, double h)
        {
            fillPts(c, SX(x1, y1), SY(y1, 0), SX(x2, y2), SY(y2, 0), SX(x2, y2), SY(y2, h), SX(x1, y1), SY(y1, h));
        }
        static void circW(Color c, double lw, double cx, double cy, double r)
        {
            var xy = new List<double>();
            for (int i = 0; i <= 36; i++) { double a = i * Math.PI * 2 / 36, x = cx + Math.Cos(a) * r, y = cy + Math.Sin(a) * r; xy.Add(SX(x, y)); xy.Add(SY(y, 0)); }
            lines(c, lw, true, xy.ToArray());
        }

        public static void DrawMatch(bool hud)
        {
            var s = M; var cf = s.st; var b = s.ball;
            var st0 = Save();
            if (s.shake > 0) Translate((U.R() - 0.5) * 14 * s.shake / 0.4, (U.R() - 0.5) * 8 * s.shake / 0.4);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;   // el césped y el fondo sin suavizado (es mucho más rápido)
            Gradient(-20, -20, W + 40, 200, cf.sky0, cf.sky1);
            rect(-20, 150, W + 40, H, cf.outside);
            drawStands();
            quadW(cf.outside, -FL - 900, -FW - 50, FL + 900, -FW - 50, FL + 900, FW + 300, -FL - 900, FW + 300);
            if (cf.sea) drawSeaFloor();
            // franjas del césped
            int n = 18; double bw = 2 * FL / n;
            for (int i = 0; i < n; i++) { double x0 = -FL + i * bw; quadW(i % 2 == 0 ? cf.g1 : cf.g2, x0, -FW, x0 + bw, -FW, x0 + bw, FW, x0, FW); }
            if (cf.tiles)
            {
                Color tl = U.Shade(cf.g1, -0.15);
                for (double tx = -FL + 100; tx < FL; tx += 100) lineW(tl, 1.3, tx, -FW, tx, FW);
                for (double ty = -FW + 117.5; ty < FW; ty += 117.5) lineW(tl, 1.3, -FL, ty, FL, ty);
            }
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            // líneas
            Color lc = U.A(cf.line, 0.85); double lw = 2.6;
            lineW(lc, lw, -FL, -FW, FL, -FW); lineW(lc, lw, FL, -FW, FL, FW); lineW(lc, lw, FL, FW, -FL, FW); lineW(lc, lw, -FL, FW, -FL, -FW);
            lineW(lc, lw, 0, -FW, 0, FW); circW(lc, lw, 0, 0, 140);
            foreach (int sd in new[] { -1, 1 })
            {
                double x0 = sd * FL, x1 = sd * (FL - 220), x2 = sd * (FL - 90);
                lineW(lc, lw, x0, -270, x1, -270); lineW(lc, lw, x1, -270, x1, 270); lineW(lc, lw, x1, 270, x0, 270);
                lineW(lc, lw, x0, -160, x2, -160); lineW(lc, lw, x2, -160, x2, 160); lineW(lc, lw, x2, 160, x0, 160);
                double px = sd * (FL - 150); ell(SX(px, 0), SY(0, 0), 4 * socK(0), 2 * socK(0), lc);
                // arcos de las esquinas
                foreach (int cy in new[] { -1, 1 }) { var xy = new List<double>(); for (int i = 0; i <= 8; i++) { double a = i * Math.PI / 2 / 8, xx = sd * FL - sd * Math.Sin(a) * 30, yy = cy * FW - cy * Math.Cos(a) * 30; xy.Add(SX(xx, yy)); xy.Add(SY(yy, 0)); } lines(lc, lw * 0.8, true, xy.ToArray()); }
            }
            ell(SX(0, 0), SY(0, 0), 5 * socK(0), 2.5 * socK(0), lc);
            // publicidad del fondo y paredes de los costados
            for (int i = 0; i < 12; i++) { double x0 = -FL - 100 + i * (2 * FL + 200) / 12.0; wallW(i % 2 == 0 ? cf.wall : cf.wall2, x0, -FW - 6, x0 + (2 * FL + 200) / 12.0, -FW - 6, 42); }
            if (!s.demo || true)
            {
                string[] ads = { "FÚTBOL TOONS", s.compName == "" ? "PARTIDO AMISTOSO" : s.compName.ToUpper(), "¡GOOOL!", "FÚTBOL TOONS" };
                for (int i = 0; i < 4; i++) { double ax = -FL + 100 + i * (2 * FL - 200) / 3.0, sx = SX(ax, -FW - 6); if (sx > -150 && sx < W + 150) txt(ads[i], sx, SY(-FW - 6, 21), 15 * socK(-FW) * 2, i % 2 == 0 ? cf.wall2 : cf.wall, "center", U.RGBA(0, 0, 0, 0.5), 2); }
            }
            foreach (int sd in new[] { -1, 1 }) { wallW(U.Shade(cf.wall, -0.2), sd * (FL + 6), -FW, sd * (FL + 6), -GW, 42); wallW(U.Shade(cf.wall, -0.2), sd * (FL + 6), GW, sd * (FL + 6), FW, 42); }
            if (cf.night) drawNightLights();
            foreach (int sd in new[] { -1, 1 }) drawGoalBack(sd);
            // sombras y anillos de equipo
            foreach (var p in s.pl)
            {
                double k = socK(p.y), x = SX(p.x, p.y), y = SY(p.y, 0); bool hu = p.human >= 0 && !s.allAI;
                ell(x, y, 19 * k, 6.5 * k, U.RGBA(0, 0, 0, 0.28));
                ellStroke(x, y, 23 * k, 8 * k, hu ? (s.np > 1 ? P_COL[p.human] : Y_) : (p.gk ? s.gkCol[p.team] : s.kit[p.team].shirt), hu ? 3.5 : 2);
            }
            { double k = socK(b.y); ell(SX(b.x, b.y), SY(b.y, 0), BR * k * (1 - Math.Min(0.5, b.z / 300)), BR * 0.4 * k, U.RGBA(0, 0, 0, 0.3)); }
            var items = new List<Tuple<double, Action>>();
            foreach (var p in s.pl) { var pp = p; items.Add(Tuple.Create(p.y, (Action)(() => drawPl(pp)))); }
            items.Add(Tuple.Create(b.y + 0.5, (Action)drawBall));
            foreach (int sd in new[] { -1, 1 }) { int d = sd; items.Add(Tuple.Create(GW, (Action)(() => drawPost(d, GW)))); }
            foreach (var it in items.OrderBy(t => t.Item1)) it.Item2();
            if (s.phase == "pens" && s.penStep == "aim" && penHumanShoots()) drawPenAim();
            double oa = ALPHA; ALPHA = 0.45;
            wallW(cf.wall, -FL - 100, FW + 8, FL + 100, FW + 8, 40);
            ALPHA = oa;
            if (cf.sea) drawBubbles();
            Restore(st0);
            if (hud) drawMatchHUD();
        }
        static void drawStands()
        {
            var s = M; var cf = s.st; double y = -FW - 60, k = socK(y);
            fillPts(cf.stand, -20, SY(y, 0), W + 20, SY(y, 0), W + 20, SY(y, 250), -20, SY(y, 250));
            bool party = s.phase == "goal" || s.phase == "end";
            double x0 = s.camX - (W / 2.0 + 30) / k, x1 = s.camX + (W / 2.0 + 30) / k;
            Color a = s.kit[0].shirt, a2 = s.kit[0].shirt2, b = s.kit[1].shirt;
            for (int r = 0; r < 4; r++)
            {
                double z = 70 + r * 48; int c0 = (int)Math.Floor(x0 / 46);
                for (int c = c0; c * 46 < x1; c++)
                {
                    int h = Math.Abs((c * 73856093) ^ (r * 19349663));
                    double bob = party ? Math.Abs(Math.Sin(T * 12 + h % 7)) * 14 : Math.Sin(T * 2 + h % 5) * 2;
                    double x = c * 46 + (r % 2) * 23, sy = SY(y, z + bob);
                    if (sy < -10) continue;
                    // la mitad izquierda alienta al local y la derecha al visitante
                    bool home = x < 0 ? h % 5 != 0 : h % 5 == 0;
                    Color shirt = h % 7 == 0 ? U.Hsl(h % 360, 0.6, 0.55) : home ? (h % 3 == 0 ? a2 : a) : b;
                    rect(SX(x, y) - 7 * k, sy, 14 * k, 16 * k, shirt);
                    circ(SX(x, y), sy - 4 * k, 7 * k, (h % 4 == 0) ? U.C("#8a5a3a") : U.C("#ffcc99"));
                }
            }
        }
        static void drawNightLights()
        {
            var s = M; double y = -FW - 60, k = socK(y);
            for (int i = -2; i <= 2; i++)
            {
                double lx = i * 700, sx = SX(lx, y), sy = SY(y, 255);
                if (sx < -150 || sx > W + 150) continue;
                fillPts(U.RGBA(255, 250, 200, 0.07), sx - 20, sy, sx + 20, sy, SX(lx * 0.5 + 280, 120), SY(120, 0), SX(lx * 0.5 - 280, 120), SY(120, 0));
                circ(sx, sy, 120 * k, U.RGBA(255, 250, 210, 0.12));
                rrFill(sx - 26, sy - 14, 52, 22, 4, U.C("#3a3a4a"));
                for (int a = 0; a < 4; a++) for (int bb = 0; bb < 2; bb++) circ(sx - 18 + a * 12, sy - 8 + bb * 10, 4.2, U.C("#fffbe0"));
            }
            int n = s.phase == "goal" || s.phase == "end" ? 10 : 2;
            for (int i = 0; i < n; i++) if (U.R() < 0.5) circ(U.R() * W, SY(y, 70 + U.R() * 150), 3 + U.R() * 3, U.RGBA(255, 255, 255, 0.9));
        }
        // Fondo de Bikini: algas y corales a los lados, burbujas que suben
        static void drawSeaFloor()
        {
            foreach (int sd in new[] { -1, 1 })
                for (int i = 0; i < 6; i++)
                {
                    double wx = sd * (FL + 150 + (i % 3) * 140), wy = -380 + i * 150, k = socK(wy), x = SX(wx, wy), y = SY(wy, 0);
                    if (x < -100 || x > W + 100) continue;
                    if (i % 2 == 0) for (int j = 0; j < 3; j++) quadLine(U.C("#2a9a4a"), 9 * k, x + (j - 1) * 14 * k, y, x + (j - 1) * 14 * k + Math.Sin(T * 2 + i + j) * 30 * k, y - 120 * k, x + (j - 1) * 20 * k, y - 220 * k);
                    else { circ(x, y - 30 * k, 40 * k, U.C("#ff6a8a")); circ(x - 30 * k, y - 50 * k, 26 * k, U.C("#ff8aa8")); circ(x + 28 * k, y - 60 * k, 22 * k, U.C("#ffa8c0")); }
                }
        }
        static void drawBubbles()
        {
            for (int i = 0; i < 26; i++)
            {
                double ph = (T * 0.15 + i * 0.137) % 1.0, x = ((i * 97) % 960) + Math.Sin(T * 2 + i) * 10, y = H - ph * (H + 40);
                ellStroke(x, y, 4 + (i % 4) * 2, 4 + (i % 4) * 2, U.RGBA(220, 250, 255, 0.55), 1.5);
            }
        }
        static void drawGoalBack(int sd)
        {
            double fx = sd * FL, bx = sd * (FL + GD); Color net = U.RGBA(255, 255, 255, 0.22), nl = U.RGBA(255, 255, 255, 0.55);
            fillPts(net, SX(bx, -GW), SY(-GW, 0), SX(bx, GW), SY(GW, 0), SX(bx, GW), SY(GW, GH), SX(bx, -GW), SY(-GW, GH));
            fillPts(net, SX(fx, -GW), SY(-GW, GH), SX(bx, -GW), SY(-GW, GH), SX(bx, GW), SY(GW, GH), SX(fx, GW), SY(GW, GH));
            fillPts(net, SX(fx, -GW), SY(-GW, 0), SX(bx, -GW), SY(-GW, 0), SX(bx, -GW), SY(-GW, GH), SX(fx, -GW), SY(-GW, GH));
            for (int i = 0; i <= 8; i++) { double yy = -GW + i * GW / 4; lines(nl, 1, false, SX(bx, yy), SY(yy, 0), SX(bx, yy), SY(yy, GH), SX(fx, yy), SY(yy, GH)); }
            for (int i = 1; i <= 3; i++) { double z = i * GH / 4; lines(nl, 1, false, SX(bx, -GW), SY(-GW, z), SX(bx, GW), SY(GW, z)); }
            lines(Color.White, 6 * socK(-GW), true, SX(fx, -GW), SY(-GW, 0), SX(fx, -GW), SY(-GW, GH));
            lines(Color.White, 6 * socK(0), true, SX(fx, -GW), SY(-GW, GH), SX(fx, GW), SY(GW, GH));
        }
        static void drawPost(int sd, double py)
        {
            double fx = sd * FL, bx = sd * (FL + GD), k = socK(py);
            fillPts(U.RGBA(255, 255, 255, 0.22), SX(fx, py), SY(py, 0), SX(bx, py), SY(py, 0), SX(bx, py), SY(py, GH), SX(fx, py), SY(py, GH));
            lines(Color.White, 6 * k, true, SX(fx, py), SY(py, 0), SX(fx, py), SY(py, GH));
        }
        static void drawPl(MPl p)
        {
            var s = M; var ch = p.ch; double k = socK(p.y), x = SX(p.x, p.y), y = SY(p.y, 0);
            if ((s.phase == "goal" && p.team == s.lastGoalTeam) || (s.phase == "end" && s.Winner == p.team)) y -= Math.Abs(Math.Sin(T * 9 + p.slot * 1.3)) * 26 * k;
            double sc = k * (ch.big ? 1.15 : 1) * ch.tall;
            double spd = hyp(p.vx, p.vy); int dir = Math.Cos(p.face) >= 0 ? 1 : -1; bool moving = spd > 25 && p.stun <= 0 && p.frozen <= 0;
            var st = Save();
            if (p.slide > 0 || p.stun > 0.3) { double py = y - 12 * sc; Translate(x, py); Rotate((p.slide > 0 ? -dir : dir) * 1.2); Translate(-x, -py); }
            bool armsUp = p.gk && (p.dive > 0 || s.ball.owner == p);
            drawBody(ch, s.kit[p.team], s.gkCol[p.team], p.gk, x, y, k, dir, p.run, moving, p.kickAnim, p.num, armsUp ? 1 : 0);
            double hr = 19 * sc * (ch.big ? 1.05 : 1), hy = y - 44 * sc - hr * 0.72;
            drawFace(p.pwT > 0 && ch.pw == "ssj" ? ssjOf(ch) : ch, x, hy, hr);
            Restore(st);
            double top = hy - hr * 1.3;
            if (p.frozen > 0) rrFill(x - 22 * sc, top, 44 * sc, y - top, 6 * sc, U.RGBA(190, 240, 255, 0.5));
            if (p.stun > 0 && p.frozen <= 0) for (int i = 0; i < 3; i++) drawStar(x + Math.Cos(T * 6 + i * 2.1) * 16 * sc, top + Math.Sin(T * 6 + i * 2.1) * 4 * sc, 5 * sc);
            if (p.pwT > 0) { Color pc = PW_COL.ContainsKey(ch.pw) ? PW_COL[ch.pw] : Y_; ellStroke(x, y - 30 * sc, 32 * k, 44 * k, U.A(pc, 0.5 + Math.Sin(T * 20) * 0.3), 3); }
            if (p.human >= 0 && !s.allAI)
            {
                Color pc = s.np > 1 ? P_COL[p.human] : Y_; double my = top - 6 + Math.Sin(T * 6) * 3;
                fillPts(pc, x - 8, my - 10, x + 8, my - 10, x, my);
                txt((s.np > 1 ? "J" + (p.human + 1) + " " : "") + ch.name, x, my - 19, 12, pc, "center", Color.Black, 3);
                if (p.charge >= 0) { double c = Math.Min(1, p.charge); rrFill(x - 24, my - 38, 48, 8, 3, U.RGBA(0, 0, 0, 0.6)); rrFill(x - 23, my - 37, 46 * c, 6, 3, U.Hsl(120 - c * 120, 0.9, 0.5)); }
            }
        }
        static void drawBall()
        {
            var b = M.ball; double k = socK(b.y), big = b.sup == "giant" && b.superT > 0 ? 2 : 1, r = BR * k * big;
            double x = SX(b.x, b.y), y = SY(b.y, b.z + BR * big);
            double oa = ALPHA;
            for (int i = 0; i < b.trail.Count; i++)
            {
                var t = b.trail[i]; double a = (i + 1) / (double)b.trail.Count;
                ALPHA = oa * a * 0.6; circ(SX(t[0], t[1]), SY(t[1], t[2] + BR), r * (0.5 + 0.5 * a), b.supCol);
            }
            ALPHA = oa * (b.sup == "ghost" && b.superT > 0 ? 0.3 : 1);
            if (b.superT > 0) circ(x, y, r * 1.5, U.A(b.supCol, 0.35));
            if (b.sup == "bubble" && b.superT > 0) { circ(x, y, r * 1.9, U.RGBA(160, 230, 255, 0.35)); ellStroke(x, y, r * 1.9, r * 1.9, U.RGBA(255, 255, 255, 0.8), 1.5); }
            circ(x, y, r, Color.White);
            for (int i = 0; i < 3; i++) { double a = b.spin + i * 2.1; circ(x + Math.Cos(a) * r * 0.5, y + Math.Sin(a) * r * 0.4, r * 0.27, DARK); }
            ellStroke(x, y, r, r, U.C("#333333"), 1.2);
            if (b.sup == "electric" && b.superT > 0) for (int i = 0; i < 3; i++) { double a = U.R() * Math.PI * 2; lines(U.C("#ffe14a"), 2, false, x, y, x + Math.Cos(a) * r * 2.2, y + Math.Sin(a) * r * 2.2); }
            ALPHA = oa;
        }
        static void drawPenAim()
        {
            var s = M; int sg = sgn(s.penTeam); double x = SX(sg * FL, s.penAimY), y = SY(s.penAimY, 45);
            ellStroke(x, y, 16, 16, Y_, 3); lines(Y_, 2, false, x - 22, y, x + 22, y); lines(Y_, 2, false, x, y - 22, x, y + 22);
        }
        static string minuteStr() { var s = M; int m = (int)Math.Min(90, Math.Floor(s.clock / s.dur * 90)); return m + "'"; }
        static void drawMatchHUD()
        {
            var s = M;
            // marcador de televisión
            rrFill(W / 2.0 - 200, 8, 400, 46, 10, U.RGBA(10, 14, 30, 0.85));
            for (int t = 0; t < 2; t++)
            {
                double cx = W / 2.0 + (t == 0 ? -1 : 1) * 150;
                rect(cx - 50 + (t == 0 ? 0 : 0), 12, 100, 4, s.kit[t].shirt);
                drawCrest(s.team[t], cx + (t == 0 ? -34 : 34), 32, 30);
                txt(s.team[t].abbr, cx + (t == 0 ? 10 : -10), 33, 18, W_, "center", Color.Black, 3);
            }
            rrFill(W / 2.0 - 50, 12, 100, 38, 8, U.RGBA(255, 255, 255, 0.12));
            txt(s.score[0] + " - " + s.score[1], W / 2.0, 31, 26, W_, "center", Color.Black, 4);
            string clk = s.phase == "pens" || (s.phase == "end" && s.penTaken[0] > 0) ? "PENALES" : s.phase == "half" ? "DESCANSO" : minuteStr() + (s.half == 1 ? "  1T" : "  2T");
            rrFill(W / 2.0 - 52, 54, 104, 20, 6, U.RGBA(10, 14, 30, 0.85));
            txt(clk, W / 2.0, 64, 12, s.phase == "pens" ? Y_ : W_, "center", Color.Black, 2);
            if (s.compName != "") txt(s.compName.ToUpper() + (s.stage != "" ? "  -  " + s.stage.ToUpper() : ""), W / 2.0, 86, 11, C("#dddddd"), "center", Color.Black, 3);
            if (s.phase == "pens" || (s.phase == "end" && s.penTaken[0] > 0)) drawPensHUD();
            // barras de súper tiro
            for (int t = 0; t < 2; t++)
            {
                double bx = t == 0 ? 16 : W - 236, by = H - 34; bool full = s.meter[t] >= 1;
                var who = s.ctl[t] ?? s.pl.First(p => p.team == t && p.slot == 0);
                Color pc = PW_COL.ContainsKey(who.ch.pw) ? PW_COL[who.ch.pw] : Y_;
                string label = (humanTeam(t) ? (s.np > 1 ? "J" + (t + 1) + " " : "") : "CPU ") + "SÚPER: " + who.ch.pwName;
                txt(label, bx + (t == 0 ? 0 : 220), by - 14, 13, pc, t == 0 ? "left" : "right", Color.Black, 3);
                rrFill(bx, by, 220, 18, 8, U.RGBA(0, 0, 0, 0.6));
                if (s.meter[t] > 0.01) rrFill(bx + 2, by + 2, 216 * s.meter[t], 14, 7, full ? U.Hsl((T * 300) % 360, 0.9, 0.6) : s.kit[t].shirt);
                if (full && humanTeam(t) && ((int)Math.Floor(T * 4)) % 2 == 0) txt("¡LISTO! pulsa " + (s.np > 1 ? (t == 0 ? "R" : "L") : "C"), bx + 110, by + 9, 12, W_, "center", Color.Black, 3);
            }
            // minimapa
            double mw = 150, mh = 80, mx = W / 2.0 - mw / 2, my = H - mh - 10;
            rrFill(mx - 5, my - 5, mw + 10, mh + 10, 8, U.RGBA(0, 0, 0, 0.5)); rect(mx, my, mw, mh, U.A(s.st.g1, 0.9));
            rectStroke(mx, my, mw, mh, U.A(s.st.line, 0.8), 1); lines(U.A(s.st.line, 0.8), 1, false, mx + mw / 2, my, mx + mw / 2, my + mh);
            double gy = mh * GW / (2 * FW);
            rect(mx - 4, my + mh / 2 - gy, 4, gy * 2, Color.White); rect(mx + mw, my + mh / 2 - gy, 4, gy * 2, Color.White);
            foreach (var p in s.pl)
            {
                double px = mx + (p.x + FL) / (2 * FL) * mw, py = my + (p.y + FW) / (2 * FW) * mh;
                if (p.human >= 0 && !s.allAI) circ(px, py, 5, s.np > 1 ? P_COL[p.human] : Y_);
                circ(px, py, 3.5, p.gk ? s.gkCol[p.team] : s.kit[p.team].shirt); ellStroke(px, py, 3.5, 3.5, U.RGBA(0, 0, 0, 0.6), 0.8);
            }
            { var b = s.ball; circ(mx + U.Clamp((b.x + FL) / (2 * FL), -0.03, 1.03) * mw, my + (b.y + FW) / (2 * FW) * mh, 2.8, Color.White); }
            // mensajes grandes
            if (s.msgT > 0)
            {
                double age = s.msgMax - s.msgT, pop = age < 0.2 ? 0.6 + age * 2 : 1, oa = ALPHA; ALPHA = Math.Min(1, s.msgT * 3);
                txt(s.msg, W / 2.0, H / 2.0 - 70, (s.msg.Length > 18 ? 38 : 54) * pop, s.msgCol, "center", Color.Black, 10);
                if (s.phase == "goal" && (s.msg.StartsWith("¡GO") || s.msg.StartsWith("¡GOLAZO"))) { var sc = s.ball.last; if (sc != null) txt(sc.ch.name + (sc.team != s.lastGoalTeam ? " (en contra)" : "") + "  " + minuteStr(), W / 2.0, H / 2.0 - 20, 24, W_, "center", Color.Black, 5); }
                ALPHA = oa;
            }
            if (s.phase == "half")
            {
                panel(W / 2.0 - 190, H / 2.0 - 10, 380, 90, 0.75, Color.Empty);
                txt(s.team[0].name + "  " + s.score[0] + " - " + s.score[1] + "  " + s.team[1].name, W / 2.0, H / 2.0 + 20, 18, W_);
                int tot = Math.Max(1, (int)(s.poss[0] + s.poss[1]));
                txt("Posesión " + (int)(s.poss[0] * 100 / tot) + "% - " + (100 - (int)(s.poss[0] * 100 / tot)) + "%   Tiros " + s.shots[0] + " - " + s.shots[1], W / 2.0, H / 2.0 + 52, 14, C("#dddddd"), "center", Color.Black, 3);
            }
            if (s.phase == "count")
            {
                int nn = (int)Math.Ceiling(s.phaseT);
                if (nn <= 3) txt(nn.ToString(), W / 2.0, H / 2.0 - 70, 90, Y_, "center", Color.Black, 12);
                drawControls(H / 2.0 + 10);
            }
            if (s.flash > 0) rect(0, 0, W, H, U.RGBA(255, 255, 255, Math.Min(1, s.flash * 3)));
        }
        static void drawPensHUD()
        {
            var s = M;
            panel(W / 2.0 - 190, 96, 380, 70, 0.75, Color.Empty);
            for (int t = 0; t < 2; t++)
            {
                double y = 116 + t * 28;
                txt(s.team[t].abbr, W / 2.0 - 150, y, 15, W_, "left", Color.Black, 3);
                int n = Math.Max(5, Math.Max(s.penTaken[0], s.penTaken[1]));
                for (int i = 0; i < n; i++)
                {
                    double cx = W / 2.0 - 70 + i * (n > 5 ? 160.0 / n * 1.5 : 30);
                    if (i < s.penLog[t].Count) circ(cx, y, 10, s.penLog[t][i] == 1 ? C("#3ac83a") : C("#e83a3a"));
                    else ellStroke(cx, y, 9, 9, U.RGBA(255, 255, 255, 0.6), 2);
                }
                txt(s.penGoals[t].ToString(), W / 2.0 + 165, y, 18, Y_, "center", Color.Black, 3);
            }
            if (s.phase != "pens") return;
            string hint = "";
            if (s.penStep == "aim" && penHumanShoots()) hint = (s.np > 1 ? "J" + (s.penTeam + 1) + ": " : "") + "¡Pulsa TIRAR cuando la mira apunte donde quieras!";
            else if (s.penStep == "aim" && penHumanKeeps()) hint = (s.np > 1 ? "J" + (2 - s.penTeam) + ": " : "") + "ARQUERO: mantén ARRIBA o ABAJO para lanzarte (nada = al medio)";
            if (hint != "") { rrFill(W / 2.0 - 300, 172, 600, 26, 8, U.RGBA(0, 0, 0, 0.6)); txt(hint, W / 2.0, 185, 14, Y_, "center", Color.Black, 3); }
        }
        static void drawControls(double y)
        {
            bool duo = M.np > 1;
            panel(140, y, 680, duo ? 112 : 96, 0.75, Color.Empty);
            txt("CONTROLES", W / 2.0, y + 16, 16, Y_);
            if (!duo)
            {
                txt("Mover: WASD o flechas   -   TIRAR: ESPACIO (mantén para más fuerza)   -   PASAR: E o X", W / 2.0, y + 42, 14, W_, "center", Color.Black, 3);
                txt("Sin la pelota:  ESPACIO = barrida   -   E = cambiar de jugador", W / 2.0, y + 62, 14, W_, "center", Color.Black, 3);
                txt("SÚPER TIRO: C cuando la barra esté llena", W / 2.0, y + 82, 14, C("#7aff7a"), "center", Color.Black, 3);
            }
            else
            {
                txt("J1:  WASD mover  -  ESPACIO tirar / barrida  -  E pasar / cambiar  -  R súper", W / 2.0, y + 44, 14, P_COL[0], "center", Color.Black, 3);
                txt("J2:  Flechas mover  -  SHIFT der. tirar / barrida  -  ENTER pasar / cambiar  -  L súper", W / 2.0, y + 68, 14, P_COL[1], "center", Color.Black, 3);
                txt("Mantén el botón de tiro para patear más fuerte", W / 2.0, y + 92, 13, W_, "center", Color.Black, 3);
            }
        }
    }
}
