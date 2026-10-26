import { LogOut } from 'lucide-react'
import { Link } from 'react-router'
import { useAuth } from '../../hooks/useAuth'
import { useLogout } from '../../hooks/useLogout'
import { ButtonLink } from '../common/Button'
import CharacterAvatar from '../common/CharacterAvatar'
import styles from './UserMenu.module.css'

/** Área de sessão da navbar: entrar/criar conta ou usuário logado + sair. */
export default function UserMenu({ onNavigate }: { onNavigate: () => void }) {
  const { status, user } = useAuth()
  const logout = useLogout()

  // Durante a verificação da sessão não mostra nada, para não piscar "Entrar".
  if (status === 'loading') return null

  if (!user) {
    return (
      <div className={styles.menu}>
        <ButtonLink to="/login" variant="ghost" onClick={onNavigate}>
          Entrar
        </ButtonLink>
        <ButtonLink to="/register" variant="primary" onClick={onNavigate}>
          Criar conta
        </ButtonLink>
      </div>
    )
  }

  return (
    <div className={styles.menu}>
      <Link to="/profile" className={styles.profile} onClick={onNavigate}>
        <CharacterAvatar name={user.username} image={null} color={user.element?.color} size="sm" />
        <span className={styles.username}>{user.username}</span>
        <span className="visually-hidden">(meu perfil)</span>
      </Link>
      <button
        type="button"
        className={styles.logout}
        onClick={() => {
          onNavigate()
          void logout()
        }}
        aria-label="Sair"
      >
        <LogOut size={18} aria-hidden="true" />
      </button>
    </div>
  )
}
