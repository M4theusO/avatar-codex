import { Crown } from 'lucide-react'
import type { RankingEntry } from '../../types/ranking'
import { formatXp } from '../../utils/ranking'
import CharacterAvatar from '../common/CharacterAvatar'
import styles from './Ranking.module.css'

/** Lista ordenada pelo backend; a posição também vem pronta (empatados dividem a posição). */
export default function RankingList({ entries }: { entries: RankingEntry[] }) {
  return (
    <ol className={styles.list} aria-label="Ranking">
      {entries.map((entry) => (
        <li
          key={entry.username}
          className={`${styles.row} ${entry.isCurrentUser ? styles.me : ''}`}
          aria-current={entry.isCurrentUser ? 'true' : undefined}
        >
          <span className={`${styles.position} ${entry.position <= 3 ? styles.podium : ''}`}>
            {entry.position === 1 && <Crown size={14} aria-hidden="true" />}
            {entry.position}º<span className="visually-hidden"> lugar</span>
          </span>
          <CharacterAvatar name={entry.username} image={null} element={entry.element} size="sm" />
          <span className={styles.player}>
            <span className={styles.username}>
              {entry.username}
              {entry.isCurrentUser && <span className={styles.you}>Você</span>}
            </span>
            <span className={styles.meta}>
              Nv. {entry.level}
              {entry.element && ` · ${entry.element.name}`}
            </span>
          </span>
          <span className={styles.xp}>{formatXp(entry.experience)}</span>
        </li>
      ))}
    </ol>
  )
}
