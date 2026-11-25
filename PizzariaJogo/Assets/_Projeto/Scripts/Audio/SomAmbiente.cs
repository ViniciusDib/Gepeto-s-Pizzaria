using UnityEngine;

namespace Pizzaria.Audio
{
    /// <summary>Som 3D em loop preso a um objeto da cena (fogo do forno, burburinho do salão).</summary>
    public class SomAmbiente : MonoBehaviour
    {
        public enum Tipo { Fogo, Salao }

        [SerializeField] private Tipo tipo = Tipo.Fogo;
        [SerializeField, Range(0f, 1f)] private float volume = 0.5f;
        [SerializeField] private float distanciaMin = 1.5f;
        [SerializeField] private float distanciaMax = 14f;

        private AudioSource fonte;

        public void Configurar(Tipo t, float vol, float min, float max)
        {
            tipo = t; volume = vol; distanciaMin = min; distanciaMax = max;
        }

        private void Start()
        {
            fonte = gameObject.AddComponent<AudioSource>();
            fonte.clip = tipo == Tipo.Fogo ? Sons.Fogo : Sons.Salao;
            fonte.loop = true;
            fonte.spatialBlend = 1f;
            fonte.rolloffMode = AudioRolloffMode.Linear;
            fonte.minDistance = distanciaMin;
            fonte.maxDistance = distanciaMax;
            fonte.dopplerLevel = 0f;
            fonte.volume = volume * Sons.Volume;
            fonte.time = Random.Range(0f, fonte.clip.length); // dois loops iguais não começam juntos
            fonte.Play();
        }

        private void Update()
        {
            if (fonte != null) fonte.volume = volume * Sons.Volume;
        }
    }
}
