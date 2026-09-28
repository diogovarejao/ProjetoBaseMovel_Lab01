using UnityEngine;
using UnityEngine.InputSystem;

namespace RAPH.S1
{
    /// <summary>
    /// Diz onde é que o utilizador "tocou" no ecrã, venha esse toque de um
    /// dedo ou de um rato.
    ///
    /// Porque é que isto existe como classe à parte:
    ///
    /// No simulador não há ecrã tátil — quem gera o evento é o rato. Num
    /// telemóvel é o dedo. Se espalhares essa diferença pelo código todo,
    /// acabas com dois caminhos em cada script e esqueces-te de um deles.
    /// Aqui a diferença vive num sítio só, e o resto do projeto pergunta
    /// sempre a mesma coisa: "houve toque? onde?".
    ///
    /// É o mesmo princípio que vais aplicar o semestre inteiro: isolar o
    /// que depende da plataforma para o resto do código não ter de saber.
    /// </summary>
    public static class EntradaDoUtilizador
    {
        /// <summary>
        /// True apenas no fotograma em que o toque ou o clique começou.
        ///
        /// Repara no "ThisFrame": sem isso terias o evento repetido em
        /// todos os fotogramas enquanto o dedo estivesse pousado, e em vez
        /// de um cubo apareciam sessenta por segundo.
        /// </summary>
        public static bool ComecouToque(out Vector2 posicaoEcra)
        {
            // Primeiro o ecrã tátil. `Touchscreen.current` é null quando
            // não há ecrã tátil — no editor, por exemplo — e por isso o `?.`
            var toque = Touchscreen.current?.primaryTouch;
            if (toque != null && toque.press.wasPressedThisFrame)
            {
                posicaoEcra = toque.position.ReadValue();
                return true;
            }

            // Depois o rato, que é o que responde dentro do simulador.
            var rato = Mouse.current;
            if (rato != null && rato.leftButton.wasPressedThisFrame)
            {
                posicaoEcra = rato.position.ReadValue();
                return true;
            }

            posicaoEcra = default;
            return false;
        }
    }
}
