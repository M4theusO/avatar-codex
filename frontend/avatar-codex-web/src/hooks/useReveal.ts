import { useEffect, useRef, useState } from 'react'

/**
 * Marca o elemento como visível na primeira vez que ele entra na tela (animação de entrada ao rolar).
 * Sem IntersectionObserver, já começa visível. Quem pede menos movimento não vê a transição: a regra
 * global de `prefers-reduced-motion` zera a duração.
 */
export function useReveal<T extends Element>() {
  const ref = useRef<T>(null)
  const [visible, setVisible] = useState(() => typeof IntersectionObserver === 'undefined')

  useEffect(() => {
    const element = ref.current
    if (visible || !element) return

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) {
          setVisible(true)
          observer.disconnect()
        }
      },
      { rootMargin: '0px 0px -10% 0px' },
    )
    observer.observe(element)
    return () => observer.disconnect()
  }, [visible])

  return { ref, visible }
}
