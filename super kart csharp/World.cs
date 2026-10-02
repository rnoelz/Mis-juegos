using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SuperKart
{
    // ---- piezas del mundo ----
    public class Pt { public double wy, wz, cx, cy, cz, scale, sx, sy, sw; }
    public class Decor { public string t; public double x, h; public int dir; }
    public class Coin { public double x, taken; }
    public class Box { public double x, t; }
    public class Ent { public double z, x; }
    public class Seg
    {
        public int index; public Pt p1 = new Pt(), p2 = new Pt(); public double curve;
        public List<Decor> decor = new List<Decor>(); public List<Coin> coins = new List<Coin>(); public List<Box> boxes = new List<Box>();
        public List<Ent> dyn = new List<Ent>(); public double clip; public bool vis; public double pad = double.NaN; public bool ramp;
    }
    public class Scr { public double x = 480, y = 460, w = 130; }
    public class AI { public double t, tx, skill, mul = 1, useT = 2, pwWait = double.NaN; }
    public class Racer : Ent
    {
        public Ch ch; public Kt kt; public St st; public bool isPlayer; public int pidx;
        public string msg = ""; public double msgT, shake, startPress = -1, skyOff; public Scr scr = new Scr();
        public double speed; public int lap, maxLap, coins; public string item, pending; public int itemCount; public double roulette;
        public double boost, spin, star, shrink, bullet, invul, hop, goldenT, ghostT, ink, hornT, air, airMax = 1, trickT; public bool trick;
        public List<double> lapTimes = new List<double>(); public double lapStart;
        public double meter, pwT, frozenT, laserT, teleT; public string pwType = "";   // súper poder
        public int drift, driftLvl; public double driftCharge, steerVis, rescue;
        public bool finished; public double finishTime; public int place = 1, lastPlace = 1; public double placeT;
        public double top, accR, han, wgt; public AI ai = new AI();
    }
    public class Proj : Ent { public string type; public Racer owner, target; public double speed, vx, life, age; public Color col; }
    public class Hazard : Ent { public string type; public double ph, sp, amp, dead, h, speed; public Color col; }

    static partial class SK
    {
        public const double SEG = 200, ROADW = 2000, CAMH = 1000, FOV = 100;
        public const int RUMBLE = 3, DRAW = 280, LAPS = 3;
        public static readonly double DEPTH = 1 / Math.Tan(FOV / 2 * Math.PI / 180);
        public static readonly double PLAYERZ = CAMH * DEPTH * 1.4;
        public const double MAXS = SEG * 60, KARTW = 400, CENTRI = 0.15;

        public static List<Seg> segments = new List<Seg>();
        public static double trackLen;
        public static Theme theme;
        public static List<Coin> allCoins = new List<Coin>();
        public static List<Box> allBoxes = new List<Box>();
        public static List<Hazard> hazards = new List<Hazard>();
        public static PointF[] map;
        public static Bitmap layerFar, layerNear; static string layerKey = "";
        static double layerScale = 1; static int layerSeed; static Theme layerTheme;
        public static double SCALE = 1, VW = W, VX = 0;
        static List<Seg> dynSegs = new List<Seg>();

        // ---------------- construcción de pista ----------------
        static double lastY() { return segments.Count > 0 ? segments[segments.Count - 1].p2.wy : 0; }
        static void addSeg(double curve, double y)
        {
            int n = segments.Count;
            var s = new Seg { index = n, curve = curve };
            s.p1.wy = lastY(); s.p1.wz = n * SEG; s.p2.wy = y; s.p2.wz = (n + 1) * SEG;
            segments.Add(s);
        }
        static void addRoad(int e, int h, int l, double curve, double hill)
        {
            double sy = lastY(), ey = sy + hill * SEG; int t = e + h + l;
            for (int n = 0; n < e; n++) addSeg(U.EaseIn(0, curve, (double)n / e), U.EaseInOut(sy, ey, (double)n / t));
            for (int n = 0; n < h; n++) addSeg(curve, U.EaseInOut(sy, ey, (double)(e + n) / t));
            for (int n = 0; n < l; n++) addSeg(U.EaseInOut(curve, 0, (double)n / l), U.EaseInOut(sy, ey, (double)(e + h + n) / t));
        }
        static void buildTrack(int ti, bool mirror)
        {
            var tr = TRACKS[ti]; theme = THEMES[tr.theme]; segments = new List<Seg>();
            double f = tr.f, m = mirror ? -1 : 1;
            for (int i = 0; i < tr.pieces.Length; i += 5)
                addRoad((int)Math.Round(tr.pieces[i] * f), (int)Math.Round(tr.pieces[i + 1] * f), (int)Math.Round(tr.pieces[i + 2] * f), tr.pieces[i + 3] * m, tr.pieces[i + 4]);
            double y = lastY(); if (Math.Abs(y) > 1) addRoad(30, 20, 30, 0, -y / SEG);
            addRoad(0, 40, 0, 0, 0);
            trackLen = segments.Count * SEG;
            var rnd = U.Mulberry((uint)(ti * 977 + 31)); int L = segments.Count;
            allCoins = new List<Coin>(); allBoxes = new List<Box>();
            Func<string, bool> far = t => t == "pyramid" || t == "building";
            for (int i = 10; i < L; i++)
            {
                var s = segments[i];
                if (i % 7 == 0) { int side = rnd() < 0.5 ? -1 : 1; string t = U.Pick(theme.decor, rnd); s.decor.Add(new Decor { t = t, x = side * (far(t) ? 2.6 + rnd() * 2 : 1.4 + rnd() * 2.2), h = rnd() }); }
                if (i % 13 == 0 && rnd() < 0.7) { string t = U.Pick(theme.decor, rnd); s.decor.Add(new Decor { t = t, x = (rnd() < 0.5 ? -1 : 1) * (far(t) ? 3 + rnd() * 2 : 1.5 + rnd() * 2.5), h = rnd() }); }
                if (Math.Abs(s.curve) > 2.5 && i % 18 == 0) s.decor.Add(new Decor { t = "sign", x = -Math.Sign(s.curve) * 1.3, dir = Math.Sign(s.curve) });
            }
            for (int i = 80; i < L - 60; i += (int)Math.Floor(90 + rnd() * 70))
            {
                double off = rnd() * 1.4 - 0.7, dir = rnd() < 0.5 ? 0 : (rnd() * 0.06 - 0.03);
                for (int k = 0; k < 6; k++) { var c = new Coin { x = U.Clamp(off + dir * k * 3, -0.85, 0.85) }; segments[i + k * 3].coins.Add(c); allCoins.Add(c); }
            }
            foreach (double fr in new[] { 0.1, 0.33, 0.57, 0.8 })
            {
                var s = segments[(int)Math.Floor(L * fr)];
                foreach (double bx in new[] { -0.66, -0.22, 0.22, 0.66 }) { var b = new Box { x = bx }; s.boxes.Add(b); allBoxes.Add(b); }
            }
            // turbos en el piso y rampas
            int ramps = 0;
            for (int i = 120; i < L - 80; i += (int)Math.Floor(110 + rnd() * 90))
            {
                bool ok = true;
                for (int k = -6; k < 8; k++) { var q = segments[i + k]; if (Math.Abs(q.curve) > 1.5 || q.boxes.Count > 0) ok = false; }
                if (!ok) continue;
                if (rnd() < 0.3 && ramps < 3) { segments[i].ramp = segments[i + 1].ramp = true; ramps++; }
                else { double x = rnd() * 1.2 - 0.6; for (int k = 0; k < 4; k++) segments[i + k].pad = x; }
            }
            // peligros en la pista
            hazards = new List<Hazard>();
            string hz;
            if (HAZARD.TryGetValue(tr.theme, out hz))
            {
                int n = hz == "car" ? 10 : 8;
                for (int i = 0; i < n; i++)
                {
                    double z = SEG * (150 + (i + rnd() * 0.5) * (L - 260) / n);
                    if (hz == "car") hazards.Add(new Hazard { type = "car", z = z, x = U.Pick(new[] { -0.55, 0, 0.55 }, rnd), speed = MAXS * (0.42 + rnd() * 0.15), col = U.C(U.Pick(new[] { "#e83a3a", "#3a8aff", "#ffd23a", "#3ad88a", "#ff7ad8" }, rnd)) });
                    else hazards.Add(new Hazard { type = hz, z = z, ph = rnd() * 6.28, sp = 0.6 + rnd() * 0.7, amp = 0.75 });
                }
            }
        }
        static double wrapZ(double z) { return ((z % trackLen) + trackLen) % trackLen; }
        static double wrapD(double d) { d = ((d % trackLen) + trackLen) % trackLen; return d > trackLen / 2 ? d - trackLen : d; }
        static double fwd(double a, double b) { return ((b - a) % trackLen + trackLen) % trackLen; }
        static Seg findSeg(double z) { return segments[(int)Math.Floor(wrapZ(z) / SEG) % segments.Count]; }
        static double prog(Racer r) { return r.lap * trackLen + r.z; }

        static void computeMap()
        {
            double S = 0; foreach (var s in segments) S += s.curve;
            double K = Math.Abs(S) > 150 ? Math.PI * 2 / S : 0.004;
            double a = -Math.PI / 2, x = 0, y = 0; var p = new List<double[]>();
            foreach (var s in segments) { a += s.curve * K; x += Math.Cos(a); y += Math.Sin(a); p.Add(new[] { x, y }); }
            int N = p.Count; double ex = p[N - 1][0], ey = p[N - 1][1];
            for (int i = 0; i < N; i++) { p[i][0] -= ex * (i + 1) / N; p[i][1] -= ey * (i + 1) / N; }
            double mnx = 1e9, mny = 1e9, mxx = -1e9, mxy = -1e9;
            foreach (var q in p) { mnx = Math.Min(mnx, q[0]); mny = Math.Min(mny, q[1]); mxx = Math.Max(mxx, q[0]); mxy = Math.Max(mxy, q[1]); }
            double sc = Math.Min(136 / Math.Max(1e-6, mxx - mnx), 136 / Math.Max(1e-6, mxy - mny));
            double ox = (156 - (mxx - mnx) * sc) / 2, oy = (156 - (mxy - mny) * sc) / 2;
            map = new PointF[N];
            for (int i = 0; i < N; i++) map[i] = new PointF(F(ox + (p[i][0] - mnx) * sc), F(oy + (p[i][1] - mny) * sc));
        }

        // ---------------- fondos (parallax) ----------------
        static void makeLayers(Theme th, int seed)
        {
            var rnd = U.Mulberry((uint)seed); const int LW = 1920, LH = 270;
            double LS = Math.Min(3, Math.Max(0.5, SCALE));   // misma escala que la pantalla para copiarlo 1:1
            layerScale = LS; layerSeed = seed; layerTheme = th;
            if (layerFar != null) { layerFar.Dispose(); layerNear.Dispose(); }
            layerFar = new Bitmap((int)(LW * LS), (int)(LH * LS), PixelFormat.Format32bppPArgb);
            layerNear = new Bitmap((int)(LW * LS), (int)(LH * LS), PixelFormat.Format32bppPArgb);
            var old = g; double oldA = ALPHA; ALPHA = 1;
            var gf = Graphics.FromImage(layerFar); var gn = Graphics.FromImage(layerNear);
            foreach (var gg in new[] { gf, gn }) { gg.SmoothingMode = SmoothingMode.AntiAlias; gg.ScaleTransform(F(LS), F(LS)); gg.Clear(Color.Transparent); }
            Action<Graphics, Action<double>> rep = (gg, fn) => { g = gg; foreach (double o in new double[] { -LW, 0, LW }) fn(o); };
            Action<Graphics, double, double, double> cloud = (gg, x, y, s) => rep(gg, o => { foreach (var q in new[] { new[] { 0, 0, 1.0 }, new[] { -0.9, 0.25, 0.7 }, new[] { 0.9, 0.25, 0.75 }, new[] { 0.4, -0.35, 0.7 } }) circ(x + o + q[0] * s, y + q[1] * s, q[2] * s, U.RGBA(255, 255, 255, 0.92)); });
            Action<Graphics, Color, double, double, int, bool> wave = (gg, col, bse, amp, n2, sharp) =>
            {
                g = gg; var ks = new List<double[]>();
                for (int i = 0; i < n2; i++) ks.Add(new[] { (1 + Math.Floor(rnd() * 5)) * (i + 1), 1.0 / (i + 1), rnd() * 6.28 });
                var xy = new List<double> { 0, LH };
                for (int x = 0; x <= LW; x += 6)
                {
                    double yy = 0, tot = 0;
                    foreach (var k in ks) { double s = Math.Sin((double)x / LW * Math.PI * 2 * k[0] + k[2]); yy += k[1] * (sharp ? 1 - Math.Abs(s) : (s + 1) / 2); tot += k[1]; }
                    xy.Add(x); xy.Add(bse - amp * yy / tot);
                }
                xy.Add(LW); xy.Add(LH);
                fillPts(col, xy.ToArray());
            };
            switch (th.bg)
            {
                case "hills":
                    wave(gf, th.far, LH - 20, 120, 3, true);
                    for (int i = 0; i < 8; i++) cloud(gf, rnd() * LW, 30 + rnd() * 70, 14 + rnd() * 12);
                    for (int i = 0; i < 12; i++)
                    {
                        double x = rnd() * LW, r = 50 + rnd() * 80;
                        rep(gn, o => { halfEll(x + o, LH + 5, r, r, th.near); rect(x + o - r * 0.22, LH - r * 0.62, 4, 11, K_); rect(x + o + r * 0.12, LH - r * 0.62, 4, 11, K_); });
                    }
                    break;
                case "ocean":
                    for (int i = 0; i < 8; i++) cloud(gf, rnd() * LW, 30 + rnd() * 80, 14 + rnd() * 12);
                    for (int i = 0; i < 4; i++) { double x = rnd() * LW, w = 90 + rnd() * 80; rep(gf, o => ell(x + o, LH - 28, w, 40, U.C("#3a9a5a"))); }
                    g = gf; rect(0, LH - 28, LW, 28, th.far);
                    g = gn; rect(0, LH - 14, LW, 14, th.near);
                    for (int i = 0; i < 70; i++) rect(rnd() * LW, LH - 13 + rnd() * 11, 20, 2, U.RGBA(255, 255, 255, 0.6));
                    break;
                case "dunes":
                    g = gf; circ(LW * 0.3, 90, 50, U.C("#fff4c0"));
                    for (int i = 0; i < 5; i++)
                    {
                        double x = rnd() * LW, s = 60 + rnd() * 90;
                        rep(gf, o => { fillPts(th.far, x + o - s, LH, x + o, LH - s, x + o + s, LH); fillPts(U.RGBA(0, 0, 0, 0.15), x + o, LH - s, x + o + s, LH, x + o + s * 0.2, LH); });
                    }
                    wave(gn, th.near, LH + 5, 45, 2, false);
                    break;
                case "mountains":
                    {
                        // montañas con nieve en la punta
                        var tmp = new Bitmap(layerFar.Width, layerFar.Height, PixelFormat.Format32bppPArgb);
                        var gt = Graphics.FromImage(tmp); gt.SmoothingMode = SmoothingMode.AntiAlias; gt.ScaleTransform(F(LS), F(LS));
                        wave(gt, th.far, LH, 210, 3, true);
                        gf.DrawImageUnscaled(tmp, 0, 0);
                        g = gf;
                        using (var region = new Region(new RectangleF(0, 0, LW, LH - 150)))
                        {
                            // pinta de blanco solo donde hay montaña y arriba de cierta altura
                            for (int yy = 0; yy < (LH - 150); yy += 2) { }
                        }
                        var gfx2 = Graphics.FromImage(tmp); gfx2.CompositingMode = CompositingMode.SourceOver;
                        // técnica: dibujar la parte alta de las montañas con color blanco usando la máscara de la propia imagen
                        var snow = new Bitmap(tmp.Width, tmp.Height, PixelFormat.Format32bppPArgb);
                        var data = tmp.LockBits(new Rectangle(0, 0, tmp.Width, tmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
                        var sd = snow.LockBits(new Rectangle(0, 0, snow.Width, snow.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
                        int limit = (int)((LH - 150) * LS);
                        var row = new int[tmp.Width];
                        for (int yy = 0; yy < limit && yy < tmp.Height; yy++)
                        {
                            System.Runtime.InteropServices.Marshal.Copy(data.Scan0 + yy * data.Stride, row, 0, tmp.Width);
                            for (int xx = 0; xx < tmp.Width; xx++) { int a = (row[xx] >> 24) & 255; row[xx] = a == 0 ? 0 : (a << 24) | (a << 16) | (a << 8) | a; }
                            System.Runtime.InteropServices.Marshal.Copy(row, 0, sd.Scan0 + yy * sd.Stride, tmp.Width);
                        }
                        tmp.UnlockBits(data); snow.UnlockBits(sd);
                        gf.ResetTransform(); gf.DrawImageUnscaled(snow, 0, 0); gf.ScaleTransform(F(LS), F(LS));
                        snow.Dispose(); gfx2.Dispose(); gt.Dispose(); tmp.Dispose();
                        for (int i = 0; i < 6; i++) cloud(gf, rnd() * LW, 30 + rnd() * 50, 12 + rnd() * 12);
                        for (int i = 0; i < 22; i++) { double x = rnd() * LW, s = 20 + rnd() * 25; rep(gn, o => fillPts(U.C("#2a5a4a"), x + o - s * 0.6, LH, x + o, LH - s * 2, x + o + s * 0.6, LH)); }
                        break;
                    }
                case "castle":
                    rep(gf, o =>
                    {
                        fillPts(U.C("#3a1414"), o + 500, LH, o + 760, LH - 190, o + 840, LH - 190, o + 1100, LH);
                        fillPts(U.C("#ff5a1a"), o + 760, LH - 190, o + 840, LH - 190, o + 815, LH - 120, o + 790, LH - 145);
                        circ(o + 800, LH - 200, 45, U.RGBA(255, 120, 30, 0.35));
                    });
                    wave(gf, th.far, LH, 60, 3, true);
                    for (int i = 0; i < 3; i++)
                    {
                        double x = 200 + i * 620 + rnd() * 100;
                        rep(gf, o =>
                        {
                            double X = x + o; Color cc = U.C("#1a0808");
                            rect(X - 60, LH - 130, 120, 130, cc); rect(X - 90, LH - 170, 40, 170, cc); rect(X + 50, LH - 170, 40, 170, cc);
                            for (int j = 0; j < 3; j++) { rect(X - 90 + j * 14, LH - 182, 8, 12, cc); rect(X + 50 + j * 14, LH - 182, 8, 12, cc); }
                            rect(X - 10, LH - 100, 20, 26, U.C("#ffb02a"));
                        });
                    }
                    g = gf; for (int i = 0; i < 60; i++) circ(rnd() * LW, rnd() * LH * 0.8, 1 + rnd() * 2, U.RGBA(255, 140, 40, 0.7));
                    wave(gn, th.near, LH + 10, 50, 3, true);
                    break;
                case "jungle":
                    wave(gf, th.far, LH - 10, 150, 3, false);
                    g = gf; rect(0, LH - 70, LW, 70, U.RGBA(255, 255, 255, 0.25));
                    wave(gn, th.near, LH + 10, 90, 3, false);
                    for (int i = 0; i < 40; i++) { double x = rnd() * LW, r = 20 + rnd() * 30, y = LH - 40 - rnd() * 50; int ii = i; rep(gn, o => circ(x + o, y, r, ii % 2 == 1 ? U.C("#2a7a2a") : U.C("#1a5a1f"))); }
                    break;
                case "ghost":
                    g = gf; circ(LW * 0.7, 70, 46, U.C("#fff6c8")); circ(LW * 0.7, 70, 60, U.RGBA(255, 246, 200, 0.15));
                    for (int i = 0; i < 5; i++) { double x = rnd() * LW, y = 30 + rnd() * 70, s = 20 + rnd() * 20; rep(gf, o => ell(x + o, y, s * 2.4, s * 0.5, U.RGBA(120, 80, 160, 0.5))); }
                    rep(gf, o =>
                    {
                        double X = o + 600; Color cc = U.C("#120818");
                        rect(X, LH - 150, 380, 150, cc); rect(X - 40, LH - 210, 90, 210, cc); rect(X + 330, LH - 230, 90, 230, cc);
                        foreach (var q in new[] { new[] { X - 40, 90, LH - 210 }, new[] { X + 330, 90, LH - 230 }, new[] { X + 120, 140, LH - 150 } })
                            fillPts(cc, q[0] - 10, q[2], q[0] + q[1] / 2, q[2] - 70, q[0] + q[1] + 10, q[2]);
                        for (int a = 0; a < 5; a++) for (int b2 = 0; b2 < 3; b2++) if ((a * 7 + b2 * 3) % 4 != 0) rect(X + 40 + a * 65, LH - 125 + b2 * 38, 16, 20, U.C("#ffd84a"));
                    });
                    for (int i = 0; i < 10; i++)
                    {
                        double x = rnd() * LW, h = 40 + rnd() * 60;
                        rep(gn, o => { lines(U.C("#0a0510"), 5, false, x + o, LH, x + o, LH - h, x + o - 20, LH - h - 25); lines(U.C("#0a0510"), 5, false, x + o, LH - h * 0.7, x + o + 25, LH - h - 10); });
                    }
                    break;
                case "city":
                    g = gf; for (int i = 0; i < 80; i++) circ(rnd() * LW, rnd() * LH * 0.5, rnd() + 0.4, U.RGBA(255, 255, 255, 0.7));
                    for (double x = 0; x < LW; x += 40 + Math.Floor(rnd() * 30))
                    {
                        double w = 36 + rnd() * 40, h = 80 + rnd() * 150; rect(x, LH - h, w, h, th.far);
                        for (double wy = LH - h + 8; wy < LH - 8; wy += 14) for (double wx = x + 5; wx < x + w - 8; wx += 10) if (rnd() < 0.45) rect(wx, wy, 5, 7, U.RGBA(255, 220, 120, 0.7));
                    }
                    g = gn;
                    for (double x = 0; x < LW; x += 50 + Math.Floor(rnd() * 40))
                    {
                        double w = 40 + rnd() * 50, h = 40 + rnd() * 80; rect(x, LH - h, w, h, th.near);
                        Color nc = U.C(U.Pick(new[] { "#ff3ad8", "#3affff", "#ffe14a" }, rnd));
                        for (double wy = LH - h + 6; wy < LH - 6; wy += 12) for (double wx = x + 4; wx < x + w - 6; wx += 9) if (rnd() < 0.3) rect(wx, wy, 4, 5, nc);
                    }
                    break;
                case "sky":
                    for (int i = 0; i < 14; i++) cloud(gf, rnd() * LW, 20 + rnd() * 150, 14 + rnd() * 22);
                    wave(gn, W_, LH - 30, 60, 3, false);
                    for (int i = 0; i < 20; i++) { double x = rnd() * LW, r = 25 + rnd() * 35; rep(gn, o => circ(x + o, LH - 40, r, W_)); }
                    break;
                case "mine":
                    g = gf; rect(0, 0, LW, 50, U.C("#120a04"));
                    for (double x = 0; x < LW; x += 20 + Math.Floor(rnd() * 30)) { double h = 20 + rnd() * 80, w = 10 + rnd() * 20; fillPts(U.C("#120a04"), x, 48, x + w / 2, 48 + h, x + w, 48); }
                    wave(gf, th.far, LH, 120, 3, true);
                    g = gf; for (int i = 0; i < 30; i++) circ(rnd() * LW, 60 + rnd() * (LH - 80), 1.5 + rnd() * 2.5, U.C(U.Pick(new[] { "#b06aff", "#4af0ff", "#ffd84a" }, rnd)));
                    wave(gn, th.near, LH + 10, 50, 3, true);
                    break;
                case "space":
                    g = gf;
                    for (int i = 0; i < 260; i++) circ(rnd() * LW, rnd() * LH, rnd() * 1.6 + 0.3, U.RGBA(255, 255, 255, 0.4 + rnd() * 0.6));
                    circ(400, 80, 40, U.C("#e8784a"));
                    { var st = g.Save(); g.TranslateTransform(400, 80); g.RotateTransform(-17); ellStroke(0, 0, 70, 14, U.RGBA(255, 220, 150, 0.8), 4); g.Restore(st); }
                    circ(1300, 140, 25, U.C("#5ab0ff")); circ(1700, 60, 15, U.C("#c0ff8a"));
                    break;
            }
            gf.Dispose(); gn.Dispose();
            g = old; ALPHA = oldA;
        }
        // Copia solo la parte visible del fondo, píxel a píxel (sin estirar la imagen = mucho más rápido)
        static void drawLayer(Bitmap img, double off, double y)
        {
            const double LW = 1920, LH = 270;
            double o = ((off * LW) % LW + LW) % LW;
            float[] e; using (var m = g.Transform) e = m.Elements;
            double s = e[0], tx = e[4], ty = e[5], ls = img.Width / LW;
            var st = g.Save();
            g.ResetTransform();
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            foreach (double cx in new[] { -o, LW - o })
            {
                double v0 = Math.Max(0, cx), v1 = Math.Min(VW, cx + LW);
                if (v1 <= v0) continue;
                var dst = new RectangleF(F(tx + v0 * s), F(ty + y * s), F((v1 - v0) * s), F(LH * s));
                var src = new RectangleF(F((v0 - cx) * ls), 0, F((v1 - v0) * ls), F(LH * ls));
                g.DrawImage(img, dst, src, GraphicsUnit.Pixel);
            }
            g.Restore(st);
        }

        // ---------------- proyección 3D de la carretera ----------------
        static void project(Pt p, double cx, double cy, double cz)
        {
            p.cx = -cx; p.cy = p.wy - cy; p.cz = p.wz - cz;
            p.scale = DEPTH / p.cz;
            p.sx = VW / 2 + p.scale * p.cx * VW / 2;
            p.sy = H / 2.0 - p.scale * p.cy * H / 2;
            p.sw = p.scale * ROADW * VW / 2;
        }
        struct SP { public double sx, sy, k; }
        static SP segPt(Seg seg, double pct, double x)
        {
            Pt a = seg.p1, b = seg.p2; double s = U.Lerp(a.scale, b.scale, pct);
            return new SP { sx = U.Lerp(a.sx, b.sx, pct) + s * x * ROADW * VW / 2, sy = U.Lerp(a.sy, b.sy, pct), k = s * VW / 2 };
        }
        static double curFog;
        static Color Fog(Color c)
        {
            if (curFog <= 0.02) return c;
            Color s = theme.sky[1]; double f = curFog;
            return Color.FromArgb(c.A, (int)(c.R + (s.R - c.R) * f), (int)(c.G + (s.G - c.G) * f), (int)(c.B + (s.B - c.B) * f));
        }
        // cuadrilátero de la carretera; se solapa un poquito para que no se vean rayitas
        static void quad(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4, Color c)
        {
            fillPts(Fog(c), x1, y1 + 0.6, x2, y2 + 0.6, x3, y3 - 0.6, x4, y4 - 0.6);
        }
        static void drawSeg(Seg seg, int n)
        {
            Pt a = seg.p1, b = seg.p2; int alt = (seg.index / RUMBLE) % 2;
            // neblina a lo lejos: los colores se mezclan con el cielo (el horizonte se funde suavemente)
            curFog = Math.Min(0.8, Math.Pow((double)n / DRAW, 1.5) * 1.25);
            Color grass = theme.grass[alt];
            if (theme.lava) grass = U.Hsl(Math.Floor(16 + Math.Sin(T * 2 + seg.index * 0.15) * 8), 1, alt == 1 ? 0.47 : 0.52);
            rect(0, b.sy - 0.6, VW, a.sy - b.sy + 0.6, Fog(grass));
            double r1 = a.sw / 7, r2 = b.sw / 7;
            quad(a.sx - a.sw - r1, a.sy, a.sx - a.sw, a.sy, b.sx - b.sw, b.sy, b.sx - b.sw - r2, b.sy, theme.rumble[alt]);
            quad(a.sx + a.sw + r1, a.sy, a.sx + a.sw, a.sy, b.sx + b.sw, b.sy, b.sx + b.sw + r2, b.sy, theme.rumble[alt]);
            bool isFinish = seg.index < 2;
            Color road = isFinish ? C("#111111") : theme.rainbow ? U.Hsl((seg.index * 6) % 360, 0.85, alt == 1 ? 0.58 : 0.52) : theme.planks ? theme.road[seg.index % 2] : theme.road[alt];
            quad(a.sx - a.sw, a.sy, a.sx + a.sw, a.sy, b.sx + b.sw, b.sy, b.sx - b.sw, b.sy, road);
            if (!theme.rainbow && !isFinish)
            {
                Color ec = U.RGBA(255, 255, 255, 0.85);
                quad(a.sx - a.sw * 0.955, a.sy, a.sx - a.sw * 0.93, a.sy, b.sx - b.sw * 0.93, b.sy, b.sx - b.sw * 0.955, b.sy, ec);
                quad(a.sx + a.sw * 0.93, a.sy, a.sx + a.sw * 0.955, a.sy, b.sx + b.sw * 0.955, b.sy, b.sx + b.sw * 0.93, b.sy, ec);
            }
            if (isFinish)
            {
                int nn = 12;
                for (int i = 0; i < nn; i++)
                {
                    if ((i + seg.index) % 2 == 1) continue;
                    double f1 = (double)i / nn, f2 = (double)(i + 1) / nn;
                    quad(a.sx - a.sw + 2 * a.sw * f1, a.sy, a.sx - a.sw + 2 * a.sw * f2, a.sy, b.sx - b.sw + 2 * b.sw * f2, b.sy, b.sx - b.sw + 2 * b.sw * f1, b.sy, W_);
                }
            }
            else if (alt == 1)
            {
                double l1 = a.sw / 40, l2 = b.sw / 40;
                foreach (double f in new[] { 1.0 / 3, 2.0 / 3 })
                {
                    double x1 = a.sx - a.sw + 2 * a.sw * f, x2 = b.sx - b.sw + 2 * b.sw * f;
                    quad(x1 - l1, a.sy, x1 + l1, a.sy, x2 + l2, b.sy, x2 - l2, b.sy, theme.lane);
                }
            }
            if (!double.IsNaN(seg.pad))
            {
                double p = seg.pad, h = 0.17; Color col = ((seg.index + (int)Math.Floor(T * 10)) % 2 == 1) ? C("#ffb000") : C("#ff5a00");
                quad(a.sx + (p - h) * a.sw, a.sy, a.sx + (p + h) * a.sw, a.sy, b.sx + (p + h) * b.sw, b.sy, b.sx + (p - h) * b.sw, b.sy, col);
                double m1 = a.sx + p * a.sw, m2 = b.sx + p * b.sw;
                quad(m1 - h * 0.6 * a.sw, a.sy, m1 + h * 0.6 * a.sw, a.sy, m2, b.sy, m2, b.sy, C("#fff2a0"));
            }
            if (seg.ramp)
            {
                quad(a.sx - a.sw, a.sy, a.sx + a.sw, a.sy, b.sx + b.sw, b.sy, b.sx - b.sw, b.sy, seg.index % 2 == 1 ? C("#2a6ae8") : C("#4a8aff"));
                int nn = 8;
                for (int i = 0; i < nn; i += 2)
                {
                    double f1 = (double)i / nn, f2 = (double)(i + 1) / nn;
                    quad(a.sx - a.sw + 2 * a.sw * f1, a.sy, a.sx - a.sw + 2 * a.sw * f2, a.sy, b.sx - b.sw + 2 * b.sw * f2, b.sy, b.sx - b.sw + 2 * b.sw * f1, b.sy, C("#ffd800"));
                }
            }
            curFog = 0;
        }
        static void drawArch(Seg seg)
        {
            Pt a = seg.p1; if (!(a.scale > 0)) return;
            double k = a.sw / ROADW, hw = a.sw * 1.2, h = 1500 * k, x = a.sx, y = a.sy;
            rect(x - hw - 70 * k, y - h, 140 * k, h, C("#e8e8e8")); rect(x + hw - 70 * k, y - h, 140 * k, h, C("#e8e8e8"));
            double bh = 300 * k, cw = 2 * hw / 24; rect(x - hw, y - h, 2 * hw, bh, C("#111111"));
            for (int i = 0; i < 24; i++) for (int j = 0; j < 2; j++) if ((i + j) % 2 == 1) rect(x - hw + i * cw, y - h + j * bh / 2, cw + 0.5, bh / 2 + 0.5, W_);
        }

        // dibuja la vista de una cámara (un jugador) en la franja x=vx..vx+vw
        static void renderView(Racer cam, double vx, double vw)
        {
            VX = vx; VW = vw;
            var st = Save();
            ClipRect(vx, 0, vw, H); Translate(vx, 0);
            renderWorld(cam);
            if (!race.demo && cam.isPlayer && (state == "race" || state == "pause" || state == "results")) drawHUD(cam);
            Restore(st);
            VX = 0; VW = W;
        }
        static void renderWorld(Racer cam)
        {
            var st = Save();
            if (cam.shake > 0) Translate((U.R() - 0.5) * 14 * cam.shake / 0.4, (U.R() - 0.5) * 10 * cam.shake / 0.4);
            Gradient(-10, -10, VW + 20, H / 2.0 + 12, theme.sky[0], theme.sky[1]);
            rect(-10, H / 2.0, VW + 20, H / 2.0 + 10, theme.lava ? C("#e84000") : theme.grass[0]);
            if (Math.Abs(layerScale - Math.Min(3, Math.Max(0.5, SCALE))) > 0.02 && layerTheme != null) makeLayers(layerTheme, layerSeed);   // la ventana cambió de tamaño
            drawLayer(layerFar, cam.skyOff * 0.5, H / 2.0 - 270 + 4); drawLayer(layerNear, cam.skyOff, H / 2.0 - 270 + 6);
            int L = segments.Count; double camZ = wrapZ(cam.z - PLAYERZ);
            Seg bse = findSeg(camZ); double basePct = (camZ % SEG) / SEG;
            Seg pSeg = findSeg(cam.z); double pPct = (cam.z % SEG) / SEG, pY = U.Lerp(pSeg.p1.wy, pSeg.p2.wy, pPct);
            double maxy = H, x = 0, dx = -(bse.curve * basePct);
            g.SmoothingMode = SmoothingMode.HighSpeed;   // la carretera sin suavizado: mucho más rápido y sin rayitas
            for (int n = 0; n < DRAW; n++)
            {
                Seg seg = segments[(bse.index + n) % L]; bool looped = seg.index < bse.index; double cz = camZ - (looped ? trackLen : 0);
                project(seg.p1, cam.x * ROADW - x, pY + CAMH, cz); project(seg.p2, cam.x * ROADW - x - dx, pY + CAMH, cz);
                x += dx; dx += seg.curve; seg.clip = maxy; seg.vis = seg.p1.cz > DEPTH;
                if (seg.p1.cz <= DEPTH || seg.p2.sy >= seg.p1.sy || seg.p2.sy >= maxy) continue;
                drawSeg(seg, n); maxy = seg.p1.sy;
            }
            g.SmoothingMode = SmoothingMode.AntiAlias;
            foreach (var s in dynSegs) s.dyn.Clear(); dynSegs.Clear();
            foreach (var r in racers) { var s = findSeg(r.z); s.dyn.Add(r); dynSegs.Add(s); }
            foreach (var p in projs) { var s = findSeg(p.z); s.dyn.Add(p); dynSegs.Add(s); }
            foreach (var h in hazards) if (h.dead <= 0) { var s = findSeg(h.z); s.dyn.Add(h); dynSegs.Add(s); }
            for (int n = DRAW - 1; n > 0; n--)
            {
                Seg seg = segments[(bse.index + n) % L]; if (!seg.vis) continue;
                if (!(seg.decor.Count > 0 || seg.coins.Count > 0 || seg.boxes.Count > 0 || seg.dyn.Count > 0 || seg.index == 2)) continue;
                var cs = Save(); ClipRect(-20, -20, VW + 40, seg.clip + 20);
                if (seg.index == 2) drawArch(seg);
                foreach (var d in seg.decor) { var p = segPt(seg, 0.5, d.x); if (p.sx < -1600 * p.k || p.sx > VW + 1600 * p.k) continue; drawDecor(d, p.sx, p.sy, p.k); }
                foreach (var c in seg.coins) if (c.taken <= 0) { var p = segPt(seg, 0.5, c.x); drawCoin(p.sx, p.sy - 170 * p.k, 130 * p.k); }
                foreach (var b in seg.boxes) if (b.t <= 0) { var p = segPt(seg, 0.5, b.x); drawBox(p.sx, p.sy, 230 * p.k); }
                if (seg.dyn.Count > 1) seg.dyn.Sort((u, v) => v.z.CompareTo(u.z));
                foreach (var o in seg.dyn)
                {
                    var p = segPt(seg, (o.z % SEG) / SEG, o.x);
                    var r = o as Racer;
                    if (r != null)
                    {
                        double w = KARTW * p.k * (r.shrink > 0 ? 0.55 : 1) * (r.pwT > 0 && r.pwType == "giant" ? 1.6 : 1); if (w > 1) drawKart(r, p.sx, p.sy, w);
                        if (r == cam) { cam.scr.x = VX + p.sx; cam.scr.y = p.sy; cam.scr.w = w; }
                        else if (r.isPlayer && players.Count > 1 && w > 12) txt("J" + (r.pidx + 1), p.sx, p.sy - w * 0.95, Math.Max(11, w * 0.22), P_COL[r.pidx], "center", Color.Black, 3);
                    }
                    else if (o is Hazard) drawHazard((Hazard)o, p.sx, p.sy, p.k);
                    else drawProj((Proj)o, p.sx, p.sy, p.k);
                }
                Restore(cs);
            }
            Restore(st);
        }
    }
}
