import type { CharacterSummary, Era, TimelineFilters as Filters } from '../../types/content'
import { Button } from '../common/Button'
import { Select } from '../common/Field'
import styles from './Timeline.module.css'

interface TimelineFiltersProps {
  filters: Filters
  eras: Era[]
  characters: CharacterSummary[]
  onChange: (changes: Filters) => void
}

function toNumber(value: string) {
  return value ? Number(value) : undefined
}

export default function TimelineFilters({
  filters,
  eras,
  characters,
  onChange,
}: TimelineFiltersProps) {
  const hasFilters = filters.eraId !== undefined || filters.characterId !== undefined

  return (
    <div className={styles.filters} role="group" aria-label="Filtros da linha do tempo">
      <Select
        label="Era"
        emptyLabel="Todas"
        value={filters.eraId?.toString() ?? ''}
        options={eras.map((era) => ({ value: String(era.id), label: era.name }))}
        onChange={(value) => onChange({ ...filters, eraId: toNumber(value) })}
      />
      <Select
        label="Personagem"
        emptyLabel="Todos"
        value={filters.characterId?.toString() ?? ''}
        options={characters.map((c) => ({ value: String(c.id), label: c.name }))}
        onChange={(value) => onChange({ ...filters, characterId: toNumber(value) })}
      />
      {hasFilters && (
        <Button variant="ghost" onClick={() => onChange({})}>
          Limpar filtros
        </Button>
      )}
    </div>
  )
}
