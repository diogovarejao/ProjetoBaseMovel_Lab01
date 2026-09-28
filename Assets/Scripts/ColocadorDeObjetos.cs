using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace RAPH.S1
{
    /// <summary>
    /// Pousa um objeto virtual num plano detetado, no sítio onde o
    /// utilizador tocou.
    ///
    /// São três passos, e é o ciclo que se repete em quase toda a RA:
    ///
    ///     1. ler o toque        — onde é que o utilizador apontou
    ///     2. lançar um raio     — o que existe no mundo naquela direção
    ///     3. instanciar         — pôr lá o objeto, na pose devolvida
    ///
    /// Lê o código todo antes de mexer. Depois volta ao guião: há três
    /// modificações pedidas, e é aí que se aprende.
    /// </summary>
    [RequireComponent(typeof(ARRaycastManager))]
    public class ColocadorDeObjetos : MonoBehaviour
    {
        [Tooltip("O que aparece quando tocas num plano. Um cubo serve.")]
        [SerializeField] private GameObject prefabParaColocar;

        [Tooltip("Se ficar desligado, cada toque coloca mais um objeto.")]
        [SerializeField] private bool apenasUmObjeto = false;

        private ARRaycastManager _raycastManager;
        private GameObject _objetoColocado;

        // Declarada uma vez, como campo, e reutilizada em cada toque.
        //
        // O método Raycast() não devolve uma lista: escreve numa lista que
        // nós fornecemos. Se criássemos uma lista nova a cada toque,
        // estaríamos a gerar lixo para o recoletor limpar — e numa
        // aplicação a 60 ou 90 fotogramas por segundo isso nota-se.
        private static readonly List<ARRaycastHit> Resultados = new();

        private void Awake()
        {
            _raycastManager = GetComponent<ARRaycastManager>();
        }

        private void Update()
        {
            // ── 1. Houve toque? ──────────────────────────────────
            if (!EntradaDoUtilizador.ComecouToque(out Vector2 posicaoEcra))
                return;

            // ── 2. O raio bateu nalgum plano? ────────────────────
            //
            // `PlaneWithinPolygon` diz: só me interessa se bater dentro
            // do polígono que o plano tem de facto. Sem esse filtro,
            // acertarias no plano matemático infinito que o atravessa a
            // sala toda, e o objeto apareceria a flutuar no ar.
            bool acertou = _raycastManager.Raycast(
                posicaoEcra,
                Resultados,
                TrackableType.PlaneWithinPolygon);

            if (!acertou)
                return;

            // O primeiro resultado é o mais próximo da câmara.
            Pose pose = Resultados[0].pose;

            // Rotação virada para a câmara (alinhada na vertical para o cubo não ficar deitado)
            Vector2 direcaoNoPlano = new Vector2(
                Camera.main.transform.forward.x,
                Camera.main.transform.forward.z).normalized;

            Quaternion rotacaoViradaParaCamera = direcaoNoPlano != Vector2.zero
                ? Quaternion.LookRotation(new Vector3(direcaoNoPlano.x, 0, direcaoNoPlano.y))
                : pose.rotation;

            // ── 3. Pousar o objeto ───────────────────────────────
            if (apenasUmObjeto)
            {
                if (_objetoColocado == null)
                {
                    _objetoColocado = Instantiate(
                        prefabParaColocar, pose.position, rotacaoViradaParaCamera);
                }
                else
                {
                    // Já existe: move-se em vez de criar outro.
                    _objetoColocado.transform.SetPositionAndRotation(
                        pose.position, rotacaoViradaParaCamera);
                }
            }
            else
            {
                // Instancia novos objetos sem substituir os anteriores
                Instantiate(
                    prefabParaColocar, pose.position, rotacaoViradaParaCamera);
            }
        }
    }
}