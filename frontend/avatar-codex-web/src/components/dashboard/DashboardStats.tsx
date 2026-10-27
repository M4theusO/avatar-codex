import { Link } from 'react-router'
import type { UserStatistics } from '../../types/progress'
import ProgressBar from '../common/ProgressBar'
import Skeleton from '../common/Skeleton'
import styles from './Dashboard.module.css'

/** Resumo da exploração; cada item leva à página da área. O detalhe completo fica no perfil. */
export default function DashboardStats({ statistics }: { statistics: UserStatistics }) {
  const items = [
    {
      to: '/characters',
      label: 'Personagens',
      value: statistics.discoveredCharacters,
      max: statistics.totalCharacters,
      progressLabel: 'Personagens descobertos',
    },
    {
      to: '/nations',
      label: 'Nações',
      value: statistics.discoveredNations,
      max: statistics.totalNations,
      progressLabel: 'Nações descobertas',
    },
    {
      to: '/achievements',
      label: 'Conquistas',
      value: statistics.unlockedAchievements,
      max: statistics.totalAchievements,
      progressLabel: 'Conquistas desbloqueadas',
    },
    {
      to: '/quizzes',
      label: 'Quizzes',
      value: statistics.completedQuizzes,
      max: statistics.totalQuizzes,
      progressLabel: 'Quizzes concluídos',
    },
  ]

  return (
    <ul className={styles.stats} aria-label="Seu progresso">
      {items.map((item) => (
        <li key={item.to}>
          <Link to={item.to} className={styles.stat}>
            <span className={styles.statLabel}>{item.label}</span>
            <span className={styles.statValue}>
              {item.value} <small>/ {item.max}</small>
            </span>
            <ProgressBar value={item.value} max={item.max} label={item.progressLabel} />
          </Link>
        </li>
      ))}
    </ul>
  )
}

export function DashboardStatsSkeleton() {
  return (
    <div className={styles.stats} aria-hidden="true">
      {Array.from({ length: 4 }, (_, index) => (
        <Skeleton key={index} height="6rem" />
      ))}
    </div>
  )
}
