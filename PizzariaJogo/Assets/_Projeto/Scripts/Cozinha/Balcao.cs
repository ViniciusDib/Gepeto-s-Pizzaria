using System.Text;
using Pizzaria.Audio;
using Pizzaria.Interacao;
using UnityEngine;

namespace Pizzaria.Cozinha
{
    /// <summary>
    /// Balcão de entrega: o jogador deixa a pizza assada, o garçom leva.
    /// Pizzas recusadas voltam pra cá marcadas como devolvidas.
    /// </summary>
    public class Balcao : Interagivel
    {
        [SerializeField] private Transform[] vagas;

        private Pizza[] pizzas;
        private PizzaVisual[] visuais;
        private readonly StringBuilder sb = new StringBuilder();

        public int TotalVagas => pizzas.Length;

        protected override void Awake()
        {
            base.Awake();
            int n = vagas != null ? vagas.Length : 0;
            pizzas = new Pizza[n];
            visuais = new PizzaVisual[n];
        }

        public Vector3 PosicaoVaga(int i) => vagas[i].position;

        /// <summary>Primeira pizza esperando o garçom (não devolvida).</summary>
        public bool ProcurarPizzaParaLevar(out int vaga)
        {
            for (vaga = 0; vaga < pizzas.Length; vaga++)
                if (pizzas[vaga] != null && !pizzas[vaga].Devolvida) return true;
            vaga = -1;
            return false;
        }

        public bool TemPizzaParaLevar(int vaga) =>
            vaga >= 0 && vaga < pizzas.Length && pizzas[vaga] != null && !pizzas[vaga].Devolvida;

        public int VagaLivre()
        {
            for (int i = 0; i < pizzas.Length; i++)
                if (pizzas[i] == null) return i;
            return -1;
        }

        public Pizza Retirar(int vaga)
        {
            var p = pizzas[vaga];
            pizzas[vaga] = null;
            if (visuais[vaga] != null) Destroy(visuais[vaga].gameObject);
            visuais[vaga] = null;
            return p;
        }

        /// <summary>Garçom devolve uma pizza recusada. Falha se o balcão estiver cheio.</summary>
        public bool Devolver(Pizza p, int vagaPreferida)
        {
            int vaga = vagaPreferida >= 0 && vagaPreferida < pizzas.Length && pizzas[vagaPreferida] == null ? vagaPreferida : VagaLivre();
            if (vaga < 0) return false;
            p.Devolvida = true;
            Colocar(p, vaga);
            return true;
        }

        private void Colocar(Pizza p, int vaga)
        {
            pizzas[vaga] = p;
            visuais[vaga] = PizzaVisual.Criar(p, vagas[vaga]);
        }

        public override string TextoInteracao(InteracaoJogador jogador)
        {
            var inv = jogador.Inventario;
            sb.Clear().Append("<b>Balcão</b>");
            bool temPizza = false, temDevolvida = false;
            for (int i = 0; i < pizzas.Length; i++)
            {
                if (pizzas[i] == null) continue;
                temPizza = true;
                if (pizzas[i].Devolvida) temDevolvida = true;
                sb.Append("   [").Append(i + 1).Append("] ").Append(pizzas[i].DescricaoEstado());
            }
            if (!temPizza) sb.Append(" <i>(vazio)</i>");

            if (inv.Pizza != null) sb.Append("\n[Clique] Deixar a pizza para o garçom");
            else if (temPizza) sb.Append(temDevolvida ? "\n[Clique] Pegar pizza devolvida" : "\n[Botão direito] Pegar a pizza de volta");
            return sb.ToString();
        }

        public override void Interagir(InteracaoJogador jogador)
        {
            var inv = jogador.Inventario;
            if (inv.Pizza == null)
            {
                PegarDevolvida(jogador);
                return;
            }

            if (inv.Pizza.Estado == EstadoPizza.Crua) { jogador.Avisar("A pizza ainda está crua: leve ao forno!"); return; }
            int vaga = VagaLivre();
            if (vaga < 0) { jogador.Avisar("O balcão está cheio!"); return; }

            var p = inv.SoltarPizza();
            p.Devolvida = false;
            Colocar(p, vaga);
            Sons.Tocar(Sons.Campainha, vagas[vaga].position, 0.8f);
        }

        public override void InteragirSecundario(InteracaoJogador jogador)
        {
            if (jogador.Inventario.Pizza != null) return;
            if (PegarDevolvida(jogador)) return;
            for (int i = 0; i < pizzas.Length; i++)
                if (pizzas[i] != null && jogador.Inventario.SegurarPizza(pizzas[i])) { Retirar(i); return; }
        }

        private bool PegarDevolvida(InteracaoJogador jogador)
        {
            for (int i = 0; i < pizzas.Length; i++)
            {
                if (pizzas[i] == null || !pizzas[i].Devolvida) continue;
                if (jogador.Inventario.SegurarPizza(pizzas[i])) Retirar(i);
                return true;
            }
            return false;
        }
    }
}
