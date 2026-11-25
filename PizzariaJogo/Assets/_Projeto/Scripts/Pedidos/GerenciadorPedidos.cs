using System.Collections.Generic;
using Pizzaria.Audio;
using Pizzaria.Cozinha;
using Pizzaria.Dados;
using Pizzaria.Interacao;
using Pizzaria.Jogo;
using UnityEngine;

namespace Pizzaria.Pedidos
{
    public class Pedido
    {
        public int Numero;
        public ReceitaSO Receita;
        public float PacienciaTotal;
        public float PacienciaRestante;
        public float Fracao => PacienciaTotal > 0f ? Mathf.Clamp01(PacienciaRestante / PacienciaTotal) : 0f;
    }

    public struct ResultadoEntrega
    {
        public bool Aceita;
        public string Motivo;
        public int Valor;
        public int Gorjeta;
        public Pedido Pedido;
    }

    /// <summary>Gera comandas, controla a paciência dos clientes e avalia as pizzas entregues.</summary>
    public class GerenciadorPedidos : MonoBehaviour
    {
        public static GerenciadorPedidos Instancia { get; private set; }

        [SerializeField, Tooltip("Cardápio do dia.")] private List<ReceitaSO> receitas = new List<ReceitaSO>();
        [SerializeField] private float primeiroPedidoEm = 3f;
        [SerializeField] private Vector2 intervaloEntrePedidos = new Vector2(22f, 35f);
        [SerializeField, Tooltip("Segundos que o cliente aceita esperar.")] private float paciencia = 120f;
        [SerializeField, Min(1)] private int maxPedidosAbertos = 3;
        [SerializeField, Range(0f, 1f), Tooltip("Gorjeta máxima (fração do preço) com paciência cheia.")]
        private float gorjetaMaxima = 0.5f;

        private readonly List<Pedido> pedidos = new List<Pedido>();
        private int proximoNumero = 1;
        private float proximoPedidoEm;
        private bool configurado;

        /// <summary>false depois que a pizzaria fecha: não chegam novos clientes.</summary>
        public bool ReceberPedidos { get; set; } = true;

        public IReadOnlyList<ReceitaSO> Receitas => receitas;
        public IReadOnlyList<Pedido> Pedidos => pedidos;
        public int Dinheiro { get; private set; }
        public int PizzasEntregues { get; private set; }
        public int PizzasRecusadas { get; private set; }
        public int ClientesPerdidos { get; private set; }

        private void Awake() => Instancia = this;

        private void OnDestroy()
        {
            if (Instancia == this) Instancia = null;
        }

        private void Start()
        {
            if (!configurado) proximoPedidoEm = Time.time + primeiroPedidoEm;
        }

        /// <summary>Aplica a dificuldade e o cardápio do dia.</summary>
        public void Configurar(DiaSO dia)
        {
            if (dia.cardapio != null && dia.cardapio.Count > 0) receitas = new List<ReceitaSO>(dia.cardapio);
            primeiroPedidoEm = dia.primeiroPedidoEm;
            intervaloEntrePedidos = dia.intervaloEntrePedidos;
            paciencia = dia.paciencia;
            maxPedidosAbertos = dia.maxPedidosAbertos;
            proximoPedidoEm = Time.time + primeiroPedidoEm;
            configurado = true;
        }

        private void Update()
        {
            if (ReceberPedidos && Time.time >= proximoPedidoEm)
            {
                if (pedidos.Count < maxPedidosAbertos && receitas.Count > 0)
                {
                    NovoPedido();
                    proximoPedidoEm = Time.time + Random.Range(intervaloEntrePedidos.x, intervaloEntrePedidos.y);
                }
                else proximoPedidoEm = Time.time + 2f; // painel cheio: tenta de novo daqui a pouco
            }

            for (int i = pedidos.Count - 1; i >= 0; i--)
            {
                var p = pedidos[i];
                p.PacienciaRestante -= Time.deltaTime;
                if (p.PacienciaRestante > 0f) continue;

                pedidos.RemoveAt(i);
                ClientesPerdidos++;
                Notificador.Avisar($"O cliente da comanda #{p.Numero} cansou de esperar e foi embora!", 3f);
                Sons.Tocar(Sons.Erro, transform.position, 1f, 0f);
            }
        }

        private void NovoPedido()
        {
            receitas.RemoveAll(r => r == null); // proteção: receita apagada/faltando no cardápio
            if (receitas.Count == 0)
            {
                Debug.LogWarning("[Pizzaria] Cardápio vazio: nenhuma receita válida para gerar pedidos.");
                return;
            }

            var p = new Pedido
            {
                Numero = proximoNumero++,
                Receita = receitas[Random.Range(0, receitas.Count)],
                PacienciaTotal = paciencia,
                PacienciaRestante = paciencia
            };
            pedidos.Add(p);
            Notificador.Avisar($"Nova comanda #{p.Numero}: {p.Receita.Nome}!");
            Sons.Tocar2D(Sons.NovaComanda, 0.7f);
        }

        /// <summary>O cliente avalia a pizza. Se servir para alguma comanda aberta (a mais antiga primeiro), paga.</summary>
        public ResultadoEntrega Entregar(Pizza pizza)
        {
            var r = new ResultadoEntrega();
            if (pizza.Estado == EstadoPizza.Pronta)
            {
                foreach (var p in pedidos)
                {
                    if (!pizza.MontagemCorreta(p.Receita)) continue;
                    r.Aceita = true;
                    r.Pedido = p;
                    r.Valor = p.Receita.preco;
                    r.Gorjeta = Mathf.RoundToInt(p.Receita.preco * gorjetaMaxima * p.Fracao);
                    Dinheiro += r.Valor + r.Gorjeta;
                    PizzasEntregues++;
                    pedidos.Remove(p);
                    return r;
                }
            }

            // Recusada: explica comparando com a comanda mais parecida
            Pedido parecido = null;
            int melhor = -1;
            foreach (var p in pedidos)
            {
                int n = pizza.PassosIniciaisCorretos(p.Receita);
                if (n > melhor) { melhor = n; parecido = p; }
            }
            r.Pedido = parecido;
            r.Motivo = pizza.Diagnosticar(parecido?.Receita) ?? "pizza errada";
            PizzasRecusadas++;
            return r;
        }
    }
}
