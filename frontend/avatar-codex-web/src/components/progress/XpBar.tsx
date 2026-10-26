import { useState, type CSSProperties } from 'react'
import type { User } from '../../types/auth'
import styles from './XpBar.module.css'

const numberFormat = new Intl.NumberFormat('pt-BR')

type XpData = Pick<
  User,
  'level' | 'experiencePoints' | 'currentLevelXp' | 'nextLevelXp' | 'levelProgress' | 'element'
>

interface XpBarProps {
  user: XpData
  /** `compact` na navbar: só a barra fina, sem textos visíveis. */
  variant?: 'full' | 'compact'
}

/**
 * Barra de XP do nível atual. Tudo vem do backend (inclusive a porcentagem); a cor é a do
 * elemento do usuário ou o neutro claro (DA-02). Ganhou XP desde que a barra apareceu? Pulsa.
 */
export default function XpBar({ user, variant = 'full' }: XpBarProps) {
  const { level, experiencePoints, currentLevelXp, nextLevelXp, levelProgress } = user
  const [initialXp] = useState(experiencePoints)
  const gained = experiencePoints !== initialXp

  const description = `Nível ${level}: ${numberFormat.format(experiencePoints)} de ${numberFormat.format(nextLevelXp)} XP`

  return (
    <div
      className={`${styles.xp} ${styles[variant]}`}
      style={user.element ? ({ '--xp-color': user.element.color } as CSSProperties) : undefined}
    >
      {variant === 'full' && (
        <div className={styles.labels}>
          <span className={styles.level}>Nível {level}</span>
          <span className={styles.amount}>
            {numberFormat.format(experiencePoints)} / {numberFormat.format(nextLevelXp)} XP
          </span>
        </div>
      )}
      <div
        className={styles.track}
        role="progressbar"
        aria-label="Experiência"
        aria-valuemin={currentLevelXp}
        aria-valuemax={nextLevelXp}
        aria-valuenow={experiencePoints}
        aria-valuetext={description}
      >
        <div className={styles.fill} style={{ width: `${levelProgress}%` }} />
        {/* key muda a cada ganho: a animação roda de novo */}
        {gained && <span key={experiencePoints} className={styles.pulse} aria-hidden="true" />}
      </div>
    </div>
  )
}
