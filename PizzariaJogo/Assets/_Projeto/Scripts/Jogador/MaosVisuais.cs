using Pizzaria.Cozinha;
using Pizzaria.Interacao;
using UnityEngine;

namespace Pizzaria.Jogador
{
    /// <summary>
    /// Mãos do cozinheiro presas à câmera: balançam ao andar, "esticam" ao interagir
    /// mostram na mão direita o ingrediente selecionado e, à frente, a pizza carregada.
    /// </summary>
    public class MaosVisuais : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private ControladorJogador controlador;
        [SerializeField] private InteracaoJogador interacao;
        [SerializeField] private Transform maoDireita;
        [SerializeField] private Transform maoEsquerda;
        [SerializeField] private Renderer itemNaMao;
        [SerializeField] private Transform pontoPizza;

        [Header("Balanço")]
        [SerializeField] private float amplitudeBalanco = 0.018f;
        [SerializeField] private float frequenciaBalanco = 2.2f;
        [SerializeField] private float distanciaEmpurrao = 0.12f;
        [SerializeField] private float duracaoEmpurrao = 0.22f;

        private Vector3 baseDireita, baseEsquerda;
        private float fase, intensidade, empurrao;
        private MaterialPropertyBlock bloco;
        private PizzaVisual pizzaVisual;
        private GameObject modeloNaMao;
        private Pizzaria.Dados.IngredienteSO ingredienteDoModelo;

        private void Awake()
        {
            bloco = new MaterialPropertyBlock();
            if (maoDireita != null) baseDireita = maoDireita.localPosition;
            if (maoEsquerda != null) baseEsquerda = maoEsquerda.localPosition;
        }

        private void OnEnable()
        {
            if (interacao != null)
            {
                interacao.Interagiu += AoInteragir;
                interacao.Inventario.Mudou += AtualizarItem;
            }
            AtualizarItem();
        }

        private void OnDisable()
        {
            if (interacao != null)
            {
                interacao.Interagiu -= AoInteragir;
                interacao.Inventario.Mudou -= AtualizarItem;
            }
        }

        private void AoInteragir() => empurrao = 1f;

        private void AtualizarItem()
        {
            if (interacao == null) return;
            AtualizarPizza();
            if (itemNaMao == null) return;
            var ing = interacao.Inventario.Selecionado;
            itemNaMao.gameObject.SetActive(ing != null);
            if (modeloNaMao != null) modeloNaMao.SetActive(ing != null);
            if (ing == null || ing == ingredienteDoModelo) return;

            // Modelo próprio do ingrediente (tomate com cabinho, raminho de manjericão...). A esfera fica só como âncora.
            if (modeloNaMao != null) Destroy(modeloNaMao);
            ingredienteDoModelo = ing;
            if (RecursosCozinha.MaterialBase != null)
            {
                itemNaMao.enabled = false;
                modeloNaMao = ModelosIngrediente.NaMao(ing, itemNaMao.transform.parent);
                modeloNaMao.transform.localPosition = itemNaMao.transform.localPosition;
                modeloNaMao.transform.localRotation = Quaternion.Euler(0f, -20f, 0f);
                foreach (var r in modeloNaMao.GetComponentsInChildren<Renderer>()) r.gameObject.layer = itemNaMao.gameObject.layer;
            }
            else
            {
                itemNaMao.GetPropertyBlock(bloco);
                bloco.SetColor(BaseColorId, ing.cor);
                itemNaMao.SetPropertyBlock(bloco);
            }
        }

        private void AtualizarPizza()
        {
            var pizza = interacao.Inventario.Pizza;
            if (pizzaVisual != null && pizzaVisual.Pizza == pizza) return;
            if (pizzaVisual != null) Destroy(pizzaVisual.gameObject);
            pizzaVisual = pizza != null && pontoPizza != null ? PizzaVisual.Criar(pizza, pontoPizza, 1f, false) : null;
        }

        private void LateUpdate()
        {
            float vel = controlador != null ? controlador.VelocidadeHorizontal : 0f;
            bool noChao = controlador == null || controlador.NoChao;

            float alvo = noChao ? Mathf.Clamp01(vel / 4f) : 0f;
            intensidade = Mathf.Lerp(intensidade, alvo, Time.deltaTime * 8f);
            fase += Time.deltaTime * vel * frequenciaBalanco;

            Vector3 balanco = new Vector3(Mathf.Cos(fase) * amplitudeBalanco,
                                          -Mathf.Abs(Mathf.Sin(fase)) * amplitudeBalanco, 0f) * intensidade;

            empurrao = Mathf.MoveTowards(empurrao, 0f, Time.deltaTime / duracaoEmpurrao);
            Vector3 esticar = new Vector3(0f, 0.02f, distanciaEmpurrao) * Mathf.Sin(empurrao * Mathf.PI);

            if (maoDireita != null) maoDireita.localPosition = baseDireita + balanco + esticar;
            if (maoEsquerda != null)
                maoEsquerda.localPosition = baseEsquerda + new Vector3(-balanco.x, balanco.y, 0f);
        }
    }
}
