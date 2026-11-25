using Pizzaria.Jogo;
using UnityEngine;

namespace Pizzaria.Audio
{
    /// <summary>
    /// Trilha sonora: uma tarantela em lá menor (6/8) composta e sintetizada por código —
    /// bandolim com trêmolo na melodia, acordeão no "pum-pá-pá", baixo dedilhado e pandeiro.
    /// Sobrevive à troca de cena, abaixa na pausa, some nas telas de fim e acelera nos dias seguintes
    /// e nos últimos 30 segundos do expediente.
    /// </summary>
    public class Musica : MonoBehaviour
    {
        private const int Taxa = 22050;
        private const float Colcheia = 0.15f;          // duração de 1/8 → ~133 bpm na semínima pontuada
        private const string ChaveVolume = "Pizzaria_VolumeMusica";

        private static Musica instancia;
        private static float volume = -1f;
        private static AudioClip clipe;

        private AudioSource fonte;
        private float volumeAtual;
        private float silencioAte;

        public static float Volume
        {
            get { if (volume < 0f) volume = PlayerPrefs.GetFloat(ChaveVolume, 0.6f); return volume; }
            set { volume = Mathf.Clamp01(value); PlayerPrefs.SetFloat(ChaveVolume, volume); }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Resetar() { instancia = null; volume = -1f; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Criar()
        {
            if (instancia != null) return;
            var go = new GameObject("Musica");
            DontDestroyOnLoad(go);
            instancia = go.AddComponent<Musica>();
        }

        /// <summary>Silencia a trilha por alguns segundos (para a fanfarra de fim de dia tocar sozinha).</summary>
        public static void Silenciar(float segundos)
        {
            if (instancia != null) instancia.silencioAte = Time.unscaledTime + segundos;
        }

        private void Start()
        {
            if (clipe == null) clipe = Compor();
            fonte = gameObject.AddComponent<AudioSource>();
            fonte.clip = clipe;
            fonte.loop = true;
            fonte.spatialBlend = 0f;
            fonte.ignoreListenerPause = true; // a pausa só abaixa o volume
            fonte.volume = 0f;
            fonte.Play();
        }

        private void Update()
        {
            if (fonte == null) return;
            var g = GerenciadorJogo.Instancia;
            float alvo = 0.7f, pitch = 1f;
            if (g != null)
            {
                switch (g.Estado)
                {
                    case EstadoJogo.Menu: alvo = 0.75f; break;
                    case EstadoJogo.Jogando: alvo = 0.55f; break;
                    case EstadoJogo.Pausado: alvo = 0.22f; break;
                    default: alvo = 0.35f; break; // telas de fim (depois da fanfarra)
                }
                pitch = 1f + 0.03f * g.IndiceDia;
                if (g.Estado == EstadoJogo.Jogando && !g.Fechado && g.TempoRestante < 30f) pitch += 0.05f;
            }
            if (Time.unscaledTime < silencioAte) alvo = 0f;

            volumeAtual = Mathf.MoveTowards(volumeAtual, alvo, Time.unscaledDeltaTime * (alvo < volumeAtual ? 1.5f : 0.4f));
            fonte.volume = volumeAtual * Volume;
            fonte.pitch = Mathf.MoveTowards(fonte.pitch, pitch, Time.unscaledDeltaTime * 0.05f);
        }

        // ------------------------------------------------------------------ Composição

        // Melodia (MIDI; 0 = segura a nota anterior). 16 compassos de 6 colcheias.
        private static readonly int[] Melodia =
        {
            69, 72, 76, 76, 74, 72,   71, 72, 74, 72, 71, 69,   68, 71, 74, 74, 72, 71,   71, 69, 68, 64, 68, 71,
            69, 72, 76, 81, 79, 77,   77, 76, 74, 77, 76, 74,   72, 71, 69, 68, 71, 74,   72, 71, 68, 69,  0,  0,
            74, 77, 81, 81, 79, 77,   76, 72, 69, 76, 74, 72,   71, 74, 76, 74, 72, 71,   72, 69, 72, 76, 72, 69,
            77, 76, 74, 74, 72, 71,   72, 71, 69, 69, 72, 76,   74, 72, 71, 68, 71, 74,   72, 71, 69, 69,  0,  0,
        };

        // Acorde de cada compasso: (fundamental MIDI do baixo, menor?)
        private static readonly (int raiz, bool menor)[] Acordes =
        {
            (45, true), (45, true), (40, false), (40, false), (45, true), (38, true), (40, false), (45, true),
            (38, true), (45, true), (40, false), (45, true), (38, true), (45, true), (40, false), (45, true),
        };

        private static float Freq(int midi) => 440f * Mathf.Pow(2f, (midi - 69) / 12f);

        private static AudioClip Compor()
        {
            const int compassos = 32; // a melodia toca 2x; na segunda o acordeão dobra a melodia
            float duracao = compassos * 6 * Colcheia;
            int total = Mathf.CeilToInt(duracao * Taxa);
            var buf = new float[total];

            for (int c = 0; c < compassos; c++)
            {
                int ci = c % 16;
                bool segundaVez = c >= 16;
                var (raiz, menor) = Acordes[ci];
                float inicio = c * 6 * Colcheia;

                // Baixo: fundamental no tempo 1, quinta no tempo 2
                Nota(buf, inicio, Freq(raiz), 2.5f * Colcheia, Baixo, 0.42f);
                Nota(buf, inicio + 3 * Colcheia, Freq(raiz + 7), 2.5f * Colcheia, Baixo, 0.36f);

                // Acordeão: "pá-pá" nas colcheias 2,3 e 5,6 (acorde na região média)
                int baseAcorde = raiz + 12;
                while (baseAcorde < 55) baseAcorde += 12;
                int[] triade = { baseAcorde, baseAcorde + (menor ? 3 : 4), baseAcorde + 7 };
                foreach (int k in new[] { 1, 2, 4, 5 })
                    foreach (int n in triade)
                        Nota(buf, inicio + k * Colcheia, Freq(n), 0.6f * Colcheia, Acordeao, 0.07f);

                // Pandeiro: forte nos tempos, leve nas outras colcheias
                for (int k = 0; k < 6; k++)
                    Pandeiro(buf, inicio + k * Colcheia, k % 3 == 0 ? 0.16f : 0.06f, c * 6 + k);

                // Melodia (bandolim). Notas seguradas ganham trêmolo.
                for (int k = 0; k < 6; k++)
                {
                    int nota = Melodia[ci * 6 + k];
                    if (nota == 0) continue;
                    int dur = 1;
                    while (k + dur < 6 && Melodia[ci * 6 + k + dur] == 0) dur++;
                    float t0 = inicio + k * Colcheia;
                    float f = Freq(nota);
                    if (dur == 1) Nota(buf, t0, f, Colcheia * 1.6f, Bandolim, 0.28f);
                    else
                        for (float tt = 0f; tt < dur * Colcheia - 0.02f; tt += 0.05f) // trêmolo
                            Nota(buf, t0 + tt, f, 0.09f, Bandolim, 0.2f);
                    if (segundaVez) Nota(buf, t0, f * 0.5f, dur * Colcheia * 0.9f, Acordeao, 0.09f);
                }
            }

            // Normaliza com folga
            float pico = 0.0001f;
            for (int i = 0; i < total; i++) pico = Mathf.Max(pico, Mathf.Abs(buf[i]));
            float ganho = 0.85f / pico;
            for (int i = 0; i < total; i++) buf[i] *= ganho;

            var clip = AudioClip.Create("Tarantela", total, 1, Taxa, false);
            clip.SetData(buf, 0);
            return clip;
        }

        private delegate float Timbre(float t, float f, float dur);

        private static void Nota(float[] buf, float inicio, float f, float dur, Timbre timbre, float vol)
        {
            int i0 = Mathf.RoundToInt(inicio * Taxa);
            int n = Mathf.CeilToInt((dur + 0.08f) * Taxa);
            for (int j = 0; j < n; j++)
            {
                int i = (i0 + j) % buf.Length; // a cauda da última nota volta no começo: loop sem emenda
                buf[i] += timbre(j / (float)Taxa, f, dur) * vol;
            }
        }

        private static float Bandolim(float t, float f, float dur)
        {
            float s = Sons.Corda(t, f, 9f) + Sons.Corda(t, f * 1.003f, 9f) * 0.8f; // par de cordas levemente desafinado
            return s * Soltura(t, dur);
        }

        private static float Baixo(float t, float f, float dur)
        {
            float w = 2f * Mathf.PI * f * t;
            float s = Mathf.Sin(w) + 0.3f * Mathf.Sin(2f * w) + 0.1f * Mathf.Sin(3f * w);
            return s * Mathf.Min(1f, t / 0.005f) * Mathf.Exp(-t * 4f) * Soltura(t, dur);
        }

        private static float Acordeao(float t, float f, float dur)
        {
            float s = 0f;
            for (int h = 1; h <= 7; h++)
            {
                float w = 2f * Mathf.PI * f * h * t;
                s += (Mathf.Sin(w) + Mathf.Sin(w * 1.004f)) / (h * 0.9f + 0.4f); // duas palhetas = som "musette"
            }
            float env = Mathf.Min(1f, t / 0.015f) * Soltura(t, dur);
            return s * 0.35f * env;
        }

        private static float Soltura(float t, float dur) => t < dur ? 1f : Mathf.Max(0f, 1f - (t - dur) / 0.06f);

        private static void Pandeiro(float[] buf, float inicio, float vol, int semente)
        {
            int i0 = Mathf.RoundToInt(inicio * Taxa);
            int n = (int)(0.08f * Taxa);
            float anterior = 0f;
            for (int j = 0; j < n; j++)
            {
                float r = Sons.Ruido(semente * 7919 + j);
                float agudo = r - anterior; // passa-alta simples: som de platinelas
                anterior = r;
                float t = j / (float)Taxa;
                float tilintar = 0.7f + 0.3f * Mathf.Sin(t * 2f * Mathf.PI * 60f);
                buf[(i0 + j) % buf.Length] += agudo * Mathf.Exp(-t * 45f) * tilintar * vol;
            }
        }
    }
}
