using System;
using System.Collections.Generic;
using System.Drawing;

namespace SuperKart
{
    // ======================= tipos de datos =======================
    public class Ch   // personaje
    {
        public string id, name, kind, letter = "", cls;
        public Color hat, shirt, over, hair, skin, kart, dress, gem, spot, vest, body, spike, shell, hood, fur, face, cap, bow, stache, nose;
        public bool big, thin, flip, pigtails, junior, bones;
        // para los personajes "persona" (los amigos): lentes, estatura (1 = normal) y peinado
        public bool glasses; public double tall = 1; public string hairStyle = "short";
        public Ch Clone() { return (Ch)MemberwiseClone(); }
    }
    public class Kt { public string id, name, shape; public int[] d; }   // vehículo: d = {vel, acel, manejo, peso}
    public class St { public int spd, acc, han, wgt; }                   // estadísticas finales
    public class Theme
    {
        public Color[] sky, grass, rumble, road;
        public Color lane, far, near;
        public string[] decor;
        public string bg;
        public bool danger, lava, rainbow, slip, planks;
    }
    public class Track { public string name, theme; public int song; public double f; public int[] pieces; }
    public class Cup { public string name, icon; public int[] tracks; }
    public class Song { public double bpm; public int[] mel, bass; }
    public class CCDef { public string n; public double mul, ai; }
    public class Diff { public string n, d; public double speed, catchup, slow, boost, start, useMin, useRand, pw; public bool noMean; }

    static partial class SK
    {
        static Color C(string h) { return U.C(h); }

        public static Ch[] CHARS;
        public static Kt[] KARTS;
        public static Dictionary<string, int[]> CLS;
        public static Dictionary<string, string> CLS_NAME;
        public static CCDef[] CC;
        public static Diff[] DIFF;
        public static Dictionary<string, Theme> THEMES;
        public static List<Track> TRACKS;
        public static Cup[] CUPS;
        public static Song[] SONGS;
        public static string[] ITEM_KEYS;
        public static Dictionary<string, string> ITEM_NAMES, ITEM_DESC;
        public static Dictionary<string, int>[] ITEM_TABLE;
        public static Dictionary<string, int> ITEM_COUNT;
        public static Dictionary<string, string> HAZARD;
        public static Dictionary<string, string[]> POWERS;
        public static Dictionary<string, double> PW_DUR;
        public static Dictionary<string, Color> PW_ICON;
        public static string[] PowerOf(Ch ch) { string[] p; return POWERS.TryGetValue(ch.id, out p) ? p : POWERS["toad"]; }
        public static Color[] P_COL;
        public static int[] PTS = { 15, 12, 10, 8, 6, 4, 2, 1 };

        public static void InitData()
        {
            CHARS = new Ch[] {
                new Ch{id="mario",name="Mario",kind="plumber",letter="M",hat=C("#e52521"),shirt=C("#e52521"),over=C("#2050d0"),hair=C("#5a2d0c"),skin=C("#ffcc99"),kart=C("#e52521"),cls="medio"},
                new Ch{id="luigi",name="Luigi",kind="plumber",letter="L",hat=C("#2fb44a"),shirt=C("#2fb44a"),over=C("#2050d0"),hair=C("#4a2508"),skin=C("#ffcc99"),kart=C("#2fb44a"),cls="medio",thin=true},
                new Ch{id="peach",name="Peach",kind="princess",hair=C("#ffd84a"),dress=C("#ff8fc8"),gem=C("#3a8cff"),skin=C("#ffe0c4"),kart=C("#ff8fc8"),cls="ligero"},
                new Ch{id="daisy",name="Daisy",kind="princess",hair=C("#c86a1e"),dress=C("#ffb52a"),gem=C("#2fb44a"),skin=C("#ffe0c4"),kart=C("#ffb52a"),cls="medio"},
                new Ch{id="toad",name="Toad",kind="toad",spot=C("#e52521"),vest=C("#2050d0"),skin=C("#ffe0c4"),kart=C("#3a7bff"),cls="ligero"},
                new Ch{id="yoshi",name="Yoshi",kind="yoshi",body=C("#4cc84c"),spike=C("#e52521"),kart=C("#4cc84c"),cls="medio"},
                new Ch{id="koopa",name="Koopa",kind="koopa",body=C("#f5d23c"),shell=C("#2fa84a"),kart=C("#2fa84a"),cls="ligero"},
                new Ch{id="shyguy",name="Shy Guy",kind="shyguy",hood=C("#d42020"),kart=C("#d42020"),cls="ligero"},
                new Ch{id="dk",name="Donkey Kong",kind="dk",fur=C("#7a4a1e"),face=C("#e8b88a"),kart=C("#8a5a2a"),cls="pesado"},
                new Ch{id="wario",name="Wario",kind="plumber",letter="W",hat=C("#f5d000"),shirt=C("#f5d000"),over=C("#7a2fb0"),hair=C("#5a2d0c"),skin=C("#ffc48a"),kart=C("#e8c000"),cls="pesado",big=true,nose=C("#ff9a8a")},
                new Ch{id="waluigi",name="Waluigi",kind="plumber",letter="L",flip=true,hat=C("#6a2fa8"),shirt=C("#6a2fa8"),over=C("#222222"),hair=C("#3a1f08"),skin=C("#ffc48a"),kart=C("#6a2fa8"),cls="pesado",thin=true,nose=C("#ffaa88")},
                new Ch{id="bowser",name="Bowser",kind="bowser",body=C("#e8c040"),shell=C("#2f8a3a"),hair=C("#e8541e"),kart=C("#2f8a3a"),cls="pesado"},
                new Ch{id="rosalina",name="Rosalina",kind="princess",hair=C("#f2e6b0"),dress=C("#6ad8e0"),gem=C("#4ae0ff"),skin=C("#fff0e4"),kart=C("#4ac8d8"),cls="pesado"},
                new Ch{id="toadette",name="Toadette",kind="toad",cap=C("#ff6aa8"),spot=C("#ffffff"),vest=C("#ff8ac0"),skin=C("#ffe0c4"),kart=C("#ff6aa8"),cls="ligero",pigtails=true},
                new Ch{id="bowserjr",name="Bowser Jr.",kind="bowser",junior=true,body=C("#e8c040"),shell=C("#2f8a3a"),hair=C("#e8541e"),kart=C("#e8a020"),cls="ligero"},
                new Ch{id="birdo",name="Birdo",kind="birdo",body=C("#ff7ab0"),bow=C("#e52521"),kart=C("#ff7ab0"),cls="medio"},
                new Ch{id="kingboo",name="Rey Boo",kind="boo",body=C("#f4f4ff"),kart=C("#b8b8e0"),cls="pesado"},
                new Ch{id="drybones",name="Huesitos",kind="koopa",bones=true,body=C("#e8e4d8"),shell=C("#b8b0a0"),kart=C("#8a8a9a"),cls="ligero"},
                new Ch{id="diddy",name="Diddy Kong",kind="dk",fur=C("#8a5226"),face=C("#f0c8a0"),cap=C("#e52521"),kart=C("#e52521"),cls="ligero"},
                new Ch{id="metal",name="Mario Metal",kind="plumber",letter="M",hat=C("#8a929e"),shirt=C("#9aa2ae"),over=C("#6a727e"),hair=C("#4a525e"),skin=C("#c4ccd6"),stache=C("#4a525e"),nose=C("#aab2bc"),kart=C("#8a929e"),cls="pesado"},
                // ---- los amigos ----
                new Ch{id="flaco",name="Flaco",kind="person",skin=C("#8a5a3a"),hair=C("#1a1210"),shirt=C("#ff8a2a"),kart=C("#ff8a2a"),glasses=true,tall=1.15,thin=true,hairStyle="short",cls="ligero"},
                new Ch{id="willito",name="Willito",kind="person",skin=C("#ffd8c0"),hair=C("#141414"),shirt=C("#2a6ae8"),kart=C("#2a6ae8"),tall=1.15,hairStyle="tuft",cls="pesado"},
                new Ch{id="chevez",name="Chevez",kind="person",skin=C("#e0b088"),hair=C("#4a2a14"),shirt=C("#2fb44a"),kart=C("#2fb44a"),glasses=true,tall=0.85,hairStyle="messy",cls="ligero"},
                new Ch{id="yoni",name="Yoni",kind="person",skin=C("#ffe0cc"),hair=C("#8a5a2a"),shirt=C("#1ab8a8"),kart=C("#1ab8a8"),tall=1.15,thin=true,hairStyle="short",cls="medio"},
                // ---- los guerreros Z ----
                new Ch{id="goku",name="Goku",kind="saiyan",hairStyle="goku",skin=C("#ffd0a8"),hair=C("#151515"),shirt=C("#ff7a1a"),over=C("#ff7a1a"),vest=C("#1a4ad8"),kart=C("#ff7a1a"),cls="medio"},
                new Ch{id="vegeta",name="Vegeta",kind="saiyan",hairStyle="vegeta",skin=C("#ffd8b8"),hair=C("#151515"),shirt=C("#1a3aa8"),over=C("#1a3aa8"),vest=C("#f4f4f4"),kart=C("#1a3aa8"),cls="pesado"},
                new Ch{id="gohan",name="Gohan",kind="saiyan",hairStyle="gohan",skin=C("#ffd8b8"),hair=C("#151515"),shirt=C("#5a2aa8"),over=C("#5a2aa8"),vest=C("#e52521"),kart=C("#5a2aa8"),tall=0.9,cls="ligero"},
                new Ch{id="piccolo",name="Piccolo",kind="saiyan",hairStyle="piccolo",skin=C("#5ac85a"),hair=C("#f4f4f4"),shirt=C("#5a2aa8"),over=C("#5a2aa8"),vest=C("#f0f0f8"),kart=C("#3a9a3a"),tall=1.15,cls="pesado"},
                new Ch{id="krilin",name="Krilin",kind="saiyan",hairStyle="krilin",skin=C("#ffd0a8"),hair=C("#151515"),shirt=C("#ff7a1a"),over=C("#ff7a1a"),vest=C("#1a4ad8"),kart=C("#ffb02a"),tall=0.85,cls="ligero"},
                new Ch{id="freezer",name="Freezer",kind="saiyan",hairStyle="freezer",skin=C("#f4f0f8"),hair=C("#8a3ab8"),shirt=C("#f4f0f8"),over=C("#f4f0f8"),vest=C("#8a3ab8"),kart=C("#8a3ab8"),tall=0.9,cls="medio"},
                new Ch{id="goten",name="Goten",kind="saiyan",hairStyle="goten",skin=C("#ffd0a8"),hair=C("#151515"),shirt=C("#ff7a1a"),over=C("#1a4ad8"),vest=C("#1a4ad8"),kart=C("#2ab8e8"),tall=0.75,cls="ligero"},
                new Ch{id="trunks",name="Trunks",kind="saiyan",hairStyle="trunks",skin=C("#ffd8b8"),hair=C("#b8a0e8"),shirt=C("#2a4ab8"),over=C("#5a5a6a"),vest=C("#222222"),kart=C("#2a4ab8"),cls="medio"},
                new Ch{id="cell",name="Cell",kind="saiyan",hairStyle="cell",skin=C("#e8dccc"),hair=C("#4ab84a"),shirt=C("#4ab84a"),over=C("#4ab84a"),vest=C("#1a1a1a"),kart=C("#2a8a3a"),tall=1.15,cls="pesado"},
                new Ch{id="buu",name="Majin Buu",kind="saiyan",hairStyle="buu",skin=C("#ff9ac8"),hair=C("#ff9ac8"),shirt=C("#6a2a9a"),over=C("#f4f4f4"),vest=C("#ffd23a"),kart=C("#ff6ab0"),big=true,cls="pesado"},
                new Ch{id="bills",name="Bills",kind="saiyan",hairStyle="bills",skin=C("#8a6ac8"),hair=C("#8a6ac8"),shirt=C("#222244"),over=C("#f4f4f4"),vest=C("#ffd23a"),kart=C("#6a4ab8"),thin=true,cls="medio"},
                new Ch{id="a18",name="Androide 18",kind="saiyan",hairStyle="a18",skin=C("#ffe0cc"),hair=C("#ffe08a"),shirt=C("#2a6ae8"),over=C("#2a3a6a"),vest=C("#222222"),kart=C("#2a6ae8"),cls="ligero"},
                new Ch{id="broly",name="Broly",kind="saiyan",hairStyle="broly",skin=C("#e8b088"),hair=C("#151515"),shirt=C("#e8b088"),over=C("#f4f4f4"),vest=C("#ffd23a"),kart=C("#3ab83a"),big=true,tall=1.15,cls="pesado"},
                new Ch{id="roshi",name="Maestro Roshi",kind="saiyan",hairStyle="roshi",skin=C("#ffd8b8"),hair=C("#f4f4f4"),shirt=C("#ff9a3a"),over=C("#ff9a3a"),vest=C("#6a8a3a"),kart=C("#ff9a3a"),tall=0.85,cls="ligero"},
                new Ch{id="bulma",name="Bulma",kind="saiyan",hairStyle="bulma",skin=C("#ffe0cc"),hair=C("#3ac8c8"),shirt=C("#ff6ab0"),over=C("#ff6ab0"),vest=C("#ffffff"),kart=C("#ff6ab0"),cls="ligero"},
                new Ch{id="jiren",name="Jiren",kind="saiyan",hairStyle="jiren",skin=C("#b8b8c8"),hair=C("#b8b8c8"),shirt=C("#d83a3a"),over=C("#1a1a1a"),vest=C("#1a1a1a"),kart=C("#d83a3a"),big=true,cls="pesado"},
            };
            CLS = new Dictionary<string, int[]> { { "ligero", new[] { 2, 5, 5, 1 } }, { "medio", new[] { 3, 3, 3, 3 } }, { "pesado", new[] { 5, 2, 2, 5 } } };
            CLS_NAME = new Dictionary<string, string> { { "ligero", "Peso ligero" }, { "medio", "Peso medio" }, { "pesado", "Peso pesado" } };

            KARTS = new Kt[] {
                new Kt{id="std",name="Kart Estándar",shape="kart",d=new[]{0,0,0,0}},
                new Kt{id="sport",name="Bólido",shape="sport",d=new[]{1,-1,0,0}},
                new Kt{id="mini",name="Mini Turbo",shape="mini",d=new[]{-1,2,1,-1}},
                new Kt{id="buggy",name="Todoterreno",shape="buggy",d=new[]{0,1,-1,1}},
                new Kt{id="bike",name="Moto Estándar",shape="bike",d=new[]{-1,1,2,-1}},
                new Kt{id="heavy",name="Tanque Bowser",shape="heavy",d=new[]{2,-2,-1,2}},
                new Kt{id="pipe",name="Tubería",shape="pipe",d=new[]{0,1,1,-1}},
                new Kt{id="rocket",name="Cohete",shape="rocket",d=new[]{2,-1,-2,1}},
            };

            CC = new CCDef[] {
                new CCDef{n="50cc",mul=0.82,ai=0.85}, new CCDef{n="100cc",mul=0.91,ai=0.91},
                new CCDef{n="150cc",mul=1,ai=0.96}, new CCDef{n="200cc",mul=1.13,ai=0.985},
            };
            // dificultad de los rivales: velocidad, cuánto te alcanzan, frecuencia de objetos y turbos
            DIFF = new Diff[] {
                new Diff{n="FÁCIL",d="Rivales tranquilos: ideal para ganar y divertirse",speed=0.86,catchup=0.04,slow=0.80,boost=0.08,start=0.15,useMin=2.5,useRand=4,noMean=true,pw=0.5},
                new Diff{n="NORMAL",d="Una carrera pareja",speed=0.95,catchup=0.08,slow=0.86,boost=0.18,start=0.3,useMin=1,useRand=3,noMean=false,pw=0.8},
                new Diff{n="DIFÍCIL",d="¡Los rivales no perdonan!",speed=1.0,catchup=0.12,slow=0.9,boost=0.25,start=0.4,useMin=0.5,useRand=2,noMean=false,pw=1},
            };

            ITEM_KEYS = new[] { "banana", "triple_banana", "green", "triple_green", "red", "blue", "mushroom", "triple", "golden", "star", "lightning", "bullet", "coin", "bomb", "blooper", "boo", "horn" };
            ITEM_NAMES = new Dictionary<string, string> {
                {"banana","Plátano"},{"triple_banana","Triple plátano"},{"green","Caparazón verde"},{"triple_green","Triple caparazón verde"},{"red","Caparazón rojo"},{"blue","Caparazón azul"},
                {"mushroom","Champiñón"},{"triple","Triple champiñón"},{"golden","Champiñón dorado"},{"star","Estrella"},{"lightning","Rayo"},{"bullet","Bill Bala"},{"coin","Monedas"},
                {"bomb","Bob-omba"},{"blooper","Blooper"},{"boo","Boo"},{"horn","Súper bocina"} };
            ITEM_DESC = new Dictionary<string, string> {
                {"mushroom","Un turbo corto"},{"triple","Tres turbos"},{"golden","Turbos sin límite por 7 segundos"},{"banana","Déjalo atrás: hace girar"},{"triple_banana","Tres plátanos"},
                {"green","Va recto y rebota en los bordes"},{"triple_green","Tres caparazones verdes"},{"red","Persigue al de adelante"},{"blue","Vuela directo al 1º lugar"},{"bomb","Explota y golpea a todos cerca"},
                {"star","Invencible y más rápido"},{"lightning","Encoge a todos los rivales"},{"bullet","Piloto automático a toda velocidad"},{"blooper","Mancha de tinta a los de adelante"},
                {"boo","Roba un objeto y te vuelve invisible"},{"horn","Destruye objetos cercanos (¡hasta el azul!)"},{"coin","Te da 2 monedas"} };
            ITEM_TABLE = new Dictionary<string, int>[] {
                new Dictionary<string,int>{{"banana",22},{"green",20},{"coin",18},{"red",8},{"mushroom",6},{"triple_banana",10},{"horn",8},{"bomb",4},{"boo",4}},
                new Dictionary<string,int>{{"banana",10},{"green",12},{"red",20},{"mushroom",14},{"coin",6},{"triple",8},{"star",4},{"triple_green",8},{"bomb",8},{"blooper",6},{"boo",4}},
                new Dictionary<string,int>{{"red",14},{"triple",18},{"mushroom",10},{"star",12},{"blue",6},{"golden",10},{"bullet",5},{"lightning",5},{"triple_green",8},{"blooper",6},{"bomb",6}},
                new Dictionary<string,int>{{"star",18},{"bullet",18},{"triple",14},{"lightning",12},{"golden",16},{"blue",6},{"red",8},{"blooper",8}},
            };
            ITEM_COUNT = new Dictionary<string, int> { { "triple", 3 }, { "triple_banana", 3 }, { "triple_green", 3 }, { "golden", 99 } };

            THEMES = new Dictionary<string, Theme>();
            THEMES["grass"] = new Theme { sky = new[] { C("#4aa8ff"), C("#c8ecff") }, grass = new[] { C("#4cc84c"), C("#43b843") }, rumble = new[] { C("#ffffff"), C("#e52521") }, road = new[] { C("#8a8a92"), C("#84848c") }, lane = C("#ffffff"), decor = new[] { "pipe", "tree", "bush", "qblock", "mushroom", "tree" }, far = C("#7ab0e0"), near = C("#3a9a3a"), bg = "hills" };
            THEMES["beach"] = new Theme { sky = new[] { C("#2fa8ff"), C("#bff0ff") }, grass = new[] { C("#f4dc9a"), C("#ecd38e") }, rumble = new[] { C("#ffffff"), C("#ff8c1a") }, road = new[] { C("#a89a86"), C("#a2947f") }, lane = C("#ffffff"), decor = new[] { "palm", "umbrella", "palm", "rock", "bush" }, far = C("#3a8ad8"), near = C("#2fc0e8"), bg = "ocean" };
            THEMES["desert"] = new Theme { sky = new[] { C("#ff9a4a"), C("#ffe0a0") }, grass = new[] { C("#e8b060"), C("#e0a656") }, rumble = new[] { C("#ffffff"), C("#c84a1e") }, road = new[] { C("#9e8670"), C("#98806a") }, lane = C("#ffe9c0"), decor = new[] { "cactus", "cactus", "rock", "pyramid", "cactus" }, far = C("#d88a4a"), near = C("#c8783a"), bg = "dunes" };
            THEMES["snow"] = new Theme { sky = new[] { C("#7ab0e0"), C("#e8f4ff") }, grass = new[] { C("#f4f8ff"), C("#e2eaf6") }, rumble = new[] { C("#ffffff"), C("#3a6ad8") }, road = new[] { C("#7e8a9a"), C("#788494") }, lane = C("#ffffff"), decor = new[] { "pine", "pine", "snowman", "ice", "pine" }, far = C("#8aa8d0"), near = C("#e0ecf8"), bg = "mountains" };
            THEMES["castle"] = new Theme { sky = new[] { C("#1a0505"), C("#6a1a0a") }, grass = new[] { C("#ff5a00"), C("#e84000") }, rumble = new[] { C("#555555"), C("#888888") }, road = new[] { C("#3a3434"), C("#363030") }, lane = C("#777777"), decor = new[] { "torch", "thwomp", "lavarock", "torch" }, far = C("#2a0a0a"), near = C("#3a1010"), bg = "castle", danger = true, lava = true };
            THEMES["rainbow"] = new Theme { sky = new[] { C("#000010"), C("#1a0a3a") }, grass = new[] { C("#06061c"), C("#06061c") }, rumble = new[] { C("#ffffff"), C("#ffe14a") }, road = null, lane = U.RGBA(255, 255, 255, 0.7), decor = new[] { "star", "star", "ring" }, far = C("#2a1a5a"), near = C("#111111"), bg = "space", danger = true, rainbow = true };
            THEMES["jungle"] = new Theme { sky = new[] { C("#5ab8a0"), C("#d8f0c0") }, grass = new[] { C("#2f8a2f"), C("#287a28") }, rumble = new[] { C("#ffffff"), C("#d89a2a") }, road = new[] { C("#8a7a5a"), C("#847454") }, lane = C("#f0e0b0"), decor = new[] { "jtree", "palm", "bush", "jtree", "barrel" }, far = C("#3a7a5a"), near = C("#1f5a2a"), bg = "jungle" };
            THEMES["ice"] = new Theme { sky = new[] { C("#9ad0ff"), C("#f0f8ff") }, grass = new[] { C("#e8f4ff"), C("#d8eaf8") }, rumble = new[] { C("#ffffff"), C("#7ad0ff") }, road = new[] { C("#b8d8f0"), C("#b0d0ea") }, lane = C("#ffffff"), decor = new[] { "ice", "penguin", "pine", "snowman", "penguin" }, far = C("#a8c8f0"), near = C("#f0f8ff"), bg = "mountains", slip = true };
            THEMES["mine"] = new Theme { sky = new[] { C("#1a120a"), C("#4a3018") }, grass = new[] { C("#4a3420"), C("#42301c") }, rumble = new[] { C("#c89a4a"), C("#6a4a2a") }, road = new[] { C("#7a5a3a"), C("#745436") }, lane = C("#e8c070"), decor = new[] { "beam", "crystal", "lantern", "crystal", "minecart" }, far = C("#2a1a0e"), near = C("#3a2614"), bg = "mine" };
            THEMES["ghost"] = new Theme { sky = new[] { C("#1a0a2a"), C("#4a2a6a") }, grass = new[] { C("#2a2a3a"), C("#262634") }, rumble = new[] { C("#7a4aa0"), C("#3a2a4a") }, road = new[] { C("#6a4a2a"), C("#5a3e20") }, lane = C("#3a2a1a"), decor = new[] { "boo", "deadtree", "grave", "candle", "boo" }, far = C("#2a1a3a"), near = C("#1a1024"), bg = "ghost", planks = true };
            THEMES["city"] = new Theme { sky = new[] { C("#05051a"), C("#2a1a4a") }, grass = new[] { C("#1a1a2a"), C("#161626") }, rumble = new[] { C("#ff3ad8"), C("#3affff") }, road = new[] { C("#2a2a34"), C("#26262f") }, lane = C("#ffe14a"), decor = new[] { "building", "lamp", "building", "neon", "lamp" }, far = C("#1a1a3a"), near = C("#0e0e22"), bg = "city" };
            THEMES["sky"] = new Theme { sky = new[] { C("#3a9aff"), C("#ffe8f8") }, grass = new[] { C("#ffffff"), C("#eef4ff") }, rumble = new[] { C("#ffd800"), C("#ffffff") }, road = new[] { C("#c8b8e8"), C("#c0b0e0") }, lane = C("#ffffff"), decor = new[] { "cloudpuff", "beanstalk", "qblock", "cloudpuff" }, far = C("#ffffff"), near = C("#e8f2ff"), bg = "sky", danger = true };

            // pieza: entrada, mantener, salida, curva, colina (de 5 en 5)
            TRACKS = new List<Track> {
                new Track{name="Circuito Mario",theme="grass",song=0,f=2,pieces=new[]{0,30,0,0,0, 20,40,20,3,0, 10,20,10,0,15, 20,30,20,-2,-15, 20,50,20,4,0, 15,40,15,0,25, 20,30,20,5,-25, 20,40,20,-3,0, 15,30,15,0,0, 20,50,20,3,10, 20,30,20,2,-10}},
                new Track{name="Playa Koopa",theme="beach",song=1,f=2,pieces=new[]{0,40,0,0,0, 30,60,30,2,10, 20,30,20,0,-10, 25,40,25,-3,0, 30,50,30,3,0, 20,40,20,0,20, 25,30,25,4,-20, 20,40,20,-2,0, 25,60,25,2,0}},
                new Track{name="Desierto Seco Seco",theme="desert",song=1,f=2,pieces=new[]{0,40,0,0,0, 25,40,25,4,0, 10,60,10,0,30, 20,30,20,-4,-30, 25,30,25,5,0, 15,70,15,0,0, 20,20,20,-3,15, 20,20,20,3,-15, 25,50,25,4,0}},
                new Track{name="Montaña DK",theme="snow",song=0,f=2,pieces=new[]{0,40,0,0,0, 20,30,20,0,40, 20,30,20,3,-20, 20,30,20,-4,20, 25,40,25,5,-40, 20,40,20,0,30, 20,30,20,-3,-30, 25,40,25,4,0, 20,20,20,3,20, 20,20,20,0,-20}},
                new Track{name="Castillo de Bowser",theme="castle",song=2,f=2,pieces=new[]{0,40,0,0,0, 15,30,15,5,0, 10,30,10,0,0, 15,30,15,-5,0, 15,40,15,5,20, 10,40,10,0,-20, 15,20,15,-4,0, 15,20,15,4,0, 15,50,15,5,0, 10,30,10,-3,10, 10,30,10,3,-10}},
                new Track{name="Senda Arcoíris",theme="rainbow",song=5,f=2.2,pieces=new[]{0,50,0,0,0, 25,50,25,3,30, 20,40,20,-4,-30, 25,30,25,5,0, 20,60,20,0,50, 20,40,20,4,-50, 25,40,25,-3,0, 25,50,25,4,20, 20,30,20,-5,-20, 25,50,25,3,0}},
                new Track{name="Selva DK",theme="jungle",song=1,f=2.5,pieces=new[]{0,40,0,0,0, 20,30,20,4,10, 15,25,15,-3,-10, 20,30,20,5,20, 10,40,10,0,-20, 20,25,20,-4,0, 20,30,20,4,15, 15,30,15,-2,-15, 20,40,20,4,0}},
                new Track{name="Tierra Sorbete",theme="ice",song=0,f=2,pieces=new[]{0,40,0,0,0, 30,50,30,3,0, 20,40,20,0,15, 30,40,30,-3,-15, 30,60,30,3,0, 20,30,20,2,10, 25,40,25,-2,-10, 30,50,30,3,0}},
                new Track{name="Mina de Wario",theme="mine",song=2,f=2.5,pieces=new[]{0,40,0,0,0, 15,30,15,4,30, 15,30,15,-4,-30, 10,40,10,0,25, 15,30,15,5,-25, 15,20,15,-4,0, 15,40,15,4,20, 15,30,15,3,-20, 10,30,10,-3,0, 15,30,15,4,0}},
                new Track{name="Mansión Boo",theme="ghost",song=3,f=2.6,pieces=new[]{0,40,0,0,0, 15,40,15,4,0, 10,30,10,0,0, 15,30,15,-4,0, 15,30,15,4,0, 10,50,10,0,15, 15,30,15,5,-15, 15,30,15,-3,0, 15,40,15,4,0}},
                new Track{name="Ciudad Neón",theme="city",song=4,f=3,pieces=new[]{0,60,0,0,0, 10,25,10,5,0, 0,60,0,0,0, 10,25,10,5,0, 0,40,0,0,10, 10,20,10,-4,-10, 10,25,10,5,0, 0,50,0,0,0, 10,25,10,4,0, 10,20,10,-3,0}},
                new Track{name="Reino Nube",theme="sky",song=5,f=2.3,pieces=new[]{0,50,0,0,0, 25,40,25,3,40, 20,40,20,-3,-20, 25,30,25,4,-20, 20,50,20,0,30, 20,30,20,5,-30, 25,40,25,-3,0, 25,50,25,4,0}},
            };
            CUPS = new Cup[] {
                new Cup{name="Champiñón",icon="mushroom",tracks=new[]{0,1,2}},
                new Cup{name="Flor",icon="flower",tracks=new[]{6,7,8}},
                new Cup{name="Estrella",icon="star",tracks=new[]{3,9,10}},
                new Cup{name="Especial",icon="crown",tracks=new[]{11,4,5}},
            };
            HAZARD = new Dictionary<string, string> { { "grass", "goomba" }, { "jungle", "goomba" }, { "beach", "crab" }, { "city", "car" }, { "castle", "podoboo" }, { "mine", "podoboo" } };
            // súper poder de cada personaje: tipo, nombre, descripción
            POWERS = new Dictionary<string, string[]> {
                {"mario",new[]{"fire","Bolas de fuego","Lanza 3 bolas de fuego"}},{"luigi",new[]{"fly","Súper salto","Vuela por encima de todo"}},
                {"peach",new[]{"shield","Escudo de corazón","Nada te puede golpear"}},{"daisy",new[]{"spin","Tornado de flores","Gira y tumba a los que toques"}},
                {"toad",new[]{"dash","Turbo hongo","Un turbo larguísimo"}},{"yoshi",new[]{"fly","Aleteo de Yoshi","Vuela por encima de todo"}},
                {"koopa",new[]{"spin","Caparazón giratorio","Gira y tumba a los que toques"}},{"shyguy",new[]{"ghost","Modo sigilo","Te vuelves invisible"}},
                {"dk",new[]{"shock","Golpe de suelo","Una onda que tumba a los de cerca"}},{"wario",new[]{"magnet","Imán de monedas","+5 monedas y turbo"}},
                {"waluigi",new[]{"spin","Remolino tramposo","Gira y tumba a los que toques"}},{"bowser",new[]{"fire","Aliento de fuego","Lanza 3 bolas de fuego"}},
                {"rosalina",new[]{"freeze","Polvo de estrellas","Congela a los de adelante"}},{"toadette",new[]{"dash","Súper carrera","Un turbo larguísimo"}},
                {"bowserjr",new[]{"fly","Payaso volador","Vuela por encima de todo"}},{"birdo",new[]{"fire","Lluvia de huevos","Lanza 3 huevos"}},
                {"kingboo",new[]{"ghost","Espectro real","Te vuelves invisible"}},{"drybones",new[]{"shield","Huesos de acero","Nada te puede golpear"}},
                {"diddy",new[]{"fly","Mochila cohete","Vuela por encima de todo"}},{"metal",new[]{"shield","Cuerpo de metal","Nada te puede golpear"}},
                // poderes únicos de los amigos
                {"flaco",new[]{"laser","Láser de lentes","Tumba a los 2 de adelante con un láser"}},{"willito",new[]{"giant","Gigante","Se hace enorme y aplasta a todos"}},
                {"chevez",new[]{"tele","Teletransporte","Desaparece y aparece más adelante"}},{"yoni",new[]{"acro","Piruetas aéreas","Vuela dando piruetas y tumba a todos"}},
                // poderes de los guerreros Z
                {"goku",new[]{"kame","Kamehameha","Un rayo de energía tumba a los 2 de adelante"}},{"vegeta",new[]{"ssj","Súper Saiyajin","Pelo dorado: más rápido y tumba a todos"}},
                {"gohan",new[]{"kame","Masenko","Un rayo de energía tumba a los 2 de adelante"}},{"piccolo",new[]{"laser","Makankosappo","Un rayo en espiral tumba a los 2 de adelante"}},
                {"krilin",new[]{"freeze","Taiyoken","Un destello ciega a los de adelante"}},{"freezer",new[]{"fire","Rayos mortales","Lanza 3 rayos de energía"}},
                {"goten",new[]{"ssj","Súper Saiyajin","Pelo dorado: más rápido y tumba a todos"}},{"trunks",new[]{"spin","Corte de espada","Gira con su espada y tumba a los que toque"}},
                {"cell",new[]{"kame","Kamehameha","Un rayo de energía tumba a los 2 de adelante"}},{"buu",new[]{"freeze","Rayo de chocolate","Convierte en chocolate a los de adelante"}},
                {"bills",new[]{"shock","Destrucción","Una onda destruye a los de cerca"}},{"a18",new[]{"dash","Energía infinita","Un turbo larguísimo"}},
                {"broly",new[]{"ssj","Saiyajin Legendario","Pelo dorado: más rápido y tumba a todos"}},{"roshi",new[]{"kame","Kamehameha","Un rayo de energía tumba a los 2 de adelante"}},
                {"bulma",new[]{"tele","Máquina del tiempo","Desaparece y aparece más adelante"}},{"jiren",new[]{"giant","Poder supremo","Se hace enorme y aplasta a todos"}},
            };
            PW_DUR = new Dictionary<string, double> { { "fly", 3 }, { "spin", 2.5 }, { "shield", 5 }, { "giant", 5 }, { "ssj", 5 }, { "acro", 3.5 } };
            PW_ICON = new Dictionary<string, Color> { {"fire",C("#ff6a1a")},{"kame",C("#4ab8ff")},{"ssj",C("#ffd800")},{"fly",C("#7ad0ff")},{"shield",C("#ff8fc8")},{"spin",C("#7aff7a")},{"dash",C("#ffb02a")},{"ghost",C("#ddddff")},
                {"shock",C("#ffd800")},{"magnet",C("#ffd23a")},{"freeze",C("#9af0ff")},{"laser",C("#ff2a2a")},{"giant",C("#2a6ae8")},{"tele",C("#c86aff")},{"acro",C("#1ab8a8")} };

            SONGS = new Song[] {
                new Song{bpm=160,mel=new[]{72,0,76,0,79,0,76,79,81,0,79,0,76,0,72,0,74,0,77,0,81,0,79,77,76,0,74,0,72,0,0,0,72,0,76,0,79,0,84,0,83,0,81,0,79,0,76,0,77,0,76,0,74,0,71,0,72,0,0,0,0,0,0,0},
                    bass=new[]{48,0,55,0,52,0,55,0,53,0,57,0,53,0,57,0,50,0,57,0,53,0,57,0,48,0,55,0,52,0,55,0,48,0,55,0,52,0,55,0,53,0,57,0,53,0,57,0,55,0,59,0,50,0,55,0,48,0,55,0,48,0,0,0}},
                new Song{bpm=150,mel=new[]{77,0,0,81,0,84,0,81,79,0,0,77,0,74,0,0,76,0,0,79,0,82,0,79,77,0,0,0,0,0,0,0,77,0,0,81,0,84,0,86,84,0,82,0,81,0,79,0,77,0,79,0,81,0,76,0,77,0,0,0,0,0,0,0},
                    bass=new[]{41,0,48,0,41,48,0,48,46,0,53,0,46,53,0,53,48,0,55,0,48,55,0,55,41,0,48,0,41,48,0,48}},
                new Song{bpm=140,mel=new[]{69,0,72,0,71,0,68,0,69,0,76,0,75,0,0,0,74,0,77,0,76,0,72,0,71,0,68,0,64,0,0,0},
                    bass=new[]{45,45,0,45,45,0,44,0,45,45,0,45,45,0,47,0,50,50,0,50,50,0,52,0,40,40,0,40,44,0,47,0}},
                new Song{bpm=120,mel=new[]{62,0,65,0,69,0,68,0,67,0,66,0,65,0,0,0,62,0,65,0,69,0,72,0,71,0,70,0,69,0,0,0},
                    bass=new[]{38,0,0,45,0,0,38,0,37,0,0,44,0,0,37,0,38,0,0,45,0,0,41,0,40,0,0,45,0,0,0,0}},
                new Song{bpm=165,mel=new[]{72,0,72,75,0,77,0,75,72,0,70,0,72,0,0,0,72,0,72,75,0,77,0,79,80,0,79,0,77,0,75,0},
                    bass=new[]{36,0,48,0,36,36,46,0,41,0,53,0,41,41,51,0,36,0,48,0,36,36,46,0,43,0,55,0,43,0,46,0}},
                new Song{bpm=130,mel=new[]{76,80,83,88,83,80,76,80,78,81,85,90,85,81,78,81,73,76,80,85,80,76,73,76,71,75,78,83,78,75,71,75},
                    bass=new[]{40,0,0,0,52,0,0,0,42,0,0,0,54,0,0,0,37,0,0,0,49,0,0,0,35,0,0,0,47,0,0,0}},
            };
            P_COL = new[] { C("#ff4a4a"), C("#4aa8ff") };
        }

        public static St Stats(Ch ch, Kt kt)
        {
            int[] b = CLS[ch.cls];
            return new St
            {
                spd = (int)U.Clamp(b[0] + kt.d[0], 1, 6),
                acc = (int)U.Clamp(b[1] + kt.d[1], 1, 6),
                han = (int)U.Clamp(b[2] + kt.d[2], 1, 6),
                wgt = (int)U.Clamp(b[3] + kt.d[3], 1, 6)
            };
        }
        public static St ClassStats(Ch ch)
        {
            int[] b = CLS[ch.cls];
            return new St { spd = b[0], acc = b[1], han = b[2], wgt = b[3] };
        }
    }
}
