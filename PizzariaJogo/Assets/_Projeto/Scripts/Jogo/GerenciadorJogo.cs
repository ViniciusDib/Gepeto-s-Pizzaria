using System;
using Pizzaria.Audio;
using Pizzaria.Interacao;
using Pizzaria.Jogador;
using Pizzaria.Pedidos;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pizzaria.Jogo
{
    public enum EstadoJogo { Menu, Jogando, Pausado, FimDoDia, Derrota, Vitoria }

    /// <summary>
    /// Máquina de estados da partida: menu → dia (jogando/pausado) → fim do dia, derrota ou vitória.
    /// Reiniciar sempre recarrega a cena, garantindo um estado limpo (sem pizzas/pedidos "presos").
    /// </summary>
    public class GerenciadorJogo : MonoBehaviour
    {
        public static GerenciadorJogo Instancia { get; private set; }

        [SerializeField] private DiaSO[] dias;
        [SerializeField] private GerenciadorPedidos pedidos;
        [SerializeField] private ControladorJogador jogador;

        // Progresso que sobrevive ao recarregar a cena
        private static int diaAtual;
        private static int dinheiroAcumulado;
        private static bool comecarDireto;
        private static string idPartida;

        private bool cursorJaTravou;

        public EstadoJogo Estado { get; private set; } = EstadoJogo.Menu;
        public DiaSO Dia => dias != null && dias.Length > 0 ? dias[Mathf.Clamp(diaAtual, 0, dias.Length - 1)] : null;
        public int IndiceDia => diaAtual;
        public int TotalDias => dias != null ? dias.Length : 0;
        public bool UltimoDia => diaAtual >= TotalDias - 1;
        public float TempoRestante { get; private set; }
        public bool Fechado { get; private set; }
        public int DinheiroAcumulado => dinheiroAcumulado;
        public int DinheiroTotal => dinheiroAcumulado + (pedidos != null ? pedidos.Dinheiro : 0);
        public GerenciadorPedidos Pedidos => pedidos;

        /// <summary>Identifica a partida atual (segunda → domingo) para o placar de recordes.</summary>
        public string IdPartida => idPartida ??= Guid.NewGuid().ToString("N");
        /// <summary>Dias concluídos nesta partida (o dia atual conta se foi vencido).</summary>
        public int DiasCompletos => diaAtual + (Estado == EstadoJogo.Vitoria || Estado == EstadoJogo.FimDoDia ? 1 : 0);

        public event Action<EstadoJogo> EstadoMudou;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetarEstatico()
        {
            diaAtual = 0;
            dinheiroAcumulado = 0;
            comecarDireto = false;
            idPartida = null;
        }

        private void Awake()
        {
            Instancia = this;
            if (pedidos == null) pedidos = FindAnyObjectByType<GerenciadorPedidos>();
            if (jogador == null) jogador = FindAnyObjectByType<ControladorJogador>();
        }

        private void OnDestroy()
        {
            if (Instancia == this) Instancia = null;
            Time.timeScale = 1f;
        }

        private void Start()
        {
            if (comecarDireto)
            {
                comecarDireto = false;
                ComecarDia();
            }
            else MudarEstado(EstadoJogo.Menu);
        }

        private void Update()
        {
            if (Estado == EstadoJogo.Pausado && Entrada.PausarPressionado)
            {
                Continuar();
                return;
            }
            if (Estado != EstadoJogo.Jogando) return;

            // Esc pausa. No navegador o Esc solta o mouse antes do jogo ver a tecla, então
            // perder o cursor travado também pausa (nunca fica "jogando sem controle").
            if (jogador != null && jogador.CursorTravado) cursorJaTravou = true;
            if (Entrada.PausarPressionado || (cursorJaTravou && jogador != null && !jogador.CursorTravado))
            {
                Pausar();
                return;
            }

            if (!Fechado)
            {
                TempoRestante -= Time.deltaTime;
                if (TempoRestante <= 0f)
                {
                    TempoRestante = 0f;
                    Fechado = true;
                    pedidos.ReceberPedidos = false;
                    Notificador.Avisar("A pizzaria fechou! Termine as comandas que ainda estão abertas.", 4f);
                }
            }

            var dia = Dia;
            if (dia != null && pedidos.ClientesPerdidos >= dia.limiteClientesPerdidos)
            {
                MudarEstado(EstadoJogo.Derrota);
                Musica.Silenciar(2.5f);
                Sons.Tocar2D(Sons.Derrota);
                return;
            }

            if (Fechado && pedidos.Pedidos.Count == 0)
            {
                MudarEstado(UltimoDia ? EstadoJogo.Vitoria : EstadoJogo.FimDoDia);
                Musica.Silenciar(UltimoDia ? 3f : 2f);
                Sons.Tocar2D(UltimoDia ? Sons.Vitoria : Sons.FimDoDia);
            }
        }

        private void OnApplicationFocus(bool foco)
        {
            if (!foco && Estado == EstadoJogo.Jogando) Pausar();
        }

        // ------------------------------------------------------------------ Ações (botões das telas)

        public void ComecarDia()
        {
            var dia = Dia;
            TempoRestante = dia != null ? dia.duracao : 180f;
            Fechado = false;
            if (dia != null) pedidos.Configurar(dia);
            pedidos.ReceberPedidos = true;
            MudarEstado(EstadoJogo.Jogando);
            Notificador.Avisar($"{(dia != null ? dia.Nome : "Dia")}: a pizzaria abriu!", 3f);
        }

        public void Pausar()
        {
            if (Estado == EstadoJogo.Jogando) MudarEstado(EstadoJogo.Pausado);
        }

        public void Continuar()
        {
            if (Estado == EstadoJogo.Pausado) MudarEstado(EstadoJogo.Jogando);
        }

        public void ReiniciarDia() => Recarregar(true);

        public void ProximoDia()
        {
            dinheiroAcumulado += pedidos.Dinheiro;
            diaAtual = Mathf.Min(diaAtual + 1, TotalDias - 1);
            Recarregar(true);
        }

        /// <summary>Recomeça o fim de semana do zero (segunda-feira).</summary>
        public void NovoJogo()
        {
            diaAtual = 0;
            dinheiroAcumulado = 0;
            idPartida = null;
            Recarregar(true);
        }

        public void IrParaMenu()
        {
            diaAtual = 0;
            dinheiroAcumulado = 0;
            idPartida = null;
            Recarregar(false);
        }

        public void Sair()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public static bool PodeSair => Application.platform != RuntimePlatform.WebGLPlayer;

        private void Recarregar(bool comecar)
        {
            comecarDireto = comecar;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void MudarEstado(EstadoJogo novo)
        {
            Estado = novo;
            cursorJaTravou = false;
            bool jogando = novo == EstadoJogo.Jogando;
            Time.timeScale = jogando ? 1f : 0f;
            AudioListener.pause = novo == EstadoJogo.Pausado;
            if (jogador != null)
            {
                jogador.Bloqueado = !jogando;
                jogador.TravarCursor(jogando);
            }
            EstadoMudou?.Invoke(novo);
        }
    }
}
