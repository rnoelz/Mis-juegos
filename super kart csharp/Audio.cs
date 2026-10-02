using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace SuperKart
{
    // Sonido sintetizado (igual que en la versión web): un pequeño mezclador que
    // genera música, efectos y el motor, y los manda a la tarjeta de sonido con waveOut.
    static class Snd
    {
        const int SR = 22050, N = 512, NB = 4;
        class Voice
        {
            public string type; public double f, f2, dur, vol, t, phase; public bool noise, music;
            public double b0, b1, b2, a1, a2, x1, x2, y1, y2;   // filtro pasa-banda del ruido
        }
        static readonly List<Voice> voices = new List<Voice>();
        static readonly object lk = new object();
        static Thread thread; static volatile bool running;
        static Random rnd = new Random();
        public static bool enabled;
        static bool _musicOn = true;
        public static bool musicOn { get { return _musicOn; } set { _musicOn = value; } }
        public static double engFreq = 55, engVol = 0;
        static double engF = 55, engV = 0, engPhase = 0, engLP = 0;
        // música
        static Song song; static int step; static double rate = 1, stepCounter;

        // ---------- winmm ----------
        [StructLayout(LayoutKind.Sequential)] struct WAVEFORMATEX { public short wFormatTag, nChannels; public int nSamplesPerSec, nAvgBytesPerSec; public short nBlockAlign, wBitsPerSample, cbSize; }
        [StructLayout(LayoutKind.Sequential)] struct WAVEHDR { public IntPtr lpData; public int dwBufferLength, dwBytesRecorded; public IntPtr dwUser; public int dwFlags, dwLoops; public IntPtr lpNext, reserved; }
        [DllImport("winmm.dll")] static extern int waveOutOpen(out IntPtr hwo, int dev, ref WAVEFORMATEX fmt, IntPtr cb, IntPtr inst, int flags);
        [DllImport("winmm.dll")] static extern int waveOutPrepareHeader(IntPtr hwo, IntPtr hdr, int size);
        [DllImport("winmm.dll")] static extern int waveOutUnprepareHeader(IntPtr hwo, IntPtr hdr, int size);
        [DllImport("winmm.dll")] static extern int waveOutWrite(IntPtr hwo, IntPtr hdr, int size);
        [DllImport("winmm.dll")] static extern int waveOutReset(IntPtr hwo);
        [DllImport("winmm.dll")] static extern int waveOutClose(IntPtr hwo);

        public static void Start()
        {
            if (running) return;
            running = true;
            // si algo falla con la tarjeta de sonido, el juego sigue en silencio
            thread = new Thread(() => { try { Run(); } catch { enabled = false; running = false; } }) { IsBackground = true, Priority = ThreadPriority.AboveNormal };
            thread.Start();
        }
        public static void Stop() { running = false; }

        static void Run()
        {
            IntPtr hwo;
            var fmt = new WAVEFORMATEX { wFormatTag = 1, nChannels = 1, nSamplesPerSec = SR, wBitsPerSample = 16, nBlockAlign = 2, nAvgBytesPerSec = SR * 2 };
            try { if (waveOutOpen(out hwo, -1, ref fmt, IntPtr.Zero, IntPtr.Zero, 0) != 0) { running = false; return; } }
            catch { running = false; return; }
            enabled = true;
            int hsz = Marshal.SizeOf(typeof(WAVEHDR));
            int flagsOff = (int)Marshal.OffsetOf(typeof(WAVEHDR), "dwFlags");
            var data = new IntPtr[NB]; var hdrs = new IntPtr[NB]; var buf = new short[N];
            for (int i = 0; i < NB; i++)
            {
                data[i] = Marshal.AllocHGlobal(N * 2); hdrs[i] = Marshal.AllocHGlobal(hsz);
                var h = new WAVEHDR { lpData = data[i], dwBufferLength = N * 2, dwFlags = 0 };
                Marshal.StructureToPtr(h, hdrs[i], false);
                Fill(buf); Marshal.Copy(buf, 0, data[i], N);
                waveOutPrepareHeader(hwo, hdrs[i], hsz); waveOutWrite(hwo, hdrs[i], hsz);
            }
            int cur = 0;
            while (running)
            {
                int flags = Marshal.ReadInt32(hdrs[cur], flagsOff);
                if ((flags & 1) == 0) { Thread.Sleep(2); continue; }   // todavía sonando
                waveOutUnprepareHeader(hwo, hdrs[cur], hsz);
                Fill(buf); Marshal.Copy(buf, 0, data[cur], N);
                var h = new WAVEHDR { lpData = data[cur], dwBufferLength = N * 2, dwFlags = 0 };
                Marshal.StructureToPtr(h, hdrs[cur], false);
                waveOutPrepareHeader(hwo, hdrs[cur], hsz); waveOutWrite(hwo, hdrs[cur], hsz);
                cur = (cur + 1) % NB;
            }
            waveOutReset(hwo);
            for (int i = 0; i < NB; i++) { waveOutUnprepareHeader(hwo, hdrs[i], hsz); Marshal.FreeHGlobal(data[i]); Marshal.FreeHGlobal(hdrs[i]); }
            waveOutClose(hwo);
        }

        static double midi(int m) { return 440 * Math.Pow(2, (m - 69) / 12.0); }

        // genera N muestras mezclando todas las voces
        static void Fill(short[] buf)
        {
            lock (lk)
            {
                double dt = 1.0 / SR;
                for (int n = 0; n < buf.Length; n++)
                {
                    // secuenciador de música
                    if (song != null)
                    {
                        stepCounter -= 1;
                        if (stepCounter <= 0)
                        {
                            double sd = 60 / (song.bpm * rate) / 2;
                            int i = step; int m = song.mel[i % song.mel.Length], b = song.bass[i % song.bass.Length];
                            if (m != 0) AddTone(midi(m), sd * 0.95, "square", 0.05, 0, 0, true);
                            if (b != 0) AddTone(midi(b), sd * 0.95, "triangle", 0.13, 0, 0, true);
                            if (i % 4 == 0) AddTone(150, 0.12, "sine", 0.25, 45, 0, true);
                            if (i % 4 == 2) AddNoise(0.05, 0.06, 0, 6000, true);
                            step++; stepCounter += sd * SR;
                        }
                    }
                    double sMain = 0, sMus = 0;
                    for (int v = voices.Count - 1; v >= 0; v--)
                    {
                        var vo = voices[v];
                        vo.t += dt;
                        if (vo.t < 0) continue;
                        if (vo.t >= vo.dur) { voices.RemoveAt(v); continue; }
                        double p = vo.t / vo.dur;
                        double env = vo.vol * Math.Pow(0.0008 / vo.vol, p);
                        double s;
                        if (vo.noise)
                        {
                            double x0 = rnd.NextDouble() * 2 - 1;
                            s = vo.b0 * x0 + vo.b1 * vo.x1 + vo.b2 * vo.x2 - vo.a1 * vo.y1 - vo.a2 * vo.y2;
                            vo.x2 = vo.x1; vo.x1 = x0; vo.y2 = vo.y1; vo.y1 = s;
                        }
                        else
                        {
                            double f = vo.f2 > 0 ? vo.f * Math.Pow(vo.f2 / vo.f, p) : vo.f;
                            vo.phase += f * dt; double fr = vo.phase - Math.Floor(vo.phase);
                            switch (vo.type)
                            {
                                case "square": s = fr < 0.5 ? 1 : -1; break;
                                case "triangle": s = 4 * Math.Abs(fr - 0.5) - 1; break;
                                case "sawtooth": s = 2 * fr - 1; break;
                                default: s = Math.Sin(2 * Math.PI * vo.phase); break;
                            }
                        }
                        if (vo.music) sMus += s * env; else sMain += s * env;
                    }
                    // motor (diente de sierra con filtro pasa-bajos)
                    engF += (engFreq - engF) * 0.002; engV += (engVol - engV) * 0.002;
                    engPhase += engF * dt; double saw = 2 * (engPhase - Math.Floor(engPhase)) - 1;
                    engLP += (saw - engLP) * 0.13;
                    double outv = (sMain + engLP * engV + (_musicOn ? sMus * 0.5 : 0)) * 0.45;
                    if (outv > 1) outv = 1; if (outv < -1) outv = -1;
                    buf[n] = (short)(outv * 30000);
                }
            }
        }

        static void AddTone(double f, double dur, string type, double vol, double slide, double delay, bool music)
        {
            voices.Add(new Voice { type = type, f = f, f2 = slide, dur = dur, vol = Math.Max(0.001, vol), t = -delay, music = music });
        }
        static void AddNoise(double dur, double vol, double delay, double freq, bool music)
        {
            double w0 = 2 * Math.PI * Math.Min(freq, SR * 0.45) / SR, alpha = Math.Sin(w0) / 2, a0 = 1 + alpha;
            voices.Add(new Voice
            {
                noise = true, dur = dur, vol = Math.Max(0.001, vol), t = -delay, music = music,
                b0 = alpha / a0, b1 = 0, b2 = -alpha / a0, a1 = -2 * Math.Cos(w0) / a0, a2 = (1 - alpha) / a0
            });
        }
        public static void Tone(double f, double dur, string type = "square", double vol = 0.15, double slide = 0, double delay = 0)
        {
            if (!enabled) return;
            lock (lk) { if (voices.Count < 120) AddTone(f, dur, type, vol, slide, delay, false); }
        }
        public static void Noise(double dur, double vol = 0.2, double delay = 0, double freq = 1200)
        {
            if (!enabled) return;
            lock (lk) { if (voices.Count < 120) AddNoise(dur, vol, delay, freq, false); }
        }
        public static void PlaySong(int i, double r)
        {
            lock (lk) { song = SK.SONGS[i]; step = 0; rate = r; stepCounter = 0; voices.RemoveAll(v => v.music); }
        }
        public static void SetMusicRate(double r) { lock (lk) { rate = r; } }
    }

    // Efectos de sonido (los mismos de la versión web)
    static class SFX
    {
        static void T(double f, double d, string ty, double v, double sl = 0, double dl = 0) { Snd.Tone(f, d, ty, v, sl, dl); }
        public static void coin() { T(988, 0.07, "square", 0.1); T(1319, 0.22, "square", 0.1, 0, 0.07); }
        public static void box() { T(520, 0.08, "square", 0.08); T(780, 0.08, "square", 0.08, 0, 0.06); T(1040, 0.12, "square", 0.08, 0, 0.12); }
        public static void itemGet() { T(1200, 0.1, "triangle", 0.15); T(1600, 0.15, "triangle", 0.15, 0, 0.08); }
        public static void boost() { T(180, 0.5, "sawtooth", 0.1, 700); Snd.Noise(0.4, 0.12, 0, 800); }
        public static void hop() { T(320, 0.08, "square", 0.06, 520); }
        public static void spark(int l) { T(new double[] { 0, 600, 800, 1000 }[l], 0.08, "square", 0.07); }
        public static void hit() { Snd.Noise(0.35, 0.3, 0, 600); T(500, 0.5, "square", 0.1, 90); }
        public static void bump() { Snd.Noise(0.12, 0.2, 0, 300); }
        public static void throwIt() { T(700, 0.12, "triangle", 0.12, 300); }
        public static void boom() { Snd.Noise(0.8, 0.4, 0, 200); T(120, 0.6, "sawtooth", 0.18, 40); }
        public static void thunder() { Snd.Noise(1, 0.45, 0, 150); }
        public static void count() { T(440, 0.3, "square", 0.13); }
        public static void go() { T(880, 0.7, "square", 0.13); }
        public static void lap() { double[] f = { 660, 880, 990 }; for (int i = 0; i < f.Length; i++) T(f[i], 0.12, "square", 0.09, 0, i * 0.1); }
        public static void last() { double[] f = { 523, 659, 784, 1047, 784, 1047 }; for (int i = 0; i < f.Length; i++) T(f[i], 0.13, "square", 0.1, 0, i * 0.11); }
        public static void finish() { double[] f = { 523, 659, 784, 1047, 1319, 1568 }; for (int i = 0; i < f.Length; i++) T(f[i], 0.2, "square", 0.1, 0, i * 0.12); T(2093, 0.8, "square", 0.09, 0, 0.75); }
        public static void sel() { T(660, 0.05, "square", 0.07); }
        public static void ok() { T(880, 0.07, "square", 0.09); T(1320, 0.1, "square", 0.09, 0, 0.06); }
        public static void back() { T(440, 0.08, "square", 0.07, 300); }
        public static void fall() { T(800, 0.9, "triangle", 0.15, 100); }
        public static void power() { double[] f = { 523, 659, 784, 1047, 1319, 1568 }; for (int i = 0; i < f.Length; i++) T(f[i], 0.09, "sawtooth", 0.08, 0, i * 0.05); T(1568, 0.5, "triangle", 0.12, 2093, 0.3); }
        public static void star() { double[] f = { 784, 988, 1175, 1568 }; for (int i = 0; i < f.Length; i++) T(f[i], 0.1, "square", 0.09, 0, i * 0.07); }
    }
}
