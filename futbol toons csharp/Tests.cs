using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace FutbolToons
{
    // Modo de prueba: "Futbol Toons.exe --test carpeta"
    // Juega solo (sin ventana ni sonido), revisa que todo funcione y guarda capturas PNG.
    static partial class G
    {
        static string tdir; static StreamWriter tlog; static Bitmap tbmp; static Graphics tg; static int tfails;
        static void L(string s) { tlog.WriteLine(s); tlog.Flush(); }
        static void Step(double secs) { int n = (int)Math.Round(secs * 60); for (int i = 0; i < n; i++) Frame(1.0 / 60); }
        static void Tap(string k) { keys.Add(k); pressed.Add(k); Frame(1.0 / 60); keys.Remove(k); Frame(1.0 / 60); }
        static void Shot(string name) { tg.Clear(Color.Black); RenderTo(tg, tbmp.Width / 960.0, 0, 0); tbmp.Save(Path.Combine(tdir, name + ".png"), ImageFormat.Png); }
        static void Try(string name, Action a)
        {
            try { a(); L("OK   " + name); }
            catch (Exception e) { tfails++; L("FAIL " + name + ": " + e); }
        }
        static string sq(Team t) { return string.Join(",", t.squad.Select(c => c.name).ToArray()); }

        public static void RunTests(string dir)
        {
            tdir = dir; Directory.CreateDirectory(dir);
            tlog = new StreamWriter(Path.Combine(dir, "log.txt"));
            noSave = true;
            tbmp = new Bitmap(1600, 900, PixelFormat.Format32bppPArgb); tg = Graphics.FromImage(tbmp);
            Try("init", () => Init());
            Try("título", () => { Step(0.5); Shot("01_titulo"); });
            Try("menú", () => { Tap("Enter"); L("  estado=" + state); Shot("02_menu"); });
            Try("amistoso: opciones, equipos y personajes", () =>
            {
                menuSel = 0; Tap("Enter"); L("  estado=" + state + " comp=" + comp.name); Shot("03_opciones");
                Tap("Enter"); L("  estado=" + state + " equipos=" + teamList.Count); cur = teamList.FindIndex(t => t.id == "bar"); Step(0.1); Shot("04_equipos");
                Tap("Enter"); cur = teamList.FindIndex(t => t.id == "rma"); Tap("Enter");
                L("  estado=" + state + " J1=" + pick[0].name + " rival=" + pick[1].name + " plantilla=" + squad.Count);
                squad.Clear(); foreach (var id in new[] { "goku", "bob", "pikachu", "homero" }) squad.Add(CHARS.First(c => c.id == id)); cur = CHARS.ToList().FindIndex(c => c.id == "scooby");
                Step(0.1); Shot("05_plantilla"); Tap("Enter"); L("  plantilla=" + squad.Count + " cursor=" + cur);
                Tap("Enter"); L("  estado=" + state + " rival: " + sq(pick[1])); Step(0.6); Shot("06_vs");
                Tap("Enter"); L("  estado=" + state + " estadio=" + M.st.name + " kits=" + M.kit[0].shirt.Name + "/" + M.kit[1].shirt.Name);
                Step(1.5); Shot("07_cuenta"); Step(2.2); L("  fase=" + M.phase);
            });
            Try("partido: tiro, pase y súper tiro", () =>
            {
                var s = M; var P = s.ctl[0]; s.ball.owner = P; P.x = 500; P.y = 30; P.face = 0; ballFollow();
                foreach (var q in s.pl) if (q.team == 1 && !q.gk) { q.x = -300; q.y = (q.slot - 1.5) * 120; }
                keys.Add("KeyD"); Step(0.3); keys.Add("Space"); pressed.Add("Space"); Step(0.4); keys.Remove("Space"); Step(0.05);
                L("  tiro: vel=" + Math.Round(hyp(s.ball.vx, s.ball.vy)) + " tiros=" + s.shots[0]); Step(0.3); keys.Clear(); Shot("08_tiro"); Step(1);
                L("  marcador=" + s.score[0] + "-" + s.score[1] + " fase=" + s.phase); Step(4.5);
                s.phase = "play"; var C0 = s.ctl[0]; var Q = s.pl.First(p => p.team == 0 && !p.gk && p != C0);
                C0.stun = C0.frozen = C0.kickCD = 0; s.ball.owner = C0; C0.x = 0; C0.y = 0; Q.x = 260; Q.y = 0; C0.face = 0; ballFollow();
                foreach (var q in s.pl) if (q.team == 1 && !q.gk) { q.x = -400; q.y = (q.slot - 1.5) * 120; }
                Tap("KeyE"); L("  pase hacia " + (s.ball.passTo != null ? s.ball.passTo.ch.name : "-") + " controla=" + s.ctl[0].ch.name); Step(0.6);
                foreach (var id in new[] { "goku", "bob", "pikachu", "homero", "scooby" })
                {
                    var p2 = s.ctl[0]; s.phase = "play"; p2.ch = CHARS.First(c => c.id == id); s.ball.owner = p2; p2.x = 350; p2.y = 0; p2.face = 0; p2.stun = p2.frozen = 0; s.meter[0] = 1; ballFollow();
                    foreach (var q in s.pl) if (q.team == 1 && !q.gk) { q.x = -300; q.y = (q.slot - 1.5) * 120; q.frozen = 0; }
                    Tap("KeyC"); Step(0.12);
                    L("  " + p2.ch.name + ": súper=" + s.ball.sup + " vel=" + Math.Round(hyp(s.ball.vx, s.ball.vy)));
                    if (id == "bob") Shot("09_super_burbuja"); if (id == "pikachu") Shot("10_super_rayo");
                    Step(3.5); if (s.phase == "goal") { if (id == "goku") Shot("11_gol"); Step(4.5); }
                }
                L("  marcador=" + s.score[0] + "-" + s.score[1]);
                M.clock = M.dur / 2 - 0.05; M.phase = "play"; Step(0.3); L("  medio tiempo: fase=" + M.phase); Shot("12_medio_tiempo"); Step(4.5); L("  segundo tiempo: mitad=" + M.half);
                M.clock = M.dur - 0.05; Step(4.5); L("  fin: estado=" + state); Shot("13_resultado"); Step(1);
                Tap("Enter"); L("  después: estado=" + state);
            });
            Try("partido completo entre bots + penales", () =>
            {
                var a = cloneTeam(CLUBS[0]); var b = cloneTeam(CLUBS[7]); a.squad = RandomSquad(); b.squad = RandomSquad(a.squad);
                NewMatch(a, b, 1, true, STADIUMS[1], 2, 1); M.allAI = true; state = "match";
                int goals = 0; bool inG = false;
                for (int i = 0; i < 60 * 260 && state == "match"; i++) { Frame(1.0 / 60); if (M.phase == "goal" && !inG) { goals++; inG = true; } else if (M.phase != "goal") inG = false; if (M.phase == "pens" && M.penTaken[0] + M.penTaken[1] == 1 && M.penStep == "fly") Shot("14_penal"); }
                L("  estado=" + state + " marcador=" + M.score[0] + "-" + M.score[1] + " penales=" + M.penGoals[0] + "-" + M.penGoals[1] + " ganador=" + M.Winner + " tiros=" + M.shots[0] + "/" + M.shots[1]);
                if (state != "mres") throw new Exception("el partido no terminó");
                // forzar penales y probarlos con un humano
                NewMatch(a, b, 1, true, STADIUMS[3], 2, 0); state = "match"; M.phase = "play"; M.half = 2; M.clock = M.dur - 0.02; M.score[0] = M.score[1] = 1; Step(0.1);
                L("  penales: fase=" + M.phase); Step(2.5); L("  paso=" + M.penStep + " tira=" + M.team[M.penTeam].abbr); Shot("15_penales_apuntar");
                for (int i = 0; i < 400 && M.phase == "pens"; i++) { if (M.penStep == "aim") { if (penHumanShoots() && M.penT > 0.8) Tap("Space"); else Step(0.1); } else Step(0.2); }
                Step(4); L("  penales: " + M.penGoals[0] + "-" + M.penGoals[1] + " ganador=" + M.Winner + " estado=" + state); Shot("16_penales_fin");
            });
            foreach (var cid in new[] { "premier", "laliga", "champions", "supercopa", "mundial" })
            {
                string id = cid;
                Try("torneo: " + id, () =>
                {
                    comp = COMPS.First(c => c.id == id); var list = CompTeams(comp); var me = list[0]; me.squad = RandomSquad();
                    tour = NewTour(comp, me); state = "hub"; Step(0.1);
                    L("  equipos=" + string.Join(",", tour.teams.Select(t => t.abbr).ToArray()) + " etapa=" + StageName(tour));
                    Shot("20_" + id + "_hub");
                    int guard = 0;
                    while (!tour.done && guard++ < 12)
                    {
                        if (guard == 1) { state = "vs"; vsT = 1; Step(0.1); if (id == "mundial") Shot("21_mundial_vs"); }
                        TourResult(tour, 2, 1, 0, 0);
                        if (guard == 2 && id != "supercopa") { state = "hub"; Step(0.05); Shot("22_" + id + "_hub2"); }
                    }
                    L("  terminado=" + tour.done + " campeón=" + tour.champion + " partidos=" + guard + " " + tour.endText);
                    if (!tour.champion) throw new Exception("ganando todo debería ser campeón");
                    state = "trophy"; store("trofeo_" + id, (Trophies(id) + 1).ToString()); Step(0.8); Shot("23_" + id + "_trofeo");
                    // torneo perdiendo
                    me.squad = RandomSquad(); tour = NewTour(comp, me); guard = 0;
                    while (!tour.done && guard++ < 12) TourResult(tour, 0, 3, 0, 0);
                    L("  perdiendo: " + tour.endText);
                });
            }
            Try("mundial: selección de selecciones", () => { comp = COMPS.First(c => c.id == "mundial"); beginTeamSelect(); Step(0.1); Shot("24_mundial_equipos"); });
            Try("vitrina y ayuda", () => { state = "cabinet"; Step(0.1); Shot("25_vitrina"); state = "help"; Step(0.1); Shot("26_ayuda"); });
            Try("los 40 personajes y estadios", () =>
            {
                comp = COMPS[0]; pick[0] = CLUBS[2]; squadFor = 0; squad = CHARS.Take(5).ToList(); cur = 0; state = "squad"; Step(0.1); Shot("27_personajes");
                for (int i = 0; i < STADIUMS.Length; i++)
                {
                    var a = cloneTeam(CLUBS[i]); var b = cloneTeam(NATIONS[i]); a.squad = CHARS.Skip(i * 5 % 35).Take(5).ToList(); b.squad = CHARS.Skip((i * 5 + 5) % 35).Take(5).ToList();
                    NewMatch(a, b, 2, false, STADIUMS[i], 3, 1); state = "match"; M.phase = "play"; keys.Add("KeyD"); keys.Add("ArrowLeft"); Step(1.2); keys.Clear();
                    Shot("28_estadio_" + i);
                }
            });
            Try("rendimiento", () =>
            {
                var sw = Stopwatch.StartNew(); int n = 60;
                for (int i = 0; i < n; i++) { Frame(1.0 / 60); tg.Clear(Color.Black); RenderTo(tg, tbmp.Width / 960.0, 0, 0); }
                L("  " + (sw.Elapsed.TotalMilliseconds / n).ToString("0.0") + " ms por cuadro a 1600x900");
                using (var bm = new Bitmap(1280, 720, PixelFormat.Format32bppPArgb)) using (var gg = Graphics.FromImage(bm))
                {
                    sw.Restart(); for (int i = 0; i < n; i++) { Frame(1.0 / 60); gg.Clear(Color.Black); RenderTo(gg, 1280 / 960.0, 0, 0); }
                    L("  " + (sw.Elapsed.TotalMilliseconds / n).ToString("0.0") + " ms por cuadro a 1280x720 (ventana normal)");
                    Func<Action, double> time = a => { var s2 = Stopwatch.StartNew(); for (int i = 0; i < 30; i++) a(); return s2.Elapsed.TotalMilliseconds / 30; };
                    Action prep = () => { gg.ResetTransform(); gg.ScaleTransform(1280 / 960f, 1280 / 960f); g = gg; SCALE = 1280 / 960.0; ALPHA = 1; };
                    L("  tribuna: " + time(() => { prep(); drawStands(); }).ToString("0.0") + " ms");
                    L("  jugadores: " + time(() => { prep(); foreach (var p in M.pl) drawPl(p); }).ToString("0.0") + " ms");
                    L("  marcador: " + time(() => { prep(); drawMatchHUD(); }).ToString("0.0") + " ms");
                    gg.ResetTransform();
                }
            });
            L(tfails == 0 ? "TODO BIEN" : ("FALLOS: " + tfails));
            tlog.Close();
        }
    }
}
