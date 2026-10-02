using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FutbolToons
{
    // ======================= tipos de datos =======================
    public class Ch   // personaje
    {
        public string id, name, kind, letter = "", cls = "medio", series;
        public Color hat, shirt, over, hair, skin, kart, dress, gem, spot, vest, body, spike, shell, hood, fur, face, cap, bow, stache, nose;
        public bool big, thin, flip, pigtails, junior, bones;
        public bool glasses; public double tall = 1; public string hairStyle = "short";
        public int spd = 3, sht = 3, tec = 3;          // velocidad, tiro y técnica (1 a 5)
        public string pw = "fire", pwName = "Súper tiro";  // súper tiro
        public Ch Clone() { return (Ch)MemberwiseClone(); }
    }
    public class Kit { public string pat; public Color shirt, shirt2, shorts, socks; }   // uniforme
    public class Team
    {
        public string id, name, abbr; public Kit home, away; public bool nation; public int rating = 3;
        public List<Ch> squad = new List<Ch>();   // 4 jugadores + arquero (el último)
    }
    public class Comp { public string id, name, fmt, desc; public Color col, col2; }      // competición
    public class Stadium { public string name; public Color g1, g2, line, outside, sky0, sky1, stand, wall, wall2; public bool night, ice, sea, tiles; }
    public class Song { public double bpm; public int[] mel, bass; }

    static partial class G
    {
        static Color C(string h) { return U.C(h); }
        public static Ch[] CHARS;
        public static Team[] CLUBS, NATIONS;
        public static Comp[] COMPS;
        public static Stadium[] STADIUMS;
        public static Song[] SONGS;
        public static Dictionary<string, Color> PW_COL;
        public static Color[] P_COL;
        public static string[] SERIES = { "Dragon Ball", "Bob Esponja", "Pokémon", "Los Simpson", "Scooby-Doo", "Hora de Aventura", "Tom y Jerry", "Looney Tunes", "Mario" };

        static Kit K(string pat, string a, string b, string shorts, string socks) { return new Kit { pat = pat, shirt = C(a), shirt2 = C(b), shorts = C(shorts), socks = C(socks) }; }
        static Team TM(string id, string name, string abbr, int rating, Kit home, Kit away, bool nation = false) { return new Team { id = id, name = name, abbr = abbr, rating = rating, home = home, away = away, nation = nation }; }
        static Ch P(Ch c, string series, int spd, int sht, int tec, string pw, string pwName) { c.series = series; c.spd = spd; c.sht = sht; c.tec = tec; c.pw = pw; c.pwName = pwName; return c; }

        public static void InitData()
        {
            const string DB = "Dragon Ball", BE = "Bob Esponja", PK = "Pokémon", SI = "Los Simpson", SC = "Scooby-Doo", HA = "Hora de Aventura", TJ = "Tom y Jerry", LT = "Looney Tunes", MA = "Mario";
            CHARS = new[] {
                // ---- Dragon Ball ----
                P(new Ch{id="goku",name="Goku",kind="saiyan",hairStyle="goku",skin=C("#ffd0a8"),hair=C("#151515"),shirt=C("#ff7a1a"),vest=C("#1a4ad8")},DB,4,5,4,"kame","Kamehameha"),
                P(new Ch{id="vegeta",name="Vegeta",kind="saiyan",hairStyle="vegeta",skin=C("#ffd8b8"),hair=C("#151515"),shirt=C("#1a3aa8"),vest=C("#f4f4f4")},DB,4,5,3,"ssj","Súper Saiyajin"),
                P(new Ch{id="gohan",name="Gohan",kind="saiyan",hairStyle="gohan",skin=C("#ffd8b8"),hair=C("#151515"),shirt=C("#5a2aa8"),vest=C("#e52521"),tall=0.9},DB,4,4,4,"kame","Masenko"),
                P(new Ch{id="piccolo",name="Piccolo",kind="saiyan",hairStyle="piccolo",skin=C("#5ac85a"),hair=C("#f4f4f4"),shirt=C("#5a2aa8"),vest=C("#f0f0f8"),tall=1.15},DB,3,4,4,"laser","Makankosappo"),
                P(new Ch{id="krilin",name="Krilin",kind="saiyan",hairStyle="krilin",skin=C("#ffd0a8"),hair=C("#151515"),shirt=C("#ff7a1a"),vest=C("#1a4ad8"),tall=0.85},DB,4,3,4,"freeze","Taiyoken"),
                P(new Ch{id="freezer",name="Freezer",kind="saiyan",hairStyle="freezer",skin=C("#f4f0f8"),hair=C("#8a3ab8"),shirt=C("#f4f0f8"),vest=C("#8a3ab8"),tall=0.9},DB,4,4,3,"fire","Rayo mortal"),
                P(new Ch{id="trunks",name="Trunks",kind="saiyan",hairStyle="trunks",skin=C("#ffd8b8"),hair=C("#b8a0e8"),shirt=C("#2a4ab8"),vest=C("#222222")},DB,4,4,4,"spin","Corte de espada"),
                P(new Ch{id="cell",name="Cell",kind="saiyan",hairStyle="cell",skin=C("#e8dccc"),hair=C("#4ab84a"),shirt=C("#4ab84a"),vest=C("#1a1a1a"),tall=1.15},DB,3,5,3,"kame","Kamehameha"),
                P(new Ch{id="buu",name="Majin Buu",kind="saiyan",hairStyle="buu",skin=C("#ff9ac8"),hair=C("#ff9ac8"),shirt=C("#6a2a9a"),vest=C("#ffd23a"),big=true},DB,2,4,3,"freeze","Rayo de chocolate"),
                P(new Ch{id="broly",name="Broly",kind="saiyan",hairStyle="broly",skin=C("#e8b088"),hair=C("#151515"),shirt=C("#e8b088"),vest=C("#ffd23a"),big=true,tall=1.15},DB,3,5,2,"ssj","Saiyajin Legendario"),
                P(new Ch{id="bills",name="Bills",kind="saiyan",hairStyle="bills",skin=C("#8a6ac8"),hair=C("#8a6ac8"),shirt=C("#222244"),vest=C("#ffd23a"),thin=true},DB,3,5,4,"shock","Destrucción"),
                P(new Ch{id="a18",name="Androide 18",kind="saiyan",hairStyle="a18",skin=C("#ffe0cc"),hair=C("#ffe08a"),shirt=C("#2a6ae8"),vest=C("#222222")},DB,5,3,4,"laser","Rayo de energía"),
                // ---- Bob Esponja ----
                P(new Ch{id="bob",name="Bob Esponja",kind="bob",skin=C("#f5e04a"),tall=0.85},BE,4,3,4,"bubble","Tiro de burbuja"),
                P(new Ch{id="patricio",name="Patricio",kind="patricio",skin=C("#ff9aa8"),big=true},BE,2,4,2,"giant","Tiro de roca"),
                P(new Ch{id="calamardo",name="Calamardo",kind="calamardo",skin=C("#8ad0c8"),tall=1.1},BE,2,3,4,"ghost","Clarinetazo"),
                P(new Ch{id="cangrejo",name="Don Cangrejo",kind="cangrejo",skin=C("#e8302a"),tall=0.9},BE,3,3,3,"spin","Pinzazo"),
                P(new Ch{id="arenita",name="Arenita",kind="arenita",skin=C("#a8683a"),tall=0.9},BE,5,4,4,"shock","Golpe de karate"),
                P(new Ch{id="plankton",name="Plankton",kind="plankton",skin=C("#3a9a3a"),tall=0.6},BE,3,2,5,"tele","Plan malvado"),
                // ---- Pokémon ----
                P(new Ch{id="pikachu",name="Pikachu",kind="pikachu",skin=C("#ffd83a"),tall=0.7},PK,5,4,4,"electric","Impactrueno"),
                P(new Ch{id="charmander",name="Charmander",kind="charmander",skin=C("#ff8a3a"),tall=0.75},PK,4,4,3,"fire","Lanzallamas"),
                P(new Ch{id="squirtle",name="Squirtle",kind="squirtle",skin=C("#7ac8e8"),tall=0.7},PK,3,3,4,"kame","Hidrobomba"),
                P(new Ch{id="bulbasaur",name="Bulbasaur",kind="bulbasaur",skin=C("#6ac8a0"),tall=0.7},PK,3,3,4,"lob","Látigo cepa"),
                P(new Ch{id="jigglypuff",name="Jigglypuff",kind="jigglypuff",skin=C("#ffb0c8"),tall=0.65},PK,3,2,4,"freeze","Canto"),
                // ---- Los Simpson ----
                P(new Ch{id="homero",name="Homero",kind="homero",skin=C("#ffd83a"),big=true},SI,2,4,2,"giant","Dona gigante"),
                P(new Ch{id="bart",name="Bart",kind="bart",skin=C("#ffd83a"),tall=0.85},SI,5,3,4,"spin","Patineta"),
                P(new Ch{id="lisa",name="Lisa",kind="lisa",skin=C("#ffd83a"),tall=0.8},SI,3,3,5,"ghost","Solo de saxofón"),
                P(new Ch{id="marge",name="Marge",kind="marge",skin=C("#ffd83a"),hair=C("#2a5ad8"),tall=1.05},SI,3,3,3,"lob","Peinado torre"),
                P(new Ch{id="milhouse",name="Milhouse",kind="milhouse",skin=C("#ffd83a"),hair=C("#3a3a8a"),tall=0.85},SI,3,2,3,"tele","Todos me odian"),
                // ---- Scooby-Doo ----
                P(new Ch{id="scooby",name="Scooby-Doo",kind="scooby",skin=C("#b8783a")},SC,4,3,3,"tele","Galleta Scooby"),
                P(new Ch{id="shaggy",name="Shaggy",kind="shaggy",skin=C("#ffd8b0"),hair=C("#7a5a2a"),tall=1.15,thin=true},SC,5,2,3,"ghost","¡Zoinks!"),
                // ---- Hora de Aventura ----
                P(new Ch{id="finn",name="Finn",kind="finn",skin=C("#ffd8c0"),tall=0.85},HA,4,4,4,"fire","Espada de fuego"),
                P(new Ch{id="jake",name="Jake",kind="jake",skin=C("#f0b030"),tall=0.85},HA,3,3,5,"giant","Jake gigante"),
                // ---- Tom y Jerry ----
                P(new Ch{id="tom",name="Tom",kind="tom",skin=C("#8a9ab0")},TJ,4,3,3,"shock","Sartenazo"),
                P(new Ch{id="jerry",name="Jerry",kind="jerry",skin=C("#b8783a"),tall=0.6},TJ,5,2,5,"tele","Escape del ratón"),
                // ---- Looney Tunes ----
                P(new Ch{id="bugs",name="Bugs Bunny",kind="bugs",skin=C("#b0b0bc")},LT,5,3,5,"ghost","¿Qué hay de nuevo?"),
                P(new Ch{id="lucas",name="Pato Lucas",kind="lucas",skin=C("#222222"),tall=0.9},LT,3,4,2,"fire","Dinamita"),
                // ---- Mario ----
                P(new Ch{id="mario",name="Mario",kind="plumber",letter="M",hat=C("#e52521"),shirt=C("#e52521"),over=C("#2050d0"),hair=C("#5a2d0c"),skin=C("#ffcc99")},MA,4,4,4,"fire","Bola de fuego"),
                P(new Ch{id="luigi",name="Luigi",kind="plumber",letter="L",hat=C("#2fb44a"),shirt=C("#2fb44a"),over=C("#2050d0"),hair=C("#4a2508"),skin=C("#ffcc99"),thin=true},MA,4,3,4,"lob","Súper salto"),
                P(new Ch{id="yoshi",name="Yoshi",kind="yoshi",body=C("#3ec43e"),spike=C("#e52521")},MA,4,3,4,"spin","Lengua de Yoshi"),
                P(new Ch{id="bowser",name="Bowser",kind="bowser",body=C("#f2b632"),hair=C("#e8512a"),shell=C("#2a8a2a"),big=true},MA,2,5,2,"fire","Aliento de fuego"),
            };
            foreach (var c in CHARS) if (c.big) c.cls = "pesado"; else if (c.tall < 0.9) c.cls = "ligero";

            PW_COL = new Dictionary<string, Color> {
                {"fire",C("#ff6a1a")},{"kame",C("#4ab8ff")},{"ssj",C("#ffd800")},{"laser",C("#ff2a2a")},{"freeze",C("#9af0ff")},{"spin",C("#7aff7a")},
                {"shock",C("#ffd23a")},{"bubble",C("#8ae8ff")},{"giant",C("#2a6ae8")},{"ghost",C("#ddddff")},{"tele",C("#c86aff")},{"electric",C("#ffe14a")},{"lob",C("#5ac84a")} };
            P_COL = new[] { C("#ff4a4a"), C("#4aa8ff") };

            // ---- clubes (uniformes inspirados en los equipos de verdad) ----
            CLUBS = new[] {
                TM("mci","Manchester City","MCI",5,K("plain","#6cabdd","#ffffff","#ffffff","#6cabdd"),K("plain","#1c2c5b","#6cabdd","#1c2c5b","#1c2c5b")),
                TM("mun","Manchester United","MUN",4,K("plain","#da291c","#000000","#ffffff","#000000"),K("plain","#ffffff","#da291c","#ffffff","#ffffff")),
                TM("che","Chelsea","CHE",4,K("plain","#034694","#ffffff","#034694","#ffffff"),K("plain","#f2e05a","#034694","#034694","#f2e05a")),
                TM("liv","Liverpool","LIV",5,K("plain","#c8102e","#f6eb61","#c8102e","#c8102e"),K("plain","#e8e4d8","#c8102e","#e8e4d8","#e8e4d8")),
                TM("ars","Arsenal","ARS",4,K("sleeves","#ef0107","#ffffff","#ffffff","#ef0107"),K("plain","#e8c840","#063672","#063672","#e8c840")),
                TM("tot","Tottenham","TOT",3,K("plain","#ffffff","#132257","#132257","#ffffff"),K("plain","#132257","#ffffff","#132257","#132257")),
                TM("rma","Real Madrid","RMA",5,K("plain","#ffffff","#d4af37","#ffffff","#ffffff"),K("plain","#4a2a7a","#ffffff","#4a2a7a","#4a2a7a")),
                TM("bar","Barcelona","BAR",5,K("stripes","#a50044","#004d98","#004d98","#a50044"),K("plain","#f5e04a","#a50044","#f5e04a","#f5e04a")),
                TM("atm","Atlético de Madrid","ATM",4,K("stripes","#cb3524","#ffffff","#272e61","#cb3524"),K("plain","#272e61","#cb3524","#272e61","#272e61")),
                TM("sev","Sevilla","SEV",3,K("plain","#ffffff","#d4021d","#ffffff","#ffffff"),K("plain","#d4021d","#ffffff","#d4021d","#d4021d")),
                TM("val","Valencia","VAL",3,K("plain","#ffffff","#ff7f00","#000000","#ffffff"),K("plain","#ff7f00","#000000","#000000","#ff7f00")),
                TM("bet","Betis","BET",3,K("stripes","#0bb363","#ffffff","#ffffff","#0bb363"),K("plain","#1a3a6a","#0bb363","#1a3a6a","#1a3a6a")),
                TM("bay","Bayern Múnich","BAY",5,K("plain","#dc052d","#ffffff","#dc052d","#dc052d"),K("plain","#ffffff","#dc052d","#ffffff","#ffffff")),
                TM("psg","París SG","PSG",5,K("vband","#004170","#da291c","#004170","#004170"),K("plain","#ffffff","#004170","#ffffff","#ffffff")),
                TM("juv","Juventus","JUV",4,K("stripes","#ffffff","#111111","#111111","#ffffff"),K("plain","#f5c400","#111111","#f5c400","#f5c400")),
                TM("mil","Milan","MIL",4,K("stripes","#fb090b","#111111","#ffffff","#111111"),K("plain","#ffffff","#fb090b","#ffffff","#ffffff")),
                TM("int","Inter","INT",4,K("stripes","#0a2a9a","#111111","#111111","#0a2a9a"),K("plain","#ffffff","#0a2a9a","#ffffff","#ffffff")),
                TM("bvb","Dortmund","BVB",4,K("plain","#fde100","#111111","#111111","#fde100"),K("plain","#111111","#fde100","#111111","#111111")),
            };
            // ---- selecciones (para el Mundial) ----
            NATIONS = new[] {
                TM("arg","Argentina","ARG",5,K("stripes","#75aadb","#ffffff","#111111","#ffffff"),K("plain","#2a2a6a","#75aadb","#2a2a6a","#2a2a6a"),true),
                TM("bra","Brasil","BRA",5,K("chest","#ffdf00","#009c3b","#002776","#ffffff"),K("plain","#002776","#ffdf00","#ffffff","#002776"),true),
                TM("esp","España","ESP",5,K("chest","#c60b1e","#ffc400","#002f6c","#002f6c"),K("plain","#ffffff","#c60b1e","#ffffff","#ffffff"),true),
                TM("fra","Francia","FRA",5,K("chest","#002654","#ed2939","#ffffff","#ed2939"),K("plain","#ffffff","#002654","#002654","#ffffff"),true),
                TM("ale","Alemania","ALE",4,K("chest","#ffffff","#111111","#111111","#ffffff"),K("plain","#111111","#ffffff","#111111","#111111"),true),
                TM("ing","Inglaterra","ING",4,K("chest","#ffffff","#cf081f","#001b5e","#ffffff"),K("plain","#cf081f","#ffffff","#cf081f","#cf081f"),true),
                TM("por","Portugal","POR",4,K("chest","#9b1b30","#006600","#006600","#9b1b30"),K("plain","#ffffff","#9b1b30","#ffffff","#ffffff"),true),
                TM("ita","Italia","ITA",4,K("chest","#0066b3","#ffffff","#ffffff","#0066b3"),K("plain","#ffffff","#0066b3","#ffffff","#ffffff"),true),
                TM("hol","Países Bajos","HOL",4,K("plain","#ff6600","#111111","#ffffff","#ff6600"),K("plain","#1a2a5a","#ff6600","#1a2a5a","#1a2a5a"),true),
                TM("mex","México","MEX",3,K("chest","#006847","#ce1126","#ffffff","#006847"),K("plain","#ffffff","#006847","#ffffff","#ffffff"),true),
                TM("col","Colombia","COL",3,K("chest","#fcd116","#003893","#003893","#ce1126"),K("plain","#003893","#fcd116","#003893","#003893"),true),
                TM("uru","Uruguay","URU",4,K("plain","#5cbfeb","#111111","#111111","#111111"),K("plain","#ffffff","#5cbfeb","#ffffff","#ffffff"),true),
                TM("per","Perú","PER",3,K("sash","#ffffff","#d91023","#ffffff","#ffffff"),K("sash","#d91023","#ffffff","#d91023","#d91023"),true),
                TM("jap","Japón","JAP",3,K("chest","#0a2a8a","#ffffff","#ffffff","#0a2a8a"),K("plain","#ffffff","#0a2a8a","#ffffff","#ffffff"),true),
            };
            COMPS = new[] {
                new Comp{id="amistoso",name="Partido Amistoso",fmt="friendly",desc="Elige dos equipos y juega un partido rápido",col=C("#2ab84a"),col2=C("#ffffff")},
                new Comp{id="premier",name="Premier League",fmt="league",desc="Liga inglesa: 6 equipos, gana el que sume más puntos",col=C("#3d195b"),col2=C("#00ff85")},
                new Comp{id="champions",name="Champions League",fmt="ko",desc="Los mejores de Europa: cuartos, semifinal y final",col=C("#0a1a5a"),col2=C("#c0c8ff")},
                new Comp{id="laliga",name="La Liga",fmt="league",desc="Liga española: 6 equipos, gana el que sume más puntos",col=C("#ee2737"),col2=C("#ffcf00")},
                new Comp{id="supercopa",name="Supercopa",fmt="ko",desc="4 equipos: semifinal y final, ¡a todo o nada!",col=C("#1a4a8a"),col2=C("#e8e8f0")},
                new Comp{id="mundial",name="Mundial",fmt="world",desc="Selecciones: fase de grupos, semifinal y final",col=C("#8a1538"),col2=C("#d4af37")},
            };
            STADIUMS = new[] {
                new Stadium{name="Estadio Clásico",g1=C("#3aa83a"),g2=C("#47b847"),line=Color.White,outside=C("#2a7a2a"),sky0=C("#4aa8ff"),sky1=C("#bfe8ff"),stand=C("#5a5a6a"),wall=C("#1a3a8a"),wall2=C("#ffffff")},
                new Stadium{name="Noche de Estrellas",g1=C("#2f8a3a"),g2=C("#379844"),line=Color.White,outside=C("#163d1e"),sky0=C("#03061a"),sky1=C("#1a2a5a"),stand=C("#1e1e38"),wall=C("#0a1a5a"),wall2=C("#c0c8ff"),night=true},
                new Stadium{name="Estadio Soleado",g1=C("#4ab83a"),g2=C("#58c446"),line=Color.White,outside=C("#3a8a2a"),sky0=C("#5ab8ff"),sky1=C("#e8f8ff"),stand=C("#c84a3a"),wall=C("#ee2737"),wall2=C("#ffcf00")},
                new Stadium{name="Estadio Mundialista",g1=C("#36a03a"),g2=C("#42ae46"),line=Color.White,outside=C("#26702a"),sky0=C("#3a98f0"),sky1=C("#cfeaff"),stand=C("#6a2a4a"),wall=C("#8a1538"),wall2=C("#d4af37")},
                new Stadium{name="Fondo de Bikini",g1=C("#e8d49a"),g2=C("#f0dca6"),line=Color.White,outside=C("#c8b47a"),sky0=C("#0a3a8a"),sky1=C("#3aa8e8"),stand=C("#2a6aa8"),wall=C("#ff8ab0"),wall2=C("#8ae8ff"),sea=true},
                new Stadium{name="Torneo de Artes Marciales",g1=C("#dcd8cc"),g2=C("#d6d2c6"),line=C("#c8402a"),outside=C("#4ab84a"),sky0=C("#4aa8ff"),sky1=C("#bfe8ff"),stand=C("#d8a040"),wall=C("#e8e0d0"),wall2=C("#c8402a"),tiles=true},
                new Stadium{name="Estadio Helado",g1=C("#bfe6f5"),g2=C("#cdeefa"),line=C("#3a7ad8"),outside=C("#9ad0e8"),sky0=C("#7a9ad8"),sky1=C("#e8f4ff"),stand=C("#5a6ab8"),wall=C("#3a7ad8"),wall2=C("#ffffff"),ice=true},
                new Stadium{name="Springfield",g1=C("#5ab83a"),g2=C("#66c446"),line=Color.White,outside=C("#4a9a2a"),sky0=C("#4aa8ff"),sky1=C("#d8f0ff"),stand=C("#e8a020"),wall=C("#ffd83a"),wall2=C("#2a5ad8")},
            };
            SONGS = new Song[] {
                // 0: menú (alegre, como de presentación de partido)
                new Song{bpm=128,mel=new[]{72,0,76,79,0,76,72,0,74,0,77,81,0,77,74,0,76,0,79,84,0,79,76,0,77,76,74,72,0,0,0,0,72,0,76,79,0,76,72,0,74,0,77,81,0,77,74,0,79,0,81,83,84,0,83,81,79,0,77,0,76,0,0,0},
                    bass=new[]{48,0,55,0,48,0,55,0,50,0,57,0,50,0,57,0,52,0,59,0,52,0,59,0,53,0,60,0,55,0,59,0}},
                // 1: partido (con ritmo)
                new Song{bpm=150,mel=new[]{67,0,67,69,71,0,67,0,72,0,71,69,67,0,64,0,65,0,65,67,69,0,65,0,71,0,69,67,65,0,62,0,64,0,64,65,67,0,64,0,72,0,71,69,67,0,72,0,74,0,72,71,69,0,67,0,65,64,62,64,67,0,0,0},
                    bass=new[]{43,0,43,0,50,0,43,0,41,0,41,0,48,0,41,0,40,0,40,0,47,0,40,0,38,0,38,0,45,0,43,0}},
                // 2: final y trofeo (épica)
                new Song{bpm=110,mel=new[]{67,0,72,0,76,0,79,0,0,0,76,0,79,0,84,0,0,0,83,0,81,0,79,0,77,0,76,0,74,0,72,0,67,0,72,0,76,0,79,0,0,0,81,0,83,0,84,0,0,0,88,0,86,0,84,0,83,0,84,0,0,0,0,0},
                    bass=new[]{48,0,0,0,55,0,0,0,53,0,0,0,57,0,0,0,52,0,0,0,55,0,0,0,50,0,0,0,55,0,0,0}},
            };
        }

        // personajes de una caricatura
        public static Ch[] OfSeries(string s) { return CHARS.Where(c => c.series == s).ToArray(); }
        public static Team TeamById(string id) { return CLUBS.Concat(NATIONS).First(t => t.id == id); }
    }
}
