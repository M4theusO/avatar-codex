import { Link } from 'react-router'
import { useStatistics } from '../../hooks/useUserData'
import ProgressBar from '../common/ProgressBar'
import Skeleton from '../common/Skeleton'
import { ErrorState } from '../common/StatusMessage'
import styles from './Profile.module.css'

export default function ExplorationStats() {
  const { data, isPending, isError, refetch } = useStatistics()

  if (isPending) {
    return (
      <div className={styles.stats} aria-busy="true">
        {Array.from({ length: 3 }, (_, index) => (
          <Skeleton key={index} height="6.5rem" />
        ))}
      </div>
    )
  }
  if (isError) {
    return (
      <ErrorState title="Não foi possível carregar suas estatísticas" onRetry={() => refetch()} />
    )
  }

  return (
    <ul className={styles.stats}>
      <li className={styles.stat}>
        <span className={styles.statLabel}>Personagens descobertos</span>
        <span className={styles.statValue}>
          {data.discoveredCharacters} <small>/ {data.totalCharacters}</small>
        </span>
        <ProgressBar
          value={data.discoveredCharacters}
          max={data.totalCharacters}
          label="Personagens descobertos"
        />
      </li>
      <li className={styles.stat}>
        <span className={styles.statLabel}>Nações descobertas</span>
        <span className={styles.statValue}>
          {data.discoveredNations} <small>/ {data.totalNations}</small>
        </span>
        <ProgressBar
          value={data.discoveredNations}
          max={data.totalNations}
          label="Nações descobertas"
        />
      </li>
      <li className={styles.stat}>
        <span className={styles.statLabel}>Favoritos</span>
        <span className={styles.statValue}>{data.favorites}</span>
        <Link to="/favorites" className={styles.statLink}>
          Ver favoritos
        </Link>
      </li>
    </ul>
  )
}
