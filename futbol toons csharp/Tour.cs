using System;
using System.Collections.Generic;
using System.Linq;

namespace FutbolToons
{
    // ======================= TORNEOS =======================
    public class Fix { public Team a, b; public int ga = -1, gb = -1, pa, pb; public bool pens; public bool Played { get { return ga >= 0; } } public Team Winner { get { return ga > gb ? a : gb > ga ? b : pa > pb ? a : b; } } }
    public class Row { public Team t; public int pj, g, e, p, gf, gc; public int Pts { get { return g * 3 + e; } } public int Dg { get { return gf - gc; } } }
    public class Tour
    {
        public Comp comp; public Team me; public List<Team> teams = new List<Team>();
        public List<List<Fix>> rounds = new List<List<Fix>>(); public int round;   // liga y grupos
        public List<Row> table = new List<Row>();                                 // liga
        public List<Row>[] gtab;                                                    // grupos del Mundial (A y B)
        public List<Fix> ko = new List<Fix>(); public List<List<Fix>> koDone = new List<List<Fix>>(); public string stage = "";
        public bool inKo, done, champion; public string endText = "";
    }

    static partial class G
    {
        public static Tour tour;
        static string[] KO_NAMES = { "Final", "Semifinal", "Cuartos de final" };

        // reparte personajes al azar a los equipos rivales (sin repetir)
        static void giveSquads(IEnumerable<Team> others, List<Ch> taken)
        {
            var pool = U.Shuffle(CHARS.Where(c => !taken.Contains(c)));
            int i = 0;
            foreach (var t in others)
            {
                if (i + 5 > pool.Count) { pool = U.Shuffle(CHARS.Where(c => !taken.Contains(c))); i = 0; }
                t.squad = pool.Skip(i).Take(5).ToList(); i += 5;
            }
        }
        public static List<Ch> RandomSquad(IEnumerable<Ch> avoid = null)
        {
            return U.Shuffle(CHARS.Where(c => avoid == null || !avoid.Contains(c))).Take(5).ToList();
        }
        public static List<Team> CompTeams(Comp c)
        {
            switch (c.id)
            {
                case "premier": return CLUBS.Take(6).ToList();
                case "laliga": case "supercopa": return CLUBS.Skip(6).Take(6).ToList();
                case "mundial": return NATIONS.ToList();
                case "champions": return CLUBS.ToList();
                default: return CLUBS.Concat(NATIONS).ToList();
            }
        }
        static Row rowOf(List<Row> tab, Team t) { return tab.First(r => r.t == t); }
        public static List<Row> Sorted(List<Row> tab) { return tab.OrderByDescending(r => r.Pts).ThenByDescending(r => r.Dg).ThenByDescending(r => r.gf).ThenBy(r => r.t.name).ToList(); }
        // todos contra todos (método del círculo)
        static List<List<Fix>> roundRobin(List<Team> ts)
        {
            var list = new List<Team>(ts); int n = list.Count; var res = new List<List<Fix>>();
            for (int r = 0; r < n - 1; r++)
            {
                var rd = new List<Fix>();
                for (int i = 0; i < n / 2; i++) { var a = list[i]; var b = list[n - 1 - i]; rd.Add(r % 2 == 0 ? new Fix { a = a, b = b } : new Fix { a = b, b = a }); }
                res.Add(rd);
                var last = list[n - 1]; list.RemoveAt(n - 1); list.Insert(1, last);
            }
            return res;
        }
        static List<Fix> pairUp(List<Team> ts) { var r = new List<Fix>(); for (int i = 0; i < ts.Count; i += 2) r.Add(new Fix { a = ts[i], b = ts[i + 1] }); return r; }

        public static Tour NewTour(Comp c, Team me)
        {
            var t = new Tour { comp = c, me = me };
            var pool = CompTeams(c).Where(x => x != me).ToList();
            int n = c.id == "supercopa" ? 4 : c.fmt == "league" ? 6 : 8;
            List<Team> others = c.id == "champions" ? U.Shuffle(pool).OrderByDescending(x => x.rating + U.R() * 2).Take(n - 1).ToList() : U.Shuffle(pool).Take(n - 1).ToList();
            t.teams = new[] { me }.Concat(others).ToList();
            giveSquads(others, me.squad);
            if (c.fmt == "league")
            {
                t.rounds = roundRobin(U.Shuffle(t.teams));
                t.table = t.teams.Select(x => new Row { t = x }).ToList();
            }
            else if (c.fmt == "ko")
            {
                var order = U.Shuffle(t.teams); t.ko = pairUp(order); t.inKo = true; t.stage = KO_NAMES[(int)Math.Log(t.teams.Count, 2) - 1];
            }
            else
            {
                var A = new[] { me }.Concat(others.Take(3)).ToList(); var B = others.Skip(3).Take(4).ToList();
                t.gtab = new[] { A.Select(x => new Row { t = x }).ToList(), B.Select(x => new Row { t = x }).ToList() };
                var ra = roundRobin(A); var rb = roundRobin(B);
                for (int i = 0; i < 3; i++) t.rounds.Add(ra[i].Concat(rb[i]).ToList());
                t.stage = "Fase de grupos";
            }
            return t;
        }
        public static Fix NextFix(Tour t)
        {
            if (t.done) return null;
            if (t.inKo) return t.ko.FirstOrDefault(f => f.a == t.me || f.b == t.me);
            return t.rounds[t.round].First(f => f.a == t.me || f.b == t.me);
        }
        public static string StageName(Tour t)
        {
            if (t.inKo) return t.stage;
            if (t.comp.fmt == "league") return "Jornada " + (t.round + 1) + " de " + t.rounds.Count;
            return "Grupos - Fecha " + (t.round + 1) + " de 3";
        }
        // resultado simulado según la fuerza de cada equipo
        static int poisson(double l) { double L = Math.Exp(-l), p = 1; int k = 0; do { k++; p *= U.R(); } while (p > L && k < 9); return k - 1; }
        static void sim(Fix f, bool ko)
        {
            double la = U.Clamp(1.3 + 0.3 * (f.a.rating - f.b.rating), 0.4, 3), lb = U.Clamp(1.3 + 0.3 * (f.b.rating - f.a.rating), 0.4, 3);
            f.ga = poisson(la); f.gb = poisson(lb);
            if (ko && f.ga == f.gb) { f.pens = true; f.pa = 3 + U.rng.Next(3); f.pb = f.pa + (U.R() < 0.5 ? 1 : -1); if (f.pb < 0) f.pb = f.pa + 1; }
        }
        static void addToTable(List<Row> tab, Fix f)
        {
            var ra = rowOf(tab, f.a); var rb = rowOf(tab, f.b);
            ra.pj++; rb.pj++; ra.gf += f.ga; ra.gc += f.gb; rb.gf += f.gb; rb.gc += f.ga;
            if (f.ga > f.gb) { ra.g++; rb.p++; } else if (f.gb > f.ga) { rb.g++; ra.p++; } else { ra.e++; rb.e++; }
        }
        // se jugó el partido del jugador: se simulan los demás y avanza el torneo
        public static void TourResult(Tour t, int gMe, int gOpp, int pMe, int pOpp)
        {
            var f = NextFix(t); bool meA = f.a == t.me;
            f.ga = meA ? gMe : gOpp; f.gb = meA ? gOpp : gMe; f.pa = meA ? pMe : pOpp; f.pb = meA ? pOpp : pMe; f.pens = pMe + pOpp > 0;
            if (t.inKo)
            {
                foreach (var o in t.ko) if (!o.Played) sim(o, true);
                t.koDone.Add(t.ko);
                if (f.Winner != t.me) { t.done = true; t.endText = "Eliminado en " + (t.stage == "Final" ? "la final" : t.stage == "Semifinal" ? "la semifinal" : "los cuartos de final"); return; }
                if (t.stage == "Final") { t.done = true; t.champion = true; return; }
                var win = t.ko.Select(x => x.Winner).ToList();
                t.ko = pairUp(win); t.stage = KO_NAMES[(int)Math.Log(win.Count, 2) - 1];
                return;
            }
            foreach (var o in t.rounds[t.round]) if (!o.Played) sim(o, false);
            foreach (var o in t.rounds[t.round])
            {
                if (t.comp.fmt == "league") addToTable(t.table, o);
                else addToTable(t.gtab[t.gtab[0].Any(r => r.t == o.a) ? 0 : 1], o);
            }
            t.round++;
            if (t.round < t.rounds.Count) return;
            if (t.comp.fmt == "league")
            {
                t.done = true; int pos = Sorted(t.table).FindIndex(r => r.t == t.me) + 1;
                t.champion = pos == 1; t.endText = "Terminaste en el " + pos + "º lugar";
                return;
            }
            // Mundial: pasan los 2 primeros de cada grupo
            var A = Sorted(t.gtab[0]); var B = Sorted(t.gtab[1]);
            if (A[0].t != t.me && A[1].t != t.me) { t.done = true; t.endText = "Eliminado en la fase de grupos (" + (A.FindIndex(r => r.t == t.me) + 1) + "º del grupo)"; return; }
            t.inKo = true; t.ko = new List<Fix> { new Fix { a = A[0].t, b = B[1].t }, new Fix { a = B[0].t, b = A[1].t } }; t.stage = "Semifinal";
        }
    }
}
