using System;

namespace Pizzaria.Interacao
{
    /// <summary>
    /// Avisos globais que não vêm de uma interação direta (apito do forno, cliente pagou...).
    /// O <see cref="InteracaoJogador"/> escuta e mostra no HUD.
    /// </summary>
    public static class Notificador
    {
        public static event Action<string, float> Aviso;

        public static void Avisar(string texto, float duracao = 2.5f) => Aviso?.Invoke(texto, duracao);
    }
}
