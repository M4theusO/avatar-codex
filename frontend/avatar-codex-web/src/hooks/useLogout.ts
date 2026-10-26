import { useNavigate } from 'react-router'
import { useAuth } from './useAuth'
import { useToast } from './useToast'

export function useLogout() {
  const { logout } = useAuth()
  const { showToast } = useToast()
  const navigate = useNavigate()

  return async () => {
    // Sai da página antes, para uma rota protegida não redirecionar ao login no meio do caminho.
    navigate('/', { replace: true })
    await logout()
    showToast('Você saiu da sua conta.', 'info')
  }
}
