import type { CharacterListItem } from '../../types/content'
import Skeleton from '../common/Skeleton'
import CharacterCard from './CharacterCard'
import cardStyles from './CharacterCard.module.css'
import styles from './CharacterGrid.module.css'

interface CharacterGridProps {
  characters: CharacterListItem[]
  /** Ajusta as colunas para um conjunto fixo (ex.: os 5 Avatares numa linha). */
  className?: string
}

export function CharacterGrid({ characters, className }: CharacterGridProps) {
  return (
    <ul className={className ? `${styles.grid} ${className}` : styles.grid}>
      {characters.map((character) => (
        <li key={character.id}>
          <CharacterCard character={character} />
        </li>
      ))}
    </ul>
  )
}

export function CharacterGridSkeleton({
  count = 8,
  className,
}: {
  count?: number
  className?: string
}) {
  return (
    <div className={className ? `${styles.grid} ${className}` : styles.grid} aria-busy="true">
      <span className="visually-hidden" role="status">
        Carregando personagens…
      </span>
      {Array.from({ length: count }, (_, index) => (
        <div key={index} className={cardStyles.card} aria-hidden="true">
          <Skeleton width="4.5rem" height="4.5rem" round />
          <Skeleton width="60%" height="1.25rem" />
          <Skeleton width="40%" height="0.875rem" />
          <Skeleton width="90%" height="2.5rem" />
        </div>
      ))}
    </div>
  )
}
