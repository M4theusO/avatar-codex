import type { ElementSummary } from '../../types/content'
import type { RankingPeriod, RankingQuery } from '../../types/ranking'
import { elementStyle } from '../../utils/labels'
import ElementIcon from '../common/ElementIcon'
import styles from './Ranking.module.css'

interface RankingFiltersProps {
  query: RankingQuery
  elements: ElementSummary[]
  onChange: (query: RankingQuery) => void
}

const periods: { value: RankingPeriod; label: string }[] = [
  { value: 'all', label: 'Geral' },
  { value: 'month', label: 'Este mês' },
]

const audiences: { following: boolean; label: string }[] = [
  { following: false, label: 'Todo mundo' },
  { following: true, label: 'Seguindo' },
]

/** Período, quem aparece (todos ou só quem você segue) e elemento como botões alternáveis (aria-pressed); trocar o filtro volta para a página 1. */
export default function RankingFilters({ query, elements, onChange }: RankingFiltersProps) {
  return (
    <div className={styles.filters}>
      <div className={styles.segments}>
        <div className={styles.segmented} role="group" aria-label="Período">
          {periods.map(({ value, label }) => (
            <button
              key={value}
              type="button"
              className={styles.segment}
              aria-pressed={query.period === value}
              onClick={() => onChange({ ...query, period: value, page: 1 })}
            >
              {label}
            </button>
          ))}
        </div>
        <div className={styles.segmented} role="group" aria-label="Quem aparece">
          {audiences.map(({ following, label }) => (
            <button
              key={label}
              type="button"
              className={styles.segment}
              aria-pressed={Boolean(query.following) === following}
              onClick={() => onChange({ ...query, following: following || undefined, page: 1 })}
            >
              {label}
            </button>
          ))}
        </div>
      </div>

      <div className={styles.chips} role="group" aria-label="Elemento">
        <button
          type="button"
          className={styles.chip}
          aria-pressed={query.elementId === undefined}
          onClick={() => onChange({ ...query, elementId: undefined, page: 1 })}
        >
          Todos
        </button>
        {elements.map((element) => (
          <button
            key={element.id}
            type="button"
            className={styles.chip}
            style={elementStyle(element.color)}
            aria-pressed={query.elementId === element.id}
            onClick={() => onChange({ ...query, elementId: element.id, page: 1 })}
          >
            <ElementIcon icon={element.icon} size={16} className={styles.chipIcon} />
            {element.name}
          </button>
        ))}
      </div>
    </div>
  )
}
