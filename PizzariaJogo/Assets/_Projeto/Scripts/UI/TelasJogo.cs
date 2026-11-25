using Pizzaria.Audio;
using Pizzaria.Jogador;
using Pizzaria.Jogo;
using UnityEngine;

namespace Pizzaria.UI
{
    /// <summary>
    /// Telas de estado em IMGUI: menu inicial, como jogar, opções, recordes, pausa, fim do dia, derrota e vitória.
    /// Toda tela tem uma saída (continuar, reiniciar ou menu) — nunca há beco sem saída.
    /// </summary>
    public class TelasJogo : MonoBehaviour
    {
        private const string TextoCreditos =
            "<b>Equipe</b>\nVinicius Dib  ·  Greta Faria  ·  Thomas Rudge  ·  Enzo Christofoletti\n\n" +
            "<b>Projeto 3D</b>\nInsper · 2026\n\n" +
            "<b>Arte, música e sons</b>\nCenário, modelos, texturas, trilha (tarantela) e efeitos sonoros\ngerados por código no próprio projeto, sem assets de terceiros.\n\n" +
            "<b>Ferramentas</b>\nUnity 6 (URP) · Input System\nParte do código escrita com auxílio do Claude Code (Anthropic).\n\n" +
            "<i>Grazie mille por jogar!</i>";

        private const string TextoComoJogar =
            "<b>Objetivo</b>\nSobreviva aos 3 dias de trabalho (segunda, sábado e domingo) sem deixar clientes demais irem embora bravos.\n\n" +
            "<b>Como fazer uma pizza</b>\n" +
            "1. Veja a comanda (canto da tela ou painel PEDIDOS).\n" +
            "2. Pegue os ingredientes nas estações do fundo (cada uma tem placa colorida).\n" +
            "3. Na BANCADA, clique uma vez por ingrediente, NA ORDEM DA RECEITA, começando pela massa.\n" +
            "4. Pegue a pizza (botão direito) e coloque no FORNO. Tire quando apitar, antes de queimar.\n" +
            "5. Deixe no BALCÃO. O garçom leva; pizza errada volta e vai para a LIXEIRA.\n\n" +
            "<b>Controles</b>\nWASD andar · Shift correr · Mouse olhar\nClique/E usar · Botão direito/Q pegar/devolver\nRoda do mouse ou 1-9 escolher ingrediente · Esc/P pausar";

        private enum SubTela { Nenhuma, ComoJogar, Opcoes, Recordes, Creditos }

        private SubTela sub;
        private string nomeJogador;
        private int posicaoSalva; // > 0 depois de salvar o recorde desta tela
        private EstadoJogo estadoAnterior = EstadoJogo.Jogando;
        private GUIStyle titulo, texto, botao, caixa, campo, papel, linha, linhaDestaque;
        private float escalaEstilos = -1f;
        private Texture2D fundo;

        private void Estilos(float escala)
        {
            if (Mathf.Approximately(escala, escalaEstilos) && titulo != null) return;
            escalaEstilos = escala;

            titulo = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = Mathf.RoundToInt(56 * escala), fontStyle = FontStyle.Bold, richText = true };
            titulo.normal.textColor = EstiloCantina.Dourado;
            texto = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperCenter, fontSize = Mathf.RoundToInt(22 * escala), richText = true, wordWrap = true };
            texto.normal.textColor = EstiloCantina.Creme;
            botao = EstiloCantina.Botao(Mathf.RoundToInt(26 * escala));
            caixa = new GUIStyle(GUI.skin.box);
            if (fundo == null)
            {
                fundo = new Texture2D(1, 1);
                fundo.SetPixel(0, 0, new Color(0.10f, 0.05f, 0.03f, 0.80f));
                fundo.Apply();
            }
            caixa.normal.background = fundo;

            campo = EstiloCantina.Papel(GUI.skin.textField);
            campo.fontSize = Mathf.RoundToInt(26 * escala);
            campo.alignment = TextAnchor.MiddleCenter;
            campo.padding = new RectOffset(10, 10, 4, 4);
            campo.focused.background = campo.hover.background = campo.normal.background;
            campo.focused.textColor = campo.hover.textColor = campo.normal.textColor;
            papel = EstiloCantina.Papel(GUI.skin.box);
            linha = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(24 * escala), richText = true, alignment = TextAnchor.MiddleLeft };
            linha.normal.textColor = EstiloCantina.TextoEscuro;
            linhaDestaque = new GUIStyle(linha) { fontStyle = FontStyle.Bold };
            linhaDestaque.normal.textColor = EstiloCantina.TomateEscuro;
        }

        private void OnGUI()
        {
            var jogo = GerenciadorJogo.Instancia;
            if (jogo == null || jogo.Estado == EstadoJogo.Jogando) { estadoAnterior = EstadoJogo.Jogando; return; }
            if (jogo.Estado != estadoAnterior)
            {
                // entrou numa tela nova: volta ao principal e prepara o registro do recorde
                estadoAnterior = jogo.Estado;
                sub = SubTela.Nenhuma;
                posicaoSalva = 0;
                nomeJogador = Placar.UltimoNome;
            }

            float escala = Screen.height / 1080f;
            Estilos(escala);
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, caixa);

            float largura = 720 * escala, x = (Screen.width - largura) / 2f, y = Screen.height * 0.14f;

            switch (sub)
            {
                case SubTela.ComoJogar:
                    Titulo(new Rect(x, y, largura, 80 * escala), "Como jogar", escala);
                    GUI.Label(new Rect(x - 100 * escala, y + 90 * escala, largura + 200 * escala, 560 * escala), TextoComoJogar, texto);
                    if (Botao(x, y + 670 * escala, largura, escala, "Voltar")) sub = SubTela.Nenhuma;
                    return;
                case SubTela.Opcoes:
                    Titulo(new Rect(x, y, largura, 80 * escala), "Opções", escala);
                    Sensibilidade(x, y + 150 * escala, largura, escala);
                    float m = Deslizar(x, y + 260 * escala, largura, escala, $"Música: {Mathf.RoundToInt(Musica.Volume * 100)}%", Musica.Volume, 0f, 1f);
                    if (!Mathf.Approximately(m, Musica.Volume)) Musica.Volume = m;
                    float e = Deslizar(x, y + 370 * escala, largura, escala, $"Efeitos sonoros: {Mathf.RoundToInt(Sons.Volume * 100)}%", Sons.Volume, 0f, 1f);
                    if (!Mathf.Approximately(e, Sons.Volume))
                    {
                        Sons.Volume = e;
                        // amostra do volume enquanto arrasta (sem metralhar)
                        if (Time.unscaledTime > proximoTesteSom) { Sons.Tocar2D(Sons.Pegar); proximoTesteSom = Time.unscaledTime + 0.25f; }
                    }
                    if (Botao(x, y + 500 * escala, largura, escala, "Voltar")) sub = SubTela.Nenhuma;
                    return;
                case SubTela.Creditos:
                    Titulo(new Rect(x, y, largura, 80 * escala), "Créditos", escala);
                    GUI.Label(new Rect(x - 100 * escala, y + 110 * escala, largura + 200 * escala, 520 * escala), TextoCreditos, texto);
                    if (Botao(x, y + 670 * escala, largura, escala, "Voltar")) sub = SubTela.Nenhuma;
                    return;
                case SubTela.Recordes:
                    Titulo(new Rect(x, y, largura, 80 * escala), "Recordes", escala);
                    TabelaRecordes(x, y + 120 * escala, largura, escala, jogo.IdPartida, jogo.TotalDias);
                    if (Botao(x, y + 720 * escala, largura, escala, "Voltar")) sub = SubTela.Nenhuma;
                    return;
            }

            var dia = jogo.Dia;
            var ped = jogo.Pedidos;
            string nomeDia = dia != null ? dia.Nome : "";
            float by = y + 300 * escala;

            switch (jogo.Estado)
            {
                case EstadoJogo.Menu:
                    Titulo(new Rect(x, y, largura, 90 * escala), "Gepeto's Pizzaria", escala);
                    GUI.Label(new Rect(x, y + 100 * escala, largura, 150 * escala),
                        "Você é o cozinheiro. Monte as pizzas na ordem da receita, asse sem queimar e entregue no balcão.\nSobreviva ao fim de semana!", texto);
                    if (Botao(x, by - 50 * escala, largura, escala, "Jogar")) jogo.NovoJogo();
                    if (Botao(x, by + 22 * escala, largura, escala, "Como jogar")) sub = SubTela.ComoJogar;
                    if (Botao(x, by + 94 * escala, largura, escala, "Recordes")) sub = SubTela.Recordes;
                    if (Botao(x, by + 166 * escala, largura, escala, "Opções")) sub = SubTela.Opcoes;
                    if (Botao(x, by + 238 * escala, largura, escala, "Créditos")) sub = SubTela.Creditos;
                    if (GerenciadorJogo.PodeSair && Botao(x, by + 310 * escala, largura, escala, "Sair")) jogo.Sair();
                    break;

                case EstadoJogo.Pausado:
                    Titulo(new Rect(x, y, largura, 90 * escala), "Pausado", escala);
                    GUI.Label(new Rect(x, y + 100 * escala, largura, 60 * escala), $"{nomeDia} · {Relogio(jogo.TempoRestante)} para fechar", texto);
                    if (Botao(x, by - 80 * escala, largura, escala, "Continuar")) jogo.Continuar();
                    if (Botao(x, by, largura, escala, "Reiniciar o dia")) jogo.ReiniciarDia();
                    if (Botao(x, by + 80 * escala, largura, escala, "Como jogar")) sub = SubTela.ComoJogar;
                    if (Botao(x, by + 160 * escala, largura, escala, "Opções")) sub = SubTela.Opcoes;
                    if (Botao(x, by + 240 * escala, largura, escala, "Menu principal")) jogo.IrParaMenu();
                    break;

                case EstadoJogo.FimDoDia:
                    Titulo(new Rect(x, y, largura, 90 * escala), $"Fim de {nomeDia}!", escala);
                    GUI.Label(new Rect(x, y + 100 * escala, largura, 180 * escala), Resumo(jogo), texto);
                    if (Botao(x, by, largura, escala, "Próximo dia")) jogo.ProximoDia();
                    if (Botao(x, by + 80 * escala, largura, escala, "Repetir o dia")) jogo.ReiniciarDia();
                    if (Botao(x, by + 160 * escala, largura, escala, "Menu principal")) jogo.IrParaMenu();
                    break;

                case EstadoJogo.Derrota:
                    Titulo(new Rect(x, y, largura, 90 * escala), "<color=#ff6a5a>Clientes bravos demais!</color>", escala);
                    GUI.Label(new Rect(x, y + 100 * escala, largura, 180 * escala),
                        $"{ped.ClientesPerdidos} clientes foram embora em {nomeDia}.\nVocê perdeu o fim de semana... mas pode tentar este dia de novo.\n\n" + Resumo(jogo), texto);
                    RegistroRecorde(jogo, x, by + 20 * escala, largura, escala);
                    if (Botao(x, by + 110 * escala, largura, escala, "Tentar o dia de novo")) jogo.ReiniciarDia();
                    if (Botao(x, by + 190 * escala, largura, escala, "Recomeçar o fim de semana")) jogo.NovoJogo();
                    if (Botao(x, by + 270 * escala, largura, escala, "Menu principal")) jogo.IrParaMenu();
                    break;

                case EstadoJogo.Vitoria:
                    Titulo(new Rect(x, y, largura, 90 * escala), "Fim de semana concluído!", escala);
                    GUI.Label(new Rect(x, y + 100 * escala, largura, 180 * escala),
                        $"Você sobreviveu aos {jogo.TotalDias} dias!\n<b>Total ganho: R$ {jogo.DinheiroTotal}</b>\n\n" + Resumo(jogo), texto);
                    RegistroRecorde(jogo, x, by + 20 * escala, largura, escala);
                    if (Botao(x, by + 110 * escala, largura, escala, "Jogar de novo")) jogo.NovoJogo();
                    if (Botao(x, by + 190 * escala, largura, escala, "Menu principal")) jogo.IrParaMenu();
                    break;
            }
        }

        private void Titulo(Rect r, string rotulo, float escala)
        {
            var cor = titulo.normal.textColor;
            titulo.normal.textColor = new Color(0.35f, 0.08f, 0.05f, 0.9f);
            GUI.Label(new Rect(r.x + 3 * escala, r.y + 4 * escala, r.width, r.height), StripCor(rotulo), titulo);
            titulo.normal.textColor = cor;
            GUI.Label(r, rotulo, titulo);
            EstiloCantina.FaixaTricolor(new Rect(r.center.x - 90 * escala, r.yMax + 2 * escala, 180 * escala, 6 * escala));
        }

        /// <summary>Remove tags de cor para desenhar a sombra do título numa cor só.</summary>
        private static string StripCor(string s) => System.Text.RegularExpressions.Regex.Replace(s, "</?color[^>]*>", "");

        private static string Resumo(GerenciadorJogo jogo)
        {
            var p = jogo.Pedidos;
            return $"Ganho no dia: R$ {p.Dinheiro}  ·  Total: R$ {jogo.DinheiroTotal}\n" +
                   $"Entregues {p.PizzasEntregues}  ·  Recusadas {p.PizzasRecusadas}  ·  Clientes perdidos {p.ClientesPerdidos}";
        }

        private float proximoTesteSom;

        private bool Botao(float x, float y, float largura, float escala, string rotulo) =>
            BotaoEm(new Rect(x + largura * 0.2f, y, largura * 0.6f, 62 * escala), rotulo);

        private bool BotaoEm(Rect r, string rotulo)
        {
            if (!GUI.Button(r, rotulo, botao)) return false;
            Sons.Tocar2D(Sons.Clique);
            return true;
        }

        private float Deslizar(float x, float y, float largura, float escala, string rotulo, float atual, float min, float max)
        {
            GUI.Label(new Rect(x, y, largura, 34 * escala), rotulo, texto);
            return GUI.HorizontalSlider(new Rect(x + largura * 0.2f, y + 44 * escala, largura * 0.6f, 24 * escala), atual, min, max);
        }

        private void Sensibilidade(float x, float y, float largura, float escala)
        {
            float atual = ControladorJogador.MultiplicadorSensibilidade;
            float novo = Deslizar(x, y, largura, escala, $"Sensibilidade do mouse: {atual:0.0}x", atual, 0.2f, 3f);
            if (!Mathf.Approximately(novo, atual)) ControladorJogador.MultiplicadorSensibilidade = novo;
        }

        /// <summary>Fim da partida: campo de nome + "Salvar recorde" (ou a posição já alcançada).</summary>
        private void RegistroRecorde(GerenciadorJogo jogo, float x, float y, float largura, float escala)
        {
            float h = 62 * escala;
            int dias = jogo.DiasCompletos, dinheiro = jogo.DinheiroTotal;
            if (posicaoSalva > 0)
            {
                string msg = posicaoSalva == 1 ? "<color=#ffcc61><b>★ NOVO RECORDE! ★</b></color>  1º lugar no placar" : $"Recorde salvo: {posicaoSalva}º lugar no placar";
                GUI.Label(new Rect(x, y + 12 * escala, largura * 0.62f, h), msg, texto);
                if (BotaoEm(new Rect(x + largura * 0.64f, y, largura * 0.3f, h), "Ver recordes")) sub = SubTela.Recordes;
                return;
            }
            int pos = Placar.PosicaoPara(jogo.IdPartida, dias, dinheiro);
            if (pos == 0)
            {
                GUI.Label(new Rect(x, y + 12 * escala, largura, h), "Não entrou no top 10 desta vez... tente de novo!", texto);
                return;
            }
            GUI.Label(new Rect(x - 60 * escala, y + 12 * escala, largura * 0.2f, h), pos == 1 ? "<color=#ffcc61><b>Recorde!</b></color>" : $"{pos}º lugar:", texto);
            GUI.SetNextControlName("NomeRecorde");
            nomeJogador = GUI.TextField(new Rect(x + largura * 0.12f, y, largura * 0.44f, h), nomeJogador ?? "", Placar.TamanhoNome, campo);
            bool enter = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return &&
                         GUI.GetNameOfFocusedControl() == "NomeRecorde";
            if (BotaoEm(new Rect(x + largura * 0.58f, y, largura * 0.36f, h), "Salvar recorde") || enter)
            {
                posicaoSalva = Mathf.Max(1, Placar.Salvar(jogo.IdPartida, nomeJogador, dias, dinheiro));
                if (posicaoSalva == 1) Sons.Tocar2D(Sons.Vitoria, 0.6f);
                GUI.FocusControl(null);
            }
        }

        private void TabelaRecordes(float x, float y, float largura, float escala, string partidaAtual, int totalDias)
        {
            var lista = Placar.Recordes;
            float lh = 50 * escala, w = largura * 1.1f, x0 = x - largura * 0.05f;
            GUI.Box(new Rect(x0, y, w, lh * (Placar.Maximo + 1) + 20 * escala), GUIContent.none, papel);
            float[] col = { 0.04f, 0.14f, 0.52f, 0.70f, 0.86f };
            void Linha(float ly, GUIStyle st, string a, string b, string c, string d, string e)
            {
                string[] v = { a, b, c, d, e };
                for (int i = 0; i < v.Length; i++)
                {
                    float fim = i + 1 < col.Length ? col[i + 1] : 1f;
                    GUI.Label(new Rect(x0 + w * col[i], ly, w * (fim - col[i]), lh), v[i], st);
                }
            }
            float yy = y + 10 * escala;
            Linha(yy, linhaDestaque, "#", "Pizzaiolo", "Dias", "R$", "Data");
            if (lista.Count == 0)
            {
                GUI.Label(new Rect(x0, yy + lh * 2, w, lh), "<i>Ninguém ainda... jogue e salve o seu recorde!</i>",
                          new GUIStyle(linha) { alignment = TextAnchor.MiddleCenter });
                return;
            }
            for (int i = 0; i < lista.Count; i++)
            {
                var r = lista[i];
                var st = r.partida == partidaAtual ? linhaDestaque : linha;
                string medalha = i == 0 ? "<color=#b8860b>1</color>" : (i + 1).ToString();
                Linha(yy + lh * (i + 1), st, medalha, r.nome, $"{r.dias}/{totalDias}", r.dinheiro.ToString(), r.data);
            }
        }

        public static string Relogio(float segundos)
        {
            int s = Mathf.CeilToInt(Mathf.Max(0f, segundos));
            return $"{s / 60}:{s % 60:00}";
        }
    }
}
