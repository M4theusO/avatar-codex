import { CircleCheck, Lock } from 'lucide-react'
import type { UserAchievement } from '../../types/progress'
import ProgressBar from '../common/ProgressBar'
import AchievementIcon from './AchievementIcon'
import styles from './AchievementCard.module.css'

const dateFormat = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short' })

interface AchievementCardProps {
  achievement: UserAchievement
  /** h3 quando o card fica dentro de uma seção com título próprio (ex.: dashboard). */
  headingLevel?: 'h2' | 'h3'
}

export default function AchievementCard({
  achievement,
  headingLevel: Heading = 'h2',
}: AchievementCardProps) {
  const { name, description, icon, experienceReward, isUnlocked, unlockedAt, progress, target } =
    achievement

  return (
    <li className={`${styles.card} ${isUnlocked ? styles.unlocked : styles.locked}`}>
      <span className={styles.badge}>
        <AchievementIcon icon={icon} size={26} />
        {!isUnlocked && (
          <span className={styles.lock}>
            <Lock size={12} aria-hidden="true" />
          </span>
        )}
      </span>

      <div className={styles.body}>
        <div className={styles.heading}>
          <Heading className={styles.name}>{name}</Heading>
          <span className={styles.xp}>+{experienceReward} XP</span>
        </div>
        <p className={styles.description}>{description}</p>

        {isUnlocked ? (
          <p className={styles.status}>
            <CircleCheck size={16} aria-hidden="true" />
            {unlockedAt
              ? `Desbloqueada em ${dateFormat.format(new Date(unlockedAt))}`
              : 'Desbloqueada'}
          </p>
        ) : (
          <div className={styles.progress}>
            <span className={styles.progressText}>
              <span className="visually-hidden">Bloqueada. Progresso: </span>
              {progress} / {target}
            </span>
            <ProgressBar value={progress} max={target} label={`Progresso de ${name}`} />
          </div>
        )}
      </div>
    </li>
  )
}
