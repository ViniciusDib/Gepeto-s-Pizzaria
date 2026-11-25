using System.Text;
using Pizzaria.Interacao;
using Pizzaria.Jogador;
using Pizzaria.Jogo;
using Pizzaria.Pedidos;
using UnityEngine;

namespace Pizzaria.UI
{
    /// <summary>
    /// HUD provisório em IMGUI (mira, dica de interação, inventário, avisos).
    /// Será substituído por uma UI de verdade na fase de arte.
    /// </summary>
    public class HudPrototipo : MonoBehaviour
    {
        [SerializeField] private InteracaoJogador interacao;
        [SerializeField] private ControladorJogador controlador;
        [SerializeField] private DicasTutorial dicas;

        private GUIStyle estiloCentro, estiloCaixa, estiloAviso, estiloDica, estiloComanda;
        private float escalaEstilos = -1f;
        private readonly StringBuilder sb = new StringBuilder();

        private void CriarEstilos(float escala)
        {
            if (estiloCentro != null && Mathf.Approximately(escala, escalaEstilos)) return;
            escalaEstilos = escala;

            estiloCentro = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                fontSize = Mathf.RoundToInt(20 * escala),
                richText = true,
                wordWrap = true
            };
            estiloCentro.normal.textColor = Color.white;

            estiloCaixa = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = Mathf.RoundToInt(17 * escala),
                richText = true,
                wordWrap = true,
                padding = new RectOffset(Px(12, escala), Px(12, escala), Px(8, escala), Px(8, escala))
            };
            estiloCaixa = EstiloCantina.Caixa(estiloCaixa);
            estiloComanda = EstiloCantina.Papel(estiloCaixa);

            estiloAviso = new GUIStyle(estiloCentro) { fontSize = Mathf.RoundToInt(24 * escala), fontStyle = FontStyle.Bold };
            estiloAviso.normal.textColor = EstiloCantina.Dourado;

            estiloDica = new GUIStyle(estiloCaixa) { alignment = TextAnchor.MiddleCenter, fontSize = Mathf.RoundToInt(20 * escala), wordWrap = true };
            estiloDica.normal.background = EstiloCantina.Arredondado(EstiloCantina.Manjericao, new Color(0.6f, 0.85f, 0.6f, 0.6f), 1.2f);
            estiloDica.normal.textColor = EstiloCantina.Creme;
        }

        private void OnGUI()
        {
            if (interacao == null) return;
            var jogo = GerenciadorJogo.Instancia;
            if (jogo != null && jogo.Estado != EstadoJogo.Jogando) return; // TelasJogo cuida das outras telas
            // nunca menor que 0.6: na janela Game pequena o texto continua legível
            float escala = Mathf.Max(0.6f, Screen.height / 1080f);
            CriarEstilos(escala);

            float cx = Screen.width * 0.5f, cy = Screen.height * 0.5f;

            if (controlador != null && !controlador.CursorTravado)
            {
                GUI.Label(new Rect(0, cy - 40 * escala, Screen.width, 80 * escala),
                          "<b>Clique para voltar ao jogo</b>\nWASD andar · Shift correr · Clique/E usar · Botão direito/Q pegar\nRoda do mouse ou 1-9 escolher ingrediente · Esc pausar",
                          estiloCentro);
                return;
            }

            // Mira
            float t = 6f * escala;
            var corAntiga = GUI.color;
            GUI.color = interacao.Alvo != null ? new Color(1f, 0.85f, 0.3f) : new Color(1f, 1f, 1f, 0.8f);
            GUI.DrawTexture(new Rect(cx - t * 0.5f, cy - t * 0.5f, t, t), Texture2D.whiteTexture);
            GUI.color = corAntiga;

            // Dica do alvo
            if (interacao.Alvo != null)
                Sombra(new Rect(cx - 300 * escala, cy + 24 * escala, 600 * escala, 80 * escala),
                       interacao.Alvo.TextoInteracao(interacao), estiloCentro);

            // Aviso
            string aviso = interacao.MensagemAtual;
            if (!string.IsNullOrEmpty(aviso))
                Sombra(new Rect(0, Screen.height * 0.25f, Screen.width, 50 * escala), aviso, estiloAviso);

            // Inventário
            var inv = interacao.Inventario;
            sb.Clear().Append("<b>Nas mãos</b>");
            int linhas = 1;
            if (inv.Pizza != null)
            {
                sb.Append($"\n<b>Pizza</b> ({inv.Pizza.DescricaoEstado()})\n  <size={Mathf.RoundToInt(14 * escala)}>{inv.Pizza.DescricaoIngredientes()}</size>");
                linhas += 2;
            }
            if (inv.Vazio && inv.Pizza == null) sb.Append("\n<i>nada</i>");
            for (int i = 0; i < inv.Tipos.Count; i++)
            {
                var ing = inv.Tipos[i];
                string hex = ColorUtility.ToHtmlStringRGB(ing.cor);
                string linha = $"{i + 1}. <color=#{hex}>■</color> {ing.Nome} × {inv.Quantidade(ing)}";
                sb.Append('\n').Append(ing == inv.Selecionado ? $"<color=#ffd54a><b>▶ {linha}</b></color>" : $"   {linha}");
                linhas++;
            }
            string inventario = sb.ToString();
            float altura = Altura(estiloCaixa, inventario, 300 * escala, (14 + 26 * Mathf.Max(2, linhas)) * escala);
            GUI.Box(new Rect(16 * escala, Screen.height - altura - 16 * escala, 300 * escala, altura), inventario, estiloCaixa);

            DesenharComandas(escala);
            DesenharDiaEDica(escala, jogo);
        }

        private void DesenharDiaEDica(float escala, GerenciadorJogo jogo)
        {
            if (jogo == null) return;
            var dia = jogo.Dia;
            string relogio = jogo.Fechado ? "<color=#ff9a6a>FECHADO</color> · termine as comandas" : $"fecha em {TelasJogo.Relogio(jogo.TempoRestante)}";
            int limite = dia != null ? dia.limiteClientesPerdidos : 0;
            string corPerdidos = jogo.Pedidos.ClientesPerdidos >= limite - 1 ? "ff6a5a" : "ffffff";
            string topo = $"<b>{(dia != null ? dia.Nome : "")}</b>  ·  {relogio}  ·  perdidos <color=#{corPerdidos}>{jogo.Pedidos.ClientesPerdidos}/{limite}</color>";
            float larguraTopo = 480 * escala;
            GUI.Box(new Rect(16 * escala, 16 * escala, larguraTopo, Altura(estiloCaixa, topo, larguraTopo, 40 * escala)), topo, estiloCaixa);

            string dica = dicas != null ? dicas.Dica : null;
            if (!string.IsNullOrEmpty(dica))
            {
                string txt = "<b>Dica:</b> " + dica;
                float h = Altura(estiloDica, txt, 760 * escala, 70 * escala);
                GUI.Box(new Rect(Screen.width * 0.5f - 380 * escala, Screen.height - 40 * escala - h, 760 * escala, h), txt, estiloDica);
            }
        }

        private void DesenharComandas(float escala)
        {
            var g = GerenciadorPedidos.Instancia;
            if (g == null) return;

            float largura = 380 * escala, x = Screen.width - largura - 16 * escala, y = 16 * escala;

            string resumo = $"<b>R$ {g.Dinheiro}</b>    entregues {g.PizzasEntregues} · recusadas {g.PizzasRecusadas} · perdidos {g.ClientesPerdidos}";
            float hResumo = Altura(estiloCaixa, resumo, largura, 40 * escala);
            GUI.Box(new Rect(x, y, largura, hResumo), resumo, estiloCaixa);
            y += hResumo + 8 * escala;

            if (g.Pedidos.Count == 0)
            {
                const string aguardando = "<i>Aguardando clientes...</i>";
                GUI.Box(new Rect(x, y, largura, Altura(estiloCaixa, aguardando, largura, 40 * escala)), aguardando, estiloCaixa);
                return;
            }

            foreach (var p in g.Pedidos)
            {
                string comanda =
                        $"<b>#{p.Numero} {p.Receita.Nome}</b>  <size={Mathf.RoundToInt(14 * escala)}>R$ {p.Receita.preco} · forno {p.Receita.tempoForno:0}s</size>\n" +
                        $"<size={Mathf.RoundToInt(15 * escala)}>{p.Receita.DescricaoPassos()}</size>";
                // + espaço embaixo para a barra de paciência não cobrir o texto
                float h = Altura(estiloComanda, comanda, largura, 60 * escala) + 10 * escala;
                GUI.Box(new Rect(x, y, largura, h), comanda, estiloComanda);

                // Barra de paciência
                var corAntiga = GUI.color;
                var barra = new Rect(x + 12 * escala, y + h - 14 * escala, largura - 24 * escala, 6 * escala);
                GUI.color = new Color(0.3f, 0.2f, 0.12f, 0.35f);
                GUI.DrawTexture(barra, Texture2D.whiteTexture);
                ColorUtility.TryParseHtmlString("#" + PainelPedidos.CorPaciencia(p.Fracao), out var cor);
                GUI.color = cor;
                barra.width *= p.Fracao;
                GUI.DrawTexture(barra, Texture2D.whiteTexture);
                GUI.color = corAntiga;

                y += h + 6 * escala;
            }
        }

        private static int Px(float valor, float escala) => Mathf.Max(2, Mathf.RoundToInt(valor * escala));

        /// <summary>Altura que o texto precisa nessa largura (com quebra de linha), nunca menor que o mínimo.</summary>
        private static float Altura(GUIStyle estilo, string texto, float largura, float minimo) =>
            Mathf.Max(minimo, Mathf.Ceil(estilo.CalcHeight(new GUIContent(texto), largura)));

        private static void Sombra(Rect r, string texto, GUIStyle estilo)
        {
            var cor = estilo.normal.textColor;
            estilo.normal.textColor = new Color(0, 0, 0, 0.75f);
            GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), texto, estilo);
            estilo.normal.textColor = cor;
            GUI.Label(r, texto, estilo);
        }
    }
}
