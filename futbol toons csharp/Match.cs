using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FutbolToons
{
    // ======================= EL PARTIDO =======================
    // 5 contra 5 (4 jugadores + arquero). El equipo 0 ataca hacia la derecha (+x) y el equipo 1 hacia la izquierda.
    public class MPl
    {
        public Ch ch; public int team, slot, human = -1, goals, num; public bool gk;
        public double x, y, vx, vy, face, hx, hy, top, acc, wgt;
        public double stun, slide, slideCD, kickCD, kickAnim, run, charge = -1, pwT, dive, frozen, aiT, cheer;
    }
    public class MBall
    {
        public double x, y, z, vx, vy, vz, spin, superT, saveCD, zig, teleT; public MPl owner, last, passTo;
        public string sup = ""; public Color supCol; public List<double[]> trail = new List<double[]>(); public double shotX;
    }
    public class MatchState
    {
        public Team[] team = new Team[2]; public Kit[] kit = new Kit[2]; public Color[] gkCol = new Color[2];
        public List<MPl> pl = new List<MPl>(); public MBall ball = new MBall();
        public int[] score = { 0, 0 }, shots = { 0, 0 }; public double[] meter = { 0, 0 }, poss = { 0, 0 };
        public MPl[] ctl = new MPl[2], chaser = new MPl[2]; public double[] switchT = { 0, 0 };
        public double clock, dur, phaseT, camX, msgT, msgMax = 1, shake, flash, resT;
        public string phase = "count", msg = "", sub = ""; public Color msgCol;
        public int kickTeam, firstKick, np, diff, half = 1, lastGoalTeam = -1; public bool knockout, allAI, demo;
        public Stadium st; public string compName = "", stage = "";
        public List<string[]> log = new List<string[]>();   // goles: equipo, nombre, minuto
        // penales
        public int[] penGoals = { 0, 0 }, penTaken = { 0, 0 }; public List<int>[] penLog = { new List<int>(), new List<int>() };
        public int penTeam, penWinner = -1; public string penStep = ""; public double penT, penAimY, penDiveY; public bool penGoal, penMiss; public MPl penShooter, penKeeper;
        public int Winner { get { return score[0] > score[1] ? 0 : score[1] > score[0] ? 1 : penWinner; } }
    }

    static partial class G
    {
        public static MatchState M;
        const double FL = 900, FW = 470, GW = 125, GH = 110, GD = 75, PR = 20, BR = 13;    // medidas de la cancha
        const double CAMD = 1100, CAMZ = 1000, FOC = 902, HZ = -315;                     // cámara de televisión
        static int sgn(int team) { return team == 0 ? 1 : -1; }
        static double hyp(double x, double y) { return Math.Sqrt(x * x + y * y); }
        static bool humanTeam(int t) { return M != null && !M.allAI && t < M.np; }
        static Ctrl ctrlOf(int t) { return M.np > 1 ? CTRL[t] : CTRL_SOLO; }

        // ---------------- armar el partido ----------------
        static double colDist(Color a, Color b) { return Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B); }
        public static MatchState NewMatch(Team a, Team b, int np, bool knockout, Stadium st, double durMin, int diff)
        {
            var s = new MatchState { np = np, knockout = knockout, st = st, dur = durMin * 60, diff = diff };
            s.team[0] = a; s.team[1] = b;
            s.kit[0] = a.home; s.kit[1] = colDist(a.home.shirt, b.home.shirt) < 150 ? b.away : b.home;
            if (colDist(s.kit[0].shirt, s.kit[1].shirt) < 120) s.kit[1] = colDist(a.home.shirt, b.home.shirt) < 150 ? b.home : b.away;
            s.gkCol[0] = colDist(s.kit[0].shirt, C("#2ac84a")) < 150 || colDist(s.kit[1].shirt, C("#2ac84a")) < 150 ? C("#8a2ac8") : C("#2ac84a");
            s.gkCol[1] = colDist(s.kit[1].shirt, C("#ff8a1a")) < 150 || colDist(s.kit[0].shirt, C("#ff8a1a")) < 150 ? C("#1a1a1a") : C("#ff8a1a");
            for (int t = 0; t < 2; t++)
            {
                var sq = s.team[t].squad;
                for (int i = 0; i < 5; i++)
                {
                    var c = sq[i]; bool gk = i == 4;
                    var p = new MPl { ch = c, team = t, slot = i, gk = gk, num = gk ? 1 : new[] { 9, 7, 11, 4 }[i] };
                    p.top = 285 + 20 * c.spd; p.acc = 5 + 0.9 * c.spd; p.wgt = c.big ? 5 : c.tall < 0.8 ? 2 : 3;
                    s.pl.Add(p);
                }
            }
            s.kickTeam = s.firstKick = U.R() < 0.5 ? 0 : 1;
            M = s; kickoff(); s.phase = "count"; s.phaseT = 3.5;
            return s;
        }
        static void home(MPl p)
        {
            int s = sgn(p.team);
            if (p.gk) { p.hx = -s * (FL - 30); p.hy = 0; return; }
            switch (p.slot)
            {
                case 0: p.hx = -s * 170; p.hy = 0; break;
                case 1: p.hx = -s * 320; p.hy = -240; break;
                case 2: p.hx = -s * 320; p.hy = 240; break;
                default: p.hx = -s * 560; p.hy = 0; break;
            }
        }
        static void setCtl(int t, MPl p)
        {
            if (p == null || M.ctl[t] == p || p.gk) return;
            if (M.ctl[t] != null) M.ctl[t].human = -1;
            M.ctl[t] = p; p.human = t; p.charge = -1; M.switchT[t] = 0.5;
        }
        static void kickoff()
        {
            var s = M; var b = s.ball;
            foreach (var p in s.pl)
            {
                home(p); p.x = p.hx; p.y = p.hy; p.vx = p.vy = 0; p.face = p.team == 0 ? 0 : Math.PI;
                p.stun = p.slide = p.slideCD = p.kickCD = p.kickAnim = p.pwT = p.dive = p.frozen = 0; p.charge = -1;
            }
            var k = s.pl.First(p => p.team == s.kickTeam && p.slot == 0);
            k.x = -sgn(s.kickTeam) * 34; k.y = 0;
            b.x = 0; b.y = 0; b.z = 0; b.vx = b.vy = b.vz = 0; b.owner = k; b.last = k; b.passTo = null; b.superT = 0; b.sup = ""; b.trail.Clear();
            for (int t = 0; t < 2; t++) { if (s.ctl[t] != null) s.ctl[t].human = -1; s.ctl[t] = null; }
            for (int t = 0; t < s.np; t++) setCtl(t, s.pl.First(p => p.team == t && p.slot == 0));
            s.camX = 0;
        }
        static void say(string m, Color c, double t) { M.msg = m; M.msgCol = c; M.msgT = t; M.msgMax = t; }
        static string tname(int t) { return M.team[t].name.ToUpper(); }

        // ---------------- sonidos ----------------
        static void sKick(double pw) { if (M.demo) return; Snd.Tone(170 + pw * 90, 0.09, "square", 0.12, 70); Snd.Noise(0.07, 0.18, 0, 700); }
        static void sWhistle(bool longW = false)
        {
            if (M.demo) return;
            Snd.Tone(2300, 0.18, "triangle", 0.08, 2150);
            if (longW) { Snd.Tone(2300, 0.18, "triangle", 0.08, 2150, 0.25); Snd.Tone(2300, 0.6, "triangle", 0.08, 2100, 0.5); }
        }
        static void sCrowd(double v) { if (!M.demo) Snd.Noise(1.6, v, 0, 1500); }

        // ---------------- un cuadro del partido ----------------
        public static void UpdateMatch(double dt)
        {
            var s = M; var b = s.ball;
            s.msgT = Math.Max(0, s.msgT - dt); s.shake = Math.Max(0, s.shake - dt); s.flash = Math.Max(0, s.flash - dt);
            for (int t = 0; t < 2; t++) s.switchT[t] = Math.Max(0, s.switchT[t] - dt);
            switch (s.phase)
            {
                case "count":
                    {
                        double prev = s.phaseT; s.phaseT -= dt;
                        if (!s.demo) foreach (int n in new[] { 3, 2, 1 }) if (prev > n && s.phaseT <= n) SFX.count();
                        if (s.phaseT <= 0) { s.phase = "play"; sWhistle(); if (!s.demo) say("¡A JUGAR!", Y_, 1.2); }
                        ballFollow(); camera(dt); return;
                    }
                case "ready":
                    s.phaseT -= dt; if (s.phaseT <= 0) { s.phase = "play"; sWhistle(); }
                    ballFollow(); camera(dt); return;
                case "goal":
                    s.phaseT -= dt;
                    foreach (var p in s.pl) move(p, 0, 0, dt);
                    ballPhys(dt); camera(dt);
                    if (!s.demo && U.R() < 0.5) addPart(U.R() * W, -10, (U.R() - 0.5) * 80, 80 + U.R() * 120, 3, U.Hsl(U.R() * 360, 0.9, 0.6), 7, 40);
                    if (s.phaseT <= 0) { kickoff(); s.phase = "ready"; s.phaseT = 1.0; }
                    return;
                case "half":
                    s.phaseT -= dt;
                    if (s.phaseT <= 0) { s.half = 2; s.kickTeam = 1 - s.firstKick; kickoff(); s.phase = "ready"; s.phaseT = 1.2; if (!s.demo) say("SEGUNDO TIEMPO", W_, 1.5); }
                    return;
                case "pens": updatePens(dt); camera(dt); return;
                case "end":
                    s.phaseT -= dt;
                    foreach (var p in s.pl) move(p, 0, 0, dt);
                    ballPhys(dt); camera(dt);
                    return;
            }
            // ---- jugando ----
            s.clock += dt;
            if (b.owner != null) s.poss[b.owner.team] += dt;
            for (int t = 0; t < 2; t++) s.meter[t] = Math.Min(1, s.meter[t] + dt * 0.024);
            if (s.half == 1 && s.clock >= s.dur / 2) { s.phase = "half"; s.phaseT = 3.2; sWhistle(true); if (!s.demo) say("MEDIO TIEMPO", W_, 3); return; }
            if (s.clock >= s.dur)
            {
                if (s.score[0] == s.score[1] && s.knockout) { startPens(); return; }
                endMatch(); return;
            }
            // quién persigue la pelota en cada equipo
            for (int t = 0; t < 2; t++)
            {
                MPl best = null; double bd = 1e9;
                foreach (var p in s.pl)
                {
                    if (p.team != t || p.gk || p.stun > 0 || p.frozen > 0) continue;
                    double d = hyp(p.x - b.x, p.y - b.y) - (p == s.chaser[t] ? 40 : 0);
                    if (b.passTo == p) d -= 400;
                    if (d < bd) { bd = d; best = p; }
                }
                s.chaser[t] = best;
                // el jugador humano maneja al que tiene la pelota o al más cercano
                if (humanTeam(t))
                {
                    if (b.owner != null && b.owner.team == t && !b.owner.gk) setCtl(t, b.owner);
                    else if (best != null && s.ctl[t] != best && s.switchT[t] <= 0 && s.ctl[t].slide <= 0 && b.passTo == null)
                    {
                        double dc = hyp(s.ctl[t].x - b.x, s.ctl[t].y - b.y), dn = hyp(best.x - b.x, best.y - b.y);
                        if (dn + 90 < dc) setCtl(t, best);
                    }
                }
            }
            foreach (var p in s.pl) updatePl(p, dt);
            collisions(dt);
            ballPhys(dt);
            touches();
            camera(dt);
        }
        static void ballFollow()
        {
            var b = M.ball; if (b.owner == null) return;
            var p = b.owner; b.x = p.x + Math.Cos(p.face) * (PR + BR + 2); b.y = p.y + Math.Sin(p.face) * (PR + BR + 2); b.z = 0;
        }
        static double camLim() { return FL - (W / 2.0) / (FOC / CAMD) + 150; }
        static void camera(double dt)
        {
            var s = M; double lim = camLim(), tx;
            if (s.phase == "pens") tx = sgn(s.penTeam) * lim;
            else tx = U.Clamp(s.ball.x + s.ball.vx * 0.15, -lim, lim);
            s.camX += (tx - s.camX) * Math.Min(1, dt * 3.5);
        }
        static void endMatch()
        {
            var s = M; s.phase = "end"; s.phaseT = 3.2; sWhistle(true);
            if (!s.demo) say("¡FINAL DEL PARTIDO!", W_, 3.2);
        }

        // ---------------- jugadores ----------------
        static void updatePl(MPl p, double dt)
        {
            var s = M; var b = s.ball;
            p.kickCD -= dt; p.slideCD -= dt; p.kickAnim = Math.Max(0, p.kickAnim - dt); p.pwT = Math.Max(0, p.pwT - dt); p.dive = Math.Max(0, p.dive - dt);
            if (p.frozen > 0) { p.frozen -= dt; move(p, 0, 0, dt); return; }
            if (p.stun > 0) { p.stun -= dt; move(p, 0, 0, dt); return; }
            if (p.slide > 0)
            {
                p.slide -= dt; double f = Math.Exp(-2.2 * dt); p.vx *= f; p.vy *= f; move(p, 0, 0, dt, true);
                foreach (var q in s.pl)
                {
                    if (q.team == p.team || q.stun > 0 || hyp(q.x - p.x, q.y - p.y) > PR * 2 + 6) continue;
                    if (q.gk && b.owner != q) continue;
                    if (b.owner == q)
                    {
                        q.stun = 0.8 + 0.1 * p.wgt; b.owner = null; double a = Math.Atan2(p.vy, p.vx);
                        b.vx = Math.Cos(a) * 260 + (U.R() - 0.5) * 120; b.vy = Math.Sin(a) * 260 + (U.R() - 0.5) * 120; b.vz = 120; b.last = q;
                        s.meter[p.team] = Math.Min(1, s.meter[p.team] + 0.1); if (!s.demo) SFX.bump(); dust(q, 8);
                    }
                    else if (!q.gk) { q.stun = 0.5; if (!s.demo) SFX.bump(); }
                }
                return;
            }
            double dx = 0, dy = 0;
            if (p.human >= 0 && !s.allAI && !p.gk)
            {
                var c = ctrlOf(p.human);
                dx = (held(c.right) ? 1 : 0) - (held(c.left) ? 1 : 0); dy = (held(c.down) ? 1 : 0) - (held(c.up) ? 1 : 0);
                double m = hyp(dx, dy); if (m > 0) { dx /= m; dy /= m; }
                bool has = b.owner == p, shootHeld = held(c.shoot), shootPress = kp(c.shoot), passPress = kp(c.pass);
                if (kp(c.power) && s.meter[p.team] >= 1) superShot(p);
                else if (has)
                {
                    if (shootPress) p.charge = 0;
                    if (p.charge >= 0)
                    {
                        if (shootHeld) { p.charge += dt / 0.7; if (p.charge >= 1.3) shoot(p, 1, dy); }
                        else shoot(p, U.Clamp(p.charge, 0.3, 1), dy);
                    }
                    if (passPress && b.owner == p) pass(p, dx, dy);
                }
                else
                {
                    p.charge = -1;
                    if (shootPress) slide(p, dx, dy);
                    else if (passPress) switchPl(p.human);
                }
            }
            else ai(p, dt, out dx, out dy);
            move(p, dx, dy, dt);
        }
        static void move(MPl p, double dx, double dy, double dt, bool free = false)
        {
            var s = M;
            if (!free)
            {
                double top = p.top * (s.ball.owner == p ? 0.88 : 1) * (p.pwT > 0 ? 1.4 : 1) * (p.dive > 0 ? 1.7 : 1);
                if (p.stun > 0 || p.frozen > 0) { dx = dy = 0; }
                double rate = p.acc * (s.st.ice ? 0.3 : 1);
                if (s.phase != "play") rate = 6;
                double k = Math.Min(1, rate * dt);
                p.vx += (dx * top - p.vx) * k; p.vy += (dy * top - p.vy) * k;
            }
            p.x += p.vx * dt; p.y += p.vy * dt;
            double sp = hyp(p.vx, p.vy);
            if (sp > 25 && p.slide <= 0 && p.stun <= 0)
            {
                double d = Math.Atan2(p.vy, p.vx) - p.face;
                while (d > Math.PI) d -= 2 * Math.PI; while (d < -Math.PI) d += 2 * Math.PI;
                p.face += d * Math.Min(1, dt * 12);
            }
            p.run += sp * dt * 0.045;
            if (p.gk)
            {
                int sg = sgn(p.team); double line = -sg * (FL - PR);
                p.x = sg > 0 ? U.Clamp(p.x, line, line + 250) : U.Clamp(p.x, line - 250, line);
                p.y = U.Clamp(p.y, -300, 300);
            }
            else { p.x = U.Clamp(p.x, -FL + PR, FL - PR); p.y = U.Clamp(p.y, -FW + PR, FW - PR); }
        }
        static void collisions(double dt)
        {
            var s = M; var b = s.ball;
            for (int i = 0; i < s.pl.Count; i++)
                for (int j = i + 1; j < s.pl.Count; j++)
                {
                    var a = s.pl[i]; var c = s.pl[j];
                    double dx = c.x - a.x, dy = c.y - a.y, d = hyp(dx, dy);
                    if (d >= PR * 2 || d < 0.01) continue;
                    double push = PR * 2 - d, wa = c.wgt / (a.wgt + c.wgt);
                    a.x -= dx / d * push * wa; a.y -= dy / d * push * wa; c.x += dx / d * push * (1 - wa); c.y += dy / d * push * (1 - wa);
                }
            // robar la pelota con el cuerpo
            var o = b.owner;
            if (o != null && !o.gk && s.phase == "play")
                foreach (var q in s.pl)
                {
                    if (q.team == o.team || q.stun > 0 || q.frozen > 0 || q.kickCD > 0 || (q.gk && Math.Abs(q.x) < FL - 250)) continue;
                    if (hyp(q.x - b.x, q.y - b.y) > PR + BR + 4) continue;
                    if (q.pwT > 0) { o.stun = 1; take(q); if (!s.demo) SFX.hit(); break; }
                    if (o.pwT > 0) { q.stun = 0.8; continue; }
                    double rate = (q.human >= 0 ? 3.2 : 2.2 * (humanTeam(q.team) ? 1 : new[] { 0.6, 1, 1.35 }[s.diff])) + 0.25 * (q.ch.tec - o.ch.tec) + 0.15 * (q.wgt - o.wgt);
                    if (U.R() < dt * Math.Max(0.5, rate)) { o.kickCD = 0.45; take(q); if (!s.demo) SFX.bump(); break; }
                }
            // súper carrera: tumba a los rivales que toca
            foreach (var p in s.pl)
            {
                if (p.pwT <= 0) continue;
                foreach (var q in s.pl) if (q.team != p.team && !q.gk && q.stun <= 0 && hyp(q.x - p.x, q.y - p.y) < PR * 2 + 4) { q.stun = 1; if (b.owner == q) take(p); }
            }
        }
        static void take(MPl p)
        {
            var s = M; var b = s.ball;
            if (b.passTo == p && b.last != null && b.last.team == p.team) s.meter[p.team] = Math.Min(1, s.meter[p.team] + 0.05);
            b.owner = p; b.vx = b.vy = b.vz = 0; b.z = 0; b.passTo = null; b.superT = 0; b.sup = ""; b.last = p; p.charge = -1;
            p.aiT = p.gk ? 0.7 + U.R() * 0.4 : 0.25 + U.R() * 0.9;
            if (humanTeam(p.team) && !p.gk) setCtl(p.team, p);
        }
        static void switchPl(int t)
        {
            var s = M; var b = s.ball; MPl best = null; double bd = 1e9;
            foreach (var p in s.pl) { if (p.team != t || p.gk || p == s.ctl[t]) continue; double d = hyp(p.x - b.x, p.y - b.y); if (d < bd) { bd = d; best = p; } }
            if (best != null) { setCtl(t, best); s.switchT[t] = 1.2; SFX.sel(); }
        }
        static void slide(MPl p, double dx, double dy)
        {
            if (p.slideCD > 0 || p.gk) return;
            double a = (dx != 0 || dy != 0) ? Math.Atan2(dy, dx) : p.face;
            p.face = a; p.slide = 0.42; p.slideCD = 1.0;
            double sp = Math.Max(hyp(p.vx, p.vy), p.top) * 1.45; p.vx = Math.Cos(a) * sp; p.vy = Math.Sin(a) * sp;
            if (!M.demo) SFX.hop(); dust(p, 6);
        }
        static void dust(MPl p, int n)
        {
            if (M.demo) return;
            double k = socK(p.y), x = SX(p.x, p.y), y = SY(p.y, 0); Color c = M.st.outside;
            for (int i = 0; i < n; i++) addPart(x + (U.R() - 0.5) * 30 * k, y, (U.R() - 0.5) * 160, -60 - U.R() * 120, 0.45, c, 5 * k + 2, 500);
        }

        // ---------------- tiros, pases y súper tiro ----------------
        static void shoot(MPl p, double pow, double aimDy)
        {
            var s = M; var b = s.ball; p.charge = -1; if (b.owner != p) return;
            int sg = sgn(p.team); double gx = sg * FL, fx = Math.Cos(p.face), fy = Math.Sin(p.face), ang;
            if (fx * sg > 0.1 && Math.Abs(gx - p.x) < 1100)
            {
                double ty = aimDy != 0 ? aimDy * GW * 0.72 : U.Clamp(p.y + fy / Math.Abs(fx) * Math.Abs(gx - p.x) * 0.5, -GW * 0.7, GW * 0.7);
                double noise = (p.human >= 0 && !s.allAI ? new[] { 0.12, 0.2, 0.26 }[s.diff] : humanTeam(p.team) ? 0.3 : new[] { 0.65, 0.4, 0.22 }[s.diff]) * (1.2 - p.ch.tec * 0.08);
                ty += (U.R() - 0.5) * 2 * GW * noise;
                ang = Math.Atan2(ty - p.y, gx - p.x);
            }
            else ang = p.face;
            double sp = 600 + 650 * pow + 35 * p.ch.sht;
            b.owner = null; b.vx = Math.Cos(ang) * sp; b.vy = Math.Sin(ang) * sp; b.vz = 90 + 230 * pow; b.z = 6;
            b.last = p; b.passTo = null; b.shotX = p.x; p.kickCD = 0.3; p.kickAnim = 0.25; p.face = ang;
            if (Math.Cos(ang) * sg > 0.3) s.shots[p.team]++;
            sKick(pow); dust(p, 4);
        }
        static void pass(MPl p, double dx, double dy, bool far = false)
        {
            var s = M; var b = s.ball; if (b.owner != p) return;
            int sg = sgn(p.team); double rx = dx, ry = dy;
            if (rx == 0 && ry == 0) { rx = Math.Cos(p.face); ry = Math.Sin(p.face); }
            double m = hyp(rx, ry); rx /= m; ry /= m;
            MPl best = null; double bs = -99;
            foreach (var t in s.pl)
            {
                if (t == p || t.team != p.team || t.gk || t.stun > 0) continue;
                double vx = t.x - p.x, vy = t.y - p.y, d = hyp(vx, vy); if (d < 1) continue;
                double sc = far ? t.x * sg / FL - d / 4000 : (vx * rx + vy * ry) / d - d / 1700;
                foreach (var q in s.pl) if (q.team != p.team && !q.gk) { double u = ((q.x - p.x) * vx + (q.y - p.y) * vy) / (d * d); if (u > 0.1 && u < 0.9 && hyp(p.x + vx * u - q.x, p.y + vy * u - q.y) < 40) sc -= 0.5; }
                if (sc > bs) { bs = sc; best = t; }
            }
            if (best == null) { shoot(p, 0.3, 0); return; }
            double lx = best.x + best.vx * 0.35, ly = best.y + best.vy * 0.35, ddx = lx - p.x, ddy = ly - p.y, dd = Math.Max(1, hyp(ddx, ddy));
            double err = (5 - p.ch.tec) * 0.035 * (U.R() - 0.5), ang = Math.Atan2(ddy, ddx) + err;
            double spd = U.Clamp(dd * 1.5 + 220, 420, 1000);
            b.owner = null; b.vx = Math.Cos(ang) * spd; b.vy = Math.Sin(ang) * spd; b.vz = dd > 480 ? 260 : 30; b.z = 4;
            b.last = p; b.passTo = best; p.kickCD = 0.3; p.kickAnim = 0.2; p.face = ang;
            if (humanTeam(p.team)) setCtl(p.team, best);
            sKick(0.2);
        }
        static void superShot(MPl p)
        {
            var s = M; var b = s.ball; if (s.meter[p.team] < 1) return;
            s.meter[p.team] = 0;
            string ty = p.ch.pw; Color col = PW_COL.ContainsKey(ty) ? PW_COL[ty] : Y_;
            if (!s.demo) { SFX.power(); say("¡" + p.ch.pwName.ToUpper() + "!", col, 1.6); s.flash = 0.2; s.shake = 0.3; }
            if (b.owner == p)
            {
                int sg = sgn(p.team); double gx = sg * FL, ty2 = (U.R() < 0.5 ? -1 : 1) * GW * (0.35 + U.R() * 0.3);
                double ang = Math.Atan2(ty2 - p.y, gx - p.x), sp = ty == "laser" || ty == "kame" ? 2100 : ty == "bubble" ? 1150 : 1500;
                b.owner = null; b.vx = Math.Cos(ang) * sp; b.vy = Math.Sin(ang) * sp; b.vz = ty == "lob" ? 420 : ty == "bubble" ? 300 : 70; b.z = 8;
                b.last = p; b.passTo = null; b.shotX = p.x; b.superT = 2.5; b.sup = ty; b.supCol = col; b.zig = 0; b.teleT = ty == "tele" ? 0.22 : 0;
                p.kickCD = 0.4; p.kickAnim = 0.3; p.face = ang; sKick(1); s.shots[p.team]++;
                if (ty == "freeze") foreach (var q in s.pl) if (q.team != p.team && !q.gk) q.frozen = 2;
                if (ty == "shock") foreach (var q in s.pl) if (q.team != p.team && !q.gk && hyp(q.x - p.x, q.y - p.y) < 320) q.stun = 1.3;
                if (ty == "ssj") p.pwT = 2.5;
            }
            else p.pwT = 3.5;   // sin la pelota: ¡súper carrera!
        }
        static double superSave(string ty) { return ty == "ghost" ? 0.1 : ty == "giant" || ty == "laser" || ty == "kame" ? 0.15 : ty == "tele" || ty == "electric" ? 0.2 : 0.25; }
        static double gkSkill(int t) { return humanTeam(t) ? 0.72 : new[] { 0.5, 0.68, 0.82 }[M.diff]; }

        // ---------------- la pelota ----------------
        static void ballPhys(double dt)
        {
            var s = M; var b = s.ball;
            b.saveCD = Math.Max(0, b.saveCD - dt);
            if (b.owner != null)
            {
                var p = b.owner; double r = PR + BR + 2;
                b.x = p.x + Math.Cos(p.face) * r; b.y = p.y + Math.Sin(p.face) * r; b.z = Math.Abs(Math.Sin(p.run * 2)) * 4;
                b.y = U.Clamp(b.y, -FW + BR, FW - BR); if (Math.Abs(b.y) > GW - BR) b.x = U.Clamp(b.x, -FL + BR, FL - BR);
                b.vx = p.vx; b.vy = p.vy; b.spin += hyp(p.vx, p.vy) * dt * 0.03; b.trail.Clear();
                return;
            }
            if (b.superT > 0)
            {
                b.superT -= dt;
                b.trail.Add(new[] { b.x, b.y, b.z }); if (b.trail.Count > 14) b.trail.RemoveAt(0);
                if (b.sup == "spin" || b.sup == "electric") { b.zig += dt * (b.sup == "electric" ? 22 : 14); b.y += Math.Cos(b.zig) * dt * (b.sup == "electric" ? 520 : 420); }
                if (b.sup == "bubble") b.vy += Math.Sin(b.superT * 6) * 900 * dt;
                if (b.teleT > 0) { b.teleT -= dt; if (b.teleT <= 0) { double nx = U.Clamp(b.x + b.vx * 0.22, -FL + 90, FL - 90); if (!s.demo) for (int i = 0; i < 10; i++) addPart(SX(b.x, b.y), SY(b.y, b.z), (U.R() - 0.5) * 300, (U.R() - 0.5) * 300, 0.5, b.supCol, 6, 0); b.x = nx; b.trail.Clear(); } }
                if (hyp(b.vx, b.vy) < 500) b.superT = 0;
            }
            else if (b.trail.Count > 0) b.trail.RemoveAt(0);
            double oldAx = Math.Abs(b.x);
            b.x += b.vx * dt; b.y += b.vy * dt; b.z += b.vz * dt;
            double grav = b.sup == "bubble" && b.superT > 0 ? 500 : 1100;
            if (b.z > 0 || b.vz > 0) b.vz -= grav * dt;
            if (b.z <= 0) { b.z = 0; if (b.vz < -140) { b.vz = -b.vz * 0.5; } else b.vz = 0; }
            double fr = b.z <= 0.5 ? (s.st.ice ? 0.5 : 1.1) : 0.15, f = Math.Exp(-fr * dt);
            b.vx *= f; b.vy *= f; b.spin += hyp(b.vx, b.vy) * dt * 0.03;
            if (b.y > FW - BR) { b.y = FW - BR; b.vy = -Math.Abs(b.vy) * 0.7; }
            if (b.y < -FW + BR) { b.y = -FW + BR; b.vy = Math.Abs(b.vy) * 0.7; }
            double ax = Math.Abs(b.x); int side = b.x > 0 ? 1 : -1;
            if (ax > FL && oldAx <= FL && (Math.Abs(b.y) > GW - 4 || b.z > GH - 4))
            {
                b.x = side * (FL - BR); b.vx = -b.vx * 0.7;
                if (s.phase == "play" && b.last != null && Math.Abs(b.y) < GW + 30 && !s.demo) say("¡AL PALO!", W_, 1);
            }
            else if (ax > FL)
            {
                if (ax > FL + GD - BR) { b.x = side * (FL + GD - BR); b.vx = -b.vx * 0.15; b.vy *= 0.5; }
                if (Math.Abs(b.y) > GW - BR) { b.y = Math.Sign(b.y) * (GW - BR); b.vy = -b.vy * 0.3; }
                if (b.z > GH - BR) { b.z = GH - BR; b.vz = -Math.Abs(b.vz) * 0.3; }
                if (ax > FL + BR && s.phase == "play") goal(side > 0 ? 0 : 1);
            }
            else if (ax > FL - BR)
            {
                bool mouth = Math.Abs(b.y) < GW - 4 && b.z < GH - 4;
                if (!mouth) { b.x = side * (FL - BR); b.vx = -b.vx * 0.7; if (b.z > GH - 8 && b.z < GH + 10 && Math.Abs(b.y) < GW) { b.vz = Math.Abs(b.vz) * 0.5 + 80; if (!s.demo) { SFX.bump(); say("¡AL TRAVESAÑO!", W_, 1); } } }
            }
            foreach (int px in new[] { -1, 1 })
                foreach (int py in new[] { -1, 1 })
                {
                    double dx = b.x - px * FL, dy = b.y - py * GW, d = hyp(dx, dy);
                    if (d < BR + 5 && d > 0.01 && b.z < GH)
                    {
                        double nx = dx / d, ny = dy / d, dot = b.vx * nx + b.vy * ny;
                        if (dot < 0) { b.vx -= 1.7 * dot * nx; b.vy -= 1.7 * dot * ny; if (!s.demo) { SFX.bump(); if (s.phase == "play") say("¡AL PALO!", W_, 1); } }
                        b.x = px * FL + nx * (BR + 5); b.y = py * GW + ny * (BR + 5);
                    }
                }
        }
        static void touches()
        {
            var s = M; var b = s.ball; if (b.owner != null || s.phase != "play") return;
            double sp = hyp(b.vx, b.vy), big = b.sup == "giant" && b.superT > 0 ? BR : 0;
            foreach (var p in s.pl)
            {
                if (p.stun > 0 || p.frozen > 0 || p.kickCD > 0) continue;
                double reach = PR + BR + big + (p.gk ? 16 + (p.dive > 0 ? 14 : 0) : 6);
                if (b.z > (p.gk ? 115 : 40) || hyp(b.x - p.x, b.y - p.y) > reach) continue;
                bool super = b.superT > 0 && b.last != null && b.last.team != p.team;
                if (p.gk)
                {
                    if (b.saveCD > 0) continue;
                    bool shot = b.last != null && b.last.team != p.team && sp > 450;
                    if (shot)
                    {
                        double chance = super ? superSave(b.sup) : U.Clamp(gkSkill(p.team) + (1 - sp / 1500) * 0.35, 0.12, 0.96);
                        if (U.R() > chance) { b.saveCD = 0.5; if (super) { p.stun = 1.2; if (!s.demo) SFX.hit(); } continue; }
                        if (super || sp > 1000 || U.R() < 0.4)
                        {
                            b.vx = -b.vx * 0.35; b.vy = b.vy * 0.4 + (U.R() - 0.5) * 500; b.vz = 200 + U.R() * 150; b.superT = 0; b.sup = "";
                            b.saveCD = 0.35; b.last = p; b.passTo = null; p.kickCD = 0.3; if (!s.demo) { SFX.bump(); say(U.R() < 0.5 ? "¡QUÉ ATAJADA!" : "¡LA SACÓ!", W_, 1); }
                            continue;
                        }
                        if (!s.demo) say("¡ATAJADA!", W_, 1);
                    }
                    take(p); continue;
                }
                if (super) { p.stun = 1.1; p.vx = b.vx * 0.3; p.vy = b.vy * 0.3; b.vx *= 0.85; b.vy *= 0.85; if (!s.demo) SFX.hit(); continue; }
                take(p);
                return;
            }
        }
        static void goal(int team)
        {
            var s = M; var b = s.ball;
            s.score[team]++; s.phase = "goal"; s.phaseT = 3.2; s.kickTeam = 1 - team; s.lastGoalTeam = team;
            var sc = b.last; bool own = sc != null && sc.team != team;
            if (sc != null && !own) sc.goals++;
            int min = (int)Math.Floor(s.clock / s.dur * 90) + 1;
            s.log.Add(new[] { team.ToString(), (sc != null ? sc.ch.name : "?") + (own ? " (en contra)" : ""), Math.Min(90, min) + "'" });
            s.meter[1 - team] = Math.Min(1, s.meter[1 - team] + 0.25);
            if (s.demo) return;
            bool far = !own && Math.Abs((team == 0 ? FL : -FL) - b.shotX) > 600;
            say(far ? "¡GOLAZO!" : "¡GOOOOL!", team == 0 ? s.kit[0].shirt : s.kit[1].shirt, 3); s.shake = 0.4;
            SFX.finish(); sCrowd(0.22);
            for (int i = 0; i < 80; i++) addPart(U.R() * W, -10 - U.R() * 60, (U.R() - 0.5) * 120, 60 + U.R() * 160, 3.5, U.Hsl(U.R() * 360, 0.9, 0.6), 7, 40);
        }

        // ---------------- penales ----------------
        static void startPens()
        {
            var s = M; s.phase = "pens"; s.penTeam = U.R() < 0.5 ? 0 : 1; s.penStep = "setup"; s.penT = 0; sWhistle(true);
            if (!s.demo) say("¡PENALES!", Y_, 2.2);
        }
        static void penSetup()
        {
            var s = M; int t = s.penTeam, sg = sgn(t);
            var shooters = s.pl.Where(p => p.team == t && !p.gk).OrderBy(p => p.slot).ToList();
            s.penShooter = shooters[s.penTaken[t] % shooters.Count];
            s.penKeeper = s.pl.First(p => p.team == 1 - t && p.gk);
            foreach (var p in s.pl)
            {
                p.vx = p.vy = 0; p.stun = p.frozen = p.slide = p.dive = 0; p.human = -1;
                if (p.gk && p != s.penKeeper) { p.x = -sg * (FL - 30); p.y = 0; }
                else if (!p.gk && p != s.penShooter) { p.x = sg * (FL - 520) + (p.team == 0 ? -40 : 40); p.y = -120 + p.slot * 60 + p.team * 20; p.face = Math.PI / 2 * (p.team == 0 ? 1 : 1); }
            }
            s.penShooter.x = sg * (FL - 190); s.penShooter.y = 0; s.penShooter.face = sg > 0 ? 0 : Math.PI;
            s.penKeeper.x = sg * (FL - 24); s.penKeeper.y = 0; s.penKeeper.face = sg > 0 ? Math.PI : 0;
            var b = s.ball; b.owner = null; b.x = sg * (FL - 150); b.y = 0; b.z = 0; b.vx = b.vy = b.vz = 0; b.superT = 0; b.sup = ""; b.trail.Clear();
            s.penStep = "aim"; s.penT = 0; s.penDiveY = 0;
        }
        static bool penHumanShoots() { return !M.allAI && M.penTeam < M.np; }
        static bool penHumanKeeps() { return !M.allAI && 1 - M.penTeam < M.np; }
        static void updatePens(double dt)
        {
            var s = M; var b = s.ball; s.penT += dt;
            int sg = sgn(s.penTeam);
            switch (s.penStep)
            {
                case "setup": if (s.penT > (s.penTaken[0] + s.penTaken[1] == 0 ? 2.2 : 0.01)) penSetup(); return;
                case "aim":
                    {
                        s.penAimY = Math.Sin(s.penT * 2.4) * GW * 0.95;
                        if (penHumanKeeps())
                        {
                            var c = ctrlOf(1 - s.penTeam);
                            s.penDiveY = held(c.up) ? -GW * 0.65 : held(c.down) ? GW * 0.65 : 0;
                        }
                        bool fire = penHumanShoots() ? (s.penT > 0.4 && kp(ctrlOf(s.penTeam).shoot)) : s.penT > (penHumanKeeps() ? 2.2 : 1.3);
                        if (!fire) return;
                        double aim = penHumanShoots() ? s.penAimY : (U.R() < 0.1 ? (U.R() < 0.5 ? -1 : 1) * GW * 1.15 : (U.R() - 0.5) * 2 * GW * 0.85);
                        if (!penHumanKeeps())
                        {
                            double guess = new[] { 0.3, 0.42, 0.55 }[s.diff];
                            double region = U.R() < guess ? (aim < -GW * 0.3 ? -1 : aim > GW * 0.3 ? 1 : 0) : U.rng.Next(3) - 1;
                            s.penDiveY = region * GW * 0.65;
                        }
                        s.penAimY = aim;
                        s.penMiss = Math.Abs(aim) >= GW - 6;
                        s.penGoal = !s.penMiss && Math.Abs(aim - s.penDiveY) > 55;
                        double tx = sg * (FL + 40), ddx = tx - b.x, ddy = aim - b.y, d = hyp(ddx, ddy);
                        b.vx = ddx / d * 1250; b.vy = ddy / d * 1250; b.vz = 70; b.last = s.penShooter;
                        s.penShooter.kickAnim = 0.3; sKick(0.8);
                        s.penKeeper.dive = 0.5; s.penStep = "fly"; s.penT = 0;
                        return;
                    }
                case "fly":
                    {
                        var k = s.penKeeper; k.y += (s.penDiveY - k.y) * Math.Min(1, dt * 9);
                        if (!s.penGoal && !s.penMiss && s.penT > 0.11 && b.vx * sg > 0) { b.vx = -b.vx * 0.25; b.vy = (U.R() - 0.5) * 500; b.vz = 220; }
                        ballPhys(dt);
                        if (s.penT > 0.7)
                        {
                            int t = s.penTeam; s.penTaken[t]++; if (s.penGoal) s.penGoals[t]++; s.penLog[t].Add(s.penGoal ? 1 : 0);
                            if (!s.demo)
                            {
                                if (s.penGoal) { say("¡GOL!", s.kit[t].shirt, 1.3); SFX.ok(); sCrowd(0.15); }
                                else { say(s.penMiss ? "¡AFUERA!" : "¡ATAJADO!", W_, 1.3); SFX.back(); }
                            }
                            s.penStep = "result"; s.penT = 0;
                        }
                        return;
                    }
                case "result":
                    {
                        ballPhys(dt);
                        if (s.penT < 1.4) return;
                        int a = s.penGoals[0], c = s.penGoals[1], ta = s.penTaken[0], tb = s.penTaken[1];
                        bool done = false;
                        if (ta <= 5 && tb <= 5) { if (a + (5 - ta) < c || c + (5 - tb) < a) done = true; else if (ta == 5 && tb == 5 && a != c) done = true; }
                        else if (ta == tb && a != c) done = true;
                        if (done)
                        {
                            s.penWinner = a > c ? 0 : 1; s.phase = "end"; s.phaseT = 3.5; sWhistle(true);
                            if (!s.demo) { say("¡" + tname(s.penWinner) + " GANA POR PENALES!", s.kit[s.penWinner].shirt, 3.5); SFX.finish(); }
                            return;
                        }
                        s.penTeam = 1 - s.penTeam; s.penStep = "setup"; s.penT = 0;
                        return;
                    }
            }
        }

        // ---------------- inteligencia de los bots ----------------
        static void ai(MPl p, double dt, out double dx, out double dy)
        {
            var s = M; var b = s.ball; int sg = sgn(p.team);
            double tx, ty, mul = humanTeam(p.team) || s.allAI ? 0.95 : new[] { 0.82, 0.93, 1.02 }[s.diff];
            if (p.gk) gkAI(p, dt, out tx, out ty);
            else if (b.owner == p)
            {
                tx = sg * FL - sg * 60; ty = U.Clamp(p.y * 0.5, -GW * 0.6, GW * 0.6);
                MPl opp = null; double od = 1e9;
                foreach (var q in s.pl) if (q.team != p.team) { double d = hyp(q.x - p.x, q.y - p.y); if (d < od) { od = d; opp = q; } }
                if (opp != null && od < 170 && (opp.x - p.x) * sg > -20) ty = p.y + (opp.y > p.y ? -1 : 1) * 220;
                double dg = hyp(sg * FL - p.x, p.y);
                p.aiT -= dt;
                if (s.meter[p.team] >= 1 && dg < 650 && dg > 200 && !humanTeam(p.team)) superShot(p);
                else if (dg < 380 + 20 * p.ch.sht && p.aiT <= 0) shoot(p, 0.45 + U.R() * 0.55, 0);
                else if (opp != null && od < 75 && U.R() < dt * 2.2) pass(p, sg, 0);
                else if (U.R() < dt * 0.3) pass(p, sg, 0);
            }
            else
            {
                bool mine = b.owner != null && b.owner.team == p.team;
                if (!mine && s.chaser[p.team] == p)
                {
                    double lead = b.owner == null ? 0.25 : 0.1; tx = b.x + b.vx * lead; ty = b.y + b.vy * lead;
                    if (b.owner != null && hyp(b.owner.x - p.x, b.owner.y - p.y) < 85 && p.slideCD <= 0)
                    {
                        double rate = humanTeam(p.team) || s.allAI ? 1.6 : new[] { 0.6, 1.6, 2.6 }[s.diff];
                        if (U.R() < dt * rate) slide(p, b.owner.x - p.x, b.owner.y - p.y);
                    }
                    if (s.meter[p.team] >= 1 && b.owner != null && !humanTeam(p.team) && U.R() < dt * 0.4) superShot(p);
                }
                else if (mine && p.slot != 3)
                {
                    tx = U.Clamp(b.x + sg * (190 + p.slot * 50), -FL + 120, FL - 140); ty = p.hy * 1.05;
                    if (hyp(tx - b.x, ty - b.y) < 140) ty += (p.slot == 1 ? -1 : 1) * 120;
                }
                else
                {
                    // defender: entre la pelota y el arco propio (el defensa se queda más atrás)
                    double back = p.slot == 3 ? 0.35 : 0.5;
                    tx = U.Lerp(p.hx, b.x, back); ty = U.Lerp(p.hy, b.y, p.slot == 3 ? 0.55 : 0.45);
                    tx = sg > 0 ? Math.Min(tx, b.x - (p.slot == 3 ? 200 : 90)) : Math.Max(tx, b.x + (p.slot == 3 ? 200 : 90));
                    tx = U.Clamp(tx, -FL + 60, FL - 60);
                }
            }
            double ddx = tx - p.x, ddy = ty - p.y, dd = hyp(ddx, ddy);
            if (dd < 8) { dx = dy = 0; return; }
            double m = Math.Min(1, dd / 60.0) * mul; dx = ddx / dd * m; dy = ddy / dd * m;
        }
        static void gkAI(MPl p, double dt, out double tx, out double ty)
        {
            var b = M.ball; int sg = sgn(p.team); double gx = -sg * (FL - 26);
            tx = gx; ty = U.Clamp(b.y * 0.4, -GW * 0.75, GW * 0.75);
            if (b.owner == p)
            {
                tx = p.x; ty = p.y; p.aiT -= dt; p.face = sg > 0 ? 0 : Math.PI;
                if (p.aiT <= 0) pass(p, sg, 0, true);
                return;
            }
            double depth = (b.x + sg * FL) * sg;
            if (b.owner == null && depth < 230 && Math.Abs(b.y) < 280 && hyp(b.vx, b.vy) < 500) { tx = b.x; ty = b.y; }
            else if (b.owner == null && b.vx * sg < -150)
            {
                double t = (gx - b.x) / b.vx;
                if (t > 0 && t < 0.9) { ty = U.Clamp(b.y + b.vy * t, -GW - 10, GW + 10); if (t < 0.35 && Math.Abs(ty - p.y) > 25) p.dive = 0.3; }
            }
            else if (b.owner != null && b.owner.team != p.team && depth < 430) { tx = gx + sg * 45; ty = U.Clamp(b.y * 0.6, -GW * 0.8, GW * 0.8); }
        }
    }
}
