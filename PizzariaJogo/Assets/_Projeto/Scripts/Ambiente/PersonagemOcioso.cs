using UnityEngine;

namespace Pizzaria.Ambiente
{
    /// <summary>
    /// Dá vida aos personagens: respiração (o tronco sobe e desce), cabeça olhando em volta,
    /// piscar de olhos e, de vez em quando, um gesto com o braço (levar o copo/garfo à boca).
    /// </summary>
    public class PersonagemOcioso : MonoBehaviour
    {
        [SerializeField] private Transform tronco;
        [SerializeField] private Transform cabeca;
        [SerializeField, Tooltip("Pálpebras/olhos que achatam ao piscar.")] private Transform[] olhos;
        [SerializeField, Tooltip("Ombro que faz o gesto (opcional).")] private Transform bracoGesto;
        [SerializeField] private float amplitudeRespiracao = 0.012f;
        [SerializeField] private float anguloMaximoCabeca = 45f;

        private Vector3 troncoBase;
        private Quaternion cabecaBase, bracoBase;
        private Vector3[] olhosBase;
        private float fase, proximaOlhada, anguloAlvo, anguloAtual, inclinacaoAlvo, inclinacaoAtual;
        private float proximaPiscada, fimPiscada, proximoGesto, inicioGesto = -10f;

        private const float DuracaoGesto = 2.2f;

        private void Awake()
        {
            if (tronco != null) troncoBase = tronco.localPosition;
            if (cabeca != null) cabecaBase = cabeca.localRotation;
            if (bracoGesto != null) bracoBase = bracoGesto.localRotation;
            if (olhos != null)
            {
                olhosBase = new Vector3[olhos.Length];
                for (int i = 0; i < olhos.Length; i++) if (olhos[i] != null) olhosBase[i] = olhos[i].localScale;
            }
            fase = Random.value * 10f;
            proximaOlhada = Random.Range(1f, 4f);
            proximaPiscada = Random.Range(0.5f, 4f);
            proximoGesto = Random.Range(3f, 12f);
        }

        private void Update()
        {
            float agora = Time.time, t = agora + fase;
            if (tronco != null)
                tronco.localPosition = troncoBase + Vector3.up * (Mathf.Sin(t * 1.6f) * amplitudeRespiracao);

            Cabeca(agora);
            Piscar(agora);
            Gesto(agora);
        }

        private void Cabeca(float agora)
        {
            if (cabeca == null) return;
            if (agora >= proximaOlhada)
            {
                // às vezes olha para os lados, às vezes volta a olhar para a frente (para a mesa/acompanhante)
                anguloAlvo = Random.value < 0.4f ? 0f : Random.Range(-anguloMaximoCabeca, anguloMaximoCabeca);
                inclinacaoAlvo = Random.Range(-8f, 12f);
                proximaOlhada = agora + Random.Range(1.5f, 5f);
            }
            float k = 1f - Mathf.Exp(-3f * Time.deltaTime);
            anguloAtual = Mathf.Lerp(anguloAtual, anguloAlvo, k);
            inclinacaoAtual = Mathf.Lerp(inclinacaoAtual, inclinacaoAlvo, k);
            cabeca.localRotation = cabecaBase * Quaternion.Euler(inclinacaoAtual, anguloAtual, 0f);
        }

        private void Piscar(float agora)
        {
            if (olhos == null || olhosBase == null) return;
            if (agora >= proximaPiscada)
            {
                fimPiscada = agora + 0.12f;
                proximaPiscada = agora + Random.Range(2f, 5.5f);
            }
            bool fechado = agora < fimPiscada;
            for (int i = 0; i < olhos.Length; i++)
            {
                if (olhos[i] == null) continue;
                var e = olhosBase[i];
                olhos[i].localScale = fechado ? new Vector3(e.x, e.y * 0.12f, e.z) : e;
            }
        }

        private void Gesto(float agora)
        {
            if (bracoGesto == null) return;
            if (agora >= proximoGesto)
            {
                inicioGesto = agora;
                proximoGesto = agora + Random.Range(6f, 15f);
            }
            float p = (agora - inicioGesto) / DuracaoGesto;
            float peso = p >= 0f && p <= 1f ? Mathf.Sin(p * Mathf.PI) : 0f; // sobe, segura, desce
            bracoGesto.localRotation = bracoBase * Quaternion.Euler(-70f * Mathf.SmoothStep(0f, 1f, peso), 0f, 0f);
        }
    }
}
