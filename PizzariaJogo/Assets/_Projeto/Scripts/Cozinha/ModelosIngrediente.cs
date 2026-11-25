using System.Collections.Generic;
using Pizzaria.Dados;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pizzaria.Cozinha
{
    /// <summary>Parte de um modelo com a cor "crua" dela (a pizza escurece essa cor ao assar).</summary>
    public struct ParteColorida
    {
        public Renderer renderer;
        public Color cor;
    }

    /// <summary>
    /// Modelos low-poly dos ingredientes, montados em tempo de jogo com primitivas + anéis (toro) gerados por código:
    /// rodela de tomate com miolo, folha de manjericão, rodela de calabresa com gordurinha, anel de cebola,
    /// azeitona fatiada... Usados nas coberturas da pizza e no ingrediente que o cozinheiro segura.
    /// </summary>
    public static class ModelosIngrediente
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly Dictionary<int, Mesh> aneis = new Dictionary<int, Mesh>();
        private static MaterialPropertyBlock bloco;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Resetar() { aneis.Clear(); bloco = null; }

        // ------------------------------------------------------------------ Coberturas da pizza

        /// <summary>Um pedaço de cobertura sobre a pizza, centrado em <paramref name="pos"/> (local à pizza).</summary>
        public static void Cobertura(VisualIngrediente visual, Color cor, Transform pai, Vector3 pos, float giro, bool sombras, List<ParteColorida> saida)
        {
            var rot = Quaternion.Euler(0f, giro, 0f);
            switch (visual)
            {
                case VisualIngrediente.Tomate: // rodela: casca vermelha, miolo mais claro e sementes
                    Parte(PrimitiveType.Cylinder, pai, pos, new Vector3(0.05f, 0.003f, 0.05f), rot, cor, sombras, saida);
                    Parte(PrimitiveType.Cylinder, pai, pos + Vector3.up * 0.0035f, new Vector3(0.038f, 0.001f, 0.038f), rot, Color.Lerp(cor, new Color(1f, 0.55f, 0.45f), 0.45f), sombras, saida);
                    for (int i = 0; i < 3; i++)
                    {
                        var d = rot * (Quaternion.Euler(0, i * 120f, 0) * new Vector3(0.009f, 0f, 0f));
                        Parte(PrimitiveType.Sphere, pai, pos + d + Vector3.up * 0.0045f, new Vector3(0.006f, 0.002f, 0.004f), rot, new Color(0.98f, 0.85f, 0.55f), false, saida);
                    }
                    break;

                case VisualIngrediente.Manjericao: // duas folhas com nervura
                    for (int i = 0; i < 2; i++)
                    {
                        var r = rot * Quaternion.Euler(0, i * 70f - 35f, 0);
                        var p = pos + r * new Vector3(0, 0, 0.012f) + Vector3.up * (0.002f + i * 0.002f);
                        Parte(PrimitiveType.Sphere, pai, p, new Vector3(0.022f, 0.003f, 0.04f), r * Quaternion.Euler(6f, 0, 0), cor, false, saida);
                        Parte(PrimitiveType.Cube, pai, p + Vector3.up * 0.0015f, new Vector3(0.002f, 0.001f, 0.032f), r, Color.Lerp(cor, Color.white, 0.25f), false, saida);
                    }
                    break;

                case VisualIngrediente.Calabresa: // rodela com borda escura e pontinhos de gordura
                    Parte(PrimitiveType.Cylinder, pai, pos, new Vector3(0.048f, 0.004f, 0.048f), rot, cor * 0.8f, sombras, saida);
                    Parte(PrimitiveType.Cylinder, pai, pos + Vector3.up * 0.0042f, new Vector3(0.042f, 0.001f, 0.042f), rot, cor, false, saida);
                    for (int i = 0; i < 4; i++)
                    {
                        var d = rot * (Quaternion.Euler(0, i * 97f, 0) * new Vector3(0.006f + i * 0.003f, 0f, 0f));
                        Parte(PrimitiveType.Sphere, pai, pos + d + Vector3.up * 0.005f, new Vector3(0.006f, 0.0015f, 0.006f), rot, new Color(0.98f, 0.82f, 0.78f), false, saida);
                    }
                    break;

                case VisualIngrediente.Cebola: // anel fino, um pouco inclinado
                    Malha(Anel(0.09f), pai, pos + Vector3.up * 0.002f, new Vector3(0.052f, 0.03f, 0.04f), rot * Quaternion.Euler(0, 0, 4f), cor, false, saida);
                    break;

                case VisualIngrediente.Azeitona: // azeitona fatiada (anel grosso) com o pimentão no meio
                    Malha(Anel(0.24f), pai, pos + Vector3.up * 0.003f, new Vector3(0.026f, 0.015f, 0.026f), rot, cor, false, saida);
                    Parte(PrimitiveType.Cylinder, pai, pos + Vector3.up * 0.002f, new Vector3(0.009f, 0.002f, 0.009f), rot, new Color(0.85f, 0.2f, 0.12f), false, saida);
                    break;

                default:
                    Parte(PrimitiveType.Sphere, pai, pos, new Vector3(0.035f, 0.014f, 0.035f), rot, cor, sombras, saida);
                    break;
            }
        }

        /// <summary>Borda estufada da massa (um toro em volta do disco).</summary>
        public static Renderer Borda(Transform pai, float raio, float altura, Color cor, bool sombras, List<ParteColorida> saida)
        {
            return Malha(Anel(0.075f), pai, new Vector3(0f, altura, 0f), new Vector3(raio * 2f, raio * 1.4f, raio * 2f), Quaternion.identity, cor, sombras, saida);
        }

        /// <summary>Gotinhas de queijo derretido por cima da camada (dá textura ao disco liso).</summary>
        public static void BolhasQueijo(Transform pai, float raio, float altura, Color cor, bool sombras, List<ParteColorida> saida)
        {
            for (int i = 0; i < 9; i++)
            {
                float a = i * 2.39996f, d = raio * 0.8f * Mathf.Sqrt((i + 0.5f) / 9f);
                var p = new Vector3(Mathf.Cos(a) * d, altura, Mathf.Sin(a) * d);
                Parte(PrimitiveType.Sphere, pai, p, new Vector3(0.04f, 0.006f, 0.032f), Quaternion.Euler(0, a * 57f, 0), Color.Lerp(cor, Color.white, 0.18f), false, saida);
            }
        }

        // ------------------------------------------------------------------ Na mão

        /// <summary>Monta o modelo do ingrediente que o cozinheiro segura (≈10 cm), sem sombras.</summary>
        public static GameObject NaMao(IngredienteSO ing, Transform pai)
        {
            var raiz = new GameObject("Modelo " + ing.Nome).transform;
            raiz.SetParent(pai, false);
            var cor = ing.cor;
            var lista = new List<ParteColorida>();
            var verde = new Color(0.22f, 0.5f, 0.2f);
            Quaternion I = Quaternion.identity;

            switch (ing.visual)
            {
                case VisualIngrediente.Massa:
                    Parte(PrimitiveType.Sphere, raiz, Vector3.zero, new Vector3(0.1f, 0.07f, 0.1f), I, cor, false, lista);
                    Parte(PrimitiveType.Sphere, raiz, new Vector3(0.01f, 0.028f, 0.005f), new Vector3(0.05f, 0.012f, 0.04f), I, new Color(0.99f, 0.97f, 0.93f), false, lista);
                    break;
                case VisualIngrediente.Molho: // potinho de cerâmica com molho e uma folhinha
                    Parte(PrimitiveType.Cylinder, raiz, Vector3.zero, new Vector3(0.09f, 0.025f, 0.09f), I, new Color(0.95f, 0.9f, 0.82f), false, lista);
                    Parte(PrimitiveType.Cylinder, raiz, new Vector3(0, 0.024f, 0), new Vector3(0.078f, 0.002f, 0.078f), I, cor, false, lista);
                    Parte(PrimitiveType.Sphere, raiz, new Vector3(0.015f, 0.027f, 0.01f), new Vector3(0.012f, 0.002f, 0.02f), Quaternion.Euler(0, 30, 0), verde, false, lista);
                    break;
                case VisualIngrediente.Queijo: // fatia de queijo com furinhos
                    Parte(PrimitiveType.Cube, raiz, Vector3.zero, new Vector3(0.1f, 0.045f, 0.07f), Quaternion.Euler(0, 15, 0), cor, false, lista);
                    for (int i = 0; i < 4; i++)
                        Parte(PrimitiveType.Sphere, raiz, new Vector3(-0.03f + i * 0.02f, 0.02f + (i % 2) * 0.003f, -0.01f + (i % 3) * 0.01f), Vector3.one * 0.012f, I, cor * 0.78f, false, lista);
                    break;
                case VisualIngrediente.Tomate:
                    Parte(PrimitiveType.Sphere, raiz, Vector3.zero, new Vector3(0.085f, 0.07f, 0.085f), I, cor, false, lista);
                    for (int i = 0; i < 5; i++)
                        Parte(PrimitiveType.Capsule, raiz, Quaternion.Euler(0, i * 72f, 0) * new Vector3(0.012f, 0.034f, 0f), new Vector3(0.008f, 0.013f, 0.004f), Quaternion.Euler(0, i * 72f, 80f), verde, false, lista);
                    Parte(PrimitiveType.Cylinder, raiz, new Vector3(0, 0.042f, 0), new Vector3(0.006f, 0.008f, 0.006f), I, verde * 0.8f, false, lista);
                    Parte(PrimitiveType.Sphere, raiz, new Vector3(-0.02f, 0.018f, -0.025f), new Vector3(0.018f, 0.012f, 0.01f), I, Color.Lerp(cor, Color.white, 0.5f), false, lista); // brilho
                    break;
                case VisualIngrediente.Manjericao: // raminho com folhas
                    Parte(PrimitiveType.Cylinder, raiz, new Vector3(0, -0.01f, 0), new Vector3(0.006f, 0.035f, 0.006f), Quaternion.Euler(20, 0, 0), verde * 0.75f, false, lista);
                    for (int i = 0; i < 6; i++)
                    {
                        var r = Quaternion.Euler(-30f - (i % 2) * 20f, i * 60f, 0);
                        Parte(PrimitiveType.Sphere, raiz, new Vector3(0, 0.01f + i * 0.006f, 0) + r * new Vector3(0, 0, 0.022f), new Vector3(0.024f, 0.005f, 0.045f), r, cor, false, lista);
                    }
                    break;
                case VisualIngrediente.Calabresa: // gomo de linguiça com as pontas amarradas
                    Parte(PrimitiveType.Capsule, raiz, Vector3.zero, new Vector3(0.042f, 0.06f, 0.042f), Quaternion.Euler(0, 0, 90), cor, false, lista);
                    Parte(PrimitiveType.Sphere, raiz, new Vector3(0.062f, 0, 0), new Vector3(0.012f, 0.008f, 0.008f), I, new Color(0.9f, 0.85f, 0.75f), false, lista);
                    Parte(PrimitiveType.Sphere, raiz, new Vector3(-0.062f, 0, 0), new Vector3(0.012f, 0.008f, 0.008f), I, new Color(0.9f, 0.85f, 0.75f), false, lista);
                    for (int i = 0; i < 5; i++)
                        Parte(PrimitiveType.Sphere, raiz, new Vector3(-0.03f + i * 0.015f, 0.019f, (i % 2) * 0.006f - 0.003f), new Vector3(0.006f, 0.003f, 0.006f), I, new Color(0.95f, 0.8f, 0.75f), false, lista);
                    break;
                case VisualIngrediente.Cebola: // cebola inteira com pontinha e raiz, e um anel solto
                    Parte(PrimitiveType.Sphere, raiz, Vector3.zero, new Vector3(0.08f, 0.072f, 0.08f), I, cor, false, lista);
                    Parte(PrimitiveType.Capsule, raiz, new Vector3(0, 0.045f, 0), new Vector3(0.016f, 0.014f, 0.016f), I, Color.Lerp(cor, new Color(0.6f, 0.45f, 0.3f), 0.5f), false, lista);
                    Parte(PrimitiveType.Sphere, raiz, new Vector3(0, -0.036f, 0), new Vector3(0.02f, 0.008f, 0.02f), I, new Color(0.7f, 0.62f, 0.5f), false, lista);
                    for (int i = 0; i < 4; i++) // listras roxas
                        Parte(PrimitiveType.Sphere, raiz, Vector3.zero, new Vector3(0.081f - i * 0.0005f, 0.073f, 0.008f), Quaternion.Euler(0, i * 45f, 0), Color.Lerp(cor, new Color(0.55f, 0.3f, 0.5f), 0.55f), false, lista);
                    break;
                case VisualIngrediente.Azeitona: // três azeitonas com o pimentão
                    for (int i = 0; i < 3; i++)
                    {
                        var p = new Vector3(-0.025f + i * 0.025f, (i % 2) * 0.01f, (i % 2) * 0.012f);
                        Parte(PrimitiveType.Sphere, raiz, p, new Vector3(0.024f, 0.022f, 0.032f), Quaternion.Euler(0, i * 25f, 0), cor, false, lista);
                        Parte(PrimitiveType.Sphere, raiz, p + Quaternion.Euler(0, i * 25f, 0) * new Vector3(0, 0, 0.015f), Vector3.one * 0.009f, I, new Color(0.85f, 0.2f, 0.12f), false, lista);
                    }
                    break;
                default:
                    Parte(PrimitiveType.Sphere, raiz, Vector3.zero, new Vector3(0.1f, 0.07f, 0.1f), I, cor, false, lista);
                    break;
            }
            return raiz.gameObject;
        }

        // ------------------------------------------------------------------ Peças

        private static Renderer Parte(PrimitiveType tipo, Transform pai, Vector3 pos, Vector3 escala, Quaternion rot, Color cor, bool sombras, List<ParteColorida> saida)
        {
            var go = GameObject.CreatePrimitive(tipo);
            Object.Destroy(go.GetComponent<Collider>());
            return Configurar(go, pai, pos, escala, rot, cor, sombras, saida);
        }

        private static Renderer Malha(Mesh malha, Transform pai, Vector3 pos, Vector3 escala, Quaternion rot, Color cor, bool sombras, List<ParteColorida> saida)
        {
            var go = new GameObject("Anel");
            go.AddComponent<MeshFilter>().sharedMesh = malha;
            go.AddComponent<MeshRenderer>();
            return Configurar(go, pai, pos, escala, rot, cor, sombras, saida);
        }

        private static Renderer Configurar(GameObject go, Transform pai, Vector3 pos, Vector3 escala, Quaternion rot, Color cor, bool sombras, List<ParteColorida> saida)
        {
            go.name = "Parte";
            go.transform.SetParent(pai, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot;
            go.transform.localScale = escala;
            var r = go.GetComponent<Renderer>();
            var mat = RecursosCozinha.MaterialBase;
            if (mat != null) r.sharedMaterial = mat;
            r.shadowCastingMode = sombras ? ShadowCastingMode.On : ShadowCastingMode.Off;
            if (bloco == null) bloco = new MaterialPropertyBlock();
            bloco.SetColor(BaseColorId, cor);
            r.SetPropertyBlock(bloco);
            saida?.Add(new ParteColorida { renderer = r, cor = cor });
            return r;
        }

        /// <summary>Toro de raio maior 0.5 no plano XZ; <paramref name="espessura"/> é o raio do tubo (fração do diâmetro).</summary>
        public static Mesh Anel(float espessura)
        {
            int chave = Mathf.RoundToInt(espessura * 1000f);
            if (aneis.TryGetValue(chave, out var m) && m != null) return m;

            const int voltas = 24, lados = 8;
            const float R = 0.5f;
            float r = espessura;
            var v = new Vector3[(voltas + 1) * (lados + 1)];
            var n = new Vector3[v.Length];
            var tris = new int[voltas * lados * 6];
            for (int i = 0; i <= voltas; i++)
            {
                float a = i / (float)voltas * Mathf.PI * 2f;
                var centro = new Vector3(Mathf.Cos(a) * R, 0f, Mathf.Sin(a) * R);
                for (int j = 0; j <= lados; j++)
                {
                    float b = j / (float)lados * Mathf.PI * 2f;
                    var normal = new Vector3(Mathf.Cos(a) * Mathf.Cos(b), Mathf.Sin(b), Mathf.Sin(a) * Mathf.Cos(b));
                    v[i * (lados + 1) + j] = centro + normal * r;
                    n[i * (lados + 1) + j] = normal;
                }
            }
            int t = 0;
            for (int i = 0; i < voltas; i++)
                for (int j = 0; j < lados; j++)
                {
                    int a0 = i * (lados + 1) + j, a1 = a0 + 1, b0 = a0 + lados + 1, b1 = b0 + 1;
                    tris[t++] = a0; tris[t++] = a1; tris[t++] = b1;
                    tris[t++] = a0; tris[t++] = b1; tris[t++] = b0;
                }
            m = new Mesh { name = "Anel " + espessura, vertices = v, normals = n, triangles = tris };
            m.RecalculateBounds();
            aneis[chave] = m;
            return m;
        }
    }
}
