import { Landmark } from 'lucide-react'
import PageHeader from '../../components/common/PageHeader'
import Skeleton from '../../components/common/Skeleton'
import { EmptyState, ErrorState } from '../../components/common/StatusMessage'
import NationCard from '../../components/nations/NationCard'
import { useNations } from '../../hooks/useContent'
import styles from './NationsPage.module.css'

export default function NationsPage() {
  const { data, isPending, isError, refetch } = useNations()

  return (
    <>
      <title>Nações · Avatar Codex</title>
      <PageHeader
        title="Nações"
        description="Os povos do mundo de Avatar, cada um ligado a um elemento e a uma cultura própria."
      />

      {isPending ? (
        <div className={styles.grid} aria-busy="true">
          <span className="visually-hidden" role="status">
            Carregando nações…
          </span>
          {Array.from({ length: 4 }, (_, index) => (
            <Skeleton key={index} height="14rem" />
          ))}
        </div>
      ) : isError ? (
        <ErrorState onRetry={() => refetch()} />
      ) : data.length === 0 ? (
        <EmptyState icon={Landmark} title="Nenhuma nação cadastrada ainda" />
      ) : (
        <ul className={styles.grid}>
          {data.map((nation) => (
            <li key={nation.id}>
              <NationCard nation={nation} />
            </li>
          ))}
        </ul>
      )}
    </>
  )
}
