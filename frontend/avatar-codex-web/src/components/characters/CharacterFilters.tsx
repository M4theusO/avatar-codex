import { useElements, useNations, useSeries } from '../../hooks/useContent'
import type { CharacterQuery, CharacterStatus, CharacterType } from '../../types/content'
import { statusLabels, typeLabels } from '../../utils/labels'
import { Button } from '../common/Button'
import { Select, type SelectOption } from '../common/Field'
import styles from './CharacterFilters.module.css'

interface CharacterFiltersProps {
  id: string
  query: CharacterQuery
  onChange: (changes: Partial<CharacterQuery>) => void
  onClear?: () => void
}

function toOptions(items: { id: number; name: string }[] | undefined): SelectOption[] {
  return (items ?? []).map((item) => ({ value: String(item.id), label: item.name }))
}

function toNumber(value: string) {
  return value ? Number(value) : undefined
}

const typeOptions = Object.entries(typeLabels).map(([value, label]) => ({ value, label }))
const statusOptions = Object.entries(statusLabels).map(([value, label]) => ({ value, label }))

export default function CharacterFilters({ id, query, onChange, onClear }: CharacterFiltersProps) {
  // As opções vêm da API; se alguma lista falhar, o filtro fica só com "Todas".
  const nations = useNations()
  const elements = useElements()
  const series = useSeries()

  return (
    <div id={id} className={styles.filters} role="group" aria-label="Filtros">
      <Select
        label="Nação"
        emptyLabel="Todas"
        value={query.nationId?.toString() ?? ''}
        options={toOptions(nations.data)}
        onChange={(value) => onChange({ nationId: toNumber(value) })}
      />
      <Select
        label="Elemento"
        emptyLabel="Todos"
        value={query.elementId?.toString() ?? ''}
        options={toOptions(elements.data)}
        onChange={(value) => onChange({ elementId: toNumber(value) })}
      />
      <Select
        label="Tipo"
        emptyLabel="Todos"
        value={query.type ?? ''}
        options={typeOptions}
        onChange={(value) => onChange({ type: (value || undefined) as CharacterType | undefined })}
      />
      <Select
        label="Status"
        emptyLabel="Todos"
        value={query.status ?? ''}
        options={statusOptions}
        onChange={(value) =>
          onChange({ status: (value || undefined) as CharacterStatus | undefined })
        }
      />
      <Select
        label="Série"
        emptyLabel="Todas"
        value={query.seriesId?.toString() ?? ''}
        options={toOptions(series.data)}
        onChange={(value) => onChange({ seriesId: toNumber(value) })}
      />
      {onClear && (
        <Button variant="ghost" onClick={onClear}>
          Limpar filtros
        </Button>
      )}
    </div>
  )
}
