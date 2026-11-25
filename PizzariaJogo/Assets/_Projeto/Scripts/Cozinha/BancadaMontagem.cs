using System.Text;
using Pizzaria.Audio;
using Pizzaria.Dados;
using Pizzaria.Interacao;
using UnityEngine;

namespace Pizzaria.Cozinha
{
    /// <summary>
    /// Onde a pizza é montada: clique com massa abre a pizza, cada clique seguinte coloca
    /// 1 unidade do ingrediente selecionado. Botão direito pega a pizza.
    /// </summary>
    public class BancadaMontagem : Interagivel
    {
        [SerializeField] private Transform pontoPizza;

        private Pizza pizza;
        private PizzaVisual visual;
        private readonly StringBuilder sb = new StringBuilder();

        /// <summary>Pizza em cima da bancada (ou null).</summary>
        public Pizza Pizza => pizza;

        public override string TextoInteracao(InteracaoJogador jogador)
        {
            var inv = jogador.Inventario;
            var sel = inv.Selecionado;
            sb.Clear();

            if (pizza == null)
            {
                if (inv.Pizza != null) return "[Clique] Colocar a pizza na bancada";
                if (sel != null && sel.forma == FormaIngrediente.Base) return $"[Clique] Abrir a {sel.Nome}";
                return "Bancada de Montagem\n<i>Pegue massa para começar uma pizza</i>";
            }

            sb.Append("<b>Pizza:</b> ").Append(pizza.DescricaoIngredientes());
            if (pizza.JaFoiAoForno) sb.Append(" (").Append(pizza.DescricaoEstado()).Append(')');

            if (!pizza.JaFoiAoForno && sel != null && sel.forma != FormaIngrediente.Base)
                sb.Append($"\n[Clique] Colocar {sel.Nome}  ({inv.Quantidade(sel)} na mão)");
            else if (!pizza.JaFoiAoForno)
                sb.Append("\n<i>Pegue ingredientes para colocar</i>");

            if (inv.Pizza == null) sb.Append("\n[Botão direito] Pegar a pizza");
            return sb.ToString();
        }

        public override void Interagir(InteracaoJogador jogador)
        {
            var inv = jogador.Inventario;
            var sel = inv.Selecionado;

            if (pizza == null)
            {
                if (inv.Pizza != null)
                {
                    Colocar(inv.SoltarPizza());
                    return;
                }
                if (sel == null) { jogador.Avisar("Pegue massa para começar a pizza!"); return; }
                if (sel.forma != FormaIngrediente.Base) { jogador.Avisar($"Abra a massa antes de colocar {sel.Nome}!"); return; }

                inv.Remover(sel, 1);
                var nova = new Pizza();
                nova.Adicionar(sel);
                Colocar(nova);
                Sons.Tocar(Sons.Colocar, pontoPizza.position, 1f);
                return;
            }

            if (pizza.JaFoiAoForno) { jogador.Avisar("Essa pizza já foi ao forno!"); return; }
            if (sel == null) { jogador.Avisar("Suas mãos estão vazias: pegue ingredientes!"); return; }
            if (sel.forma == FormaIngrediente.Base) { jogador.Avisar("Essa pizza já tem massa!"); return; }

            inv.Remover(sel, 1);
            pizza.Adicionar(sel);
            Sons.Tocar(Sons.Colocar, pontoPizza.position, 0.9f);
        }

        public override void InteragirSecundario(InteracaoJogador jogador)
        {
            if (pizza == null) return;
            if (!jogador.Inventario.SegurarPizza(pizza))
            {
                jogador.Avisar("Você já está segurando uma pizza!");
                return;
            }
            pizza = null;
            if (visual != null) Destroy(visual.gameObject);
            visual = null;
        }

        private void Colocar(Pizza p)
        {
            pizza = p;
            visual = PizzaVisual.Criar(p, pontoPizza != null ? pontoPizza : transform);
        }
    }
}
