using Pizzaria.Audio;
using Pizzaria.Dados;
using Pizzaria.Interacao;
using UnityEngine;

namespace Pizzaria.Cozinha
{
    /// <summary>Caixa/pote de um ingrediente. Clique pega, clique direito devolve.</summary>
    public class EstacaoIngrediente : Interagivel
    {
        [SerializeField] private IngredienteSO ingrediente;
        [SerializeField, Min(1)] private int quantidadePorClique = 1;

        public IngredienteSO Ingrediente => ingrediente;

        public override string TextoInteracao(InteracaoJogador jogador)
        {
            if (ingrediente == null) return "(sem ingrediente)";
            int q = jogador.Inventario.Quantidade(ingrediente);
            string texto = $"[Clique] Pegar {ingrediente.Nome}  ({q}/{ingrediente.maxNaMao})";
            if (q > 0) texto += "\n[Botão direito] Devolver";
            return texto;
        }

        public override void Interagir(InteracaoJogador jogador)
        {
            if (ingrediente == null) return;
            if (jogador.Inventario.Adicionar(ingrediente, quantidadePorClique) == 0)
            {
                jogador.Avisar($"Não cabe mais {ingrediente.Nome} nas mãos!");
                Sons.Tocar(Sons.Erro, transform.position, 0.35f, 0f);
            }
            else Sons.Tocar(Sons.Pegar, transform.position, 0.8f, 0f);
        }

        public override void InteragirSecundario(InteracaoJogador jogador)
        {
            if (ingrediente != null && jogador.Inventario.Remover(ingrediente, 1))
                Sons.Tocar(Sons.Colocar, transform.position, 0.5f, 0f);
        }
    }
}
