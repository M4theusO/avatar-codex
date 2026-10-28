import type { Reward } from '../types/progress'

/**
 * XP do próprio evento (descoberta, quiz…), sem o das conquistas e dos desafios do dia que ele
 * concluiu: cada parte é anunciada no seu toast. Só separa o que a API já somou; nada é calculado aqui.
 */
export function eventXp(reward: Reward) {
  const achievements = reward.achievementsUnlocked.reduce((sum, a) => sum + a.experienceReward, 0)
  const challenges = reward.challengesCompleted.reduce((sum, c) => sum + c.experienceReward, 0)
  return reward.xpGained - achievements - challenges
}
