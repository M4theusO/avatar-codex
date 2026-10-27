import { History } from 'lucide-react'
import { Link, useSearchParams } from 'react-router'
import BackLink from '../../components/common/BackLink'
import { ButtonLink } from '../../components/common/Button'
import PageHeader from '../../components/common/PageHeader'
import Pagination from '../../components/common/Pagination'
import Skeleton from '../../components/common/Skeleton'
import { EmptyState, ErrorState } from '../../components/common/StatusMessage'
import { useQuizHistory } from '../../hooks/useQuizzes'
import styles from './QuizHistoryPage.module.css'

const dateFormat = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' })

export default function QuizHistoryPage() {
  const [params, setParams] = useSearchParams()
  const page = Math.max(1, Number(params.get('page')) || 1)
  const { data, isPending, isError, refetch } = useQuizHistory(page)

  function handlePageChange(next: number) {
    setParams(next > 1 ? { page: String(next) } : {})
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  return (
    <>
      <title>Histórico de quizzes · Avatar Codex</title>
      <BackLink to="/quizzes" label="Todos os quizzes" />
      <PageHeader
        title="Histórico de quizzes"
        description="Todas as suas tentativas, inclusive as que não renderam XP."
      />

      {isPending ? (
        <Skeleton height="16rem" />
      ) : isError ? (
        <ErrorState title="Não foi possível carregar seu histórico" onRetry={() => refetch()} />
      ) : data.items.length === 0 ? (
        <EmptyState
          icon={History}
          title="Você ainda não jogou nenhum quiz"
          description="Escolha um quiz e teste o que você sabe sobre Avatar."
          action={<ButtonLink to="/quizzes">Ver quizzes</ButtonLink>}
        />
      ) : (
        <div className={styles.content}>
          <ol className={styles.list}>
            {data.items.map((attempt) => (
              <li key={attempt.id} className={styles.item}>
                <div className={styles.main}>
                  <Link to={`/quizzes/${attempt.quizId}`} className={styles.title}>
                    {attempt.quizTitle}
                  </Link>
                  <time className={styles.date} dateTime={attempt.completedAt}>
                    {dateFormat.format(new Date(attempt.completedAt))}
                  </time>
                </div>
                <div className={styles.numbers}>
                  <span className={styles.score}>
                    {attempt.score}/{attempt.totalQuestions}
                    <small>{attempt.percentage}%</small>
                  </span>
                  <span className={attempt.experienceEarned > 0 ? styles.xp : styles.noXp}>
                    {attempt.experienceEarned > 0 ? `+${attempt.experienceEarned} XP` : 'Sem XP'}
                  </span>
                </div>
              </li>
            ))}
          </ol>
          <Pagination
            page={data.page}
            totalPages={data.totalPages}
            onPageChange={handlePageChange}
          />
        </div>
      )}
    </>
  )
}
