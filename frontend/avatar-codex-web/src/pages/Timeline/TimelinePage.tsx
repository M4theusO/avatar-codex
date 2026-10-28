import { Hourglass } from 'lucide-react'
import { useSearchParams } from 'react-router'
import { Button } from '../../components/common/Button'
import PageHeader from '../../components/common/PageHeader'
import Skeleton from '../../components/common/Skeleton'
import { EmptyState, ErrorState } from '../../components/common/StatusMessage'
import TimelineEraSection from '../../components/timeline/TimelineEraSection'
import TimelineFilters from '../../components/timeline/TimelineFilters'
import styles from '../../components/timeline/Timeline.module.css'
import { useEras, useTimeline } from '../../hooks/useContent'
import type { TimelineFilters as Filters } from '../../types/content'
import {
  parseTimelineFilters,
  timelineCharacters,
  timelineFiltersToParams,
} from '../../utils/timeline'

export default function TimelinePage() {
  const [params, setParams] = useSearchParams()
  const filters = parseTimelineFilters(params)
  const timeline = useTimeline(filters)
  // As opções de personagem vêm da linha do tempo completa (mesma consulta quando não há filtro).
  const everything = useTimeline({})
  const eras = useEras()

  function handleChange(next: Filters) {
    setParams(timelineFiltersToParams(next), { replace: true })
  }

  return (
    <div className={styles.page}>
      <title>Linha do tempo · Avatar Codex</title>
      <PageHeader
        title="Linha do tempo"
        description="Dos primeiros Avatares à República Unida. Datas em AG/BG: anos depois/antes do genocídio dos Nômades do Ar, a contagem usada pelos fãs."
      />

      <TimelineFilters
        filters={filters}
        eras={eras.data ?? []}
        characters={timelineCharacters(everything.data)}
        onChange={handleChange}
      />

      {timeline.isPending ? (
        <div className={styles.timeline} aria-busy="true">
          <span className="visually-hidden" role="status">
            Carregando a linha do tempo…
          </span>
          <Skeleton height="4rem" width="60%" />
          <Skeleton height="10rem" />
          <Skeleton height="10rem" />
        </div>
      ) : timeline.isError ? (
        <ErrorState
          title="Não foi possível carregar a linha do tempo"
          onRetry={() => timeline.refetch()}
        />
      ) : timeline.data.length === 0 ? (
        <EmptyState
          icon={Hourglass}
          title="Nenhum evento encontrado"
          description="Nenhum acontecimento combina com esses filtros."
          action={<Button onClick={() => handleChange({})}>Limpar filtros</Button>}
        />
      ) : (
        <div className={styles.timeline}>
          {timeline.data.map((era) => (
            <TimelineEraSection key={era.id} era={era} />
          ))}
        </div>
      )}
    </div>
  )
}
