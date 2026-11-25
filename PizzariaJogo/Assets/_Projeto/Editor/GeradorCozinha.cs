using System.Collections.Generic;
using System.IO;
using Pizzaria.Cozinha;
using Pizzaria.Dados;
using Pizzaria.Interacao;
using Pizzaria.Jogador;
using Pizzaria.Jogo;
using Pizzaria.Pedidos;
using Pizzaria.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pizzaria.EditorTools
{
    /// <summary>
    /// Gera a cena greybox da cozinha (blocos simples) + os assets de dados iniciais.
    /// Menu: Pizzaria > Gerar Cozinha Greybox. Roda sozinho na primeira abertura do projeto.
    /// </summary>
    public static partial class GeradorCozinha
    {
        public const string CaminhoCena = "Assets/Scenes/Cozinha.unity";
        /// <summary>Muda quando o gerador ganha conteúdo novo; a cena salva guarda um objeto com esse nome.</summary>
        public const string MarcaVersao = "VersaoCena_4_Som";
        private const string PastaDados = "Assets/_Projeto/Dados";
        private const string PastaIngredientes = PastaDados + "/Ingredientes";
        private const string PastaReceitas = PastaDados + "/Receitas";
        private const string PastaDias = PastaDados + "/Dias";
        private const string PastaMateriais = "Assets/_Projeto/Materiais";

        // Dimensões da cozinha (metros)
        private const float Largura = 10f;      // eixo X
        private const float Profundidade = 8f;  // eixo Z
        private const float AlturaParede = 3f;
        private const float AlturaBancada = 0.9f;

        private static readonly Dictionary<string, Material> cacheMateriais = new Dictionary<string, Material>();

        [MenuItem("Pizzaria/Gerar Cozinha Greybox", priority = 0)]
        public static void GerarPeloMenu()
        {
            if (File.Exists(CaminhoCena) &&
                !EditorUtility.DisplayDialog("Gerar cozinha",
                    "A cena Cozinha.unity já existe e será substituída. Continuar?", "Substituir", "Cancelar"))
                return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Gerar();
        }

        public static void Gerar()
        {
            cacheMateriais.Clear();
            MalhasProcedurais.LimparCache();

            // A cena nova vem PRIMEIRO: abrir uma cena descarrega assets sem uso, o que invalidaria
            // as referências aos ingredientes/receitas se eles fossem carregados antes.
            var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var ingredientes = CriarIngredientes();
            var receitas = CriarReceitas(ingredientes);
            var dias = CriarDias(receitas);

            CriarIluminacao();
            CriarPosProcessamento();
            CriarSala();
            CriarExterior();
            CriarSalao();
            var gerenciador = CriarGerenciadores(new List<ReceitaSO>(receitas.Values), dias);
            CriarEstacoes(ingredientes);
            CriarDecoracaoCozinha(receitas);
            CriarJogador(new Vector3(0f, 0f, -1.5f));
            LigarReferencias(gerenciador);
            TexturasProcedurais.AplicarNosMateriais(nome =>
                cacheMateriais.TryGetValue(nome, out var m) ? m : AssetDatabase.LoadAssetAtPath<Material>($"{PastaMateriais}/{nome}.mat"));

            new GameObject(MarcaVersao); // marca a versão do gerador que criou esta cena
            Directory.CreateDirectory(Path.GetDirectoryName(CaminhoCena));
            EditorSceneManager.SaveScene(cena, CaminhoCena);
            RegistrarNoBuild();
            AssetDatabase.SaveAssets();

            Debug.Log("[Pizzaria] Cozinha greybox gerada em " + CaminhoCena + ". Aperte Play para testar!");
        }

        // ------------------------------------------------------------------ Dados

        private static Dictionary<string, IngredienteSO> CriarIngredientes()
        {
            CriarPasta(PastaIngredientes);
            var lista = new (string id, string nome, Color cor, int max, FormaIngrediente forma, VisualIngrediente visual)[]
            {
                ("Massa",      "Massa",       new Color(0.96f, 0.88f, 0.70f), 6,  FormaIngrediente.Base,      VisualIngrediente.Massa),
                ("Molho",      "Molho",       new Color(0.80f, 0.15f, 0.10f), 10, FormaIngrediente.Camada,    VisualIngrediente.Molho),
                ("Queijo",     "Queijo",      new Color(1.00f, 0.86f, 0.35f), 10, FormaIngrediente.Camada,    VisualIngrediente.Queijo),
                ("Tomate",     "Tomate",      new Color(0.95f, 0.30f, 0.25f), 10, FormaIngrediente.Cobertura, VisualIngrediente.Tomate),
                ("Manjericao", "Manjericão",  new Color(0.20f, 0.60f, 0.20f), 10, FormaIngrediente.Cobertura, VisualIngrediente.Manjericao),
                ("Calabresa",  "Calabresa",   new Color(0.62f, 0.20f, 0.18f), 10, FormaIngrediente.Cobertura, VisualIngrediente.Calabresa),
                ("Cebola",     "Cebola",      new Color(0.93f, 0.88f, 0.95f), 10, FormaIngrediente.Cobertura, VisualIngrediente.Cebola),
                ("Azeitona",   "Azeitona",    new Color(0.18f, 0.16f, 0.14f), 10, FormaIngrediente.Cobertura, VisualIngrediente.Azeitona),
            };

            var dic = new Dictionary<string, IngredienteSO>();
            foreach (var (id, nome, cor, max, forma, visual) in lista)
            {
                string caminho = $"{PastaIngredientes}/{id}.asset";
                var so = AssetDatabase.LoadAssetAtPath<IngredienteSO>(caminho);
                if (so == null)
                {
                    so = ScriptableObject.CreateInstance<IngredienteSO>();
                    so.nomeExibicao = nome;
                    so.cor = cor;
                    so.maxNaMao = max;
                    so.forma = forma;
                    so.visual = visual;
                    AssetDatabase.CreateAsset(so, caminho);
                }
                else if (so.forma != forma || so.visual != visual)
                {
                    // assets criados antes dos campos "forma"/"visual" existirem
                    so.forma = forma;
                    so.visual = visual;
                    EditorUtility.SetDirty(so);
                }
                dic[id] = so;
            }
            return dic;
        }

        private static Dictionary<string, ReceitaSO> CriarReceitas(Dictionary<string, IngredienteSO> i)
        {
            CriarPasta(PastaReceitas);
            PassoReceita P(string id, int q) => new PassoReceita(i[id], q);

            return new Dictionary<string, ReceitaSO>
            {
                // Segunda (fáceis)
                ["Mussarela"] = CriarReceita("Mussarela", "Mussarela", 8f, 20,
                    P("Massa", 1), P("Molho", 2), P("Queijo", 3)),
                ["Marguerita"] = CriarReceita("Marguerita", "Marguerita", 9f, 25,
                    P("Massa", 1), P("Molho", 2), P("Queijo", 2), P("Tomate", 3), P("Manjericao", 2)),
                // Sábado (mais ingredientes)
                ["Calabresa"] = CriarReceita("Calabresa", "Calabresa", 9f, 28,
                    P("Massa", 1), P("Molho", 2), P("Queijo", 2), P("Calabresa", 3), P("Cebola", 2)),
                ["Napolitana"] = CriarReceita("Napolitana", "Napolitana", 9f, 28,
                    P("Massa", 1), P("Molho", 2), P("Queijo", 3), P("Tomate", 2), P("Azeitona", 2)),
                // Domingo (inventadas pela equipe)
                ["DoChef"] = CriarReceita("DoChef", "Pizza do Chef", 10f, 38,
                    P("Massa", 1), P("Molho", 1), P("Queijo", 2), P("Calabresa", 2), P("Cebola", 1), P("Azeitona", 2), P("Manjericao", 1)),
                ["Domingueira"] = CriarReceita("Domingueira", "Domingueira", 10f, 36,
                    P("Massa", 1), P("Molho", 3), P("Queijo", 1), P("Tomate", 2), P("Calabresa", 2), P("Manjericao", 2)),
            };
        }

        private static DiaSO[] CriarDias(Dictionary<string, ReceitaSO> r)
        {
            CriarPasta(PastaDias);
            return new[]
            {
                CriarDia("1_Segunda", "Segunda-feira", "Dia tranquilo: poucos clientes e sabores fáceis. As dicas mostram cada passo.",
                    180f, 4f, new Vector2(28f, 38f), 130f, 2, 3, true, r["Mussarela"], r["Marguerita"]),
                CriarDia("2_Sabado", "Sábado", "Mais clientes, menos paciência e sabores novos: Calabresa e Napolitana.",
                    210f, 3f, new Vector2(18f, 26f), 95f, 3, 3, false,
                    r["Mussarela"], r["Marguerita"], r["Calabresa"], r["Napolitana"]),
                CriarDia("3_Domingo", "Domingo", "Casa cheia! Clientes impacientes e as receitas inventadas do chef.",
                    240f, 3f, new Vector2(13f, 20f), 75f, 4, 4, false,
                    r["Marguerita"], r["Calabresa"], r["Napolitana"], r["DoChef"], r["Domingueira"]),
            };
        }

        private static DiaSO CriarDia(string id, string nome, string descricao, float duracao, float primeiro, Vector2 intervalo,
                                      float paciencia, int maxAbertos, int limitePerdidos, bool dicas, params ReceitaSO[] cardapio)
        {
            string caminho = $"{PastaDias}/{id}.asset";
            var existente = AssetDatabase.LoadAssetAtPath<DiaSO>(caminho);
            if (existente != null) return existente; // preserva ajustes de balanceamento feitos no Inspector

            var d = ScriptableObject.CreateInstance<DiaSO>();
            d.nomeExibicao = nome;
            d.descricao = descricao;
            d.duracao = duracao;
            d.primeiroPedidoEm = primeiro;
            d.intervaloEntrePedidos = intervalo;
            d.paciencia = paciencia;
            d.maxPedidosAbertos = maxAbertos;
            d.limiteClientesPerdidos = limitePerdidos;
            d.mostrarDicas = dicas;
            d.cardapio = new List<ReceitaSO>(cardapio);
            AssetDatabase.CreateAsset(d, caminho);
            return d;
        }

        private static ReceitaSO CriarReceita(string id, string nome, float forno, int preco, params PassoReceita[] passos)
        {
            string caminho = $"{PastaReceitas}/{id}.asset";
            var existente = AssetDatabase.LoadAssetAtPath<ReceitaSO>(caminho);
            if (existente != null) return existente;

            var r = ScriptableObject.CreateInstance<ReceitaSO>();
            r.nomeExibicao = nome;
            r.tempoForno = forno;
            r.preco = preco;
            r.passos = new List<PassoReceita>(passos);
            AssetDatabase.CreateAsset(r, caminho);
            return r;
        }

        // ------------------------------------------------------------------ Helpers

        private static Transform Ponto(string nome, Transform pai, Vector3 posMundo)
        {
            var t = new GameObject(nome).transform;
            t.SetParent(pai, true);
            t.position = posMundo;
            return t;
        }

        /// <summary>Primitiva sem collider (decoração de NPC).</summary>
        private static GameObject Peca(PrimitiveType tipo, string nome, Transform pai, Vector3 posLocal, Vector3 escala, Material mat, Vector3 rotLocal = default)
        {
            var go = GameObject.CreatePrimitive(tipo);
            go.name = nome;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(pai, false);
            go.transform.localPosition = posLocal;
            go.transform.localRotation = Quaternion.Euler(rotLocal);
            go.transform.localScale = escala;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        private static void AtribuirLista(Object alvo, string campo, params Object[] valores)
        {
            var so = new SerializedObject(alvo);
            var prop = so.FindProperty(campo);
            if (prop == null || !prop.isArray)
            {
                Debug.LogError($"[Pizzaria] Lista '{campo}' não encontrada em {alvo.GetType().Name}");
                return;
            }
            prop.arraySize = valores.Length;
            for (int i = 0; i < valores.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = valores[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject Bloco(string nome, Transform pai, Vector3 posMundo, Vector3 tamanho, Material mat, bool semCollider = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = nome;
            go.transform.SetParent(pai, true);
            go.transform.position = posMundo;
            go.transform.localScale = Vector3.one;
            // escala em mundo, independente do pai
            var escalaPai = pai != null ? pai.lossyScale : Vector3.one;
            go.transform.localScale = new Vector3(tamanho.x / escalaPai.x, tamanho.y / escalaPai.y, tamanho.z / escalaPai.z);
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.isStatic = true;
            if (semCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private static TextMesh Texto3D(Transform pai, string texto, Vector3 pos, Quaternion rot, float escala, Color cor)
        {
            var go = new GameObject("Texto " + texto);
            go.transform.SetParent(pai, true);
            go.transform.position = pos;
            go.transform.rotation = rot;

            var tm = go.AddComponent<TextMesh>();
            tm.text = texto;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontSize = 96;
            tm.characterSize = 0.022f * escala;
            tm.fontStyle = FontStyle.Bold;
            tm.color = cor;

            var fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (fonte != null)
            {
                tm.font = fonte;
                go.GetComponent<MeshRenderer>().sharedMaterial = MaterialTexto(fonte);
                go.AddComponent<Pizzaria.UI.TextoComProfundidade>();
            }
            return tm;
        }

        /// <summary>
        /// Material de texto com teste de profundidade. O material padrão da fonte desenha por cima de tudo,
        /// o que fazia o letreiro aparecer através da parede e as placas se sobreporem.
        /// </summary>
        private static Material MaterialTexto(Font fonte)
        {
            const string caminho = PastaMateriais + "/Texto3D.mat";
            var shader = Shader.Find("Pizzaria/Texto3D");
            if (shader == null) return fonte.material; // shader ainda não importado: usa o padrão
            var m = AssetDatabase.LoadAssetAtPath<Material>(caminho);
            if (m == null)
            {
                CriarPasta(PastaMateriais);
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, caminho);
            }
            m.shader = shader;
            m.mainTexture = fonte.material.mainTexture;
            EditorUtility.SetDirty(m);
            return m;
        }

        private static Material Mat(string nome, Color cor) => Mat(nome, cor, 0.25f);

        private static Material Mat(string nome, Color cor, float suavidade, float metal = 0f)
        {
            if (cacheMateriais.TryGetValue(nome, out var m) && m != null) return m;

            CriarPasta(PastaMateriais);
            string caminho = $"{PastaMateriais}/{nome}.mat";
            m = AssetDatabase.LoadAssetAtPath<Material>(caminho);
            if (m == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, caminho);
            }
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", cor);
            m.color = cor;
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", suavidade);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metal);
            EditorUtility.SetDirty(m);

            cacheMateriais[nome] = m;
            return m;
        }

        /// <summary>Material que brilha sozinho (lâmpadas, brasa, neon). Intensidade &gt; 1 ativa o bloom.</summary>
        private static Material MatEmissivo(string nome, Color cor, float intensidade)
        {
            var m = Mat(nome, cor, 0.5f);
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", cor * intensidade);
            EditorUtility.SetDirty(m);
            return m;
        }

        private static void Atribuir(Object alvo, string campo, Object valor)
        {
            var so = new SerializedObject(alvo);
            var prop = so.FindProperty(campo);
            if (prop == null)
            {
                Debug.LogError($"[Pizzaria] Campo '{campo}' não encontrado em {alvo.GetType().Name}");
                return;
            }
            prop.objectReferenceValue = valor;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CriarPasta(string caminho)
        {
            if (AssetDatabase.IsValidFolder(caminho)) return;
            string pai = Path.GetDirectoryName(caminho).Replace('\\', '/');
            CriarPasta(pai);
            AssetDatabase.CreateFolder(pai, Path.GetFileName(caminho));
        }

        private static void RegistrarNoBuild()
        {
            var cenas = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(CaminhoCena, true) };
            EditorBuildSettings.scenes = cenas.ToArray();
        }
    }

    /// <summary>Gera a cozinha automaticamente na primeira vez que o projeto abre.</summary>
    [InitializeOnLoad]
    internal static class GeradorAutomatico
    {
        private const string Chave = "Pizzaria_GeradorChecado_" + GeradorCozinha.MarcaVersao; // nova versão = checa de novo

        static GeradorAutomatico()
        {
            if (Application.isBatchMode) return; // linha de comando: quem chama decide (BuildPizzaria.BuildWindowsLote)
            if (SessionState.GetBool(Chave, false)) return;
            SessionState.SetBool(Chave, true);

            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                if (!File.Exists(GeradorCozinha.CaminhoCena)) { GeradorCozinha.Gerar(); return; }

                // Cena feita por uma versão antiga do gerador: oferece regenerar (senão o visual novo nunca aparece)
                if (File.ReadAllText(GeradorCozinha.CaminhoCena).Contains(GeradorCozinha.MarcaVersao)) return;
                if (EditorUtility.DisplayDialog("Pizzaria",
                        "A cena Cozinha.unity foi gerada por uma versão antiga do gerador e não tem o visual novo.\n\nGerar a cozinha de novo agora?",
                        "Gerar de novo", "Agora não")
                    && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    GeradorCozinha.Gerar();
                }
            };
        }
    }
}
