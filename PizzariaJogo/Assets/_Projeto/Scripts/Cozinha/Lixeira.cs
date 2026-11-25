using Pizzaria.Audio;
using Pizzaria.Interacao;

namespace Pizzaria.Cozinha
{
    /// <summary>Joga fora a pizza das mãos (se tiver) ou os ingredientes.</summary>
    public class Lixeira : Interagivel
    {
        public override string TextoInteracao(InteracaoJogador jogador)
        {
            var inv = jogador.Inventario;
            if (inv.Pizza != null) return "[Clique] Jogar a pizza fora";
            return inv.Vazio ? "Lixeira" : "[Clique] Jogar fora os ingredientes";
        }

        public override void Interagir(InteracaoJogador jogador)
        {
            var inv = jogador.Inventario;
            if (inv.Pizza != null)
            {
                inv.SoltarPizza();
                jogador.Avisar("Pizza no lixo.");
                Sons.Tocar(Sons.Lixo, transform.position, 0.9f);
            }
            else if (!inv.Vazio)
            {
                inv.Esvaziar();
                jogador.Avisar("Mãos vazias.");
                Sons.Tocar(Sons.Lixo, transform.position, 0.7f);
            }
        }
    }
}
