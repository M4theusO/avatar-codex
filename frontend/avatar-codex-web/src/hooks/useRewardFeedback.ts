import type { Reward } from '../types/progress'
import { useAuth } from './useAuth'
import { useToast } from './useToast'

/**
 * Mostra o `reward` devolvido pela API e atualiza XP/nível do usuário em memória.
 * Nada é calculado aqui: os números vêm prontos do backend (regra "não confiar no cliente").
 */
export function useRewardFeedback() {
  const { updateUser } = useAuth()
  const { showToast } = useToast()

  return (reward: Reward, message: string) => {
    updateUser({ experiencePoints: reward.totalXp, level: reward.level })
    showToast(message, 'reward')
    if (reward.leveledUp) {
      showToast(`🎉 Novo nível! Você alcançou o nível ${reward.level}.`, 'reward')
    }
  }
}
