import { useEffect, useRef } from 'react'
import type { QuizQuestion } from '../../types/quiz'
import ProgressBar from '../common/ProgressBar'
import styles from './QuizPlay.module.css'

interface QuizQuestionStepProps {
  question: QuizQuestion
  index: number
  total: number
  selectedOptionId: number | undefined
  onSelect: (optionId: number) => void
}

const letters = ['A', 'B', 'C', 'D', 'E', 'F']

/** Uma pergunta por vez. Rádios nativos estilizados como botões: setas do teclado e leitor de tela funcionam. */
export default function QuizQuestionStep({
  question,
  index,
  total,
  selectedOptionId,
  onSelect,
}: QuizQuestionStepProps) {
  const headingRef = useRef<HTMLHeadingElement>(null)
  const isFirstRender = useRef(true)

  // Ao trocar de pergunta, o foco vai para o enunciado (não na primeira, para não pular o cabeçalho).
  useEffect(() => {
    if (isFirstRender.current) {
      isFirstRender.current = false
      return
    }
    headingRef.current?.focus()
  }, [question.id])

  return (
    <section className={styles.step}>
      <div className={styles.progress}>
        <span className={styles.counter}>
          Pergunta {index + 1} de {total}
        </span>
        <ProgressBar value={index + 1} max={total} label="Progresso do quiz" />
      </div>

      {/* O enunciado é o nome do grupo de opções (no lugar de um <legend>, para poder receber foco). */}
      <fieldset className={styles.fieldset} aria-labelledby={`question-${question.id}`}>
        <h2
          id={`question-${question.id}`}
          ref={headingRef}
          tabIndex={-1}
          className={styles.question}
        >
          {question.text}
        </h2>
        <div className={styles.options}>
          {question.options.map((option, optionIndex) => (
            <label
              key={option.id}
              className={`${styles.option} ${selectedOptionId === option.id ? styles.selected : ''}`}
            >
              <input
                type="radio"
                name={`question-${question.id}`}
                className={styles.radio}
                checked={selectedOptionId === option.id}
                onChange={() => onSelect(option.id)}
              />
              <span className={styles.letter} aria-hidden="true">
                {letters[optionIndex] ?? optionIndex + 1}
              </span>
              <span>{option.text}</span>
            </label>
          ))}
        </div>
      </fieldset>
    </section>
  )
}
