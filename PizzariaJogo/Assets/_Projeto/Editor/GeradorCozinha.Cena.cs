using System.Collections.Generic;
using Pizzaria.Ambiente;
using Pizzaria.Cozinha;
using Pizzaria.Dados;
using Pizzaria.Interacao;
using Pizzaria.Jogador;
using Pizzaria.Jogo;
using Pizzaria.Pedidos;
using Pizzaria.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Pizzaria.EditorTools
{
    /// <summary>
    /// Direção de arte: cantina italiana aconchegante, low-poly estilizado.
    /// Fim de tarde (sol dourado entrando pela janela), luz quente das luminárias e do forno a lenha,
    /// paleta creme / terracota / verde-oliva / madeira, com pontos de vermelho-tomate.
    /// </summary>
    public static partial class GeradorCozinha
    {
        private const string CaminhoPos = "Assets/_Projeto/Configuracoes/PosCantina.asset";

        // Paleta
        private static readonly Color CorCreme = new Color(0.96f, 0.91f, 0.82f);
        private static readonly Color CorTomate = new Color(0.80f, 0.18f, 0.14f);
        private static readonly Color CorManjericao = new Color(0.20f, 0.45f, 0.24f);
        private static readonly Color CorVerdeEsmalte = new Color(0.16f, 0.32f, 0.24f);
        private static readonly Color CorMadeiraEscura = new Color(0.36f, 0.23f, 0.15f);
        private static readonly Color CorLuzQuente = new Color(1f, 0.80f, 0.56f);

        private static System.Random rnd = new System.Random(7);
        private static float R(float min, float max) => min + (float)rnd.NextDouble() * (max - min);

        // ------------------------------------------------------------------ Luz e pós

        private static void CriarIluminacao()
        {
            rnd = new System.Random(7); // decoração sempre igual a cada geração

            // Sol de fim de tarde: baixo e alaranjado, entra pela janela da parede direita
            var sol = new GameObject("Sol (fim de tarde)");
            var luz = sol.AddComponent<Light>();
            luz.type = LightType.Directional;
            luz.color = new Color(1f, 0.74f, 0.50f);
            luz.intensity = 2.2f;
            luz.shadows = LightShadows.Soft;
            luz.shadowStrength = 0.9f;
            sol.transform.rotation = Quaternion.LookRotation(new Vector3(-0.85f, -0.42f, 0.30f));
            RenderSettings.sun = luz;

            var ceu = AssetDatabase.LoadAssetAtPath<Material>($"{PastaMateriais}/Ceu.mat");
            if (ceu == null)
            {
                ceu = new Material(Shader.Find("Skybox/Procedural"));
                CriarPasta(PastaMateriais);
                AssetDatabase.CreateAsset(ceu, $"{PastaMateriais}/Ceu.mat");
            }
            ceu.SetFloat("_SunSize", 0.06f);
            ceu.SetFloat("_SunSizeConvergence", 4f);
            ceu.SetFloat("_AtmosphereThickness", 1.6f);
            ceu.SetColor("_SkyTint", new Color(0.62f, 0.52f, 0.50f));
            ceu.SetColor("_GroundColor", new Color(0.40f, 0.34f, 0.28f));
            ceu.SetFloat("_Exposure", 1.15f);
            EditorUtility.SetDirty(ceu);
            RenderSettings.skybox = ceu;

            // Ambiente quente e um pouco escuro: quem ilumina são as luminárias e o forno
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.46f, 0.42f, 0.40f);
            RenderSettings.ambientEquatorColor = new Color(0.36f, 0.29f, 0.24f);
            RenderSettings.ambientGroundColor = new Color(0.20f, 0.15f, 0.12f);

            // Névoa leve: só se nota lá fora, pela janela, e dá profundidade
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.78f, 0.60f, 0.46f);
            RenderSettings.fogStartDistance = 14f;
            RenderSettings.fogEndDistance = 60f;
        }

        private static void CriarPosProcessamento()
        {
            CriarPasta("Assets/_Projeto/Configuracoes");
            var perfil = AssetDatabase.LoadAssetAtPath<VolumeProfile>(CaminhoPos);
            if (perfil == null)
            {
                perfil = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(perfil, CaminhoPos);
            }

            var tone = Componente<Tonemapping>(perfil);
            tone.mode.Override(TonemappingMode.ACES);

            var bloom = Componente<Bloom>(perfil);
            bloom.threshold.Override(1.0f);
            bloom.intensity.Override(0.75f);
            bloom.scatter.Override(0.65f);
            bloom.tint.Override(new Color(1f, 0.88f, 0.75f));

            var cor = Componente<ColorAdjustments>(perfil);
            cor.postExposure.Override(0.45f);
            cor.contrast.Override(12f);
            cor.saturation.Override(10f);
            cor.colorFilter.Override(new Color(1f, 0.97f, 0.92f));

            var wb = Componente<WhiteBalance>(perfil);
            wb.temperature.Override(8f);

            var vinheta = Componente<Vignette>(perfil);
            vinheta.intensity.Override(0.28f);
            vinheta.smoothness.Override(0.45f);
            vinheta.color.Override(new Color(0.12f, 0.05f, 0.02f));

            EditorUtility.SetDirty(perfil);

            var go = new GameObject("Pos-Processamento");
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.sharedProfile = perfil;
        }

        private static T Componente<T>(VolumeProfile perfil) where T : VolumeComponent
        {
            if (perfil.TryGet<T>(out var c)) return c;
            c = perfil.Add<T>(true);
            c.name = typeof(T).Name;
            c.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(c, perfil);
            return c;
        }

        // ------------------------------------------------------------------ Cozinha (estrutura)

        private static void CriarSala()
        {
            var sala = new GameObject("Sala").transform;
            float hx = Largura / 2f, hz = Profundidade / 2f;

            Bloco("Piso", sala, new Vector3(0, -0.05f, 0), new Vector3(Largura, 0.1f, Profundidade), Mat("Piso", new Color(0.55f, 0.50f, 0.46f), 0.35f));
            Bloco("Teto", sala, new Vector3(0, AlturaParede + 0.05f, 0), new Vector3(Largura, 0.1f, Profundidade), Mat("Teto", CorCreme));

            var parede = Mat("Parede", new Color(0.93f, 0.88f, 0.80f));
            Bloco("Parede Fundo", sala, new Vector3(0, AlturaParede / 2, hz), new Vector3(Largura, AlturaParede, 0.2f), parede);
            Bloco("Parede Esquerda", sala, new Vector3(-hx, AlturaParede / 2, 0), new Vector3(0.2f, AlturaParede, Profundidade), parede);

            // Parede direita com uma JANELA de verdade (vão aberto): o sol entra e desenha um retângulo no chão
            const float jz0 = -2.6f, jz1 = -1.0f, jy0 = 1.1f, jy1 = 2.3f;
            Bloco("Parede Direita Baixo", sala, new Vector3(hx, jy0 / 2, 0), new Vector3(0.2f, jy0, Profundidade), parede);
            Bloco("Parede Direita Cima", sala, new Vector3(hx, (jy1 + AlturaParede) / 2, 0), new Vector3(0.2f, AlturaParede - jy1, Profundidade), parede);
            Bloco("Parede Direita Frente", sala, new Vector3(hx, (jy0 + jy1) / 2, (-hz + jz0) / 2), new Vector3(0.2f, jy1 - jy0, jz0 + hz), parede);
            Bloco("Parede Direita Tras", sala, new Vector3(hx, (jy0 + jy1) / 2, (jz1 + hz) / 2), new Vector3(0.2f, jy1 - jy0, hz - jz1), parede);
            CriarJanela(sala, hx, jz0, jz1, jy0, jy1);

            // Parede da frente com abertura do balcão no meio (x de -1.5 a 1.5)
            float larguraLado = (Largura - 3f) / 2f;
            Bloco("Parede Frente E", sala, new Vector3(-hx + larguraLado / 2, AlturaParede / 2, -hz), new Vector3(larguraLado, AlturaParede, 0.2f), parede);
            Bloco("Parede Frente D", sala, new Vector3(hx - larguraLado / 2, AlturaParede / 2, -hz), new Vector3(larguraLado, AlturaParede, 0.2f), parede);
            Bloco("Parede Frente Topo", sala, new Vector3(0, 2.4f, -hz), new Vector3(3f, 1.2f, 0.2f), parede);

            // Azulejo "metrô" creme atrás das bancadas, até 2.3 m
            var metro = Mat("Azulejo Metro", CorCreme, 0.75f);
            Bloco("Azulejo Fundo", sala, new Vector3(0, 1.15f, hz - 0.11f), new Vector3(Largura - 0.2f, 2.3f, 0.02f), metro, true);
            var friso = Mat("Viga", CorMadeiraEscura);
            Bloco("Friso Azulejo Fundo", sala, new Vector3(0, 2.32f, hz - 0.12f), new Vector3(Largura - 0.2f, 0.05f, 0.04f), friso, true);

            // Barrado de azulejo verde nas outras paredes, com friso de madeira em cima
            var verde = Mat("Azulejo Verde", CorVerdeEsmalte, 0.8f);
            const float hb = 1.1f;
            Bloco("Barrado Esquerda", sala, new Vector3(-hx + 0.11f, hb / 2, 0), new Vector3(0.02f, hb, Profundidade - 0.2f), verde, true);
            Bloco("Barrado Direita", sala, new Vector3(hx - 0.11f, hb / 2, 0), new Vector3(0.02f, hb, Profundidade - 0.2f), verde, true);
            Bloco("Barrado Frente E", sala, new Vector3(-hx + larguraLado / 2 + 0.05f, hb / 2, -hz + 0.11f), new Vector3(larguraLado - 0.1f, hb, 0.02f), verde, true);
            Bloco("Barrado Frente D", sala, new Vector3(hx - larguraLado / 2 - 0.05f, hb / 2, -hz + 0.11f), new Vector3(larguraLado - 0.1f, hb, 0.02f), verde, true);
            Bloco("Friso Esquerda", sala, new Vector3(-hx + 0.12f, hb, 0), new Vector3(0.04f, 0.05f, Profundidade - 0.2f), friso, true);
            Bloco("Friso Direita", sala, new Vector3(hx - 0.12f, hb, 0), new Vector3(0.04f, 0.05f, Profundidade - 0.2f), friso, true);
            Bloco("Friso Frente E", sala, new Vector3(-hx + larguraLado / 2 + 0.05f, hb, -hz + 0.12f), new Vector3(larguraLado - 0.1f, 0.05f, 0.04f), friso, true);
            Bloco("Friso Frente D", sala, new Vector3(hx - larguraLado / 2 - 0.05f, hb, -hz + 0.12f), new Vector3(larguraLado - 0.1f, 0.05f, 0.04f), friso, true);

            // Vigas de madeira no teto
            foreach (float z in new[] { -2.6f, 0f, 2.6f })
                Bloco("Viga", sala, new Vector3(0, AlturaParede - 0.12f, z), new Vector3(Largura - 0.2f, 0.24f, 0.2f), friso, true);
        }

        private static void CriarJanela(Transform pai, float hx, float z0, float z1, float y0, float y1)
        {
            var janela = new GameObject("Janela").transform;
            janela.SetParent(pai);
            var tinta = Mat("Janela", new Color(0.22f, 0.40f, 0.30f), 0.5f);
            float zc = (z0 + z1) / 2f, yc = (y0 + y1) / 2f, lz = z1 - z0, ly = y1 - y0;

            Bloco("Peitoril", janela, new Vector3(hx - 0.06f, y0 - 0.02f, zc), new Vector3(0.34f, 0.05f, lz + 0.2f), Mat("Pedra", new Color(0.6f, 0.58f, 0.55f), 0.3f));
            Bloco("Moldura Topo", janela, new Vector3(hx, y1 + 0.03f, zc), new Vector3(0.26f, 0.06f, lz + 0.12f), tinta, true);
            Bloco("Moldura E", janela, new Vector3(hx, yc, z0 - 0.03f), new Vector3(0.26f, ly, 0.06f), tinta, true);
            Bloco("Moldura D", janela, new Vector3(hx, yc, z1 + 0.03f), new Vector3(0.26f, ly, 0.06f), tinta, true);
            Bloco("Travessa V", janela, new Vector3(hx + 0.05f, yc, zc), new Vector3(0.04f, ly, 0.04f), tinta, true);
            Bloco("Travessa H", janela, new Vector3(hx + 0.05f, y0 + ly * 0.55f, zc), new Vector3(0.04f, 0.04f, lz), tinta, true);

            // Venezianas abertas do lado de fora
            foreach (float s in new[] { -1f, 1f })
            {
                float z = s < 0 ? z0 - 0.45f : z1 + 0.45f;
                Bloco("Veneziana", janela, new Vector3(hx + 0.12f, yc, z), new Vector3(0.03f, ly + 0.1f, 0.8f), tinta, true);
                for (int i = 0; i < 7; i++)
                    Bloco("Ripa", janela, new Vector3(hx + 0.14f, y0 + 0.1f + i * ly / 7f, z), new Vector3(0.02f, 0.03f, 0.74f), Mat("Janela Sombra", new Color(0.14f, 0.27f, 0.20f)), true);
            }

            // Vasinhos de ervas no peitoril
            for (int i = 0; i < 3; i++)
            {
                var pos = new Vector3(hx - 0.1f, y0 + 0.005f, z0 + 0.35f + i * 0.45f);
                VasoComPlanta(janela, pos, 0.16f, 0.12f);
            }
        }

        private static void CriarExterior()
        {
            var fora = new GameObject("Exterior").transform;
            Bloco("Gramado", fora, new Vector3(30f, -0.08f, 0f), new Vector3(50f, 0.1f, 80f), Mat("Grama", new Color(0.40f, 0.48f, 0.24f)), true);
            Bloco("Calcada", fora, new Vector3(6.2f, -0.04f, 0f), new Vector3(2.2f, 0.1f, 30f), Mat("Pedra", new Color(0.6f, 0.58f, 0.55f), 0.3f), true);
            Bloco("Muro", fora, new Vector3(9.5f, 0.45f, -2f), new Vector3(0.4f, 0.9f, 22f), Mat("Parede", CorCreme), true);
            var tronco = Mat("Lenha", CorMadeiraEscura);
            var copa = Mat("Copa", new Color(0.26f, 0.42f, 0.20f));
            var copa2 = Mat("Copa Clara", new Color(0.42f, 0.55f, 0.24f));
            var arvores = new[] { new Vector3(12f, 0, -4f), new Vector3(15f, 0, 1.5f), new Vector3(11f, 0, 4.5f), new Vector3(18f, 0, -8f), new Vector3(20f, 0, 5f), new Vector3(13.5f, 0, -10f) };
            foreach (var p in arvores)
            {
                float h = R(2.5f, 3.6f);
                Prim(PrimitiveType.Cylinder, "Tronco", fora, p + Vector3.up * h * 0.35f, new Vector3(0.3f, h * 0.35f, 0.3f), tronco);
                Prim(PrimitiveType.Sphere, "Copa", fora, p + Vector3.up * h, Vector3.one * R(2.2f, 3f), copa);
                Prim(PrimitiveType.Sphere, "Copa", fora, p + new Vector3(R(-0.6f, 0.6f), h * 1.25f, R(-0.6f, 0.6f)), Vector3.one * R(1.4f, 2f), copa2);
            }
            // Ciprestes italianos ao fundo
            for (int i = 0; i < 6; i++)
            {
                var p = new Vector3(R(24f, 34f), 0, -14f + i * 5.5f);
                Prim(PrimitiveType.Capsule, "Cipreste", fora, p + Vector3.up * 2.6f, new Vector3(1.1f, 2.8f, 1.1f), copa);
            }
        }

        // ------------------------------------------------------------------ Estações (jogabilidade)

        private static void CriarEstacoes(Dictionary<string, IngredienteSO> ing)
        {
            var raiz = new GameObject("Estacoes").transform;
            float hz = Profundidade / 2f, hx = Largura / 2f;
            var marmore = Mat("Marmore", Color.white, 0.7f);
            var matMadeira = Mat("Madeira", new Color(0.55f, 0.38f, 0.25f));
            var escura = Mat("Viga", CorMadeiraEscura);
            var metal = Mat("Metal", new Color(0.6f, 0.6f, 0.62f), 0.7f, 0.8f);

            // Bancada comprida no fundo: armário de madeira com tampo de mármore
            Bloco("Bancada Ingredientes", raiz, new Vector3(0, (AlturaBancada - 0.04f) / 2, hz - 0.45f), new Vector3(Largura - 0.4f, AlturaBancada - 0.04f, 0.8f), matMadeira);
            Bloco("Tampo Ingredientes", raiz, new Vector3(0, AlturaBancada - 0.02f, hz - 0.47f), new Vector3(Largura - 0.36f, 0.04f, 0.86f), marmore);
            Bloco("Rodape Ingredientes", raiz, new Vector3(0, 0.05f, hz - 0.86f), new Vector3(Largura - 0.4f, 0.1f, 0.02f), escura, true);
            // Na ordem em que costumam entrar na receita: massa → molho → queijo → coberturas
            string[] ordem = { "Massa", "Molho", "Queijo", "Tomate", "Manjericao", "Calabresa", "Cebola", "Azeitona" };
            float espacamento = 1.15f;
            float inicio = -espacamento * (ordem.Length - 1) / 2f;
            for (int i = 0; i < ordem.Length; i++)
            {
                var so = ing[ordem[i]];
                var pos = new Vector3(inicio + i * espacamento, AlturaBancada, hz - 0.45f);
                CriarEstacaoIngrediente(raiz, so, ordem[i], pos);
            }
            DetalharBancadaFundo(raiz, inicio, espacamento, ordem.Length);

            // Bancada de montagem (ilha central): móvel de madeira com gavetas e tampo de mármore
            var montagem = Bloco("Bancada de Montagem", raiz, new Vector3(0, AlturaBancada / 2, 0.8f), new Vector3(2.2f, AlturaBancada, 1.1f), matMadeira);
            Bloco("Tampo", montagem.transform, new Vector3(0, AlturaBancada + 0.02f, 0.8f), new Vector3(2.3f, 0.04f, 1.2f), marmore, true);
            montagem.isStatic = false;
            var bancada = montagem.AddComponent<BancadaMontagem>();
            Atribuir(bancada, "pontoPizza", Ponto("Ponto Pizza Bancada", raiz, new Vector3(0, AlturaBancada + 0.045f, 0.8f)));
            Placa(raiz, "MONTAGEM", new Vector3(0, 2.1f, 0.8f), Quaternion.identity, new Color(0.55f, 0.38f, 0.25f), 1f, true);

            // Forno a lenha (cúpula de terracota sobre base de tijolos) na parede direita
            var forno = CriarFornoALenha(raiz, hx);
            var compForno = forno.AddComponent<Forno>();
            AtribuirLista(compForno, "vagas",
                Ponto("Vaga Forno 1", raiz, new Vector3(hx - 1.56f, 0.68f, 0.27f)),
                Ponto("Vaga Forno 2", raiz, new Vector3(hx - 1.56f, 0.68f, 0.73f)));
            Placa(raiz, "FORNO", new Vector3(hx - 1.55f, 2.25f, 0.5f), Quaternion.Euler(0, 90, 0), new Color(0.85f, 0.35f, 0.1f), 1f, true);

            // Balcão de entrega na abertura da frente
            var balcao = Bloco("Balcao", raiz, new Vector3(0, 0.53f, -hz + 0.1f), new Vector3(3f, 1.06f, 0.6f), matMadeira);
            Bloco("Tampo Balcao", balcao.transform, new Vector3(0, 1.08f, -hz + 0.1f), new Vector3(3.1f, 0.04f, 0.7f), marmore); // collider filho: clicar no tampo usa o balcão
            balcao.isStatic = false;
            var compBalcao = balcao.AddComponent<Balcao>();
            AtribuirLista(compBalcao, "vagas",
                Ponto("Vaga Balcao 1", raiz, new Vector3(-0.9f, 1.105f, -hz + 0.1f)),
                Ponto("Vaga Balcao 2", raiz, new Vector3(0f, 1.105f, -hz + 0.1f)),
                Ponto("Vaga Balcao 3", raiz, new Vector3(0.9f, 1.105f, -hz + 0.1f)));
            CriarGarcom(raiz, compBalcao);
            Placa(raiz, "BALCÃO", new Vector3(0, 2.0f, -hz + 0.12f), Quaternion.Euler(0, 180, 0), new Color(0.2f, 0.45f, 0.75f));

            // Painel de pedidos (parede esquerda): lousa com moldura de madeira
            var painel = Bloco("Painel de Pedidos", raiz, new Vector3(-hx + 0.13f, 1.7f, -0.5f), new Vector3(0.06f, 1.0f, 2.2f), Mat("Painel", new Color(0.12f, 0.17f, 0.15f), 0.1f));
            painel.isStatic = false;
            var compPainel = painel.AddComponent<PainelPedidos>();
            Moldura(raiz, new Vector3(-hx + 0.15f, 1.7f, -0.5f), 2.2f, 1.0f, Quaternion.Euler(0, -90, 0), 0.06f);
            // Texto lido por quem está na cozinha olhando para -X: canto superior esquerdo = z menor
            var textoPainel = Texto3D(raiz, "Comandas", new Vector3(-hx + 0.17f, 2.15f, -0.5f - 1.02f), Quaternion.Euler(0, -90, 0), 0.28f, new Color(0.97f, 0.96f, 0.9f));
            textoPainel.anchor = TextAnchor.UpperLeft;
            textoPainel.alignment = TextAlignment.Left;
            textoPainel.fontStyle = FontStyle.Normal;
            textoPainel.richText = true;
            Atribuir(compPainel, "texto", textoPainel);
            Placa(raiz, "PEDIDOS", new Vector3(-hx + 0.12f, 2.45f, -0.5f), Quaternion.Euler(0, -90, 0), new Color(0.3f, 0.3f, 0.35f));

            // Lixeira de inox com tampa e pedal
            var lixo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lixo.name = "Lixeira";
            lixo.transform.SetParent(raiz);
            lixo.transform.position = new Vector3(-hx + 0.5f, 0.4f, 2.6f);
            lixo.transform.localScale = new Vector3(0.5f, 0.4f, 0.5f);
            lixo.GetComponent<Renderer>().sharedMaterial = Mat("Lixeira", new Color(0.3f, 0.42f, 0.34f), 0.6f, 0.3f);
            lixo.AddComponent<Lixeira>();
            Prim(PrimitiveType.Cylinder, "Tampa Lixeira", raiz, new Vector3(-hx + 0.5f, 0.81f, 2.6f), new Vector3(0.54f, 0.02f, 0.54f), metal);
            Prim(PrimitiveType.Sphere, "Puxador Tampa", raiz, new Vector3(-hx + 0.5f, 0.84f, 2.6f), new Vector3(0.1f, 0.04f, 0.06f), metal);
            Prim(PrimitiveType.Cube, "Pedal", raiz, new Vector3(-hx + 0.82f, 0.04f, 2.6f), new Vector3(0.16f, 0.03f, 0.12f), metal);
            Texto3D(lixo.transform.parent, "LIXO", new Vector3(-hx + 0.5f, 1.05f, 2.6f), Quaternion.Euler(0, -90, 0), 0.6f, Color.white);
        }

        private static GameObject CriarFornoALenha(Transform pai, float hx)
        {
            // Raiz sem escala com o collider: as peças visuais ficam como filhas (todas destacam ao mirar)
            var forno = new GameObject("Forno");
            forno.transform.SetParent(pai);
            forno.transform.position = new Vector3(4.26f, 0.72f, 0.5f);
            var col = forno.AddComponent<BoxCollider>();
            col.size = new Vector3(1.28f, 1.44f, 1.7f);
            var t = forno.transform;

            var tijolo = Mat("Forno", new Color(0.55f, 0.24f, 0.16f));
            var pedra = Mat("Pedra", new Color(0.6f, 0.58f, 0.55f), 0.3f);
            var cupula = Mat("Cupula", new Color(0.74f, 0.36f, 0.22f), 0.2f);
            var interior = MatEmissivo("Interior Forno", new Color(0.30f, 0.07f, 0.03f), 0.8f);

            Bloco("Base", t, new Vector3(4.26f, 0.31f, 0.5f), new Vector3(1.28f, 0.62f, 1.7f), tijolo, true);
            Bloco("Laje", t, new Vector3(4.26f, 0.635f, 0.5f), new Vector3(1.3f, 0.03f, 1.72f), pedra, true);
            Malha(MalhasProcedurais.Cupula, "Cupula", t, new Vector3(4.45f, 0.65f, 0.5f), new Vector3(1.1f, 0.82f, 1.5f), cupula);
            Bloco("Garganta", t, new Vector3(4.05f, 0.95f, 0.5f), new Vector3(0.24f, 0.62f, 1.3f), tijolo, true);

            // Fachada: dois pilares + verga, deixando a boca aberta (z 0.15..0.85, y 0.65..1.0)
            Bloco("Pilar E", t, new Vector3(3.735f, 1.05f, 0f), new Vector3(0.23f, 0.8f, 0.3f), tijolo, true);
            Bloco("Pilar D", t, new Vector3(3.735f, 1.05f, 1f), new Vector3(0.23f, 0.8f, 0.3f), tijolo, true);
            Bloco("Verga", t, new Vector3(3.735f, 1.225f, 0.5f), new Vector3(0.23f, 0.45f, 0.7f), tijolo, true);
            // Arco de pedras em volta da boca
            for (int i = 0; i <= 8; i++)
            {
                float a = Mathf.PI * i / 8f;
                var p = new Vector3(3.61f, 1.0f + Mathf.Sin(a) * 0.4f, 0.5f + Mathf.Cos(a) * 0.4f);
                Prim(PrimitiveType.Cube, "Pedra do Arco", t, p, new Vector3(0.04f, 0.13f, 0.08f), pedra, Quaternion.Euler(-a * Mathf.Rad2Deg + 90f, 0, 0));
            }

            // Fundo incandescente, lenha, brasa e chamas
            Bloco("Fundo Brasa", t, new Vector3(3.925f, 0.82f, 0.5f), new Vector3(0.01f, 0.36f, 0.7f), interior, true);
            var lenha = Mat("Lenha", CorMadeiraEscura);
            Prim(PrimitiveType.Cylinder, "Lenha", t, new Vector3(3.88f, 0.69f, 0.42f), new Vector3(0.06f, 0.2f, 0.06f), lenha, Quaternion.Euler(90, 25, 0));
            Prim(PrimitiveType.Cylinder, "Lenha", t, new Vector3(3.88f, 0.69f, 0.58f), new Vector3(0.06f, 0.2f, 0.06f), lenha, Quaternion.Euler(90, -25, 0));
            Prim(PrimitiveType.Sphere, "Brasa", t, new Vector3(3.87f, 0.665f, 0.5f), new Vector3(0.12f, 0.03f, 0.5f), MatEmissivo("Brasa", new Color(1f, 0.32f, 0.06f), 3f));
            var chama = MatEmissivo("Chama", new Color(1f, 0.55f, 0.15f), 4.5f);
            var chamaClara = MatEmissivo("Chama Clara", new Color(1f, 0.82f, 0.35f), 5f);
            Prim(PrimitiveType.Sphere, "Chama", t, new Vector3(3.89f, 0.77f, 0.40f), new Vector3(0.07f, 0.18f, 0.08f), chama);
            Prim(PrimitiveType.Sphere, "Chama", t, new Vector3(3.89f, 0.80f, 0.52f), new Vector3(0.08f, 0.24f, 0.09f), chama);
            Prim(PrimitiveType.Sphere, "Chama", t, new Vector3(3.885f, 0.75f, 0.61f), new Vector3(0.06f, 0.14f, 0.07f), chama);
            Prim(PrimitiveType.Sphere, "Chama Miolo", t, new Vector3(3.87f, 0.74f, 0.5f), new Vector3(0.05f, 0.12f, 0.06f), chamaClara);

            // Chaminé até o teto
            var chamine = Mat("Chamine", new Color(0.18f, 0.17f, 0.17f), 0.4f, 0.6f);
            Prim(PrimitiveType.Cylinder, "Chamine", t, new Vector3(4.55f, 2.2f, 0.5f), new Vector3(0.24f, 0.8f, 0.24f), chamine);
            Prim(PrimitiveType.Cylinder, "Anel Chamine", t, new Vector3(4.55f, 1.45f, 0.5f), new Vector3(0.34f, 0.04f, 0.34f), chamine);

            // Prateleira de pedra na boca, onde as pizzas assam à vista do jogador (com collider: clicar nela usa o forno)
            Bloco("Prateleira do Forno", t, new Vector3(hx - 1.55f, 0.63f, 0.5f), new Vector3(0.45f, 0.06f, 1.2f), pedra);
            Bloco("Mao Francesa", t, new Vector3(hx - 1.43f, 0.5f, 0.05f), new Vector3(0.2f, 0.2f, 0.08f), tijolo, true);
            Bloco("Mao Francesa", t, new Vector3(hx - 1.43f, 0.5f, 0.95f), new Vector3(0.2f, 0.2f, 0.08f), tijolo, true);

            // Luz do fogo, tremulando
            var luzGo = new GameObject("Luz do Fogo");
            luzGo.transform.SetParent(t);
            luzGo.transform.position = new Vector3(3.5f, 0.9f, 0.5f);
            var luz = luzGo.AddComponent<Light>();
            luz.type = LightType.Point;
            luz.color = new Color(1f, 0.52f, 0.22f);
            luz.intensity = 2.4f;
            luz.range = 3.8f;
            luz.shadows = LightShadows.None;
            luzGo.AddComponent<LuzTremulante>();

            Fagulhas(t, new Vector3(3.86f, 0.72f, 0.5f));
            SomLoop(t, "Som Fogo", new Vector3(3.86f, 0.6f, 0.5f), Pizzaria.Audio.SomAmbiente.Tipo.Fogo, 0.5f, 1f, 7f);
            MarcarEstatico(t, false);
            return forno;
        }

        private static void SomLoop(Transform pai, string nome, Vector3 pos, Pizzaria.Audio.SomAmbiente.Tipo tipo, float vol, float min, float max)
        {
            var go = new GameObject(nome);
            go.transform.SetParent(pai);
            go.transform.position = pos;
            go.AddComponent<Pizzaria.Audio.SomAmbiente>().Configurar(tipo, vol, min, max);
        }

        private static void Fagulhas(Transform pai, Vector3 pos)
        {
            var go = new GameObject("Fagulhas");
            go.transform.SetParent(pai);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(-90, 0, 0); // emite para cima
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.008f, 0.018f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.7f, 0.25f), new Color(1f, 0.4f, 0.1f));
            main.maxParticles = 40;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.05f;
            var em = ps.emission;
            em.rateOverTime = 14f;
            var forma = ps.shape;
            forma.shapeType = ParticleSystemShapeType.Box;
            forma.scale = new Vector3(0.06f, 0.5f, 0.02f);
            var ruido = ps.noise;
            ruido.enabled = true;
            ruido.strength = 0.15f;
            ruido.frequency = 2f;
            var cor = ps.colorOverLifetime;
            cor.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            cor.color = g;

            var r = go.GetComponent<ParticleSystemRenderer>();
            var mat = AssetDatabase.LoadAssetAtPath<Material>($"{PastaMateriais}/Fagulha.mat");
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (shader != null)
                {
                    mat = new Material(shader);
                    AssetDatabase.CreateAsset(mat, $"{PastaMateriais}/Fagulha.mat");
                }
            }
            if (mat != null)
            {
                mat.SetColor("_BaseColor", new Color(4f, 2f, 0.6f, 1f)); // HDR: brilha no bloom
                EditorUtility.SetDirty(mat);
                r.sharedMaterial = mat;
            }
        }

        private static void CriarEstacaoIngrediente(Transform pai, IngredienteSO so, string id, Vector3 p)
        {
            var estacao = new GameObject("Estacao " + so.Nome);
            estacao.transform.SetParent(pai);
            estacao.transform.position = p;
            var t = estacao.transform;

            // Caixote de madeira (com collider) + conteúdo com a cor do ingrediente (é ele que acende ao mirar)
            var caixote = Mat("Caixote", new Color(0.78f, 0.60f, 0.40f));
            var ripa = Mat("Viga", CorMadeiraEscura);
            Bloco("Caixote", t, p + new Vector3(0, 0.08f, 0), new Vector3(0.68f, 0.16f, 0.48f), caixote);
            DetalharCaixote(t, p, ripa);
            Vector3 topo = p + new Vector3(0, 0.16f, 0);

            var cor = Mat("Ing_" + so.name, so.cor, 0.45f);
            ConteudoEstacao(t, id, topo, cor);

            MarcarEstatico(t, false); // o destaque ao mirar mexe nos renderers: melhor não entrarem no static batching
            var comp = estacao.AddComponent<EstacaoIngrediente>();
            Atribuir(comp, "ingrediente", so);

            // Placa grande e colorida acima, bem sinalizada
            Placa(t, so.Nome.ToUpperInvariant(), p + new Vector3(0, 1.05f, 0.25f), Quaternion.identity, so.cor, 0.82f);
        }

        // ------------------------------------------------------------------ Jogador

        private static void CriarJogador(Vector3 pos)
        {
            var jogador = new GameObject("Jogador");
            jogador.transform.position = pos;

            var cc = jogador.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0, 0.9f, 0);
            cc.stepOffset = 0.3f;

            var camGo = new GameObject("Camera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(jogador.transform, false);
            camGo.transform.localPosition = new Vector3(0, 1.65f, 0);
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.03f;
            cam.farClipPlane = 120f;
            cam.fieldOfView = 70f;
            camGo.AddComponent<AudioListener>();
            var dadosCam = cam.GetUniversalAdditionalCameraData();
            dadosCam.renderPostProcessing = true;
            dadosCam.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            dadosCam.antialiasingQuality = AntialiasingQuality.High;

            var controlador = jogador.AddComponent<ControladorJogador>();
            Atribuir(controlador, "cameraJogador", camGo.transform);
            jogador.AddComponent<InventarioJogador>();
            var interacao = jogador.AddComponent<InteracaoJogador>();

            // Mãos (dólmã branco de cozinheiro, com punho vermelho)
            var maosGo = new GameObject("Maos");
            maosGo.transform.SetParent(camGo.transform, false);
            var pele = Mat("Pele", new Color(0.93f, 0.74f, 0.60f), 0.35f);
            var manga = Mat("Manga Branca", new Color(0.97f, 0.97f, 0.97f));
            var punho = Mat("Punho", CorTomate);

            var maoD = CriarMao("Mao Direita", maosGo.transform, new Vector3(0.28f, -0.27f, 0.45f), pele, manga, punho, true);
            var maoE = CriarMao("Mao Esquerda", maosGo.transform, new Vector3(-0.28f, -0.29f, 0.42f), pele, manga, punho, false);

            var item = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            item.name = "Item na Mao";
            Object.DestroyImmediate(item.GetComponent<Collider>());
            item.transform.SetParent(maoD, false);
            item.transform.localPosition = new Vector3(-0.01f, 0.06f, 0.07f);
            item.transform.localScale = new Vector3(0.11f, 0.07f, 0.11f);
            item.GetComponent<Renderer>().sharedMaterial = Mat("Item", Color.white, 0.45f);
            item.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            item.SetActive(false);

            var pontoPizza = new GameObject("Ponto Pizza").transform;
            pontoPizza.SetParent(maosGo.transform, false);
            pontoPizza.localPosition = new Vector3(0f, -0.30f, 0.50f);
            pontoPizza.localRotation = Quaternion.Euler(-25f, 0f, 0f);

            var maos = maosGo.AddComponent<MaosVisuais>();
            Atribuir(maos, "pontoPizza", pontoPizza);
            Atribuir(maos, "controlador", controlador);
            Atribuir(maos, "interacao", interacao);
            Atribuir(maos, "maoDireita", maoD);
            Atribuir(maos, "maoEsquerda", maoE);
            Atribuir(maos, "itemNaMao", item.GetComponent<Renderer>());

            var hud = jogador.AddComponent<HudPrototipo>();
            Atribuir(hud, "interacao", interacao);
            Atribuir(hud, "controlador", controlador);
        }

        private static Transform CriarMao(string nome, Transform pai, Vector3 pos, Material pele, Material manga, Material punho, bool direita)
        {
            var mao = new GameObject(nome).transform;
            mao.SetParent(pai, false);
            mao.localPosition = pos;
            mao.localRotation = Quaternion.Euler(10f, direita ? -8f : 8f, 0f);
            float s = direita ? 1f : -1f;

            void P(PrimitiveType tipo, string n, Vector3 p, Vector3 e, Material m, Vector3 r = default)
            {
                var go = Peca(tipo, n, mao, p, e, m, r);
                go.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            }

            P(PrimitiveType.Capsule, "Antebraco", new Vector3(0, -0.02f, -0.2f), new Vector3(0.1f, 0.16f, 0.1f), manga, new Vector3(90f, 0, 0));
            P(PrimitiveType.Cylinder, "Punho", new Vector3(0, -0.015f, -0.06f), new Vector3(0.105f, 0.018f, 0.105f), punho, new Vector3(90f, 0, 0));
            P(PrimitiveType.Sphere, "Palma", new Vector3(0, 0, 0.02f), new Vector3(0.085f, 0.04f, 0.1f), pele);
            for (int i = 0; i < 4; i++)
                P(PrimitiveType.Capsule, "Dedo", new Vector3((-0.03f + i * 0.02f) * s, -0.004f, 0.08f - Mathf.Abs(i - 1.5f) * 0.006f),
                  new Vector3(0.018f, 0.025f, 0.018f), pele, new Vector3(100f, 0, 0));
            P(PrimitiveType.Capsule, "Polegar", new Vector3(-0.048f * s, 0.008f, 0.03f), new Vector3(0.022f, 0.026f, 0.022f), pele, new Vector3(80f, -35f * s, 0));
            return mao;
        }

        // ------------------------------------------------------------------ Salão

        private static void CriarSalao()
        {
            // Área dos clientes do outro lado do balcão (por onde o garçom anda)
            var salao = new GameObject("Salao").transform;
            float hz = Profundidade / 2f, hx = Largura / 2f;
            float zc = -hz - 2.6f, z0 = -hz - 0.1f, z1 = -hz - 5.1f;

            Bloco("Piso Salao", salao, new Vector3(0, -0.05f, zc), new Vector3(Largura, 0.1f, 5f), Mat("Piso Salao", new Color(0.62f, 0.42f, 0.30f), 0.4f));
            var paredeSalao = Mat("Parede Salao", new Color(0.85f, 0.70f, 0.55f));
            Bloco("Parede Salao", salao, new Vector3(0, AlturaParede / 2, z1), new Vector3(Largura, AlturaParede, 0.2f), paredeSalao);
            Bloco("Parede Salao E", salao, new Vector3(-hx, AlturaParede / 2, zc), new Vector3(0.2f, AlturaParede, 5f), paredeSalao);
            Bloco("Parede Salao D", salao, new Vector3(hx, AlturaParede / 2, zc), new Vector3(0.2f, AlturaParede, 5f), paredeSalao);
            Bloco("Teto Salao", salao, new Vector3(0, AlturaParede + 0.05f, zc), new Vector3(Largura, 0.1f, 5.2f), Mat("Teto", CorCreme));
            SomLoop(salao, "Som Burburinho", new Vector3(0, 1.3f, zc - 0.8f), Pizzaria.Audio.SomAmbiente.Tipo.Salao, 0.4f, 2.5f, 13f);

            // Lambri de madeira escura até 1 m nas paredes do salão (e no lado de fora da cozinha)
            var lambri = Mat("Lambri", CorMadeiraEscura, 0.35f);
            var friso = Mat("Viga", CorMadeiraEscura);
            Bloco("Lambri Fundo", salao, new Vector3(0, 0.5f, z1 + 0.11f), new Vector3(Largura - 0.2f, 1f, 0.02f), lambri, true);
            Bloco("Lambri E", salao, new Vector3(-hx + 0.11f, 0.5f, zc), new Vector3(0.02f, 1f, 4.8f), lambri, true);
            Bloco("Lambri D", salao, new Vector3(hx - 0.11f, 0.5f, zc), new Vector3(0.02f, 1f, 4.8f), lambri, true);
            float larguraLado = (Largura - 3f) / 2f;
            Bloco("Lambri Cozinha E", salao, new Vector3(-hx + larguraLado / 2, 0.5f, z0 - 0.01f), new Vector3(larguraLado - 0.2f, 1f, 0.02f), lambri, true);
            Bloco("Lambri Cozinha D", salao, new Vector3(hx - larguraLado / 2, 0.5f, z0 - 0.01f), new Vector3(larguraLado - 0.2f, 1f, 0.02f), lambri, true);
            Bloco("Friso Fundo", salao, new Vector3(0, 1.0f, z1 + 0.12f), new Vector3(Largura - 0.2f, 0.05f, 0.04f), friso, true);

            // Frente do balcão (lado do salão) com a faixa tricolor
            float zFrente = -hz - 0.21f; // face do balcão voltada para o salão
            Bloco("Faixa Verde", salao, new Vector3(-1f, 0.85f, zFrente), new Vector3(1f, 0.14f, 0.02f), Mat("Bandeira Verde", new Color(0.0f, 0.55f, 0.27f)), true);
            Bloco("Faixa Branca", salao, new Vector3(0f, 0.85f, zFrente), new Vector3(1f, 0.14f, 0.02f), Mat("Bandeira Branca", new Color(0.96f, 0.95f, 0.92f)), true);
            Bloco("Faixa Vermelha", salao, new Vector3(1f, 0.85f, zFrente), new Vector3(1f, 0.14f, 0.02f), Mat("Bandeira Vermelha", new Color(0.80f, 0.14f, 0.16f)), true);

            // Mesas redondas com toalha xadrez; o garçom anda pela faixa z ≈ -5.5..-6.2, então as mesas ficam atrás
            var mesas = new[] { new Vector3(-3.3f, 0, -hz - 3f), new Vector3(3.3f, 0, -hz - 3f), new Vector3(-1.2f, 0, -hz - 4.15f), new Vector3(1.2f, 0, -hz - 4.15f) };
            for (int i = 0; i < mesas.Length; i++)
                CriarMesa(salao, mesas[i], i);

            // Luminárias sobre as mesas
            foreach (var m in mesas)
                Luminaria(salao, new Vector3(m.x, 2.15f, m.z), 1.3f, 3.4f);

            // Letreiro neon na parede do fundo
            CriarLetreiro(salao, new Vector3(-0.6f, 2.15f, z1 + 0.12f));

            // Quadros nas paredes laterais
            Quadro(salao, new Vector3(-hx + 0.12f, 1.8f, -5.6f), Quaternion.Euler(0, -90, 0), 0);
            Quadro(salao, new Vector3(-hx + 0.12f, 1.75f, -7.8f), Quaternion.Euler(0, -90, 0), 1);
            Quadro(salao, new Vector3(hx - 0.12f, 1.8f, -6.7f), Quaternion.Euler(0, 90, 0), 2);

            // Porta de entrada
            var porta = Mat("Porta", new Color(0.30f, 0.18f, 0.12f), 0.35f);
            Bloco("Batente", salao, new Vector3(3.6f, 1.1f, z1 + 0.12f), new Vector3(1.2f, 2.2f, 0.05f), friso, true);
            Bloco("Porta", salao, new Vector3(3.6f, 1.05f, z1 + 0.15f), new Vector3(1.0f, 2.1f, 0.04f), porta, true);
            Bloco("Vidro Porta", salao, new Vector3(3.6f, 1.55f, z1 + 0.175f), new Vector3(0.6f, 0.7f, 0.01f), MatEmissivo("Vidro Rua", new Color(1f, 0.72f, 0.45f), 1.2f), true);
            Prim(PrimitiveType.Sphere, "Macaneta", salao, new Vector3(3.25f, 1.0f, z1 + 0.19f), Vector3.one * 0.06f, Mat("Latao", new Color(0.85f, 0.65f, 0.3f), 0.8f, 1f));

            // Plantas nos cantos
            VasoComPlanta(salao, new Vector3(-4.5f, 0, z1 + 0.45f), 0.7f, 0.55f);
            VasoComPlanta(salao, new Vector3(4.55f, 0, -hz - 0.5f), 0.6f, 0.5f);
            VasoComPlanta(salao, new Vector3(-4.55f, 0, -hz - 0.5f), 0.6f, 0.5f);

            Varal(salao, new Vector3(-hx + 0.1f, 2.75f, -6.0f), new Vector3(hx - 0.1f, 2.75f, -6.0f), 0.35f, 22);
        }

        private static void CriarMesa(Transform pai, Vector3 p, int indice)
        {
            var mesa = new GameObject("Mesa " + (indice + 1)).transform;
            mesa.SetParent(pai);
            mesa.position = p;
            var madeira = Mat("Mesa", new Color(0.45f, 0.30f, 0.20f), 0.4f);
            var toalha = Mat("Toalha", new Color(0.85f, 0.20f, 0.20f));
            var ferro = Mat("Ferro", new Color(0.12f, 0.12f, 0.12f), 0.4f, 0.6f);

            Prim(PrimitiveType.Cylinder, "Pe", mesa, p + new Vector3(0, 0.37f, 0), new Vector3(0.08f, 0.37f, 0.08f), ferro);
            Prim(PrimitiveType.Cylinder, "Base", mesa, p + new Vector3(0, 0.02f, 0), new Vector3(0.5f, 0.02f, 0.5f), ferro);
            Prim(PrimitiveType.Cylinder, "Tampo", mesa, p + new Vector3(0, 0.74f, 0), new Vector3(0.9f, 0.02f, 0.9f), madeira);
            Prim(PrimitiveType.Cylinder, "Toalha", mesa, p + new Vector3(0, 0.66f, 0), new Vector3(0.98f, 0.105f, 0.98f), toalha);

            float topo = 0.77f;
            // Vela na garrafa (chama brilha no bloom)
            Malha(MalhasProcedurais.Garrafa, "Garrafa Vela", mesa, p + new Vector3(0, topo, 0), new Vector3(0.09f, 0.2f, 0.09f), Mat("Vidro Verde", new Color(0.12f, 0.32f, 0.18f), 0.9f));
            Prim(PrimitiveType.Cylinder, "Vela", mesa, p + new Vector3(0, topo + 0.24f, 0), new Vector3(0.025f, 0.04f, 0.025f), Mat("Cera", CorCreme, 0.3f));
            Prim(PrimitiveType.Sphere, "Chama Vela", mesa, p + new Vector3(0, topo + 0.3f, 0), new Vector3(0.015f, 0.03f, 0.015f), MatEmissivo("Chama", new Color(1f, 0.55f, 0.15f), 4.5f));

            // Pratos, copos e (em duas mesas) uma pizza servida
            var prato = Mat("Ceramica", CorCreme, 0.7f);
            var vidro = Mat("Vidro", new Color(0.75f, 0.85f, 0.85f), 0.95f);
            foreach (float s in new[] { -1f, 1f })
            {
                Malha(MalhasProcedurais.Prato, "Prato", mesa, p + new Vector3(0.28f * s, topo, 0), Vector3.one * 0.24f, prato);
                Malha(MalhasProcedurais.Pote, "Copo", mesa, p + new Vector3(0.24f * s, topo, 0.2f), new Vector3(0.06f, 0.12f, 0.06f), vidro);
            }
            if (indice % 2 == 0) PizzaDecorativa(mesa, p + new Vector3(0, topo + 0.005f, -0.18f), 0.32f);

            // Cadeiras dos dois lados e clientes em algumas delas
            for (int lado = 0; lado < 2; lado++)
            {
                float s = lado == 0 ? -1f : 1f;
                var posCadeira = p + new Vector3(0.72f * s, 0, 0);
                var rot = Quaternion.Euler(0, s < 0 ? 90f : -90f, 0); // de frente para a mesa
                Cadeira(mesa, posCadeira, rot);
                bool ocupada = (indice + lado) % 3 != 2;
                if (ocupada) Cliente(mesa, posCadeira, rot, indice * 2 + lado);
            }
        }

        private static void Cadeira(Transform pai, Vector3 p, Quaternion rot)
        {
            var c = new GameObject("Cadeira").transform;
            c.SetParent(pai);
            c.position = p;
            c.rotation = rot;
            var madeira = Mat("Mesa", new Color(0.45f, 0.30f, 0.20f), 0.4f);
            Peca(PrimitiveType.Cube, "Assento", c, new Vector3(0, 0.45f, 0), new Vector3(0.42f, 0.04f, 0.42f), madeira);
            Peca(PrimitiveType.Cube, "Encosto", c, new Vector3(0, 0.75f, -0.19f), new Vector3(0.42f, 0.36f, 0.04f), madeira);
            foreach (var (x, z) in new[] { (-0.18f, -0.18f), (0.18f, -0.18f), (-0.18f, 0.18f), (0.18f, 0.18f) })
                Peca(PrimitiveType.Cube, "Perna", c, new Vector3(x, 0.215f, z), new Vector3(0.04f, 0.43f, 0.04f), madeira);
            MarcarEstatico(c);
        }

        private static void PizzaDecorativa(Transform pai, Vector3 p, float diametro)
        {
            Prim(PrimitiveType.Cylinder, "Pizza Borda", pai, p + Vector3.up * 0.012f, new Vector3(diametro, 0.012f, diametro), Mat("Massa Assada", new Color(0.86f, 0.58f, 0.30f)));
            Prim(PrimitiveType.Cylinder, "Pizza Molho", pai, p + Vector3.up * 0.026f, new Vector3(diametro * 0.86f, 0.003f, diametro * 0.86f), Mat("Molho Assado", new Color(0.72f, 0.16f, 0.08f)));
            Prim(PrimitiveType.Cylinder, "Pizza Queijo", pai, p + Vector3.up * 0.029f, new Vector3(diametro * 0.78f, 0.003f, diametro * 0.78f), Mat("Queijo Assado", new Color(1f, 0.82f, 0.38f), 0.5f));
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3f + 0.3f, r = diametro * 0.24f;
                Prim(PrimitiveType.Cylinder, "Calabresa", pai, p + new Vector3(Mathf.Cos(a) * r, 0.033f, Mathf.Sin(a) * r), new Vector3(diametro * 0.13f, 0.003f, diametro * 0.13f), Mat("Calabresa Assada", new Color(0.62f, 0.18f, 0.14f)));
            }
            Prim(PrimitiveType.Sphere, "Manjericao", pai, p + new Vector3(0, 0.034f, 0), new Vector3(0.04f, 0.008f, 0.03f), Mat("Folha", CorManjericao));
        }

        // ------------------------------------------------------------------ Gerenciadores e NPCs

        private static GameObject CriarGerenciadores(List<ReceitaSO> receitas, DiaSO[] dias)
        {
            var go = new GameObject("Gerenciador");
            var recursos = go.AddComponent<RecursosCozinha>();
            Atribuir(recursos, "materialBase", Mat("Pizza Base", Color.white, 0.35f));

            var pedidos = go.AddComponent<GerenciadorPedidos>();
            AtribuirLista(pedidos, "receitas", receitas.ToArray()); // o dia substitui pelo cardápio dele

            var jogo = go.AddComponent<GerenciadorJogo>();
            AtribuirLista(jogo, "dias", dias);
            Atribuir(jogo, "pedidos", pedidos);

            go.AddComponent<DicasTutorial>();
            go.AddComponent<TelasJogo>();
            return go;
        }

        /// <summary>Liga referências entre objetos criados em funções diferentes.</summary>
        private static void LigarReferencias(GameObject gerenciador)
        {
            var jogador = Object.FindAnyObjectByType<ControladorJogador>();
            var interacao = Object.FindAnyObjectByType<InteracaoJogador>();
            var dicas = gerenciador.GetComponent<DicasTutorial>();

            Atribuir(gerenciador.GetComponent<GerenciadorJogo>(), "jogador", jogador);
            Atribuir(dicas, "jogador", interacao);
            Atribuir(dicas, "bancada", Object.FindAnyObjectByType<BancadaMontagem>());
            Atribuir(dicas, "forno", Object.FindAnyObjectByType<Forno>());

            var hud = Object.FindAnyObjectByType<HudPrototipo>();
            if (hud != null) Atribuir(hud, "dicas", dicas);
        }
    }
}
