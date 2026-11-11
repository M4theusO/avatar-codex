import type { ReactNode } from 'react'
import type { ElementSummary } from '../../types/content'
import { elementStyle } from '../../utils/labels'
import { userAvatarImage } from '../../utils/userAvatar'
import CharacterAvatar from '../common/CharacterAvatar'
import XpBar, { type XpData } from '../progress/XpBar'
import styles from './ProfileHero.module.css'

interface ProfileHeroProps {
  profile: XpData & { username: string; avatarUrl: string | null; element: ElementSummary | null }
  /** Linha abaixo do nome (o email no próprio perfil, um aviso no perfil público). */
  subtitle?: ReactNode
  /** Pares <div><dt/><dd/></div> da lista de dados (elemento, membro desde...). */
  details: ReactNode
  /** Botão no canto (Sair, Editar perfil). */
  action?: ReactNode
}

/** Cabeçalho do perfil, o próprio e o público: foto, nome, barra de XP e dados, no tom do elemento. */
export default function ProfileHero({ profile, subtitle, details, action }: ProfileHeroProps) {
  return (
    <header className={styles.hero} style={elementStyle(profile.element?.color)}>
      <CharacterAvatar
        name={profile.username}
        image={userAvatarImage(profile.avatarUrl)}
        element={profile.element}
        size="lg"
      />
      <div className={styles.info}>
        <h1 className={styles.name}>{profile.username}</h1>
        {subtitle && <p className={styles.subtitle}>{subtitle}</p>}
        <div className={styles.xp}>
          <XpBar user={profile} />
        </div>
        <dl className={styles.stats}>{details}</dl>
      </div>
      {action && <div className={styles.action}>{action}</div>}
    </header>
  )
}
