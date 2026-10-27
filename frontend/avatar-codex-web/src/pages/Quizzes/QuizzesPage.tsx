import { History, SearchX } from 'lucide-react'
import { useSearchParams } from 'react-router'
import { Button, ButtonLink } from '../../components/common/Button'
import { Select } from '../../components/common/Field'
import PageHeader from '../../components/common/PageHeader'
import Skeleton from '../../components/common/Skeleton'
import { EmptyState, ErrorState } from '../../components/common/StatusMessage'
import QuizCard from '../../components/quiz/QuizCard'
import { useAuth } from '../../hooks/useAuth'
import { useQuizzes } from '../../hooks/useQuizzes'
import type { QuizDifficulty, QuizFilters } from '../../types/quiz'
import { difficultyLabels } from '../../utils/labels'
import styles from './QuizzesPage.module.css'

const difficultyOptions = Object.entries(difficultyLabels).map(([value, label]) => ({
  value,
  label,
}))

function isDifficulty(value: string | null): value is QuizDifficulty {
  return value !== null && value in difficultyLabels
}

export default function QuizzesPage() {
  const { user } = useAuth()
  const [params, setParams] = useSearchParams()

  // Filtros na URL: dá para compartilhar e voltar com o navegador.
  const difficulty = params.get('difficulty')
  const filters: QuizFilters = {
    category: params.get('category') || undefined,
    difficulty: isDifficulty(difficulty) ? difficulty : undefined,
  }
  const hasFilters = Boolean(filters.category || filters.difficulty)

  const { data, isPending, isError, refetch } = useQuizzes(filters)
  // As categorias vêm do catálogo completo (mesma consulta da página sem filtros, em cache).
  const catalog = useQuizzes()
  const categories = [...new Set((catalog.data ?? []).map((quiz) => quiz.category))].sort()

  function setFilter(name: keyof QuizFilters, value: string) {
    setParams((current) => {
      const next = new URLSearchParams(current)
      if (value) next.set(name, value)
      else next.delete(name)
      return next
    })
  }

  return (
    <>
      <title>Quizzes · Avatar Codex</title>
      <PageHeader
        title="Quizzes"
        description="Teste o que você sabe sobre o mundo de Avatar. A primeira conclusão de cada quiz rende XP, e gabaritar rende um bônus."
      />

      <div className={styles.toolbar}>
        <div className={styles.filters} role="group" aria-label="Filtros">
          <Select
            label="Categoria"
            emptyLabel="Todas"
            value={filters.category ?? ''}
            options={categories.map((category) => ({ value: category, label: category }))}
            onChange={(value) => setFilter('category', value)}
          />
          <Select
            label="Dificuldade"
            emptyLabel="Todas"
            value={filters.difficulty ?? ''}
            options={difficultyOptions}
            onChange={(value) => setFilter('difficulty', value)}
          />
        </div>
        {user && (
          <ButtonLink to="/quizzes/history" variant="ghost">
            <History size={18} aria-hidden="true" />
            Meu histórico
          </ButtonLink>
        )}
      </div>

      {isPending ? (
        <div className={styles.grid} aria-busy="true">
          {Array.from({ length: 4 }, (_, index) => (
            <Skeleton key={index} height="14rem" />
          ))}
        </div>
      ) : isError ? (
        <ErrorState title="Não foi possível carregar os quizzes" onRetry={() => refetch()} />
      ) : data.length === 0 ? (
        <EmptyState
          icon={SearchX}
          title="Nenhum quiz encontrado"
          description={hasFilters ? 'Tente outra categoria ou dificuldade.' : undefined}
          action={
            hasFilters && (
              <Button variant="ghost" onClick={() => setParams({})}>
                Limpar filtros
              </Button>
            )
          }
        />
      ) : (
        <ul className={styles.grid}>
          {data.map((quiz) => (
            <li key={quiz.id}>
              <QuizCard quiz={quiz} />
            </li>
          ))}
        </ul>
      )}
    </>
  )
}
