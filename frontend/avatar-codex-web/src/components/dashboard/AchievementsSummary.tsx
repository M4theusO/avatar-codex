import type { UserAchievement } from '../../types/progress'
import AchievementCard from '../achievements/AchievementCard'
import AchievementIcon from '../achievements/AchievementIcon'
import styles from './Dashboard.module.css'

const dateFormat = new Intl.DateTimeFormat('pt-BR', { day: 'numeric', month: 'short' })

interface AchievementsSummaryProps {
  recent: UserAchievement[]
  next: UserAchievement | null
}

/** Últimas conquistas desbloqueadas e a próxima meta (escolhida pelo backend). */
export default function AchievementsSummary({ recent, next }: AchievementsSummaryProps) {
  return (
    <div className={styles.achievements}>
      {recent.length === 0 ? (
        <p className={styles.note}>Nenhuma conquista desbloqueada ainda.</p>
      ) : (
        <ul className={styles.recentList}>
          {recent.map((achievement) => (
            <li key={achievement.id} className={styles.recentItem}>
              <span className={styles.recentIcon}>
                <AchievementIcon icon={achievement.icon} size={18} />
              </span>
              <span className={styles.recentText}>
                <span className={styles.recentName}>{achievement.name}</span>
                {achievement.unlockedAt && (
                  <span className={styles.recentDate}>
                    {dateFormat.format(new Date(achievement.unlockedAt))}
                  </span>
                )}
              </span>
              <span className={styles.recentXp}>+{achievement.experienceReward} XP</span>
            </li>
          ))}
        </ul>
      )}

      {next ? (
        <>
          <h3 className={styles.subheading}>Próxima conquista</h3>
          <ul className={styles.nextList}>
            <AchievementCard achievement={next} headingLevel="h3" />
          </ul>
        </>
      ) : (
        <p className={styles.note}>Você desbloqueou todas as conquistas. Parabéns!</p>
      )}
    </div>
  )
}
