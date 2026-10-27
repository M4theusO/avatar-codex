import type { User } from '../../types/auth'
import { elementStyle } from '../../utils/labels'
import XpBar from '../progress/XpBar'
import styles from './Dashboard.module.css'

/** Saudação e nível. Usa o usuário da sessão: aparece na hora, sem esperar o dashboard carregar. */
export default function DashboardHero({ user }: { user: User }) {
  return (
    <header className={styles.hero} style={elementStyle(user.element?.color)}>
      <div>
        <p className={styles.eyebrow}>Sua jornada</p>
        <h1 className={styles.greeting}>Olá, {user.username}</h1>
        <p className={styles.subtitle}>Continue de onde parou.</p>
      </div>
      <div className={styles.heroXp}>
        <XpBar user={user} />
      </div>
    </header>
  )
}
