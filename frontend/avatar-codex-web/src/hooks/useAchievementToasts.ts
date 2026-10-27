import type { AchievementReward } from '../types/progress'
import { useToast } from './useToast'

/** Um toast por conquista desbloqueada (RF-ACH-02), com os dados do `reward` da API. */
export function useAchievementToasts() {
  const { showToast } = useToast()

  return (achievements: AchievementReward[]) => {
    for (const achievement of achievements) {
      showToast(
        `Conquista desbloqueada — ${achievement.name}! +${achievement.experienceReward} XP`,
        'achievement',
      )
    }
  }
}
