using System.Collections.Generic;
using System.Text;
using Pizzaria.Dados;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pizzaria.EditorTools
{
    /// <summary>Decoração da cozinha e peças reutilizáveis (luminária, varal, quadro, letreiro, vaso, placa...).</summary>
    public static partial class GeradorCozinha
    {
        // ------------------------------------------------------------------ Decoração da cozinha

        private static void CriarDecoracaoCozinha(Dictionary<string, ReceitaSO> receitas)
        {
            var deco = new GameObject("Decoracao Cozinha").transform;
            float hx = Largura / 2f, hz = Profundidade / 2f;
            var escura = Mat("Viga", CorMadeiraEscura);
            var metal = Mat("Metal", new Color(0.6f, 0.6f, 0.62f), 0.7f, 0.8f);
            var ferro = Mat("Ferro", new Color(0.12f, 0.12f, 0.12f), 0.4f, 0.6f);

            // Luminárias pendentes (verde esmaltado) — ilha, bancada do fundo e frente
            foreach (float x in new[] { -0.85f, 0.85f }) Luminaria(deco, new Vector3(x, 2.3f, 0.8f), 1.9f, 4.5f);
            foreach (float x in new[] { -3.2f, 0f, 3.2f }) Luminaria(deco, new Vector3(x, 2.5f, hz - 0.95f), 1.7f, 4.5f);
            foreach (float x in new[] { -2.6f, 2.6f }) Luminaria(deco, new Vector3(x, 2.4f, -2.2f), 1.6f, 4.5f);

            DetalharIlha(deco);
            DetalharBalcao(deco);
            Malha(MalhasProcedurais.Sino, "Campainha", deco, new Vector3(1.36f, 1.1f, -hz + 0.05f), Vector3.one * 0.1f, Mat("Latao", new Color(0.85f, 0.65f, 0.3f), 0.8f, 1f));

            // Prateleira alta no fundo com potes, garrafas, latas e pratos
            float ys = 2.45f, zs = hz - 0.24f;
            Bloco("Prateleira Fundo", deco, new Vector3(0, ys, zs), new Vector3(Largura - 0.6f, 0.035f, 0.26f), escura, true);
            for (float x = -4.2f; x <= 4.2f; x += 1.4f)
                Bloco("Mao Francesa", deco, new Vector3(x, ys - 0.08f, zs + 0.05f), new Vector3(0.03f, 0.14f, 0.16f), ferro, true);
            CriarItensPrateleira(deco, -4.55f, 4.55f, ys + 0.0175f, zs);

            // Parede esquerda: barra com panelas de cobre penduradas
            Prim(PrimitiveType.Cylinder, "Barra Panelas", deco, new Vector3(-hx + 0.16f, 2.3f, 1.6f), new Vector3(0.025f, 0.75f, 0.025f), ferro, Quaternion.Euler(90, 0, 0));
            var cobre = Mat("Cobre", new Color(0.78f, 0.45f, 0.28f), 0.75f, 0.9f);
            float[] zPanelas = { 0.95f, 1.35f, 1.8f, 2.2f };
            for (int i = 0; i < zPanelas.Length; i++)
            {
                float z = zPanelas[i], d = i % 2 == 0 ? 0.3f : 0.24f;
                var m = i == 2 ? ferro : cobre;
                Prim(PrimitiveType.Cylinder, "Panela", deco, new Vector3(-hx + 0.16f, 2.08f - d / 2f, z), new Vector3(d, 0.02f, d), m, Quaternion.Euler(0, 0, 90));
                Prim(PrimitiveType.Cube, "Cabo", deco, new Vector3(-hx + 0.16f, 2.2f, z), new Vector3(0.02f, 0.2f, 0.03f), m);
            }

            // Parede direita: pá de pizza, lenha empilhada e sacos de farinha perto do forno
            var madeira = Mat("Madeira", new Color(0.55f, 0.38f, 0.25f));
            Prim(PrimitiveType.Cylinder, "Pa Cabo", deco, new Vector3(hx - 0.14f, 1.3f, 1.7f), new Vector3(0.035f, 0.5f, 0.035f), madeira);
            Prim(PrimitiveType.Cylinder, "Pa", deco, new Vector3(hx - 0.14f, 1.98f, 1.7f), new Vector3(0.36f, 0.006f, 0.36f), metal, Quaternion.Euler(0, 0, 90));
            var lenha = Mat("Lenha", CorMadeiraEscura);
            float[][] fileiras = { new[] { 4.52f, 4.66f, 4.80f }, new[] { 4.59f, 4.73f }, new[] { 4.66f } };
            for (int f = 0; f < fileiras.Length; f++)
                foreach (float x in fileiras[f])
                    Prim(PrimitiveType.Cylinder, "Lenha", deco, new Vector3(x, 0.07f + f * 0.12f, 2.5f + R(-0.04f, 0.04f)), new Vector3(0.13f, 0.38f, 0.13f), lenha, Quaternion.Euler(90, R(-4, 4), 0));
            ColisorInvisivel("Colisor Lenha", deco, new Vector3(4.66f, 0.2f, 2.5f), new Vector3(0.42f, 0.4f, 0.8f));

            var saco = Mat("Saco", new Color(0.88f, 0.82f, 0.68f));
            Prim(PrimitiveType.Sphere, "Saco de Farinha", deco, new Vector3(4.5f, 0.27f, -0.75f), new Vector3(0.42f, 0.55f, 0.34f), saco, Quaternion.Euler(0, 10, 4));
            Prim(PrimitiveType.Sphere, "Boca do Saco", deco, new Vector3(4.5f, 0.56f, -0.75f), new Vector3(0.14f, 0.08f, 0.12f), saco);
            Prim(PrimitiveType.Sphere, "Saco de Farinha", deco, new Vector3(4.12f, 0.21f, -0.8f), new Vector3(0.38f, 0.42f, 0.32f), saco, Quaternion.Euler(0, -25, -8));
            ColisorInvisivel("Colisor Sacos", deco, new Vector3(4.32f, 0.3f, -0.77f), new Vector3(0.8f, 0.6f, 0.42f));

            // Frente direita: carrinho com caixas de pizza e lousa de receitas
            Bloco("Carrinho", deco, new Vector3(2.4f, 0.375f, -hz + 0.45f), new Vector3(0.7f, 0.75f, 0.5f), madeira);
            var papelao = Mat("Papelao", new Color(0.78f, 0.62f, 0.42f));
            for (int i = 0; i < 6; i++)
                Prim(PrimitiveType.Cube, "Caixa de Pizza", deco, new Vector3(2.4f + R(-0.02f, 0.02f), 0.776f + i * 0.052f, -hz + 0.45f), new Vector3(0.42f, 0.05f, 0.42f), papelao, Quaternion.Euler(0, R(-6, 6), 0));
            Prim(PrimitiveType.Cylinder, "Selo", deco, new Vector3(2.4f, 1.088f, -hz + 0.45f), new Vector3(0.16f, 0.002f, 0.16f), Mat("Selo", CorTomate));
            CriarLousaReceitas(deco, receitas, new Vector3(3.25f, 1.85f, -hz + 0.12f));

            // Relógio de parede (frente esquerda)
            var rel = new Vector3(-3.2f, 2.25f, -hz + 0.12f);
            Prim(PrimitiveType.Cylinder, "Aro Relogio", deco, rel, new Vector3(0.46f, 0.012f, 0.46f), escura, Quaternion.Euler(90, 0, 0));
            Prim(PrimitiveType.Cylinder, "Mostrador", deco, rel + new Vector3(0, 0, 0.01f), new Vector3(0.4f, 0.01f, 0.4f), Mat("Ceramica", CorCreme, 0.7f), Quaternion.Euler(90, 0, 0));
            for (int i = 0; i < 12; i++)
            {
                float a = i * Mathf.PI / 6f;
                Prim(PrimitiveType.Cube, "Marca", deco, rel + new Vector3(Mathf.Sin(a) * 0.16f, Mathf.Cos(a) * 0.16f, 0.022f), new Vector3(0.012f, i % 3 == 0 ? 0.04f : 0.022f, 0.004f), ferro, Quaternion.Euler(0, 0, -a * Mathf.Rad2Deg));
            }
            Prim(PrimitiveType.Cube, "Ponteiro Hora", deco, rel + new Vector3(0.035f, 0.035f, 0.026f), new Vector3(0.014f, 0.1f, 0.004f), ferro, Quaternion.Euler(0, 0, -45));
            Prim(PrimitiveType.Cube, "Ponteiro Minuto", deco, rel + new Vector3(-0.03f, 0.055f, 0.03f), new Vector3(0.01f, 0.13f, 0.004f), ferro, Quaternion.Euler(0, 0, 30));

            // Plantas nos cantos da frente e bandeirinhas
            VasoComPlanta(deco, new Vector3(-4.45f, 0, -hz + 0.5f), 0.55f, 0.45f);
            VasoComPlanta(deco, new Vector3(4.45f, 0, -hz + 0.5f), 0.5f, 0.42f);
            Varal(deco, new Vector3(-hx + 0.1f, 2.78f, -3.2f), new Vector3(hx - 0.1f, 2.78f, -3.2f), 0.25f, 26);
            Varal(deco, new Vector3(-hx + 0.1f, 2.7f, 2.0f), new Vector3(-1.4f, 2.7f, -1.8f), 0.2f, 14);
        }

        private static void CriarItensPrateleira(Transform pai, float x0, float x1, float y, float z)
        {
            var conteudos = new[] { new Color(0.97f, 0.95f, 0.9f), new Color(0.95f, 0.78f, 0.35f), new Color(0.65f, 0.15f, 0.1f), new Color(0.45f, 0.28f, 0.18f), new Color(0.85f, 0.75f, 0.55f) };
            var vidro = Mat("Vidro", new Color(0.75f, 0.85f, 0.85f), 0.95f);
            var tampa = Mat("Tampa", CorVerdeEsmalte, 0.6f, 0.3f);
            float x = x0;
            int i = 0;
            while (x < x1)
            {
                int tipo = rnd.Next(0, 5);
                var p = new Vector3(x, y, z + R(-0.03f, 0.03f));
                switch (tipo)
                {
                    case 0:
                    case 1: // pote de vidro com conteúdo colorido
                    {
                        float h = R(0.16f, 0.26f), r = R(0.09f, 0.13f);
                        var c = conteudos[rnd.Next(conteudos.Length)];
                        Malha(MalhasProcedurais.Pote, "Pote", pai, p, new Vector3(r * 2f, h, r * 2f), vidro);
                        Prim(PrimitiveType.Cylinder, "Conteudo", pai, p + Vector3.up * h * 0.38f, new Vector3(r * 1.85f, h * 0.36f, r * 1.85f), Mat("Pote_" + ColorUtility.ToHtmlStringRGB(c), c));
                        Prim(PrimitiveType.Cylinder, "Tampa", pai, p + Vector3.up * (h * 0.92f + 0.01f), new Vector3(r * 1.7f, 0.012f, r * 1.7f), tampa);
                        x += r * 2f + 0.06f;
                        break;
                    }
                    case 2: // garrafa de azeite ou vinho
                    {
                        bool vinho = rnd.Next(2) == 0;
                        float h = R(0.26f, 0.32f);
                        Malha(MalhasProcedurais.Garrafa, vinho ? "Vinho" : "Azeite", pai, p, new Vector3(0.08f, h, 0.08f),
                              vinho ? Mat("Vidro Vinho", new Color(0.25f, 0.05f, 0.08f), 0.9f) : Mat("Vidro Azeite", new Color(0.35f, 0.42f, 0.10f), 0.9f));
                        Prim(PrimitiveType.Cylinder, "Rotulo", pai, p + Vector3.up * h * 0.32f, new Vector3(0.082f, h * 0.1f, 0.082f), Mat("Rotulo", CorCreme));
                        x += 0.12f;
                        break;
                    }
                    case 3: // latas de tomate pelado
                    {
                        int n = rnd.Next(1, 3);
                        for (int k = 0; k < n; k++)
                        {
                            Prim(PrimitiveType.Cylinder, "Lata", pai, p + new Vector3(0, 0.06f + k * 0.12f, 0), new Vector3(0.1f, 0.06f, 0.1f), Mat("Lata", CorTomate, 0.6f, 0.4f));
                            Prim(PrimitiveType.Cylinder, "Rotulo Lata", pai, p + new Vector3(0, 0.06f + k * 0.12f, 0), new Vector3(0.102f, 0.025f, 0.102f), Mat("Rotulo", CorCreme));
                        }
                        x += 0.16f;
                        break;
                    }
                    default: // pilha de pratos
                    {
                        int n = rnd.Next(3, 6);
                        for (int k = 0; k < n; k++)
                            Malha(MalhasProcedurais.Prato, "Prato", pai, p + Vector3.up * k * 0.022f, Vector3.one * 0.24f, Mat("Ceramica", CorCreme, 0.7f));
                        x += 0.3f;
                        break;
                    }
                }
                x += R(0.02f, 0.08f);
                i++;
            }
        }

        private static void CriarLousaReceitas(Transform pai, Dictionary<string, ReceitaSO> receitas, Vector3 centro)
        {
            // Lousa voltada para dentro da cozinha (quem lê olha para -Z)
            const float largura = 1.6f, altura = 1.25f;
            var rot = Quaternion.Euler(0, 180, 0);
            Bloco("Lousa Receitas", pai, centro, new Vector3(largura, altura, 0.03f), Mat("Painel", new Color(0.12f, 0.17f, 0.15f), 0.1f), true);
            Moldura(pai, centro + new Vector3(0, 0, 0.01f), largura, altura, rot, 0.06f);

            var sb = new StringBuilder("<b>RECEITAS DA CASA</b>\n");
            foreach (var r in receitas.Values)
            {
                if (r == null) continue;
                sb.Append("<b>").Append(r.Nome).Append("</b>  <color=#f2c46d>R$ ").Append(r.preco).Append("</color>\n  ");
                string passos = r.DescricaoPassos();
                // quebra a cada 4 passos para caber na lousa
                var partes = passos.Split(new[] { " › " }, System.StringSplitOptions.None);
                for (int i = 0; i < partes.Length; i++)
                {
                    if (i > 0) sb.Append(i % 4 == 0 ? "\n  " : " › ");
                    sb.Append(partes[i]);
                }
                sb.Append('\n');
            }
            var texto = Texto3D(pai, "Receitas", centro + new Vector3(largura / 2f - 0.07f, altura / 2f - 0.06f, 0.02f), rot, 0.25f, new Color(0.96f, 0.95f, 0.9f));
            texto.text = sb.ToString().TrimEnd('\n');
            texto.anchor = TextAnchor.UpperLeft;
            texto.alignment = TextAlignment.Left;
            texto.fontStyle = FontStyle.Normal;
            texto.richText = true;
        }

        // ------------------------------------------------------------------ Salão: letreiro e quadros

        private static void CriarLetreiro(Transform pai, Vector3 pos)
        {
            var rot = Quaternion.Euler(0, 180, 0); // lido por quem olha para -Z (da cozinha/salão para a parede do fundo)
            Prim(PrimitiveType.Cube, "Letreiro", pai, pos, new Vector3(2.7f, 0.75f, 0.04f), Mat("Viga", CorMadeiraEscura), rot);
            var neon = MatEmissivo("Neon", new Color(1f, 0.25f, 0.2f), 6f);
            float w = 2.55f, h = 0.62f, z = pos.z + 0.03f;
            Prim(PrimitiveType.Cylinder, "Neon", pai, new Vector3(pos.x, pos.y + h / 2, z), new Vector3(0.022f, w / 2, 0.022f), neon, Quaternion.Euler(0, 0, 90));
            Prim(PrimitiveType.Cylinder, "Neon", pai, new Vector3(pos.x, pos.y - h / 2, z), new Vector3(0.022f, w / 2, 0.022f), neon, Quaternion.Euler(0, 0, 90));
            Prim(PrimitiveType.Cylinder, "Neon", pai, new Vector3(pos.x - w / 2, pos.y, z), new Vector3(0.022f, h / 2, 0.022f), neon);
            Prim(PrimitiveType.Cylinder, "Neon", pai, new Vector3(pos.x + w / 2, pos.y, z), new Vector3(0.022f, h / 2, 0.022f), neon);
            Texto3D(pai, "PIZZARIA", new Vector3(pos.x, pos.y + 0.07f, pos.z + 0.035f), rot, 1.5f, new Color(1f, 0.93f, 0.8f));
            var sub = Texto3D(pai, "forno a lenha", new Vector3(pos.x, pos.y - 0.19f, pos.z + 0.035f), rot, 0.55f, new Color(1f, 0.75f, 0.35f));
            sub.fontStyle = FontStyle.Italic;

            var luz = new GameObject("Brilho Neon").AddComponent<Light>();
            luz.transform.SetParent(pai);
            luz.transform.position = pos + new Vector3(0, 0, 0.35f);
            luz.type = LightType.Point;
            luz.color = new Color(1f, 0.35f, 0.3f);
            luz.intensity = 0.9f;
            luz.range = 2.5f;
            luz.shadows = LightShadows.None;
        }

        private static void Quadro(Transform pai, Vector3 pos, Quaternion rot, int tipo)
        {
            const float lw = 0.8f, lh = 0.6f;
            Moldura(pai, pos, lw, lh, rot, 0.06f);
            // Tela: a frente fica no -Z local (de frente para quem olha a parede)
            Vector3 L(float x, float y, float z) => pos + rot * new Vector3(x, y, z);
            switch (tipo)
            {
                case 0: // bandeira
                    Prim(PrimitiveType.Cube, "Tela Verde", pai, L(-lw / 3f, 0, 0), new Vector3(lw / 3f, lh, 0.02f), Mat("Bandeira Verde", new Color(0f, 0.55f, 0.27f)), rot);
                    Prim(PrimitiveType.Cube, "Tela Branca", pai, L(0, 0, 0), new Vector3(lw / 3f, lh, 0.02f), Mat("Bandeira Branca", new Color(0.96f, 0.95f, 0.92f)), rot);
                    Prim(PrimitiveType.Cube, "Tela Vermelha", pai, L(lw / 3f, 0, 0), new Vector3(lw / 3f, lh, 0.02f), Mat("Bandeira Vermelha", new Color(0.80f, 0.14f, 0.16f)), rot);
                    break;
                case 1: // paisagem: vulcão, mar e sol
                    Prim(PrimitiveType.Cube, "Ceu", pai, L(0, 0, 0), new Vector3(lw, lh, 0.02f), Mat("Tinta Ceu", new Color(0.95f, 0.70f, 0.50f)), rot);
                    Prim(PrimitiveType.Cube, "Vulcao", pai, L(0.05f, -0.12f, -0.012f), new Vector3(0.3f, 0.3f, 0.005f), Mat("Tinta Vulcao", new Color(0.42f, 0.30f, 0.30f)), rot * Quaternion.Euler(0, 0, 45));
                    Prim(PrimitiveType.Cube, "Mar", pai, L(0, -0.22f, -0.016f), new Vector3(lw, 0.16f, 0.005f), Mat("Tinta Mar", new Color(0.20f, 0.42f, 0.55f)), rot);
                    Prim(PrimitiveType.Cylinder, "Sol", pai, L(-0.22f, 0.13f, -0.012f), new Vector3(0.12f, 0.003f, 0.12f), Mat("Tinta Sol", new Color(1f, 0.88f, 0.5f)), rot * Quaternion.Euler(90, 0, 0));
                    break;
                default: // natureza-morta de pizza
                    Prim(PrimitiveType.Cube, "Tela", pai, L(0, 0, 0), new Vector3(lw, lh, 0.02f), Mat("Tinta Creme", CorCreme), rot);
                    Prim(PrimitiveType.Cylinder, "Pizza", pai, L(0, -0.02f, -0.012f), new Vector3(0.4f, 0.003f, 0.4f), Mat("Massa Assada", new Color(0.86f, 0.58f, 0.30f)), rot * Quaternion.Euler(90, 0, 0));
                    Prim(PrimitiveType.Cylinder, "Molho", pai, L(0, -0.02f, -0.016f), new Vector3(0.34f, 0.003f, 0.34f), Mat("Molho Assado", new Color(0.72f, 0.16f, 0.08f)), rot * Quaternion.Euler(90, 0, 0));
                    for (int i = 0; i < 5; i++)
                    {
                        float a = i * Mathf.PI * 0.4f;
                        Prim(PrimitiveType.Cylinder, "Queijo", pai, L(Mathf.Cos(a) * 0.09f, -0.02f + Mathf.Sin(a) * 0.09f, -0.02f), new Vector3(0.07f, 0.003f, 0.07f), Mat("Queijo Assado", new Color(1f, 0.82f, 0.38f), 0.5f), rot * Quaternion.Euler(90, 0, 0));
                    }
                    break;
            }
        }

        // ------------------------------------------------------------------ Peças reutilizáveis

        /// <summary>Primitiva decorativa sem collider, posição/rotação/escala em mundo (o pai não pode ter escala).</summary>
        private static GameObject Prim(PrimitiveType tipo, string nome, Transform pai, Vector3 pos, Vector3 escala, Material mat, Quaternion? rot = null)
        {
            var go = GameObject.CreatePrimitive(tipo);
            go.name = nome;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(pai, true);
            go.transform.SetPositionAndRotation(pos, rot ?? Quaternion.identity);
            go.transform.localScale = escala;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.isStatic = true;
            return go;
        }

        /// <summary>Malha procedural decorativa (sem collider).</summary>
        private static GameObject Malha(Mesh malha, string nome, Transform pai, Vector3 pos, Vector3 escala, Material mat, Quaternion? rot = null)
        {
            var go = new GameObject(nome);
            go.transform.SetParent(pai, true);
            go.transform.SetPositionAndRotation(pos, rot ?? Quaternion.identity);
            go.transform.localScale = escala;
            go.AddComponent<MeshFilter>().sharedMesh = malha;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            go.isStatic = true;
            return go;
        }

        private static void ColisorInvisivel(string nome, Transform pai, Vector3 pos, Vector3 tamanho)
        {
            var go = new GameObject(nome);
            go.transform.SetParent(pai, true);
            go.transform.position = pos;
            go.AddComponent<BoxCollider>().size = tamanho;
            go.isStatic = true;
        }

        private static void MarcarEstatico(Transform raiz, bool estatico = true)
        {
            foreach (var t in raiz.GetComponentsInChildren<Transform>(true))
                t.gameObject.isStatic = estatico;
        }

        /// <summary>Moldura de 4 réguas de madeira em volta de um retângulo (largura em X local, altura em Y).</summary>
        private static void Moldura(Transform pai, Vector3 centro, float largura, float altura, Quaternion rot, float e)
        {
            var mat = Mat("Viga", CorMadeiraEscura);
            Vector3 L(float x, float y) => centro + rot * new Vector3(x, y, -0.01f);
            Prim(PrimitiveType.Cube, "Moldura", pai, L(0, altura / 2), new Vector3(largura + e * 2, e, e), mat, rot);
            Prim(PrimitiveType.Cube, "Moldura", pai, L(0, -altura / 2), new Vector3(largura + e * 2, e, e), mat, rot);
            Prim(PrimitiveType.Cube, "Moldura", pai, L(-largura / 2, 0), new Vector3(e, altura, e), mat, rot);
            Prim(PrimitiveType.Cube, "Moldura", pai, L(largura / 2, 0), new Vector3(e, altura, e), mat, rot);
        }

        private static void VasoComPlanta(Transform pai, Vector3 basePos, float alturaVaso, float raioCopa, Material folha = null)
        {
            float s = alturaVaso / 0.88f;
            Malha(MalhasProcedurais.Vaso, "Vaso", pai, basePos, Vector3.one * s, Mat("Vaso", new Color(0.74f, 0.36f, 0.22f)));
            Prim(PrimitiveType.Cylinder, "Terra", pai, basePos + Vector3.up * 0.78f * s, new Vector3(0.9f * s, 0.005f, 0.9f * s), Mat("Terra", new Color(0.25f, 0.17f, 0.12f)));
            var f1 = folha != null ? folha : Mat("Folha", CorManjericao);
            var f2 = folha != null ? folha : Mat("Folha Clara", new Color(0.35f, 0.55f, 0.25f));
            var centro = basePos + Vector3.up * (alturaVaso + raioCopa * 0.55f);
            int n = 7;
            for (int i = 0; i < n; i++)
            {
                float a = i / (float)n * Mathf.PI * 2f + R(-0.3f, 0.3f);
                var off = i == 0 ? new Vector3(0, raioCopa * 0.5f, 0) : new Vector3(Mathf.Cos(a) * raioCopa * 0.55f, R(-0.15f, 0.25f) * raioCopa, Mathf.Sin(a) * raioCopa * 0.55f);
                Prim(PrimitiveType.Sphere, "Folhagem", pai, centro + off, Vector3.one * raioCopa * R(0.75f, 1.05f), i % 2 == 0 ? f1 : f2);
            }
        }

        /// <summary>Luminária pendente esmaltada com lâmpada (brilha no bloom) e luz pontual quente, sem sombra.</summary>
        private static void Luminaria(Transform pai, Vector3 baseCupula, float intensidade, float alcance)
        {
            const float escala = 0.42f;
            float topoCupula = baseCupula.y + 0.34f * escala;
            float fio = AlturaParede - topoCupula;
            Prim(PrimitiveType.Cylinder, "Fio", pai, new Vector3(baseCupula.x, topoCupula + fio / 2f, baseCupula.z), new Vector3(0.012f, fio / 2f, 0.012f), Mat("Ferro", new Color(0.12f, 0.12f, 0.12f), 0.4f, 0.6f));
            Prim(PrimitiveType.Cylinder, "Canopla", pai, new Vector3(baseCupula.x, AlturaParede - 0.01f, baseCupula.z), new Vector3(0.1f, 0.01f, 0.1f), Mat("Abajur", CorVerdeEsmalte, 0.65f, 0.3f));
            Malha(MalhasProcedurais.Abajur, "Cupula", pai, baseCupula, Vector3.one * escala, Mat("Abajur", CorVerdeEsmalte, 0.65f, 0.3f));
            Prim(PrimitiveType.Sphere, "Lampada", pai, baseCupula + Vector3.up * 0.05f, Vector3.one * 0.08f, MatEmissivo("Lampada", new Color(1f, 0.82f, 0.55f), 3.5f));

            var luz = new GameObject("Luz Luminaria").AddComponent<Light>();
            luz.transform.SetParent(pai);
            luz.transform.position = baseCupula - Vector3.up * 0.04f;
            luz.type = LightType.Point;
            luz.color = CorLuzQuente;
            luz.intensity = intensidade;
            luz.range = alcance;
            luz.shadows = LightShadows.None;
        }

        /// <summary>Varal de bandeirinhas verde/branco/vermelho entre dois pontos, com barriga (catenária aproximada).</summary>
        private static void Varal(Transform pai, Vector3 a, Vector3 b, float barriga, int bandeiras)
        {
            var fio = Mat("Ferro", new Color(0.12f, 0.12f, 0.12f), 0.4f, 0.6f);
            var cores = new[]
            {
                Mat("Bandeira Verde", new Color(0f, 0.55f, 0.27f)),
                Mat("Bandeira Branca", new Color(0.96f, 0.95f, 0.92f)),
                Mat("Bandeira Vermelha", new Color(0.80f, 0.14f, 0.16f)),
            };
            Vector3 PontoVaral(float t) => Vector3.Lerp(a, b, t) + Vector3.down * (barriga * 4f * t * (1f - t));

            const int segmentos = 12;
            for (int i = 0; i < segmentos; i++)
            {
                var p = PontoVaral(i / (float)segmentos);
                var q = PontoVaral((i + 1) / (float)segmentos);
                var d = q - p;
                Prim(PrimitiveType.Cylinder, "Fio Varal", pai, (p + q) / 2f, new Vector3(0.008f, d.magnitude / 2f, 0.008f), fio, Quaternion.FromToRotation(Vector3.up, d));
            }

            var dir = (b - a).normalized;
            var olhar = Quaternion.LookRotation(Vector3.Cross(dir, Vector3.up), Vector3.up);
            for (int i = 0; i < bandeiras; i++)
            {
                float t = (i + 0.5f) / bandeiras;
                var go = Malha(MalhasProcedurais.Bandeirinha, "Bandeirinha", pai, PontoVaral(t), new Vector3(0.2f, 0.24f, 1f), cores[i % 3], olhar * Quaternion.Euler(0, 0, R(-6f, 6f)));
                go.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        /// <summary>Placa com texto, fundo colorido e moldura de madeira. "pendurada" adiciona correntes até o teto.</summary>
        private static void Placa(Transform pai, string texto, Vector3 pos, Quaternion rot, Color corFundo, float escala = 1f, bool pendurada = false)
        {
            var placa = new GameObject("Placa " + texto).transform;
            placa.SetParent(pai, true);
            placa.position = pos;
            placa.rotation = rot;
            placa.localScale = Vector3.one * escala;

            var moldura = GameObject.CreatePrimitive(PrimitiveType.Cube);
            moldura.name = "Moldura";
            Object.DestroyImmediate(moldura.GetComponent<Collider>());
            moldura.transform.SetParent(placa, false);
            moldura.transform.localPosition = new Vector3(0, 0, 0.008f);
            moldura.transform.localScale = new Vector3(1.4f, 0.48f, 0.025f);
            moldura.GetComponent<Renderer>().sharedMaterial = Mat("Viga", CorMadeiraEscura);

            var fundo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fundo.name = "Fundo";
            Object.DestroyImmediate(fundo.GetComponent<Collider>());
            fundo.transform.SetParent(placa, false);
            fundo.transform.localScale = new Vector3(1.3f, 0.38f, 0.03f);
            fundo.GetComponent<Renderer>().sharedMaterial = Mat("Placa_" + ColorUtility.ToHtmlStringRGB(corFundo), corFundo, 0.3f);

            float lum = corFundo.r * 0.3f + corFundo.g * 0.59f + corFundo.b * 0.11f;
            Color corTexto = lum > 0.6f ? new Color(0.15f, 0.12f, 0.1f) : Color.white;

            // O texto é lido por quem olha na direção +Z local da placa, então fica na face -Z
            // nomes compridos (MANJERICÃO, CALABRESA) diminuem para caber na placa e não invadir a vizinha
            float ajuste = Mathf.Min(1f, 6.5f / Mathf.Max(1, texto.Length));
            var t = Texto3D(placa, texto, Vector3.zero, Quaternion.identity, ajuste, corTexto);
            t.transform.localPosition = new Vector3(0, 0, -0.02f);
            t.transform.localRotation = Quaternion.identity;

            if (pendurada)
            {
                var corrente = Mat("Ferro", new Color(0.12f, 0.12f, 0.12f), 0.4f, 0.6f);
                foreach (float s in new[] { -1f, 1f })
                {
                    var p = pos + rot * new Vector3(s * 0.55f * escala, 0.24f * escala, 0);
                    float alt = AlturaParede - p.y;
                    Prim(PrimitiveType.Cylinder, "Corrente", pai, p + Vector3.up * alt / 2f, new Vector3(0.01f, alt / 2f, 0.01f), corrente);
                }
            }
            MarcarEstatico(placa);
        }
    }
}
