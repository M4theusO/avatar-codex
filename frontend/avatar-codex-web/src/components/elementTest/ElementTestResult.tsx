import { RotateCcw, UserRound } from 'lucide-react'
import { useEffect, useRef } from 'react'
import type { ElementTestResult as Result } from '../../types/elementTest'
import { elementStyle } from '../../utils/labels'
import { Button, ButtonLink } from '../common/Button'
import ElementIcon from '../common/ElementIcon'
import ProgressBar from '../common/ProgressBar'
import styles from './ElementTest.module.css'

interface ElementTestResultProps {
  result: Result
  onRetake: () => void
}

/** Resultado vindo do backend: o elemento, a descrição dele e os pontos de cada elemento. */
export default function ElementTestResult({ result, onRetake }: ElementTestResultProps) {
  const { element, scores } = result
  const total = scores.reduce((sum, score) => sum + score.points, 0)
  const headingRef = useRef<HTMLHeadingElement>(null)

  // O resultado substitui as perguntas: o foco vai para ele, para leitores de tela anunciarem.
  useEffect(() => headingRef.current?.focus(), [])

  return (
    <section className={styles.result} style={elementStyle(element.color)}>
      <div className={styles.reveal}>
        <span className={styles.symbol}>
          <ElementIcon icon={element.icon} size={44} />
        </span>
        <p className={styles.eyebrow}>Seu elemento é</p>
        <h2 ref={headingRef} tabIndex={-1} className={styles.elementName}>
          {element.name}
        </h2>
        <p className={styles.description}>{element.description}</p>
      </div>

      <div className={styles.scores}>
        <h3 className={styles.scoresTitle}>Suas respostas</h3>
        <ul className={styles.scoreList}>
          {scores.map((score) => (
            <li key={score.element.id} style={elementStyle(score.element.color)}>
              <span className={styles.scoreName}>
                <ElementIcon icon={score.element.icon} size={16} className={styles.scoreIcon} />
                {score.element.name}
              </span>
              <ProgressBar
                value={score.points}
                max={total}
                label={`Respostas de ${score.element.name}`}
              />
              <span className={styles.scorePoints}>
                {score.points} de {total}
              </span>
            </li>
          ))}
        </ul>
      </div>

      <div className={styles.actions}>
        <Button onClick={onRetake}>
          <RotateCcw size={18} aria-hidden="true" />
          Refazer o teste
        </Button>
        <ButtonLink to="/profile" variant="primary">
          <UserRound size={18} aria-hidden="true" />
          Ver meu perfil
        </ButtonLink>
      </div>
    </section>
  )
}
