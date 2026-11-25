using System;
using Pizzaria.Jogador;
using UnityEngine;

namespace Pizzaria.Interacao
{
    /// <summary>Raycast a partir da câmera: descobre o que está na mira e dispara a interação.</summary>
    [RequireComponent(typeof(ControladorJogador), typeof(InventarioJogador))]
    public class InteracaoJogador : MonoBehaviour
    {
        [SerializeField] private float alcance = 2.6f;
        [SerializeField] private LayerMask camadas = ~0;

        private ControladorJogador controlador;
        private string mensagem;
        private float mensagemAte;

        private InventarioJogador inventario;
        public InventarioJogador Inventario => inventario != null ? inventario : (inventario = GetComponent<InventarioJogador>());
        public Interagivel Alvo { get; private set; }

        /// <summary>Disparado a cada interação (usado pela animação das mãos).</summary>
        public event Action Interagiu;

        public string MensagemAtual => Time.time < mensagemAte ? mensagem : null;

        private void Awake()
        {
            controlador = GetComponent<ControladorJogador>();
        }

        private void OnEnable() => Notificador.Aviso += AoNotificar;
        private void OnDisable() => Notificador.Aviso -= AoNotificar;
        private void AoNotificar(string texto, float duracao) => Avisar(texto, duracao);

        private void Update()
        {
            AtualizarAlvo();
            TrocarSelecao();

            if (Alvo == null || !controlador.CursorTravado || controlador.Bloqueado) return;
            if (controlador.FrameEmQueTravou == Time.frameCount) return;

            if (Entrada.InteragirPressionado)
            {
                Alvo.Interagir(this);
                Interagiu?.Invoke();
            }
            else if (Entrada.InteragirSecundarioPressionado)
            {
                Alvo.InteragirSecundario(this);
                Interagiu?.Invoke();
            }
        }

        private void TrocarSelecao()
        {
            if (!controlador.CursorTravado || controlador.Bloqueado) return;
            int roda = Entrada.RodaMouse;
            if (roda != 0) Inventario.Ciclar(roda);
            int numero = Entrada.NumeroPressionado;
            if (numero >= 0) Inventario.Selecionar(numero);
        }

        private void AtualizarAlvo()
        {
            Interagivel novo = null;
            Transform cam = controlador.Camera;

            if (cam != null && controlador.CursorTravado &&
                Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, alcance, camadas, QueryTriggerInteraction.Ignore))
            {
                novo = hit.collider.GetComponentInParent<Interagivel>();
            }

            if (novo == Alvo) return;
            if (Alvo != null) Alvo.Destacar(false);
            Alvo = novo;
            if (Alvo != null) Alvo.Destacar(true);
        }

        /// <summary>Mostra um aviso curto na tela (ex.: "Mãos cheias!").</summary>
        public void Avisar(string texto, float duracao = 1.8f)
        {
            mensagem = texto;
            mensagemAte = Time.time + duracao;
        }
    }
}
