using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pizzaria.EditorTools
{
    /// <summary>
    /// Malhas low-poly geradas por código (sólidos de revolução: tigela, garrafa, vaso, cúpula do forno...)
    /// salvas em Assets/_Projeto/Malhas. Dão formas arredondadas que as primitivas do Unity não têm.
    /// </summary>
    public static class MalhasProcedurais
    {
        private const string Pasta = "Assets/_Projeto/Malhas";
        private static readonly Dictionary<string, Mesh> cache = new Dictionary<string, Mesh>();

        public static void LimparCache() => cache.Clear();

        // Perfis (raio, altura) de baixo para cima pelo lado de fora; voltando por dentro quando a peça é oca.
        // Unidade: raio máx. ~0.5 e altura ~1 — a escala do objeto define o tamanho real.
        public static Mesh Tigela => Torno("Tigela", 16, new[] {
            V(0, 0), V(0.32f, 0), V(0.45f, 0.12f), V(0.5f, 0.32f), V(0.5f, 0.35f),
            V(0.46f, 0.35f), V(0.42f, 0.15f), V(0.3f, 0.07f), V(0, 0.07f) });

        public static Mesh Garrafa => Torno("Garrafa", 12, new[] {
            V(0, 0), V(0.5f, 0), V(0.5f, 0.58f), V(0.38f, 0.72f), V(0.17f, 0.82f), V(0.15f, 0.95f), V(0.19f, 0.97f), V(0.19f, 1f), V(0, 1f) });

        public static Mesh Pote => Torno("Pote", 14, new[] {
            V(0, 0), V(0.44f, 0), V(0.5f, 0.08f), V(0.5f, 0.78f), V(0.42f, 0.86f), V(0.42f, 0.92f), V(0, 0.92f) });

        public static Mesh Vaso => Torno("Vaso", 14, new[] {
            V(0, 0), V(0.34f, 0), V(0.48f, 0.74f), V(0.55f, 0.76f), V(0.55f, 0.88f), V(0.47f, 0.88f), V(0.45f, 0.8f), V(0, 0.8f) });

        public static Mesh Prato => Torno("Prato", 18, new[] {
            V(0, 0), V(0.34f, 0), V(0.5f, 0.08f), V(0.47f, 0.09f), V(0.33f, 0.03f), V(0, 0.03f) });

        public static Mesh Abajur => Torno("Abajur", 16, new[] {
            V(0, 0.31f), V(0.06f, 0.31f), V(0.12f, 0.28f), V(0.47f, 0.01f), V(0.5f, 0),
            V(0.13f, 0.3f), V(0.06f, 0.34f), V(0, 0.34f) });

        public static Mesh Cupula => Torno("Cupula", 22, new[] {
            V(0.52f, 0), V(0.51f, 0.22f), V(0.44f, 0.52f), V(0.3f, 0.76f), V(0.13f, 0.9f), V(0, 0.93f) });

        public static Mesh Sino => Torno("Sino", 14, new[] {
            V(0, 0), V(0.5f, 0), V(0.45f, 0.12f), V(0.3f, 0.5f), V(0.12f, 0.62f), V(0.05f, 0.72f), V(0.08f, 0.8f), V(0, 0.82f) });

        /// <summary>Bandeirinha triangular (dupla face), com a ponta para baixo.</summary>
        public static Mesh Bandeirinha
        {
            get
            {
                if (cache.TryGetValue("Bandeirinha", out var m) && m != null) return m;
                m = ObterAsset("Bandeirinha");
                var v = new[] { new Vector3(-0.5f, 0, 0), new Vector3(0.5f, 0, 0), new Vector3(0, -1f, 0) };
                m.vertices = new[] { v[0], v[1], v[2], v[0], v[1], v[2] };
                m.uv = new[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 0) };
                m.triangles = new[] { 0, 1, 2, 3, 5, 4 };
                m.normals = new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.forward, Vector3.forward, Vector3.forward };
                m.RecalculateBounds();
                return Salvar("Bandeirinha", m);
            }
        }

        /// <summary>Toro (anel) salvo como asset — mesma geometria usada em tempo de jogo pelas coberturas.</summary>
        public static Mesh Anel(float espessura)
        {
            string nome = "Anel_" + Mathf.RoundToInt(espessura * 1000f);
            if (cache.TryGetValue(nome, out var m) && m != null) return m;
            var origem = Pizzaria.Cozinha.ModelosIngrediente.Anel(espessura);
            m = ObterAsset(nome);
            m.vertices = origem.vertices;
            m.normals = origem.normals;
            m.triangles = origem.triangles;
            m.RecalculateBounds();
            return Salvar(nome, m);
        }

        private static Vector2 V(float r, float y) => new Vector2(r, y);

        /// <summary>Sólido de revolução: gira o perfil em volta do eixo Y.</summary>
        private static Mesh Torno(string nome, int lados, Vector2[] perfil)
        {
            if (cache.TryGetValue(nome, out var m) && m != null) return m;
            m = ObterAsset(nome);

            int aneis = perfil.Length, colunas = lados + 1; // coluna extra fecha a costura do UV
            var verts = new Vector3[aneis * colunas];
            var uvs = new Vector2[verts.Length];

            var comprimento = new float[aneis];
            for (int i = 1; i < aneis; i++) comprimento[i] = comprimento[i - 1] + Vector2.Distance(perfil[i], perfil[i - 1]);
            float total = Mathf.Max(0.0001f, comprimento[aneis - 1]);

            for (int i = 0; i < aneis; i++)
                for (int j = 0; j < colunas; j++)
                {
                    float a = j / (float)lados * Mathf.PI * 2f;
                    verts[i * colunas + j] = new Vector3(Mathf.Cos(a) * perfil[i].x, perfil[i].y, Mathf.Sin(a) * perfil[i].x);
                    uvs[i * colunas + j] = new Vector2(j / (float)lados, comprimento[i] / total);
                }

            var tris = new List<int>();
            for (int i = 0; i < aneis - 1; i++)
                for (int j = 0; j < lados; j++)
                {
                    int a = i * colunas + j, b = (i + 1) * colunas + j, c = b + 1, d = a + 1;
                    tris.Add(a); tris.Add(b); tris.Add(c);
                    tris.Add(a); tris.Add(c); tris.Add(d);
                }

            m.vertices = verts;
            m.uv = uvs;
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return Salvar(nome, m);
        }

        private static Mesh ObterAsset(string nome)
        {
            var m = AssetDatabase.LoadAssetAtPath<Mesh>($"{Pasta}/{nome}.asset");
            if (m != null) { m.Clear(); return m; } // reaproveita o asset: a cena antiga continua apontando para ele
            return new Mesh { name = nome };
        }

        private static Mesh Salvar(string nome, Mesh m)
        {
            string caminho = $"{Pasta}/{nome}.asset";
            if (!AssetDatabase.Contains(m))
            {
                if (!AssetDatabase.IsValidFolder(Pasta)) AssetDatabase.CreateFolder("Assets/_Projeto", "Malhas");
                AssetDatabase.CreateAsset(m, caminho);
            }
            else EditorUtility.SetDirty(m);
            cache[nome] = m;
            return m;
        }
    }
}
