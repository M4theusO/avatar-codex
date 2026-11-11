import { Medal } from 'lucide-react'
import { useSearchParams } from 'react-router'
import PageHeader from '../../components/common/PageHeader'
import Pagination from '../../components/common/Pagination'
import Skeleton from '../../components/common/Skeleton'
import { EmptyState, ErrorState } from '../../components/common/StatusMessage'
import MyPosition from '../../components/ranking/MyPosition'
import RankingFilters from '../../components/ranking/RankingFilters'
import RankingList from '../../components/ranking/RankingList'
import { useAuth } from '../../hooks/useAuth'
import { useElements } from '../../hooks/useContent'
import { useRanking } from '../../hooks/useRanking'
import type { ElementSummary } from '../../types/content'
import type { Ranking, RankingQuery } from '../../types/ranking'
import { monthLabel, parseRankingQuery, rankingQueryToParams } from '../../utils/ranking'
import styles from './RankingPage.module.css'

export default function RankingPage() {
  const { user } = useAuth()
  const [params, setParams] = useSearchParams()
  const query = parseRankingQuery(params)
  const ranking = useRanking(query)
  const elements = useElements()
  const filterElement = elements.data?.find((e) => e.id === query.elementId)

  function handleChange(next: RankingQuery) {
    setParams(rankingQueryToParams(next), { replace: true })
  }

  return (
    <div className={styles.page}>
      <title>Ranking · Avatar Codex</title>
      <PageHeader
        title="Ranking"
        description="Quem mais ganhou XP explorando o universo de Avatar: no geral, neste mês, por elemento ou só entre quem você segue."
      />

      <RankingFilters query={query} elements={elements.data ?? []} onChange={handleChange} />

      {ranking.isPending ? (
        <div className={styles.loading} aria-busy="true">
          <span className="visually-hidden" role="status">
            Carregando o ranking…
          </span>
          <Skeleton height="3.5rem" />
          {Array.from({ length: 5 }, (_, index) => (
            <Skeleton key={index} height="4rem" />
          ))}
        </div>
      ) : ranking.isError ? (
        <ErrorState title="Não foi possível carregar o ranking" onRetry={() => ranking.refetch()} />
      ) : (
        <RankingContent
          data={ranking.data}
          filterElement={filterElement}
          userElement={user?.element ?? null}
          onPageChange={(page) => handleChange({ ...query, page })}
        />
      )}
    </div>
  )
}

interface RankingContentProps {
  data: Ranking
  filterElement: ElementSummary | undefined
  userElement: ElementSummary | null
  onPageChange: (page: number) => void
}

function RankingContent({ data, filterElement, userElement, onPageChange }: RankingContentProps) {
  return (
    <>
      {data.periodStart && data.periodEnd && (
        <p className={styles.period}>{monthLabel(data.periodStart, data.periodEnd)}</p>
      )}
      <MyPosition ranking={data} filterElement={filterElement} userElement={userElement} />
      {data.entries.items.length === 0 ? (
        <EmptyState
          icon={Medal}
          title="Ninguém neste ranking ainda"
          description={
            data.following
              ? 'Nem você nem quem você segue ganhou XP neste período.'
              : filterElement
                ? `Ninguém com o elemento ${filterElement.name} ganhou XP neste período.`
                : 'Ninguém ganhou XP neste período.'
          }
        />
      ) : (
        <RankingList entries={data.entries.items} />
      )}
      {data.following && data.entries.totalItems <= 1 && (
        <p className={styles.hint}>
          Siga outros jogadores pelo perfil deles (é só tocar no nome em "Todo mundo") para comparar
          o XP de vocês aqui.
        </p>
      )}
      <Pagination
        page={data.entries.page}
        totalPages={data.entries.totalPages}
        onPageChange={onPageChange}
      />
    </>
  )
}
