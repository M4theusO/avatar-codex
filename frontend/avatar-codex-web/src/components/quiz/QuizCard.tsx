import { CircleCheck, ListChecks } from 'lucide-react'
import type { QuizListItem } from '../../types/quiz'
import { difficultyLabels } from '../../utils/labels'
import { Badge } from '../common/Badge'
import { ButtonLink } from '../common/Button'
import styles from './QuizCard.module.css'

export default function QuizCard({ quiz }: { quiz: QuizListItem }) {
  const { id, title, description, category, difficulty, questionCount, bestResult } = quiz

  return (
    <article className={styles.card}>
      <div className={styles.tags}>
        <Badge>{category}</Badge>
        <span className={`${styles.difficulty} ${styles[difficulty]}`}>
          <span className={styles.dots} aria-hidden="true" />
          {difficultyLabels[difficulty]}
        </span>
      </div>

      <h2 className={styles.title}>{title}</h2>
      <p className={styles.description}>{description}</p>

      <div className={styles.footer}>
        <div className={styles.meta}>
          <span className={styles.metaItem}>
            <ListChecks size={16} aria-hidden="true" />
            {questionCount} perguntas
          </span>
          {bestResult && (
            <span className={`${styles.metaItem} ${styles.best}`}>
              <CircleCheck size={16} aria-hidden="true" />
              Melhor: {bestResult.score}/{bestResult.totalQuestions} ({bestResult.percentage}%)
            </span>
          )}
        </div>
        <ButtonLink
          to={`/quizzes/${id}`}
          variant={bestResult ? 'secondary' : 'primary'}
          aria-label={`${bestResult ? 'Jogar de novo' : 'Jogar'}: ${title}`}
        >
          {bestResult ? 'Jogar de novo' : 'Jogar'}
        </ButtonLink>
      </div>
    </article>
  )
}
