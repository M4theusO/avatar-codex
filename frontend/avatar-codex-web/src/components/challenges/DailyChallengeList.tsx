import { Clock } from 'lucide-react'
import type { DailyChallenges } from '../../types/progress'
import ChallengeCard from './ChallengeCard'
import styles from './Challenges.module.css'

const timeFormat = new Intl.DateTimeFormat('pt-BR', { hour: '2-digit', minute: '2-digit' })

interface DailyChallengeListProps {
  daily: DailyChallenges
  headingLevel?: 'h2' | 'h3'
}

/** Desafios do dia e o horário do próximo reset (meia-noite UTC no horário local). */
export default function DailyChallengeList({ daily, headingLevel }: DailyChallengeListProps) {
  const completed = daily.challenges.filter((c) => c.isCompleted).length

  return (
    <div className={styles.daily}>
      <p className={styles.summary}>
        <span>
          {completed} de {daily.challenges.length} concluídos
        </span>
        <span className={styles.reset}>
          <Clock size={14} aria-hidden="true" />
          Novos desafios às {timeFormat.format(new Date(daily.resetsAt))}
        </span>
      </p>
      <ul className={styles.list}>
        {daily.challenges.map((challenge) => (
          <ChallengeCard key={challenge.id} challenge={challenge} headingLevel={headingLevel} />
        ))}
      </ul>
    </div>
  )
}
