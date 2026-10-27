import { CircleCheck, CircleX, History, RotateCcw, Sparkles, Trophy } from 'lucide-react'
import { useEffect, useRef } from 'react'
import type { Quiz, QuizAttemptResult } from '../../types/quiz'
import { Button, ButtonLink } from '../common/Button'
import ProgressBar from '../common/ProgressBar'
import styles from './QuizPlay.module.css'

interface QuizResultProps {
  quiz: Quiz
  result: QuizAttemptResult
  onPlayAgain: () => void
}

function headline(percentage: number) {
  if (percentage === 100) return 'Gabaritou!'
  if (percentage >= 70) return 'Mandou bem!'
  if (percentage >= 40) return 'Bom começo!'
  return 'Continue explorando!'
}

/** Resultado da correção feita pelo backend: pontuação, XP, conquistas e correção pergunta a pergunta. */
export default function QuizResult({ quiz, result, onPlayAgain }: QuizResultProps) {
  const headingRef = useRef<HTMLHeadingElement>(null)
  useEffect(() => headingRef.current?.focus(), [])

  const achievements = result.reward?.achievementsUnlocked ?? []
  const questions = new Map(quiz.questions.map((question) => [question.id, question]))
  const optionText = (questionId: number, optionId: number) =>
    questions.get(questionId)?.options.find((option) => option.id === optionId)?.text ?? '—'

  return (
    <div className={styles.result}>
      <section className={styles.summary} aria-labelledby="quiz-result-title">
        <h2 id="quiz-result-title" ref={headingRef} tabIndex={-1} className={styles.headline}>
          {headline(result.percentage)}
        </h2>
        <p className={styles.score}>
          <strong>
            {result.score} / {result.totalQuestions}
          </strong>
          <span>{result.percentage}% de acertos</span>
        </p>
        <ProgressBar value={result.score} max={result.totalQuestions} label="Acertos" />

        {result.experienceEarned > 0 ? (
          <p className={styles.xp}>
            <Sparkles size={18} aria-hidden="true" />+{result.experienceEarned} XP
            {result.percentage === 100 && ' (com o bônus de gabarito)'}
          </p>
        ) : (
          <p className={styles.noXp}>
            Você já recebeu o XP deste quiz. Esta tentativa fica registrada no seu histórico.
          </p>
        )}

        {achievements.length > 0 && (
          <ul className={styles.achievements} aria-label="Conquistas desbloqueadas">
            {achievements.map((achievement) => (
              <li key={achievement.id} className={styles.achievement}>
                <Trophy size={16} aria-hidden="true" />
                {achievement.name} · +{achievement.experienceReward} XP
              </li>
            ))}
          </ul>
        )}

        <div className={styles.actions}>
          <Button variant="primary" onClick={onPlayAgain}>
            <RotateCcw size={18} aria-hidden="true" />
            Jogar novamente
          </Button>
          <ButtonLink to="/quizzes/history" variant="secondary">
            <History size={18} aria-hidden="true" />
            Ver histórico
          </ButtonLink>
          <ButtonLink to="/quizzes" variant="ghost">
            Outros quizzes
          </ButtonLink>
        </div>
      </section>

      <section aria-labelledby="quiz-review-title">
        <h2 id="quiz-review-title" className={styles.reviewTitle}>
          Correção
        </h2>
        <ol className={styles.review}>
          {result.results.map((item) => {
            const question = questions.get(item.questionId)
            const Icon = item.isCorrect ? CircleCheck : CircleX
            return (
              <li
                key={item.questionId}
                className={`${styles.reviewItem} ${item.isCorrect ? '' : styles.wrong}`}
              >
                <Icon size={20} className={styles.reviewIcon} aria-hidden="true" />
                <div className={styles.reviewBody}>
                  <p className={styles.reviewQuestion}>
                    <span className="visually-hidden">
                      {item.isCorrect ? 'Acertou: ' : 'Errou: '}
                    </span>
                    {question?.text}
                  </p>
                  <p className={styles.reviewAnswer}>
                    Sua resposta:{' '}
                    <strong>{optionText(item.questionId, item.selectedOptionId)}</strong>
                  </p>
                  {!item.isCorrect && (
                    <p className={styles.reviewAnswer}>
                      Resposta correta:{' '}
                      <strong>{optionText(item.questionId, item.correctOptionId)}</strong>
                    </p>
                  )}
                  {item.explanation && <p className={styles.explanation}>{item.explanation}</p>}
                </div>
              </li>
            )
          })}
        </ol>
      </section>
    </div>
  )
}
