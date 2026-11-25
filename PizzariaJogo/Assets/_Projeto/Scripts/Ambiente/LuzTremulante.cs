using UnityEngine;

namespace Pizzaria.Ambiente
{
    /// <summary>Faz uma luz tremular como fogo (forno a lenha, velas). Usa ruído Perlin, então não "pisca" seco.</summary>
    [RequireComponent(typeof(Light))]
    public class LuzTremulante : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f), Tooltip("Quanto a intensidade varia (fração da intensidade base).")]
        private float variacao = 0.25f;
        [SerializeField] private float velocidade = 6f;
        [SerializeField, Tooltip("Também desloca um pouco a luz, para as sombras dançarem.")]
        private float deslocamento = 0.02f;

        private Light luz;
        private float intensidadeBase;
        private Vector3 posicaoBase;
        private float semente;

        private void Awake()
        {
            luz = GetComponent<Light>();
            intensidadeBase = luz.intensity;
            posicaoBase = transform.localPosition;
            semente = Random.value * 100f;
        }

        private void Update()
        {
            // Time.time para de correr na pausa (timeScale 0): o fogo "congela" junto com o jogo
            float t = Time.time * velocidade + semente;
            float n = Mathf.PerlinNoise(t, 0.37f) * 0.7f + Mathf.PerlinNoise(t * 2.3f, 5.1f) * 0.3f;
            luz.intensity = intensidadeBase * (1f + (n - 0.5f) * 2f * variacao);
            if (deslocamento > 0f)
                transform.localPosition = posicaoBase + new Vector3(
                    (Mathf.PerlinNoise(t * 0.7f, 9.3f) - 0.5f) * deslocamento,
                    (Mathf.PerlinNoise(t * 0.7f, 2.9f) - 0.5f) * deslocamento, 0f);
        }
    }
}
