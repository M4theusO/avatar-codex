import { ArrowLeft, ArrowRight, Send } from 'lucide-react'
import { useState } from 'react'
import { useParams } from 'react-router'
import BackLink from '../../components/common/BackLink'
import { Badge } from '../../components/common/Badge'
import { Button } from '../../components/common/Button'
import Skeleton from '../../components/common/Skeleton'
import { ErrorState } from '../../components/common/StatusMessage'
import QuizQuestionStep from '../../components/quiz/QuizQuestionStep'
import QuizResult from '../../components/quiz/QuizResult'
import styles from '../../components/quiz/QuizPlay.module.css'
import { useQuiz, useSubmitQuiz } from '../../hooks/useQuizzes'
import { useRewardFeedback } from '../../hooks/useRewardFeedback'
import type { QuizAttemptResult } from '../../types/quiz'
import { getApiError, isNotFoundError } from '../../utils/apiError'
import { difficultyLabels } from '../../utils/labels'

export default function QuizPlayPage() {
  const id = Number(useParams().id)
  const { data: quiz, isPending, isError, error, refetch } = useQuiz(id)
  const submit = useSubmitQuiz(id)
  const celebrate = useRewardFeedback()

  const [index, setIndex] = useState(0)
  const [answers, setAnswers] = useState<Record<number, number>>({})
  const [result, setResult] = useState<QuizAttemptResult | null>(null)

  if (isPending) {
    return (
      <div className={styles.page} aria-busy="true">
        <Skeleton height="2.5rem" width="60%" />
        <Skeleton height="22rem" />
      </div>
    )
  }

  if (isError) {
    const notFound = isNotFoundError(error)
    return (
      <ErrorState
        title={notFound ? 'Quiz não encontrado' : 'Não foi possível carregar o quiz'}
        description={notFound ? 'Ele pode ter sido removido. Escolha outro na lista.' : undefined}
        onRetry={notFound ? undefined : () => refetch()}
      />
    )
  }

  const questions = quiz.questions
  const question = questions[index]
  const isLast = index === questions.length - 1
  const answeredCount = questions.filter((q) => answers[q.id] !== undefined).length
  const allAnswered = answeredCount === questions.length

  function handleSubmit() {
    if (!quiz || !allAnswered || submit.isPending) return
    submit.mutate(
      quiz.questions.map((q) => ({ questionId: q.id, optionId: answers[q.id] })),
      {
        onSuccess: (attempt) => {
          setResult(attempt)
          // Os números vêm prontos do backend; só o XP do quiz vai no toast, conquistas têm o seu.
          if (attempt.reward) {
            celebrate(
              attempt.reward,
              attempt.experienceEarned > 0
                ? `🎯 Quiz concluído — ${attempt.score}/${attempt.totalQuestions}! +${attempt.experienceEarned} XP`
                : undefined,
            )
          }
          window.scrollTo({ top: 0, behavior: 'smooth' })
        },
      },
    )
  }

  function playAgain() {
    setAnswers({})
    setIndex(0)
    setResult(null)
    submit.reset()
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  return (
    <div className={styles.page}>
      <title>{`${quiz.title} · Quizzes · Avatar Codex`}</title>
      <BackLink to="/quizzes" label="Todos os quizzes" />

      <header className={styles.header}>
        <div className={styles.tags}>
          <Badge>{quiz.category}</Badge>
          <Badge>{difficultyLabels[quiz.difficulty]}</Badge>
        </div>
        <h1 className={styles.title}>{quiz.title}</h1>
        {!result && <p className={styles.description}>{quiz.description}</p>}
      </header>

      {result ? (
        <QuizResult quiz={quiz} result={result} onPlayAgain={playAgain} />
      ) : (
        <>
          <QuizQuestionStep
            question={question}
            index={index}
            total={questions.length}
            selectedOptionId={answers[question.id]}
            onSelect={(optionId) =>
              setAnswers((current) => ({ ...current, [question.id]: optionId }))
            }
          />

          {submit.isError && (
            <p className={styles.submitError} role="alert">
              {getApiError(submit.error).message} Suas respostas foram mantidas; tente enviar de
              novo.
            </p>
          )}

          <div className={styles.navigation}>
            <Button onClick={() => setIndex(index - 1)} disabled={index === 0}>
              <ArrowLeft size={18} aria-hidden="true" />
              Anterior
            </Button>
            {isLast ? (
              <Button
                variant="primary"
                onClick={handleSubmit}
                disabled={!allAnswered || submit.isPending}
              >
                <Send size={18} aria-hidden="true" />
                {submit.isPending ? 'Enviando…' : 'Enviar respostas'}
              </Button>
            ) : (
              <Button
                variant="primary"
                onClick={() => setIndex(index + 1)}
                disabled={answers[question.id] === undefined}
              >
                Próxima
                <ArrowRight size={18} aria-hidden="true" />
              </Button>
            )}
          </div>
          {isLast && !allAnswered && (
            <p className={styles.hint}>
              Faltam {questions.length - answeredCount} resposta(s). Volte às perguntas anteriores
              para responder.
            </p>
          )}
        </>
      )}
    </div>
  )
}
