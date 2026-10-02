using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace SuperKart
{
    // Modo de prueba: "Super Kart.exe --test carpeta"
    // Juega solo (sin ventana ni sonido), revisa que todo funcione y guarda capturas PNG.
    static partial class SK
    {
        static string tdir; static StreamWriter tlog; static Bitmap tbmp; static Graphics tg; static int tfails;
        static void L(string s) { tlog.WriteLine(s); tlog.Flush(); }
        static void Step(double secs) { int n = (int)Math.Round(secs * 60); for (int i = 0; i < n; i++) Frame(1.0 / 60); }
        static void Tap(string k) { keys.Add(k); pressed.Add(k); Frame(1.0 / 60); keys.Remove(k); Frame(1.0 / 60); }
        static void Shot(string name)
        {
            tg.Clear(Color.Black);
            RenderTo(tg, tbmp.Width / 960.0, 0, 0);
            tbmp.Save(Path.Combine(tdir, name + ".png"), ImageFormat.Png);
        }
        static void Try(string name, Action a)
        {
            try { a(); L("OK   " + name); }
            catch (Exception e) { tfails++; L("FAIL " + name + ": " + e); }
        }

        public static void RunTests(string dir)
        {
            tdir = dir; Directory.CreateDirectory(dir);
            tlog = new StreamWriter(Path.Combine(dir, "log.txt"));
            noSave = true;
            tbmp = new Bitmap(1600, 900, PixelFormat.Format32bppPArgb); tg = Graphics.FromImage(tbmp);
            SCALE = 1600 / 960.0;
            Try("init", () => Init());
            Try("titulo", () => { Step(0.5); Shot("01_titulo"); });
            Try("ayuda", () => { Tap("KeyI"); L("  estado=" + state); Shot("02_ayuda"); Tap("Escape"); });
            Try("menu modo", () => { Tap("Enter"); L("  estado=" + state); Shot("03_modo"); });
            Try("personajes", () => { Tap("Enter"); menu.ch[0] = 20; Step(0.2); L("  estado=" + state + " personajes=" + CHARS.Length); Shot("04_personajes"); });
            Try("karts", () => { Tap("Enter"); Step(0.2); Shot("05_karts"); });
            Try("copas", () => { Tap("Enter"); Step(0.2); Shot("06_copas"); });
            Try("gran premio", () =>
            {
                Tap("Enter"); L("  estado=" + state + " pista=" + TRACKS[race.ti].name + " jugador=" + players[0].ch.name);
                keys.Add("ArrowUp"); Step(1); Shot("07_cuenta"); Step(4.5);
                L("  fase=" + race.phase + " vel=" + Math.Round(players[0].speed) + " lugar=" + players[0].place); Shot("08_carrera");
            });
            Try("objetos", () =>
            {
                foreach (var it in ITEM_KEYS)
                {
                    var P = players[0]; P.item = it; P.itemCount = ITEM_COUNT.ContainsKey(it) ? ITEM_COUNT[it] : 1; P.roulette = 0;
                    Tap("KeyX"); Step(0.3);
                }
                L("  proyectiles=" + projs.Count + " monedas=" + players[0].coins);
                Step(1.5); Shot("09_objetos");
            });
            Try("12 pistas", () =>
            {
                menu.mode = 1;
                for (int i = 0; i < TRACKS.Count; i++)
                {
                    menu.tr = i; startFree(); race.count = 0.01; keys.Add("ArrowUp"); Step(3);
                    int pads = segments.Count(s => !double.IsNaN(s.pad)), ramps = segments.Count(s => s.ramp);
                    L("  pista " + i + " " + TRACKS[i].name + ": segs=" + segments.Count + " vuelta~" + (trackLen / MAXS).ToString("0.0") + "s turbos=" + pads + " rampas=" + ramps + " peligros=" + hazards.Count + " lugar=" + players[0].place);
                    Shot("10_pista_" + i.ToString("00"));
                }
            });
            Try("fin de carrera", () =>
            {
                menu.mode = 0; menu.cup = 0; startGP(); race.count = 0.01; Step(0.5);
                var P = players[0]; P.lap = LAPS; P.maxLap = LAPS; P.z = trackLen - 50; P.speed = MAXS;
                Step(6); L("  estado=" + state); Shot("11_resultados");
                Tap("Enter"); L("  siguiente: estado=" + state + " carrera=" + gp.race);
                gp.race = 2; finishRace(); Tap("Enter"); L("  podio: estado=" + state); Step(0.5); Shot("12_podio");
                Tap("Enter"); L("  menu: estado=" + state);
            });
            Try("2 jugadores", () =>
            {
                menu.np = 2; menu.mode = 1; menu.tr = 10; menu.ch = new[] { 20, 21 }; menu.kt = new[] { 0, 5 };
                startFree(); race.count = 0.01; keys.Add("KeyW"); keys.Add("ArrowUp"); Step(4);
                L("  jugadores=" + string.Join(", ", players.Select(p => p.ch.name + "/J" + (p.pidx + 1)).ToArray()) + " vel=" + string.Join(",", players.Select(p => Math.Round(p.speed).ToString()).ToArray()));
                Shot("13_dos_jugadores");
                keys.Clear(); menu.np = 1;
            });
            Try("contrarreloj", () =>
            {
                menu.mode = 2; menu.tr = 6; menu.mirror = true; startFree(); race.count = 0.01; keys.Add("ArrowUp"); Step(2);
                L("  tt=" + race.tt + " espejo=" + race.mirror + " corredores=" + racers.Count + " objeto=" + players[0].item + "x" + players[0].itemCount);
                var P = players[0]; int ri = segments.FindIndex(s => s.ramp);
                if (ri >= 0) { P.z = ri * SEG - 300; P.x = 0; P.speed = MAXS; Step(0.25); L("  en el aire=" + P.air.ToString("0.00")); Tap("Space"); Step(1.2); L("  turbo del truco=" + P.boost.ToString("0.00")); Shot("14_rampa"); }
                P.lap = LAPS; P.maxLap = LAPS; P.z = trackLen - 50; P.speed = MAXS; Step(6);
                L("  estado=" + state + " total=" + (results.tt != null ? U.FmtTime(results.tt.total) : "-") + " nuevoRecord=" + (results.tt != null && results.tt.newRec) + " guardado=" + (getRec(menu.tr) != null));
                Shot("15_contrarreloj");
                menu.mirror = false; keys.Clear();
            });
            Try("amigos en carrera", () =>
            {
                menu.mode = 1;
                var grid = new List<Entry>();
                foreach (var id in new[] { "flaco", "willito", "chevez", "yoni", "mario", "peach", "bowser" }) grid.Add(new Entry { ch = CHARS.First(c => c.id == id), kt = KARTS[grid.Count % KARTS.Length] });
                grid.Add(new Entry { ch = CHARS.First(c => c.id == "luigi"), kt = KARTS[0], isPlayer = true });
                roster = grid; startRace(0, grid, false); state = "race"; race.count = 0.01; Step(1.5);
                double pz = players[0].z;
                for (int i = 0; i < racers.Count; i++) { var r = racers[i]; if (r.isPlayer) continue; r.z = wrapZ(pz + 450 + (i / 4) * 500); r.x = -0.75 + (i % 4) * 0.5; }
                Step(0.05); Shot("16_amigos");
                state = "char"; menu.ch[0] = 21; Step(0.1); Shot("17_seleccion_amigos");
            });
            Try("súper poderes (24 personajes)", () =>
            {
                state = "race"; menu.np = 1; menu.mode = 1; menu.tr = 0;
                for (int ci = 0; ci < CHARS.Length; ci++)
                {
                    menu.ch = new[] { ci, 0 }; startFree(); state = "race"; race.count = 0.01; keys.Add("ArrowUp"); Step(1.5);
                    var P = players[0]; P.meter = 1; Tap("KeyC"); Step(0.2);
                    L("  " + CHARS[ci].name + ": poder=" + PowerOf(CHARS[ci])[0] + " activo=" + (P.pwType == "" ? "-" : P.pwType) + " pwT=" + P.pwT.ToString("0.0") + " boost=" + P.boost.ToString("0.0") +
                      " ghost=" + P.ghostT.ToString("0.0") + " monedas=" + P.coins + " proys=" + projs.Count + " congelados=" + racers.Count(o => o.frozenT > 0) + " tumbados=" + racers.Count(o => o.spin > 0) + " barra=" + P.meter.ToString("0.00"));
                }
                // los bots también usan su poder
                menu.ch = new[] { 0, 0 }; startFree(); state = "race"; race.count = 0.01; foreach (var r in racers) r.meter = 1; Step(5);
                L("  bots que usaron su poder: " + racers.Count(r => !r.isPlayer && r.meter < 1) + "/7");
                // capturas de los poderes de los amigos
                foreach (var id in new[] { "flaco", "willito", "chevez", "yoni" })
                {
                    var grid = new List<Entry>();
                    foreach (var o in new[] { "flaco", "willito", "chevez", "yoni", "peach", "rosalina", "bowser", "mario" }.Where(x => x != id).Take(7)) grid.Add(new Entry { ch = CHARS.First(c => c.id == o), kt = KARTS[grid.Count % KARTS.Length] });
                    grid.Add(new Entry { ch = CHARS.First(c => c.id == id), kt = KARTS[0], isPlayer = true });
                    roster = grid; startRace(0, grid, false); state = "race"; race.count = 0.01; Step(1.5);
                    var P = players[0]; double pz = P.z;
                    for (int i = 0; i < racers.Count; i++) { var r = racers[i]; if (r.isPlayer) continue; r.z = wrapZ(pz + 700 + (i / 4) * 700); r.x = -0.6 + (i % 4) * 0.4; r.speed = 0; }
                    P.speed = 0; computePlaces(); P.meter = 1; activatePower(P); Step(0.3);
                    if (id == "flaco") P.laserT = 0.6; if (id == "chevez") P.teleT = 0.3;
                    Shot("18_poder_" + id);
                }
                keys.Clear();
            });
            Try("guerreros Z", () =>
            {
                keys.Clear(); menu.np = 1; state = "char"; menu.mode = 1; menu.ch[0] = CHARS.ToList().FindIndex(c => c.id == "goku"); Step(0.1); Shot("30_z_seleccion");
                state = "title"; Step(0.1); Shot("31_z_titulo");
                foreach (var id in new[] { "goku", "vegeta", "trunks", "broly" })
                {
                    var grid = new List<Entry>();
                    foreach (var o in new[] { "roshi", "bulma", "jiren", "broly", "a18", "trunks", "goku", "vegeta" }.Where(x => x != id).Take(7)) grid.Add(new Entry { ch = CHARS.First(c => c.id == o), kt = KARTS[grid.Count % KARTS.Length] });
                    grid.Add(new Entry { ch = CHARS.First(c => c.id == id), kt = KARTS[0], isPlayer = true });
                    roster = grid; startRace(0, grid, false); state = "race"; race.count = 0.01; Step(1.5);
                    var P = players[0]; double pz = P.z;
                    for (int i = 0; i < racers.Count; i++) { var r = racers[i]; if (r.isPlayer) continue; r.z = wrapZ(pz + 700 + (i / 4) * 700); r.x = -0.6 + (i % 4) * 0.4; r.speed = 0; }
                    P.speed = 0; computePlaces(); P.meter = 1; activatePower(P); Step(0.1);
                    L("  " + id + ": poder=" + PowerOf(P.ch)[0] + " activo=" + P.pwType + " laser=" + P.laserT.ToString("0.0") + " tumbados=" + racers.Count(o => o.spin > 0));
                    Shot("32_z_poder_" + id);
                }
                menu.ch = new[] { CHARS.ToList().FindIndex(c => c.id == "goku"), 0 };
                var tz = new[] { new[] { "goku", "vegeta", "gohan", "piccolo" }, new[] { "jiren", "roshi", "bulma", "broly" } }
                    .Select(ids => ids.Select(i => CHARS.First(c => c.id == i)).ToList()).ToArray();
                menu.sfield = 0; startSoccer(tz); soc.phaseT = 0.01; Step(0.3);
                var V = soc.pl.First(p => p.ch.id == "vegeta"); V.pwT = 3; V.x = soc.ctl[0].x + 60; V.y = soc.ctl[0].y + 40;
                Step(0.05); Shot("33_z_futbol");
                soc = null; toMenu(); menu.ch = new[] { 0, 1 };
            });
            Try("fútbol: menú y saque", () =>
            {
                keys.Clear(); state = "mode"; menu.np = 1; menu.row = 1; menu.mode = 2; Tap("ArrowRight");
                L("  modo=" + menu.mode); Shot("19_futbol_menu");
                Tap("Enter"); L("  estado=" + state); menu.ch[0] = 0; Tap("Enter");
                L("  estado=" + state + " rojo=" + string.Join(",", soc.pl.Where(p => p.team == 0).Select(p => p.ch.name + (p.gk ? "(arq)" : "")).ToArray()) +
                  " azul=" + string.Join(",", soc.pl.Where(p => p.team == 1).Select(p => p.ch.name + (p.gk ? "(arq)" : "")).ToArray()));
                Step(1.5); Shot("20_futbol_cuenta"); Step(2.2);
                L("  fase=" + soc.phase + " dueño=" + (soc.ball.owner != null ? soc.ball.owner.ch.name : "-"));
            });
            Try("fútbol: tiro y pase", () =>
            {
                var s = soc; var P = s.ctl[0]; s.ball.owner = P; P.x = 450; P.y = 40; P.face = 0; socBallFollow();
                foreach (var q in s.pl) if (q.team == 1 && !q.gk) { q.x = -300; q.y = (q.slot - 1) * 150; }
                keys.Add("KeyD"); Step(0.3); keys.Add("Space"); pressed.Add("Space"); Step(0.4); keys.Remove("Space"); Step(0.05);
                L("  tiro: vel pelota=" + Math.Round(hyp(s.ball.vx, s.ball.vy)) + " dueño=" + (s.ball.owner != null ? s.ball.owner.ch.name : "-"));
                Step(1.2); keys.Clear(); L("  marcador=" + s.score[0] + "-" + s.score[1] + " fase=" + s.phase); Shot("21_futbol_tiro");
                Step(4.5);
                var Q = s.pl.First(p => p.team == 0 && !p.gk && p != s.ctl[0]); var C0 = s.ctl[0];
                s.phase = "play"; C0.stun = C0.frozen = C0.kickCD = 0; s.ball.owner = C0; C0.x = 0; C0.y = 0; Q.x = 250; Q.y = 0; C0.face = 0; socBallFollow();
                foreach (var q in s.pl) if (q.team == 1 && !q.gk) { q.x = -400; q.y = (q.slot - 1) * 150; }
                Tap("KeyE"); L("  pase hacia " + (s.ball.passTo != null ? s.ball.passTo.ch.name : "-") + " controla=" + s.ctl[0].ch.name); Step(0.6);
                L("  pase: ahora controla=" + s.ctl[0].ch.name + " (antes " + C0.ch.name + ") dueño=" + (s.ball.owner != null ? s.ball.owner.ch.name : "-"));
                Shot("22_futbol_pase");
            });
            Try("fútbol: súper tiros", () =>
            {
                var s = soc;
                foreach (var id in new[] { "mario", "rosalina", "kingboo", "willito", "chevez" })
                {
                    var P = s.ctl[0]; P.ch = CHARS.First(c => c.id == id); s.phase = "play";
                    s.ball.owner = P; P.x = 300; P.y = 0; P.face = 0; P.stun = P.frozen = 0; s.meter[0] = 1; socBallFollow();
                    foreach (var q in s.pl) if (q.team == 1 && !q.gk) { q.x = -300; q.y = (q.slot - 1) * 150; q.frozen = 0; }
                    Tap("KeyC"); Step(0.12);
                    L("  " + P.ch.name + ": súper=" + s.ball.sup + " vel=" + Math.Round(hyp(s.ball.vx, s.ball.vy)) + " congelados=" + s.pl.Count(p => p.frozen > 0) + " barra=" + s.meter[0].ToString("0.0"));
                    if (id == "mario") Shot("23_futbol_super");
                    Step(3.5); if (s.phase == "goal") Step(4.5);
                }
                L("  marcador=" + s.score[0] + "-" + s.score[1]);
            });
            Try("fútbol: partido completo entre bots", () =>
            {
                menu.sdur = 0; menu.diff = 1; startSoccer(); soc.allAI = true; foreach (var p in soc.pl) p.human = -1;
                int goals = 0, shots = 0; SBall b = soc.ball; double lastV = 0; bool shotGoal = false;
                for (int i = 0; i < 60 * 200 && state == "soccer"; i++)
                {
                    Frame(1.0 / 60); double v = hyp(b.vx, b.vy); if (b.owner == null && v > 700 && lastV < 700) shots++; lastV = v;
                    if (soc.phase == "goal" && !shotGoal) { goals++; shotGoal = true; if (goals == 1) { Step(0.3); Shot("24_futbol_gol"); } } else if (soc.phase != "goal") shotGoal = false;
                }
                L("  estado=" + state + " marcador=" + soc.score[0] + "-" + soc.score[1] + " goles=" + goals + " tiros=" + shots + " gol de oro=" + soc.golden + " reloj=" + soc.clock.ToString("0"));
                if (state != "sres") throw new Exception("el partido no terminó");
                Step(1.2); Shot("25_futbol_resultado");
                Tap("Enter"); L("  revancha: estado=" + state + " marcador=" + soc.score[0] + "-" + soc.score[1]);
            });
            Try("fútbol: 2 jugadores y canchas", () =>
            {
                menu.np = 2; menu.ch = new[] { 20, 21 }; menu.sdur = 1;
                for (int f = 0; f < CANCHAS.Length; f++)
                {
                    menu.sfield = f; startSoccer(); soc.phaseT = 0.01; Step(0.1);
                    keys.Add("KeyD"); keys.Add("ArrowLeft"); Step(1.5); keys.Clear();
                    soc.camX = f % 2 == 0 ? -360 : 360; Shot("26_cancha_" + f);
                }
                L("  J1=" + soc.ctl[0].ch.name + " J2=" + soc.ctl[1].ch.name + " fase=" + soc.phase);
                Tap("Escape"); L("  pausa: estado=" + state); Shot("27_futbol_pausa");
                menu.p = 2; Tap("Enter"); L("  salir: estado=" + state + " soc=" + (soc == null ? "null" : "ok"));
                menu.np = 1; menu.ch = new[] { 0, 1 };
                // rendimiento del fútbol
                startSoccer(); soc.phaseT = 0.01; Step(2);
                var sw = Stopwatch.StartNew(); int n = 60;
                for (int i = 0; i < n; i++) { Frame(1.0 / 60); tg.Clear(Color.Black); RenderTo(tg, tbmp.Width / 960.0, 0, 0); }
                L("  fútbol: " + (sw.Elapsed.TotalMilliseconds / n).ToString("0.0") + " ms por cuadro a 1600x900");
                soc = null; toMenu();
            });
            Try("rendimiento", () =>
            {
                state = "race"; menu.mode = 1; menu.tr = 0; startFree(); race.count = 0.01; keys.Add("ArrowUp"); Step(2);
                var sw = Stopwatch.StartNew(); int n = 60;
                for (int i = 0; i < n; i++) { Frame(1.0 / 60); tg.Clear(Color.Black); RenderTo(tg, tbmp.Width / 960.0, 0, 0); }
                double ms = sw.Elapsed.TotalMilliseconds / n;
                L("  " + ms.ToString("0.0") + " ms por cuadro a 1600x900 (~" + (1000 / ms).ToString("0") + " FPS)");
                // desglose de tiempos
                Func<Action, double> time = a => { var s2 = Stopwatch.StartNew(); for (int i = 0; i < 30; i++) a(); return s2.Elapsed.TotalMilliseconds / 30; };
                double sc = tbmp.Width / 960.0;
                Action prep = () => { tg.ResetTransform(); tg.ScaleTransform((float)sc, (float)sc); g = tg; SCALE = sc; ALPHA = 1; VW = W; VX = 0; };
                L("  logica: " + time(() => Frame(1.0 / 60)).ToString("0.0") + " ms");
                L("  cielo+fondos: " + time(() => { prep(); Gradient(-10, -10, VW + 20, H / 2.0 + 12, theme.sky[0], theme.sky[1]); drawLayer(layerFar, 0, 4); drawLayer(layerNear, 0, 6); }).ToString("0.0") + " ms");
                L("  mundo completo: " + time(() => { prep(); renderWorld(players[0]); }).ToString("0.0") + " ms");
                L("  HUD: " + time(() => { prep(); drawHUD(players[0]); }).ToString("0.0") + " ms");
                L("  un texto: " + time(() => { prep(); txt("VUELTA 1/3", 480, 30, 26, W_); }).ToString("0.00") + " ms");
                L("  una cabeza: " + time(() => { prep(); drawHead(CHARS[0], 74, 130, 12); }).ToString("0.00") + " ms");
                tg.ResetTransform();
                keys.Clear();
            });
            L(tfails == 0 ? "TODO BIEN" : ("FALLOS: " + tfails));
            tlog.Close();
        }
    }
}
