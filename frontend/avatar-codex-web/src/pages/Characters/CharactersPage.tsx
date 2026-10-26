import { SearchX, SlidersHorizontal } from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import CharacterFilters from '../../components/characters/CharacterFilters'
import { CharacterGrid, CharacterGridSkeleton } from '../../components/characters/CharacterGrid'
import { Button } from '../../components/common/Button'
import { SearchInput, Select } from '../../components/common/Field'
import PageHeader from '../../components/common/PageHeader'
import Pagination from '../../components/common/Pagination'
import { EmptyState, ErrorState } from '../../components/common/StatusMessage'
import { useCharacterQueryParams } from '../../hooks/useCharacterQueryParams'
import { useCharacters } from '../../hooks/useContent'
import type { CharacterSort, SortOrder } from '../../types/content'
import { filterKeys, hasActiveFilters } from '../../utils/characterQuery'
import styles from './CharactersPage.module.css'

const SEARCH_DEBOUNCE_MS = 300

const sortOptions = [
  { value: 'name-asc', label: 'Nome (A–Z)' },
  { value: 'name-desc', label: 'Nome (Z–A)' },
  { value: 'nation-asc', label: 'Nação' },
  { value: 'element-asc', label: 'Elemento' },
]

export default function CharactersPage() {
  const { query, update, clear } = useCharacterQueryParams()
  const characters = useCharacters(query)
  const [filtersOpen, setFiltersOpen] = useState(false)

  // A busca digitada vai para a URL com debounce; se a URL mudar por fora
  // (voltar do navegador, limpar filtros), o campo acompanha.
  const [searchText, setSearchText] = useState(query.search ?? '')
  const [urlSearch, setUrlSearch] = useState(query.search)
  if (query.search !== urlSearch) {
    setUrlSearch(query.search)
    if ((query.search ?? '') !== searchText.trim()) setSearchText(query.search ?? '')
  }

  const searchTimer = useRef<ReturnType<typeof setTimeout>>(undefined)
  useEffect(() => () => clearTimeout(searchTimer.current), [])

  function handleSearchChange(value: string) {
    setSearchText(value)
    clearTimeout(searchTimer.current)
    searchTimer.current = setTimeout(
      () => update({ search: value.trim() || undefined }, { replace: true }),
      SEARCH_DEBOUNCE_MS,
    )
  }

  function handleClear() {
    clearTimeout(searchTimer.current)
    setSearchText('')
    clear()
  }

  function handlePageChange(page: number) {
    update({ page })
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  function handleSortChange(value: string) {
    const [sort, order] = value.split('-') as [CharacterSort, SortOrder]
    update({ sort, order })
  }

  const activeFilterCount = filterKeys.filter((key) => key !== 'search' && query[key]).length
  const filtersActive = hasActiveFilters(query)

  return (
    <>
      <title>Personagens · Avatar Codex</title>
      <PageHeader
        title="Personagens"
        description="Dobradores, Avatares e não dobradores das duas séries. Busque pelo nome ou combine filtros."
      />

      <div className={styles.toolbar}>
        <div className={styles.search}>
          <SearchInput
            label="Buscar personagem"
            placeholder="Buscar por nome ou apelido…"
            value={searchText}
            onChange={handleSearchChange}
          />
        </div>
        <div className={styles.sort}>
          <Select
            label="Ordenar por"
            value={`${query.sort ?? 'name'}-${query.order ?? 'asc'}`}
            options={sortOptions}
            onChange={handleSortChange}
          />
        </div>
        <Button
          className={styles.filtersToggle}
          onClick={() => setFiltersOpen((open) => !open)}
          aria-expanded={filtersOpen}
          aria-controls="character-filters"
        >
          <SlidersHorizontal size={18} aria-hidden="true" />
          Filtros{activeFilterCount > 0 && ` (${activeFilterCount})`}
        </Button>
      </div>

      <div className={styles.layout}>
        <aside className={`${styles.sidebar} ${filtersOpen ? styles.sidebarOpen : ''}`}>
          <CharacterFilters
            id="character-filters"
            query={query}
            onChange={update}
            onClear={filtersActive ? handleClear : undefined}
          />
        </aside>

        <section className={styles.results} aria-labelledby="results-title">
          <h2 id="results-title" className="visually-hidden">
            Resultados
          </h2>
          <Results
            state={characters}
            filtersActive={filtersActive}
            onClear={handleClear}
            onPageChange={handlePageChange}
          />
        </section>
      </div>
    </>
  )
}

interface ResultsProps {
  state: ReturnType<typeof useCharacters>
  filtersActive: boolean
  onClear: () => void
  onPageChange: (page: number) => void
}

function Results({ state, filtersActive, onClear, onPageChange }: ResultsProps) {
  const { data, isPending, isError, isPlaceholderData, refetch } = state

  if (isPending) return <CharacterGridSkeleton />
  if (isError && !data) return <ErrorState onRetry={() => refetch()} />

  if (data.items.length === 0) {
    const pageOutOfRange = data.totalItems > 0
    return (
      <EmptyState
        icon={SearchX}
        title={pageOutOfRange ? 'Esta página não existe' : 'Nenhum personagem encontrado'}
        description={
          pageOutOfRange
            ? 'A lista tem menos páginas do que o endereço indica.'
            : 'Tente buscar por outro nome ou remover alguns filtros.'
        }
        action={
          pageOutOfRange ? (
            <Button onClick={() => onPageChange(1)}>Ir para a primeira página</Button>
          ) : (
            filtersActive && <Button onClick={onClear}>Limpar filtros</Button>
          )
        }
      />
    )
  }

  return (
    <div className={styles.resultsBody}>
      <p className={styles.count} role="status">
        {data.totalItems}{' '}
        {data.totalItems === 1 ? 'personagem encontrado' : 'personagens encontrados'}
      </p>
      <div
        className={isPlaceholderData ? styles.loadingMore : undefined}
        aria-busy={isPlaceholderData}
      >
        <CharacterGrid characters={data.items} />
      </div>
      <Pagination page={data.page} totalPages={data.totalPages} onPageChange={onPageChange} />
    </div>
  )
}
