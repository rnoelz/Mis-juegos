using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FutbolToons
{
    // Pantallas: título, menús, selección de equipo y personajes, torneos, resultados y trofeos
    static partial class G
    {
        static Color Y_ { get { return C("#ffd800"); } }
        static void panel(double x, double y, double w, double h, double a, Color border)
        {
            rrFill(x, y, w, h, 16, U.RGBA(10, 12, 36, a));
            rrStroke(x, y, w, h, 16, border.IsEmpty ? U.RGBA(255, 255, 255, 0.75) : border, border.IsEmpty ? 2.5 : 4.5);
        }
        static void dim(double a) { rect(0, 0, W, H, U.RGBA(0, 0, 0, a)); }
        static void hint(string s) { txt(s, W / 2.0, H - 16, 14, W_); }
        static void drawStar(double x, double y, double R)
        {
            g.FillPolygon(B(C("#ffd800")), starPts(x, y, R, R * 0.45));
            g.DrawPolygon(P(C("#c88a00"), Math.Max(1, R * 0.08)), starPts(x, y, R, R * 0.45));
        }
        static void stars(double x, double y, int n, double r) { for (int i = 0; i < 5; i++) { if (i < n) drawStar(x + i * r * 2.2, y, r); else g.DrawPolygon(P(U.RGBA(255, 255, 255, 0.4), 1.5), starPts(x + i * r * 2.2, y, r, r * 0.45)); } }
        static void drawParts()
        {
            double oa = ALPHA;
            foreach (var p in parts) { ALPHA = Math.Max(0, Math.Min(1, p.life / p.max)); rect(p.x - p.size / 2, p.y - p.size / 2, p.size, p.size, p.col); }
            ALPHA = oa;
        }
        static void title(string s, double y = 40, double size = 34) { txt(s, W / 2.0, y, size, Y_, "center", C("#3a1a00"), 8); }
        // personaje de cuerpo entero con el uniforme del equipo
        static void figure(Ch c, Kit k, double x, double y, double s, bool gk = false, int num = 0, double bob = 0)
        {
            double sc = s * (c.big ? 1.15 : 1) * c.tall;
            drawBody(c, k, C("#2ac84a"), gk, x, y - bob, s, 1, T * 3, false, 0, num, gk ? 1 : 0);
            double hr = 19 * sc * (c.big ? 1.05 : 1);
            drawFace(c, x, y - bob - 44 * sc - hr * 0.72, hr);
        }

        static void drawTitle()
        {
            dim(0.35); double b = Math.Sin(T * 3) * 6;
            txt("FÚTBOL", W / 2.0, 90 + b * 0.5, 64, W_, "center", C("#0a2a6a"), 14);
            txt("TOONS", W / 2.0, 168 + b, 96, Y_, "center", C("#8a2a00"), 16);
            drawBallIcon(W / 2.0 + 205, 160 + b, 30 + Math.Sin(T * 6) * 2);
            int n = CHARS.Length; double sp = 900.0 / n;
            for (int i = 0; i < n; i++) drawFace(CHARS[i], W / 2.0 + (i - (n - 1) / 2.0) * sp, 270 + Math.Sin(T * 4 + i * 0.6) * 6, sp * 0.45);
            string[] tr = { "premier", "champions", "laliga", "supercopa", "mundial" };
            for (int i = 0; i < 5; i++) drawTrophy(tr[i], W / 2.0 + (i - 2) * 110, 400 + Math.Sin(T * 2 + i) * 3, 64);
            if (((int)Math.Floor(T * 2)) % 2 == 0) txt("Presiona ENTER para empezar", W / 2.0, 445, 26, W_);
            txt("Dragon Ball - Bob Esponja - Pokémon - Los Simpson - Scooby-Doo - Hora de Aventura - Tom y Jerry - Looney Tunes - Mario", W / 2.0, 485, 12, C("#ffe14a"), "center", Color.Black, 3);
            txt("Juego de fans - no oficial", W - 10, H - 8, 11, C("#dddddd"), "right", Color.Black, 3);
        }
        static void drawMenu()
        {
            dim(0.5); title("¿QUÉ QUIERES JUGAR?");
            for (int i = 0; i < 6; i++)
            {
                var c = COMPS[i]; double x = 40 + (i % 3) * 296, y = 74 + (i / 3) * 196; bool sel = menuSel == i;
                double w = 284, h = 184;
                Gradient(x, y, w, h, U.Shade(c.col, sel ? 0.15 : 0), U.Shade(c.col, -0.45));
                rrStroke(x, y, w, h, 14, sel ? Y_ : U.RGBA(255, 255, 255, 0.5), sel ? 5 : 2);
                drawTrophy(c.id, x + 58, y + 158 + (sel ? Math.Sin(T * 5) * 4 : 0), 88);
                txt(c.name.ToUpper(), x + 186, y + 36, c.name.Length > 14 ? 15 : 20, sel ? Y_ : W_, "center", Color.Black, 4);
                var lines2 = wrap(c.desc, 20);
                for (int j = 0; j < lines2.Count; j++) txt(lines2[j], x + 186, y + 74 + j * 18, 11.5, W_, "center", Color.Black, 3);
                int tw = Trophies(c.id); if (c.fmt != "friendly" && tw > 0) txt("Ganada " + tw + (tw == 1 ? " vez" : " veces"), x + 186, y + 160, 13, C("#7aff7a"), "center", Color.Black, 3);
            }
            string[] bt = { "VITRINA DE TROFEOS", "CÓMO JUGAR" };
            for (int i = 0; i < 2; i++)
            {
                double x = 200 + i * 290, y = 472; bool sel = menuSel == 6 + i;
                rrFill(x, y, 270, 44, 12, sel ? U.RGBA(255, 215, 0, 0.35) : U.RGBA(10, 12, 36, 0.75));
                rrStroke(x, y, 270, 44, 12, sel ? Y_ : U.RGBA(255, 255, 255, 0.6), sel ? 4 : 2);
                txt(bt[i], x + 135, y + 22, 18, sel ? Y_ : W_);
            }
            hint("Flechas: elegir   ENTER: aceptar   ESC: atrás   M: música   F11: pantalla completa");
        }
        static List<string> wrap(string s, int n)
        {
            var res = new List<string>(); string line = "";
            foreach (var w in s.Split(' ')) { if ((line + " " + w).Trim().Length > n) { res.Add(line.Trim()); line = w; } else line += " " + w; }
            if (line.Trim() != "") res.Add(line.Trim()); return res;
        }
        static void drawOpts()
        {
            dim(0.5); bool fr = comp.fmt == "friendly";
            drawTrophy(comp.id, W / 2.0, 120, 90);
            title(comp.name.ToUpper(), 150, 34);
            var rows = new List<string[]> {
                new[]{ "DURACIÓN: " + DURS[durIdx] + " MINUTOS", "Cada partido tiene dos tiempos" },
                new[]{ "DIFICULTAD: " + DIFF_N[diff], new[]{ "Rivales tranquilos: ¡ideal para empezar!", "Rivales que juegan bien", "¡Rivales muy rápidos y con mucha puntería!" }[diff] },
            };
            if (fr) { rows.Add(new[] { np == 1 ? "1 JUGADOR" : "2 JUGADORES", np == 1 ? "Tú contra la máquina" : "J1 contra J2 en el mismo teclado" }); rows.Add(new[] { "ESTADIO: " + STADIUMS[stadIdx].name.ToUpper(), "Dónde se juega el partido" }); }
            for (int i = 0; i < rows.Count; i++)
            {
                double y = 190 + i * 74; bool sel = optRow == i;
                panel(200, y, 560, 66, sel ? 0.85 : 0.6, sel ? Y_ : Color.Empty);
                txt((sel ? "<   " : "") + rows[i][0] + (sel ? "   >" : ""), W / 2.0, y + 24, rows[i][0].Length > 26 ? 18 : 22, sel ? Y_ : W_);
                txt(rows[i][1], W / 2.0, y + 50, 13, C("#dddddd"), "center", Color.Black, 3);
            }
            hint("Arriba/abajo: opción   Izq/der: cambiar   ENTER: elegir equipo   ESC: atrás");
        }
        static void drawTeamSel()
        {
            dim(0.55);
            bool fr = comp.fmt == "friendly";
            string t = !fr ? "ELIGE TU EQUIPO" : pickStep == 0 ? (np > 1 ? "J1: ELIGE TU EQUIPO" : "ELIGE TU EQUIPO") : (np > 1 ? "J2: ELIGE TU EQUIPO" : "ELIGE AL RIVAL");
            title(t, 34, 32);
            int cols = 6, n = teamList.Count, rowsN = (n + cols - 1) / cols; double cw = 148, chh = rowsN > 4 ? 76 : rowsN > 3 ? 92 : 112, gx = 36, gy = 64;
            for (int i = 0; i < n; i++)
            {
                var tm = teamList[i]; double x = gx + (i % cols) * cw + 4, y = gy + (i / cols) * chh; bool sel = cur == i, taken = pickStep == 1 && tm == pick[0];
                rrFill(x, y, cw - 8, chh - 8, 10, sel ? U.RGBA(255, 215, 0, 0.3) : taken ? U.RGBA(80, 80, 80, 0.7) : U.RGBA(10, 12, 36, 0.7));
                rrStroke(x, y, cw - 8, chh - 8, 10, sel ? Y_ : U.A(tm.home.shirt, 0.8), sel ? 4 : 2);
                double s = Math.Min(54, chh * 0.55);
                drawShirt(tm.home, x + 34, y + (chh - 8) * 0.42, s, tm.nation ? "10" : null);
                drawCrest(tm, x + cw - 34, y + (chh - 8) * 0.38, s * 0.65);
                txt(tm.name, x + (cw - 8) / 2, y + chh - 20, tm.name.Length > 14 ? 10 : 12, W_, "center", Color.Black, 3);
            }
            // información del equipo elegido
            var c = teamList[cur]; double py = gy + rowsN * chh + 4;
            if (py < 440)
            {
                panel(36, py, 888, Math.Min(90, 500 - py), 0.75, Color.Empty);
                drawShirt(c.home, 90, py + 45, 60); drawShirt(c.away, 160, py + 45, 60);
                txt("LOCAL", 90, py + 82, 10, W_); txt("VISITA", 160, py + 82, 10, W_);
                txt(c.name.ToUpper(), 480, py + 32, 26, W_);
                txt("Fuerza", 420, py + 64, 14, C("#9ad8ff"), "right"); stars(436, py + 64, c.rating, 8);
                if (pickStep == 1 && !fr) { }
            }
            hint("Flechas: elegir   ENTER: aceptar   ESC: atrás");
        }
        static void drawSquad()
        {
            dim(0.6);
            var tm = pick[squadFor]; Kit k = tm.home;
            title((comp.fmt == "friendly" && np > 1 ? "J" + (squadFor + 1) + ": " : "") + "ARMA TU EQUIPO (" + squad.Count + "/5)", 30, 28);
            int cols = 8; double cw = 74, chh = 70, gx = 18, gy = 56;
            string lastSeries = "";
            for (int i = 0; i < CHARS.Length; i++)
            {
                var c = CHARS[i]; double x = gx + (i % cols) * cw, y = gy + (i / cols) * chh; bool sel = cur == i; int si = squad.IndexOf(c);
                rrFill(x, y, cw - 6, chh - 6, 10, si >= 0 ? U.A(k.shirt, 0.55) : U.RGBA(10, 12, 36, 0.7));
                rrStroke(x, y, cw - 6, chh - 6, 10, sel ? Y_ : si >= 0 ? W_ : U.RGBA(255, 255, 255, 0.35), sel ? 4 : si >= 0 ? 3 : 1.5);
                drawFace(c, x + (cw - 6) / 2, y + 28 + (sel ? Math.Sin(T * 8) * 2 : 0), 17);
                txt(c.name, x + (cw - 6) / 2, y + chh - 16, c.name.Length > 10 ? 8 : 10, W_, "center", Color.Black, 2.5);
                if (si >= 0) { circ(x + 12, y + 12, 9, Y_); txt(si == 4 ? "A" : (si + 1).ToString(), x + 12, y + 12, 11, DARK, "center", Color.Black, 0); }
                lastSeries = c.series;
            }
            // botón LISTO
            bool full = squad.Count == 5, bs = cur == 40;
            double bx = gx, by = gy + 5 * chh + 2;
            rrFill(bx, by, 8 * cw - 6, 34, 10, full ? (bs ? U.RGBA(60, 200, 60, 0.9) : U.RGBA(40, 140, 40, 0.8)) : U.RGBA(80, 80, 80, 0.7));
            rrStroke(bx, by, 8 * cw - 6, 34, 10, bs ? Y_ : U.RGBA(255, 255, 255, 0.5), bs ? 4 : 2);
            txt(full ? "¡LISTO! A JUGAR" : "Elige " + (5 - squad.Count) + " más (o pulsa R para completar al azar)", bx + (8 * cw - 6) / 2, by + 17, full ? 20 : 14, W_);
            // panel derecho: el equipo y el personaje elegido
            double px = 614; panel(px, 52, 330, 448, 0.8, Color.Empty);
            drawCrest(tm, px + 34, 82, 38); txt(tm.name.ToUpper(), px + 190, 82, tm.name.Length > 14 ? 15 : 18, W_);
            for (int i = 0; i < 5; i++)
            {
                double fx = px + 40 + i * 63, fy = 200;
                if (i < squad.Count) figure(squad[i], k, fx, fy, 0.95, i == 4, i == 4 ? 1 : new[] { 9, 7, 11, 4 }[i], Math.Abs(Math.Sin(T * 4 + i)) * 3);
                else { ellStroke(fx, fy - 40, 20, 40, U.RGBA(255, 255, 255, 0.3), 2); txt("?", fx, fy - 40, 24, U.RGBA(255, 255, 255, 0.4), "center", Color.Black, 0); }
                txt(i == 4 ? "ARQUERO" : new[] { "DEL", "DEL", "DEL", "DEF" }[i], fx, fy + 14, 9, C("#9ad8ff"), "center", Color.Black, 2);
            }
            if (cur < 40)
            {
                var c = CHARS[cur];
                drawFace(c, px + 70, 300, 38);
                txt(c.name, px + 200, 270, c.name.Length > 12 ? 17 : 21, W_); txt(c.series, px + 200, 294, 13, C("#9ad8ff"), "center", Color.Black, 3);
                string[] st = { "Velocidad", "Tiro", "Técnica" }; int[] v = { c.spd, c.sht, c.tec };
                for (int i = 0; i < 3; i++) { txt(st[i], px + 30, 360 + i * 26, 14, W_, "left", Color.Black, 3); for (int j = 0; j < 5; j++) rrFill(px + 140 + j * 34, 352 + i * 26, 28, 16, 4, j < v[i] ? Y_ : U.RGBA(255, 255, 255, 0.2)); }
                Color pc = PW_COL.ContainsKey(c.pw) ? PW_COL[c.pw] : Y_;
                txt("SÚPER TIRO: " + c.pwName, px + 165, 450, 14, pc, "center", Color.Black, 3);
            }
            else txt("El 5º personaje que elijas será el arquero", px + 165, 330, 13, W_, "center", Color.Black, 3);
            hint("Flechas: mover   ENTER: poner/quitar   R: completar al azar   Retroceso: quitar el último   ESC: atrás");
        }
        static void squadRow(Team t, Kit k, double cx, double y, double s, bool flip)
        {
            for (int i = 0; i < 5; i++) figure(t.squad[i], k, cx + (i - 2) * 58 * s * (flip ? -1 : 1), y + (i % 2) * 10, s, i == 4, i == 4 ? 1 : new[] { 9, 7, 11, 4 }[i], Math.Abs(Math.Sin(T * 3 + i)) * 2);
        }
        static void drawVs()
        {
            dim(0.6);
            Team a, b; string sub;
            if (comp.fmt == "friendly") { a = pick[0]; b = pick[1]; sub = "PARTIDO AMISTOSO - " + STADIUMS[stadIdx].name.ToUpper(); }
            else { var f = NextFix(tour); a = tour.me; b = f.a == tour.me ? f.b : f.a; sub = comp.name.ToUpper() + " - " + StageName(tour).ToUpper(); }
            Kit ka = a.home, kb = colDist(a.home.shirt, b.home.shirt) < 150 ? b.away : b.home;
            double sl = Math.Min(1, vsT * 3);
            txt(sub, W / 2.0, 34, 18, Y_, "center", Color.Black, 4);
            for (int t = 0; t < 2; t++)
            {
                var tm = t == 0 ? a : b; var k = t == 0 ? ka : kb; double cx = t == 0 ? 240 - (1 - sl) * 400 : 720 + (1 - sl) * 400;
                Gradient(cx - 210, 60, 420, 400, U.A(k.shirt, 0.55), U.RGBA(0, 0, 0, 0.3));
                drawCrest(tm, cx, 120, 80);
                txt(tm.name.ToUpper(), cx, 190, tm.name.Length > 14 ? 22 : 28, W_, "center", Color.Black, 5);
                txt(comp.fmt == "friendly" && np > 1 ? "J" + (t + 1) : t == 0 ? "TÚ" : "CPU", cx, 222, 16, t == 0 ? Y_ : C("#9ad8ff"), "center", Color.Black, 3);
                squadRow(tm, k, cx, 380, 1.15, t == 1);
                for (int i = 0; i < 5; i++) txt(tm.squad[i].name, cx + (i - 2) * 66 * (t == 1 ? -1 : 1), 410 + (i % 2) * 14, 9, W_, "center", Color.Black, 2);
            }
            txt("VS", W / 2.0, 240, 70 * (0.8 + 0.2 * sl), Y_, "center", C("#8a2a00"), 12);
            if (vsT > 0.5 && ((int)Math.Floor(T * 2)) % 2 == 0) hint("ENTER: ¡a jugar!   ESC: atrás");
        }
        static void tableBox(List<Row> rows, double x, double y, double w, string head, int qualify)
        {
            panel(x, y, w, 36 + rows.Count * 30, 0.82, Color.Empty);
            txt(head, x + 14, y + 18, 14, Y_, "left", Color.Black, 3);
            string[] hd = { "PJ", "G", "E", "P", "DG", "PTS" };
            for (int i = 0; i < hd.Length; i++) txt(hd[i], x + w - 250 + i * 42, y + 18, 11, C("#9ad8ff"), "center", Color.Black, 2);
            var srt = Sorted(rows);
            for (int i = 0; i < srt.Count; i++)
            {
                var r = srt[i]; double ry = y + 36 + i * 30; bool me = tour != null && r.t == tour.me;
                if (me) rrFill(x + 6, ry + 1, w - 12, 27, 6, U.RGBA(255, 200, 0, 0.35));
                else if (i < qualify) rrFill(x + 6, ry + 1, w - 12, 27, 6, U.RGBA(60, 200, 60, 0.18));
                txt((i + 1).ToString(), x + 22, ry + 15, 14, W_, "center", Color.Black, 2);
                drawCrest(r.t, x + 50, ry + 15, 22);
                txt(r.t.name, x + 68, ry + 15, 13, me ? Y_ : W_, "left", Color.Black, 3);
                int[] v = { r.pj, r.g, r.e, r.p, r.Dg, r.Pts };
                for (int j = 0; j < v.Length; j++) txt((j == 4 && v[j] > 0 ? "+" : "") + v[j], x + w - 250 + j * 42, ry + 15, j == 5 ? 15 : 13, j == 5 ? Y_ : W_, "center", Color.Black, 2);
            }
        }
        static void fixLine(Fix f, double x, double y, double w)
        {
            bool me = tour != null && (f.a == tour.me || f.b == tour.me);
            rrFill(x, y, w, 26, 6, me ? U.RGBA(255, 200, 0, 0.3) : U.RGBA(255, 255, 255, 0.07));
            drawCrest(f.a, x + 16, y + 13, 20); txt(f.a.abbr, x + 34, y + 13, 12, W_, "left", Color.Black, 2);
            drawCrest(f.b, x + w - 16, y + 13, 20); txt(f.b.abbr, x + w - 34, y + 13, 12, W_, "right", Color.Black, 2);
            txt(f.Played ? f.ga + " - " + f.gb + (f.pens ? " (" + f.pa + "-" + f.pb + " pen.)" : "") : "vs", x + w / 2, y + 13, 12, f.Played ? Y_ : C("#bbbbbb"), "center", Color.Black, 2);
        }
        static void drawHub()
        {
            dim(0.6); var t = tour;
            drawTrophy(t.comp.id, 60, 80, 60);
            txt(t.comp.name.ToUpper(), 110, 40, 26, Y_, "left", C("#3a1a00"), 6);
            txt(StageName(t), 110, 70, 15, W_, "left", Color.Black, 3);
            if (t.comp.fmt == "league") tableBox(t.table, 30, 100, 540, "CLASIFICACIÓN", 1);
            else if (t.comp.fmt == "world" && !t.inKo) { tableBox(t.gtab[0], 30, 100, 540, "GRUPO A", 2); tableBox(t.gtab[1], 30, 270, 540, "GRUPO B", 2); }
            else
            {
                panel(30, 100, 540, 400, 0.82, Color.Empty);
                txt("CAMINO A LA FINAL", 300, 120, 16, Y_);
                double y = 146;
                foreach (var rd in t.koDone) { foreach (var f in rd) { fixLine(f, 50, y, 500); y += 30; } y += 8; }
                txt(t.stage.ToUpper(), 300, y + 10, 14, C("#9ad8ff")); y += 26;
                foreach (var f in t.ko) { fixLine(f, 50, y, 500); y += 30; }
            }
            // próximo partido
            var nf = NextFix(t); var opp = nf.a == t.me ? nf.b : nf.a;
            panel(590, 100, 340, 300, 0.85, Y_);
            txt("PRÓXIMO PARTIDO", 760, 124, 17, Y_);
            drawCrest(t.me, 680, 200, 70); drawCrest(opp, 840, 200, 70);
            txt("VS", 760, 200, 30, W_);
            txt(t.me.name, 680, 258, 14, W_); txt(opp.name, 840, 258, 14, W_);
            for (int i = 0; i < 5; i++) { drawFace(t.me.squad[i], 620 + i * 30, 300, 12); drawFace(opp.squad[i], 780 + i * 30, 300, 12); }
            if (t.inKo) txt("Si hay empate: ¡penales!", 760, 340, 13, C("#ffb0b0"), "center", Color.Black, 3);
            if (((int)Math.Floor(T * 2)) % 2 == 0) txt("ENTER: jugar", 760, 375, 20, W_);
            // resultados de la fecha anterior
            if (!t.inKo && t.round > 0)
            {
                panel(590, 410, 340, 90, 0.75, Color.Empty); txt("FECHA ANTERIOR", 760, 424, 12, C("#9ad8ff"));
                var pr = t.rounds[t.round - 1]; for (int i = 0; i < Math.Min(3, pr.Count); i++) fixLine(pr[i], 600, 436 + i * 21 * 0.95, 320);
            }
            hint("ENTER: jugar el partido   ESC: abandonar el torneo");
        }
        static void drawQuit()
        {
            dim(0.6); panel(280, 170, 400, 180, 0.9, Color.Empty);
            txt("¿Abandonar el torneo?", W / 2.0, 210, 24, Y_);
            string[] o = { "Sí, salir al menú", "No, seguir jugando" };
            for (int i = 0; i < 2; i++) txt((pauseSel == i ? "> " : "") + o[i], W / 2.0, 262 + i * 40, 20, pauseSel == i ? Y_ : W_);
        }
        static void drawPause()
        {
            dim(0.55); panel(300, 110, 360, 190, 0.9, Color.Empty); txt("PAUSA", W / 2.0, 150, 40, Y_);
            string[] o = { "Continuar", comp != null && comp.fmt != "friendly" ? "Abandonar el torneo" : "Salir al menú" };
            for (int i = 0; i < 2; i++) txt((pauseSel == i ? "> " : "") + o[i], W / 2.0, 210 + i * 44, 22, pauseSel == i ? Y_ : W_);
            drawControls(330);
        }
        static void drawResult()
        {
            var s = M; dim(0.6);
            int w = s.Winner;
            string head = w < 0 ? "¡EMPATE!" : s.np > 1 ? "¡GANA " + s.team[w].name.ToUpper() + "!" : w == 0 ? "¡GANASTE!" : "PERDISTE...";
            txt(head, W / 2.0, 44, 44, w < 0 ? W_ : w == 0 || s.np > 1 ? Y_ : C("#ff8a8a"), "center", Color.Black, 8);
            panel(110, 82, 740, 380, 0.85, Color.Empty);
            for (int t = 0; t < 2; t++)
            {
                double cx = t == 0 ? 260 : 700;
                drawCrest(s.team[t], cx, 140, 70); txt(s.team[t].name.ToUpper(), cx, 198, 18, W_, "center", Color.Black, 4);
                int row = 0;
                foreach (var gl in s.log.Where(l => l[0] == t.ToString()).Take(6)) { drawBallIcon(cx - 90, 232 + row * 22, 7); txt(gl[1] + "  " + gl[2], cx - 76, 232 + row * 22, 13, W_, "left", Color.Black, 3); row++; }
            }
            txt(s.score[0] + " - " + s.score[1], W / 2.0, 140, 60, W_, "center", Color.Black, 8);
            if (s.penTaken[0] > 0) txt("Penales: " + s.penGoals[0] + " - " + s.penGoals[1], W / 2.0, 190, 18, Y_, "center", Color.Black, 4);
            int tot = Math.Max(1, (int)(s.poss[0] + s.poss[1])), p0 = (int)Math.Round(s.poss[0] * 100 / tot);
            double sy = 380;
            txt("ESTADÍSTICAS", W / 2.0, sy - 22, 14, C("#9ad8ff"));
            txt(p0 + "%", W / 2.0 - 160, sy, 16, W_); txt("Posesión", W / 2.0, sy, 14, C("#dddddd")); txt((100 - p0) + "%", W / 2.0 + 160, sy, 16, W_);
            rrFill(W / 2.0 - 200, sy + 14, 400, 10, 5, s.kit[1].shirt); rrFill(W / 2.0 - 200, sy + 14, 400 * p0 / 100.0, 10, 5, s.kit[0].shirt);
            txt(s.shots[0].ToString(), W / 2.0 - 160, sy + 44, 16, W_); txt("Tiros", W / 2.0, sy + 44, 14, C("#dddddd")); txt(s.shots[1].ToString(), W / 2.0 + 160, sy + 44, 16, W_);
            if (w == 0 || (w == 1 && s.np > 1)) if (U.R() < 0.5) confetti(1);
            if (((int)Math.Floor(T * 2)) % 2 == 0) hint(comp.fmt == "friendly" ? "ENTER: volver al menú   R: revancha" : "ENTER: continuar");
        }
        static void drawTrophyScreen()
        {
            dim(0.55);
            double b = Math.Sin(T * 2) * 8;
            for (int i = 0; i < 12; i++) { double a = T * 0.4 + i * Math.PI / 6; fillPts(U.RGBA(255, 230, 120, 0.08), W / 2.0, 250, W / 2.0 + Math.Cos(a) * 700, 250 + Math.Sin(a) * 700, W / 2.0 + Math.Cos(a + 0.15) * 700, 250 + Math.Sin(a + 0.15) * 700); }
            drawTrophy(comp.id, W / 2.0, 430 + b, 220);
            txt("¡CAMPEÓN!", W / 2.0, 52, 58, Y_, "center", C("#8a2a00"), 12);
            txt(comp.name.ToUpper(), W / 2.0, 104, 28, W_, "center", Color.Black, 6);
            drawCrest(tour.me, 150, 260, 90); txt(tour.me.name.ToUpper(), 150, 330, 16, W_);
            squadRow(tour.me, tour.me.home, 800, 380, 0.9, false);
            int n = Trophies(comp.id);
            txt("Ya tienes " + n + (n == 1 ? " trofeo" : " trofeos") + " de " + comp.name, W / 2.0, 470, 16, C("#7aff7a"), "center", Color.Black, 4);
            hint("ENTER: volver al menú (el trofeo queda guardado en tu vitrina)");
        }
        static void drawOver()
        {
            dim(0.65);
            drawTrophy(comp.id, W / 2.0, 260, 160, false);
            txt(comp.name.ToUpper(), W / 2.0, 60, 30, Y_, "center", Color.Black, 6);
            txt(tour.endText, W / 2.0, 320, 26, W_, "center", Color.Black, 5);
            txt("¡No te rindas, inténtalo otra vez!", W / 2.0, 360, 18, C("#9ad8ff"), "center", Color.Black, 4);
            if (comp.fmt == "league") tableMini();
            hint("ENTER: volver al menú");
        }
        static void tableMini()
        {
            var srt = Sorted(tour.table);
            for (int i = 0; i < srt.Count; i++) txt((i + 1) + ". " + srt[i].t.name + "  " + srt[i].Pts + " pts", W / 2.0, 395 + i * 18, 13, srt[i].t == tour.me ? Y_ : W_, "center", Color.Black, 3);
        }
        static void drawCabinet()
        {
            dim(0.65); title("VITRINA DE TROFEOS");
            panel(40, 80, 880, 380, 0.8, Color.Empty);
            rect(60, 330, 840, 14, C("#8a5a2a")); rect(60, 344, 840, 8, C("#5a3a1a"));
            for (int i = 1; i < 6; i++)
            {
                var c = COMPS[i]; double x = 60 + 84 + (i - 1) * 168; int n = Trophies(c.id);
                drawTrophy(c.id, x, 330, 150, n > 0);
                txt(c.name, x, 380, 15, n > 0 ? Y_ : C("#aaaaaa"), "center", Color.Black, 3);
                txt(n > 0 ? "x" + n : "Aún no la ganas", x, 406, n > 0 ? 22 : 12, n > 0 ? W_ : C("#888888"), "center", Color.Black, 3);
            }
            int tot = COMPS.Skip(1).Sum(c => Trophies(c.id));
            txt("Trofeos ganados: " + tot, W / 2.0, 440, 16, C("#7aff7a"), "center", Color.Black, 3);
            hint("ENTER o ESC: volver");
        }
        static void drawHelp()
        {
            dim(0.7); title("CÓMO JUGAR");
            panel(30, 70, 440, 420, 0.8, Color.Empty);
            txt("CONTROLES", 250, 92, 18, Y_);
            string[] l1 = { "Mover: WASD o flechas", "TIRAR: ESPACIO", "  (mantenlo para patear más fuerte)", "PASAR: E o X", "Sin la pelota:", "  ESPACIO = barrida para robar", "  E = cambiar de jugador", "SÚPER TIRO: C (con la barra llena)", "Pausa: ESC", "", "2 JUGADORES:", "J1: WASD, ESPACIO, E, R", "J2: Flechas, SHIFT der., ENTER, L" };
            for (int i = 0; i < l1.Length; i++) txt(l1[i], 50, 124 + i * 27, 15, i == 10 ? C("#7aff7a") : W_, "left", Color.Black, 3);
            panel(490, 70, 440, 420, 0.8, Color.Empty);
            txt("CONSEJOS", 710, 92, 18, Y_);
            string[] l2 = { "Corre hacia el arco y tira: ¡el tiro", "  apunta solo al arco!", "Mantén ARRIBA o ABAJO al tirar para", "  apuntar a una esquina.", "La barra de súper tiro se llena sola,", "  con barridas y si te meten gol.", "Cada personaje tiene su súper tiro:", "  Kamehameha, burbuja, rayo...", "En eliminatorias, si hay empate", "  ¡se define por penales!", "Gana torneos para llenar tu", "  vitrina de trofeos." };
            for (int i = 0; i < l2.Length; i++) txt(l2[i], 510, 124 + i * 28, 14, W_, "left", Color.Black, 3);
            hint("ENTER o ESC: volver");
        }

        // ---------------- dibujar todo ----------------
        public static void Draw()
        {
            if (M != null) DrawMatch(!M.demo && (state == "match" || state == "mpause"));
            else { rect(0, 0, W, H, C("#1a3a6a")); }
            switch (state)
            {
                case "title": drawTitle(); break;
                case "menu": drawMenu(); break;
                case "opts": drawOpts(); break;
                case "team": drawTeamSel(); break;
                case "squad": drawSquad(); break;
                case "vs": drawVs(); break;
                case "hub": drawHub(); break;
                case "quit": drawHub(); drawQuit(); break;
                case "mpause": drawPause(); break;
                case "mres": drawResult(); break;
                case "trophy": drawTrophyScreen(); break;
                case "over": drawOver(); break;
                case "cabinet": drawCabinet(); break;
                case "help": drawHelp(); break;
            }
            drawParts();
        }
    }
}
