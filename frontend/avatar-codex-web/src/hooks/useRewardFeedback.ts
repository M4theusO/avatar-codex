import type { Reward } from '../types/progress'
import { useAuth } from './useAuth'
import { useLevelUp } from './useLevelUp'
import { useToast } from './useToast'

/**
 * Mostra o `reward` devolvido pela API e atualiza XP/nível/barra do usuário em memória.
 * Nada é calculado aqui: os números vêm prontos do backend (regra "não confiar no cliente").
 */
export function useRewardFeedback() {
  const { updateUser } = useAuth()
  const { showToast } = useToast()
  const { showLevelUp } = useLevelUp()

  return (reward: Reward, message: string) => {
    updateUser({
      experiencePoints: reward.totalXp,
      level: reward.level,
      currentLevelXp: reward.currentLevelXp,
      nextLevelXp: reward.nextLevelXp,
      levelProgress: reward.levelProgress,
    })
    showToast(message, 'reward')
    if (reward.leveledUp) showLevelUp(reward.level)
  }
}
