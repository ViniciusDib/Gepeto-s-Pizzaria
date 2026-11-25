using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pizzaria.Audio
{
    /// <summary>
    /// Efeitos sonoros sintetizados por código (nenhum arquivo de áudio): campainhas, caixa registradora,
    /// passos, forno, fogo crepitando, burburinho do salão, fanfarras de fim de dia/vitória/derrota...
    /// Volume ajustável nas opções (salvo em PlayerPrefs).
    /// </summary>
    public static class Sons
    {
        private const int Taxa = 22050;
        private const string ChaveVolume = "Pizzaria_VolumeEfeitos";

        private static AudioClip apito, queimou, dinheiro, erro, pop, pegar, colocar, campainha, novaComanda,
                                 forno, passo, lixo, clique, fimDoDia, vitoria, derrota, fogo, salao;
        private static AudioSource fonte2D;
        private static float volume = -1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Resetar() { fonte2D = null; volume = -1f; }

        /// <summary>Volume geral dos efeitos (0..1).</summary>
        public static float Volume
        {
            get { if (volume < 0f) volume = PlayerPrefs.GetFloat(ChaveVolume, 0.8f); return volume; }
            set { volume = Mathf.Clamp01(value); PlayerPrefs.SetFloat(ChaveVolume, volume); }
        }

        // ------------------------------------------------------------------ Catálogo

        public static AudioClip Apito => apito != null ? apito : apito = Notas("Apito", 0.35f, Onda.Seno,
            (1568f, 0.10f), (0f, 0.05f), (1568f, 0.10f), (0f, 0.05f), (2093f, 0.28f));

        public static AudioClip Queimou => queimou != null ? queimou : queimou = Notas("Queimou", 0.25f, Onda.Aspera,
            (220f, 0.25f), (0f, 0.08f), (185f, 0.45f));

        /// <summary>Caixa registradora: "ka" + "ching" com moedinhas.</summary>
        public static AudioClip Dinheiro => dinheiro != null ? dinheiro : dinheiro = Sintetizar("Dinheiro", 0.75f, (t, i) =>
        {
            float s = Ruido(i) * Env(t, 0.001f, 0.03f) * 0.5f;                       // "ka" mecânico
            float t2 = t - 0.07f;
            if (t2 > 0f) s += Sino(t2, 2637f, 5f) * 0.5f + Sino(t2, 3520f, 6f) * 0.35f; // "ching"
            for (int k = 0; k < 4; k++) { float tk = t - 0.15f - k * 0.07f; if (tk > 0f) s += Sino(tk, 4200f + k * 380f, 25f) * 0.2f; }
            return s * 0.55f;
        });

        public static AudioClip Erro => erro != null ? erro : erro = Notas("Erro", 0.22f, Onda.Aspera, (330f, 0.15f), (247f, 0.3f));

        public static AudioClip Pop => pop != null ? pop : pop = Sintetizar("Pop", 0.12f, (t, i) =>
            Mathf.Sin(2f * Mathf.PI * (500f + 900f * Mathf.Exp(-t * 60f)) * t) * Env(t, 0.002f, 0.05f) * 0.5f);

        /// <summary>Pegar ingrediente: "plic" curtinho e agudo.</summary>
        public static AudioClip Pegar => pegar != null ? pegar : pegar = Sintetizar("Pegar", 0.1f, (t, i) =>
            Mathf.Sin(2f * Mathf.PI * (900f + 700f * t * 10f) * t) * Env(t, 0.001f, 0.035f) * 0.35f);

        /// <summary>Colocar ingrediente na pizza: "chlup" macio (ruído grave + batidinha).</summary>
        public static AudioClip Colocar => colocar != null ? colocar : colocar = Sintetizar("Colocar", 0.18f, (t, i) =>
            (Grave(i, 0.08f) * 1.6f + Mathf.Sin(2f * Mathf.PI * 140f * t) * 0.6f) * Env(t, 0.003f, 0.06f) * 0.5f);

        /// <summary>Sininho do balcão (pizza pronta para o garçom).</summary>
        public static AudioClip Campainha => campainha != null ? campainha : campainha = Sintetizar("Campainha", 1.2f, (t, i) =>
            (Sino(t, 1760f, 3f) + Sino(t, 1760f * 2.76f, 5f) * 0.4f + Sino(t, 1760f * 5.4f, 9f) * 0.15f) * 0.4f);

        /// <summary>Nova comanda: "ding-dong".</summary>
        public static AudioClip NovaComanda => novaComanda != null ? novaComanda : novaComanda = Sintetizar("NovaComanda", 1.1f, (t, i) =>
        {
            float s = Sino(t, 1318.5f, 4f) + Sino(t, 2637f, 6f) * 0.3f;
            float t2 = t - 0.28f;
            if (t2 > 0f) s += Sino(t2, 1046.5f, 3.5f) + Sino(t2, 2093f, 6f) * 0.3f;
            return s * 0.32f;
        });

        /// <summary>Pá deslizando na pedra do forno + "fuuu" do fogo.</summary>
        public static AudioClip Forno => forno != null ? forno : forno = Sintetizar("Forno", 0.6f, (t, i) =>
        {
            float raspar = Ruido(i) * Env(t, 0.01f, 0.18f) * 0.25f;
            float fogo = Grave(i, 0.03f) * Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / 0.6f)) * 1.8f;
            return (raspar + fogo) * 0.6f;
        });

        public static AudioClip Passo => passo != null ? passo : passo = Sintetizar("Passo", 0.12f, (t, i) =>
            (Grave(i, 0.15f) * 2f + Ruido(i) * 0.15f) * Env(t, 0.002f, 0.04f) * 0.45f);

        /// <summary>Lixeira: tampa batendo + coisas caindo.</summary>
        public static AudioClip Lixo => lixo != null ? lixo : lixo = Sintetizar("Lixo", 0.45f, (t, i) =>
        {
            float s = (Sino(t, 420f, 18f) * 0.5f + Ruido(i) * 0.4f) * Env(t, 0.001f, 0.08f);
            float t2 = t - 0.12f;
            if (t2 > 0f) s += Grave(i, 0.12f) * Env(t2, 0.002f, 0.1f) * 1.4f;
            return s * 0.5f;
        });

        public static AudioClip Clique => clique != null ? clique : clique = Sintetizar("Clique", 0.06f, (t, i) =>
            Mathf.Sin(2f * Mathf.PI * 1200f * t) * Env(t, 0.001f, 0.015f) * 0.3f);

        /// <summary>Fim do dia: arpejo maior subindo (estilo bandolim).</summary>
        public static AudioClip FimDoDia => fimDoDia != null ? fimDoDia : fimDoDia = Arpejo("FimDoDia", new[] { 69, 73, 76, 81, 85 }, 0.11f, 1.2f);

        public static AudioClip Vitoria => vitoria != null ? vitoria : vitoria = Arpejo("Vitoria", new[] { 64, 69, 73, 76, 81, 76, 81, 85, 88 }, 0.12f, 1.8f);

        /// <summary>Derrota: "uóu uóu uóóóu" descendo.</summary>
        public static AudioClip Derrota => derrota != null ? derrota : derrota = Sintetizar("Derrota", 2.0f, (t, i) =>
        {
            float[] notas = { 233f, 220f, 207.6f, 196f };
            int n = Mathf.Min(3, (int)(t / 0.38f));
            float tl = t - n * 0.38f;
            float f = notas[n] * (n == 3 ? 1f - 0.04f * Mathf.Sin(tl * 30f) * Mathf.Min(1f, tl) : 1f);
            float dur = n == 3 ? 0.9f : 0.34f;
            float env = Env(tl, 0.02f, dur * 0.35f) * (tl < dur ? 1f : 0f);
            float wah = 0.5f + 0.5f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(tl / dur));
            float s = 0f;
            for (int h = 1; h <= 6; h++) s += Mathf.Sin(2f * Mathf.PI * f * h * tl) / h * (h == 1 ? 1f : wah);
            return s * env * 0.25f;
        });

        /// <summary>Loop: fogo crepitando (ronco grave + estalos).</summary>
        public static AudioClip Fogo => fogo != null ? fogo : fogo = Loop("Fogo", 4f, (t, i) =>
        {
            float s = Grave(i, 0.02f) * 2.2f;
            // estalos: posições pseudoaleatórias fixas
            int bloco = (int)(t * 9f);
            float inicio = bloco / 9f + Hash(bloco) * 0.1f;
            float te = t - inicio;
            if (te > 0f && Hash(bloco * 7 + 3) > 0.35f) s += Ruido(i) * Mathf.Exp(-te * 120f) * (0.4f + Hash(bloco * 13) * 0.6f);
            return s * 0.5f;
        });

        /// <summary>Loop: burburinho de clientes conversando + talheres.</summary>
        public static AudioClip Salao => salao != null ? salao : salao = CriarSalao();

        private static AudioClip CriarSalao()
        {
            var filtro = new float[5]; // um passa-baixa por voz
            return Loop("Salao", 6f, (t, i) =>
        {
            float s = 0f;
            for (int v = 0; v < 5; v++)
            {
                // cada "voz": ruído grave modulado por sílabas (4-6 por segundo), com pausas
                float silabas = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * (4f + v * 0.6f) * t + v * 1.7f);
                float frase = Mathf.Clamp01(Mathf.Sin(2f * Mathf.PI * (0.17f + v * 0.05f) * t + v) * 2f);
                filtro[v] += (Ruido(i + v * 9973) - filtro[v]) * (0.06f + v * 0.01f);
                s += filtro[v] * 3f * silabas * frase;
            }
            int bloco = (int)(t * 2.5f);
            float te = t - (bloco / 2.5f + Hash(bloco + 50) * 0.3f);
            if (te > 0f && Hash(bloco * 3 + 1) > 0.55f) s += Sino(te, 3000f + Hash(bloco) * 1500f, 30f) * 0.25f;
            return s * 0.35f;
        });
        }

        // ------------------------------------------------------------------ Tocar

        /// <summary>Toca um som numa posição. espacial: 0 = 2D (sem posição), 1 = totalmente 3D.</summary>
        public static void Tocar(AudioClip clip, Vector3 posicao, float vol = 1f, float espacial = 0.5f)
        {
            if (clip == null || Volume <= 0f) return;
            if (espacial <= 0f) { Tocar2D(clip, vol); return; }
            var go = new GameObject("Som " + clip.name);
            go.transform.position = posicao;
            var src = go.AddComponent<AudioSource>();
            src.clip = clip;
            src.volume = vol * Volume;
            src.spatialBlend = espacial;
            src.minDistance = 3f;
            src.pitch = Random.Range(0.95f, 1.05f);
            src.Play();
            UnityEngine.Object.Destroy(go, clip.length + 0.2f);
        }

        /// <summary>Som de interface/aviso: toca numa fonte que sobrevive à troca de cena e à pausa.</summary>
        public static void Tocar2D(AudioClip clip, float vol = 1f)
        {
            if (clip == null || Volume <= 0f) return;
            if (fonte2D == null)
            {
                var go = new GameObject("Sons 2D");
                UnityEngine.Object.DontDestroyOnLoad(go);
                fonte2D = go.AddComponent<AudioSource>();
                fonte2D.spatialBlend = 0f;
                fonte2D.ignoreListenerPause = true;
            }
            fonte2D.PlayOneShot(clip, vol * Volume);
        }

        // ------------------------------------------------------------------ Síntese

        private enum Onda { Seno, Aspera }

        private static AudioClip Notas(string nome, float vol, Onda onda, params (float freq, float dur)[] notas)
        {
            float total = 0f;
            foreach (var n in notas) total += n.dur;
            return Sintetizar(nome, total, (t, i) =>
            {
                float acc = 0f;
                foreach (var (freq, dur) in notas)
                {
                    if (t < acc + dur)
                    {
                        if (freq <= 0f) return 0f;
                        float tl = t - acc;
                        float env = Mathf.Min(1f, tl / 0.005f) * Mathf.Min(1f, (dur - tl) / 0.03f);
                        float s = Mathf.Sin(2f * Mathf.PI * freq * tl);
                        if (onda == Onda.Aspera) s = Mathf.Sign(s) * 0.6f + s * 0.4f; // meio onda quadrada: soa "errado"
                        return s * env * vol;
                    }
                    acc += dur;
                }
                return 0f;
            });
        }

        /// <summary>Arpejo de bandolim (notas MIDI) que termina numa nota longa.</summary>
        private static AudioClip Arpejo(string nome, int[] notas, float passo, float cauda)
        {
            float total = notas.Length * passo + cauda;
            return Sintetizar(nome, total, (t, i) =>
            {
                float s = 0f;
                for (int k = 0; k < notas.Length; k++)
                {
                    float tk = t - k * passo;
                    if (tk < 0f) break;
                    float f = 440f * Mathf.Pow(2f, (notas[k] - 69) / 12f);
                    bool ultima = k == notas.Length - 1;
                    // a última nota tem "tremolo" de bandolim
                    float trem = ultima ? 0.75f + 0.25f * Mathf.Sin(2f * Mathf.PI * 14f * tk) : 1f;
                    s += Corda(tk, f, ultima ? 1.6f : 7f) * trem * (ultima ? 0.7f : 0.5f);
                }
                return s * 0.4f;
            });
        }

        public static float Corda(float t, float f, float decaimento)
        {
            if (t < 0f) return 0f;
            float w = 2f * Mathf.PI * f * t;
            float s = Mathf.Sin(w) + 0.5f * Mathf.Sin(2f * w) + 0.25f * Mathf.Sin(3f * w) + 0.12f * Mathf.Sin(4f * w);
            return s * Mathf.Min(1f, t / 0.003f) * Mathf.Exp(-t * decaimento) * 0.5f;
        }

        private static float Sino(float t, float f, float decaimento) =>
            t < 0f ? 0f : Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Min(1f, t / 0.002f) * Mathf.Exp(-t * decaimento);

        private static float Env(float t, float ataque, float meiaVida) =>
            t < 0f ? 0f : Mathf.Min(1f, t / ataque) * Mathf.Exp(-t / meiaVida);

        /// <summary>Ruído branco determinístico (o mesmo índice dá sempre o mesmo valor).</summary>
        public static float Ruido(int i)
        {
            unchecked
            {
                uint x = (uint)i * 747796405u + 2891336453u;
                x = ((x >> (int)((x >> 28) + 4u)) ^ x) * 277803737u;
                x = (x >> 22) ^ x;
                return x / (float)uint.MaxValue * 2f - 1f;
            }
        }

        private static float Hash(int i) => Ruido(i * 31 + 17) * 0.5f + 0.5f;

        // filtro passa-baixa simples sobre o ruído (estado por chamada sequencial)
        private static float graveEstado;
        private static int graveUltimo = int.MinValue;
        private static float Grave(int i, float suavidade)
        {
            if (i != graveUltimo + 1) graveEstado = 0f;
            graveUltimo = i;
            graveEstado += (Ruido(i) - graveEstado) * suavidade;
            return graveEstado;
        }

        private static AudioClip Sintetizar(string nome, float duracao, Func<float, int, float> amostra)
        {
            int total = Mathf.CeilToInt(duracao * Taxa);
            var dados = new float[total];
            graveUltimo = int.MinValue;
            for (int i = 0; i < total; i++)
            {
                float t = i / (float)Taxa;
                float fade = Mathf.Min(1f, (total - i) / (Taxa * 0.01f)); // evita estalo no fim
                dados[i] = Mathf.Clamp(amostra(t, i) * fade, -1f, 1f);
            }
            var clip = AudioClip.Create(nome, total, 1, Taxa, false);
            clip.SetData(dados, 0);
            return clip;
        }

        /// <summary>Clipe em loop: o fim é misturado com o começo para não ter "emenda".</summary>
        private static AudioClip Loop(string nome, float duracao, Func<float, int, float> amostra)
        {
            int total = Mathf.CeilToInt(duracao * Taxa), cruz = Taxa / 4;
            var bruto = new float[total + cruz];
            graveUltimo = int.MinValue;
            for (int i = 0; i < bruto.Length; i++) bruto[i] = amostra(i / (float)Taxa, i);
            var dados = new float[total];
            float pico = 0.0001f;
            for (int i = 0; i < total; i++)
            {
                float s = bruto[i];
                if (i < cruz) { float k = i / (float)cruz; s = bruto[i] * k + bruto[total + i] * (1f - k); }
                dados[i] = s;
                pico = Mathf.Max(pico, Mathf.Abs(s));
            }
            float ganho = 0.7f / pico; // loops saem sempre com o mesmo nível; o volume fica no SomAmbiente
            for (int i = 0; i < total; i++) dados[i] *= ganho;
            var clip = AudioClip.Create(nome, total, 1, Taxa, false);
            clip.SetData(dados, 0);
            return clip;
        }
    }
}
