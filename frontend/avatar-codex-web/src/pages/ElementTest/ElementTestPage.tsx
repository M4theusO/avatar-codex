import { ArrowLeft, ArrowRight, Send, Sparkles } from 'lucide-react'
import { useState } from 'react'
import { ElementBadge } from '../../components/common/Badge'
import { Button } from '../../components/common/Button'
import PageHeader from '../../components/common/PageHeader'
import Skeleton from '../../components/common/Skeleton'
import { ErrorState } from '../../components/common/StatusMessage'
import elementStyles from '../../components/elementTest/ElementTest.module.css'
import ElementTestResult from '../../components/elementTest/ElementTestResult'
import styles from '../../components/quiz/QuizPlay.module.css'
import QuizQuestionStep from '../../components/quiz/QuizQuestionStep'
import { useAuth } from '../../hooks/useAuth'
import { useElementTest, useSubmitElementTest } from '../../hooks/useElementTest'
import { useRewardFeedback } from '../../hooks/useRewardFeedback'
import type { ElementTestResult as Result } from '../../types/elementTest'
import { getApiError } from '../../utils/apiError'

export default function ElementTestPage() {
  const { user } = useAuth()
  const { data: test, isPending, isError, refetch } = useElementTest()
  const submit = useSubmitElementTest()
  const celebrate = useRewardFeedback()

  const [started, setStarted] = useState(false)
  const [index, setIndex] = useState(0)
  const [answers, setAnswers] = useState<Record<number, number>>({})
  const [result, setResult] = useState<Result | null>(null)

  const header = (
    <PageHeader
      title="Qual é o seu elemento?"
      description="Oito perguntas sobre como você age e o que valoriza. O resultado vai para o seu perfil."
    />
  )

  if (isPending) {
    return (
      <div className={styles.page} aria-busy="true">
        {header}
        <Skeleton height="16rem" />
      </div>
    )
  }

  if (isError) {
    return (
      <div className={styles.page}>
        {header}
        <ErrorState onRetry={() => refetch()} />
      </div>
    )
  }

  const questions = test.questions
  const question = questions[index]
  const isLast = index === questions.length - 1
  const allAnswered = questions.every((q) => answers[q.id] !== undefined)

  function handleSubmit() {
    if (!allAnswered || submit.isPending) return
    submit.mutate(
      questions.map((q) => ({ questionId: q.id, optionId: answers[q.id] })),
      {
        onSuccess: (testResult) => {
          setResult(testResult)
          // Os números vêm prontos do backend; conquistas e desafios têm os próprios toasts.
          const { reward, experienceEarned, element } = testResult
          if (reward) {
            celebrate(
              reward,
              experienceEarned > 0
                ? `Elemento descoberto — ${element.name}! +${experienceEarned} XP`
                : undefined,
            )
          }
          window.scrollTo({ top: 0, behavior: 'smooth' })
        },
      },
    )
  }

  function retake() {
    setAnswers({})
    setIndex(0)
    setResult(null)
    setStarted(true)
    submit.reset()
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  return (
    <div className={styles.page}>
      <title>Teste de elemento · Avatar Codex</title>
      {header}

      {result ? (
        <ElementTestResult result={result} onRetake={retake} />
      ) : !started ? (
        <section className={elementStyles.intro} aria-label="Sobre o teste">
          <span className={elementStyles.ring} aria-hidden="true" />
          <p className={elementStyles.introText}>
            Não há respostas certas: escolha a opção que mais combina com você. Cada resposta
            aproxima você de um dos quatro elementos.
          </p>
          {user?.element ? (
            <p className={elementStyles.current}>
              Seu elemento atual é <ElementBadge element={user.element} />. Refazer o teste
              substitui o resultado.
            </p>
          ) : (
            <p className={elementStyles.current}>
              <Sparkles size={16} aria-hidden="true" />
              Ganhe +{test.experienceReward} XP ao concluir o teste pela primeira vez.
            </p>
          )}
          <Button variant="primary" onClick={() => setStarted(true)}>
            Começar
            <ArrowRight size={18} aria-hidden="true" />
          </Button>
        </section>
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
            progressLabel="Progresso do teste"
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
                {submit.isPending ? 'Calculando…' : 'Ver meu elemento'}
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
        </>
      )}
    </div>
  )
}
