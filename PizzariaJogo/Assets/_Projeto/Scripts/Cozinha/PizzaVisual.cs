using System.Collections.Generic;
using Pizzaria.Dados;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pizzaria.Cozinha
{
    /// <summary>
    /// Desenha uma <see cref="Pizza"/> com primitivas (greybox): disco de massa, camadas
    /// de molho/queijo e pedacinhos de cobertura. Escurece conforme assa/queima.
    /// </summary>
    public class PizzaVisual : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly Color CorTostada = new Color(0.78f, 0.50f, 0.22f);
        private static readonly Color CorQueimada = new Color(0.10f, 0.08f, 0.06f);

        private const float RaioMassa = 0.18f;
        private const float EspessuraMassa = 0.025f;
        private const float EspessuraCamada = 0.006f;

        private struct Parte
        {
            public Renderer renderer;
            public Color cor;
            public bool massa;
        }

        private readonly List<Parte> partes = new List<Parte>();
        private readonly List<ParteColorida> temporario = new List<ParteColorida>();
        private MaterialPropertyBlock bloco;
        private Pizza pizza;
        private bool sombras = true;
        private float ultimoTom = -1f;

        public Pizza Pizza => pizza;

        public static PizzaVisual Criar(Pizza pizza, Transform pai, float escala = 1f, bool sombras = true)
        {
            var go = new GameObject("Pizza");
            go.transform.SetParent(pai, false);
            go.transform.localScale = Vector3.one * escala;
            var v = go.AddComponent<PizzaVisual>();
            v.sombras = sombras;
            v.Vincular(pizza);
            return v;
        }

        private void Vincular(Pizza p)
        {
            pizza = p;
            pizza.Mudou += Reconstruir;
            Reconstruir();
        }

        private void OnDestroy()
        {
            if (pizza != null) pizza.Mudou -= Reconstruir;
        }

        private void Reconstruir()
        {
            foreach (var p in partes)
                if (p.renderer != null) Destroy(p.renderer.gameObject);
            partes.Clear();
            ultimoTom = -1f;
            if (pizza == null) return;

            var camadas = new Dictionary<IngredienteSO, (Transform t, int n)>();
            float altura = 0f;
            int pedacos = 0;

            foreach (var ing in pizza.Camadas)
            {
                switch (ing.forma)
                {
                    case FormaIngrediente.Base:
                        Disco(ing.cor, RaioMassa, altura, EspessuraMassa, true);
                        ModelosIngrediente.Borda(transform, RaioMassa * 0.96f, altura + EspessuraMassa * 0.7f, ing.cor, sombras, temporario);
                        Registrar(true);
                        altura += EspessuraMassa;
                        break;

                    case FormaIngrediente.Camada:
                        if (camadas.TryGetValue(ing, out var c))
                        {
                            // mais cliques = espalha mais
                            c.n++;
                            float r = Mathf.Min(RaioMassa * 0.93f, RaioMassa * 0.62f + c.n * 0.02f);
                            c.t.localScale = new Vector3(r * 2f, c.t.localScale.y, r * 2f);
                            camadas[ing] = c;
                        }
                        else
                        {
                            var d = Disco(ing.cor, RaioMassa * 0.64f, altura, EspessuraCamada, false);
                            camadas[ing] = (d.transform, 1);
                            altura += EspessuraCamada;
                            if (ing.visual == VisualIngrediente.Queijo)
                            {
                                ModelosIngrediente.BolhasQueijo(transform, RaioMassa * 0.6f, altura, ing.cor, sombras, temporario);
                                Registrar(false);
                            }
                        }
                        break;

                    default: // Cobertura: 3 pedacinhos por clique
                        for (int k = 0; k < 3; k++, pedacos++)
                        {
                            // espiral dourada: espalha bem sem aleatoriedade
                            float ang = pedacos * 2.39996f;
                            float dist = RaioMassa * 0.75f * Mathf.Sqrt((pedacos % 18 + 0.5f) / 18f);
                            var pos = new Vector3(Mathf.Cos(ang) * dist, altura + 0.004f, Mathf.Sin(ang) * dist);
                            ModelosIngrediente.Cobertura(ing.visual, ing.cor, transform, pos, pedacos * 137.5f, sombras, temporario);
                            Registrar(false);
                        }
                        break;
                }
            }
            AtualizarTom();
        }

        /// <summary>Passa as peças recém-criadas por ModelosIngrediente para a lista que escurece no forno.</summary>
        private void Registrar(bool massa)
        {
            foreach (var t in temporario)
                partes.Add(new Parte { renderer = t.renderer, cor = t.cor, massa = massa });
            temporario.Clear();
        }

        private Renderer Disco(Color cor, float raio, float base_, float espessura, bool massa)
        {
            // Cilindro padrão: 2 m de altura, 1 m de diâmetro
            var pos = new Vector3(0f, base_ + espessura * 0.5f, 0f);
            return Peca(PrimitiveType.Cylinder, cor, pos, new Vector3(raio * 2f, espessura * 0.5f, raio * 2f), massa);
        }

        private Renderer Peca(PrimitiveType tipo, Color cor, Vector3 pos, Vector3 escala, bool massa)
        {
            var go = GameObject.CreatePrimitive(tipo);
            go.name = massa ? "Massa" : "Ingrediente";
            Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(transform, false);
            go.transform.localPosition = pos;
            go.transform.localScale = escala;

            var r = go.GetComponent<Renderer>();
            var mat = RecursosCozinha.MaterialBase;
            if (mat != null) r.sharedMaterial = mat;
            r.shadowCastingMode = sombras ? ShadowCastingMode.On : ShadowCastingMode.Off;

            partes.Add(new Parte { renderer = r, cor = cor, massa = massa });
            return r;
        }

        private void Update() => AtualizarTom();

        private void AtualizarTom()
        {
            if (pizza == null) return;
            float assado = pizza.ProgressoForno, queima = pizza.ProgressoQueima;
            float tom = assado + queima;
            if (Mathf.Abs(tom - ultimoTom) < 0.01f) return;
            ultimoTom = tom;

            if (bloco == null) bloco = new MaterialPropertyBlock();
            foreach (var p in partes)
            {
                if (p.renderer == null) continue;
                Color c = p.massa ? Color.Lerp(p.cor, CorTostada, assado * 0.7f) : Color.Lerp(p.cor, p.cor * 0.85f, assado);
                c = Color.Lerp(c, CorQueimada, queima * (p.massa ? 1f : 0.85f));
                c.a = 1f;
                p.renderer.GetPropertyBlock(bloco);
                bloco.SetColor(BaseColorId, c);
                p.renderer.SetPropertyBlock(bloco);
            }
        }
    }
}
