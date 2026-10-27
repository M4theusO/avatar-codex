import type { Reward } from '../types/progress'

/**
 * XP do próprio evento (descoberta, quiz…), sem o das conquistas que ele desbloqueou:
 * cada parte é anunciada no seu toast. Só separa o que a API já somou; nada é calculado aqui.
 */
export function eventXp(reward: Reward) {
  return (
    reward.xpGained - reward.achievementsUnlocked.reduce((sum, a) => sum + a.experienceReward, 0)
  )
}
