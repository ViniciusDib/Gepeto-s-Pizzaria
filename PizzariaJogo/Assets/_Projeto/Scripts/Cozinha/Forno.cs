using System.Text;
using Pizzaria.Audio;
using Pizzaria.Interacao;
using Pizzaria.Pedidos;
using UnityEngine;

namespace Pizzaria.Cozinha
{
    /// <summary>
    /// Forno com vagas. A pizza crua entra, assa pelo tempo da receita, apita quando fica
    /// pronta e queima se ficar tempo demais.
    /// </summary>
    public class Forno : Interagivel
    {
        [SerializeField] private Transform[] vagas;
        [SerializeField, Tooltip("Intervalo (s) entre apitos enquanto tem pizza pronta esperando.")]
        private float intervaloLembrete = 3f;

        private Pizza[] pizzas;
        private PizzaVisual[] visuais;
        private float[] proximoApito;
        private readonly StringBuilder sb = new StringBuilder();

        protected override void Awake()
        {
            base.Awake();
            int n = vagas != null ? vagas.Length : 0;
            pizzas = new Pizza[n];
            visuais = new PizzaVisual[n];
            proximoApito = new float[n];
        }

        private void Update()
        {
            for (int i = 0; i < pizzas.Length; i++)
            {
                var p = pizzas[i];
                if (p == null) continue;

                var antes = p.Estado;
                p.Assar(Time.deltaTime);
                var depois = p.Estado;

                if (antes != depois && depois == EstadoPizza.Pronta)
                {
                    Notificador.Avisar("Pizza pronta! Tire do forno antes que queime.");
                    Apitar(i);
                }
                else if (antes != depois && depois == EstadoPizza.Queimada)
                {
                    Notificador.Avisar("Uma pizza queimou no forno!");
                    Sons.Tocar(Sons.Queimou, transform.position, 1f, 0.4f);
                }
                else if (depois == EstadoPizza.Pronta && Time.time >= proximoApito[i])
                {
                    Apitar(i);
                }
            }
        }

        public bool TemPizzaPronta
        {
            get
            {
                foreach (var p in pizzas) if (p != null && p.Estado != EstadoPizza.Crua) return true;
                return false;
            }
        }

        public bool TemPizzaAssando
        {
            get
            {
                foreach (var p in pizzas) if (p != null && p.Estado == EstadoPizza.Crua) return true;
                return false;
            }
        }

        private void Apitar(int vaga)
        {
            Sons.Tocar(Sons.Apito, transform.position, 1f, 0.4f);
            proximoApito[vaga] = Time.time + intervaloLembrete;
        }

        public override string TextoInteracao(InteracaoJogador jogador)
        {
            var inv = jogador.Inventario;
            sb.Clear().Append("<b>Forno</b>");
            bool temAssando = false, temPronta = false;
            for (int i = 0; i < pizzas.Length; i++)
            {
                var p = pizzas[i];
                sb.Append("   [").Append(i + 1).Append("] ");
                if (p == null) { sb.Append("vazio"); continue; }
                sb.Append(p.DescricaoEstado());
                if (p.Estado == EstadoPizza.Crua) temAssando = true; else temPronta = true;
            }

            if (inv.Pizza != null)
            {
                if (inv.Pizza.Estado == EstadoPizza.Crua) sb.Append("\n[Clique] Colocar a pizza no forno");
            }
            else
            {
                if (temPronta) sb.Append("\n[Clique] Tirar pizza");
                if (temAssando) sb.Append("\n[Botão direito] Tirar antes da hora");
            }
            return sb.ToString();
        }

        public override void Interagir(InteracaoJogador jogador)
        {
            var inv = jogador.Inventario;
            if (inv.Pizza != null)
            {
                Colocar(jogador);
                return;
            }

            // Tira a pizza que está há mais tempo depois do ponto
            int melhor = -1;
            for (int i = 0; i < pizzas.Length; i++)
            {
                var p = pizzas[i];
                if (p == null || p.Estado == EstadoPizza.Crua) continue;
                if (melhor < 0 || p.TempoAssado - p.TempoNecessario > pizzas[melhor].TempoAssado - pizzas[melhor].TempoNecessario)
                    melhor = i;
            }

            if (melhor >= 0) Retirar(melhor, jogador);
            else if (VagaOcupada() >= 0) jogador.Avisar("Ainda está assando... espere o apito!");
            else jogador.Avisar("O forno está vazio.");
        }

        public override void InteragirSecundario(InteracaoJogador jogador)
        {
            if (jogador.Inventario.Pizza != null) return;
            int vaga = VagaOcupada();
            if (vaga >= 0) Retirar(vaga, jogador);
        }

        private void Colocar(InteracaoJogador jogador)
        {
            var inv = jogador.Inventario;
            var pizza = inv.Pizza;
            if (pizza.Estado != EstadoPizza.Crua) { jogador.Avisar("Essa pizza já está assada!"); return; }

            int vaga = -1;
            for (int i = 0; i < pizzas.Length; i++)
                if (pizzas[i] == null) { vaga = i; break; }
            if (vaga < 0) { jogador.Avisar("O forno está cheio!"); return; }

            // O tempo de forno vem da receita que a pizza "parece" ser; se não bater com nenhuma, usa o padrão.
            var receita = pizza.IdentificarReceita(GerenciadorPedidos.Instancia != null ? GerenciadorPedidos.Instancia.Receitas : null);
            if (receita != null) pizza.DefinirTempoForno(receita.tempoForno, receita.toleranciaQueima);
            else pizza.DefinirTempoForno(Pizza.TempoFornoPadrao, Pizza.ToleranciaPadrao);

            inv.SoltarPizza();
            pizzas[vaga] = pizza;
            visuais[vaga] = PizzaVisual.Criar(pizza, vagas[vaga]);
            Sons.Tocar(Sons.Forno, transform.position, 1f, 0.6f);
        }

        private void Retirar(int vaga, InteracaoJogador jogador)
        {
            var p = pizzas[vaga];
            if (!jogador.Inventario.SegurarPizza(p)) return;
            if (p.Estado == EstadoPizza.Crua) jogador.Avisar("Tirou antes da hora: a pizza ainda está crua.");
            pizzas[vaga] = null;
            if (visuais[vaga] != null) Destroy(visuais[vaga].gameObject);
            visuais[vaga] = null;
            Sons.Tocar(Sons.Forno, transform.position, 0.7f, 0.6f);
        }

        private int VagaOcupada()
        {
            for (int i = 0; i < pizzas.Length; i++)
                if (pizzas[i] != null) return i;
            return -1;
        }
    }
}
