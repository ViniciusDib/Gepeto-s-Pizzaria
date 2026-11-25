using Pizzaria.Interacao;
using UnityEngine;

namespace Pizzaria.Cozinha
{
    /// <summary>Marcador temporário para estações ainda não implementadas (bancada, forno, balcão...).</summary>
    public class PontoPlaceholder : Interagivel
    {
        [SerializeField] private string nome = "Estação";
        [SerializeField] private string descricao = "Em breve";

        public void Configurar(string nomeEstacao, string desc)
        {
            nome = nomeEstacao;
            descricao = desc;
        }

        public override string TextoInteracao(InteracaoJogador jogador) => $"{nome}\n<i>{descricao}</i>";

        public override void Interagir(InteracaoJogador jogador) =>
            jogador.Avisar($"{nome}: ainda não implementado.");
    }
}
