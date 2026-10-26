import { Navigate, Outlet, useLocation, useSearchParams } from 'react-router'
import { useAuth } from '../../hooks/useAuth'
import { loginPath, safeRedirect } from '../../utils/redirect'
import Skeleton from '../common/Skeleton'

function SessionLoading() {
  return (
    <div aria-busy="true">
      <span className="visually-hidden" role="status">
        Verificando sua sessão…
      </span>
      <Skeleton height="16rem" />
    </div>
  )
}

/** Rotas 🔒: sem sessão, vai para o login e volta para cá depois. */
export function RequireAuth() {
  const { status } = useAuth()
  const location = useLocation()

  if (status === 'loading') return <SessionLoading />
  if (status === 'anonymous') {
    return <Navigate to={loginPath(location.pathname + location.search)} replace />
  }
  return <Outlet />
}

/** Login e cadastro: quem já está logado segue para o destino. */
export function GuestOnly() {
  const { status } = useAuth()
  const [params] = useSearchParams()

  if (status === 'loading') return <SessionLoading />
  if (status === 'authenticated') {
    return <Navigate to={safeRedirect(params.get('redirect'))} replace />
  }
  return <Outlet />
}
