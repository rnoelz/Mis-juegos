using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace FutbolToons
{
    public class Ctrl { public string[] up, down, left, right, shoot, pass, power; }
    public class Part { public double x, y, vx, vy, life, max, size, grav; public Color col; }

    static partial class G
    {
        public static double T, SCALE = 1;
        public static string state = "title";
        public static bool wantFullscreenToggle;
        public static List<Part> parts = new List<Part>();

        // ---------------- teclado ----------------
        public static HashSet<string> keys = new HashSet<string>(), pressed = new HashSet<string>();
        static bool kp(params string[] c) { foreach (var k in c) if (pressed.Contains(k)) return true; return false; }
        static bool held(string[] c) { foreach (var k in c) if (keys.Contains(k)) return true; return false; }
        static Ctrl CTRL_SOLO = new Ctrl { up = new[] { "ArrowUp", "KeyW" }, down = new[] { "ArrowDown", "KeyS" }, left = new[] { "ArrowLeft", "KeyA" }, right = new[] { "ArrowRight", "KeyD" }, shoot = new[] { "Space", "ShiftLeft", "ShiftRight" }, pass = new[] { "KeyE", "KeyX", "Enter" }, power = new[] { "KeyC", "KeyR" } };
        static Ctrl[] CTRL = {
            new Ctrl { up = new[] { "KeyW" }, down = new[] { "KeyS" }, left = new[] { "KeyA" }, right = new[] { "KeyD" }, shoot = new[] { "Space", "ShiftLeft" }, pass = new[] { "KeyE", "KeyQ" }, power = new[] { "KeyR" } },
            new Ctrl { up = new[] { "ArrowUp" }, down = new[] { "ArrowDown" }, left = new[] { "ArrowLeft" }, right = new[] { "ArrowRight" }, shoot = new[] { "ShiftRight", "ControlRight", "Numpad0" }, pass = new[] { "Enter" }, power = new[] { "KeyL" } },
        };

        // ---------------- datos guardados (trofeos) ----------------
        static Dictionary<string, string> save; public static bool noSave;
        static string SavePath() { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FutbolToons", "save.txt"); }
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
            store(k); save[k] = v; if (noSave) return;
            try { Directory.CreateDirectory(Path.GetDirectoryName(SavePath())); File.WriteAllLines(SavePath(), save.Select(p => p.Key + "=" + p.Value).ToArray()); } catch { }
        }
        static int storeInt(string k) { int v; return int.TryParse(store(k) ?? "0", out v) ? v : 0; }
        public static int Trophies(string comp) { return storeInt("trofeo_" + comp); }

        // ---------------- partículas ----------------
        static void addPart(double x, double y, double vx, double vy, double life, Color col, double size, double grav)
        {
            if (parts.Count < 600) parts.Add(new Part { x = x, y = y, vx = vx, vy = vy, life = life, max = life, col = col, size = size, grav = grav });
        }
        static void updateParts(double dt)
        {
            foreach (var p in parts) { p.x += p.vx * dt; p.y += p.vy * dt; p.vy += p.grav * dt; p.life -= dt; }
            parts.RemoveAll(p => p.life <= 0);
        }
        static void confetti(int n) { for (int i = 0; i < n; i++) addPart(U.R() * W, -10 - U.R() * 40, (U.R() - 0.5) * 80, 60 + U.R() * 120, 4, U.Hsl(U.R() * 360, 0.9, 0.6), 7, 30); }

        // ---------------- opciones y selección ----------------
        public static Comp comp;
        public static int np = 1, durIdx = 1, diff = 0, stadIdx = 0, cur = 0, optRow = 0, menuSel = 0, pauseSel = 0;
        public static readonly double[] DURS = { 2, 3, 4, 5 };
        public static readonly string[] DIFF_N = { "FÁCIL", "NORMAL", "DIFÍCIL" };
        public static Team[] pick = new Team[2]; public static int pickStep, squadFor;
        public static List<Ch> squad = new List<Ch>();
        static List<Team> teamList = new List<Team>();
        static Fix curFix; static bool meIsA;

        static Team cloneTeam(Team t) { var c = new Team { id = t.id, name = t.name, abbr = t.abbr, home = t.home, away = t.away, nation = t.nation, rating = t.rating }; return c; }
        // partido de fondo en los menús (todos los jugadores son bots)
        static void newDemo()
        {
            var ts = U.Shuffle(CLUBS.Concat(NATIONS)).Take(2).Select(cloneTeam).ToList();
            ts[0].squad = RandomSquad(); ts[1].squad = RandomSquad(ts[0].squad);
            NewMatch(ts[0], ts[1], 0, false, STADIUMS[U.rng.Next(STADIUMS.Length)], 2, 1);
            M.allAI = true; M.demo = true; M.phase = "play"; M.phaseT = 0;
        }
        static void toMenu() { tour = null; parts.Clear(); newDemo(); state = "menu"; Snd.PlaySong(0, 1); }
        static Stadium compStadium(Comp c)
        {
            switch (c.id) { case "premier": return STADIUMS[0]; case "champions": case "supercopa": return STADIUMS[1]; case "laliga": return STADIUMS[2]; case "mundial": return STADIUMS[3]; default: return STADIUMS[stadIdx]; }
        }
        static void beginTeamSelect()
        {
            teamList = CompTeams(comp); pick = new Team[2]; pickStep = 0; cur = 0; state = "team";
        }
        static void beginSquad(int who)
        {
            squadFor = who; var t = pick[who];
            squad = RandomSquad(who == 1 && pick[0] != null ? pick[0].squad : null);
            cur = 40; state = "squad";
        }
        static void squadDone()
        {
            pick[squadFor].squad = new List<Ch>(squad);
            if (comp.fmt == "friendly")
            {
                if (squadFor == 0 && np > 1) { beginSquad(1); return; }
                if (np == 1) pick[1].squad = RandomSquad(pick[0].squad);
                state = "vs"; vsT = 0;
                return;
            }
            tour = NewTour(comp, pick[0]); state = "hub"; Snd.PlaySong(0, 1);
        }
        static double vsT;
        static void playNext()
        {
            if (comp.fmt == "friendly")
            {
                var m = NewMatch(pick[0], pick[1], np, false, STADIUMS[stadIdx], DURS[durIdx], diff);
                m.compName = ""; state = "match"; Snd.PlaySong(1, 1); parts.Clear(); return;
            }
            curFix = NextFix(tour); meIsA = curFix.a == tour.me;
            bool ko = tour.inKo;
            var st = compStadium(comp);
            if (tour.inKo && tour.stage == "Final") st = comp.id == "mundial" ? STADIUMS[3] : STADIUMS[1];
            var mm = NewMatch(tour.me, meIsA ? curFix.b : curFix.a, 1, ko, st, DURS[durIdx], diff);
            mm.compName = comp.name; mm.stage = StageName(tour);
            state = "match"; Snd.PlaySong(tour.inKo && tour.stage == "Final" ? 2 : 1, 1); parts.Clear();
        }
        static void afterMatch()
        {
            var m = M;
            if (comp.fmt == "friendly") { toMenu(); return; }
            TourResult(tour, m.score[0], m.score[1], m.penGoals[0], m.penGoals[1]);
            newDemo();
            if (tour.done)
            {
                if (tour.champion) { state = "trophy"; store("trofeo_" + comp.id, (Trophies(comp.id) + 1).ToString()); Snd.PlaySong(2, 1); SFX.finish(); parts.Clear(); }
                else { state = "over"; Snd.PlaySong(0, 1); }
            }
            else { state = "hub"; Snd.PlaySong(0, 1); }
        }

        // ---------------- menús ----------------
        static int gridNav(int i, int n, int cols, bool up, bool dn, bool lf, bool rt)
        {
            int o = i;
            if (lf) i = (i - 1 + n) % n; if (rt) i = (i + 1) % n; if (up) i = (i - cols + n) % n; if (dn) i = (i + cols) % n;
            if (i != o) SFX.sel(); return i;
        }
        static void handleInput()
        {
            if (kp("KeyM")) Snd.musicOn = !Snd.musicOn;
            if (kp("F11")) wantFullscreenToggle = true;
            bool up = kp("ArrowUp", "KeyW"), dn = kp("ArrowDown", "KeyS"), lf = kp("ArrowLeft", "KeyA"), rt = kp("ArrowRight", "KeyD"), ok = kp("Enter", "Space"), bk = kp("Escape", "Backspace");
            switch (state)
            {
                case "title": if (ok) { state = "menu"; SFX.ok(); } break;
                case "menu":
                    {
                        // 6 competiciones (2 filas de 3) y abajo: vitrina (6) y cómo jugar (7)
                        int o = menuSel;
                        if (menuSel < 6)
                        {
                            int row = menuSel / 3, col = menuSel % 3;
                            if (lf) col = (col + 2) % 3; if (rt) col = (col + 1) % 3;
                            menuSel = row * 3 + col;
                            if (up && row == 1) menuSel -= 3;
                            else if (dn && row == 0) menuSel += 3;
                            else if (dn && row == 1) menuSel = col < 2 ? 6 : 7;
                        }
                        else { if (lf || rt) menuSel = menuSel == 6 ? 7 : 6; if (up) menuSel = menuSel == 6 ? 3 : 5; }
                        if (menuSel != o) SFX.sel();
                        if (ok)
                        {
                            SFX.ok();
                            if (menuSel == 6) state = "cabinet";
                            else if (menuSel == 7) state = "help";
                            else { comp = COMPS[menuSel]; optRow = 0; np = 1; state = "opts"; }
                        }
                        else if (bk) { state = "title"; SFX.back(); }
                        break;
                    }
                case "opts":
                    {
                        bool fr = comp.fmt == "friendly"; int rows = fr ? 4 : 2;
                        if (up) { optRow = (optRow + rows - 1) % rows; SFX.sel(); }
                        if (dn) { optRow = (optRow + 1) % rows; SFX.sel(); }
                        if (lf || rt)
                        {
                            int d = rt ? 1 : -1;
                            if (optRow == 0) durIdx = (durIdx + d + DURS.Length) % DURS.Length;
                            else if (optRow == 1) diff = (diff + d + 3) % 3;
                            else if (optRow == 2) np = np == 1 ? 2 : 1;
                            else stadIdx = (stadIdx + d + STADIUMS.Length) % STADIUMS.Length;
                            SFX.sel();
                        }
                        if (ok) { SFX.ok(); beginTeamSelect(); }
                        else if (bk) { state = "menu"; SFX.back(); }
                        break;
                    }
                case "team":
                    {
                        cur = gridNav(cur, teamList.Count, 6, up, dn, lf, rt);
                        if (ok)
                        {
                            var t = teamList[cur];
                            if (pickStep == 1 && t == pick[0]) { SFX.back(); break; }
                            SFX.ok(); pick[pickStep] = t;
                            if (comp.fmt == "friendly" && pickStep == 0) { pickStep = 1; cur = (cur + 1) % teamList.Count; }
                            else beginSquad(0);
                        }
                        else if (bk) { SFX.back(); if (pickStep == 1) pickStep = 0; else state = "opts"; }
                        break;
                    }
                case "squad":
                    {
                        if (cur == 40) { if (up) { cur = 32; SFX.sel(); } else if (lf || rt || dn) { } }
                        else if (dn && cur >= 32) { cur = 40; SFX.sel(); }
                        else cur = gridNav(cur, 40, 8, up, dn, lf, rt);
                        if (kp("KeyR")) { squad = squad.Concat(U.Shuffle(CHARS.Where(c => !squad.Contains(c))).Take(5 - squad.Count)).ToList(); SFX.ok(); }
                        if (kp("Backspace") && squad.Count > 0) { squad.RemoveAt(squad.Count - 1); SFX.back(); break; }
                        if (ok)
                        {
                            if (cur == 40) { if (squad.Count == 5) { SFX.ok(); squadDone(); } else SFX.back(); }
                            else
                            {
                                var c = CHARS[cur];
                                if (squad.Contains(c)) { squad.Remove(c); SFX.back(); }
                                else if (squad.Count < 5) { squad.Add(c); SFX.ok(); if (squad.Count == 5) cur = 40; }
                                else SFX.back();
                            }
                        }
                        else if (kp("Escape")) { SFX.back(); if (squadFor == 1) beginSquad(0); else { state = "team"; pickStep = 0; } }
                        break;
                    }
                case "hub":
                    if (ok) { SFX.ok(); state = "vs"; vsT = 0; }
                    else if (bk) { SFX.back(); state = "quit"; pauseSel = 1; }
                    break;
                case "quit":
                    if (up || dn) { pauseSel = 1 - pauseSel; SFX.sel(); }
                    if (ok) { if (pauseSel == 0) toMenu(); else state = "hub"; }
                    else if (bk) state = "hub";
                    break;
                case "vs":
                    if (ok && vsT > 0.3) { SFX.ok(); playNext(); }
                    else if (bk) { SFX.back(); state = comp.fmt == "friendly" ? "squad" : "hub"; }
                    break;
                case "match": if (kp("Escape", "KeyP")) { state = "mpause"; pauseSel = 0; SFX.sel(); } break;
                case "mpause":
                    if (up || dn) { pauseSel = 1 - pauseSel; SFX.sel(); }
                    if (kp("Escape", "KeyP")) { state = "match"; break; }
                    if (ok) { if (pauseSel == 0) state = "match"; else { SFX.back(); toMenu(); } }
                    break;
                case "mres":
                    if (M.resT < 0.8) break;
                    if (kp("Enter")) { SFX.ok(); afterMatch(); }
                    else if (kp("KeyR") && comp.fmt == "friendly") { SFX.ok(); pick[0].squad = M.team[0].squad; playNext(); }
                    break;
                case "trophy": if (ok) { SFX.ok(); toMenu(); } break;
                case "over": if (ok) { SFX.ok(); toMenu(); } break;
                case "cabinet": case "help": if (ok || bk) { SFX.back(); state = "menu"; } break;
            }
        }

        // ---------------- un cuadro del juego ----------------
        public static void Init()
        {
            InitData(); InitFonts();
            newDemo(); Snd.PlaySong(0, 1);
        }
        public static void Frame(double dt)
        {
            T += dt;
            handleInput();
            if (M != null)
            {
                if (M.demo)
                {
                    UpdateMatch(dt);
                    if (M.phase == "end" || M.clock > M.dur - 1) newDemo();
                }
                else if (state == "match")
                {
                    UpdateMatch(dt);
                    if (M.phase == "end" && M.phaseT <= 0) { state = "mres"; M.resT = 0; }
                }
                else if (state == "mres") M.resT += dt;
            }
            if (state == "vs") vsT += dt;
            if (state == "trophy" && U.R() < 0.6) confetti(2);
            updateParts(dt);
            pressed.Clear();
        }
    }
}
