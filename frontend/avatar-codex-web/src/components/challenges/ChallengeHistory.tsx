import { CircleCheck, History } from 'lucide-react'
import { useState } from 'react'
import { useChallengeHistory } from '../../hooks/useUserData'
import Pagination from '../common/Pagination'
import Skeleton from '../common/Skeleton'
import { ErrorState } from '../common/StatusMessage'
import styles from './Challenges.module.css'

const dateFormat = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' })

/** Desafios concluídos (RF-CHAL-02), mais recentes primeiro. */
export default function ChallengeHistory() {
  const [page, setPage] = useState(1)
  const { data, isPending, isError, refetch } = useChallengeHistory(page)

  if (isPending) return <Skeleton height="10rem" />
  if (isError) {
    return <ErrorState title="Não foi possível carregar o histórico" onRetry={() => refetch()} />
  }
  if (data.items.length === 0) {
    return (
      <p className={styles.empty}>
        <History size={16} aria-hidden="true" />
        Os desafios que você concluir aparecem aqui.
      </p>
    )
  }

  return (
    <div className={styles.history}>
      <ol className={styles.historyList}>
        {data.items.map((challenge) => (
          <li key={challenge.id} className={styles.historyItem}>
            <CircleCheck size={18} className={styles.historyIcon} aria-hidden="true" />
            <span className={styles.historyText}>
              <span className={styles.historyTitle}>{challenge.title}</span>
              <time className={styles.historyDate} dateTime={challenge.completedAt}>
                {dateFormat.format(new Date(challenge.completedAt))}
              </time>
            </span>
            <span className={styles.historyXp}>+{challenge.experienceReward} XP</span>
          </li>
        ))}
      </ol>
      <Pagination page={data.page} totalPages={data.totalPages} onPageChange={setPage} />
    </div>
  )
}
