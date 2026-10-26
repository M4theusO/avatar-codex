import { Eye, Landmark, Sparkles, type LucideIcon } from 'lucide-react'
import { useState } from 'react'
import { useExperienceHistory } from '../../hooks/useUserData'
import type { ExperienceType } from '../../types/progress'
import Pagination from '../common/Pagination'
import Skeleton from '../common/Skeleton'
import { EmptyState, ErrorState } from '../common/StatusMessage'
import styles from './Profile.module.css'

const icons: Partial<Record<ExperienceType, LucideIcon>> = {
  CharacterDiscovered: Eye,
  NationDiscovered: Landmark,
}

const dateFormat = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' })

/** Cada movimentação de XP (RF-XP-01). A descrição vem pronta do backend. */
export default function ExperienceHistory() {
  const [page, setPage] = useState(1)
  const { data, isPending, isError, refetch } = useExperienceHistory(page)

  if (isPending) return <Skeleton height="12rem" />
  if (isError) {
    return (
      <ErrorState title="Não foi possível carregar seu histórico de XP" onRetry={() => refetch()} />
    )
  }
  if (data.items.length === 0) {
    return (
      <EmptyState
        icon={Sparkles}
        title="Nenhum XP ainda"
        description="Descobrir personagens e nações rende XP. Ele aparece aqui."
      />
    )
  }

  return (
    <div className={styles.history}>
      <ol className={styles.historyList}>
        {data.items.map((transaction) => {
          const Icon = icons[transaction.type] ?? Sparkles
          return (
            <li key={transaction.id} className={styles.historyItem}>
              <Icon size={18} className={styles.historyIcon} aria-hidden="true" />
              <span className={styles.historyText}>
                <span>{transaction.description}</span>
                <time className={styles.historyDate} dateTime={transaction.createdAt}>
                  {dateFormat.format(new Date(transaction.createdAt))}
                </time>
              </span>
              <span className={styles.historyAmount}>+{transaction.amount} XP</span>
            </li>
          )
        })}
      </ol>
      <Pagination page={data.page} totalPages={data.totalPages} onPageChange={setPage} />
    </div>
  )
}
