import { useEffect, useEffectEvent, useRef } from 'react'
import { discoverCharacter, discoverNation } from '../services/userService'
import type { Reward } from '../types/progress'
import { useAuth } from './useAuth'
import { useRewardFeedback } from './useRewardFeedback'
import { useInvalidateUserContent } from './useUserData'

const discover = { character: discoverCharacter, nation: discoverNation }

/**
 * Registra a descoberta (RN-12) só para usuário logado e só quando `name` existe, isto é,
 * depois que o conteúdo principal foi carregado e exibido (nunca em loading, erro ou 404).
 * Envia uma única vez por página, mesmo com o efeito duplo do StrictMode; a garantia contra
 * XP duplicado é do backend.
 */
export function useDiscovery(kind: 'character' | 'nation', id: number, name: string | undefined) {
  const { status } = useAuth()
  const celebrate = useRewardFeedback()
  const invalidate = useInvalidateUserContent()
  const sentFor = useRef<string | null>(null)

  const onDiscovered = useEffectEvent((reward: Reward, discoveredName: string) => {
    celebrate(
      reward,
      `✨ Nova descoberta — Você descobriu ${discoveredName}! +${reward.xpGained} XP`,
    )
    invalidate()
  })

  useEffect(() => {
    const key = `${kind}:${id}`
    if (status !== 'authenticated' || !name || sentFor.current === key) return
    sentFor.current = key

    discover[kind](id)
      .then(({ isNew, reward }) => {
        if (isNew && reward) onDiscovered(reward, name)
      })
      .catch(() => {
        // A descoberta é um bônus: se falhar, a página continua normal e tenta na próxima visita.
        sentFor.current = null
      })
  }, [status, kind, id, name])
}
