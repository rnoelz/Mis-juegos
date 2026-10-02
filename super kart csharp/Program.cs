using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SuperKart
{
    // Ventana del juego: bucle de ~60 cuadros por segundo, teclado y dibujo escalado
    class GameForm : Form
    {
        [DllImport("user32.dll")] static extern short GetAsyncKeyState(int vKey);
        static readonly string[] keyNames = { "ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight", "KeyW", "KeyA", "KeyS", "KeyD", "KeyE", "KeyQ", "KeyX", "KeyI", "KeyH", "KeyM", "KeyP", "KeyC", "KeyR", "KeyL",
            "Space", "Enter", "Escape", "Backspace", "ShiftLeft", "ShiftRight", "ControlRight", "Numpad0", "F11" };
        static readonly int[] keyCodes = { 0x26, 0x28, 0x25, 0x27, 0x57, 0x41, 0x53, 0x44, 0x45, 0x51, 0x58, 0x49, 0x48, 0x4D, 0x50, 0x43, 0x52, 0x4C,
            0x20, 0x0D, 0x1B, 0x08, 0xA0, 0xA1, 0xA3, 0x60, 0x7A };

        Timer timer = new Timer();
        Stopwatch sw = Stopwatch.StartNew();
        double last;
        bool fullscreen;

        public GameForm()
        {
            Text = "Super Kart Bros";
            ClientSize = new Size(1280, 720);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.Black;
            KeyPreview = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SK.SCALE = Math.Min(ClientSize.Width / 960.0, ClientSize.Height / 540.0);
            SK.Init();
            timer.Interval = 15;
            timer.Tick += (s, e) => Tick();
            timer.Start();
            last = sw.Elapsed.TotalSeconds;
            // que las flechas y el espacio no hagan nada raro en la ventana
            KeyDown += (s, e) => { e.Handled = true; e.SuppressKeyPress = true; };
        }

        protected override bool IsInputKey(Keys keyData) { return true; }

        void Tick()
        {
            double now = sw.Elapsed.TotalSeconds; double dt = Math.Min(0.05, now - last); last = now;
            if (Form.ActiveForm == this)
            {
                for (int i = 0; i < keyNames.Length; i++)
                {
                    bool down = (GetAsyncKeyState(keyCodes[i]) & 0x8000) != 0;
                    string k = keyNames[i];
                    if (down && !SK.keys.Contains(k)) { SK.keys.Add(k); SK.pressed.Add(k); }
                    else if (!down) SK.keys.Remove(k);
                }
            }
            else SK.keys.Clear();
            SK.Frame(dt);
            if (SK.wantFullscreenToggle) { SK.wantFullscreenToggle = false; ToggleFullscreen(); }
            Invalidate();
        }

        void ToggleFullscreen()
        {
            fullscreen = !fullscreen;
            if (fullscreen) { FormBorderStyle = FormBorderStyle.None; WindowState = FormWindowState.Normal; WindowState = FormWindowState.Maximized; }
            else { FormBorderStyle = FormBorderStyle.Sizable; WindowState = FormWindowState.Normal; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);
            double s = Math.Min(ClientSize.Width / 960.0, ClientSize.Height / 540.0);
            if (s <= 0) return;
            double ox = (ClientSize.Width - 960 * s) / 2, oy = (ClientSize.Height - 540 * s) / 2;
            SK.RenderTo(g, s, ox, oy);
        }

        protected override void OnFormClosed(FormClosedEventArgs e) { Snd.Stop(); base.OnFormClosed(e); }
    }

    static partial class SK
    {
        // dibuja un cuadro completo en cualquier Graphics (ventana o imagen de prueba)
        public static void RenderTo(Graphics gr, double s, double ox, double oy)
        {
            gr.SmoothingMode = SmoothingMode.AntiAlias;
            gr.InterpolationMode = InterpolationMode.Bilinear;
            gr.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            gr.CompositingQuality = CompositingQuality.HighSpeed;
            gr.TranslateTransform((float)ox, (float)oy);
            gr.ScaleTransform((float)s, (float)s);
            gr.SetClip(new RectangleF(0, 0, W, H));
            SCALE = s; g = gr; ALPHA = 1;
            Draw();
            gr.ResetTransform(); gr.ResetClip();
        }
    }

    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length >= 1 && args[0] == "--test")
            {
                SK.RunTests(args.Length >= 2 ? args[1] : "test_out");
                return;
            }
            Application.EnableVisualStyles();
            Snd.Start();
            Application.Run(new GameForm());
        }
    }
}
