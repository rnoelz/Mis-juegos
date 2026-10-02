using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SuperKart
{
    // Pantallas: HUD de carrera, menús, resultados, podio y ayuda
    static partial class SK
    {
        static Color placeCol(int n) { return n == 1 ? C("#ffd700") : n == 2 ? C("#dfe6ee") : n == 3 ? C("#e8964a") : C("#7ad0ff"); }
        static Color Y_ { get { return C("#ffd800"); } }

        static void drawMinimap(double x, double y)
        {
            rrFill(x, y, 156, 156, 14, U.RGBA(0, 0, 0, 0.35));
            var pts2 = new List<PointF>();
            for (int i = 0; i < map.Length; i += 3) pts2.Add(new PointF(F(x + map[i].X), F(y + map[i].Y)));
            var arr = pts2.ToArray();
            g.DrawPolygon(P(W_, 7, true), arr); g.DrawPolygon(P(theme.rainbow ? C("#c86aff") : C("#555555"), 4, true), arr);
            var s0 = map[0];
            rect(x + s0.X - 6, y + s0.Y - 3, 12, 6, K_); rect(x + s0.X - 6, y + s0.Y - 3, 4, 3, W_); rect(x + s0.X + 2, y + s0.Y - 3, 4, 3, W_); rect(x + s0.X - 2, y + s0.Y, 4, 3, W_);
            for (int i = ranking.Count - 1; i >= 0; i--)
            {
                var r = ranking[i]; if (r.isPlayer) continue;
                var p = map[(int)Math.Floor(r.z / SEG) % map.Length]; circ(x + p.X, y + p.Y, 5, Color.Black); circ(x + p.X, y + p.Y, 3.8, r.ch.kart);
            }
            foreach (var h in players)
            {
                var p = map[(int)Math.Floor(h.z / SEG) % map.Length];
                circ(x + p.X, y + p.Y, 8, players.Count > 1 ? P_COL[h.pidx] : W_); circ(x + p.X, y + p.Y, 6, h.ch.kart);
            }
        }

        // HUD de un jugador, dibujado dentro de su vista (ancho VW)
        static void drawHUD(Racer P)
        {
            bool duo = players.Count > 1; double cx = VW / 2;
            if (P.ink > 0)
            {
                double oa = ALPHA; ALPHA = Math.Min(1, P.ink) * 0.88;
                for (int i = 0; i < 7; i++)
                {
                    double ix = VW * (0.15 + ((i * 37) % 70) / 100.0), iy = H * (0.25 + ((i * 53) % 55) / 100.0), ir = 50 + (i * 29) % 60;
                    ell(ix, iy, ir * 1.3, ir, C("#120a1a"), i); circ(ix + ir, iy + ir * 0.6, ir * 0.35, C("#120a1a"));
                }
                ALPHA = oa;
            }
            if (P.boost > 0 || P.bullet > 0 || P.star > 0)
                for (int i = 0; i < 14; i++)
                {
                    double a = U.R() * Math.PI * 2, r1 = (duo ? 140 : 220) + U.R() * 150, r2 = r1 + 80 + U.R() * 120;
                    lines(U.RGBA(255, 255, 255, 0.5), 2, false, cx + Math.Cos(a) * r1, H / 2.0 + Math.Sin(a) * r1 * 0.6, cx + Math.Cos(a) * r2, H / 2.0 + Math.Sin(a) * r2 * 0.6);
                }
            // caja de objeto
            double bx = duo && P.pidx == 1 ? VW - 106 : 18;
            rrFill(bx, 16, 88, 88, 16, U.RGBA(0, 0, 0, 0.45)); rrStroke(bx, 16, 88, 88, 16, duo ? P_COL[P.pidx] : W_, 4);
            string it = P.roulette > 0 ? ITEM_KEYS[(int)Math.Floor(T * 16) % ITEM_KEYS.Length] : P.item;
            if (it != null) drawItemIcon(it, bx + 44, 60, 64);
            if (P.item != null && P.item != "golden" && P.roulette <= 0 && P.itemCount > 1) txt("x" + P.itemCount, bx + 80, 96, 20, W_);
            if (P.item == "golden" && P.goldenT > 0) rect(bx + 8, 94, 72 * P.goldenT / 7, 6, C("#ffc800"));
            if (duo) txt("J" + (P.pidx + 1), bx + 44, 118, 20, P_COL[P.pidx]);
            else if (P.item != null && P.roulette <= 0) txt(ITEM_NAMES[P.item], 112, 30, 14, W_, "left");
            // barra de súper poder
            {
                var PW = PowerOf(P.ch); string pk = duo ? (P.pidx == 1 ? "L" : "R") : "C"; bool full = P.meter >= 1;
                double pmx = duo ? bx : 112, pmy = duo ? 134 : 58, pmw = duo ? 88 : 130;
                if (!duo) txt("PODER: " + PW[1], pmx, pmy - 12, 12, C("#ffe14a"), "left", Color.Black, 3);
                rrFill(pmx, pmy, pmw, 12, 6, U.RGBA(0, 0, 0, 0.55));
                double dur; double fr = P.pwT > 0 ? P.pwT / (PW_DUR.TryGetValue(P.pwType, out dur) ? dur : 1) : P.meter;
                Color bc = P.pwT > 0 ? C("#7aff7a") : full ? U.Hsl(T * 400, 0.9, 0.6) : PW_ICON[PW[0]];
                rrFill(pmx, pmy, Math.Max(6, pmw * fr), 12, 6, bc);
                rrStroke(pmx, pmy, pmw, 12, 6, W_, 2);
                if (full && P.pwT <= 0 && ((int)Math.Floor(T * 4)) % 2 == 0) txt("¡PODER! (" + pk + ")", pmx + pmw / 2, pmy + 24, 13, W_);
            }
            // clasificación (solo en 1 jugador)
            if (!duo && !race.tt)
                for (int i = 0; i < ranking.Count; i++)
                {
                    var r = ranking[i]; double y = 130 + i * 33;
                    rrFill(18, y - 14, 88, 28, 10, r == P ? U.RGBA(255, 215, 0, 0.75) : U.RGBA(0, 0, 0, 0.35));
                    txt((i + 1).ToString(), 36, y, 15, r == P ? W_ : placeCol(i + 1)); drawHead(r.ch, 74, y, 12);
                }
            // vuelta y tiempo
            if (race.phase != "count")
            {
                double lx = duo ? cx + (P.pidx == 0 ? -10 : -30) : cx;
                txt("VUELTA " + Math.Min(LAPS, Math.Max(1, P.lap)) + "/" + LAPS, lx, 30, duo ? 20 : 26, W_);
                txt(U.FmtTime(P.finished ? P.finishTime : race.time), lx, 56, duo ? 16 : 20, C("#ffe14a"));
            }
            if (!duo) drawMinimap(W - 170, 14);
            // monedas y velocidad
            drawCoinIcon(40, H - 38, 40); txt("x " + P.coins, 66, H - 38, 28, P.coins >= 10 ? C("#ffd23a") : W_, "left");
            txt(Math.Round(Math.Max(0, P.speed) / MAXS * 120) + " km/h", 22, H - 84, 18, W_, "left");
            // posición o contrarreloj
            if (race.tt)
            {
                var rec = getRec(race.ti);
                txt("CONTRARRELOJ", VW - 20, H - 90, 20, C("#7aff7a"), "right");
                txt("Récord: " + (rec != null ? U.FmtTime(rec.total) : "--:--.--"), VW - 20, H - 60, 18, W_, "right");
                if (P.lapTimes.Count > 0) txt("Última vuelta: " + U.FmtTime(P.lapTimes[P.lapTimes.Count - 1]), VW - 20, H - 32, 16, C("#ffe14a"), "right");
            }
            else
            {
                double sc = 1 + P.placeT * 1.2; var st = Save(); Translate(VW - (duo ? 62 : 80), H - 62); Scale(sc, sc);
                txt(P.place + "º", 0, 0, duo ? 58 : 72, placeCol(P.place), "center", Color.Black, 10); Restore(st);
            }
            // aviso de caparazón azul
            if (projs.Any(p => p.type == "blue" && p.target == P) && ((int)Math.Floor(T * 6)) % 2 == 1) { drawShell(cx, 140, 60, C("#2a6ae8"), true); txt("¡CUIDADO!", cx, 160, 22, C("#7ab8ff")); }
            if (P.msgT > 0) txt(P.msg, cx, H * 0.32, duo ? 32 : 48, C("#ffe14a"));
        }
        // cosas que se dibujan una sola vez sobre toda la pantalla
        static void drawRaceOverlay()
        {
            bool duo = players.Count > 1;
            if (duo) { rect(W / 2 - 3, 0, 6, H, Color.Black); var st = Save(); Translate(W / 2 - 62, 8); Scale(0.8, 0.8); drawMinimap(0, 0); Restore(st); }
            if (race.phase == "count")
            {
                double c = race.count; int stage = c > 3 ? 0 : c > 2 ? 1 : c > 1 ? 2 : 3;
                if (!duo || c > 3) drawLakitu(W / 2 - 60, duo ? 200 : 110, 70, stage);
                if (c > 3)
                {
                    txt(TRACKS[race.ti].name, W / 2, H * 0.6, 52, W_);
                    if (gp != null) txt("Copa " + CUPS[gp.cup].name + " - Carrera " + (gp.race + 1) + "/3", W / 2, H * 0.6 + 50, 22, C("#ffe14a"));
                    txt("Truco: acelera justo cuando aparezca el 2 para salir con turbo", W / 2, H - 24, 16, W_);
                }
                else txt(((int)Math.Ceiling(c)).ToString(), W / 2, H * 0.44, 110, C("#ffe14a"));
            }
            else if (race.goT > 0)
            {
                if (!duo) drawLakitu(W / 2 - 60, 110 - (1.2 - race.goT) * 180, 70, 4);
                txt("¡YA!", W / 2, H * 0.44, 110 * (1 + (1.2 - race.goT) * 0.3), C("#3aff6a"));
            }
            if (race.flash > 0) rect(0, 0, W, H, U.RGBA(255, 255, 255, Math.Min(1, race.flash * 1.4)));
        }

        // ---------------- menús ----------------
        static void panel(double x, double y, double w, double h, double a, Color border)
        {
            rrFill(x, y, w, h, 18, U.RGBA(10, 10, 40, a));
            rrStroke(x, y, w, h, 18, border.IsEmpty ? U.RGBA(255, 255, 255, 0.8) : border, border.IsEmpty ? 3 : 5);
        }
        static void dim(double a) { rect(0, 0, W, H, U.RGBA(0, 0, 0, a)); }
        static void hint(string s) { txt(s, W / 2, H - 18, 15, W_); }
        static void statBars(double x, double y, St st, bool small = false)
        {
            double rh = small ? 24 : 30, fs = small ? 11 : 14, ox = small ? 70 : 112, bs = small ? 14 : 22, bw = small ? 11 : 18;
            var rows = small ? new[] { "Veloc.", "Acel.", "Manejo", "Peso" } : new[] { "Velocidad", "Aceleración", "Manejo", "Peso" };
            int[] vals = { st.spd, st.acc, st.han, st.wgt };
            for (int i = 0; i < 4; i++)
            {
                double yy = y + i * rh; txt(rows[i], x, yy, fs, W_, "left", Color.Black, 3);
                for (int j = 0; j < 6; j++) rrFill(x + ox + j * bs, yy - bw * 0.45, bw, bw * 0.9, 3, j < vals[i] ? Y_ : U.RGBA(255, 255, 255, 0.2));
            }
        }
        static void drawTitle()
        {
            dim(0.25); double b = Math.Sin(T * 3) * 6;
            txt("SUPER", W / 2, 110 + b * 0.5, 52, Y_, "center", C("#7a2a00"), 10);
            txt("KART BROS", W / 2, 185 + b, 92, W_, "center", W_, 22);
            txt("KART BROS", W / 2, 185 + b, 92, C("#e52521"), "center", Color.Black, 10);
            for (int i = 0; i < CHARS.Length; i++) { double hs = Math.Min(38, 900.0 / CHARS.Length); drawHead(CHARS[i], W / 2 + (i - (CHARS.Length - 1) / 2.0) * hs, 285 + Math.Sin(T * 4 + i * 0.7) * 7, hs * 0.42); }
            if (((int)Math.Floor(T * 2)) % 2 == 0) txt("Presiona ENTER para empezar", W / 2, 352, 28, W_);
            txt("Presiona I para ver cómo jugar y todos los objetos   -   ¡NUEVO! Modo FÚTBOL en el menú", W / 2, 380, 14, C("#ffe14a"), "center", Color.Black, 3);
            panel(110, 395, 740, 118, 0.6, Color.Empty);
            txt("CONTROLES", W / 2, 414, 18, Y_);
            txt("J1:  WASD manejar  -  ESPACIO derrapar  -  E objeto  -  R súper poder", W / 2, 442, 16, P_COL[0]);
            txt("J2:  Flechas manejar  -  SHIFT der. derrapar  -  ENTER objeto  -  L súper poder", W / 2, 466, 16, P_COL[1]);
            txt("Solo: cualquiera de los dos  -  C súper poder  -  ESC pausa  -  M música  -  F11 pantalla completa", W / 2, 490, 14, W_);
            txt("Juego de fans - no oficial", W - 10, H - 8, 11, C("#dddddd"), "right", Color.Black, 3);
        }
        static void drawMode()
        {
            bool fut = menu.mode == 3;
            dim(0.4); txt(fut ? "PREPARA EL PARTIDO" : "PREPARA LA CARRERA", W / 2, 38, 36, Y_);
            int np = menu.mode == 2 ? 1 : menu.np;
            var rows = new[] {
                new[]{ np==1?"1 JUGADOR":"2 JUGADORES", menu.mode==2?"La contrarreloj es de 1 jugador":fut?(np==1?"Tú y tu equipo contra la máquina":"J1 (rojo) contra J2 (azul) en la misma cancha"):np==1?"Tú contra 7 rivales":"Pantalla dividida: J1 a la izquierda, J2 a la derecha" },
                new[]{ new[]{"GRAN PREMIO","CARRERA LIBRE","CONTRARRELOJ","FÚTBOL"}[menu.mode], new[]{"Corre las 3 pistas de una copa y gana el trofeo","Elige cualquier pista y compite","Tú solo contra el reloj: ¡bate tu récord!","¡NUEVO! Partido 4 contra 4 con súper tiros"}[menu.mode] },
                fut ? new[]{ "DURACIÓN: "+SOC_DUR[menu.sdur]+" MIN", "Si terminan empatados... ¡gol de oro!" }
                    : new[]{ "VELOCIDAD "+CC[menu.cc].n, new[]{"Tranquilo, para aprender","Normal","Rápido y difícil","¡Velocidad extrema!"}[menu.cc] },
                new[]{ "RIVALES: "+DIFF[menu.diff].n, menu.mode==2?"(En contrarreloj corres solo)":DIFF[menu.diff].d },
                fut ? new[]{ CANCHAS[menu.sfield].name.ToUpper(), "Cancha: "+CANCHAS[menu.sfield].desc }
                    : new[]{ menu.mirror?"PISTAS: ESPEJO":"PISTAS: NORMALES", menu.mirror?"Las curvas van al revés: ¡todo es nuevo!":"Las pistas como siempre" },
            };
            for (int i = 0; i < rows.Length; i++)
            {
                double y = 74 + i * 86; bool sel = menu.row == i;
                panel(200, y, 560, 78, sel ? 0.8 : 0.5, sel ? Y_ : Color.Empty);
                double ix = 248, iy = y + 39;
                if (i == 0) { if (np == 1) drawHead(CHARS[0], ix, iy, 26); else { drawHead(CHARS[0], ix - 16, iy, 22); drawHead(CHARS[1], ix + 18, iy + 4, 22); } }
                else if (i == 1)
                {
                    if (menu.mode == 0) drawTrophy(ix, iy + 24, 46, C("#ffd700"));
                    else if (menu.mode == 1) { for (int a = 0; a < 4; a++) for (int bb = 0; bb < 4; bb++) if ((a + bb) % 2 == 1) rect(ix - 20 + a * 10, iy - 20 + bb * 10, 10, 10, W_); rectStroke(ix - 20, iy - 20, 40, 40, W_, 2); }
                    else if (menu.mode == 3) drawSoccerBall(ix, iy + Math.Abs(Math.Sin(T * 5)) * -6, 22);
                    else { circ(ix, iy, 24, W_); circ(ix, iy, 20, C("#2a2a3a")); lines(W_, 3, true, ix, iy - 15, ix, iy, ix + 11, iy + 4); }
                }
                else if (i == 2 && fut) { circ(ix, iy, 24, W_); circ(ix, iy, 20, C("#2a2a3a")); lines(W_, 3, true, ix, iy - 15, ix, iy, ix + 11, iy + 4); }
                else if (i == 2) drawMushroom(ix, iy - 6, 44);
                else if (i == 3) { for (int k = 0; k < 3; k++) { if (k <= menu.diff) drawStar(ix - 22 + k * 22, iy, 11); else g.DrawPolygon(P(U.RGBA(255, 255, 255, 0.5), 2), starPts(ix - 22 + k * 22, iy, 11, 5)); } }
                else if (fut) { var cf = CANCHAS[menu.sfield]; rect(ix - 26, iy - 17, 52, 34, cf.g1); rectStroke(ix - 26, iy - 17, 52, 34, cf.line, 2); lines(cf.line, 2, false, ix, iy - 17, ix, iy + 17); ellStroke(ix, iy, 7, 7, cf.line, 2); }
                else { var st = Save(); Translate(ix, iy); if (menu.mirror) Scale(-1, 1); txt("R", 0, 0, 34, C("#7ad0ff")); Restore(st); }
                txt((sel ? "<   " : "") + rows[i][0] + (sel ? "   >" : ""), 515, y + 29, rows[i][0].Length > 20 ? 20 : 26, sel ? Y_ : W_);
                txt(rows[i][1], 515, y + 58, 13, C("#dddddd"), "center", Color.Black, 3);
            }
            hint("Arriba/abajo: elegir opción   Izq/der: cambiar   ENTER aceptar   ESC atrás");
        }
        static void drawCharSel()
        {
            bool duo = menu.np > 1;
            dim(0.45); txt(duo ? "ELIJAN SUS PERSONAJES" : "ELIGE TU PERSONAJE", W / 2, 44, 38, Y_);
            int cols = 8, nrows = (CHARS.Length + 7) / 8; double cw = 78, chh = nrows > 4 ? 56 : nrows > 3 ? 68 : 86, gx = 34, gy = 80, hr = nrows > 4 ? 15 : nrows > 3 ? 18 : 21, py = Math.Max(350, gy + nrows * chh + 6);
            for (int i = 0; i < CHARS.Length; i++)
            {
                var ch = CHARS[i]; double cx = gx + (i % cols) * cw, cy = gy + (i / cols) * chh;
                var who = new List<int>(); for (int p = 0; p < menu.np; p++) if (menu.ch[p] == i) who.Add(p);
                bool sel = who.Count > 0; Color col = duo && sel ? P_COL[who[who.Count - 1]] : Y_;
                rrFill(cx, cy, cw - 6, chh - 6, 12, sel ? U.RGBA(255, 215, 0, 0.3) : U.RGBA(10, 10, 40, 0.6));
                rrStroke(cx, cy, cw - 6, chh - 6, 12, sel ? col : U.RGBA(255, 255, 255, 0.5), sel ? 5 : 2);
                bool locked = who.Any(p => menu.lck[p]);
                drawHead(ch, cx + (cw - 6) / 2, cy + chh * 0.4 + (sel && !locked ? Math.Sin(T * 8) * 3 : 0), hr);
                txt(ch.name, cx + (cw - 6) / 2, cy + chh - (nrows > 4 ? 14 : 16), ch.name.Length > 8 ? 9 : nrows > 4 ? 10 : 11, W_, "center", Color.Black, 3);
                if (duo) foreach (int p in who) txt("J" + (p + 1) + (menu.lck[p] ? " OK" : ""), p == 0 ? cx + 18 : cx + cw - 24, cy + 11, 10, P_COL[p], "center", Color.Black, 3);
            }
            panel(670, 80, 270, 380, 0.65, Color.Empty);
            if (!duo)
            {
                var ch = CHARS[menu.ch[0]]; drawHead(ch, 805, 165, 56); txt(ch.name, 805, 248, 28, W_); txt(CLS_NAME[ch.cls], 805, 276, 15, C("#9ad8ff"));
                var PW = PowerOf(ch); txt("PODER: " + PW[1], 805, 302, 15, PW_ICON[PW[0]], "center", Color.Black, 3); txt(PW[2], 805, 322, 11, W_, "center", Color.Black, 3);
                statBars(688, 352, ClassStats(ch));
            }
            else for (int p = 0; p < 2; p++)
                {
                    var ch = CHARS[menu.ch[p]]; double top = 88 + p * 186;
                    txt("J" + (p + 1), 692, top + 18, 20, P_COL[p], "left"); drawHead(ch, 740, top + 95, 46);
                    txt(ch.name, 865, top + 65, ch.name.Length > 8 ? 16 : 20, W_); txt(CLS_NAME[ch.cls], 865, top + 92, 13, C("#9ad8ff"), "center", Color.Black, 3);
                    { var PW = PowerOf(ch); txt(PW[1], 865, top + 114, 12, PW_ICON[PW[0]], "center", Color.Black, 3); }
                    if (menu.lck[p]) txt("¡LISTO!", 865, top + 146, 22, C("#7aff7a"));
                    else if (((int)Math.Floor(T * 3)) % 2 == 1) txt("eligiendo...", 865, top + 146, 14, C("#dddddd"), "center", Color.Black, 3);
                }
            panel(34, py, 620, 460 - py, 0.5, Color.Empty);
            double lh = (460 - py) / 4;
            txt("Ligeros: aceleran rápido y giran fácil.", 344, py + lh, 16, W_, "center", Color.Black, 3);
            txt("Medios: equilibrados en todo.", 344, py + lh * 2, 16, W_, "center", Color.Black, 3);
            txt("Pesados: más velocidad punta y empujan a los demás.", 344, py + lh * 3, 16, W_, "center", Color.Black, 3);
            hint(duo ? "J1: WASD + ESPACIO   -   J2: Flechas + ENTER   -   Q / Retroceso cancelar   -   ESC atrás" : "Flechas para moverte   ENTER aceptar   ESC atrás");
        }
        static void drawKartSel()
        {
            bool duo = menu.np > 1;
            dim(0.45); txt(duo ? "ELIJAN SUS VEHÍCULOS" : "ELIGE TU VEHÍCULO", W / 2, 44, 38, Y_);
            int cols = 4; double cw = 156, chh = 170, gx = 40, gy = 88;
            for (int i = 0; i < KARTS.Length; i++)
            {
                var k = KARTS[i]; double cx = gx + (i % cols) * cw, cy = gy + (i / cols) * chh;
                var who = new List<int>(); for (int p = 0; p < menu.np; p++) if (menu.kt[p] == i) who.Add(p);
                bool sel = who.Count > 0; Color col = duo && sel ? P_COL[who[who.Count - 1]] : Y_;
                rrFill(cx, cy, cw - 10, chh - 10, 14, sel ? U.RGBA(255, 215, 0, 0.3) : U.RGBA(10, 10, 40, 0.6));
                rrStroke(cx, cy, cw - 10, chh - 10, 14, sel ? col : U.RGBA(255, 255, 255, 0.5), sel ? 5 : 2);
                var d = makeDummy(CHARS[menu.ch[sel ? who[0] : 0]], k); if (!sel) d.steerVis = 0;
                drawKart(d, cx + (cw - 10) / 2, cy + 118, 100); txt(k.name, cx + (cw - 10) / 2, cy + 140, 14, W_, "center", Color.Black, 3);
                if (duo) foreach (int p in who) txt("J" + (p + 1) + (menu.lck[p] ? " OK" : ""), p == 0 ? cx + 24 : cx + cw - 34, cy + 14, 13, P_COL[p], "center", Color.Black, 3);
            }
            panel(670, 88, 270, 372, 0.65, Color.Empty);
            if (!duo)
            {
                var ch = CHARS[menu.ch[0]]; var k = KARTS[menu.kt[0]];
                drawKart(makeDummy(ch, k), 805, 250, 170);
                txt(k.name, 805, 286, 24, W_); txt(ch.name + (k.id == "buggy" ? " - bueno fuera de pista" : ""), 805, 312, 13, C("#9ad8ff"), "center", Color.Black, 3);
                statBars(688, 345, Stats(ch, k));
            }
            else for (int p = 0; p < 2; p++)
                {
                    var ch = CHARS[menu.ch[p]]; var k = KARTS[menu.kt[p]]; double top = 96 + p * 184;
                    txt("J" + (p + 1), 690, top + 16, 18, P_COL[p], "left"); txt(k.name, 850, top + 16, 15, W_);
                    drawKart(makeDummy(ch, k), 728, top + 130, 84);
                    statBars(778, top + 48, Stats(ch, k), true);
                    if (menu.lck[p]) txt("¡LISTO!", 728, top + 160, 16, C("#7aff7a"));
                }
            hint(duo ? "J1: WASD + ESPACIO   -   J2: Flechas + ENTER   -   Q / Retroceso cancelar   -   ESC atrás" : "Flechas para moverte   ENTER aceptar   ESC atrás");
        }
        static void drawCupSel()
        {
            dim(0.45); txt("ELIGE LA COPA", W / 2, 50, 40, Y_);
            for (int i = 0; i < CUPS.Length; i++)
            {
                var c = CUPS[i]; double x = 24 + i * 232, y = 100, cx = x + 106; bool sel = menu.cup == i; double b = sel ? Math.Sin(T * 6) : 0;
                panel(x, y, 212, 330, sel ? 0.8 : 0.5, sel ? Y_ : Color.Empty);
                int won = cupBest(i); if (won > 0) drawTrophy(x + 186, y + 48, 34, placeCol(won));
                if (c.icon == "mushroom") drawMushroom(cx, y + 80, 100 + b * 6);
                else if (c.icon == "star") drawStar(cx, y + 90, 55 + b * 4);
                else if (c.icon == "flower") drawFlower(cx, y + 90, 50 + b * 4);
                else crown(cx, y + 92, 110 + b * 6, C("#e52521"));
                txt("COPA", cx, y + 170, 20, sel ? Y_ : W_); txt(c.name.ToUpper(), cx, y + 198, c.name.Length > 8 ? 20 : 24, sel ? Y_ : W_);
                for (int j = 0; j < c.tracks.Length; j++) { var t = TRACKS[c.tracks[j]]; txt(t.name, cx, y + 245 + j * 28, t.name.Length > 16 ? 12 : 14, W_, "center", Color.Black, 3); }
            }
            hint("Izq/der: elegir   ENTER empezar   ESC atrás");
        }
        static void drawTrackCard(double x, double y, double w, double h, int ti, bool sel)
        {
            var th = THEMES[TRACKS[ti].theme];
            var st = Save();
            using (var path = rrPath(x, y, w, h, 14)) g.SetClip(path, System.Drawing.Drawing2D.CombineMode.Intersect);
            double hy = y + h * 0.5;
            Gradient(x, y, w, h * 0.5, th.sky[0], th.sky[1]);
            rect(x, hy, w, h, th.lava ? C("#ff5a00") : th.grass[0]);
            Color roadCol = th.rainbow ? U.Hsl(T * 60, 0.85, 0.55) : th.road[0];
            fillPts(th.rumble[1], x + w * 0.47, hy, x + w * 0.53, hy, x + w * 0.92, y + h, x + w * 0.08, y + h);
            fillPts(roadCol, x + w * 0.48, hy, x + w * 0.52, hy, x + w * 0.86, y + h, x + w * 0.14, y + h);
            var oldTheme = theme; theme = th;
            drawDecor(new Decor { t = th.decor[0], x = -1, h = 0.3 }, x + w * 0.12, hy + 24, 0.028);
            drawDecor(new Decor { t = th.decor[1], x = 1, h = 0.7 }, x + w * 0.88, hy + 30, 0.032);
            theme = oldTheme;
            Restore(st);
            rrStroke(x, y, w, h, 14, sel ? Y_ : U.RGBA(255, 255, 255, 0.6), sel ? 5 : 2);
        }
        static void drawTrackSel()
        {
            dim(0.45); txt("ELIGE LA PISTA", W / 2, 44, 38, Y_);
            for (int i = 0; i < TRACKS.Count; i++)
            {
                var t = TRACKS[i]; double x = 36 + (i % 4) * 226, y = 78 + (i / 4) * 146; bool sel = i == menu.tr;
                drawTrackCard(x, y, 210, 104, i, sel); txt(t.name, x + 105, y + 120, 15, sel ? Y_ : W_);
                if (menu.mode == 2) { var rec = getRec(i); txt(rec != null ? "Récord " + U.FmtTime(rec.total) : "Sin récord", x + 105, y + 92, 13, rec != null ? C("#7aff7a") : C("#dddddd"), "center", Color.Black, 3); }
            }
            hint("Flechas para moverte   ENTER correr   ESC atrás");
        }
        static void drawPause()
        {
            dim(0.55); panel(330, 140, 300, 260, 0.85, Color.Empty); txt("PAUSA", W / 2, 180, 40, Y_);
            var o = new[] { "Continuar", "Reiniciar carrera", "Salir al menú" };
            for (int i = 0; i < 3; i++) txt((menu.p == i ? "> " : "") + o[i], W / 2, 245 + i * 50, 24, menu.p == i ? Y_ : W_);
        }
        static void drawHelp()
        {
            dim(0.72); txt("CÓMO JUGAR", W / 2, 36, 34, Y_);
            var order = new[] { "mushroom", "triple", "golden", "banana", "triple_banana", "green", "triple_green", "red", "blue", "bomb", "star", "lightning", "bullet", "blooper", "boo", "horn", "coin" };
            for (int i = 0; i < order.Length; i++)
            {
                string k = order[i]; int col = i < 9 ? 0 : 1; double x = 20 + col * 312, y = 86 + (i % 9) * 47;
                rrFill(x, y - 21, 300, 42, 10, U.RGBA(255, 255, 255, 0.08));
                drawItemIcon(k, x + 24, y, 36); txt(ITEM_NAMES[k], x + 50, y - 9, 13, Y_, "left", Color.Black, 3); txt(ITEM_DESC[k], x + 50, y + 10, 11, W_, "left", Color.Black, 3);
            }
            panel(640, 64, 300, 430, 0.6, Color.Empty); txt("Y ADEMÁS...", 790, 88, 18, C("#7aff7a"));
            var tips = new[] {
                new[]{"box","Cajas ?","Pasa por ellas: te dan","un objeto al azar."}, new[]{"coin","Monedas","Hasta 10: cada una te","hace un poco más rápido."},
                new[]{"pad","Turbo naranja","Pisa las flechas del","piso para acelerar."}, new[]{"ramp","Rampa azul","Salta y pulsa DERRAPE","en el aire: ¡truco + turbo!"},
                new[]{"drift","Derrape","Mantén y gira: chispas","azul, naranja y morado."}, new[]{"go","Salida turbo","Acelera justo cuando","aparece el 2."},
                new[]{"hz","Peligros","Goombas, cangrejos, autos","y Podoboos: ¡esquívalos!"}, new[]{"pw","Súper poder","Llena la barra y pulsa C","(J1: R, J2: L). ¡Cada uno el suyo!"} };
            for (int i = 0; i < tips.Length; i++)
            {
                double y = 122 + i * 46, x = 660; string ic = tips[i][0];
                if (ic == "box") drawBox(x + 16, y + 22, 30);
                else if (ic == "coin") drawCoinIcon(x + 16, y, 30);
                else if (ic == "pad") { rect(x + 2, y - 12, 28, 24, C("#ff8a00")); txt(">>", x + 16, y, 16, C("#fff2a0"), "center", Color.Black, 2); }
                else if (ic == "ramp") fillPts(C("#2a6ae8"), x, y + 12, x + 32, y + 12, x + 32, y - 12);
                else if (ic == "drift") { circ(x + 8, y, 6, C("#4ab8ff")); circ(x + 18, y - 4, 6, C("#ff9a1a")); circ(x + 28, y, 6, C("#d05aff")); }
                else if (ic == "go") circ(x + 16, y, 13, C("#3aff6a"));
                else if (ic == "pw") drawStar(x + 16, y, 13);
                else drawHazard(new Hazard { type = "goomba" }, x + 16, y + 16, 0.08);
                txt(tips[i][1], x + 42, y - 12, 14, Y_, "left", Color.Black, 3); txt(tips[i][2], x + 42, y + 5, 11, W_, "left", Color.Black, 3); txt(tips[i][3], x + 42, y + 20, 11, W_, "left", Color.Black, 3);
            }
            hint("ENTER o ESC: volver");
        }
        static void drawResults()
        {
            if (results.tt != null)
            {
                var r = results.tt; var p = players[0]; dim(0.5); txt("CONTRARRELOJ - " + TRACKS[race.ti].name.ToUpper(), W / 2, 50, 30, Y_);
                panel(230, 90, 500, 360, 0.8, Color.Empty); drawHead(p.ch, W / 2, 150, 40);
                txt("TIEMPO TOTAL", W / 2, 215, 18, C("#9ad8ff")); txt(U.FmtTime(r.total), W / 2, 255, 48, W_);
                for (int i = 0; i < r.laps.Count; i++) txt("Vuelta " + (i + 1) + ":  " + U.FmtTime(r.laps[i]), W / 2, 305 + i * 28, 18, r.laps[i] == r.best ? C("#7aff7a") : W_);
                if (r.newRec && ((int)Math.Floor(T * 4)) % 2 == 1) txt("¡NUEVO RÉCORD!", W / 2, 420, 30, Y_);
                else if (!r.newRec) txt("Récord: " + U.FmtTime(r.old.total), W / 2, 420, 20, C("#dddddd"));
                if (((int)Math.Floor(T * 2)) % 2 == 0) hint("ENTER: elegir otra pista");
                return;
            }
            dim(0.45); txt(gp != null ? "RESULTADOS - CARRERA " + (gp.race + 1) + "/3" : "RESULTADOS", W / 2, 40, 34, Y_);
            bool duo = players.Count > 1;
            for (int i = 0; i < results.list.Count; i++)
            {
                var e = results.list[i]; double y = 92 + i * 50; bool isP = e.r.isPlayer;
                rrFill(140, y - 21, 680, 44, 12, isP ? (duo ? P_COL[e.r.pidx] : U.RGBA(255, 200, 0, 0.8)) : U.RGBA(20, 20, 60, 0.8));
                txt((i + 1) + "º", 185, y, 26, placeCol(i + 1)); drawHead(e.r.ch, 245, y, 18);
                txt(e.r.ch.name + (isP && duo ? "  (J" + (e.r.pidx + 1) + ")" : ""), 275, y, 20, W_, "left"); txt(U.FmtTime(e.time), 540, y, 18, W_);
                if (gp != null) { txt("+" + e.pts, 650, y, 18, C("#7aff7a")); txt(gp.points[e.r.ch.id] + " pts", 750, y, 18, W_); }
            }
            if (((int)Math.Floor(T * 2)) % 2 == 0) hint(gp != null ? (gp.race < 2 ? "ENTER: siguiente carrera" : "ENTER: ver el podio") : "ENTER: elegir otra pista");
        }
        static void drawPodium()
        {
            dim(0.5);
            var st = gp.roster.OrderByDescending(e => gp.points[e.ch.id]).ToList();
            var hum = new List<int[]>(); for (int i = 0; i < st.Count; i++) if (st[i].isPlayer) hum.Add(new[] { st[i].pidx, i + 1 });
            int pi = hum.Min(h => h[1]);
            txt("COPA " + CUPS[gp.cup].name.ToUpper(), W / 2, 44, 40, Y_);
            string msg;
            if (hum.Count > 1)
            {
                var w = hum.First(h => h[1] == pi);
                msg = (pi == 1 ? "¡J" + (w[0] + 1) + " GANÓ LA COPA!   " : "") + string.Join("   -   ", hum.Select(h => "J" + (h[0] + 1) + ": " + h[1] + "º").ToArray());
            }
            else msg = pi <= 3 ? "¡" + new[] { "GANASTE EL TROFEO DE ORO", "GANASTE EL TROFEO DE PLATA", "GANASTE EL TROFEO DE BRONCE" }[pi - 1] + "!" : "Quedaste en " + pi + "º lugar. ¡Inténtalo otra vez!";
            txt(msg, W / 2, 94, 26, pi <= 3 ? C("#7aff7a") : W_);
            if (pi <= 3) { drawTrophy(150, 180, 80, placeCol(pi)); drawTrophy(W - 150, 180, 80, placeCol(pi)); }
            var spots = new[] { new[] { 0, W / 2, 170 }, new[] { 1, W / 2 - 210, 125 }, new[] { 2, W / 2 + 210, 90 } };
            foreach (var s in spots)
            {
                int i = s[0]; double x = s[1], h = s[2]; if (i >= st.Count) continue; var e = st[i]; double by = 470;
                rrFill(x - 85, by - h, 170, h, 10, placeCol(i + 1)); txt((i + 1).ToString(), x, by - h / 2, 54, W_);
                drawHead(e.ch, x, by - h - 50 + Math.Sin(T * 5 + i) * 5, 40); txt(e.ch.name, x, by - h - 112, 20, e.isPlayer ? C("#ffe14a") : W_); txt(gp.points[e.ch.id] + " pts", x, by + 20, 18, W_);
            }
            for (int i = 0; i < 3; i++) if (U.R() < 0.5) addPart(U.R() * W, -10, (U.R() - 0.5) * 60, 60 + U.R() * 80, 5, U.Hsl(U.R() * 360, 0.9, 0.6), 7, 30);
            hint("ENTER: volver al menú");
        }
        static void drawParts()
        {
            double oa = ALPHA;
            foreach (var p in parts) { ALPHA = Math.Max(0, Math.Min(1, p.life / p.max)); rect(p.x - p.size / 2, p.y - p.size / 2, p.size, p.size, p.col); }
            ALPHA = oa;
        }

        // ---------------- dibujar todo ----------------
        public static void Draw()
        {
            if (isSoc() && soc != null)
            {
                drawSoccer();
                if (state == "spause") drawSocPause(); else if (state == "sres") drawSocRes();
                drawParts();
                return;
            }
            if (race != null)
            {
                if (cams.Count > 1) for (int i = 0; i < cams.Count; i++) renderView(cams[i], i * W / 2.0, W / 2.0);
                else renderView(cams[0], 0, W);
                if (!race.demo && (state == "race" || state == "pause" || state == "results")) drawRaceOverlay();
            }
            switch (state)
            {
                case "title": drawTitle(); break;
                case "mode": drawMode(); break;
                case "help": drawHelp(); break;
                case "char": drawCharSel(); break;
                case "kart": drawKartSel(); break;
                case "cup": drawCupSel(); break;
                case "track": drawTrackSel(); break;
                case "pause": drawPause(); break;
                case "results": drawResults(); break;
                case "podium": drawPodium(); break;
            }
            drawParts();
        }
    }
}
