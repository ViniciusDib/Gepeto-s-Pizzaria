using UnityEngine;

namespace Pizzaria.Jogador
{
    /// <summary>Movimento em primeira pessoa com CharacterController + olhar com o mouse.</summary>
    [RequireComponent(typeof(CharacterController))]
    public class ControladorJogador : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] private Transform cameraJogador;

        [Header("Movimento")]
        [SerializeField] private float velocidade = 3.2f;
        [SerializeField] private float velocidadeCorrida = 5f;
        [SerializeField] private float aceleracao = 12f;
        [SerializeField] private float gravidade = -20f;

        [Header("Olhar")]
        [SerializeField, Tooltip("Sensibilidade base; o jogador ajusta um multiplicador no menu de pausa.")]
        private float sensibilidade = 0.12f;
        [SerializeField] private float limitePitch = 85f;

        [Header("Segurança")]
        [SerializeField, Tooltip("Se cair abaixo dessa altura (bug de colisão), volta ao ponto inicial.")]
        private float alturaMinima = -5f;

        private const string ChaveSensibilidade = "Pizzaria_Sensibilidade";
        private static float multiplicadorSensibilidade = -1f;

        /// <summary>Multiplicador da sensibilidade do mouse (0.2 a 3), salvo entre sessões.</summary>
        public static float MultiplicadorSensibilidade
        {
            get
            {
                if (multiplicadorSensibilidade < 0f) multiplicadorSensibilidade = PlayerPrefs.GetFloat(ChaveSensibilidade, 1f);
                return multiplicadorSensibilidade;
            }
            set
            {
                multiplicadorSensibilidade = Mathf.Clamp(value, 0.2f, 3f);
                PlayerPrefs.SetFloat(ChaveSensibilidade, multiplicadorSensibilidade);
            }
        }

        private Vector3 posicaoInicial;
        private Quaternion rotacaoInicial;

        private CharacterController controller;
        private Vector3 velocidadePlana;
        private float velocidadeVertical;
        private float pitch;
        private float distanciaPasso;

        /// <summary>Quando true, o jogador não anda nem olha (menus, cutscenes).</summary>
        public bool Bloqueado { get; set; }

        public bool CursorTravado => Cursor.lockState == CursorLockMode.Locked;

        /// <summary>Frame em que o cursor foi travado por clique — usado para não interagir com esse mesmo clique.</summary>
        public int FrameEmQueTravou { get; private set; } = -1;

        public float VelocidadeHorizontal => velocidadePlana.magnitude;
        public bool NoChao => controller != null && controller.isGrounded;
        public Transform Camera => cameraJogador;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (cameraJogador == null && UnityEngine.Camera.main != null)
                cameraJogador = UnityEngine.Camera.main.transform;
        }

        private void Start()
        {
            posicaoInicial = transform.position;
            rotacaoInicial = transform.rotation;
            // Sem gerenciador de jogo na cena (cena de teste): trava direto como antes
            if (Pizzaria.Jogo.GerenciadorJogo.Instancia == null) TravarCursor(true);
        }

        private void Update()
        {
            AtualizarCursor();

            bool podeControlar = CursorTravado && !Bloqueado;
            if (podeControlar) Olhar();
            Mover(podeControlar ? Entrada.Movimento() : Vector2.zero);

            if (transform.position.y < alturaMinima) Reposicionar();
        }

        /// <summary>Volta ao ponto inicial (rede de segurança contra cair do cenário).</summary>
        public void Reposicionar()
        {
            controller.enabled = false;
            transform.SetPositionAndRotation(posicaoInicial, rotacaoInicial);
            controller.enabled = true;
            velocidadePlana = Vector3.zero;
            velocidadeVertical = 0f;
        }

        private void AtualizarCursor()
        {
            // Com GerenciadorJogo, o Esc é a pausa (tratada lá)
            if (Pizzaria.Jogo.GerenciadorJogo.Instancia == null && Entrada.SoltarCursorPressionado)
            {
                TravarCursor(false);
            }
            else if (!CursorTravado && !Bloqueado && Entrada.CliqueEsquerdo)
            {
                TravarCursor(true);
                FrameEmQueTravou = Time.frameCount;
            }
        }

        public void TravarCursor(bool travar)
        {
            Cursor.lockState = travar ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !travar;
            if (travar) FrameEmQueTravou = Time.frameCount;
        }

        private void Olhar()
        {
            Vector2 delta = Entrada.OlharDelta() * (sensibilidade * MultiplicadorSensibilidade);
            transform.Rotate(0f, delta.x, 0f, Space.Self);

            pitch = Mathf.Clamp(pitch - delta.y, -limitePitch, limitePitch);
            if (cameraJogador != null)
                cameraJogador.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void Mover(Vector2 input)
        {
            float alvoVel = Entrada.Correndo ? velocidadeCorrida : velocidade;
            Vector3 desejado = (transform.right * input.x + transform.forward * input.y) * alvoVel;
            velocidadePlana = Vector3.MoveTowards(velocidadePlana, desejado, aceleracao * Time.deltaTime);

            if (controller.isGrounded && velocidadeVertical < 0f)
                velocidadeVertical = -2f; // mantém colado no chão
            velocidadeVertical += gravidade * Time.deltaTime;

            Vector3 deslocamento = velocidadePlana + Vector3.up * velocidadeVertical;
            controller.Move(deslocamento * Time.deltaTime);
            TocarPassos();
        }

        /// <summary>Um passo a cada ~0.75 m andados no chão (mais espaçados e fortes correndo).</summary>
        private void TocarPassos()
        {
            float v = velocidadePlana.magnitude;
            if (!controller.isGrounded || v < 0.5f) { distanciaPasso = Mathf.Min(distanciaPasso, 0.5f); return; }
            distanciaPasso += v * Time.deltaTime;
            float passada = v > velocidade + 0.5f ? 0.95f : 0.75f;
            if (distanciaPasso < passada) return;
            distanciaPasso = 0f;
            Pizzaria.Audio.Sons.Tocar(Pizzaria.Audio.Sons.Passo, transform.position, v > velocidade + 0.5f ? 0.55f : 0.4f, 0f);
        }
    }
}
