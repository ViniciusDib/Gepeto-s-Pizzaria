using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pizzaria.EditorTools
{
    /// <summary>
    /// Gera texturas simples por código (piso cerâmico, azulejo, madeira, tijolo, inox, toalha xadrez),
    /// salva como PNG em Assets/_Projeto/Texturas e aplica nos materiais da cozinha.
    /// Podem ser trocadas por texturas de verdade sem mudar nada no código: basta substituir o PNG.
    /// </summary>
    public static class TexturasProcedurais
    {
        private const string Pasta = "Assets/_Projeto/Texturas";
        private const int Tam = 256;

        /// <summary>Aplica as texturas nos materiais pelo nome (os que não existirem são ignorados).</summary>
        public static void AplicarNosMateriais(Func<string, Material> buscarMaterial)
        {
            Aplicar(buscarMaterial("Piso"), "Piso_Ceramica", new Vector2(5f, 4f), PisoCeramico);
            Aplicar(buscarMaterial("Azulejo"), "Azulejo", new Vector2(10f, 1f), Azulejo);
            Aplicar(buscarMaterial("Parede"), "Parede_Reboco", new Vector2(3f, 1f), c => Reboco(c, new Color(0.95f, 0.90f, 0.82f)));
            Aplicar(buscarMaterial("Parede Salao"), "Parede_Salao", new Vector2(3f, 1f), c => Reboco(c, new Color(0.88f, 0.72f, 0.56f)));
            Aplicar(buscarMaterial("Madeira"), "Madeira", new Vector2(2f, 1f), c => Madeira(c, new Color(0.58f, 0.40f, 0.26f)));
            Aplicar(buscarMaterial("Mesa"), "Madeira_Escura", new Vector2(1f, 1f), c => Madeira(c, new Color(0.42f, 0.28f, 0.18f)));
            Aplicar(buscarMaterial("Piso Salao"), "Piso_Tacos", new Vector2(4f, 3f), c => Madeira(c, new Color(0.62f, 0.44f, 0.30f)));
            Aplicar(buscarMaterial("Bancada"), "Inox", new Vector2(2f, 1f), Inox);
            Aplicar(buscarMaterial("Metal"), "Inox", new Vector2(1f, 1f), Inox);
            Aplicar(buscarMaterial("Forno"), "Tijolo", new Vector2(2f, 2f), Tijolo);
            Aplicar(buscarMaterial("Toalha"), "Toalha_Xadrez", new Vector2(2f, 2f), Toalha);
            // Fase de arte (cantina italiana)
            Aplicar(buscarMaterial("Marmore"), "Marmore", new Vector2(1f, 1f), Marmore);
            Aplicar(buscarMaterial("Azulejo Metro"), "Azulejo_Metro", new Vector2(6f, 2f), AzulejoMetro);
            Aplicar(buscarMaterial("Cupula"), "Terracota", new Vector2(2f, 1f), Terracota);
            Aplicar(buscarMaterial("Vaso"), "Terracota", new Vector2(1f, 1f), Terracota);
            Aplicar(buscarMaterial("Azulejo Verde"), "Azulejo_Verde", new Vector2(20f, 2.5f), AzulejoVerde);
            Aplicar(buscarMaterial("Pedra"), "Pedra", new Vector2(1f, 1f), Pedra);
            Aplicar(buscarMaterial("Lambri"), "Madeira_Escura", new Vector2(8f, 1f), c => Madeira(c, new Color(0.42f, 0.28f, 0.18f)));
            Aplicar(buscarMaterial("Viga"), "Madeira_Escura", new Vector2(6f, 1f), c => Madeira(c, new Color(0.42f, 0.28f, 0.18f)));
            Aplicar(buscarMaterial("Caixote"), "Madeira_Clara", new Vector2(1f, 1f), c => Madeira(c, new Color(0.78f, 0.60f, 0.40f)));
            Aplicar(buscarMaterial("Teto"), "Teto_Reboco", new Vector2(4f, 3f), c => Reboco(c, new Color(0.93f, 0.90f, 0.84f)));
            Aplicar(buscarMaterial("Lenha"), "Madeira_Escura", new Vector2(1f, 1f), c => Madeira(c, new Color(0.42f, 0.28f, 0.18f)));
            AssetDatabase.SaveAssets();
        }

        private static void Aplicar(Material mat, string nomeTextura, Vector2 repeticao, Func<Vector2Int, Color> pixel)
        {
            if (mat == null) return;
            var tex = ObterOuCriar(nomeTextura, pixel);
            if (mat.HasProperty("_BaseMap")) { mat.SetTexture("_BaseMap", tex); mat.SetTextureScale("_BaseMap", repeticao); }
            mat.mainTexture = tex;
            mat.mainTextureScale = repeticao;
            // A cor vem da textura; o material fica branco para não tingir
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            mat.color = Color.white;
            EditorUtility.SetDirty(mat);
        }

        private static Texture2D ObterOuCriar(string nome, Func<Vector2Int, Color> pixel)
        {
            string caminho = $"{Pasta}/{nome}.png";
            var existente = AssetDatabase.LoadAssetAtPath<Texture2D>(caminho);
            if (existente != null) return existente; // não sobrescreve se alguém trocou pelo PNG de verdade

            Directory.CreateDirectory(Pasta);
            var tex = new Texture2D(Tam, Tam, TextureFormat.RGBA32, false);
            var px = new Color[Tam * Tam];
            for (int y = 0; y < Tam; y++)
                for (int x = 0; x < Tam; x++)
                    px[y * Tam + x] = pixel(new Vector2Int(x, y));
            tex.SetPixels(px);
            tex.Apply();
            File.WriteAllBytes(caminho, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(caminho);
            if (AssetImporter.GetAtPath(caminho) is TextureImporter imp)
            {
                imp.wrapMode = TextureWrapMode.Repeat;
                imp.filterMode = FilterMode.Trilinear;
                imp.mipmapEnabled = true;
                imp.anisoLevel = 4;
                imp.maxTextureSize = Tam;
                imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(caminho);
        }

        // ------------------------------------------------------------------ Padrões

        /// <summary>Ruído contínuo que repete nas bordas (a textura emenda sem costura).</summary>
        private static float Ruido(float x, float y, float escala, float semente)
        {
            float u = x / Tam, v = y / Tam;
            float a = Mathf.PerlinNoise(semente + u * escala, semente + v * escala);
            float b = Mathf.PerlinNoise(semente + (u - 1f) * escala, semente + v * escala);
            float c = Mathf.PerlinNoise(semente + u * escala, semente + (v - 1f) * escala);
            float d = Mathf.PerlinNoise(semente + (u - 1f) * escala, semente + (v - 1f) * escala);
            return Mathf.Lerp(Mathf.Lerp(a, b, u), Mathf.Lerp(c, d, u), v);
        }

        private static Color Variar(Color c, float n, float forca)
        {
            float f = 1f + (n - 0.5f) * forca;
            return new Color(c.r * f, c.g * f, c.b * f, 1f);
        }

        private static bool Rejunte(int coord, int tamanhoPeca, int largura) => coord % tamanhoPeca < largura;

        private static Color PisoCeramico(Vector2Int p)
        {
            const int peca = Tam / 2;
            if (Rejunte(p.x, peca, 3) || Rejunte(p.y, peca, 3)) return new Color(0.35f, 0.32f, 0.30f);
            bool escura = ((p.x / peca) + (p.y / peca)) % 2 == 0;
            var cor = escura ? new Color(0.70f, 0.36f, 0.24f) : new Color(0.90f, 0.84f, 0.74f);
            return Variar(cor, Ruido(p.x, p.y, 12f, 3.1f), 0.18f);
        }

        private static Color Azulejo(Vector2Int p)
        {
            const int peca = Tam / 4;
            if (Rejunte(p.x, peca, 2) || Rejunte(p.y, peca, 2)) return new Color(0.80f, 0.80f, 0.78f);
            var cor = new Color(0.60f, 0.80f, 0.82f);
            // leve brilho no canto de cada azulejo
            float bx = (p.x % peca) / (float)peca, by = (p.y % peca) / (float)peca;
            float brilho = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(bx, by), new Vector2(0.3f, 0.7f)) * 1.6f) * 0.12f;
            return Variar(cor, Ruido(p.x, p.y, 20f, 7.7f), 0.08f) + new Color(brilho, brilho, brilho, 0f);
        }

        private static Color Reboco(Vector2Int p, Color baseCor)
        {
            float n = Ruido(p.x, p.y, 8f, 1.3f) * 0.6f + Ruido(p.x, p.y, 32f, 9.2f) * 0.4f;
            return Variar(baseCor, n, 0.14f);
        }

        private static Color Madeira(Vector2Int p, Color baseCor)
        {
            const int tabua = Tam / 4;
            if (Rejunte(p.x, tabua, 2)) return baseCor * 0.55f;
            int indice = p.x / tabua;
            float veio = Mathf.Sin((p.y / (float)Tam * 6f + Ruido(p.x, p.y, 6f, indice * 4.1f) * 3f) * Mathf.PI * 2f) * 0.5f + 0.5f;
            var cor = Variar(baseCor, 0.5f + (indice % 2 == 0 ? 0.06f : -0.06f), 1f);
            return Variar(cor, veio * 0.6f + Ruido(p.x, p.y, 40f, indice) * 0.4f, 0.22f);
        }

        private static Color Inox(Vector2Int p)
        {
            // riscos horizontais de metal escovado
            float riscos = Ruido(p.x * 0.05f, p.y * 6f, 4f, 2.2f);
            return Variar(new Color(0.78f, 0.79f, 0.81f), riscos * 0.7f + Ruido(p.x, p.y, 10f, 5.5f) * 0.3f, 0.16f);
        }

        private static Color Tijolo(Vector2Int p)
        {
            const int altura = Tam / 8, largura = Tam / 4;
            int linha = p.y / altura;
            int x = p.x + (linha % 2 == 0 ? 0 : largura / 2);
            if (Rejunte(p.y, altura, 3) || Rejunte(x, largura, 3)) return new Color(0.30f, 0.27f, 0.25f);
            int id = (x / largura) * 31 + linha * 17;
            var cor = new Color(0.55f, 0.24f, 0.16f);
            cor = Variar(cor, 0.5f + ((id * 7919) % 100) / 400f - 0.12f, 1f);
            return Variar(cor, Ruido(p.x, p.y, 24f, 4.4f), 0.25f);
        }

        private static Color Marmore(Vector2Int p)
        {
            // veios: seno distorcido por ruído (turbulência)
            float turb = Ruido(p.x, p.y, 4f, 2.7f) * 0.6f + Ruido(p.x, p.y, 12f, 8.1f) * 0.3f + Ruido(p.x, p.y, 36f, 1.9f) * 0.1f;
            float veio = Mathf.Abs(Mathf.Sin((p.x / (float)Tam + p.y / (float)Tam) * Mathf.PI * 2f + turb * 9f));
            veio = Mathf.Pow(1f - veio, 6f);
            var cor = Color.Lerp(new Color(0.95f, 0.94f, 0.92f), new Color(0.55f, 0.55f, 0.58f), veio * 0.8f);
            return Variar(cor, Ruido(p.x, p.y, 20f, 3.3f), 0.05f);
        }

        private static Color AzulejoMetro(Vector2Int p)
        {
            // azulejo "metrô": peças 2:1 em amarração de tijolo, creme claro com brilho na borda
            const int altura = Tam / 4, largura = Tam / 2;
            int linha = p.y / altura;
            int x = p.x + (linha % 2 == 0 ? 0 : largura / 2);
            if (Rejunte(p.y, altura, 3) || Rejunte(x, largura, 3)) return new Color(0.62f, 0.60f, 0.56f);
            float bx = (x % largura) / (float)largura, by = (p.y % altura) / (float)altura;
            float borda = Mathf.Min(Mathf.Min(bx, 1f - bx) * 2f, Mathf.Min(by, 1f - by)) * 8f;
            float sombra = Mathf.Clamp01(borda) * 0.1f;
            var cor = new Color(0.86f, 0.89f, 0.84f) * (0.9f + sombra);
            cor.a = 1f;
            return Variar(cor, Ruido(p.x, p.y, 16f, 4.2f), 0.06f);
        }

        private static Color AzulejoVerde(Vector2Int p)
        {
            // azulejo quadrado verde-garrafa esmaltado, com borda chanfrada mais escura e variação de tom por peça
            const int peca = Tam / 2;
            if (Rejunte(p.x, peca, 3) || Rejunte(p.y, peca, 3)) return new Color(0.78f, 0.76f, 0.70f);
            float bx = (p.x % peca) / (float)peca, by = (p.y % peca) / (float)peca;
            float borda = Mathf.Clamp01(Mathf.Min(Mathf.Min(bx, 1f - bx), Mathf.Min(by, 1f - by)) * 10f);
            int id = (p.x / peca) * 13 + (p.y / peca) * 7;
            var cor = new Color(0.16f, 0.36f, 0.27f) * (0.88f + ((id * 37) % 10) / 40f) * (0.8f + borda * 0.2f);
            cor.a = 1f;
            return Variar(cor, Ruido(p.x, p.y, 10f, 2.2f), 0.12f);
        }

        private static Color Terracota(Vector2Int p)
        {
            float n = Ruido(p.x, p.y, 6f, 5.9f) * 0.5f + Ruido(p.x, p.y, 24f, 0.4f) * 0.35f + Ruido(p.x, p.y, 64f, 7.4f) * 0.15f;
            return Variar(new Color(0.74f, 0.36f, 0.22f), n, 0.35f);
        }

        private static Color Pedra(Vector2Int p)
        {
            float n = Ruido(p.x, p.y, 10f, 6.2f) * 0.5f + Ruido(p.x, p.y, 60f, 2.4f) * 0.5f;
            float pinta = Ruido(p.x, p.y, 90f, 9.9f) > 0.68f ? 0.75f : 1f;
            return Variar(new Color(0.55f, 0.53f, 0.50f) * pinta, n, 0.3f);
        }

        private static Color Toalha(Vector2Int p)
        {
            const int quadro = Tam / 8;
            bool a = (p.x / quadro) % 2 == 0, b = (p.y / quadro) % 2 == 0;
            Color vermelho = new Color(0.80f, 0.15f, 0.15f), branco = new Color(0.95f, 0.93f, 0.90f);
            Color cor = a && b ? vermelho : (a || b ? Color.Lerp(vermelho, branco, 0.55f) : branco);
            return Variar(cor, Ruido(p.x, p.y, 48f, 6.6f), 0.08f);
        }
    }
}
