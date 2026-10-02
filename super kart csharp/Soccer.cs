using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SuperKart
{
    // ======================= MODO FÚTBOL =======================
    // 4 contra 4 (3 jugadores + arquero) con los personajes del juego.
    // El equipo 0 (rojo) ataca hacia la derecha (+x) y el equipo 1 (azul) hacia la izquierda.
    public class SPl
    {
        public Ch ch; public St st; public int team, slot, human = -1, goals; public bool gk;
        public double x, y, vx, vy, face, hx, hy, top, acc;
        public double stun, slide, slideCD, kickCD, kickAnim, run, charge = -1, pwT, dive, frozen, aiT;
    }
    public class SBall
    {
        public double x, y, z, vx, vy, vz, spin, superT, saveCD, zig, teleT; public SPl owner, last, passTo;
        public string sup = ""; public Color supCol; public List<double[]> trail = new List<double[]>();
    }
    public class Cancha { public string name, desc; public Color g1, g2, line, outside, sky0, sky1, stand, wall, wall2; public bool ice, night, rainbow, moon, tiles, heavy; public string[] deco = new string[0]; }
    public class SocState
    {
        public List<SPl> pl = new List<SPl>(); public SBall ball = new SBall();
        public int[] score = { 0, 0 }; public double[] meter = { 0, 0 }; public SPl[] ctl = new SPl[2]; public Ch[] cap = new Ch[2];
        public SPl[] chaser = new SPl[2]; public double[] switchT = { 0, 0 };
        public double clock, dur, phaseT, camX, msgT, msgMax = 1, shake, flash, resT;
        public string phase = "count", msg = ""; public Color msgCol;
        public int field, kickTeam, np, diff, lastGoalTeam = -1; public bool golden, endPending, allAI;
        public List<string[]> log = new List<string[]>();   // goles: equipo, nombre, minuto
    }

    static partial class SK
    {
        public static SocState soc;
        const double FL = 820, FW = 440, GW = 115, GH = 105, GD = 70, PR = 20, BR = 13;   // medidas de la cancha
        const double CAMD = 1100, CAMZ = 1000, FOC = 902, HZ = -315;                      // cámara de televisión
        static readonly double[] SOC_DUR = { 2, 3, 4, 5 };
        static Color[] TEAM_COL = { U.C("#e83a3a"), U.C("#3a7ae8") };
        static string[] TEAM_NAME = { "ROJO", "AZUL" };
        static Cancha[] CANCHAS = {
            new Cancha{name="Estadio Champiñón",desc="Césped perfecto para jugar",g1=U.C("#3aa83a"),g2=U.C("#47b847"),line=Color.White,outside=U.C("#2a7a2a"),sky0=U.C("#4aa8ff"),sky1=U.C("#bfe8ff"),stand=U.C("#c83a3a"),wall=U.C("#e52521"),wall2=U.C("#ffffff")},
            new Cancha{name="Playa Koopa",desc="Arena calientita junto al mar",g1=U.C("#e8cf8a"),g2=U.C("#f0d99a"),line=U.C("#ffffff"),outside=U.C("#d8b86a"),sky0=U.C("#2ab8ff"),sky1=U.C("#aef0ff"),stand=U.C("#2a9ad8"),wall=U.C("#ff8a2a"),wall2=U.C("#2ac8a8")},
            new Cancha{name="Estadio Helado",desc="¡Cuidado, se resbala!",g1=U.C("#bfe6f5"),g2=U.C("#cdeefa"),line=U.C("#3a7ad8"),outside=U.C("#9ad0e8"),sky0=U.C("#7a9ad8"),sky1=U.C("#e8f4ff"),stand=U.C("#5a6ab8"),wall=U.C("#3a7ad8"),wall2=U.C("#ffffff"),ice=true},
            new Cancha{name="Castillo de Bowser",desc="Fútbol entre lava y piedra",g1=U.C("#6a5a6a"),g2=U.C("#766676"),line=U.C("#ffb02a"),outside=U.C("#ff5a00"),sky0=U.C("#1a0a1a"),sky1=U.C("#8a1a1a"),stand=U.C("#3a2a3a"),wall=U.C("#2a2a2a"),wall2=U.C("#ffb02a")},
            new Cancha{name="Estadio de Noche",desc="¡Partido bajo las luces del estadio!",g1=U.C("#2f8a3a"),g2=U.C("#379844"),line=U.C("#ffffff"),outside=U.C("#163d1e"),sky0=U.C("#03061a"),sky1=U.C("#1a2a5a"),stand=U.C("#1e1e38"),wall=U.C("#ffd800"),wall2=U.C("#1a1a3a"),night=true},
            new Cancha{name="Selva DK",desc="Fútbol entre palmeras y barriles",g1=U.C("#2a7a2a"),g2=U.C("#348a30"),line=U.C("#fff8d0"),outside=U.C("#1f5a1a"),sky0=U.C("#3a9a5a"),sky1=U.C("#bff0a0"),stand=U.C("#6a3a1a"),wall=U.C("#8a5a2a"),wall2=U.C("#d8a040"),deco=new[]{"jtree","palm","barrel","bush"}},
            new Cancha{name="Desierto Seco",desc="Mucho sol, cactus y pirámides",g1=U.C("#d8a860"),g2=U.C("#e2b46c"),line=U.C("#ffffff"),outside=U.C("#e8c07a"),sky0=U.C("#ffb060"),sky1=U.C("#ffe8b0"),stand=U.C("#b86a3a"),wall=U.C("#c8783a"),wall2=U.C("#ffe0a0"),deco=new[]{"cactus","pyramid","rock","cactus"}},
            new Cancha{name="Senda Arcoíris",desc="¡Una cancha de todos los colores en el espacio!",g1=U.C("#ff6ab0"),g2=U.C("#6ad0ff"),line=U.C("#ffffff"),outside=U.C("#0a0a2a"),sky0=U.C("#000010"),sky1=U.C("#2a1a5a"),stand=U.C("#1a0a3a"),wall=U.C("#ff4af0"),wall2=U.C("#4af0ff"),rainbow=true,deco=new[]{"star","ring"}},
            new Cancha{name="Estadio Lunar",desc="¡Poca gravedad: la pelota vuela!",g1=U.C("#9a9aa8"),g2=U.C("#a6a6b4"),line=U.C("#4af0ff"),outside=U.C("#5a5a68"),sky0=U.C("#000008"),sky1=U.C("#101030"),stand=U.C("#2a2a3a"),wall=U.C("#4a4a5a"),wall2=U.C("#4af0ff"),moon=true,deco=new[]{"crystal","rock"}},
            // ---- canchas de Dragon Ball ----
            new Cancha{name="Torneo de Artes Marciales",desc="¡En el ring del gran torneo!",g1=U.C("#dcd8cc"),g2=U.C("#d6d2c6"),line=U.C("#c8402a"),outside=U.C("#4ab84a"),sky0=U.C("#4aa8ff"),sky1=U.C("#bfe8ff"),stand=U.C("#d8a040"),wall=U.C("#e8e0d0"),wall2=U.C("#c8402a"),tiles=true,deco=new[]{"pillar","bush","pillar","jtree"}},
            new Cancha{name="Planeta Namek",desc="Cielo verde, agua y las esferas del dragón",g1=U.C("#5ac8a0"),g2=U.C("#66d4ac"),line=U.C("#ffffff"),outside=U.C("#3aa8c8"),sky0=U.C("#7ae87a"),sky1=U.C("#d8ffb0"),stand=U.C("#3a8a6a"),wall=U.C("#2a8a6a"),wall2=U.C("#8ae8c8"),deco=new[]{"namektree","dragonball","namektree","rock"}},
            new Cancha{name="Isla Kame House",desc="Fútbol en la playa del Maestro Roshi",g1=U.C("#f0d99a"),g2=U.C("#e8cf8a"),line=U.C("#ffffff"),outside=U.C("#2ab8e8"),sky0=U.C("#4ac8ff"),sky1=U.C("#c8f4ff"),stand=U.C("#ff8ab0"),wall=U.C("#ff6a9a"),wall2=U.C("#ffffff"),deco=new[]{"kamehouse","palm","dragonball","palm"}},
            new Cancha{name="Habitación del Tiempo",desc="¡Gravedad pesada: la pelota casi no sube!",g1=U.C("#f8f8fa"),g2=U.C("#f0f0f4"),line=U.C("#b8b8d0"),outside=U.C("#ffffff"),sky0=U.C("#ffffff"),sky1=U.C("#eef4ff"),stand=U.C("#e0e0ea"),wall=U.C("#d0d0dc"),wall2=U.C("#ffffff"),heavy=true},
            new Cancha{name="Planeta de Bills",desc="El planeta del Dios de la Destrucción",g1=U.C("#6aa85a"),g2=U.C("#74b464"),line=U.C("#ffe08a"),outside=U.C("#5a3a8a"),sky0=U.C("#3a1a6a"),sky1=U.C("#c88ae8"),stand=U.C("#4a2a6a"),wall=U.C("#8a6ac8"),wall2=U.C("#ffd23a"),deco=new[]{"pine","dragonball","rock","pine"}},
        };
        static bool isSoc() { return state == "soccer" || state == "spause" || state == "sres"; }
        static double hyp(double x, double y) { return Math.Sqrt(x * x + y * y); }
        static int sgn(int team) { return team == 0 ? 1 : -1; }
        static bool humanTeam(int t) { return soc != null && !soc.allAI && t < soc.np; }
        static Ctrl socCtrl(int t) { return soc.np > 1 ? CTRL[t] : CTRL_SOLO; }

        // ---------------- armar el partido ----------------
        static SPl mkSPl(Ch ch, int team, int slot, bool gk)
        {
            var st = ClassStats(ch);
            return new SPl { ch = ch, st = st, team = team, slot = slot, gk = gk, top = 300 + 16 * st.spd + 6 * st.acc, acc = 4 + 1.1 * st.acc };
        }
        static void startSoccer(List<Ch>[] teams = null)
        {
            var s = new SocState { field = menu.sfield, np = menu.np, diff = menu.diff, dur = SOC_DUR[menu.sdur] * 60 };
            if (teams == null)
            {
                var used = new HashSet<Ch>();
                Ch c0 = CHARS[menu.ch[0]]; used.Add(c0);
                Ch c1 = s.np > 1 ? CHARS[menu.ch[1]] : U.Shuffle(CHARS.Where(c => !used.Contains(c)))[0]; used.Add(c1);
                teams = new[] { new List<Ch> { c0 }, new List<Ch> { c1 } };
                for (int t = 0; t < 2; t++) foreach (var m in U.Shuffle(CHARS.Where(c => !used.Contains(c))).Take(3)) { used.Add(m); teams[t].Add(m); }
            }
            for (int t = 0; t < 2; t++) { s.cap[t] = teams[t][0]; for (int i = 0; i < 4; i++) s.pl.Add(mkSPl(teams[t][i], t, i, i == 3)); }
            soc = s;
            s.kickTeam = U.R() < 0.5 ? 0 : 1;
            socKickoff(); s.phase = "count"; s.phaseT = 3.5;
            parts = new List<Part>();
            state = "soccer"; Snd.PlaySong(1, 1.05);
        }
        static List<Ch>[] socTeams() { return new[] { soc.pl.Where(p => p.team == 0).OrderBy(p => p.slot).Select(p => p.ch).ToList(), soc.pl.Where(p => p.team == 1).OrderBy(p => p.slot).Select(p => p.ch).ToList() }; }
        static void socHome(SPl p)
        {
            int s = sgn(p.team);
            if (p.gk) { p.hx = -s * (FL - 30); p.hy = 0; }
            else if (p.slot == 0) { p.hx = -s * 170; p.hy = 0; }
            else { p.hx = -s * 390; p.hy = p.slot == 1 ? -210 : 210; }
        }
        static void setCtl(int t, SPl p)
        {
            if (soc.ctl[t] == p || p == null || p.gk) return;
            if (soc.ctl[t] != null) soc.ctl[t].human = -1;
            soc.ctl[t] = p; p.human = t; p.charge = -1; soc.switchT[t] = 0.5;
        }
        static void socKickoff()
        {
            var s = soc; var b = s.ball;
            foreach (var p in s.pl)
            {
                socHome(p); p.x = p.hx; p.y = p.hy; p.vx = p.vy = 0; p.face = p.team == 0 ? 0 : Math.PI;
                p.stun = p.slide = p.slideCD = p.kickCD = p.kickAnim = p.pwT = p.dive = p.frozen = 0; p.charge = -1;
            }
            var k = s.pl.First(p => p.team == s.kickTeam && p.slot == 0);
            k.x = -sgn(s.kickTeam) * 34; k.y = 0;
            b.x = 0; b.y = 0; b.z = 0; b.vx = b.vy = b.vz = 0; b.owner = k; b.last = k; b.passTo = null; b.superT = 0; b.sup = ""; b.trail.Clear();
            for (int t = 0; t < 2; t++) { if (s.ctl[t] != null) s.ctl[t].human = -1; s.ctl[t] = null; }
            for (int t = 0; t < s.np; t++) setCtl(t, s.pl.First(p => p.team == t && p.slot == 0));
            s.camX = 0;
        }
        static void showSMsg(string m, Color c, double t) { soc.msg = m; soc.msgCol = c; soc.msgT = t; soc.msgMax = t; }

        // ---------------- sonidos ----------------
        static void sKick(double pw) { Snd.Tone(170 + pw * 90, 0.09, "square", 0.12, 70); Snd.Noise(0.07, 0.18, 0, 700); }
        static void sWhistle(bool longW = false)
        {
            Snd.Tone(2300, 0.18, "triangle", 0.08, 2150);
            if (longW) { Snd.Tone(2300, 0.18, "triangle", 0.08, 2150, 0.25); Snd.Tone(2300, 0.6, "triangle", 0.08, 2100, 0.5); }
        }

        // ---------------- un cuadro del partido ----------------
        static void updateSoccer(double dt)
        {
            var s = soc; var b = s.ball;
            s.msgT = Math.Max(0, s.msgT - dt); s.shake = Math.Max(0, s.shake - dt); s.flash = Math.Max(0, s.flash - dt);
            for (int t = 0; t < 2; t++) s.switchT[t] = Math.Max(0, s.switchT[t] - dt);
            switch (s.phase)
            {
                case "count":
                    {
                        double prev = s.phaseT; s.phaseT -= dt;
                        foreach (int n in new[] { 3, 2, 1 }) if (prev > n && s.phaseT <= n) SFX.count();
                        if (s.phaseT <= 0) { s.phase = "play"; sWhistle(); showSMsg("¡A JUGAR!", Y_, 1.2); }
                        socBallFollow(); socCamera(dt); return;
                    }
                case "ready":
                    s.phaseT -= dt; if (s.phaseT <= 0) { s.phase = "play"; sWhistle(); }
                    socBallFollow(); socCamera(dt); return;
                case "goal":
                    s.phaseT -= dt;
                    foreach (var p in s.pl) socMove(p, 0, 0, dt);
                    socBall(dt); socCamera(dt);
                    if (U.R() < 0.5) addPart(U.R() * W, -10, (U.R() - 0.5) * 80, 80 + U.R() * 120, 3, U.Hsl(U.R() * 360, 0.9, 0.6), 7, 40);
                    if (s.phaseT <= 0) { if (s.endPending) socEndMatch(); else { socKickoff(); s.phase = "ready"; s.phaseT = 1.0; } }
                    return;
                case "end":
                    s.phaseT -= dt;
                    foreach (var p in s.pl) socMove(p, 0, 0, dt);
                    socBall(dt); socCamera(dt);
                    if (s.phaseT <= 0 && state == "soccer") { state = "sres"; s.resT = 0; socSaveResult(); }
                    return;
            }
            // ---- jugando ----
            s.clock += dt;
            for (int t = 0; t < 2; t++) s.meter[t] = Math.Min(1, s.meter[t] + dt * 0.022);
            if (!s.golden && s.clock >= s.dur)
            {
                if (s.score[0] == s.score[1]) { s.golden = true; sWhistle(); showSMsg("¡GOL DE ORO!", Y_, 2.5); }
                else { socEndMatch(); return; }
            }
            if (s.golden && s.clock >= s.dur + 90) { socEndMatch(); return; }
            // quién persigue la pelota en cada equipo
            for (int t = 0; t < 2; t++)
            {
                SPl best = null; double bd = 1e9;
                foreach (var p in s.pl)
                {
                    if (p.team != t || p.gk || p.stun > 0 || p.frozen > 0) continue;
                    double d = hyp(p.x - b.x, p.y - b.y) - (p == s.chaser[t] ? 40 : 0);
                    if (b.passTo == p) d -= 400;
                    if (d < bd) { bd = d; best = p; }
                }
                s.chaser[t] = best;
                // el jugador humano maneja al que tiene la pelota o al más cercano
                if (t < s.np && !s.allAI)
                {
                    if (b.owner != null && b.owner.team == t && !b.owner.gk) setCtl(t, b.owner);
                    else if (best != null && s.ctl[t] != best && s.switchT[t] <= 0 && s.ctl[t].slide <= 0 && b.passTo == null)
                    {
                        double dc = hyp(s.ctl[t].x - b.x, s.ctl[t].y - b.y), dn = hyp(best.x - b.x, best.y - b.y);
                        if (dn + 90 < dc) setCtl(t, best);
                    }
                }
            }
            foreach (var p in s.pl) updateSPl(p, dt);
            socCollisions(dt);
            socBall(dt);
            socTouches();
            socCamera(dt);
        }
        static void socBallFollow()
        {
            var b = soc.ball; if (b.owner == null) return;
            var p = b.owner; b.x = p.x + Math.Cos(p.face) * (PR + BR + 2); b.y = p.y + Math.Sin(p.face) * (PR + BR + 2); b.z = 0;
        }
        static void socCamera(double dt)
        {
            var s = soc; double lim = FL - (W / 2.0) / (FOC / CAMD) + 140;
            double tx = U.Clamp(s.ball.x + s.ball.vx * 0.15, -lim, lim);
            s.camX += (tx - s.camX) * Math.Min(1, dt * 3.5);
        }
        static void socEndMatch()
        {
            var s = soc; s.phase = "end"; s.phaseT = 3.2; sWhistle(true);
            showSMsg("¡FINAL DEL PARTIDO!", W_, 3.2);
        }
        static void socSaveResult()
        {
            var s = soc; if (s.np > 1 || s.allAI) return;
            string k = s.score[0] > s.score[1] ? "futbol_g" : s.score[0] < s.score[1] ? "futbol_p" : "futbol_e";
            int v; int.TryParse(store(k) ?? "0", out v); store(k, (v + 1).ToString());
            if (s.score[0] > s.score[1]) SFX.finish();
        }

        // ---------------- jugadores ----------------
        static void updateSPl(SPl p, double dt)
        {
            var s = soc; var b = s.ball;
            p.kickCD -= dt; p.slideCD -= dt; p.kickAnim = Math.Max(0, p.kickAnim - dt); p.pwT = Math.Max(0, p.pwT - dt); p.dive = Math.Max(0, p.dive - dt);
            if (p.frozen > 0) { p.frozen -= dt; socMove(p, 0, 0, dt); return; }
            if (p.stun > 0) { p.stun -= dt; socMove(p, 0, 0, dt); return; }
            if (p.slide > 0)
            {
                p.slide -= dt; double f = Math.Exp(-2.2 * dt); p.vx *= f; p.vy *= f; socMove(p, 0, 0, dt, true);
                foreach (var q in s.pl)
                {
                    if (q.team == p.team || q.stun > 0 || hyp(q.x - p.x, q.y - p.y) > PR * 2 + 6) continue;
                    if (q.gk && b.owner != q) continue;
                    if (b.owner == q)
                    {
                        q.stun = 0.8 + 0.1 * p.st.wgt; b.owner = null; double a = Math.Atan2(p.vy, p.vx);
                        b.vx = Math.Cos(a) * 260 + (U.R() - 0.5) * 120; b.vy = Math.Sin(a) * 260 + (U.R() - 0.5) * 120; b.vz = 120; b.last = q;
                        s.meter[p.team] = Math.Min(1, s.meter[p.team] + 0.1); SFX.bump(); socDust(q, 8);
                    }
                    else if (!q.gk) { q.stun = 0.5; SFX.bump(); }
                }
                return;
            }
            double dx = 0, dy = 0;
            if (p.human >= 0 && !s.allAI && !p.gk)
            {
                var c = socCtrl(p.human);
                dx = (held(c.right) ? 1 : 0) - (held(c.left) ? 1 : 0); dy = (held(c.brake) ? 1 : 0) - (held(c.acc) ? 1 : 0);
                double m = hyp(dx, dy); if (m > 0) { dx /= m; dy /= m; }
                bool has = b.owner == p, shootHeld = held(c.drift), shootPress = kp(c.drift), passPress = kp(c.use);
                if (kp(c.power) && s.meter[p.team] >= 1) socSuper(p);
                else if (has)
                {
                    if (shootPress) p.charge = 0;
                    if (p.charge >= 0)
                    {
                        if (shootHeld) { p.charge += dt / 0.7; if (p.charge >= 1.3) socShoot(p, 1, dy); }
                        else socShoot(p, U.Clamp(p.charge, 0.25, 1), dy);
                    }
                    if (passPress && b.owner == p) socPass(p, dx, dy);
                }
                else
                {
                    p.charge = -1;
                    if (shootPress) socSlide(p, dx, dy);
                    else if (passPress) socSwitch(p.human);
                }
            }
            else socAI(p, dt, out dx, out dy);
            socMove(p, dx, dy, dt);
        }
        static void socMove(SPl p, double dx, double dy, double dt, bool free = false)
        {
            var s = soc; var cf = CANCHAS[s.field];
            if (!free)
            {
                double top = p.top * (s.ball.owner == p ? 0.88 : 1) * (p.pwT > 0 ? 1.4 : 1) * (p.dive > 0 ? 1.7 : 1);
                if (p.stun > 0 || p.frozen > 0) { dx = dy = 0; }
                double rate = p.acc * (cf.ice ? 0.3 : 1);
                if (s.phase != "play") { rate = 6; }
                double k = Math.Min(1, rate * dt);
                p.vx += (dx * top - p.vx) * k; p.vy += (dy * top - p.vy) * k;
            }
            p.x += p.vx * dt; p.y += p.vy * dt;
            double sp = hyp(p.vx, p.vy);
            if (sp > 25 && p.slide <= 0 && p.stun <= 0)
            {
                double a = Math.Atan2(p.vy, p.vx), d = a - p.face;
                while (d > Math.PI) d -= 2 * Math.PI; while (d < -Math.PI) d += 2 * Math.PI;
                p.face += d * Math.Min(1, dt * 12);
            }
            p.run += sp * dt * 0.045;
            // límites de la cancha (el arquero se queda en su área)
            if (p.gk)
            {
                int sg = sgn(p.team); double line = -sg * (FL - PR);
                p.x = sg > 0 ? U.Clamp(p.x, line, line + 240) : U.Clamp(p.x, line - 240, line);
                p.y = U.Clamp(p.y, -290, 290);
            }
            else { p.x = U.Clamp(p.x, -FL + PR, FL - PR); p.y = U.Clamp(p.y, -FW + PR, FW - PR); }
        }
        static void socCollisions(double dt)
        {
            var s = soc; var b = s.ball;
            for (int i = 0; i < s.pl.Count; i++)
                for (int j = i + 1; j < s.pl.Count; j++)
                {
                    var a = s.pl[i]; var c = s.pl[j];
                    double dx = c.x - a.x, dy = c.y - a.y, d = hyp(dx, dy);
                    if (d >= PR * 2 || d < 0.01) continue;
                    double push = PR * 2 - d, wa = c.st.wgt / (double)(a.st.wgt + c.st.wgt);
                    a.x -= dx / d * push * wa; a.y -= dy / d * push * wa; c.x += dx / d * push * (1 - wa); c.y += dy / d * push * (1 - wa);
                }
            // robar la pelota con el cuerpo
            var o = b.owner;
            if (o != null && !o.gk && s.phase == "play")
                foreach (var q in s.pl)
                {
                    if (q.team == o.team || q.stun > 0 || q.frozen > 0 || q.kickCD > 0 || (q.gk && !o.gk && Math.Abs(q.x) < FL - 240)) continue;
                    if (hyp(q.x - b.x, q.y - b.y) > PR + BR + 4) continue;
                    if (q.pwT > 0) { o.stun = 1; socTake(q); SFX.hit(); break; }
                    if (o.pwT > 0) { q.stun = 0.8; SFX.bump(); continue; }
                    double rate = (q.human >= 0 ? 3.2 : 2.2 * (humanTeam(q.team) ? 1 : new[] { 0.7, 1, 1.35 }[s.diff])) + 0.25 * (q.st.wgt - o.st.wgt);
                    if (U.R() < dt * Math.Max(0.6, rate)) { o.kickCD = 0.45; socTake(q); SFX.bump(); break; }
                }
            // súper carrera: tumba a los rivales que toca
            foreach (var p in s.pl)
            {
                if (p.pwT <= 0) continue;
                foreach (var q in s.pl) if (q.team != p.team && !q.gk && q.stun <= 0 && hyp(q.x - p.x, q.y - p.y) < PR * 2 + 4) { q.stun = 1; SFX.bump(); if (b.owner == q) socTake(p); }
            }
        }
        static void socTake(SPl p)
        {
            var s = soc; var b = s.ball;
            if (b.passTo == p && b.last != null && b.last.team == p.team) s.meter[p.team] = Math.Min(1, s.meter[p.team] + 0.05);
            b.owner = p; b.vx = b.vy = b.vz = 0; b.z = 0; b.passTo = null; b.superT = 0; b.sup = ""; b.last = p; p.charge = -1;
            p.aiT = p.gk ? 0.7 + U.R() * 0.4 : 0.25 + U.R() * 0.9;
            if (humanTeam(p.team) && !p.gk && p.team < s.np) setCtl(p.team, p);
        }
        static void socSwitch(int t)
        {
            var s = soc; var b = s.ball; SPl best = null; double bd = 1e9;
            foreach (var p in s.pl) { if (p.team != t || p.gk || p == s.ctl[t]) continue; double d = hyp(p.x - b.x, p.y - b.y); if (d < bd) { bd = d; best = p; } }
            if (best != null) { setCtl(t, best); s.switchT[t] = 1.2; SFX.sel(); }
        }
        static void socSlide(SPl p, double dx, double dy)
        {
            if (p.slideCD > 0 || p.gk) return;
            double a = (dx != 0 || dy != 0) ? Math.Atan2(dy, dx) : p.face;
            p.face = a; p.slide = 0.42; p.slideCD = 1.0;
            double sp = Math.Max(hyp(p.vx, p.vy), p.top) * 1.45; p.vx = Math.Cos(a) * sp; p.vy = Math.Sin(a) * sp;
            SFX.hop(); socDust(p, 6);
        }
        static void socDust(SPl p, int n)
        {
            double k = socK(p.y), x = SX(p.x, p.y), y = SY(p.y, 0); Color c = CANCHAS[soc.field].outside;
            for (int i = 0; i < n; i++) addPart(x + (U.R() - 0.5) * 30 * k, y, (U.R() - 0.5) * 160, -60 - U.R() * 120, 0.45, c, 5 * k + 2, 500);
        }

        // ---------------- tiros, pases y súper tiro ----------------
        static void socShoot(SPl p, double pow, double aimDy)
        {
            var s = soc; var b = s.ball; p.charge = -1; if (b.owner != p) return;
            int sg = sgn(p.team); double gx = sg * FL, fx = Math.Cos(p.face), fy = Math.Sin(p.face), ang;
            if (fx * sg > 0.15 && Math.Abs(gx - p.x) < 1000)
            {
                double ty = aimDy != 0 ? aimDy * GW * 0.72 : U.Clamp(p.y + fy / Math.Abs(fx) * Math.Abs(gx - p.x) * 0.5, -GW * 0.7, GW * 0.7);
                double noise = p.human >= 0 && !s.allAI ? 0.22 : humanTeam(p.team) ? 0.3 : new[] { 0.6, 0.38, 0.2 }[s.diff];
                ty += (U.R() - 0.5) * 2 * GW * noise;
                ang = Math.Atan2(ty - p.y, gx - p.x);
            }
            else ang = p.face;
            double sp = 620 + 680 * pow + 22 * p.st.wgt;
            b.owner = null; b.vx = Math.Cos(ang) * sp; b.vy = Math.Sin(ang) * sp; b.vz = 90 + 230 * pow; b.z = 6;
            b.last = p; b.passTo = null; p.kickCD = 0.3; p.kickAnim = 0.25; p.face = ang;
            sKick(pow); socDust(p, 4);
        }
        static void socPass(SPl p, double dx, double dy, bool far = false)
        {
            var s = soc; var b = s.ball; if (b.owner != p) return;
            int sg = sgn(p.team); double rx = dx, ry = dy;
            if (rx == 0 && ry == 0) { rx = Math.Cos(p.face); ry = Math.Sin(p.face); }
            double m = hyp(rx, ry); rx /= m; ry /= m;
            SPl best = null; double bs = -99;
            foreach (var t in s.pl)
            {
                if (t == p || t.team != p.team || t.gk || t.stun > 0) continue;
                double vx = t.x - p.x, vy = t.y - p.y, d = hyp(vx, vy); if (d < 1) continue;
                double sc = far ? t.x * sg / FL - d / 4000 : (vx * rx + vy * ry) / d - d / 1600;
                // que no haya un rival justo en el camino
                foreach (var q in s.pl) if (q.team != p.team && !q.gk) { double u = ((q.x - p.x) * vx + (q.y - p.y) * vy) / (d * d); if (u > 0.1 && u < 0.9 && hyp(p.x + vx * u - q.x, p.y + vy * u - q.y) < 40) sc -= 0.5; }
                if (sc > bs) { bs = sc; best = t; }
            }
            if (best == null) { socShoot(p, 0.3, 0); return; }
            double lx = best.x + best.vx * 0.35, ly = best.y + best.vy * 0.35, ddx = lx - p.x, ddy = ly - p.y, dd = Math.Max(1, hyp(ddx, ddy));
            double spd = U.Clamp(dd * 1.5 + 220, 420, 1000);
            b.owner = null; b.vx = ddx / dd * spd; b.vy = ddy / dd * spd; b.vz = dd > 480 ? 260 : 30; b.z = 4;
            b.last = p; b.passTo = best; p.kickCD = 0.3; p.kickAnim = 0.2; p.face = Math.Atan2(ddy, ddx);
            if (humanTeam(p.team) && p.team < s.np) setCtl(p.team, best);
            sKick(0.2);
        }
        static void socSuper(SPl p)
        {
            var s = soc; var b = s.ball; if (s.meter[p.team] < 1) return;
            s.meter[p.team] = 0;
            var PW = PowerOf(p.ch); string ty = PW[0]; Color col = PW_ICON[ty];
            SFX.power(); showSMsg("¡" + PW[1].ToUpper() + "!", col, 1.6); s.flash = 0.2; s.shake = 0.3;
            if (b.owner == p)
            {
                int sg = sgn(p.team); double gx = sg * FL, ty2 = (U.R() < 0.5 ? -1 : 1) * GW * (0.35 + U.R() * 0.3);
                double ang = Math.Atan2(ty2 - p.y, gx - p.x), sp = ty == "laser" || ty == "kame" ? 2100 : 1500;
                b.owner = null; b.vx = Math.Cos(ang) * sp; b.vy = Math.Sin(ang) * sp; b.vz = ty == "fly" || ty == "acro" ? 380 : 70; b.z = 8;
                b.last = p; b.passTo = null; b.superT = 2.5; b.sup = ty; b.supCol = col; b.zig = 0; b.teleT = ty == "tele" ? 0.22 : 0;
                p.kickCD = 0.4; p.kickAnim = 0.3; p.face = ang; sKick(1);
                if (ty == "freeze") foreach (var q in s.pl) if (q.team != p.team && !q.gk) q.frozen = 2;
                if (ty == "shock") foreach (var q in s.pl) if (q.team != p.team && !q.gk && hyp(q.x - p.x, q.y - p.y) < 300) q.stun = 1.3;
                if (ty == "magnet") s.meter[p.team] = 0.3;
                if (ty == "ssj") p.pwT = 2.5;
            }
            else p.pwT = 3.5;   // sin la pelota: ¡súper carrera!
        }
        static double superSave(string ty) { return ty == "ghost" ? 0.1 : ty == "giant" || ty == "laser" || ty == "kame" ? 0.15 : ty == "tele" ? 0.2 : 0.25; }
        static double gkSkill(int t) { return humanTeam(t) ? 0.72 : new[] { 0.55, 0.7, 0.84 }[soc.diff]; }

        // ---------------- la pelota ----------------
        static void socBall(double dt)
        {
            var s = soc; var b = s.ball;
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
                if (b.sup == "spin") { b.zig += dt * 14; b.y += Math.Cos(b.zig) * dt * 420; }
                if (b.teleT > 0) { b.teleT -= dt; if (b.teleT <= 0) { double nx = U.Clamp(b.x + b.vx * 0.22, -FL + 90, FL - 90); for (int i = 0; i < 10; i++) addPart(SX(b.x, b.y), SY(b.y, b.z), (U.R() - 0.5) * 300, (U.R() - 0.5) * 300, 0.5, b.supCol, 6, 0); b.x = nx; b.trail.Clear(); } }
                if (hyp(b.vx, b.vy) < 500) b.superT = 0;
            }
            else if (b.trail.Count > 0) b.trail.RemoveAt(0);
            double oldAx = Math.Abs(b.x);
            b.x += b.vx * dt; b.y += b.vy * dt; b.z += b.vz * dt;
            if (b.z > 0 || b.vz > 0) b.vz -= (CANCHAS[s.field].moon ? 480 : CANCHAS[s.field].heavy ? 2200 : 1100) * dt;
            if (b.z <= 0) { b.z = 0; if (b.vz < -140) { b.vz = -b.vz * 0.5; } else b.vz = 0; }
            double fr = b.z <= 0.5 ? (CANCHAS[s.field].ice ? 0.5 : 1.1) : 0.15; double f = Math.Exp(-fr * dt);
            b.vx *= f; b.vy *= f; b.spin += hyp(b.vx, b.vy) * dt * 0.03;
            // paredes laterales
            if (b.y > FW - BR) { b.y = FW - BR; b.vy = -Math.Abs(b.vy) * 0.7; }
            if (b.y < -FW + BR) { b.y = -FW + BR; b.vy = Math.Abs(b.vy) * 0.7; }
            double ax = Math.Abs(b.x); int side = b.x > 0 ? 1 : -1;
            if (ax > FL && oldAx <= FL && (Math.Abs(b.y) > GW - 4 || b.z > GH - 4))
            {
                // pegó en la pared de al lado del arco (o por encima del travesaño)
                b.x = side * (FL - BR); b.vx = -b.vx * 0.7;
            }
            else if (ax > FL)
            {
                // adentro del arco: la red la frena
                if (ax > FL + GD - BR) { b.x = side * (FL + GD - BR); b.vx = -b.vx * 0.15; b.vy *= 0.5; }
                if (Math.Abs(b.y) > GW - BR) { b.y = Math.Sign(b.y) * (GW - BR); b.vy = -b.vy * 0.3; }
                if (b.z > GH - BR) { b.z = GH - BR; b.vz = -Math.Abs(b.vz) * 0.3; }
                if (ax > FL + BR && s.phase == "play") socGoal(side > 0 ? 0 : 1);
            }
            else if (ax > FL - BR)
            {
                bool mouth = Math.Abs(b.y) < GW - 4 && b.z < GH - 4;
                if (!mouth) { b.x = side * (FL - BR); b.vx = -b.vx * 0.7; if (b.z > GH - 8 && b.z < GH + 10 && Math.Abs(b.y) < GW) { b.vz = Math.Abs(b.vz) * 0.5 + 80; SFX.bump(); } }
            }
            // postes
            foreach (int px in new[] { -1, 1 })
                foreach (int py in new[] { -1, 1 })
                {
                    double dx = b.x - px * FL, dy = b.y - py * GW, d = hyp(dx, dy);
                    if (d < BR + 5 && d > 0.01 && b.z < GH)
                    {
                        double nx = dx / d, ny = dy / d, dot = b.vx * nx + b.vy * ny;
                        if (dot < 0) { b.vx -= 1.7 * dot * nx; b.vy -= 1.7 * dot * ny; SFX.bump(); }
                        b.x = px * FL + nx * (BR + 5); b.y = py * GW + ny * (BR + 5);
                    }
                }
        }
        static void socTouches()
        {
            var s = soc; var b = s.ball; if (b.owner != null || s.phase != "play") return;
            double sp = hyp(b.vx, b.vy); double big = b.sup == "giant" && b.superT > 0 ? BR : 0;
            foreach (var p in s.pl)
            {
                if (p.stun > 0 || p.frozen > 0 || p.kickCD > 0) continue;
                double reach = PR + BR + big + (p.gk ? 16 + (p.dive > 0 ? 14 : 0) : 6);
                if (b.z > (p.gk ? 110 : 40) || hyp(b.x - p.x, b.y - p.y) > reach) continue;
                bool super = b.superT > 0 && b.last != null && b.last.team != p.team;
                if (p.gk)
                {
                    if (b.saveCD > 0) continue;
                    bool shot = b.last != null && b.last.team != p.team && sp > 450;
                    if (shot)
                    {
                        double chance = super ? superSave(b.sup) : U.Clamp(gkSkill(p.team) + (1 - sp / 1500) * 0.35, 0.12, 0.96);
                        if (U.R() > chance) { b.saveCD = 0.5; if (super) { p.stun = 1.2; SFX.hit(); } continue; }
                        if (super || sp > 1000 || U.R() < 0.4)
                        {
                            b.vx = -b.vx * 0.35; b.vy = b.vy * 0.4 + (U.R() - 0.5) * 500; b.vz = 200 + U.R() * 150; b.superT = 0; b.sup = "";
                            b.saveCD = 0.35; b.last = p; b.passTo = null; p.kickCD = 0.3; SFX.bump(); showSMsg("¡ATAJADA!", W_, 1); continue;
                        }
                        showSMsg("¡ATAJADA!", W_, 1);
                    }
                    socTake(p); continue;
                }
                if (super) { p.stun = 1.1; p.vx = b.vx * 0.3; p.vy = b.vy * 0.3; b.vx *= 0.85; b.vy *= 0.85; SFX.hit(); continue; }
                socTake(p);
                return;
            }
        }
        static void socGoal(int team)
        {
            var s = soc; var b = s.ball;
            s.score[team]++; s.phase = "goal"; s.phaseT = 3.2; s.kickTeam = 1 - team; s.lastGoalTeam = team;
            var sc = b.last; bool own = sc != null && sc.team != team;
            if (sc != null && !own) sc.goals++;
            int min = (int)Math.Floor(s.clock / 60) + 1;
            s.log.Add(new[] { team.ToString(), (sc != null ? sc.ch.name : "?") + (own ? " (en contra)" : ""), min + "'" });
            s.meter[1 - team] = Math.Min(1, s.meter[1 - team] + 0.25);
            if (s.golden) s.endPending = true;
            showSMsg("¡GOOOOL!", TEAM_COL[team], 3); s.shake = 0.4;
            SFX.finish(); Snd.Noise(1.6, 0.2, 0, 1500);
            for (int i = 0; i < 80; i++) addPart(U.R() * W, -10 - U.R() * 60, (U.R() - 0.5) * 120, 60 + U.R() * 160, 3.5, U.Hsl(U.R() * 360, 0.9, 0.6), 7, 40);
        }

        // ---------------- inteligencia de los bots ----------------
        static void socAI(SPl p, double dt, out double dx, out double dy)
        {
            var s = soc; var b = s.ball; int sg = sgn(p.team);
            double tx, ty, mul = humanTeam(p.team) || s.allAI ? 0.95 : new[] { 0.84, 0.94, 1.02 }[s.diff];
            if (p.gk) socGK(p, dt, out tx, out ty);
            else if (b.owner == p)
            {
                // con la pelota: avanzar hacia el arco rival
                tx = sg * FL - sg * 60; ty = U.Clamp(p.y * 0.5, -GW * 0.6, GW * 0.6);
                SPl opp = null; double od = 1e9;
                foreach (var q in s.pl) if (q.team != p.team) { double d = hyp(q.x - p.x, q.y - p.y); if (d < od) { od = d; opp = q; } }
                if (opp != null && od < 170 && (opp.x - p.x) * sg > -20) ty = p.y + (opp.y > p.y ? -1 : 1) * 220;
                double dg = hyp(sg * FL - p.x, p.y);
                p.aiT -= dt;
                if (s.meter[p.team] >= 1 && dg < 650 && dg > 200 && !humanTeam(p.team)) socSuper(p);
                else if (dg < 430 && p.aiT <= 0) socShoot(p, 0.45 + U.R() * 0.55, 0);
                else if (opp != null && od < 75 && U.R() < dt * 2.2) socPass(p, sg, 0);
                else if (U.R() < dt * 0.25) socPass(p, sg, 0);
            }
            else
            {
                bool mine = b.owner != null && b.owner.team == p.team;
                if (!mine && s.chaser[p.team] == p)
                {
                    double lead = b.owner == null ? 0.25 : 0.1; tx = b.x + b.vx * lead; ty = b.y + b.vy * lead;
                    if (b.owner != null && hyp(b.owner.x - p.x, b.owner.y - p.y) < 85 && p.slideCD <= 0)
                    {
                        double rate = humanTeam(p.team) || s.allAI ? 1.6 : new[] { 0.7, 1.6, 2.6 }[s.diff];
                        if (U.R() < dt * rate) socSlide(p, b.owner.x - p.x, b.owner.y - p.y);
                    }
                    if (s.meter[p.team] >= 1 && b.owner != null && !humanTeam(p.team) && U.R() < dt * 0.4) socSuper(p);
                }
                else if (mine)
                {
                    // apoyar el ataque: buscar espacio adelante
                    tx = U.Clamp(b.x + sg * (190 + p.slot * 50), -FL + 120, FL - 140); ty = p.hy * 1.1;
                    if (hyp(tx - b.x, ty - b.y) < 140) ty += (p.slot == 1 ? -1 : 1) * 120;
                }
                else
                {
                    // defender: entre la pelota y el arco propio
                    tx = U.Lerp(p.hx, b.x, 0.5); ty = U.Lerp(p.hy, b.y, 0.45);
                    tx = sg > 0 ? Math.Min(tx, b.x - 90) : Math.Max(tx, b.x + 90);
                    tx = U.Clamp(tx, -FL + 60, FL - 60);
                }
            }
            double ddx = tx - p.x, ddy = ty - p.y, dd = hyp(ddx, ddy);
            if (dd < 8) { dx = dy = 0; return; }
            double m = Math.Min(1, dd / 60.0) * mul; dx = ddx / dd * m; dy = ddy / dd * m;
        }
        static void socGK(SPl p, double dt, out double tx, out double ty)
        {
            var b = soc.ball; int sg = sgn(p.team); double gx = -sg * (FL - 26);
            tx = gx; ty = U.Clamp(b.y * 0.4, -GW * 0.75, GW * 0.75);
            if (b.owner == p)
            {
                tx = p.x; ty = p.y; p.aiT -= dt; p.face = sg > 0 ? 0 : Math.PI;
                if (p.aiT <= 0) socPass(p, sg, 0, true);
                return;
            }
            double depth = (b.x + sg * FL) * sg;   // distancia de la pelota a la línea del arco propio
            if (b.owner == null && depth < 220 && Math.Abs(b.y) < 270 && hyp(b.vx, b.vy) < 500) { tx = b.x; ty = b.y; }
            else if (b.owner == null && b.vx * sg < -150)
            {
                double t = (gx - b.x) / b.vx;
                if (t > 0 && t < 0.9) { ty = U.Clamp(b.y + b.vy * t, -GW - 10, GW + 10); if (t < 0.35 && Math.Abs(ty - p.y) > 25) p.dive = 0.3; }
            }
            else if (b.owner != null && b.owner.team != p.team && depth < 420) { tx = gx + sg * 45; ty = U.Clamp(b.y * 0.6, -GW * 0.8, GW * 0.8); }
        }

        // =================== dibujo ===================
        static double socK(double y) { return FOC / (FW + CAMD - y); }
        static double SX(double x, double y) { return W / 2.0 + (x - soc.camX) * socK(y); }
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

        static void drawSoccer()
        {
            var s = soc; var cf = CANCHAS[s.field]; var b = s.ball;
            var st0 = Save();
            if (s.shake > 0) Translate((U.R() - 0.5) * 14 * s.shake / 0.4, (U.R() - 0.5) * 8 * s.shake / 0.4);
            Gradient(-20, -20, W + 40, 200, cf.sky0, cf.sky1);
            rect(-20, 150, W + 40, H, cf.outside);
            drawStands(cf);
            quadW(cf.outside, -FL - 900, -FW - 50, FL + 900, -FW - 50, FL + 900, FW + 300, -FL - 900, FW + 300);
            if (s.field == 3) foreach (int sd in new[] { -1, 1 }) for (int i = 0; i < 3; i++) { double lx = sd * (FL + 260), ly = -220 + i * 220, kk = socK(ly); ell(SX(lx, ly), SY(ly, 0), (70 + Math.Sin(T * 2 + i) * 8) * kk, 22 * kk, U.C("#ffd23a")); }
            // franjas del césped
            int n = 16; double bw = 2 * FL / n;
            for (int i = 0; i < n; i++) { double x0 = -FL + i * bw; quadW(cf.rainbow ? U.Hsl(i * 360.0 / n, 0.7, i % 2 == 0 ? 0.55 : 0.6) : i % 2 == 0 ? cf.g1 : cf.g2, x0, -FW, x0 + bw, -FW, x0 + bw, FW, x0, FW); }
            if (cf.moon) foreach (var c in new[] { new[] { -520.0, -210, 70 }, new[] { 310.0, 250, 85 }, new[] { 620.0, -160, 55 }, new[] { -230.0, 290, 50 }, new[] { 120.0, -300, 75 }, new[] { -660.0, 230, 45 } })
                { double kk = socK(c[1]), cx = SX(c[0], c[1]), cy = SY(c[1], 0); ell(cx, cy, c[2] * kk, c[2] * 0.36 * kk, U.C("#83839a")); ell(cx + 4 * kk, cy + 3 * kk, c[2] * 0.8 * kk, c[2] * 0.26 * kk, U.C("#70708a")); }
            if (cf.tiles)
            {
                Color tl = U.Shade(cf.g1, -0.15);
                for (double tx = -FL + 100; tx < FL; tx += 100) lineW(tl, 1.3, tx, -FW, tx, FW);
                for (double ty = -FW + 110; ty < FW; ty += 110) lineW(tl, 1.3, -FL, ty, FL, ty);
            }
            if (cf.deco.Length > 0) drawSocDeco(cf);
            // líneas
            Color lc = U.A(cf.line, 0.85); double lw = 2.6;
            lineW(lc, lw, -FL, -FW, FL, -FW); lineW(lc, lw, FL, -FW, FL, FW); lineW(lc, lw, FL, FW, -FL, FW); lineW(lc, lw, -FL, FW, -FL, -FW);
            lineW(lc, lw, 0, -FW, 0, FW); circW(lc, lw, 0, 0, 130);
            foreach (int sd in new[] { -1, 1 })
            {
                double x0 = sd * FL, x1 = sd * (FL - 200), x2 = sd * (FL - 80);
                lineW(lc, lw, x0, -250, x1, -250); lineW(lc, lw, x1, -250, x1, 250); lineW(lc, lw, x1, 250, x0, 250);
                lineW(lc, lw, x0, -150, x2, -150); lineW(lc, lw, x2, -150, x2, 150); lineW(lc, lw, x2, 150, x0, 150);
                double px = sd * (FL - 140); ell(SX(px, 0), SY(0, 0), 4 * socK(0), 2 * socK(0), lc);
            }
            ell(SX(0, 0), SY(0, 0), 5 * socK(0), 2.5 * socK(0), lc);
            // pared del fondo y paredes de los costados
            for (int i = 0; i < 12; i++) { double x0 = -FL - 100 + i * (2 * FL + 200) / 12.0; wallW(i % 2 == 0 ? cf.wall : cf.wall2, x0, -FW - 6, x0 + (2 * FL + 200) / 12.0, -FW - 6, 42); }
            foreach (int sd in new[] { -1, 1 }) { wallW(U.Shade(cf.wall, -0.2), sd * (FL + 6), -FW, sd * (FL + 6), -GW, 42); wallW(U.Shade(cf.wall, -0.2), sd * (FL + 6), GW, sd * (FL + 6), FW, 42); }
            if (cf.night) drawNightLights();
            // arcos (red y poste lejano)
            foreach (int sd in new[] { -1, 1 }) drawGoalBack(sd);
            // sombras y anillos de equipo
            foreach (var p in s.pl)
            {
                double k = socK(p.y), x = SX(p.x, p.y), y = SY(p.y, 0);
                ell(x, y, 19 * k, 6.5 * k, U.RGBA(0, 0, 0, 0.28));
                Color rc = p.human >= 0 && !s.allAI ? (s.np > 1 ? P_COL[p.human] : Y_) : TEAM_COL[p.team];
                ellStroke(x, y, 23 * k, 8 * k, rc, p.human >= 0 && !s.allAI ? 3.5 : 2);
            }
            { double k = socK(b.y); ell(SX(b.x, b.y), SY(b.y, 0), BR * k * (1 - Math.Min(0.5, b.z / 300)), BR * 0.4 * k, U.RGBA(0, 0, 0, 0.3)); }
            // todo lo que tiene altura, ordenado de atrás hacia adelante
            var items = new List<Tuple<double, Action>>();
            foreach (var p in s.pl) { var pp = p; items.Add(Tuple.Create(p.y, (Action)(() => drawSPl(pp)))); }
            items.Add(Tuple.Create(b.y + 0.5, (Action)drawSBall));
            foreach (int sd in new[] { -1, 1 }) { int d = sd; items.Add(Tuple.Create(GW, (Action)(() => drawPost(d, GW)))); }
            foreach (var it in items.OrderBy(t => t.Item1)) it.Item2();
            // pared de adelante (transparente)
            double oa = ALPHA; ALPHA = 0.45;
            wallW(cf.wall, -FL - 100, FW + 8, FL + 100, FW + 8, 40);
            ALPHA = oa;
            Restore(st0);
            drawSocHUD();
        }
        static void drawStands(Cancha cf)
        {
            var s = soc; double y = -FW - 60, k = socK(y);
            fillPts(cf.stand, -20, SY(y, 0), W + 20, SY(y, 0), W + 20, SY(y, 250), -20, SY(y, 250));
            bool party = s.phase == "goal" || s.phase == "end";
            double x0 = s.camX - (W / 2.0 + 30) / k, x1 = s.camX + (W / 2.0 + 30) / k;
            for (int r = 0; r < 4; r++)
            {
                double z = 70 + r * 48; int c0 = (int)Math.Floor(x0 / 46);
                for (int c = c0; c * 46 < x1; c++)
                {
                    int h = (c * 73856093) ^ (r * 19349663); h = Math.Abs(h);
                    double bob = party ? Math.Abs(Math.Sin(T * 12 + h % 7)) * 14 : Math.Sin(T * 2 + h % 5) * 2;
                    double x = c * 46 + (r % 2) * 23, sy = SY(y, z + bob);
                    if (sy < -10) continue;
                    Color shirt = (h % 3 == 0) ? TEAM_COL[0] : (h % 3 == 1) ? TEAM_COL[1] : U.Hsl(h % 360, 0.6, 0.55);
                    rect(SX(x, y) - 7 * k, sy, 14 * k, 16 * k, shirt);
                    circ(SX(x, y), sy - 4 * k, 7 * k, (h % 4 == 0) ? U.C("#8a5a3a") : U.C("#ffcc99"));
                }
            }
        }
        // decorados de Dragon Ball (k = escala de la cámara en ese punto)
        static readonly string[] DB_DECO = { "namektree", "kamehouse", "dragonball", "pillar" };
        static void drawDBDeco(string t, double x, double y, double k, int seed)
        {
            switch (t)
            {
                case "namektree":
                    lines(C("#8a7a5a"), 14 * k, false, x, y, x, y - 210 * k);
                    circ(x, y - 250 * k, 72 * k, C("#3a9a4a")); circ(x - 22 * k, y - 270 * k, 30 * k, C("#5ac86a"));
                    break;
                case "kamehouse":
                    {
                        double w = 190 * k, h = 110 * k;
                        rect(x - w / 2, y - h, w, h, C("#ff8ab0"));
                        fillPts(C("#e52521"), x - w * 0.6, y - h, x + w * 0.6, y - h, x + w * 0.35, y - h - 75 * k, x - w * 0.35, y - h - 75 * k);
                        rect(x - 25 * k, y - 85 * k, 50 * k, 85 * k, C("#8a4a2a"));
                        rect(x - w * 0.4, y - h * 0.78, 50 * k, 40 * k, C("#bfefff")); rect(x + w * 0.4 - 50 * k, y - h * 0.78, 50 * k, 40 * k, C("#bfefff"));
                        if (k > 0.25) txt("KAME HOUSE", x, y - h + 16 * k, 20 * k, W_, "center", Color.Black, 2);
                        break;
                    }
                case "dragonball":
                    {
                        double R = 28 * k, cy = y - R; int n = seed % 7 + 1;
                        ell(x, y, R, R * 0.3, U.RGBA(0, 0, 0, 0.2));
                        circ(x, cy, R, C("#ff9a1a")); circ(x - R * 0.35, cy - R * 0.35, R * 0.28, C("#ffd8a0"));
                        for (int i = 0; i < n; i++)
                        {
                            double a = i * Math.PI * 2 / n, d = n == 1 ? 0 : R * 0.45;
                            g.FillPolygon(B(C("#e52521")), starPts(x + Math.Cos(a) * d, cy + Math.Sin(a) * d, R * 0.24, R * 0.1));
                        }
                        break;
                    }
                case "pillar":
                    rect(x - 22 * k, y - 230 * k, 44 * k, 230 * k, C("#f0ece0")); rect(x - 10 * k, y - 230 * k, 8 * k, 230 * k, C("#ffffff"));
                    rect(x - 32 * k, y - 250 * k, 64 * k, 22 * k, C("#c8402a")); rect(x - 32 * k, y - 12 * k, 64 * k, 12 * k, C("#c8c0b0"));
                    break;
            }
        }
        // decorados a los lados de la cancha (detrás de los arcos), de atrás hacia adelante
        static void drawSocDeco(Cancha cf)
        {
            for (int r = 0; r < 5; r++)
            {
                double y = -380 + r * 190, kd = socK(y) * 0.17;
                foreach (int sd in new[] { -1, 1 })
                    for (int j = 1; j >= 0; j--)
                    {
                        double x = sd * (FL + 170 + j * 150 + (r % 2) * 50), sx = SX(x, y);
                        if (sx < -120 || sx > W + 120) continue;
                        string t = cf.deco[(r + j * 3 + (sd > 0 ? 1 : 0)) % cf.deco.Length];
                        if (DB_DECO.Contains(t)) drawDBDeco(t, sx, SY(y, 0), socK(y), r * 3 + j * 2 + (sd > 0 ? 1 : 0)); else drawDecor(new Decor { t = t, x = sd, h = 0.3 + 0.1 * r, dir = sd }, sx, SY(y, 0), kd);
                    }
            }
        }
        // estadio de noche: torres de luces arriba de la tribuna, haces de luz y flashes de cámaras
        static void drawNightLights()
        {
            var s = soc; double y = -FW - 60, k = socK(y);
            for (int i = -2; i <= 2; i++)
            {
                double lx = i * 650, sx = SX(lx, y), sy = SY(y, 255);
                if (sx < -150 || sx > W + 150) continue;
                fillPts(U.RGBA(255, 250, 200, 0.07), sx - 20, sy, sx + 20, sy, SX(lx * 0.5 + 260, 120), SY(120, 0), SX(lx * 0.5 - 260, 120), SY(120, 0));
                circ(sx, sy, 60 * k * 2, U.RGBA(255, 250, 210, 0.12));
                rrFill(sx - 26, sy - 14, 52, 22, 4, U.C("#3a3a4a"));
                for (int a = 0; a < 4; a++) for (int b = 0; b < 2; b++) circ(sx - 18 + a * 12, sy - 8 + b * 10, 4.2, U.C("#fffbe0"));
            }
            bool party = s.phase == "goal" || s.phase == "end";
            int n = party ? 10 : 2;
            for (int i = 0; i < n; i++) if (U.R() < 0.5) { double fx = U.R() * W, fy = SY(y, 70 + U.R() * 150); circ(fx, fy, 3 + U.R() * 3, U.RGBA(255, 255, 255, 0.9)); }
        }
        static void drawGoalBack(int sd)
        {
            double fx = sd * FL, bx = sd * (FL + GD); bool dk = CANCHAS[soc.field].heavy; Color net = dk ? U.RGBA(90, 90, 130, 0.22) : U.RGBA(255, 255, 255, 0.22), nl = dk ? U.RGBA(90, 90, 130, 0.55) : U.RGBA(255, 255, 255, 0.55), pc = dk ? C("#5a5a78") : Color.White;
            // red de atrás, techo y costados
            fillPts(net, SX(bx, -GW), SY(-GW, 0), SX(bx, GW), SY(GW, 0), SX(bx, GW), SY(GW, GH), SX(bx, -GW), SY(-GW, GH));
            fillPts(net, SX(fx, -GW), SY(-GW, GH), SX(bx, -GW), SY(-GW, GH), SX(bx, GW), SY(GW, GH), SX(fx, GW), SY(GW, GH));
            fillPts(net, SX(fx, -GW), SY(-GW, 0), SX(bx, -GW), SY(-GW, 0), SX(bx, -GW), SY(-GW, GH), SX(fx, -GW), SY(-GW, GH));
            for (int i = 0; i <= 8; i++) { double yy = -GW + i * GW / 4; lines(nl, 1, false, SX(bx, yy), SY(yy, 0), SX(bx, yy), SY(yy, GH), SX(fx, yy), SY(yy, GH)); }
            for (int i = 1; i <= 3; i++) { double z = i * GH / 4; lines(nl, 1, false, SX(bx, -GW), SY(-GW, z), SX(bx, GW), SY(GW, z)); }
            // poste lejano y travesaño
            double k = socK(-GW);
            lines(pc, 6 * k, true, SX(fx, -GW), SY(-GW, 0), SX(fx, -GW), SY(-GW, GH));
            lines(pc, 6 * socK(0), true, SX(fx, -GW), SY(-GW, GH), SX(fx, GW), SY(GW, GH));
        }
        static void drawPost(int sd, double py)
        {
            double fx = sd * FL, bx = sd * (FL + GD), k = socK(py); bool dk = CANCHAS[soc.field].heavy; Color net = dk ? U.RGBA(90, 90, 130, 0.22) : U.RGBA(255, 255, 255, 0.22);
            fillPts(net, SX(fx, py), SY(py, 0), SX(bx, py), SY(py, 0), SX(bx, py), SY(py, GH), SX(fx, py), SY(py, GH));
            lines(dk ? C("#5a5a78") : Color.White, 6 * k, true, SX(fx, py), SY(py, 0), SX(fx, py), SY(py, GH));
        }
        static void drawSPl(SPl p)
        {
            var s = soc; var ch = p.ch; double k = socK(p.y), x = SX(p.x, p.y), y = SY(p.y, 0);
            if ((s.phase == "goal" && p.team == s.lastGoalTeam) || (s.phase == "end" && s.score[p.team] > s.score[1 - p.team])) y -= Math.Abs(Math.Sin(T * 9 + p.slot * 1.3)) * 26 * k;
            double sc = k * (((ch.kind == "bowser" && !ch.junior) || (ch.kind == "dk" && ch.cap.IsEmpty) || ch.big) ? 1.15 : (ch.kind == "toad" || ch.junior || ch.kind == "shyguy") ? 0.9 : 1) * (ch.kind == "person" || ch.kind == "saiyan" ? ch.tall : 1);
            Color c1 = U.First(ch.shirt, ch.dress, ch.vest, ch.body, ch.hood, ch.fur, ch.over);
            Color c2 = U.First(ch.over, ch.body, ch.fur, ch.hood, ch.dress, ch.shirt);
            if (ch.kind == "person") c2 = U.C("#2a3a6a");
            Color skin = U.First(ch.skin, ch.body, ch.fur);
            double spd = hyp(p.vx, p.vy), ph = p.run; int dir = Math.Cos(p.face) >= 0 ? 1 : -1; bool moving = spd > 25 && p.stun <= 0 && p.frozen <= 0;
            double oa = ALPHA;
            var st = Save();
            if (p.slide > 0 || p.stun > 0.3) { double py = y - 12 * sc; Translate(x, py); Rotate((p.slide > 0 ? -dir : dir) * 1.2); Translate(-x, -py); }
            double hip = y - 24 * sc;
            if (ch.kind != "boo")
                for (int l = -1; l <= 1; l += 2)
                {
                    double a = ph + (l > 0 ? Math.PI : 0), sw = moving ? Math.Sin(a) * 9 * sc : 0;
                    if (p.kickAnim > 0 && l == 1) sw = 15 * sc;
                    double fx = x + l * 5 * sc + sw * dir, fy = y - (moving ? Math.Max(0, -Math.Cos(a)) * 6 * sc : 0);
                    lines(c2, 8 * sc, true, x + l * 5 * sc, hip, fx, fy - 4 * sc);
                    ell(fx + dir * 3 * sc, fy - 3 * sc, 7 * sc, 4.5 * sc, ch.kind == "person" ? U.C("#222222") : ch.kind == "saiyan" ? (ch.hairStyle == "vegeta" ? Color.White : ch.vest) : U.C("#6a3a1a"));
                }
            double bw = 28 * sc, bh = 28 * sc, by = hip - bh + 6 * sc;
            if (ch.kind == "koopa" || ch.kind == "bowser") ell(x - dir * bw * 0.45, by + bh * 0.5, bw * 0.38, bh * 0.55, ch.shell);
            if (ch.kind == "princess") fillPts(ch.dress, x - bw * 0.3, by + 4 * sc, x + bw * 0.3, by + 4 * sc, x + bw * 0.62, hip + 8 * sc, x - bw * 0.62, hip + 8 * sc);
            if (ch.kind == "boo") ell(x, by + bh * 0.4, bw * 0.55, bh * 0.75, U.C("#f4f4ff"));
            else rrFill(x - bw / 2, by, bw, bh, bw * 0.35, c1);
            if (ch.kind == "plumber") { rect(x - bw * 0.4, by + bh * 0.5, bw * 0.8, bh * 0.5, ch.over); rect(x - bw * 0.3, by + 2 * sc, bw * 0.12, bh * 0.5, ch.over); rect(x + bw * 0.18, by + 2 * sc, bw * 0.12, bh * 0.5, ch.over); }
            // banda del equipo
            rect(x - bw * 0.5, by + bh * 0.3, bw, 5 * sc, TEAM_COL[p.team]);
            // brazos (los arqueros con guantes)
            double asw = moving ? Math.Sin(ph) * 7 * sc : 0, ar = (p.gk ? 7 : 5) * sc, ay = by + bh * 0.4;
            if (p.gk && (p.dive > 0 || s.ball.owner == p)) ay = by - 2 * sc;
            circ(x - bw * 0.58 - asw * dir, ay, ar, p.gk ? Color.White : skin);
            circ(x + bw * 0.58 + asw * dir, ay, ar, p.gk ? Color.White : skin);
            double hr = 20 * sc * (ch.kind == "bowser" || ch.big ? 1.1 : 1);
            if (p.frozen > 0) ALPHA = oa * 0.85;
            drawHead(p.pwT > 0 && PowerOf(ch)[0] == "ssj" ? ssjOf(ch) : ch, x, by - hr * 0.72, hr);
            Restore(st); ALPHA = oa;
            double top = by - hr * 2;
            if (p.frozen > 0) rrFill(x - 22 * sc, top, 44 * sc, y - top, 6 * sc, U.RGBA(190, 240, 255, 0.5));
            if (p.stun > 0 && p.frozen <= 0) for (int i = 0; i < 3; i++) drawStar(x + Math.Cos(T * 6 + i * 2.1) * 16 * sc, top + Math.Sin(T * 6 + i * 2.1) * 4 * sc, 5 * sc);
            if (p.pwT > 0) { var PW = PowerOf(ch); ellStroke(x, y - 30 * sc, 30 * k, 40 * k, U.A(PW_ICON[PW[0]], 0.5 + Math.Sin(T * 20) * 0.3), 3); }
            if (p.human >= 0 && !s.allAI)
            {
                Color pc = s.np > 1 ? P_COL[p.human] : Y_; double my = top - 10 + Math.Sin(T * 6) * 3;
                fillPts(pc, x - 8, my - 10, x + 8, my - 10, x, my);
                if (s.np > 1) txt("J" + (p.human + 1), x, my - 20, 13, pc, "center", Color.Black, 3);
                if (p.charge >= 0) { double c = Math.Min(1, p.charge); rrFill(x - 22, my - 36, 44, 8, 3, U.RGBA(0, 0, 0, 0.6)); rrFill(x - 21, my - 35, 42 * c, 6, 3, U.Hsl(120 - c * 120, 0.9, 0.5)); }
            }
        }
        static void drawSBall()
        {
            var b = soc.ball; double k = socK(b.y), r = BR * k * (b.sup == "giant" && b.superT > 0 ? 2 : 1);
            double x = SX(b.x, b.y), y = SY(b.y, b.z + BR * (r / (BR * k)));
            double oa = ALPHA;
            for (int i = 0; i < b.trail.Count; i++)
            {
                var t = b.trail[i]; double a = (i + 1) / (double)b.trail.Count;
                ALPHA = oa * a * 0.6; circ(SX(t[0], t[1]), SY(t[1], t[2] + BR), r * (0.5 + 0.5 * a), b.supCol);
            }
            ALPHA = oa * (b.sup == "ghost" && b.superT > 0 ? 0.3 : 1);
            if (b.superT > 0) circ(x, y, r * 1.5, U.A(b.supCol, 0.35));
            circ(x, y, r, Color.White);
            for (int i = 0; i < 3; i++) { double a = b.spin + i * 2.1; circ(x + Math.Cos(a) * r * 0.5, y + Math.Sin(a) * r * 0.4, r * 0.27, U.C("#222222")); }
            ellStroke(x, y, r, r, U.C("#333333"), 1.2);
            ALPHA = oa;
        }
        static void drawSoccerBall(double x, double y, double r)
        {
            circ(x, y, r, Color.White);
            for (int i = 0; i < 5; i++) { double a = i * Math.PI * 2 / 5 - Math.PI / 2; circ(x + Math.Cos(a) * r * 0.62, y + Math.Sin(a) * r * 0.62, r * 0.2, U.C("#222222")); }
            circ(x, y, r * 0.28, U.C("#222222"));
            ellStroke(x, y, r, r, U.C("#333333"), 1.5);
        }
        static string socClock(double secs) { int t = (int)Math.Ceiling(Math.Max(0, secs)); return (t / 60) + ":" + (t % 60).ToString("00"); }
        static void drawSocHUD()
        {
            var s = soc; var cf = CANCHAS[s.field];
            // marcador
            rrFill(W / 2.0 - 175, 8, 350, 62, 14, U.RGBA(0, 0, 0, 0.6));
            rrFill(W / 2.0 - 171, 12, 60, 54, 10, U.A(TEAM_COL[0], 0.85)); rrFill(W / 2.0 + 111, 12, 60, 54, 10, U.A(TEAM_COL[1], 0.85));
            drawHead(s.cap[0], W / 2.0 - 141, 40, 19); drawHead(s.cap[1], W / 2.0 + 141, 40, 19);
            txt(TEAM_NAME[0], W / 2.0 - 75, 24, 12, TEAM_COL[0], "center", Color.Black, 3); txt(TEAM_NAME[1], W / 2.0 + 75, 24, 12, TEAM_COL[1], "center", Color.Black, 3);
            txt(s.score[0] + "  -  " + s.score[1], W / 2.0, 34, 32, W_, "center", Color.Black, 5);
            string clk = s.golden ? "GOL DE ORO" : socClock(s.dur - s.clock);
            txt(clk, W / 2.0, 59, 14, s.golden ? Y_ : (s.dur - s.clock < 10 && s.phase == "play" ? C("#ff6a6a") : W_), "center", Color.Black, 3);
            // barras de súper tiro
            for (int t = 0; t < 2; t++)
            {
                double bx = t == 0 ? 16 : W - 236, by = H - 34; bool full = s.meter[t] >= 1;
                var who = s.ctl[t] ?? s.pl.First(p => p.team == t && p.slot == 0); var PW = PowerOf(who.ch);
                string label = (humanTeam(t) ? (s.np > 1 ? "J" + (t + 1) + " " : "") : "CPU ") + "SÚPER: " + PW[1];
                txt(label, bx + (t == 0 ? 0 : 220), by - 14, 13, PW_ICON[PW[0]], t == 0 ? "left" : "right", Color.Black, 3);
                rrFill(bx, by, 220, 18, 8, U.RGBA(0, 0, 0, 0.6));
                rrFill(bx + 2, by + 2, 216 * s.meter[t], 14, 7, full ? U.Hsl((T * 300) % 360, 0.9, 0.6) : TEAM_COL[t]);
                if (full && humanTeam(t)) { string key = s.np > 1 ? (t == 0 ? "R" : "L") : "C"; if (((int)Math.Floor(T * 4)) % 2 == 0) txt("¡LISTO! pulsa " + key, bx + 110, by + 9, 12, W_, "center", Color.Black, 3); }
            }
            // minimapa
            double mw = 150, mh = 82, mx = W / 2.0 - mw / 2, my = H - mh - 10;
            rrFill(mx - 5, my - 5, mw + 10, mh + 10, 8, U.RGBA(0, 0, 0, 0.5)); rect(mx, my, mw, mh, U.A(cf.g1, 0.9));
            rectStroke(mx, my, mw, mh, U.A(cf.line, 0.8), 1); lines(U.A(cf.line, 0.8), 1, false, mx + mw / 2, my, mx + mw / 2, my + mh);
            double gy = mh * GW / (2 * FW);
            rect(mx - 4, my + mh / 2 - gy, 4, gy * 2, Color.White); rect(mx + mw, my + mh / 2 - gy, 4, gy * 2, Color.White);
            foreach (var p in s.pl)
            {
                double px = mx + (p.x + FL) / (2 * FL) * mw, py = my + (p.y + FW) / (2 * FW) * mh;
                if (p.human >= 0 && !s.allAI) circ(px, py, 5, s.np > 1 ? P_COL[p.human] : Y_);
                circ(px, py, 3.5, TEAM_COL[p.team]);
            }
            { var b = s.ball; circ(mx + U.Clamp((b.x + FL) / (2 * FL), -0.03, 1.03) * mw, my + (b.y + FW) / (2 * FW) * mh, 2.8, Color.White); }
            // mensajes grandes
            if (s.msgT > 0)
            {
                double age = s.msgMax - s.msgT, pop = age < 0.2 ? 0.6 + age * 2 : 1, oa = ALPHA; ALPHA = Math.Min(1, s.msgT * 3);
                txt(s.msg, W / 2.0, H / 2.0 - 70, 54 * pop, s.msgCol, "center", Color.Black, 10);
                if (s.phase == "goal" && s.msg.StartsWith("¡GO")) { var sc = s.ball.last; if (sc != null) txt(sc.ch.name + (sc.team != s.lastGoalTeam ? " (en contra)" : ""), W / 2.0, H / 2.0 - 20, 24, W_, "center", Color.Black, 5); }
                ALPHA = oa;
            }
            if (s.phase == "count")
            {
                int nn = (int)Math.Ceiling(s.phaseT);
                if (nn <= 3) txt(nn.ToString(), W / 2.0, H / 2.0 - 70, 90, Y_, "center", Color.Black, 12);
                drawSocControls(H / 2.0 + 10);
            }
            if (s.flash > 0) rect(0, 0, W, H, U.RGBA(255, 255, 255, Math.Min(1, s.flash * 3)));
        }
        static void drawSocControls(double y)
        {
            bool duo = soc.np > 1;
            panel(150, y, 660, duo ? 112 : 96, 0.7, Color.Empty);
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
        static void drawSocPause()
        {
            dim(0.55); panel(330, 110, 300, 240, 0.85, Color.Empty); txt("PAUSA", W / 2.0, 150, 40, Y_);
            var o = new[] { "Continuar", "Reiniciar partido", "Salir al menú" };
            for (int i = 0; i < 3; i++) txt((menu.p == i ? "> " : "") + o[i], W / 2.0, 210 + i * 46, 24, menu.p == i ? Y_ : W_);
            drawSocControls(380);
        }
        static void drawSocRes()
        {
            var s = soc; dim(0.55);
            int w = s.score[0] > s.score[1] ? 0 : s.score[1] > s.score[0] ? 1 : -1;
            string title = w < 0 ? "¡EMPATE!" : s.np > 1 ? "¡GANA J" + (w + 1) + "!" : w == 0 ? "¡GANASTE EL PARTIDO!" : "¡PERDISTE! Inténtalo otra vez";
            txt(title, W / 2.0, 52, w == 1 && s.np == 1 ? 34 : 44, w < 0 ? W_ : TEAM_COL[w], "center", Color.Black, 8);
            panel(130, 92, 700, 350, 0.8, Color.Empty);
            for (int t = 0; t < 2; t++)
            {
                double cx = t == 0 ? 290 : 670;
                drawHead(s.cap[t], cx, 160 + (w == t ? Math.Sin(T * 6) * 5 : 0), 44);
                txt("EQUIPO " + TEAM_NAME[t] + (humanTeam(t) ? (s.np > 1 ? " (J" + (t + 1) + ")" : " (TÚ)") : ""), cx, 228, 18, TEAM_COL[t], "center", Color.Black, 4);
                var mates = s.pl.Where(p => p.team == t).OrderBy(p => p.slot).ToList();
                for (int i = 1; i < mates.Count; i++) drawHead(mates[i].ch, cx - 60 + (i - 1) * 60, 265, 15);
                int row = 0;
                foreach (var g in s.log.Where(l => l[0] == t.ToString()).Take(5)) { drawSoccerBall(cx - 80, 302 + row * 24, 7); txt(g[1] + "  " + g[2], cx - 66, 302 + row * 24, 14, W_, "left", Color.Black, 3); row++; }
            }
            txt(s.score[0] + " - " + s.score[1], W / 2.0, 170, 64, W_, "center", Color.Black, 8);
            if (s.golden && s.endPending) txt("¡con gol de oro!", W / 2.0, 220, 16, Y_, "center", Color.Black, 3);
            if (s.np == 1 && !s.allAI)
            {
                int gg, pp, ee; int.TryParse(store("futbol_g") ?? "0", out gg); int.TryParse(store("futbol_p") ?? "0", out pp); int.TryParse(store("futbol_e") ?? "0", out ee);
                txt("Tus partidos:  " + gg + " ganados  -  " + ee + " empatados  -  " + pp + " perdidos", W / 2.0, 422, 14, C("#dddddd"), "center", Color.Black, 3);
            }
            if (w == 0 || (w == 1 && s.np > 1)) for (int i = 0; i < 2; i++) if (U.R() < 0.5) addPart(U.R() * W, -10, (U.R() - 0.5) * 60, 60 + U.R() * 80, 5, U.Hsl(U.R() * 360, 0.9, 0.6), 7, 30);
            if (((int)Math.Floor(T * 2)) % 2 == 0) hint("ENTER: revancha   -   ESC: volver al menú");
        }
        static void socInput(bool up, bool dn, bool ok, bool bk)
        {
            switch (state)
            {
                case "soccer": if (kp("Escape", "KeyP")) { state = "spause"; menu.p = 0; SFX.sel(); } break;
                case "spause":
                    if (up) { menu.p = (menu.p + 2) % 3; SFX.sel(); }
                    if (dn) { menu.p = (menu.p + 1) % 3; SFX.sel(); }
                    if (kp("Escape", "KeyP")) { state = "soccer"; break; }
                    if (ok) { if (menu.p == 0) state = "soccer"; else if (menu.p == 1) startSoccer(socTeams()); else { soc = null; toMenu(); } }
                    break;
                case "sres":
                    if (soc.resT < 1) break;
                    if (kp("Enter")) { SFX.ok(); startSoccer(socTeams()); }
                    else if (bk) { SFX.back(); soc = null; toMenu(); }
                    break;
            }
        }
    }
}
