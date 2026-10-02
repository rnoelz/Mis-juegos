using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SuperKart
{
    public class Entry { public Ch ch; public Kt kt; public bool isPlayer; public int pidx; }
    public class RaceState { public int ti; public bool demo, tt, mirror; public double time, count, flash, bumpT, goT, doneT; public string phase; }
    public class GPState { public int cup, race; public Dictionary<string, int> points = new Dictionary<string, int>(); public List<Entry> roster; }
    public class ResEntry { public Racer r; public double time; public int pts; }
    public class Rec { public double total, lap; public string ch; }
    public class TTRes { public double total, best; public List<double> laps; public Rec old; public bool newRec; }
    public class Results { public List<ResEntry> list = new List<ResEntry>(); public TTRes tt; }
    public class Part { public double x, y, vx, vy, life, max, size, grav; public Color col; }
    public class Inp { public bool acc, brake, drift, driftPress, use, power; public int steer; }
    public class Ctrl { public string[] acc, brake, left, right, drift, use, power; }
    public class MenuKeys { public string up, dn, lf, rt; public string[] ok, cancel; }
    public class MenuState
    {
        public int np = 1, row = 0, mode = 0, diff = 0, cc = 2, cup = 0, tr = 0, p = 0; public bool mirror;
        public int sdur = 1, sfield = 0;   // fútbol: duración y cancha
        public int[] ch = { 0, 1 }, kt = { 0, 0 }; public bool[] lck = { false, false };
    }

    static partial class SK
    {
        public static double T;
        public static string state = "title";
        public static MenuState menu = new MenuState();
        public static RaceState race;
        public static List<Racer> racers = new List<Racer>(), players = new List<Racer>(), cams = new List<Racer>(), ranking = new List<Racer>();
        public static List<Proj> projs = new List<Proj>();
        public static List<Part> parts = new List<Part>();
        public static GPState gp;
        public static List<Entry> roster = new List<Entry>(), curGrid = new List<Entry>();
        public static Results results;
        public static bool wantFullscreenToggle;

        // ---------------- teclado ----------------
        public static HashSet<string> keys = new HashSet<string>(), pressed = new HashSet<string>();
        static bool kp(params string[] c) { foreach (var k in c) if (pressed.Contains(k)) return true; return false; }
        static bool held(string[] c) { foreach (var k in c) if (keys.Contains(k)) return true; return false; }
        static Ctrl CTRL_SOLO = new Ctrl { acc = new[] { "ArrowUp", "KeyW" }, brake = new[] { "ArrowDown", "KeyS" }, left = new[] { "ArrowLeft", "KeyA" }, right = new[] { "ArrowRight", "KeyD" }, drift = new[] { "Space", "ShiftLeft", "ShiftRight" }, use = new[] { "KeyX", "KeyE", "Enter" }, power = new[] { "KeyC", "KeyR" } };
        static Ctrl[] CTRL = {
            new Ctrl { acc = new[] { "KeyW" }, brake = new[] { "KeyS" }, left = new[] { "KeyA" }, right = new[] { "KeyD" }, drift = new[] { "Space", "ShiftLeft" }, use = new[] { "KeyE", "KeyQ" }, power = new[] { "KeyR" } },
            new Ctrl { acc = new[] { "ArrowUp" }, brake = new[] { "ArrowDown" }, left = new[] { "ArrowLeft" }, right = new[] { "ArrowRight" }, drift = new[] { "ShiftRight", "ControlRight", "Numpad0" }, use = new[] { "Enter" }, power = new[] { "KeyL" } },
        };
        static MenuKeys[] MENU_KEYS = {
            new MenuKeys { up = "KeyW", dn = "KeyS", lf = "KeyA", rt = "KeyD", ok = new[] { "Space", "KeyE" }, cancel = new[] { "KeyQ" } },
            new MenuKeys { up = "ArrowUp", dn = "ArrowDown", lf = "ArrowLeft", rt = "ArrowRight", ok = new[] { "Enter" }, cancel = new[] { "Backspace" } },
        };
        static Ctrl ctrlFor(Racer r) { return players.Count > 1 ? CTRL[r.pidx] : CTRL_SOLO; }
        static Inp playerInput(Racer r)
        {
            var c = ctrlFor(r);
            return new Inp { acc = held(c.acc), brake = held(c.brake), steer = (held(c.right) ? 1 : 0) - (held(c.left) ? 1 : 0), drift = held(c.drift), driftPress = kp(c.drift), use = kp(c.use), power = kp(c.power) };
        }

        // ---------------- datos guardados (récords y trofeos) ----------------
        static Dictionary<string, string> save;
        static string SavePath()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SuperKartBros");
            return Path.Combine(dir, "save.txt");
        }
        public static bool noSave;   // en modo de prueba no se toca el archivo del jugador
        static string store(string k)
        {
            if (save == null)
            {
                save = new Dictionary<string, string>();
                try { if (!noSave && File.Exists(SavePath())) foreach (var line in File.ReadAllLines(SavePath())) { int i = line.IndexOf('='); if (i > 0) save[line.Substring(0, i)] = line.Substring(i + 1); } } catch { }
            }
            string v; return save.TryGetValue(k, out v) ? v : null;
        }
        static void store(string k, string v)
        {
            store(k); save[k] = v;
            if (noSave) return;
            try { Directory.CreateDirectory(Path.GetDirectoryName(SavePath())); File.WriteAllLines(SavePath(), save.Select(p => p.Key + "=" + p.Value).ToArray()); } catch { }
        }
        static string recKey(int ti) { return "tt_" + ti + "_" + menu.cc + "_" + (menu.mirror ? 1 : 0); }
        static string cupKey(int c) { return "cup_" + c + "_" + menu.cc + "_" + (menu.mirror ? 1 : 0); }
        static Rec getRec(int ti)
        {
            string s = store(recKey(ti)); if (s == null) return null;
            var p = s.Split(';'); var ci = System.Globalization.CultureInfo.InvariantCulture;
            return new Rec { total = double.Parse(p[0], ci), lap = double.Parse(p[1], ci), ch = p.Length > 2 ? p[2] : "" };
        }
        static int cupBest(int c) { string s = store(cupKey(c)); int v; return s != null && int.TryParse(s, out v) ? v : 0; }

        // ---------------- corredores ----------------
        static Diff dif() { return DIFF[race != null && race.demo ? 1 : menu.diff]; }
        static Racer makeRacer(Entry e, bool demo)
        {
            var st = Stats(e.ch, e.kt); var cc = demo ? CC[2] : CC[menu.cc];
            var r = new Racer { ch = e.ch, kt = e.kt, st = st, isPlayer = e.isPlayer, pidx = e.pidx };
            r.top = MAXS * cc.mul * (0.88 + 0.03 * st.spd); r.accR = MAXS * cc.mul * (0.16 + 0.055 * st.acc); r.han = 1.8 + 0.22 * st.han; r.wgt = st.wgt;
            r.ai.skill = (demo ? 0.93 : cc.ai * DIFF[menu.diff].speed) * (0.95 + U.R() * 0.06);
            return r;
        }
        static Racer makeDummy(Ch ch, Kt kt) { return new Racer { ch = ch, kt = kt, steerVis = Math.Sin(T * 2) * 0.6 }; }
        static void showMsg(Racer r, string m, double t) { r.msg = m; r.msgT = t; }
        static void sparkle(Racer r) { var s = r.scr; for (int i = 0; i < 6; i++) addPart(s.x + (U.R() - 0.5) * s.w * 0.5, s.y - s.w * 0.6, (U.R() - 0.5) * 200, -150 - U.R() * 200, 0.6, C("#ffd23a"), 5, 600); }
        static Racer leadHuman() { Racer b = null; foreach (var p in players) if (b == null || prog(p) > prog(b)) b = p; return b; }

        static string rollItem(int place)
        {
            int b = place == 1 ? 0 : place <= 3 ? 1 : place <= 6 ? 2 : 3; var tb = ITEM_TABLE[b];
            int tot = 0; foreach (var v in tb.Values) tot += v;
            double x = U.R() * tot;
            foreach (var kv in tb) { x -= kv.Value; if (x <= 0) return kv.Key; }
            return "banana";
        }
        static Proj mkProj(string type, Racer owner, double z, double x, double speed = 0, double vx = 0, Racer target = null, double life = 40)
        {
            return new Proj { type = type, owner = owner, z = wrapZ(z), x = x, speed = speed, vx = vx, target = target, life = life };
        }
        static void useItem(Racer r)
        {
            if (r.item == null || r.roulette > 0) return;
            string it = r.item; bool pl = r.isPlayer;
            switch (it)
            {
                case "mushroom": case "triple": r.boost = Math.Max(r.boost, 1.3); if (pl) SFX.boost(); break;
                case "golden": r.boost = Math.Max(r.boost, 0.9); if (r.goldenT <= 0) r.goldenT = 7; r.itemCount = 99; if (pl) SFX.boost(); break;
                case "banana": case "triple_banana": projs.Add(mkProj("banana", r, r.z - 260, r.x)); if (pl) SFX.throwIt(); break;
                case "green": case "triple_green": projs.Add(mkProj("green", r, r.z + 300, r.x, Math.Max(r.speed, 0) + MAXS * 0.75, (U.R() - 0.5) * 0.3, null, 6)); if (pl) SFX.throwIt(); break;
                case "bomb": projs.Add(mkProj("bomb", r, r.z + 300, r.x, Math.Max(r.speed, 0) + MAXS * 0.45, 0, null, 3)); if (pl) SFX.throwIt(); break;
                case "red":
                    {
                        Racer tgt = r.place > 1 ? ranking[r.place - 2] : null;
                        projs.Add(mkProj(tgt != null ? "red" : "green", r, r.z + 300, r.x, Math.Max(r.speed, 0) + MAXS * 0.8, 0, tgt, 8)); if (pl) SFX.throwIt(); break;
                    }
                case "blue": projs.Add(mkProj("blue", r, r.z + 300, r.x, 0, 0, null, 25)); if (pl) SFX.throwIt(); break;
                case "star": r.star = 7; if (pl) SFX.star(); break;
                case "lightning":
                    foreach (var o in racers) if (o != r && o.star <= 0 && o.bullet <= 0 && o.rescue <= 0) { o.shrink = 4; o.spin = Math.Max(o.spin, 0.8); o.speed *= 0.5; o.item = null; o.roulette = 0; o.drift = 0; o.driftLvl = 0; }
                    race.flash = 0.5; SFX.thunder(); if (!race.demo) foreach (var h in players) if (h != r) showMsg(h, "¡RAYO!", 1.2);
                    break;
                case "bullet": r.bullet = 3.5; r.drift = 0; r.driftLvl = 0; if (pl) SFX.boost(); break;
                case "coin": r.coins = Math.Min(10, r.coins + 2); if (pl) SFX.coin(); break;
                case "blooper":
                    foreach (var o in racers) if (o != r && o.place < r.place) { o.ink = 4.5; if (o.isPlayer && !race.demo) showMsg(o, "¡TINTA!", 1.2); }
                    if (pl || players.Any(h => h.place < r.place)) Snd.Tone(300, 0.4, "sine", 0.15, 120);
                    break;
                case "boo":
                    {
                        var vict = racers.Where(o => o != r && o.item != null && o.roulette <= 0 && o.goldenT <= 0).ToList();
                        Racer v = vict.Count > 0 ? vict[U.rng.Next(vict.Count)] : null;
                        r.ghostT = 3.5; r.item = null; r.itemCount = 0;
                        if (v != null) { r.item = v.item; r.itemCount = v.itemCount; v.item = null; v.itemCount = 0; if (v.isPlayer && !race.demo) showMsg(v, "¡BOO TE ROBÓ!", 1.5); }
                        else { r.item = rollItem(r.place); r.itemCount = ITEM_COUNT.ContainsKey(r.item) ? ITEM_COUNT[r.item] : 1; }
                        if (pl) Snd.Tone(500, 0.5, "sine", 0.12, 900);
                        return;
                    }
                case "horn":
                    r.hornT = 0.5;
                    foreach (var p in projs) if (p.type != "boom" && p.owner != r && Math.Abs(wrapD(p.z - r.z)) < 900) p.life = 0;
                    foreach (var o in racers) if (o != r && Math.Abs(wrapD(o.z - r.z)) < 550 && Math.Abs(o.x - r.x) < 0.6) hitRacer(o, "horn");
                    if (pl || !race.demo) Snd.Tone(160, 0.6, "sawtooth", 0.18, 140);
                    break;
            }
            r.itemCount--; if (r.itemCount <= 0) { r.item = null; r.itemCount = 0; }
        }
        static bool hitRacer(Racer r, string kind)
        {
            if (r.invul > 0 || r.rescue > 0 || isSafe(r)) return false;
            bool big = kind == "blue";
            r.spin = big ? 1.8 : 1.1; r.speed *= big ? 0.05 : 0.35; r.coins = Math.Max(0, r.coins - (big ? 3 : 2)); r.drift = 0; r.driftLvl = 0; r.invul = r.spin + 0.5; r.boost = 0;
            if (r.isPlayer && !race.demo) { r.shake = 0.4; SFX.hit(); for (int i = 0; i < 10; i++) addPart(r.scr.x, r.scr.y - r.scr.w * 0.4, (U.R() - 0.5) * 400, -200 - U.R() * 300, 0.8, C("#ffd23a"), 5, 600); }
            return true;
        }

        // ---------------- súper poderes ----------------
        static bool flying(Racer o) { return o.pwT > 0 && (o.pwType == "fly" || o.pwType == "acro"); }
        static bool isSafe(Racer o)
        {
            return o.star > 0 || o.bullet > 0 || o.ghostT > 0 ||
                (o.pwT > 0 && (o.pwType == "fly" || o.pwType == "acro" || o.pwType == "shield" || o.pwType == "giant" || o.pwType == "spin" || o.pwType == "ssj"));
        }
        static bool smasher(Racer o) { return o.star > 0 || o.bullet > 0 || (o.pwT > 0 && (o.pwType == "giant" || o.pwType == "spin" || o.pwType == "acro" || o.pwType == "ssj")); }
        static void activatePower(Racer r)
        {
            if (r.meter < 1 || r.pwT > 0 || r.spin > 0 || r.rescue > 0 || r.frozenT > 0) return;
            var P = PowerOf(r.ch); string t = P[0]; bool pl = r.isPlayer && !race.demo;
            double dur; r.meter = 0; r.pwT = PW_DUR.TryGetValue(t, out dur) ? dur : 0; r.pwType = r.pwT > 0 ? t : "";
            if (pl) { SFX.power(); showMsg(r, "¡" + P[1].ToUpper() + "!", 1.4); }
            switch (t)
            {
                case "fly": case "acro": r.drift = 0; r.driftLvl = 0; break;
                case "dash": r.boost = Math.Max(r.boost, 2.2); break;
                case "ghost": r.ghostT = 4; break;
                case "fire":
                    {
                        string ty = r.ch.id == "birdo" ? "egg" : "fire";
                        foreach (double dx in new[] { -0.28, 0, 0.28 }) projs.Add(mkProj(ty, r, r.z + 300, U.Clamp(r.x + dx, -1, 1), Math.Max(r.speed, 0) + MAXS * 0.9, dx * 0.4, null, 2.5));
                        break;
                    }
                case "shock":
                    r.hornT = 0.5;
                    foreach (var o in racers) if (o != r && Math.Abs(wrapD(o.z - r.z)) < 900 && Math.Abs(o.x - r.x) < 1) hitRacer(o, "shock");
                    if (!race.demo) SFX.boom();
                    break;
                case "magnet": r.coins = Math.Min(10, r.coins + 5); r.boost = Math.Max(r.boost, 1); if (pl) { SFX.coin(); sparkle(r); } break;
                case "freeze":
                    foreach (var o in racers) if (o != r && o.place < r.place && fwd(r.z, o.z) < 9000 && !isSafe(o))
                        { o.frozenT = 1.8; o.speed *= 0.3; o.drift = 0; o.driftLvl = 0; if (o.isPlayer && !race.demo) showMsg(o, "¡CONGELADO!", 1.2); }
                    break;
                case "laser": case "kame":
                    {
                        r.laserT = 0.7; int n = 0;
                        for (int i = r.place - 2; i >= 0 && n < 2; i--) { var o = ranking[i]; if (o != r && fwd(r.z, o.z) < 12000 && hitRacer(o, "laser")) n++; }
                        if (!race.demo) Snd.Tone(1200, 0.5, "sawtooth", 0.12, 300);
                        break;
                    }
                case "tele":
                    r.teleT = 0.6; r.z += 1800; if (r.z >= trackLen) { r.z -= trackLen; r.lap++; onLap(r); }
                    r.x = U.Clamp(r.x, -0.8, 0.8); r.invul = Math.Max(r.invul, 0.5);
                    break;
            }
        }
        static void onLap(Racer r)
        {
            if (r.lap <= r.maxLap) return; r.maxLap = r.lap; if (race.demo) return;
            if (r.isPlayer && r.lap >= 2) { r.lapTimes.Add(race.time - r.lapStart); r.lapStart = race.time; }
            if (r.lap > LAPS && !r.finished) { r.finished = true; r.finishTime = race.time; if (r.isPlayer) playerFinished(r); return; }
            if (r.isPlayer)
            {
                if (r.lap == LAPS) { showMsg(r, "¡ÚLTIMA VUELTA!", 2.2); SFX.last(); Snd.SetMusicRate(1.12); }
                else if (r.lap > 1) { showMsg(r, "VUELTA " + r.lap, 1.5); SFX.lap(); }
            }
        }
        static void playerFinished(Racer r)
        {
            showMsg(r, "¡META! " + racers.Count(x => x.finished) + "º", 3); SFX.finish();
            if (players.All(p => p.finished)) { race.phase = "done"; race.doneT = 0; Snd.SetMusicRate(1); }
        }
        static void pickups(Racer r, double z0, double z1)
        {
            int L = segments.Count; int i = (int)Math.Floor(wrapZ(z0) / SEG), j = (int)Math.Floor(wrapZ(z1) / SEG); int n = ((j - i) % L + L) % L; if (n > 6) n = 6;
            for (int k = 0; k <= n; k++)
            {
                var s = segments[(i + k) % L];
                if (!double.IsNaN(s.pad) && Math.Abs(r.x - s.pad) < 0.22 && r.boost < 0.8) { r.boost = Math.Max(r.boost, 0.9); if (r.isPlayer && !race.demo) SFX.boost(); }
                if (s.ramp && r.air <= 0 && r.speed > MAXS * 0.3)
                {
                    r.air = r.airMax = 0.6 + 0.3 * Math.Min(1.3, r.speed / MAXS); r.drift = 0; r.driftLvl = 0;
                    if (r.isPlayer && !race.demo) Snd.Tone(400, 0.25, "square", 0.08, 800);
                    else if (!r.isPlayer && U.R() < 0.6) { r.trick = true; r.trickT = 0.4; }
                }
                foreach (var c in s.coins) if (c.taken <= 0 && Math.Abs(c.x - r.x) < 0.13) { c.taken = 10; if (r.coins < 10) r.coins++; r.meter = Math.Min(1, r.meter + 0.06); if (r.isPlayer && !race.demo) { SFX.coin(); sparkle(r); } }
                foreach (var b in s.boxes) if (b.t <= 0 && Math.Abs(b.x - r.x) < 0.16)
                    {
                        b.t = 2.5;
                        if (r.item == null && r.roulette <= 0)
                        {
                            r.roulette = r.isPlayer ? 1.3 : 0.8; r.pending = rollItem(r.place);
                            if (!r.isPlayer && dif().noMean && (r.pending == "blue" || r.pending == "lightning" || r.pending == "bullet")) r.pending = "mushroom";
                            r.ai.useT = dif().useMin + U.R() * dif().useRand;
                            if (r.isPlayer && !race.demo) SFX.box();
                        }
                    }
            }
        }

        static void updateRacer(Racer r, Inp inp, double dt)
        {
            r.boost = Math.Max(0, r.boost - dt); r.star = Math.Max(0, r.star - dt); r.shrink = Math.Max(0, r.shrink - dt); r.bullet = Math.Max(0, r.bullet - dt);
            r.invul = Math.Max(0, r.invul - dt); r.spin = Math.Max(0, r.spin - dt); r.hop = Math.Max(0, r.hop - dt);
            r.ghostT = Math.Max(0, r.ghostT - dt); r.ink = Math.Max(0, r.ink - dt); r.hornT = Math.Max(0, r.hornT - dt); r.trickT = Math.Max(0, r.trickT - dt);
            r.frozenT = Math.Max(0, r.frozenT - dt); r.laserT = Math.Max(0, r.laserT - dt); r.teleT = Math.Max(0, r.teleT - dt);
            if (r.pwT > 0) { r.pwT -= dt; if (r.pwT <= 0) { r.pwT = 0; r.pwType = ""; } }
            else if (r.meter < 1) r.meter = Math.Min(1, r.meter + dt / 14 * (r.isPlayer ? 1 : dif().pw));
            if (r.goldenT > 0) { r.goldenT -= dt; if (r.goldenT <= 0 && r.item == "golden") { r.item = null; r.itemCount = 0; } }
            if (r.air > 0) { r.air -= dt; if (r.air <= 0 && r.trick) { r.trick = false; r.boost = Math.Max(r.boost, 1.0); if (r.isPlayer) SFX.boost(); } }
            if (r.roulette > 0) { r.roulette -= dt; if (r.roulette <= 0) { r.item = r.pending; r.itemCount = ITEM_COUNT.ContainsKey(r.item) ? ITEM_COUNT[r.item] : 1; if (r.isPlayer && !race.demo) SFX.itemGet(); } }
            if (r.rescue > 0) { r.rescue -= dt; r.speed = 0; if (r.rescue <= 0) { r.x = 0; r.invul = 1.2; } return; }
            var seg = findSeg(r.z);
            double top = r.top * (1 + 0.012 * r.coins);
            if (!r.isPlayer || r.finished) top *= r.ai.mul;
            if (r.star > 0) top *= 1.2; if (r.shrink > 0) top *= 0.72;
            if (r.pwT > 0) top *= r.pwType == "fly" ? 1.25 : r.pwType == "acro" ? 1.3 : r.pwType == "spin" ? 1.3 : r.pwType == "ssj" ? 1.3 : r.pwType == "giant" ? 1.15 : 1;
            bool off = Math.Abs(r.x) > 1.05;
            if (off && r.boost <= 0 && r.star <= 0 && r.bullet <= 0 && r.air <= 0 && !flying(r)) top *= r.kt.id == "buggy" ? 0.7 : 0.45;
            if (r.boost > 0) top *= 1.38;
            if (r.bullet > 0) top = r.top * 1.7;
            if (r.spin > 0 || r.frozenT > 0) r.speed -= r.speed * Math.Min(1, dt * (r.frozenT > 0 ? 4 : 3));
            else if (r.boost > 0 || r.bullet > 0) r.speed = Math.Min(top, Math.Max(r.speed, top * 0.8) + MAXS * 1.5 * dt);
            else if (inp.acc) { if (r.speed < top) r.speed = Math.Min(top, r.speed + r.accR * (1 - 0.65 * Math.Max(0, r.speed) / top) * dt); else r.speed = Math.Max(top, r.speed - MAXS * 0.8 * dt); }
            else if (inp.brake) r.speed = Math.Max(-MAXS * 0.18, r.speed - MAXS * 1.2 * dt);
            else r.speed = r.speed > 0 ? Math.Max(0, r.speed - MAXS * 0.35 * dt) : Math.Min(0, r.speed + MAXS * 0.35 * dt);
            if (r.speed > top && !(r.boost > 0 || r.bullet > 0)) r.speed = Math.Max(top, r.speed - MAXS * 0.8 * dt);
            double sp = r.speed / MAXS;
            // salto, truco y derrape
            if (inp.driftPress && r.air > 0 && !r.trick && r.spin <= 0) { r.trick = true; r.trickT = 0.4; if (r.isPlayer) Snd.Tone(900, 0.15, "square", 0.08, 1400); }
            else if (inp.driftPress && r.air <= 0 && r.hop <= 0 && r.speed > MAXS * 0.3 && r.spin <= 0 && r.bullet <= 0) { r.hop = 0.28; if (r.isPlayer) SFX.hop(); }
            if (inp.drift && r.drift == 0 && r.hop > 0 && inp.steer != 0 && r.speed > MAXS * 0.35) { r.drift = inp.steer; r.driftCharge = 0; r.driftLvl = 0; }
            if (r.drift != 0)
            {
                if (!inp.drift || r.speed < MAXS * 0.3 || r.spin > 0)
                {
                    if (r.driftLvl > 0 && r.spin <= 0) { r.boost = Math.Max(r.boost, new[] { 0, 0.6, 1.1, 1.7 }[r.driftLvl]); if (r.isPlayer) SFX.boost(); }
                    r.drift = 0; r.driftCharge = 0; r.driftLvl = 0;
                }
                else
                {
                    r.driftCharge += dt * (inp.steer == r.drift ? 1.7 : inp.steer == 0 ? 1.1 : 0.6);
                    int lvl = r.driftCharge > 3.3 ? 3 : r.driftCharge > 1.9 ? 2 : r.driftCharge > 0.8 ? 1 : 0;
                    if (lvl > r.driftLvl) { r.driftLvl = lvl; if (r.isPlayer) SFX.spark(lvl); }
                }
            }
            // dirección
            r.steerVis = U.Lerp(r.steerVis, inp.steer, Math.Min(1, dt * 8));
            if (r.spin <= 0 && r.bullet <= 0 && r.frozenT <= 0)
            {
                double st = inp.steer; if (r.drift != 0) st = r.drift * 0.6 + inp.steer * 0.5;
                double grip = Math.Min(1, Math.Abs(sp) * 1.8);
                r.x += st * r.han * (theme.slip ? 0.8 : 1) * dt * grip * (r.speed < 0 ? -1 : 1);
            }
            if (r.bullet > 0) r.x = U.Lerp(r.x, 0, Math.Min(1, dt * 2.5));
            if (r.isPlayer && r.bullet <= 0) { double s2 = Math.Min(1.2, Math.Max(0, sp)); r.x -= dt * 2 * s2 * s2 * seg.curve * CENTRI * (theme.slip ? 1.35 : 1); }
            // avanzar
            double z0 = r.z; r.z += r.speed * dt;
            if (r.z >= trackLen) { r.z -= trackLen; r.lap++; onLap(r); } else if (r.z < 0) { r.z += trackLen; r.lap--; }
            if (theme.danger && Math.Abs(r.x) > 1.3 && r.bullet <= 0 && r.air <= 0 && !flying(r))
            {
                r.rescue = 1.6; r.speed = 0; r.drift = 0; r.driftLvl = 0; r.coins = Math.Max(0, r.coins - 2);
                if (r.isPlayer && !race.demo) { SFX.fall(); showMsg(r, "¡TE CAÍSTE!", 1.3); }
            }
            r.x = U.Clamp(r.x, -2.4, 2.4);
            if (!r.isPlayer) r.x = U.Clamp(r.x, -1.1, 1.1);
            if (r.speed > 0) pickups(r, z0, r.z);
        }

        static Inp aiInput(Racer r, double dt)
        {
            var a = r.ai; var inp = new Inp { acc = true };
            a.t -= dt;
            if (a.t <= 0) { a.t = 0.8 + U.R() * 2.2; a.tx = U.R() * 1.5 - 0.75; }
            int L = segments.Count, si = (int)Math.Floor(r.z / SEG);
            if (r.item == null && r.roulette <= 0)
            {
                for (int k = 4; k < 25; k++)
                {
                    var s = segments[(si + k) % L];
                    if (s.boxes.Count > 0) { Box best = null; double bd = 9; foreach (var b in s.boxes) if (b.t <= 0 && Math.Abs(b.x - r.x) < bd) { bd = Math.Abs(b.x - r.x); best = b; } if (best != null) { a.tx = best.x; a.t = 0.5; } break; }
                }
            }
            foreach (var p in projs) { if (p.type != "banana" && p.type != "green") continue; double d = fwd(r.z, p.z); if (d > 100 && d < 2200 && Math.Abs(p.x - a.tx) < 0.28) { a.tx = p.x + (p.x > 0 ? -0.45 : 0.45); a.t = 0.6; } }
            foreach (var h in hazards) { if (h.dead > 0 || h.type == "podoboo") continue; double d = fwd(r.z, h.z); if (d > 150 && d < 2000 && Math.Abs(h.x - a.tx) < 0.3) { a.tx = h.x + (h.x > 0 ? -0.5 : 0.5); a.t = 0.5; } }
            double steer = U.Clamp((a.tx - r.x) * 3, -1, 1);
            if (r.ink > 0) steer = U.Clamp(steer + Math.Sin(T * 3 + a.skill * 50) * 0.6, -1, 1);
            a.mul = a.skill;
            var LH = leadHuman();
            if (LH != null && !race.demo && !r.isPlayer)
            {
                double gap = prog(LH) - prog(r); var D = dif();
                if (gap > 3000) a.mul *= 1 + Math.Min(D.catchup, (gap - 3000) / 50000);
                else if (gap < -1500) a.mul *= Math.Max(D.slow, 1 - (-gap - 1500) / 40000);
            }
            if (Math.Abs(segments[si % L].curve) >= 4 && U.R() < dt * dif().boost && r.boost <= 0) r.boost = 0.5;
            if (r.item != null && r.roulette <= 0) { a.useT -= dt; if (a.useT <= 0) { useItem(r); a.useT = dif().useMin + U.R() * dif().useRand; } }
            if (r.meter >= 1 && r.pwT <= 0) { if (double.IsNaN(a.pwWait)) a.pwWait = 0.5 + U.R() * 3; a.pwWait -= dt; if (a.pwWait <= 0) { activatePower(r); a.pwWait = double.NaN; } }
            aiSteer = steer;
            return inp;
        }
        static double aiSteer;   // dirección continua de la IA (en JS steer podía ser decimal)

        static void updateProjs(double dt)
        {
            var added = new List<Proj>();
            foreach (var p in projs)
            {
                p.age += dt; p.life -= dt;
                if (p.type == "green" || p.type == "fire" || p.type == "egg") { p.z += p.speed * dt; p.x += p.vx * dt; if (Math.Abs(p.x) > 1.1) { p.vx = -p.vx; p.x = U.Clamp(p.x, -1.1, 1.1); } }
                else if (p.type == "bomb")
                {
                    p.z += p.speed * dt; p.speed = Math.Max(0, p.speed - MAXS * 0.9 * dt);
                    bool touch = false; if (p.age > 0.3) foreach (var r in racers) if (r != p.owner && Math.Abs(wrapD(r.z - p.z)) < 170 && Math.Abs(r.x - p.x) < 0.2) touch = true;
                    if (touch || p.life <= 0.05)
                    {
                        foreach (var r in racers) if (Math.Abs(wrapD(r.z - p.z)) < 520 && Math.Abs(r.x - p.x) < 0.5) hitRacer(r, "blue");
                        var b = mkProj("boom", p.owner, p.z, p.x, 0, 0, null, 0.7); b.col = C("#ff8c1e"); added.Add(b); p.life = 0;
                        if (!race.demo && players.Any(h => Math.Abs(wrapD(h.z - p.z)) < 4000)) SFX.boom();
                    }
                }
                else if (p.type == "red")
                {
                    var t = p.target;
                    if (t == null || t.rescue > 0) { p.type = "green"; p.vx = 0; continue; }
                    double d = fwd(p.z, t.z); p.speed = Math.Max(t.speed, 0) + MAXS * 0.8; p.z += p.speed * dt;
                    if (d < 2500) p.x = U.Lerp(p.x, t.x, Math.Min(1, dt * (d < 800 ? 10 : 3))); else p.x = U.Lerp(p.x, 0, Math.Min(1, dt));
                }
                else if (p.type == "blue")
                {
                    var t = ranking[0]; double d = fwd(p.z, t.z); p.target = t; p.speed = MAXS * 2.2; p.x = U.Lerp(p.x, t.x, Math.Min(1, dt * 4));
                    if (d < p.speed * dt + 150)
                    {
                        foreach (var r in racers) if (Math.Abs(wrapD(r.z - t.z)) < 500 && Math.Abs(r.x - t.x) < 0.5) hitRacer(r, "blue");
                        added.Add(mkProj("boom", p.owner, t.z, t.x, 0, 0, null, 0.7)); p.life = 0;
                        if (!race.demo) SFX.boom();
                    }
                    else p.z += p.speed * dt;
                }
                p.z = wrapZ(p.z);
            }
            projs.AddRange(added);
            foreach (var p in projs)
            {
                if (p.type == "blue" || p.type == "boom" || p.type == "bomb" || p.life <= 0) continue;
                foreach (var r in racers)
                {
                    if (r.rescue > 0 || (r == p.owner && p.age < 0.5)) continue;
                    if (Math.Abs(wrapD(r.z - p.z)) < 170 && Math.Abs(r.x - p.x) < 0.17)
                    {
                        if (r.star > 0 || r.bullet > 0) { p.life = 0; break; }
                        if (hitRacer(r, p.type)) { p.life = 0; break; }
                    }
                }
            }
            for (int i = 0; i < projs.Count; i++) for (int j = i + 1; j < projs.Count; j++)
                {
                    var a = projs[i]; var b = projs[j];
                    if (a.life <= 0 || b.life <= 0 || a.type == "blue" || b.type == "blue" || a.type == "boom" || b.type == "boom") continue;
                    if (a.type == "banana" && b.type == "banana") continue;
                    if (Math.Abs(wrapD(a.z - b.z)) < 150 && Math.Abs(a.x - b.x) < 0.15) { a.life = 0; b.life = 0; }
                }
            projs.RemoveAll(p => p.life <= 0);
        }
        static void updateHazards(double dt)
        {
            foreach (var h in hazards)
            {
                if (h.dead > 0) { h.dead -= dt; continue; }
                if (h.type == "car") h.z = wrapZ(h.z + h.speed * dt);
                else if (h.type == "podoboo") h.h = Math.Max(0, Math.Sin(T * 1.4 + h.ph)) * 900;
                else h.x = Math.Sin(T * h.sp + h.ph) * h.amp;
                foreach (var r in racers)
                {
                    if (r.rescue > 0 || r.air > 0.15 || flying(r)) continue;
                    if (h.type == "podoboo" && h.h > 250) continue;
                    if (Math.Abs(wrapD(r.z - h.z)) < (h.type == "car" ? 230 : 170) && Math.Abs(r.x - h.x) < (h.type == "car" ? 0.22 : 0.17))
                    {
                        if (smasher(r)) { h.dead = 5; if (r.isPlayer && !race.demo) Snd.Tone(700, 0.2, "square", 0.1, 200); }
                        else if (hitRacer(r, "hz") && h.type == "car") r.speed = Math.Min(r.speed, h.speed * 0.5);
                    }
                }
            }
        }
        static void racerCollisions()
        {
            for (int i = 0; i < racers.Count; i++) for (int j = i + 1; j < racers.Count; j++)
                {
                    var a = racers[i]; var b = racers[j]; if (a.rescue > 0 || b.rescue > 0) continue;
                    double dz = wrapD(a.z - b.z), dx = a.x - b.x;
                    if (Math.Abs(dz) < 170 && Math.Abs(dx) < 0.18)
                    {
                        if ((flying(a) && a.pwType != "acro") || (flying(b) && b.pwType != "acro")) continue;
                        bool aS = smasher(a), bS = smasher(b);
                        if (aS && !bS) { hitRacer(b, "star"); continue; }
                        if (bS && !aS) { hitRacer(a, "star"); continue; }
                        double ov = 0.18 - Math.Abs(dx), dir = dx >= 0 ? 1 : -1, tw = a.wgt + b.wgt;
                        a.x += dir * ov * (b.wgt / tw) * 1.1; b.x -= dir * ov * (a.wgt / tw) * 1.1;
                        var back = dz < 0 ? a : b; var front = dz < 0 ? b : a; if (back.speed > front.speed) back.speed = front.speed * 0.97;
                        if ((a.isPlayer || b.isPlayer) && race.bumpT <= 0 && !race.demo) { SFX.bump(); race.bumpT = 0.3; }
                    }
                }
        }
        static List<Racer> computePlaces()
        {
            ranking = racers.OrderBy(r => r.finished ? 0 : 1).ThenBy(r => r.finished ? r.finishTime : -prog(r)).ToList();
            for (int i = 0; i < ranking.Count; i++) ranking[i].place = i + 1;
            return ranking;
        }

        // ---------------- flujo de carrera ----------------
        static void startRace(int ti, List<Entry> grid, bool demo)
        {
            bool tt = !demo && menu.mode == 2 && gp == null, mirror = !demo && menu.mirror;
            buildTrack(ti, mirror);
            string lk = ti + "_" + Math.Min(3, Math.Max(0.5, SCALE)).ToString("0.00");
            if (layerKey != lk) { layerKey = lk; makeLayers(theme, ti * 13 + 1); }
            computeMap();
            curGrid = grid; projs = new List<Proj>(); parts = new List<Part>();
            race = new RaceState { ti = ti, demo = demo, tt = tt, mirror = mirror, count = demo ? 0 : 4, phase = demo ? "race" : "count" };
            racers = grid.Select(e => makeRacer(e, demo)).ToList();
            for (int i = 0; i < racers.Count; i++) { var r = racers[i]; r.z = trackLen - 200 - i * 300; r.x = i % 2 == 1 ? 0.4 : -0.4; r.ai.tx = r.x; }
            players = racers.Where(r => r.isPlayer).OrderBy(r => r.pidx).ToList();
            cams = players.Count > 0 ? players : new List<Racer> { racers[0] };
            if (tt) { foreach (var b in allBoxes) b.t = double.PositiveInfinity; foreach (var p in players) { p.item = "triple"; p.itemCount = 3; p.x = 0; } }
            computePlaces();
            if (!demo) Snd.PlaySong(TRACKS[ti].song, 1);
        }
        static void newDemo()
        {
            var g2 = U.Shuffle(CHARS).Take(8).Select(ch => new Entry { ch = ch, kt = KARTS[U.rng.Next(KARTS.Length)] }).ToList();
            startRace(U.rng.Next(TRACKS.Count), g2, true);
        }
        static List<Entry> makeRoster()
        {
            var humans = new List<Entry>();
            for (int p = 0; p < menu.np; p++) humans.Add(new Entry { ch = CHARS[menu.ch[p]], kt = KARTS[menu.kt[p]], isPlayer = true, pidx = p });
            var others = U.Shuffle(CHARS.Where(c => !humans.Any(h => h.ch == c))).Take(8 - humans.Count).Select(ch => new Entry { ch = ch, kt = KARTS[U.rng.Next(KARTS.Length)] });
            return humans.Concat(others).ToList();
        }
        static List<Entry> gridOrder()
        {
            if (gp != null && gp.race > 0) return gp.roster.OrderByDescending(e => gp.points[e.ch.id]).ToList();
            return U.Shuffle(roster.Where(e => !e.isPlayer)).Concat(roster.Where(e => e.isPlayer)).ToList();
        }
        static void startGP()
        {
            roster = makeRoster(); gp = new GPState { cup = menu.cup, race = 0, roster = roster };
            foreach (var e in roster) gp.points[e.ch.id] = 0;
            startRace(CUPS[menu.cup].tracks[0], gridOrder(), false); state = "race";
        }
        static void startFree()
        {
            gp = null;
            if (menu.mode == 2) { roster = new List<Entry> { new Entry { ch = CHARS[menu.ch[0]], kt = KARTS[menu.kt[0]], isPlayer = true, pidx = 0 } }; startRace(menu.tr, roster, false); }
            else { roster = makeRoster(); startRace(menu.tr, gridOrder(), false); }
            state = "race";
        }
        static void toMenu() { gp = null; newDemo(); state = "mode"; Snd.PlaySong(0, 0.9); }
        static void finishRace()
        {
            var rank = computePlaces(); results = new Results(); double lastT = 0;
            for (int i = 0; i < rank.Count; i++)
            {
                var r = rank[i];
                double t = r.finished ? r.finishTime : race.time + ((LAPS + 1) * trackLen - prog(r)) / (r.top * 0.92);
                t = Math.Max(t, lastT + 0.05); lastT = t;
                results.list.Add(new ResEntry { r = r, time = t, pts = i < PTS.Length ? PTS[i] : 0 });
            }
            if (gp != null) foreach (var e in results.list) gp.points[e.r.ch.id] = gp.points[e.r.ch.id] + e.pts;
            state = "results";
            if (race.tt)
            {
                var p = players[0]; double total = p.finishTime, best = p.lapTimes.Count > 0 ? p.lapTimes.Min() : total; var old = getRec(race.ti);
                results.tt = new TTRes { total = total, laps = new List<double>(p.lapTimes), best = best, old = old, newRec = old == null || total < old.total };
                var ci = System.Globalization.CultureInfo.InvariantCulture;
                if (results.tt.newRec) store(recKey(race.ti), total.ToString(ci) + ";" + best.ToString(ci) + ";" + p.ch.id);
            }
        }
        static void nextAfterResults()
        {
            if (gp != null)
            {
                if (gp.race < 2) { gp.race++; startRace(CUPS[gp.cup].tracks[gp.race], gridOrder(), false); state = "race"; }
                else
                {
                    state = "podium"; SFX.finish();
                    var st = gp.roster.OrderByDescending(e => gp.points[e.ch.id]).ToList();
                    int best = 99; for (int i = 0; i < st.Count; i++) if (st[i].isPlayer) best = Math.Min(best, i + 1);
                    int old = cupBest(gp.cup); if (best <= 3 && (old == 0 || best < old)) store(cupKey(gp.cup), best.ToString());
                }
            }
            else { newDemo(); state = "track"; Snd.PlaySong(0, 0.9); }
        }

        static void updateRace(double dt)
        {
            race.flash = Math.Max(0, race.flash - dt); race.bumpT = Math.Max(0, race.bumpT - dt); race.goT = Math.Max(0, race.goT - dt);
            foreach (var p in players) { p.msgT = Math.Max(0, p.msgT - dt); p.shake = Math.Max(0, p.shake - dt); }
            if (race.phase == "count")
            {
                double prev = race.count; race.count -= dt;
                if (state == "race") foreach (var p in players) if (kp(ctrlFor(p).acc) && p.startPress < 0) p.startPress = race.count;
                foreach (int n in new[] { 3, 2, 1 }) if (prev > n && race.count <= n) SFX.count();
                if (race.count <= 0)
                {
                    race.phase = "race"; race.goT = 1.2; SFX.go();
                    foreach (var p in players) if (held(ctrlFor(p).acc) && p.startPress > 0.6 && p.startPress < 1.9) { p.boost = 1.3; showMsg(p, "¡SALIDA TURBO!", 1.2); SFX.boost(); }
                    foreach (var r in racers) if (!r.isPlayer && U.R() < dif().start) r.boost = 0.9;
                }
                computePlaces(); return;
            }
            race.time += dt;
            foreach (var c in allCoins) if (c.taken > 0) c.taken -= dt;
            foreach (var b in allBoxes) if (b.t > 0) b.t -= dt;
            var camZ0 = cams.Select(c => c.z).ToList();
            foreach (var r in racers)
            {
                Inp inp; double steerOverride = double.NaN;
                if (r.isPlayer && !r.finished && !race.demo) { inp = playerInput(r); if (inp.use && state == "race") useItem(r); if (inp.power && state == "race") activatePower(r); }
                else { inp = aiInput(r, dt); steerOverride = aiSteer; }
                updateRacerWithSteer(r, inp, dt, steerOverride);
            }
            updateProjs(dt); updateHazards(dt); racerCollisions(); computePlaces();
            for (int i = 0; i < cams.Count; i++) { var c = cams[i]; c.skyOff += 0.0008 * findSeg(c.z).curve * (wrapD(c.z - camZ0[i]) / SEG); }
            foreach (var p in players) { if (p.place != p.lastPlace) { p.placeT = 0.35; p.lastPlace = p.place; } p.placeT = Math.Max(0, p.placeT - dt); }
            if (race.phase == "done") { race.doneT += dt; if (race.doneT > 4.5 && state == "race") finishRace(); }
        }
        // La IA usa una dirección decimal (como en la versión web); el jugador usa -1/0/1.
        static void updateRacerWithSteer(Racer r, Inp inp, double dt, double steer)
        {
            if (double.IsNaN(steer)) { updateRacer(r, inp, dt); return; }
            // la física corre con dirección 0 y después se suma el giro decimal de la IA
            int s = steer > 0.33 ? 1 : steer < -0.33 ? -1 : 0;
            inp.steer = 0;
            updateRacer(r, inp, dt);
            if (r.spin <= 0 && r.bullet <= 0 && r.rescue <= 0 && r.frozenT <= 0)
            {
                double grip = Math.Min(1, Math.Abs(r.speed / MAXS) * 1.8);
                r.x += steer * r.han * (theme.slip ? 0.8 : 1) * dt * grip * (r.speed < 0 ? -1 : 1);
                if (!r.isPlayer) r.x = U.Clamp(r.x, -1.1, 1.1);
            }
            r.steerVis = U.Lerp(r.steerVis, s, Math.Min(1, dt * 8));
        }

        // ---------------- partículas ----------------
        static void addPart(double x, double y, double vx, double vy, double life, Color col, double size, double grav)
        {
            parts.Add(new Part { x = x, y = y, vx = vx, vy = vy, life = life, max = life, col = col, size = size, grav = grav });
        }
        static void updateParts(double dt)
        {
            foreach (var p in parts) { p.x += p.vx * dt; p.y += p.vy * dt; p.vy += p.grav * dt; p.life -= dt; }
            parts.RemoveAll(p => p.life <= 0);
        }

        // ---------------- menús ----------------
        static int gridNav(int i, int n, int cols, bool up, bool dn, bool lf, bool rt)
        {
            int o = i;
            if (lf) i = (i - 1 + n) % n; if (rt) i = (i + 1) % n; if (up) i = (i - cols + n) % n; if (dn) i = (i + cols) % n;
            if (i != o) SFX.sel(); return i;
        }
        // selección de personaje/vehículo: en 2 jugadores cada uno mueve su cursor y confirma
        static void selScreen(bool chars, int n, int cols, string next, string prev, bool up, bool dn, bool lf, bool rt, bool ok, bool bk)
        {
            int[] arr = chars ? menu.ch : menu.kt;
            if (menu.np == 1)
            {
                arr[0] = gridNav(arr[0], n, cols, up, dn, lf, rt);
                if (ok) { state = next; SFX.ok(); } else if (bk) { state = prev; SFX.back(); }
                return;
            }
            for (int p = 0; p < 2; p++)
            {
                var c = MENU_KEYS[p];
                if (!menu.lck[p])
                {
                    arr[p] = gridNav(arr[p], n, cols, kp(c.up), kp(c.dn), kp(c.lf), kp(c.rt));
                    if (kp(c.ok))
                    {
                        if (chars && menu.lck[1 - p] && menu.ch[p] == menu.ch[1 - p]) SFX.back();   // no se puede repetir personaje
                        else { menu.lck[p] = true; SFX.ok(); }
                    }
                }
                else if (kp(c.cancel)) { menu.lck[p] = false; SFX.back(); }
            }
            if (menu.lck[0] && menu.lck[1]) { menu.lck = new[] { false, false }; state = next; }
            else if (kp("Escape")) { menu.lck = new[] { false, false }; state = prev; SFX.back(); }
        }
        static void handleInput()
        {
            if (kp("KeyM")) Snd.musicOn = !Snd.musicOn;
            if (kp("F11")) wantFullscreenToggle = true;
            bool up = kp("ArrowUp", "KeyW"), dn = kp("ArrowDown", "KeyS"), lf = kp("ArrowLeft", "KeyA"), rt = kp("ArrowRight", "KeyD"), ok = kp("Enter", "Space"), bk = kp("Escape", "Backspace");
            switch (state)
            {
                case "title": if (kp("KeyI", "KeyH")) { state = "help"; SFX.ok(); } else if (ok) { state = "mode"; SFX.ok(); } break;
                case "help": if (ok || bk || kp("KeyI", "KeyH")) { state = "title"; SFX.back(); } break;
                case "mode":
                    if (up) { menu.row = (menu.row + 4) % 5; SFX.sel(); }
                    if (dn) { menu.row = (menu.row + 1) % 5; SFX.sel(); }
                    if (lf || rt)
                    {
                        int d = rt ? 1 : -1;
                        if (menu.row == 0) menu.np = menu.np == 1 ? 2 : 1;
                        else if (menu.row == 1) menu.mode = (menu.mode + d + 4) % 4;
                        else if (menu.row == 2) { if (menu.mode == 3) menu.sdur = (int)U.Clamp(menu.sdur + d, 0, SOC_DUR.Length - 1); else menu.cc = (int)U.Clamp(menu.cc + d, 0, CC.Length - 1); }
                        else if (menu.row == 3) menu.diff = (int)U.Clamp(menu.diff + d, 0, 2);
                        else if (menu.mode == 3) menu.sfield = (menu.sfield + d + CANCHAS.Length) % CANCHAS.Length;
                        else menu.mirror = !menu.mirror;
                        SFX.sel();
                    }
                    if (ok) { if (menu.mode == 2) menu.np = 1; state = "char"; menu.lck = new[] { false, false }; if (menu.ch[0] == menu.ch[1]) menu.ch[1] = (menu.ch[0] + 1) % CHARS.Length; SFX.ok(); }
                    else if (bk) { state = "title"; SFX.back(); }
                    break;
                case "char":
                    selScreen(true, CHARS.Length, 8, menu.mode == 3 ? "sstart" : "kart", "mode", up, dn, lf, rt, ok, bk);
                    if (state == "sstart") startSoccer();
                    break;
                case "soccer": case "spause": case "sres": socInput(up, dn, ok, bk); break;
                case "kart": selScreen(false, KARTS.Length, 4, menu.mode == 0 ? "cup" : "track", "char", up, dn, lf, rt, ok, bk); break;
                case "cup":
                    if (lf || rt) { menu.cup = (menu.cup + (rt ? 1 : CUPS.Length - 1)) % CUPS.Length; SFX.sel(); }
                    if (ok) { SFX.ok(); startGP(); } else if (bk) { state = "kart"; SFX.back(); }
                    break;
                case "track":
                    menu.tr = gridNav(menu.tr, TRACKS.Count, 4, up, dn, lf, rt);
                    if (ok) { SFX.ok(); startFree(); } else if (bk) { state = "kart"; SFX.back(); }
                    break;
                case "race": if (kp("Escape", "KeyP")) { state = "pause"; menu.p = 0; SFX.sel(); } break;
                case "pause":
                    if (up) { menu.p = (menu.p + 2) % 3; SFX.sel(); }
                    if (dn) { menu.p = (menu.p + 1) % 3; SFX.sel(); }
                    if (kp("Escape", "KeyP")) { state = "race"; break; }
                    if (ok) { if (menu.p == 0) state = "race"; else if (menu.p == 1) { startRace(race.ti, curGrid, false); state = "race"; } else toMenu(); }
                    break;
                case "results": if (ok) { SFX.ok(); nextAfterResults(); } break;
                case "podium": if (ok) { SFX.ok(); toMenu(); } break;
            }
        }

        // ---------------- un cuadro del juego ----------------
        public static void Init()
        {
            InitData(); InitFonts();
            newDemo(); Snd.PlaySong(0, 0.9);
        }
        public static void Frame(double dt)
        {
            T += dt;
            handleInput();
            if (isSoc() && soc != null) { if (state == "soccer") updateSoccer(dt); else if (state == "sres") soc.resT += dt; }
            else if (state != "pause" && race != null) updateRace(dt);
            updateParts(dt);
            bool on = state == "race" && players.Count > 0 && race != null && !race.demo;
            double sp = on ? players.Max(p => Math.Abs(p.speed)) / MAXS : 0; bool bo = on && players.Any(p => p.boost > 0);
            Snd.engFreq = 55 + sp * 150 + (bo ? 40 : 0); Snd.engVol = on ? 0.03 + sp * 0.03 : 0;
            pressed.Clear();
        }
    }
}
