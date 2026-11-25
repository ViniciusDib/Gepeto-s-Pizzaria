using System.Text;
using Pizzaria.Interacao;
using UnityEngine;

namespace Pizzaria.Pedidos
{
    /// <summary>Quadro de comandas na parede: mostra cada pedido com a receita e a paciência.</summary>
    public class PainelPedidos : Interagivel
    {
        [SerializeField] private TextMesh texto;

        private float proximaAtualizacao;
        private readonly StringBuilder sb = new StringBuilder();

        private void Update()
        {
            if (texto == null || Time.time < proximaAtualizacao) return;
            proximaAtualizacao = Time.time + 0.25f;

            var g = GerenciadorPedidos.Instancia;
            sb.Clear();
            if (g == null || g.Pedidos.Count == 0)
            {
                sb.Append("Nenhuma comanda no momento...");
            }
            else
            {
                foreach (var p in g.Pedidos)
                {
                    sb.Append($"<color=#{CorPaciencia(p.Fracao)}>#{p.Numero}  {p.Receita.Nome.ToUpperInvariant()}   ({Mathf.CeilToInt(p.PacienciaRestante)}s)</color>\n");
                    var passos = p.Receita.passos;
                    sb.Append("   ");
                    for (int i = 0; i < passos.Count; i++)
                    {
                        if (i > 0) sb.Append(i % 4 == 0 ? "\n   " : " > "); // quebra a cada 4 passos para caber no quadro
                        sb.Append(passos[i].ingrediente != null ? passos[i].ingrediente.Nome : "?").Append(' ').Append(passos[i].quantidade);
                    }
                    sb.Append("\n\n");
                }
            }
            texto.text = sb.ToString();
        }

        public static string CorPaciencia(float fracao) =>
            fracao > 0.5f ? "8fe388" : fracao > 0.25f ? "ffd54a" : "ff6a5a";

        public override string TextoInteracao(InteracaoJogador jogador) =>
            "Painel de Pedidos\n<i>As comandas também aparecem no canto da tela</i>";

        public override void Interagir(InteracaoJogador jogador) { }
    }
}
