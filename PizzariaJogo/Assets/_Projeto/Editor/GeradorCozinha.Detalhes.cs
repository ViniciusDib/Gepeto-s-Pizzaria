using Pizzaria.Ambiente;
using Pizzaria.Cozinha;
using UnityEngine;

namespace Pizzaria.EditorTools
{
    /// <summary>Detalhes finos: ingredientes das estações, bancadas (portas, puxadores, utensílios) e personagens.</summary>
    public static partial class GeradorCozinha
    {
        // ------------------------------------------------------------------ Materiais de apoio

        private static Material MCeramica => Mat("Ceramica", CorCreme, 0.7f);
        private static Material MTerracota => Mat("Vaso", new Color(0.74f, 0.36f, 0.22f));
        private static Material MMetal => Mat("Metal", new Color(0.6f, 0.6f, 0.62f), 0.7f, 0.8f);
        private static Material MEscura => Mat("Viga", CorMadeiraEscura);
        private static Material MClara => Mat("Caixote", new Color(0.78f, 0.60f, 0.40f));
        private static Material MFarinha => Mat("Farinha", new Color(0.97f, 0.95f, 0.9f));
        private static Material MFolha => Mat("Folha", CorManjericao);
        private static Material MMarmore => Mat("Marmore", Color.white, 0.7f);

        /// <summary>Folha (manjericão, cabinho...) = esfera achatada com nervura clara. Comprimento ao longo do Z local.</summary>
        private static void Folha(Transform pai, Vector3 pos, Quaternion rot, float comprimento, Material mat)
        {
            Prim(PrimitiveType.Sphere, "Folha", pai, pos, new Vector3(comprimento * 0.58f, comprimento * 0.12f, comprimento), mat, rot);
            Prim(PrimitiveType.Cube, "Nervura", pai, pos + rot * new Vector3(0, comprimento * 0.06f, 0), new Vector3(comprimento * 0.05f, comprimento * 0.02f, comprimento * 0.8f), Mat("Nervura", new Color(0.45f, 0.7f, 0.4f)), rot);
        }

        private static void Tomate(Transform pai, Vector3 pos, float tamanho, Material cor, float giro)
        {
            Prim(PrimitiveType.Sphere, "Tomate", pai, pos, new Vector3(tamanho, tamanho * 0.82f, tamanho), cor, Quaternion.Euler(0, giro, 0));
            var topo = pos + Vector3.up * tamanho * 0.4f;
            for (int i = 0; i < 5; i++)
            {
                var r = Quaternion.Euler(0, giro + i * 72f, 0);
                Prim(PrimitiveType.Capsule, "Sepala", pai, topo + r * new Vector3(0, 0, tamanho * 0.13f), new Vector3(tamanho * 0.09f, tamanho * 0.14f, tamanho * 0.04f), MFolha, r * Quaternion.Euler(80, 0, 0));
            }
            Prim(PrimitiveType.Cylinder, "Cabinho", pai, topo + Vector3.up * tamanho * 0.06f, new Vector3(tamanho * 0.07f, tamanho * 0.08f, tamanho * 0.07f), Mat("Cabinho", new Color(0.3f, 0.45f, 0.2f)));
        }

        // ------------------------------------------------------------------ Estações de ingrediente

        private static void DetalharCaixote(Transform t, Vector3 p, Material ripa)
        {
            // ripas da frente, cantoneiras e alças vazadas nas laterais
            Bloco("Ripa", t, p + new Vector3(0, 0.045f, -0.243f), new Vector3(0.69f, 0.03f, 0.01f), ripa, true);
            Bloco("Ripa", t, p + new Vector3(0, 0.115f, -0.243f), new Vector3(0.69f, 0.03f, 0.01f), ripa, true);
            foreach (float x in new[] { -0.33f, 0.33f })
                foreach (float z in new[] { -0.23f, 0.23f })
                    Bloco("Cantoneira", t, p + new Vector3(x, 0.082f, z), new Vector3(0.035f, 0.166f, 0.035f), ripa, true);
            foreach (float x in new[] { -0.342f, 0.342f })
                Bloco("Alca", t, p + new Vector3(x, 0.11f, 0), new Vector3(0.006f, 0.03f, 0.12f), Mat("Sombra", new Color(0.12f, 0.08f, 0.05f)), true);
        }

        private static void ConteudoEstacao(Transform t, string id, Vector3 topo, Material cor)
        {
            var corBase = cor.HasProperty("_BaseColor") ? cor.GetColor("_BaseColor") : cor.color;
            switch (id)
            {
                case "Massa":
                {
                    // tabuleiro enfarinhado com bolas de massa, espátula e farinha espalhada
                    Bloco("Tabuleiro", t, topo + new Vector3(0, 0.01f, 0), new Vector3(0.64f, 0.02f, 0.44f), MClara, true);
                    foreach (var (x, z, sx, sz) in new[] { (0f, 0.215f, 0.64f, 0.02f), (0f, -0.215f, 0.64f, 0.02f), (0.315f, 0f, 0.02f, 0.44f), (-0.315f, 0f, 0.02f, 0.44f) })
                        Bloco("Borda Tabuleiro", t, topo + new Vector3(x, 0.03f, z), new Vector3(sx, 0.025f, sz), MEscura, true);
                    Bloco("Farinha", t, topo + new Vector3(0, 0.021f, 0), new Vector3(0.6f, 0.002f, 0.4f), MFarinha, true);
                    for (int i = 0; i < 6; i++)
                    {
                        float s = R(0.15f, 0.18f);
                        var pos = topo + new Vector3(-0.19f + (i % 3) * 0.19f + R(-0.015f, 0.015f), 0.06f, (i < 3 ? -0.09f : 0.09f) + R(-0.01f, 0.01f));
                        Prim(PrimitiveType.Sphere, "Bola de Massa", t, pos, new Vector3(s, s * 0.6f, s), cor, Quaternion.Euler(0, R(0, 360), 0));
                        Prim(PrimitiveType.Sphere, "Farinha na Massa", t, pos + new Vector3(R(-0.02f, 0.02f), s * 0.27f, R(-0.02f, 0.02f)), new Vector3(s * 0.5f, s * 0.08f, s * 0.4f), MFarinha);
                    }
                    for (int i = 0; i < 12; i++)
                        Prim(PrimitiveType.Sphere, "Farinha Solta", t, topo + new Vector3(R(-0.3f, 0.3f), 0.023f, R(-0.2f, 0.2f)), new Vector3(R(0.02f, 0.05f), 0.003f, R(0.02f, 0.05f)), MFarinha);
                    // espátula de massa (lâmina de inox com pegador de madeira) apoiada na borda
                    Prim(PrimitiveType.Cube, "Espatula", t, topo + new Vector3(0.36f, 0.05f, -0.16f), new Vector3(0.1f, 0.003f, 0.08f), MMetal, Quaternion.Euler(0, 20, 25));
                    Prim(PrimitiveType.Cylinder, "Pegador", t, topo + new Vector3(0.405f, 0.075f, -0.14f), new Vector3(0.022f, 0.05f, 0.022f), MClara, Quaternion.Euler(90, 20, 25));
                    break;
                }

                case "Molho":
                {
                    // tigela de barro com molho, borda mais escura, folhinhas, concha e lata de tomate pelado
                    var centro = topo + new Vector3(-0.06f, 0, 0);
                    Malha(MalhasProcedurais.Tigela, "Tigela", t, centro, Vector3.one * 0.44f, MTerracota);
                    Prim(PrimitiveType.Cylinder, "Molho", t, centro + new Vector3(0, 0.128f, 0), new Vector3(0.39f, 0.005f, 0.39f), cor);
                    Malha(MalhasProcedurais.Anel(0.06f), "Borda do Molho", t, centro + new Vector3(0, 0.13f, 0), new Vector3(0.38f, 0.05f, 0.38f), Mat("Molho Borda", corBase * 0.7f, 0.6f));
                    for (int i = 0; i < 6; i++) // pedacinhos de tomate no molho
                        Prim(PrimitiveType.Sphere, "Pedaco", t, centro + new Vector3(R(-0.13f, 0.13f), 0.134f, R(-0.13f, 0.13f)), new Vector3(0.03f, 0.01f, 0.025f), Mat("Molho Pedaco", corBase * 1.15f, 0.6f));
                    Folha(t, centro + new Vector3(0.04f, 0.137f, -0.05f), Quaternion.Euler(0, 40, 0), 0.06f, MFolha);
                    Folha(t, centro + new Vector3(0.07f, 0.138f, -0.03f), Quaternion.Euler(0, 100, 0), 0.05f, MFolha);
                    // concha: cabo inclinado saindo da tigela, bojo dentro do molho
                    Prim(PrimitiveType.Sphere, "Bojo Concha", t, centro + new Vector3(-0.08f, 0.13f, 0.06f), new Vector3(0.08f, 0.03f, 0.08f), MMetal);
                    Prim(PrimitiveType.Cylinder, "Cabo Concha", t, centro + new Vector3(-0.16f, 0.21f, 0.1f), new Vector3(0.016f, 0.11f, 0.016f), MMetal, Quaternion.Euler(-20, 0, 50));
                    // lata
                    var lata = topo + new Vector3(0.25f, 0, -0.13f);
                    Prim(PrimitiveType.Cylinder, "Lata", t, lata + new Vector3(0, 0.065f, 0), new Vector3(0.1f, 0.065f, 0.1f), Mat("Lata", CorTomate, 0.6f, 0.4f));
                    Prim(PrimitiveType.Cylinder, "Rotulo Lata", t, lata + new Vector3(0, 0.065f, 0), new Vector3(0.102f, 0.03f, 0.102f), Mat("Rotulo", CorCreme));
                    Prim(PrimitiveType.Cylinder, "Tampa Lata", t, lata + new Vector3(0, 0.131f, 0), new Vector3(0.096f, 0.002f, 0.096f), MMetal);
                    Tomate(t, topo + new Vector3(0.25f, 0.045f, 0.12f), 0.1f, cor, 30f);
                    break;
                }

                case "Queijo":
                {
                    // tigela de queijo ralado, bolinhas de muçarela num pratinho e uma peça com furos
                    var centro = topo + new Vector3(-0.08f, 0, 0.02f);
                    Malha(MalhasProcedurais.Tigela, "Tigela", t, centro, Vector3.one * 0.42f, MCeramica);
                    Malha(MalhasProcedurais.Anel(0.12f), "Faixa Tigela", t, centro + new Vector3(0, 0.1f, 0), new Vector3(0.43f, 0.05f, 0.43f), Mat("Faixa Azul", new Color(0.22f, 0.38f, 0.62f), 0.7f));
                    Prim(PrimitiveType.Sphere, "Monte de Queijo", t, centro + new Vector3(0, 0.12f, 0), new Vector3(0.34f, 0.12f, 0.34f), cor);
                    for (int i = 0; i < 26; i++)
                        Prim(PrimitiveType.Cube, "Queijo Ralado", t, centro + new Vector3(R(-0.13f, 0.13f), R(0.15f, 0.18f), R(-0.13f, 0.13f)),
                             new Vector3(0.01f, 0.01f, R(0.04f, 0.07f)), cor, Quaternion.Euler(R(-20, 40), R(0, 360), 0));
                    var prato = topo + new Vector3(0.24f, 0, 0.12f);
                    Malha(MalhasProcedurais.Prato, "Pratinho", t, prato, Vector3.one * 0.2f, MCeramica);
                    var mussarela = Mat("Mussarela", new Color(0.98f, 0.97f, 0.92f), 0.6f);
                    for (int i = 0; i < 3; i++)
                        Prim(PrimitiveType.Sphere, "Bolinha de Mussarela", t, prato + new Vector3(-0.03f + i * 0.03f, 0.03f + (i == 1 ? 0.02f : 0f), (i - 1) * 0.02f), Vector3.one * 0.045f, mussarela);
                    // peça triangular (cunha) com furos
                    var cunha = topo + new Vector3(0.25f, 0.045f, -0.12f);
                    var rot = Quaternion.Euler(0, 30, 0);
                    Prim(PrimitiveType.Cube, "Peca de Queijo", t, cunha, new Vector3(0.15f, 0.09f, 0.1f), cor, rot);
                    Prim(PrimitiveType.Cube, "Casca", t, cunha + rot * new Vector3(0, 0, 0.051f), new Vector3(0.15f, 0.09f, 0.004f), Mat("Casca Queijo", new Color(0.9f, 0.62f, 0.2f)), rot);
                    var furo = Mat("Furo Queijo", corBase * 0.72f);
                    foreach (var f in new[] { new Vector3(-0.04f, 0.02f, -0.05f), new Vector3(0.03f, -0.015f, -0.05f), new Vector3(0.05f, 0.025f, -0.05f), new Vector3(-0.076f, 0f, 0.01f) })
                        Prim(PrimitiveType.Sphere, "Furo", t, cunha + rot * f, Vector3.one * R(0.015f, 0.025f), furo);
                    break;
                }

                case "Tomate":
                {
                    for (int i = 0; i < 7; i++)
                    {
                        var pos = topo + new Vector3(-0.24f + (i % 4) * 0.15f + R(-0.015f, 0.015f), 0.05f, i < 4 ? -0.1f : 0.08f);
                        Tomate(t, pos, R(0.11f, 0.13f), cor, R(0, 360));
                    }
                    // tomate cortado ao meio: casca, polpa clara e sementes nas 3 cavidades
                    var meio = topo + new Vector3(0.22f, 0.018f, 0.12f);
                    Prim(PrimitiveType.Cylinder, "Meio Tomate", t, meio, new Vector3(0.12f, 0.018f, 0.12f), cor);
                    Prim(PrimitiveType.Cylinder, "Polpa", t, meio + Vector3.up * 0.019f, new Vector3(0.1f, 0.001f, 0.1f), Mat("Polpa Tomate", new Color(1f, 0.45f, 0.38f), 0.7f));
                    var semente = Mat("Semente", new Color(0.98f, 0.85f, 0.45f), 0.7f);
                    for (int c = 0; c < 3; c++)
                    {
                        var cav = Quaternion.Euler(0, c * 120f, 0) * new Vector3(0.025f, 0, 0);
                        Prim(PrimitiveType.Sphere, "Cavidade", t, meio + cav + Vector3.up * 0.0195f, new Vector3(0.03f, 0.002f, 0.022f), Mat("Cavidade", new Color(0.85f, 0.25f, 0.2f), 0.8f), Quaternion.Euler(0, c * 120f, 0));
                        for (int k = 0; k < 3; k++)
                            Prim(PrimitiveType.Sphere, "Semente", t, meio + cav + Quaternion.Euler(0, k * 120f, 0) * new Vector3(0.007f, 0, 0) + Vector3.up * 0.021f, new Vector3(0.006f, 0.002f, 0.004f), semente);
                    }
                    break;
                }

                case "Manjericao":
                {
                    // vaso de barro com caules e três "andares" de folhas
                    var basePos = topo + new Vector3(-0.1f, 0, 0);
                    const float alturaVaso = 0.17f;
                    float sv = alturaVaso / 0.88f;
                    Malha(MalhasProcedurais.Vaso, "Vaso", t, basePos, Vector3.one * sv, MTerracota);
                    Prim(PrimitiveType.Cylinder, "Terra", t, basePos + Vector3.up * 0.78f * sv, new Vector3(0.9f * sv, 0.005f, 0.9f * sv), Mat("Terra", new Color(0.25f, 0.17f, 0.12f)));
                    var caule = Mat("Caule", new Color(0.3f, 0.5f, 0.22f));
                    var centro = basePos + Vector3.up * alturaVaso;
                    for (int i = 0; i < 4; i++)
                    {
                        var off = Quaternion.Euler(0, i * 90f + 20f, 0) * new Vector3(0.025f, 0, 0);
                        Prim(PrimitiveType.Cylinder, "Caule", t, centro + off + Vector3.up * 0.07f, new Vector3(0.008f, 0.07f, 0.008f), caule, Quaternion.Euler(off.z * 200f, 0, -off.x * 200f));
                    }
                    for (int andar = 0; andar < 3; andar++)
                        for (int i = 0; i < 6; i++)
                        {
                            var r = Quaternion.Euler(0, i * 60f + andar * 30f, 0) * Quaternion.Euler(-30f + andar * 12f, 0, 0);
                            float L = 0.075f - andar * 0.012f;
                            Folha(t, centro + Vector3.up * (0.03f + andar * 0.045f) + r * new Vector3(0, 0, L * 0.5f), r, L, cor);
                        }
                    Folha(t, centro + Vector3.up * 0.17f, Quaternion.Euler(-70, 0, 0), 0.04f, cor);
                    // maço solto amarrado com barbante
                    for (int i = 0; i < 5; i++)
                        Folha(t, topo + new Vector3(0.2f, 0.01f + i * 0.004f, -0.05f + i * 0.012f), Quaternion.Euler(0, 160 + i * 14f, 0), 0.07f, cor);
                    Prim(PrimitiveType.Cylinder, "Barbante", t, topo + new Vector3(0.24f, 0.012f, 0.04f), new Vector3(0.02f, 0.006f, 0.02f), Mat("Barbante", new Color(0.85f, 0.75f, 0.55f)), Quaternion.Euler(0, 0, 90));
                    break;
                }

                case "Calabresa":
                {
                    // tábua com alça, duas linguiças amarradas, uma cortada mostrando o miolo, rodelas e faca
                    var tabua = Mat("Madeira", new Color(0.55f, 0.38f, 0.25f));
                    Bloco("Tabua", t, topo + new Vector3(-0.02f, 0.012f, 0), new Vector3(0.56f, 0.024f, 0.38f), tabua, true);
                    Prim(PrimitiveType.Cylinder, "Alca Tabua", t, topo + new Vector3(0.29f, 0.012f, 0), new Vector3(0.1f, 0.012f, 0.1f), tabua);
                    var barbante = Mat("Barbante", new Color(0.85f, 0.75f, 0.55f));
                    var gordura = Mat("Gordura", new Color(0.97f, 0.82f, 0.78f), 0.5f);
                    var miolo = Mat("Miolo Calabresa", Color.Lerp(corBase, new Color(1f, 0.6f, 0.55f), 0.35f), 0.4f);
                    foreach (var (z, giro, cortada) in new[] { (-0.09f, 12f, false), (0.06f, -8f, true) })
                    {
                        var c = topo + new Vector3(-0.1f, 0.06f, z);
                        var r = Quaternion.Euler(0, giro, 90);
                        Prim(PrimitiveType.Capsule, "Calabresa", t, c, new Vector3(0.075f, 0.17f, 0.075f), cor, r);
                        var eixo = r * Vector3.up;
                        for (int k = 0; k < 6; k++)
                            Prim(PrimitiveType.Sphere, "Gordura", t, c + eixo * (-0.1f + k * 0.04f) + Vector3.up * 0.036f, new Vector3(0.008f, 0.004f, 0.008f), gordura);
                        Prim(PrimitiveType.Sphere, "No", t, c - eixo * 0.175f, new Vector3(0.02f, 0.015f, 0.015f), barbante);
                        if (cortada)
                            Prim(PrimitiveType.Cylinder, "Corte", t, c + eixo * 0.168f, new Vector3(0.07f, 0.003f, 0.07f), miolo, r);
                        else
                            Prim(PrimitiveType.Sphere, "No", t, c + eixo * 0.175f, new Vector3(0.02f, 0.015f, 0.015f), barbante);
                    }
                    for (int i = 0; i < 6; i++)
                    {
                        var pos = topo + new Vector3(0.12f + (i % 2) * 0.075f, 0.028f + (i / 2) * 0.003f, -0.1f + (i / 2) * 0.08f);
                        Prim(PrimitiveType.Cylinder, "Rodela", t, pos, new Vector3(0.068f, 0.004f, 0.068f), cor);
                        Prim(PrimitiveType.Cylinder, "Miolo", t, pos + Vector3.up * 0.004f, new Vector3(0.058f, 0.001f, 0.058f), miolo);
                        for (int k = 0; k < 3; k++)
                            Prim(PrimitiveType.Sphere, "Gordura", t, pos + Quaternion.Euler(0, k * 120f + i * 40f, 0) * new Vector3(0.014f, 0, 0) + Vector3.up * 0.0055f, new Vector3(0.008f, 0.002f, 0.008f), gordura);
                    }
                    // faca
                    var fr = Quaternion.Euler(0, -25, 0);
                    var faca = topo + new Vector3(0.05f, 0.027f, 0.15f);
                    Prim(PrimitiveType.Cube, "Lamina", t, faca, new Vector3(0.18f, 0.003f, 0.035f), MMetal, fr);
                    Prim(PrimitiveType.Cube, "Cabo Faca", t, faca + fr * new Vector3(-0.14f, 0.004f, 0), new Vector3(0.1f, 0.016f, 0.025f), MEscura, fr);
                    break;
                }

                case "Cebola":
                {
                    // tigela de anéis, cebola inteira com listras e meia cebola mostrando as camadas
                    var centro = topo + new Vector3(-0.08f, 0, 0);
                    Malha(MalhasProcedurais.Tigela, "Tigela", t, centro, Vector3.one * 0.42f, MCeramica);
                    var roxo = Mat("Cebola Roxa", new Color(0.62f, 0.32f, 0.48f), 0.5f);
                    for (int i = 0; i < 16; i++)
                        Malha(MalhasProcedurais.Anel(0.09f), "Anel de Cebola", t, centro + new Vector3(R(-0.1f, 0.1f), 0.06f + i * 0.0045f, R(-0.1f, 0.1f)),
                              new Vector3(R(0.06f, 0.1f), 0.04f, R(0.06f, 0.1f)), i % 4 == 0 ? roxo : cor, Quaternion.Euler(R(-20, 20), R(0, 180), R(-20, 20)));
                    var inteira = topo + new Vector3(0.24f, 0.06f, -0.12f);
                    Prim(PrimitiveType.Sphere, "Cebola", t, inteira, new Vector3(0.12f, 0.11f, 0.12f), roxo);
                    for (int i = 0; i < 4; i++)
                        Prim(PrimitiveType.Sphere, "Listra", t, inteira, new Vector3(0.121f, 0.111f, 0.01f), Mat("Cebola Listra", new Color(0.48f, 0.22f, 0.38f), 0.5f), Quaternion.Euler(0, i * 45f, 0));
                    Prim(PrimitiveType.Capsule, "Ponta", t, inteira + Vector3.up * 0.065f, new Vector3(0.02f, 0.02f, 0.02f), Mat("Casca Seca", new Color(0.65f, 0.5f, 0.35f)));
                    Prim(PrimitiveType.Sphere, "Raiz", t, inteira - Vector3.up * 0.054f, new Vector3(0.03f, 0.01f, 0.03f), Mat("Casca Seca", new Color(0.65f, 0.5f, 0.35f)));
                    var meia = topo + new Vector3(0.24f, 0.022f, 0.12f);
                    Prim(PrimitiveType.Cylinder, "Meia Cebola", t, meia, new Vector3(0.11f, 0.022f, 0.11f), roxo);
                    Prim(PrimitiveType.Cylinder, "Corte", t, meia + Vector3.up * 0.0225f, new Vector3(0.1f, 0.001f, 0.1f), cor);
                    for (int k = 1; k <= 3; k++)
                        Malha(MalhasProcedurais.Anel(0.04f), "Camada", t, meia + Vector3.up * 0.023f, new Vector3(0.025f * k, 0.01f, 0.025f * k), Mat("Cebola Camada", new Color(0.82f, 0.68f, 0.82f), 0.6f));
                    break;
                }

                case "Azeitona":
                {
                    // tigela de azeitonas (algumas recheadas com pimentão) e um pires de azeite
                    var centro = topo + new Vector3(-0.06f, 0, 0);
                    Malha(MalhasProcedurais.Tigela, "Tigela", t, centro, Vector3.one * 0.42f, MTerracota);
                    var pimentao = Mat("Pimentao", new Color(0.85f, 0.2f, 0.12f), 0.6f);
                    var verdes = Mat("Azeitona Verde", new Color(0.45f, 0.5f, 0.18f), 0.75f);
                    for (int i = 0; i < 22; i++)
                    {
                        var r = Quaternion.Euler(R(-20, 20), R(0, 360), 0);
                        var pos = centro + new Vector3(R(-0.12f, 0.12f), R(0.1f, 0.135f), R(-0.12f, 0.12f));
                        Prim(PrimitiveType.Sphere, "Azeitona", t, pos, new Vector3(0.036f, 0.032f, 0.048f), i % 5 == 4 ? verdes : cor, r);
                        if (i % 3 == 0) Prim(PrimitiveType.Sphere, "Pimentao", t, pos + r * new Vector3(0, 0, 0.022f), Vector3.one * 0.013f, pimentao);
                    }
                    var pires = topo + new Vector3(0.24f, 0, 0.11f);
                    Malha(MalhasProcedurais.Prato, "Pires", t, pires, Vector3.one * 0.18f, MCeramica);
                    Prim(PrimitiveType.Cylinder, "Azeite", t, pires + Vector3.up * 0.008f, new Vector3(0.11f, 0.002f, 0.11f), Mat("Azeite", new Color(0.75f, 0.65f, 0.15f), 0.95f));
                    Malha(MalhasProcedurais.Garrafa, "Galheteiro", t, topo + new Vector3(0.26f, 0, -0.12f), new Vector3(0.07f, 0.2f, 0.07f), Mat("Vidro Azeite", new Color(0.35f, 0.42f, 0.10f), 0.9f));
                    break;
                }

                default:
                    Bloco("Conteudo", t, topo + new Vector3(0, 0.02f, 0), new Vector3(0.62f, 0.04f, 0.42f), cor, true);
                    break;
            }
        }

        // ------------------------------------------------------------------ Bancadas

        /// <summary>Portas almofadadas com puxador de barra, borda boleada no mármore e utensílios entre as estações.</summary>
        private static void DetalharBancadaFundo(Transform raiz, float inicio, float esp, int estacoes)
        {
            float hz = Profundidade / 2f, zf = hz - 0.85f;
            var porta = MClara;
            for (int i = 0; i < estacoes; i++)
            {
                float x = inicio + i * esp;
                Bloco("Porta", raiz, new Vector3(x, 0.47f, zf - 0.007f), new Vector3(esp - 0.1f, 0.66f, 0.014f), porta, true);
                Bloco("Almofada", raiz, new Vector3(x, 0.47f, zf - 0.016f), new Vector3(esp - 0.32f, 0.44f, 0.006f), Mat("Madeira", new Color(0.55f, 0.38f, 0.25f)), true);
                Prim(PrimitiveType.Cylinder, "Puxador", raiz, new Vector3(x, 0.74f, zf - 0.045f), new Vector3(0.016f, 0.09f, 0.016f), MMetal, Quaternion.Euler(0, 0, 90));
                foreach (float s in new[] { -0.07f, 0.07f })
                    Prim(PrimitiveType.Cube, "Suporte", raiz, new Vector3(x + s, 0.74f, zf - 0.028f), new Vector3(0.012f, 0.012f, 0.035f), MMetal);
            }
            Prim(PrimitiveType.Cylinder, "Borda Boleada", raiz, new Vector3(0, AlturaBancada - 0.02f, hz - 0.9f), new Vector3(0.042f, (Largura - 0.36f) / 2f, 0.042f), MMarmore, Quaternion.Euler(0, 0, 90));

            // utensílios nos vãos entre as estações, encostados no azulejo
            float y = AlturaBancada, z = hz - 0.2f;
            for (int i = 0; i < estacoes - 1; i++)
            {
                var p = new Vector3(inicio + (i + 0.5f) * esp, y, z);
                switch (i % 7)
                {
                    case 0: // saleiro e pimenteiro
                        Malha(MalhasProcedurais.Pote, "Saleiro", raiz, p + new Vector3(-0.04f, 0, 0), new Vector3(0.05f, 0.09f, 0.05f), MCeramica);
                        Malha(MalhasProcedurais.Pote, "Pimenteiro", raiz, p + new Vector3(0.04f, 0, 0), new Vector3(0.05f, 0.09f, 0.05f), Mat("Ceramica Preta", new Color(0.12f, 0.12f, 0.13f), 0.7f));
                        break;
                    case 1: // pote de utensílios com colheres de pau
                        Malha(MalhasProcedurais.Pote, "Pote Utensilios", raiz, p, new Vector3(0.12f, 0.16f, 0.12f), MTerracota);
                        for (int k = 0; k < 3; k++)
                        {
                            var r = Quaternion.Euler(R(-12, 12), 0, -15f + k * 15f);
                            var b = p + new Vector3(-0.02f + k * 0.02f, 0.14f, 0);
                            Prim(PrimitiveType.Cylinder, "Colher de Pau", raiz, b + r * new Vector3(0, 0.07f, 0), new Vector3(0.014f, 0.1f, 0.014f), MClara, r);
                            Prim(PrimitiveType.Sphere, "Ponta", raiz, b + r * new Vector3(0, 0.18f, 0), new Vector3(0.04f, 0.055f, 0.015f), MClara, r);
                        }
                        break;
                    case 2: // azeite e vinagre
                        Malha(MalhasProcedurais.Garrafa, "Azeite", raiz, p + new Vector3(-0.04f, 0, 0), new Vector3(0.065f, 0.22f, 0.065f), Mat("Vidro Azeite", new Color(0.35f, 0.42f, 0.10f), 0.9f));
                        Malha(MalhasProcedurais.Garrafa, "Vinagre", raiz, p + new Vector3(0.04f, 0, 0.01f), new Vector3(0.055f, 0.18f, 0.055f), Mat("Vidro Vinho", new Color(0.25f, 0.05f, 0.08f), 0.9f));
                        break;
                    case 3: // cabeças de alho num pratinho
                        Malha(MalhasProcedurais.Prato, "Pratinho", raiz, p, Vector3.one * 0.16f, MCeramica);
                        for (int k = 0; k < 3; k++)
                        {
                            var a = p + new Vector3(-0.03f + k * 0.03f, 0.03f + (k == 1 ? 0.015f : 0), (k - 1) * 0.01f);
                            Prim(PrimitiveType.Sphere, "Alho", raiz, a, new Vector3(0.045f, 0.04f, 0.045f), Mat("Alho", new Color(0.96f, 0.93f, 0.86f), 0.4f));
                            Prim(PrimitiveType.Capsule, "Ponta Alho", raiz, a + Vector3.up * 0.022f, new Vector3(0.008f, 0.01f, 0.008f), Mat("Alho", new Color(0.96f, 0.93f, 0.86f), 0.4f));
                        }
                        break;
                    case 4: // moedor de pimenta
                        Prim(PrimitiveType.Cylinder, "Moedor", raiz, p + Vector3.up * 0.09f, new Vector3(0.05f, 0.09f, 0.05f), MEscura);
                        Prim(PrimitiveType.Cylinder, "Anel Moedor", raiz, p + Vector3.up * 0.13f, new Vector3(0.054f, 0.006f, 0.054f), MMetal);
                        Prim(PrimitiveType.Sphere, "Pomo", raiz, p + Vector3.up * 0.19f, Vector3.one * 0.035f, MEscura);
                        break;
                    case 5: // três potinhos de ervas secas
                        for (int k = 0; k < 3; k++)
                        {
                            var q = p + new Vector3(-0.05f + k * 0.05f, 0, 0);
                            Malha(MalhasProcedurais.Pote, "Ervas", raiz, q, new Vector3(0.045f, 0.08f, 0.045f), Mat("Vidro", new Color(0.75f, 0.85f, 0.85f), 0.95f));
                            Prim(PrimitiveType.Cylinder, "Ervas Secas", raiz, q + Vector3.up * 0.03f, new Vector3(0.04f, 0.028f, 0.04f), Mat("Ervas Secas", new Color(0.4f, 0.45f, 0.2f)));
                            Prim(PrimitiveType.Cylinder, "Tampa", raiz, q + Vector3.up * 0.078f, new Vector3(0.04f, 0.006f, 0.04f), Mat("Tampa", CorVerdeEsmalte, 0.6f, 0.3f));
                        }
                        break;
                    default: // tábua apoiada no azulejo
                        Prim(PrimitiveType.Cube, "Tabua Apoiada", raiz, p + new Vector3(0, 0.15f, 0.04f), new Vector3(0.22f, 0.3f, 0.02f), MClara, Quaternion.Euler(-12, 0, 0));
                        Prim(PrimitiveType.Cylinder, "Furo", raiz, p + new Vector3(0, 0.27f, 0.015f), new Vector3(0.04f, 0.012f, 0.04f), Mat("Sombra", new Color(0.12f, 0.08f, 0.05f)), Quaternion.Euler(78, 0, 0));
                        break;
                }
            }
        }

        /// <summary>Ilha de montagem: gavetas e portas almofadadas, borda boleada e utensílios de pizzaiolo.</summary>
        private static void DetalharIlha(Transform deco)
        {
            const float zc = 0.8f, hxI = 1.1f, hzI = 0.55f;
            float zFrente = zc - hzI, zTras = zc + hzI, topo = AlturaBancada + 0.04f;
            var porta = MClara;
            foreach (float x in new[] { -0.73f, 0f, 0.73f })
            {
                // frente (lado do jogador): gaveta em cima, porta embaixo
                Bloco("Gaveta", deco, new Vector3(x, 0.76f, zFrente - 0.007f), new Vector3(0.66f, 0.2f, 0.014f), porta, true);
                Prim(PrimitiveType.Cylinder, "Puxador Gaveta", deco, new Vector3(x, 0.76f, zFrente - 0.035f), new Vector3(0.016f, 0.1f, 0.016f), MMetal, Quaternion.Euler(0, 0, 90));
                Bloco("Porta", deco, new Vector3(x, 0.36f, zFrente - 0.007f), new Vector3(0.66f, 0.52f, 0.014f), porta, true);
                Bloco("Almofada", deco, new Vector3(x, 0.36f, zFrente - 0.016f), new Vector3(0.48f, 0.36f, 0.006f), Mat("Madeira", new Color(0.55f, 0.38f, 0.25f)), true);
                Prim(PrimitiveType.Sphere, "Puxador Porta", deco, new Vector3(x + 0.25f, 0.55f, zFrente - 0.03f), Vector3.one * 0.035f, MMetal);
                // trás
                Bloco("Porta Tras", deco, new Vector3(x, 0.45f, zTras + 0.007f), new Vector3(0.66f, 0.72f, 0.014f), porta, true);
                Prim(PrimitiveType.Sphere, "Puxador Tras", deco, new Vector3(x - 0.25f, 0.6f, zTras + 0.03f), Vector3.one * 0.035f, MMetal);
            }
            foreach (float s in new[] { -1f, 1f })
                Bloco("Lateral", deco, new Vector3(s * (hxI + 0.007f), 0.45f, zc), new Vector3(0.014f, 0.72f, 0.9f), porta, true);
            Bloco("Rodape Ilha", deco, new Vector3(0, 0.04f, zFrente - 0.006f), new Vector3(2.2f, 0.08f, 0.012f), MEscura, true);

            // borda boleada do tampo (frente, trás e laterais)
            Prim(PrimitiveType.Cylinder, "Borda", deco, new Vector3(0, AlturaBancada + 0.02f, zc - 0.6f), new Vector3(0.042f, 1.15f, 0.042f), MMarmore, Quaternion.Euler(0, 0, 90));
            Prim(PrimitiveType.Cylinder, "Borda", deco, new Vector3(0, AlturaBancada + 0.02f, zc + 0.6f), new Vector3(0.042f, 1.15f, 0.042f), MMarmore, Quaternion.Euler(0, 0, 90));
            Prim(PrimitiveType.Cylinder, "Borda", deco, new Vector3(-1.15f, AlturaBancada + 0.02f, zc), new Vector3(0.042f, 0.6f, 0.042f), MMarmore, Quaternion.Euler(90, 0, 0));
            Prim(PrimitiveType.Cylinder, "Borda", deco, new Vector3(1.15f, AlturaBancada + 0.02f, zc), new Vector3(0.042f, 0.6f, 0.042f), MMarmore, Quaternion.Euler(90, 0, 0));

            // Utensílios (o centro fica livre para a pizza)
            var rr = Quaternion.Euler(0, 15, 90);
            var rolo = new Vector3(-0.82f, topo + 0.028f, 1.15f);
            Prim(PrimitiveType.Cylinder, "Rolo de Massa", deco, rolo, new Vector3(0.055f, 0.14f, 0.055f), MClara, rr);
            foreach (float s in new[] { -1f, 1f })
                Prim(PrimitiveType.Cylinder, "Cabo Rolo", deco, rolo + rr * new Vector3(0, s * 0.19f, 0), new Vector3(0.025f, 0.05f, 0.025f), MEscura, rr);

            Prim(PrimitiveType.Cube, "Espatula", deco, new Vector3(-0.78f, topo + 0.002f, 0.45f), new Vector3(0.13f, 0.003f, 0.1f), MMetal, Quaternion.Euler(0, -20, 0));
            Prim(PrimitiveType.Cylinder, "Pegador", deco, new Vector3(-0.78f, topo + 0.012f, 0.4f) + Quaternion.Euler(0, -20, 0) * new Vector3(0, 0, -0.005f), new Vector3(0.022f, 0.065f, 0.022f), MClara, Quaternion.Euler(0, -20, 90));

            Malha(MalhasProcedurais.Pote, "Polvilhador", deco, new Vector3(-0.98f, topo, 0.62f), new Vector3(0.07f, 0.11f, 0.07f), MMetal);
            Prim(PrimitiveType.Sphere, "Tampa Polvilhador", deco, new Vector3(-0.98f, topo + 0.1f, 0.62f), new Vector3(0.06f, 0.035f, 0.06f), MMetal);

            Malha(MalhasProcedurais.Tigela, "Tigela Farinha", deco, new Vector3(0.85f, topo, 1.15f), Vector3.one * 0.28f, MCeramica);
            Prim(PrimitiveType.Sphere, "Farinha", deco, new Vector3(0.85f, topo + 0.07f, 1.15f), new Vector3(0.22f, 0.07f, 0.22f), MFarinha);
            Malha(MalhasProcedurais.Garrafa, "Azeite", deco, new Vector3(0.98f, topo, 0.45f), new Vector3(0.075f, 0.26f, 0.075f), Mat("Vidro Azeite", new Color(0.35f, 0.42f, 0.10f), 0.9f));
            Prim(PrimitiveType.Cylinder, "Rolha", deco, new Vector3(0.98f, topo + 0.27f, 0.45f), new Vector3(0.025f, 0.015f, 0.025f), MClara);

            // carretilha (cortador de pizza)
            var cr = Quaternion.Euler(0, 30, 0);
            var carretilha = new Vector3(0.68f, topo + 0.004f, 0.42f);
            Prim(PrimitiveType.Cylinder, "Disco", deco, carretilha, new Vector3(0.08f, 0.002f, 0.08f), MMetal, cr);
            Prim(PrimitiveType.Cylinder, "Cabo Carretilha", deco, carretilha + cr * new Vector3(0.1f, 0.008f, 0), new Vector3(0.022f, 0.06f, 0.022f), Mat("Cabo Vermelho", CorTomate, 0.5f), cr * Quaternion.Euler(0, 0, 90));

            // concha no copo de molho de reserva
            Malha(MalhasProcedurais.Pote, "Copo Concha", deco, new Vector3(0.55f, topo, 1.22f), new Vector3(0.09f, 0.1f, 0.09f), MCeramica);
            Prim(PrimitiveType.Cylinder, "Cabo Concha", deco, new Vector3(0.56f, topo + 0.15f, 1.23f), new Vector3(0.012f, 0.08f, 0.012f), MMetal, Quaternion.Euler(10, 0, -12));

            // porta-receita: bloquinho de madeira com cartão inclinado
            Prim(PrimitiveType.Cube, "Suporte Receita", deco, new Vector3(-0.6f, topo + 0.015f, 1.25f), new Vector3(0.12f, 0.03f, 0.05f), MEscura);
            Prim(PrimitiveType.Cube, "Cartao Receita", deco, new Vector3(-0.6f, topo + 0.08f, 1.26f), new Vector3(0.14f, 0.1f, 0.003f), Mat("Papel", new Color(0.98f, 0.95f, 0.86f)), Quaternion.Euler(-15, 180, 0));
            Prim(PrimitiveType.Cube, "Faixa Cartao", deco, new Vector3(-0.6f, topo + 0.115f, 1.252f), new Vector3(0.14f, 0.015f, 0.004f), Mat("Selo", CorTomate), Quaternion.Euler(-15, 180, 0));
        }

        /// <summary>Balcão de entrega: almofadas do lado da cozinha, borda boleada e itens de atendimento.</summary>
        private static void DetalharBalcao(Transform deco)
        {
            float hz = Profundidade / 2f, zCozinha = -hz + 0.4f, yTopo = 1.1f;
            foreach (float x in new[] { -1f, 0f, 1f })
            {
                Bloco("Almofada Balcao", deco, new Vector3(x, 0.52f, zCozinha + 0.007f), new Vector3(0.9f, 0.8f, 0.014f), MClara, true);
                Bloco("Almofada Interna", deco, new Vector3(x, 0.52f, zCozinha + 0.016f), new Vector3(0.7f, 0.6f, 0.006f), Mat("Madeira", new Color(0.55f, 0.38f, 0.25f)), true);
            }
            Bloco("Rodape Balcao", deco, new Vector3(0, 0.04f, zCozinha + 0.01f), new Vector3(3f, 0.08f, 0.012f), MEscura, true);
            Prim(PrimitiveType.Cylinder, "Borda", deco, new Vector3(0, 1.08f, -hz + 0.45f), new Vector3(0.042f, 1.55f, 0.042f), MMarmore, Quaternion.Euler(0, 0, 90));
            Prim(PrimitiveType.Cylinder, "Borda", deco, new Vector3(0, 1.08f, -hz - 0.25f), new Vector3(0.042f, 1.55f, 0.042f), MMarmore, Quaternion.Euler(0, 0, 90));

            // espeto de comandas com papeizinhos
            var espeto = new Vector3(-1.38f, yTopo, -hz + 0.25f);
            Prim(PrimitiveType.Cylinder, "Base Espeto", deco, espeto + Vector3.up * 0.004f, new Vector3(0.07f, 0.004f, 0.07f), Mat("Ferro", new Color(0.12f, 0.12f, 0.12f), 0.4f, 0.6f));
            Prim(PrimitiveType.Cylinder, "Espeto", deco, espeto + Vector3.up * 0.06f, new Vector3(0.006f, 0.06f, 0.006f), MMetal);
            for (int i = 0; i < 4; i++)
                Prim(PrimitiveType.Cube, "Comanda", deco, espeto + Vector3.up * (0.015f + i * 0.012f), new Vector3(0.07f, 0.002f, 0.1f), Mat("Papel", new Color(0.98f, 0.95f, 0.86f)), Quaternion.Euler(R(-8, 8), R(0, 360), R(-8, 8)));

            // porta-guardanapos
            var pg = new Vector3(-1.36f, yTopo, -hz - 0.08f);
            Prim(PrimitiveType.Cube, "Base Guardanapo", deco, pg + Vector3.up * 0.004f, new Vector3(0.12f, 0.008f, 0.07f), MMetal);
            foreach (float s in new[] { -1f, 1f })
                Prim(PrimitiveType.Cube, "Lado", deco, pg + new Vector3(0, 0.045f, s * 0.032f), new Vector3(0.12f, 0.08f, 0.004f), MMetal);
            Prim(PrimitiveType.Cube, "Guardanapos", deco, pg + Vector3.up * 0.05f, new Vector3(0.11f, 0.09f, 0.055f), Mat("Guardanapo", new Color(0.98f, 0.98f, 0.96f)));

            // cardápio de mesa (cavalete) virado para o salão
            var card = new Vector3(1.32f, yTopo, -hz - 0.15f);
            foreach (float s in new[] { -1f, 1f })
                Prim(PrimitiveType.Cube, "Cardapio", deco, card + new Vector3(0, 0.06f, s * 0.018f), new Vector3(0.1f, 0.13f, 0.003f), Mat("Papel", new Color(0.98f, 0.95f, 0.86f)), Quaternion.Euler(s * 16f, 0, 0));
            Prim(PrimitiveType.Cube, "Faixa Cardapio", deco, card + new Vector3(0, 0.1f, -0.03f), new Vector3(0.1f, 0.02f, 0.004f), Mat("Selo", CorTomate), Quaternion.Euler(-16, 0, 0));
        }

        // ------------------------------------------------------------------ Personagens

        private struct Visual
        {
            public Color camisa, calca, pele, cabelo, boinaCor;
            public bool garcom, oculos, barba, boina, mangaCurta;
            public int estiloCabelo;
        }

        private static readonly Color[] CoresPele = { new Color(0.96f, 0.80f, 0.66f), new Color(0.87f, 0.66f, 0.50f), new Color(0.66f, 0.46f, 0.32f), new Color(0.45f, 0.30f, 0.22f) };
        private static readonly Color[] CoresCabelo = { new Color(0.10f, 0.08f, 0.07f), new Color(0.35f, 0.22f, 0.12f), new Color(0.85f, 0.68f, 0.38f), new Color(0.62f, 0.60f, 0.58f), new Color(0.62f, 0.25f, 0.12f) };
        private static readonly Color[] CoresCamisa = { new Color(0.86f, 0.62f, 0.20f), new Color(0.20f, 0.50f, 0.52f), new Color(0.90f, 0.45f, 0.38f), new Color(0.20f, 0.27f, 0.45f), new Color(0.45f, 0.52f, 0.28f), new Color(0.62f, 0.50f, 0.70f), new Color(0.92f, 0.88f, 0.80f) };
        private static readonly Color[] CoresCalca = { new Color(0.22f, 0.30f, 0.45f), new Color(0.35f, 0.26f, 0.18f), new Color(0.30f, 0.30f, 0.32f), new Color(0.15f, 0.15f, 0.17f) };

        private static void Cliente(Transform pai, Vector3 posCadeira, Quaternion rot, int semente)
        {
            int estilo = semente * 3 % 5;
            var v = new Visual
            {
                camisa = CoresCamisa[semente * 3 % CoresCamisa.Length],
                calca = CoresCalca[semente % CoresCalca.Length],
                pele = CoresPele[semente * 7 % CoresPele.Length],
                cabelo = CoresCabelo[semente * 5 % CoresCabelo.Length],
                estiloCabelo = estilo,
                oculos = semente % 4 == 1,
                barba = semente % 5 == 2 && estilo != 1 && estilo != 2,
                boina = semente % 6 == 3,
                boinaCor = new Color(0.55f, 0.15f, 0.15f),
                mangaCurta = semente % 3 == 0
            };
            var raiz = new GameObject("Cliente").transform;
            raiz.SetParent(pai);
            raiz.position = posCadeira;
            raiz.rotation = rot; // a cadeira tem o encosto em -Z: o cliente olha para +Z da cadeira (a mesa)
            Boneco(raiz, v, true, out var tronco, out var cabeca, out var olhos, out var ombroD);

            // metade dos clientes segura um copo (o gesto vira "beber")
            if (semente % 2 == 0)
            {
                var copo = new GameObject("Copo").transform;
                copo.SetParent(ombroD, false);
                copo.localPosition = new Vector3(0, -0.6f, 0.03f);
                copo.localRotation = Quaternion.Euler(55f, 0, 0); // compensa o braço inclinado: copo em pé
                Peca(PrimitiveType.Cylinder, "Vidro", copo, Vector3.zero, new Vector3(0.055f, 0.05f, 0.055f), Mat("Vidro", new Color(0.75f, 0.85f, 0.85f), 0.95f));
                var bebida = semente % 4 == 0 ? new Color(0.45f, 0.06f, 0.1f) : new Color(0.95f, 0.6f, 0.15f);
                Peca(PrimitiveType.Cylinder, "Bebida", copo, new Vector3(0, -0.01f, 0), new Vector3(0.05f, 0.035f, 0.05f), Mat("Bebida_" + ColorUtility.ToHtmlStringRGB(bebida), bebida, 0.9f));
            }

            var ocioso = raiz.gameObject.AddComponent<PersonagemOcioso>();
            Atribuir(ocioso, "tronco", tronco);
            Atribuir(ocioso, "cabeca", cabeca);
            Atribuir(ocioso, "bracoGesto", ombroD);
            AtribuirLista(ocioso, "olhos", olhos);
        }

        /// <summary>
        /// Personagem low-poly olhando para +Z local, sentado ou em pé: rosto com olhos (que piscam), sobrancelhas,
        /// orelhas, nariz, boca e bochechas; 5 cortes de cabelo; óculos, barba e boina opcionais; gola, botões e cinto.
        /// </summary>
        private static Transform Boneco(Transform raiz, Visual v, bool sentado, out Transform tronco, out Transform cabeca, out Transform[] olhos, out Transform ombroD)
        {
            var corpo = new GameObject("Corpo").transform;
            corpo.SetParent(raiz, false);

            var camisa = Mat("Roupa_" + ColorUtility.ToHtmlStringRGB(v.camisa), v.camisa, 0.2f);
            var calca = Mat("Roupa_" + ColorUtility.ToHtmlStringRGB(v.calca), v.calca, 0.2f);
            var pele = Mat("Pele_" + ColorUtility.ToHtmlStringRGB(v.pele), v.pele, 0.35f);
            var cabelo = Mat("Cabelo_" + ColorUtility.ToHtmlStringRGB(v.cabelo), v.cabelo, 0.35f);
            var sapato = Mat("Sapato", new Color(0.12f, 0.09f, 0.08f), 0.6f);
            var sola = Mat("Sola", new Color(0.3f, 0.22f, 0.16f));
            var branco = Mat("Manga Branca", new Color(0.97f, 0.97f, 0.97f));
            var detalhe = Mat("Roupa_" + ColorUtility.ToHtmlStringRGB(v.camisa * 0.78f), v.camisa * 0.78f, 0.2f);
            var mangaMat = v.garcom ? branco : camisa;

            void Sapato(Vector3 p)
            {
                Peca(PrimitiveType.Sphere, "Sapato", corpo, p + new Vector3(0, 0.04f, 0.03f), new Vector3(0.12f, 0.08f, 0.22f), sapato);
                Peca(PrimitiveType.Cube, "Sola", corpo, p + new Vector3(0, 0.008f, 0.03f), new Vector3(0.115f, 0.016f, 0.21f), sola);
            }

            float baseTronco;
            if (sentado)
            {
                baseTronco = 0.47f;
                foreach (float x in new[] { -0.1f, 0.1f })
                {
                    Peca(PrimitiveType.Capsule, "Coxa", corpo, new Vector3(x, 0.53f, 0.17f), new Vector3(0.15f, 0.21f, 0.15f), calca, new Vector3(90, 0, 0));
                    Peca(PrimitiveType.Capsule, "Canela", corpo, new Vector3(x, 0.27f, 0.36f), new Vector3(0.13f, 0.24f, 0.13f), calca);
                    Sapato(new Vector3(x, 0f, 0.37f));
                }
            }
            else
            {
                baseTronco = 0.82f;
                foreach (float x in new[] { -0.1f, 0.1f })
                {
                    Peca(PrimitiveType.Capsule, "Perna", corpo, new Vector3(x, 0.45f, 0), new Vector3(0.15f, 0.4f, 0.15f), calca);
                    Sapato(new Vector3(x, 0f, 0f));
                }
            }

            tronco = new GameObject("Tronco").transform;
            tronco.SetParent(corpo, false);
            tronco.localPosition = new Vector3(0, baseTronco, 0);
            Peca(PrimitiveType.Capsule, "Torso", tronco, new Vector3(0, 0.38f, 0), new Vector3(0.44f, 0.4f, 0.3f), camisa);
            Peca(PrimitiveType.Cylinder, "Cinto", tronco, new Vector3(0, 0.06f, 0), new Vector3(0.43f, 0.025f, 0.3f), Mat("Cinto", new Color(0.2f, 0.12f, 0.07f), 0.5f));
            Peca(PrimitiveType.Cube, "Fivela", tronco, new Vector3(0, 0.06f, 0.15f), new Vector3(0.05f, 0.04f, 0.01f), Mat("Latao", new Color(0.85f, 0.65f, 0.3f), 0.8f, 1f));

            if (v.garcom)
            {
                // colete preto sobre camisa branca, gravata-borboleta, avental comprido com bloquinho
                Peca(PrimitiveType.Cube, "Camisa", tronco, new Vector3(0, 0.6f, 0.13f), new Vector3(0.14f, 0.24f, 0.05f), branco);
                Peca(PrimitiveType.Cylinder, "Gola", tronco, new Vector3(0, 0.76f, 0), new Vector3(0.15f, 0.025f, 0.13f), branco);
                var vermelho = Mat("Gravata", new Color(0.7f, 0.1f, 0.1f));
                foreach (float s in new[] { -1f, 1f })
                    Peca(PrimitiveType.Cube, "Laco", tronco, new Vector3(s * 0.028f, 0.71f, 0.16f), new Vector3(0.04f, 0.035f, 0.015f), vermelho, new Vector3(0, 0, 45));
                Peca(PrimitiveType.Cube, "No", tronco, new Vector3(0, 0.71f, 0.165f), new Vector3(0.02f, 0.02f, 0.02f), vermelho);
                for (int i = 0; i < 3; i++)
                    Peca(PrimitiveType.Sphere, "Botao Colete", tronco, new Vector3(0, 0.44f - i * 0.08f, 0.152f), Vector3.one * 0.018f, Mat("Latao", new Color(0.85f, 0.65f, 0.3f), 0.8f, 1f));
                Peca(PrimitiveType.Cube, "Avental", tronco, new Vector3(0, -0.2f, 0.155f), new Vector3(0.38f, 0.55f, 0.015f), branco);
                Peca(PrimitiveType.Cube, "Bolso", tronco, new Vector3(0.08f, -0.08f, 0.165f), new Vector3(0.12f, 0.08f, 0.006f), Mat("Bolso", new Color(0.88f, 0.88f, 0.86f)));
                Peca(PrimitiveType.Cube, "Bloquinho", tronco, new Vector3(0.08f, -0.04f, 0.165f), new Vector3(0.07f, 0.06f, 0.012f), Mat("Papel", new Color(0.98f, 0.95f, 0.86f)));
            }
            else
            {
                Peca(PrimitiveType.Cylinder, "Gola", tronco, new Vector3(0, 0.76f, 0), new Vector3(0.16f, 0.025f, 0.14f), detalhe);
                for (int i = 0; i < 3; i++)
                    Peca(PrimitiveType.Sphere, "Botao", tronco, new Vector3(0, 0.62f - i * 0.12f, 0.15f), Vector3.one * 0.018f, detalhe);
                Peca(PrimitiveType.Cube, "Bolso", tronco, new Vector3(-0.09f, 0.56f, 0.143f), new Vector3(0.08f, 0.07f, 0.01f), detalhe);
            }

            // Braços: pivô no ombro. Sentado, os braços vão para a frente (apoiados na mesa).
            ombroD = null;
            foreach (float s in new[] { -1f, 1f })
            {
                var ombro = new GameObject(s < 0 ? "Ombro E" : "Ombro D").transform;
                ombro.SetParent(tronco, false);
                ombro.localPosition = new Vector3(0.26f * s, 0.66f, 0);
                float angulo = sentado ? -55f : (v.garcom && s > 0 ? -80f : 0f);
                ombro.localRotation = Quaternion.Euler(angulo, 0, s * (sentado ? 8f : 6f));
                Peca(PrimitiveType.Sphere, "Ombro", ombro, Vector3.zero, Vector3.one * 0.13f, mangaMat);
                Peca(PrimitiveType.Capsule, "Braco", ombro, new Vector3(0, -0.15f, 0), new Vector3(0.12f, 0.16f, 0.12f), mangaMat);
                bool antebracoPele = v.mangaCurta && !v.garcom;
                Peca(PrimitiveType.Capsule, "Antebraco", ombro, new Vector3(0, -0.4f, 0.01f), new Vector3(0.105f, 0.15f, 0.105f), antebracoPele ? pele : mangaMat);
                if (!antebracoPele)
                    Peca(PrimitiveType.Cylinder, "Punho", ombro, new Vector3(0, -0.52f, 0.01f), new Vector3(0.11f, 0.015f, 0.11f), v.garcom ? branco : detalhe);
                Peca(PrimitiveType.Sphere, "Mao", ombro, new Vector3(0, -0.58f, 0.01f), new Vector3(0.085f, 0.1f, 0.065f), pele);
                Peca(PrimitiveType.Capsule, "Polegar", ombro, new Vector3(-0.035f * s, -0.56f, 0.04f), new Vector3(0.025f, 0.03f, 0.025f), pele, new Vector3(30, 0, 0));
                if (s > 0) ombroD = ombro;
                // garçom: pano de prato no braço esquerdo
                if (v.garcom && s < 0)
                    Peca(PrimitiveType.Cube, "Pano", ombro, new Vector3(0, -0.36f, 0.065f), new Vector3(0.13f, 0.24f, 0.02f), branco);
            }

            cabeca = new GameObject("Cabeca").transform;
            cabeca.SetParent(tronco, false);
            cabeca.localPosition = new Vector3(0, 0.82f, 0);
            var h = cabeca;
            Peca(PrimitiveType.Cylinder, "Pescoco", h, new Vector3(0, -0.04f, 0), new Vector3(0.1f, 0.06f, 0.1f), pele);
            Peca(PrimitiveType.Sphere, "Cabeca", h, new Vector3(0, 0.14f, 0), new Vector3(0.28f, 0.3f, 0.28f), pele);
            Peca(PrimitiveType.Sphere, "Queixo", h, new Vector3(0, 0.06f, 0.05f), new Vector3(0.16f, 0.12f, 0.15f), pele);
            foreach (float s in new[] { -1f, 1f })
                Peca(PrimitiveType.Sphere, "Orelha", h, new Vector3(s * 0.138f, 0.13f, -0.005f), new Vector3(0.035f, 0.065f, 0.045f), pele);

            // Olhos: pivô por olho (esclera + íris + brilho) — o PersonagemOcioso achata o pivô para piscar
            var esclera = Mat("Esclera", new Color(0.98f, 0.98f, 0.96f), 0.6f);
            var iris = Mat("Olho", new Color(0.05f, 0.05f, 0.06f), 0.9f);
            olhos = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                float s = i == 0 ? -1f : 1f;
                var olho = new GameObject("Olho").transform;
                olho.SetParent(h, false);
                olho.localPosition = new Vector3(s * 0.055f, 0.155f, 0.118f);
                Peca(PrimitiveType.Sphere, "Esclera", olho, Vector3.zero, new Vector3(0.05f, 0.055f, 0.03f), esclera);
                Peca(PrimitiveType.Sphere, "Iris", olho, new Vector3(0, -0.003f, 0.012f), new Vector3(0.028f, 0.032f, 0.015f), iris);
                Peca(PrimitiveType.Sphere, "Brilho", olho, new Vector3(0.006f, 0.006f, 0.019f), Vector3.one * 0.007f, esclera);
                olhos[i] = olho;
                Peca(PrimitiveType.Cube, "Sobrancelha", h, new Vector3(s * 0.057f, 0.198f, 0.128f), new Vector3(0.05f, 0.011f, 0.015f), cabelo, new Vector3(0, 0, -s * 8f));
                Peca(PrimitiveType.Sphere, "Bochecha", h, new Vector3(s * 0.078f, 0.1f, 0.112f), new Vector3(0.045f, 0.025f, 0.012f), Mat("Bochecha", new Color(0.95f, 0.55f, 0.5f), 0.3f));
            }
            Peca(PrimitiveType.Sphere, "Nariz", h, new Vector3(0, 0.12f, 0.142f), new Vector3(0.04f, 0.05f, 0.045f), pele);
            Peca(PrimitiveType.Capsule, "Boca", h, new Vector3(0, 0.07f, 0.128f), new Vector3(0.014f, 0.028f, 0.01f), Mat("Boca", new Color(0.55f, 0.18f, 0.15f), 0.5f), new Vector3(0, 0, 90));

            // Cabelo
            if (v.estiloCabelo != 3)
                Peca(PrimitiveType.Sphere, "Cabelo", h, new Vector3(0, 0.215f, -0.02f), new Vector3(0.3f, 0.2f, 0.3f), cabelo);
            switch (v.estiloCabelo)
            {
                case 0: // curto
                    Peca(PrimitiveType.Sphere, "Nuca", h, new Vector3(0, 0.15f, -0.055f), new Vector3(0.28f, 0.22f, 0.2f), cabelo);
                    break;
                case 1: // comprido
                    Peca(PrimitiveType.Capsule, "Cabelo Comprido", h, new Vector3(0, 0.05f, -0.08f), new Vector3(0.28f, 0.17f, 0.12f), cabelo);
                    foreach (float s in new[] { -1f, 1f })
                        Peca(PrimitiveType.Sphere, "Mecha", h, new Vector3(s * 0.12f, 0.08f, -0.01f), new Vector3(0.07f, 0.2f, 0.1f), cabelo);
                    break;
                case 2: // coque
                    Peca(PrimitiveType.Sphere, "Nuca", h, new Vector3(0, 0.15f, -0.055f), new Vector3(0.28f, 0.2f, 0.18f), cabelo);
                    Peca(PrimitiveType.Sphere, "Coque", h, new Vector3(0, 0.28f, -0.09f), Vector3.one * 0.12f, cabelo);
                    break;
                case 3: // calvo, cabelo só dos lados
                    foreach (float s in new[] { -1f, 1f })
                        Peca(PrimitiveType.Sphere, "Lateral", h, new Vector3(s * 0.12f, 0.14f, -0.03f), new Vector3(0.07f, 0.08f, 0.18f), cabelo);
                    Peca(PrimitiveType.Sphere, "Nuca", h, new Vector3(0, 0.12f, -0.11f), new Vector3(0.22f, 0.09f, 0.1f), cabelo);
                    break;
                default: // cacheado
                    for (int i = 0; i < 9; i++)
                    {
                        float a = i / 9f * Mathf.PI * 2f;
                        Peca(PrimitiveType.Sphere, "Cacho", h, new Vector3(Mathf.Cos(a) * 0.11f, 0.22f + (i % 2) * 0.03f, Mathf.Sin(a) * 0.1f - 0.03f), Vector3.one * 0.11f, cabelo);
                    }
                    break;
            }

            if (v.garcom) // bigode de pizzaiolo, com as pontas viradas, e lápis atrás da orelha
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    Peca(PrimitiveType.Capsule, "Bigode", h, new Vector3(s * 0.03f, 0.088f, 0.138f), new Vector3(0.028f, 0.04f, 0.026f), cabelo, new Vector3(0, 0, s * 70f));
                    Peca(PrimitiveType.Sphere, "Ponta Bigode", h, new Vector3(s * 0.065f, 0.1f, 0.13f), Vector3.one * 0.022f, cabelo);
                }
                Peca(PrimitiveType.Cylinder, "Lapis", h, new Vector3(0.14f, 0.17f, 0.02f), new Vector3(0.01f, 0.05f, 0.01f), Mat("Lapis", new Color(0.95f, 0.75f, 0.2f)), new Vector3(70, 0, 0));
            }
            if (v.barba)
                Peca(PrimitiveType.Sphere, "Barba", h, new Vector3(0, 0.05f, 0.065f), new Vector3(0.2f, 0.12f, 0.14f), cabelo);
            if (v.oculos)
            {
                var armacao = Mat("Armacao", new Color(0.1f, 0.08f, 0.07f), 0.6f);
                foreach (float s in new[] { -1f, 1f })
                {
                    Peca(PrimitiveType.Cube, "Haste", h, new Vector3(s * 0.11f, 0.16f, 0.06f), new Vector3(0.006f, 0.008f, 0.12f), armacao);
                    var lente = new GameObject("Lente").AddComponent<MeshFilter>();
                    lente.transform.SetParent(h, false);
                    lente.transform.localPosition = new Vector3(s * 0.057f, 0.155f, 0.142f);
                    lente.transform.localRotation = Quaternion.Euler(90, 0, 0);
                    lente.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
                    lente.sharedMesh = MalhasProcedurais.Anel(0.07f);
                    lente.gameObject.AddComponent<MeshRenderer>().sharedMaterial = armacao;
                }
                Peca(PrimitiveType.Cube, "Ponte", h, new Vector3(0, 0.158f, 0.145f), new Vector3(0.03f, 0.006f, 0.006f), armacao);
            }
            if (v.boina)
            {
                var boina = Mat("Boina_" + ColorUtility.ToHtmlStringRGB(v.boinaCor), v.boinaCor, 0.2f);
                Peca(PrimitiveType.Sphere, "Boina", h, new Vector3(0.01f, 0.285f, -0.01f), new Vector3(0.32f, 0.08f, 0.32f), boina, new Vector3(0, 0, -8));
                Peca(PrimitiveType.Cylinder, "Pino", h, new Vector3(0.01f, 0.33f, -0.01f), new Vector3(0.015f, 0.01f, 0.015f), boina);
            }

            return corpo;
        }

        private static void CriarGarcom(Transform pai, Balcao balcao)
        {
            float hz = Profundidade / 2f;
            var espera = Ponto("Garcom Espera", pai, new Vector3(1.8f, 0f, -hz - 1.6f));
            var salao = Ponto("Garcom Salao", pai, new Vector3(-3.3f, 0f, -hz - 2.1f));

            var garcom = new GameObject("Garcom");
            garcom.transform.SetParent(pai);
            garcom.transform.position = espera.position;
            garcom.transform.rotation = Quaternion.identity;

            var v = new Visual
            {
                camisa = new Color(0.12f, 0.12f, 0.14f),
                calca = new Color(0.12f, 0.12f, 0.14f),
                pele = new Color(0.93f, 0.74f, 0.60f),
                cabelo = new Color(0.15f, 0.1f, 0.08f),
                garcom = true,
                estiloCabelo = 0
            };
            var corpo = Boneco(garcom.transform, v, false, out _, out var cabeca, out var olhos, out _);

            // Bandeja na mão direita (braço estendido para a frente)
            Peca(PrimitiveType.Cylinder, "Bandeja", corpo, new Vector3(0.18f, 1.34f, 0.5f), new Vector3(0.44f, 0.008f, 0.44f), MMetal);
            Peca(PrimitiveType.Cylinder, "Aro Bandeja", corpo, new Vector3(0.18f, 1.35f, 0.5f), new Vector3(0.45f, 0.006f, 0.45f), Mat("Latao", new Color(0.85f, 0.65f, 0.3f), 0.8f, 1f));
            var pontoBandeja = new GameObject("Ponto Bandeja").transform;
            pontoBandeja.SetParent(corpo, false);
            pontoBandeja.localPosition = new Vector3(0.18f, 1.36f, 0.5f);

            var comp = garcom.AddComponent<Garcom>();
            Atribuir(comp, "balcao", balcao);
            Atribuir(comp, "pontoEspera", espera);
            Atribuir(comp, "pontoSalao", salao);
            Atribuir(comp, "bandeja", pontoBandeja);
            Atribuir(comp, "corpo", corpo);

            // pisca e olha em volta enquanto espera (sem respiração no tronco: o Garcom já mexe no corpo)
            var ocioso = garcom.AddComponent<PersonagemOcioso>();
            Atribuir(ocioso, "cabeca", cabeca);
            AtribuirLista(ocioso, "olhos", olhos);
        }
    }
}
