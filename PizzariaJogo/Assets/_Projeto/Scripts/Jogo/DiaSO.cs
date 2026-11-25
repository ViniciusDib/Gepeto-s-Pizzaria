using System.Collections.Generic;
using Pizzaria.Dados;
using UnityEngine;

namespace Pizzaria.Jogo
{
    /// <summary>Configuração de um dia de trabalho (dificuldade, cardápio, duração).</summary>
    [CreateAssetMenu(fileName = "Dia", menuName = "Pizzaria/Dia")]
    public class DiaSO : ScriptableObject
    {
        public string nomeExibicao = "Segunda-feira";
        [TextArea] public string descricao = "";

        [Header("Duração")]
        [Tooltip("Segundos até a pizzaria fechar. Depois disso não chegam novos pedidos.")]
        [Min(30f)] public float duracao = 180f;

        [Header("Pedidos")]
        public List<ReceitaSO> cardapio = new List<ReceitaSO>();
        [Min(0f)] public float primeiroPedidoEm = 4f;
        public Vector2 intervaloEntrePedidos = new Vector2(25f, 35f);
        [Tooltip("Segundos que cada cliente aceita esperar.")]
        [Min(10f)] public float paciencia = 120f;
        [Min(1)] public int maxPedidosAbertos = 2;

        [Header("Derrota")]
        [Tooltip("Quantos clientes podem ir embora bravos antes de perder o fim de semana.")]
        [Min(1)] public int limiteClientesPerdidos = 3;

        [Header("Tutorial")]
        public bool mostrarDicas = false;

        public string Nome => string.IsNullOrEmpty(nomeExibicao) ? name : nomeExibicao;
    }
}
