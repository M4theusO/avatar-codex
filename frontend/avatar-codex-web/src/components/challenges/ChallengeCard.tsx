import { CircleCheck, Compass, Landmark, ListChecks, Target } from 'lucide-react'
import type { ChallengeType, DailyChallenge } from '../../types/progress'
import ProgressBar from '../common/ProgressBar'
import styles from './Challenges.module.css'

const icons: Record<ChallengeType, typeof Target> = {
  DiscoverCharacters: Compass,
  DiscoverNations: Landmark,
  CompleteQuizzes: ListChecks,
  QuizScoreAtLeast: Target,
}

/** Em `QuizScoreAtLeast` o progresso é a melhor porcentagem do dia. */
function progressText({ type, progress, target }: DailyChallenge) {
  return type === 'QuizScoreAtLeast' ? `${progress}% de ${target}%` : `${progress} / ${target}`
}

interface ChallengeCardProps {
  challenge: DailyChallenge
  /** h3 quando o card fica dentro de uma seção com título próprio. */
  headingLevel?: 'h2' | 'h3'
}

export default function ChallengeCard({
  challenge,
  headingLevel: Heading = 'h2',
}: ChallengeCardProps) {
  const { title, description, experienceReward, isCompleted, progress, target, type } = challenge
  const Icon = icons[type] ?? Target

  return (
    <li className={`${styles.card} ${isCompleted ? styles.completed : ''}`}>
      <span className={styles.icon}>
        <Icon size={20} aria-hidden="true" />
      </span>
      <div className={styles.body}>
        <div className={styles.heading}>
          <Heading className={styles.title}>{title}</Heading>
          <span className={styles.xp}>+{experienceReward} XP</span>
        </div>
        <p className={styles.description}>{description}</p>
        {isCompleted ? (
          <p className={styles.status}>
            <CircleCheck size={16} aria-hidden="true" />
            Concluído
          </p>
        ) : (
          <div className={styles.progress}>
            <span className={styles.progressText}>
              <span className="visually-hidden">Progresso: </span>
              {progressText(challenge)}
            </span>
            <ProgressBar value={progress} max={target} label={`Progresso de ${title}`} />
          </div>
        )}
      </div>
    </li>
  )
}
