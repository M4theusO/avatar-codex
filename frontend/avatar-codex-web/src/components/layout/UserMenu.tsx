import { LogOut, Star, Target, Trophy } from 'lucide-react'
import { Link, NavLink } from 'react-router'
import { useAuth } from '../../hooks/useAuth'
import { useLogout } from '../../hooks/useLogout'
import { ButtonLink } from '../common/Button'
import CharacterAvatar from '../common/CharacterAvatar'
import XpBar from '../progress/XpBar'
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
        <CharacterAvatar name={user.username} image={null} element={user.element} size="sm" />
        <span className={styles.identity}>
          <span className={styles.username}>
            {user.username}
            <span className="visually-hidden"> (meu perfil)</span>
          </span>
          <span className={styles.levelRow}>
            <span className={styles.level}>Nv. {user.level}</span>
            <XpBar user={user} variant="compact" />
          </span>
        </span>
      </Link>
      <NavLink
        to="/favorites"
        className={({ isActive }) => `${styles.iconButton} ${isActive ? styles.iconActive : ''}`}
        onClick={onNavigate}
        aria-label="Favoritos"
      >
        <Star size={18} aria-hidden="true" />
      </NavLink>
      <NavLink
        to="/challenges"
        className={({ isActive }) => `${styles.iconButton} ${isActive ? styles.iconActive : ''}`}
        onClick={onNavigate}
        aria-label="Desafios diários"
      >
        <Target size={18} aria-hidden="true" />
      </NavLink>
      <NavLink
        to="/achievements"
        className={({ isActive }) => `${styles.iconButton} ${isActive ? styles.iconActive : ''}`}
        onClick={onNavigate}
        aria-label="Conquistas"
      >
        <Trophy size={18} aria-hidden="true" />
      </NavLink>
      <button
        type="button"
        className={styles.iconButton}
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
