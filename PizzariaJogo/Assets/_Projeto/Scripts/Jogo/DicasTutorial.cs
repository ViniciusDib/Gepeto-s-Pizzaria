using Pizzaria.Cozinha;
using Pizzaria.Dados;
using Pizzaria.Interacao;
using UnityEngine;

namespace Pizzaria.Jogo
{
    /// <summary>
    /// Tutorial contextual (só nos dias com "mostrarDicas"): olha o estado da cozinha
    /// e diz qual é o próximo passo para atender a comanda mais antiga.
    /// </summary>
    public class DicasTutorial : MonoBehaviour
    {
        [SerializeField] private InteracaoJogador jogador;
        [SerializeField] private BancadaMontagem bancada;
        [SerializeField] private Forno forno;

        private float proximaAtualizacao;

        public string Dica { get; private set; }

        private void Update()
        {
            if (Time.unscaledTime < proximaAtualizacao) return;
            proximaAtualizacao = Time.unscaledTime + 0.2f;
            Dica = Calcular();
        }

        private string Calcular()
        {
            var jogo = GerenciadorJogo.Instancia;
            if (jogo == null || jogo.Estado != EstadoJogo.Jogando || jogo.Dia == null || !jogo.Dia.mostrarDicas) return null;
            if (jogador == null || bancada == null) return null;

            var pedidos = jogo.Pedidos.Pedidos;
            var inv = jogador.Inventario;

            // 1) Pizza nas mãos
            if (inv.Pizza != null)
            {
                var p = inv.Pizza;
                if (p.Devolvida || p.Estado == EstadoPizza.Queimada) return "Essa pizza não serve mais: jogue na LIXEIRA e faça outra.";
                if (p.Estado == EstadoPizza.Pronta) return "Pizza assada! Leve até o BALCÃO (na frente da cozinha) e clique para o garçom levar.";
                return "Leve a pizza até o FORNO (parede direita) e clique para assar.";
            }

            if (forno != null && forno.TemPizzaPronta) return "O forno apitou! Vá até o FORNO e clique para tirar a pizza.";
            if (pedidos.Count == 0)
                return forno != null && forno.TemPizzaAssando
                    ? "A pizza está assando. Espere o apito!"
                    : "Aguarde: as comandas aparecem no canto da tela e no painel PEDIDOS.";

            ReceitaSO receita = pedidos[0].Receita;

            // 2) Pizza na bancada
            var naBancada = bancada.Pizza;
            if (naBancada != null)
            {
                if (naBancada.JaFoiAoForno) return "Pegue a pizza da bancada com o BOTÃO DIREITO.";
                var prox = naBancada.ProximoIngrediente(receita, out bool montada, out bool errada);
                if (montada) return $"{receita.Nome} montada! Pegue com o BOTÃO DIREITO e leve ao FORNO.";
                if (errada) return $"Ops, saiu da receita da {receita.Nome}. Pegue a pizza (BOTÃO DIREITO), jogue na LIXEIRA e recomece.";
                return PegarOuColocar(prox, inv, "na BANCADA");
            }

            // 3) Bancada vazia: começar pela massa
            if (forno != null && forno.TemPizzaAssando && pedidos.Count == 1) return "A pizza está assando. Espere o apito!";
            return PegarOuColocar(receita.passos.Count > 0 ? receita.passos[0].ingrediente : null, inv, "na BANCADA DE MONTAGEM");
        }

        private static string PegarOuColocar(IngredienteSO ing, Jogador.InventarioJogador inv, string onde)
        {
            if (ing == null) return null;
            if (inv.Quantidade(ing) <= 0) return $"Pegue {ing.Nome.ToUpperInvariant()} na estação com a placa {ing.Nome.ToUpperInvariant()}.";
            if (inv.Selecionado != ing) return $"Selecione {ing.Nome} (roda do mouse ou número) e clique {onde}.";
            return $"Clique {onde} para colocar {ing.Nome}.";
        }
    }
}
