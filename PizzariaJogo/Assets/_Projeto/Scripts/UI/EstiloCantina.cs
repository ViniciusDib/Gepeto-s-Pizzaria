using System.Collections.Generic;
using UnityEngine;

namespace Pizzaria.UI
{
    /// <summary>
    /// Identidade visual da interface (mesma paleta do cenário): creme, vermelho-tomate, verde-manjericão e madeira.
    /// Gera por código texturas de cantos arredondados (9-slice) para as caixas e botões do IMGUI.
    /// </summary>
    public static class EstiloCantina
    {
        public static readonly Color Creme = new Color(0.98f, 0.94f, 0.85f);
        public static readonly Color Tomate = new Color(0.80f, 0.20f, 0.15f);
        public static readonly Color TomateClaro = new Color(0.92f, 0.32f, 0.24f);
        public static readonly Color TomateEscuro = new Color(0.58f, 0.12f, 0.09f);
        public static readonly Color Manjericao = new Color(0.12f, 0.30f, 0.20f, 0.92f);
        public static readonly Color MadeiraTranslucida = new Color(0.14f, 0.08f, 0.05f, 0.84f);
        public static readonly Color Dourado = new Color(1f, 0.80f, 0.38f);
        public static readonly Color TextoEscuro = new Color(0.24f, 0.14f, 0.09f);
        public static readonly Color VerdeBandeira = new Color(0f, 0.55f, 0.27f);
        public static readonly Color BrancoBandeira = new Color(0.96f, 0.95f, 0.92f);
        public static readonly Color VermelhoBandeira = new Color(0.80f, 0.14f, 0.16f);

        private const int Tam = 32, Raio = 10;
        private static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Resetar() => cache.Clear(); // domain reload desligado: texturas antigas foram destruídas

        /// <summary>Use com GUIStyle.border = Borda para os cantos não esticarem.</summary>
        public static RectOffset Borda => new RectOffset(Raio + 1, Raio + 1, Raio + 1, Raio + 1);

        /// <summary>Retângulo arredondado preenchido, com contorno opcional.</summary>
        public static Texture2D Arredondado(Color preenchimento, Color contorno, float espessuraContorno = 1.5f)
        {
            string chave = ColorUtility.ToHtmlStringRGBA(preenchimento) + ColorUtility.ToHtmlStringRGBA(contorno) + espessuraContorno;
            if (cache.TryGetValue(chave, out var tex) && tex != null) return tex;

            tex = new Texture2D(Tam, Tam, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.DontSave };
            var px = new Color[Tam * Tam];
            for (int y = 0; y < Tam; y++)
                for (int x = 0; x < Tam; x++)
                {
                    // distância até a borda de um retângulo arredondado (positiva = dentro)
                    float cx = Mathf.Clamp(x + 0.5f, Raio, Tam - Raio), cy = Mathf.Clamp(y + 0.5f, Raio, Tam - Raio);
                    float d = Raio - Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                    float alfa = Mathf.Clamp01(d);
                    var c = d < espessuraContorno ? contorno : preenchimento;
                    if (espessuraContorno <= 0f) c = preenchimento;
                    c.a *= alfa;
                    px[y * Tam + x] = c;
                }
            tex.SetPixels(px);
            tex.Apply();
            cache[chave] = tex;
            return tex;
        }

        /// <summary>Botão vermelho-tomate arredondado com texto creme.</summary>
        public static GUIStyle Botao(int tamanhoFonte)
        {
            var s = new GUIStyle(GUI.skin.button)
            {
                fontSize = tamanhoFonte,
                fontStyle = FontStyle.Bold,
                border = Borda,
                alignment = TextAnchor.MiddleCenter
            };
            s.normal.background = Arredondado(Tomate, TomateEscuro, 2f);
            s.hover.background = Arredondado(TomateClaro, TomateEscuro, 2f);
            s.active.background = Arredondado(TomateEscuro, TomateEscuro, 2f);
            s.focused.background = s.normal.background;
            s.normal.textColor = s.hover.textColor = s.active.textColor = s.focused.textColor = Creme;
            return s;
        }

        /// <summary>Caixa escura (madeira translúcida) com filete dourado.</summary>
        public static GUIStyle Caixa(GUIStyle baseEstilo)
        {
            var s = new GUIStyle(baseEstilo) { border = Borda };
            s.normal.background = Arredondado(MadeiraTranslucida, new Color(Dourado.r, Dourado.g, Dourado.b, 0.55f), 1.2f);
            s.normal.textColor = Creme;
            return s;
        }

        /// <summary>Comanda de papel creme com texto escuro.</summary>
        public static GUIStyle Papel(GUIStyle baseEstilo)
        {
            var s = new GUIStyle(baseEstilo) { border = Borda };
            s.normal.background = Arredondado(new Color(0.99f, 0.96f, 0.88f, 0.96f), new Color(0.75f, 0.62f, 0.45f, 1f), 1.2f);
            s.normal.textColor = TextoEscuro;
            return s;
        }

        /// <summary>Faixinha verde-branco-vermelho (detalhe de identidade).</summary>
        public static void FaixaTricolor(Rect r)
        {
            var antiga = GUI.color;
            float w = r.width / 3f;
            GUI.color = VerdeBandeira; GUI.DrawTexture(new Rect(r.x, r.y, w, r.height), Texture2D.whiteTexture);
            GUI.color = BrancoBandeira; GUI.DrawTexture(new Rect(r.x + w, r.y, w, r.height), Texture2D.whiteTexture);
            GUI.color = VermelhoBandeira; GUI.DrawTexture(new Rect(r.x + 2 * w, r.y, w, r.height), Texture2D.whiteTexture);
            GUI.color = antiga;
        }
    }
}
