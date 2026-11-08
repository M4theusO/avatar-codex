import { useNavigate } from 'react-router'
import { deleteAccount } from '../services/authService'
import { useAuth } from './useAuth'
import { useToast } from './useToast'

/**
 * Exclui a conta e encerra a sessão. Erros (senha errada, conta demo) sobem para quem chamou,
 * que continua na tela; só depois do sucesso a página muda e a sessão local termina.
 */
export function useDeleteAccount() {
  const { logout } = useAuth()
  const { showToast } = useToast()
  const navigate = useNavigate()

  return async (password: string) => {
    await deleteAccount(password)
    // Mesmo caminho do "Sair": sai da rota protegida antes de a sessão acabar.
    navigate('/', { replace: true })
    await logout()
    showToast('Sua conta foi excluída.', 'info')
  }
}
