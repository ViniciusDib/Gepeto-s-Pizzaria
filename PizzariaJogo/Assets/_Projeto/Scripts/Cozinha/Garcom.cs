using Pizzaria.Audio;
using Pizzaria.Interacao;
using Pizzaria.Pedidos;
using UnityEngine;

namespace Pizzaria.Cozinha
{
    /// <summary>
    /// NPC simples: busca pizzas no balcão, leva até o salão (onde o cliente avalia)
    /// e, se a pizza for recusada, traz de volta para o balcão.
    /// </summary>
    public class Garcom : MonoBehaviour
    {
        private enum Estado { Esperando, IndoBuscar, Levando, Devolvendo, Voltando }

        [SerializeField] private Balcao balcao;
        [SerializeField] private Transform pontoEspera;
        [SerializeField] private Transform pontoSalao;
        [SerializeField] private Transform bandeja;
        [SerializeField] private float velocidade = 2.4f;
        [SerializeField, Tooltip("Onde ele para em relação à vaga do balcão.")]
        private Vector3 offsetAtendimento = new Vector3(0f, 0f, -0.6f);
        [SerializeField] private float tempoComCliente = 1f;
        [SerializeField, Tooltip("Parte visual que balança ao andar (opcional).")] private Transform corpo;

        private Estado estado;
        private int vagaAlvo = -1;
        private Pizza carregando;
        private PizzaVisual visual;
        private float esperarAte;
        private bool andouNesteQuadro;
        private float faseAndar;

        private void Update()
        {
            switch (estado)
            {
                case Estado.Esperando:
                    OlharPara(balcao.transform.position);
                    if (balcao.ProcurarPizzaParaLevar(out vagaAlvo)) estado = Estado.IndoBuscar;
                    break;

                case Estado.IndoBuscar:
                    if (!balcao.TemPizzaParaLevar(vagaAlvo)) { estado = Estado.Voltando; break; } // jogador pegou de volta
                    if (!Andar(PontoAtendimento(vagaAlvo))) break;
                    Segurar(balcao.Retirar(vagaAlvo));
                    estado = Estado.Levando;
                    break;

                case Estado.Levando:
                    if (!Andar(pontoSalao.position)) break;
                    if (esperarAte <= 0f) { esperarAte = Time.time + tempoComCliente; break; }
                    if (Time.time < esperarAte) break;
                    esperarAte = 0f;
                    EntregarAoCliente();
                    break;

                case Estado.Devolvendo:
                    if (vagaAlvo < 0) vagaAlvo = balcao.VagaLivre();
                    if (vagaAlvo < 0) { Andar(PontoAtendimento(0)); break; } // balcão cheio: espera
                    if (!Andar(PontoAtendimento(vagaAlvo))) break;
                    if (balcao.Devolver(carregando, vagaAlvo))
                    {
                        Soltar();
                        estado = Estado.Voltando;
                    }
                    else vagaAlvo = -1;
                    break;

                case Estado.Voltando:
                    if (Andar(pontoEspera.position)) estado = Estado.Esperando;
                    break;
            }
        }

        private void EntregarAoCliente()
        {
            var g = GerenciadorPedidos.Instancia;
            if (g == null) { Soltar(); estado = Estado.Voltando; return; }

            var r = g.Entregar(carregando);
            if (r.Aceita)
            {
                string gorjeta = r.Gorjeta > 0 ? $" + R$ {r.Gorjeta} de gorjeta" : "";
                Notificador.Avisar($"Comanda #{r.Pedido.Numero} entregue! R$ {r.Valor}{gorjeta}", 3f);
                Sons.Tocar(Sons.Dinheiro, transform.position, 1f, 0f);
                Soltar();
                estado = Estado.Voltando;
            }
            else
            {
                Notificador.Avisar($"Cliente recusou: {r.Motivo}. O garçom vai devolver a pizza.", 3.5f);
                Sons.Tocar(Sons.Erro, transform.position, 1f, 0f);
                vagaAlvo = balcao.VagaLivre();
                estado = Estado.Devolvendo;
            }
        }

        private void LateUpdate()
        {
            if (corpo == null) return;
            // passinhos: sobe e desce enquanto anda, volta suave ao parar
            if (andouNesteQuadro) faseAndar += Time.deltaTime * velocidade * 4.5f;
            float alvo = andouNesteQuadro ? Mathf.Abs(Mathf.Sin(faseAndar)) * 0.05f : 0f;
            var p = corpo.localPosition;
            p.y = Mathf.Lerp(p.y, alvo, 1f - Mathf.Exp(-20f * Time.deltaTime));
            corpo.localPosition = p;
            corpo.localRotation = Quaternion.Euler(0f, 0f, andouNesteQuadro ? Mathf.Sin(faseAndar) * 3f : 0f);
            andouNesteQuadro = false;
        }

        private Vector3 PontoAtendimento(int vaga)
        {
            var p = balcao.PosicaoVaga(vaga) + offsetAtendimento;
            p.y = transform.position.y;
            return p;
        }

        /// <summary>Anda até o destino; retorna true quando chegou.</summary>
        private bool Andar(Vector3 destino)
        {
            destino.y = transform.position.y;
            Vector3 dir = destino - transform.position;
            if (dir.sqrMagnitude < 0.0025f) return true;

            OlharPara(destino);
            andouNesteQuadro = true;
            transform.position = Vector3.MoveTowards(transform.position, destino, velocidade * Time.deltaTime);
            return false;
        }

        private void OlharPara(Vector3 alvo)
        {
            Vector3 dir = alvo - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
        }

        private void Segurar(Pizza p)
        {
            carregando = p;
            visual = PizzaVisual.Criar(p, bandeja != null ? bandeja : transform);
        }

        private void Soltar()
        {
            carregando = null;
            if (visual != null) Destroy(visual.gameObject);
            visual = null;
        }
    }
}
