import type { Reward } from '../types/progress'
import { useAchievementToasts } from './useAchievementToasts'
import { useAuth } from './useAuth'
import { useLevelUp } from './useLevelUp'
import { useToast } from './useToast'

/**
 * Mostra o `reward` devolvido pela API (toast do evento, das conquistas e modal de nível) e
 * atualiza XP/nível/barra do usuário em memória.
 * Nada é calculado aqui: os números vêm prontos do backend (regra "não confiar no cliente").
 */
export function useRewardFeedback() {
  const { updateUser } = useAuth()
  const { showToast } = useToast()
  const { showLevelUp } = useLevelUp()
  const announceAchievements = useAchievementToasts()

  return (reward: Reward, message: string) => {
    updateUser({
      experiencePoints: reward.totalXp,
      level: reward.level,
      currentLevelXp: reward.currentLevelXp,
      nextLevelXp: reward.nextLevelXp,
      levelProgress: reward.levelProgress,
    })
    showToast(message, 'reward')
    announceAchievements(reward.achievementsUnlocked)
    if (reward.leveledUp) showLevelUp(reward.level)
  }
}
