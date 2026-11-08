import { LogOut, Medal, Star, Target, Trophy, type LucideIcon } from 'lucide-react'
import { Link, NavLink } from 'react-router'
import { useAuth } from '../../hooks/useAuth'
import { useLogout } from '../../hooks/useLogout'
import { ButtonLink } from '../common/Button'
import { userAvatarImage } from '../../utils/userAvatar'
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
        <CharacterAvatar
          name={user.username}
          image={userAvatarImage(user.avatarUrl)}
          element={user.element}
          size="sm"
        />
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
      <IconLink to="/favorites" label="Favoritos" icon={Star} onNavigate={onNavigate} />
      <IconLink to="/challenges" label="Desafios diários" icon={Target} onNavigate={onNavigate} />
      <IconLink to="/achievements" label="Conquistas" icon={Trophy} onNavigate={onNavigate} />
      <IconLink to="/ranking" label="Ranking" icon={Medal} onNavigate={onNavigate} />
      <button
        type="button"
        className={`${styles.iconButton} ${styles.alignEnd}`}
        onClick={() => {
          onNavigate()
          void logout()
        }}
        aria-label="Sair"
      >
        <LogOut size={18} aria-hidden="true" />
        <Tooltip label="Sair" />
      </button>
    </div>
  )
}

/**
 * Dica visual com o nome do atalho, no hover e no foco pelo teclado. Para leitores de tela o nome
 * já vem do aria-label, então a dica fica escondida deles (aria-hidden).
 */
function Tooltip({ label }: { label: string }) {
  return (
    <span className={styles.tooltip} aria-hidden="true">
      {label}
    </span>
  )
}

interface IconLinkProps {
  to: string
  label: string
  icon: LucideIcon
  onNavigate: () => void
}

function IconLink({ to, label, icon: Icon, onNavigate }: IconLinkProps) {
  return (
    <NavLink
      to={to}
      className={({ isActive }) => `${styles.iconButton} ${isActive ? styles.iconActive : ''}`}
      onClick={onNavigate}
      aria-label={label}
    >
      <Icon size={18} aria-hidden="true" />
      <Tooltip label={label} />
    </NavLink>
  )
}
