import { Trophy } from 'lucide-react'
import AchievementCard from '../../components/achievements/AchievementCard'
import PageHeader from '../../components/common/PageHeader'
import ProgressBar from '../../components/common/ProgressBar'
import Skeleton from '../../components/common/Skeleton'
import { EmptyState, ErrorState } from '../../components/common/StatusMessage'
import { useAchievements } from '../../hooks/useUserData'
import styles from './AchievementsPage.module.css'

export default function AchievementsPage() {
  const { data, isPending, isError, refetch } = useAchievements()

  return (
    <>
      <title>Conquistas · Avatar Codex</title>
      <PageHeader
        title="Conquistas"
        description="Explore o mundo de Avatar para desbloquear conquistas e ganhar XP."
      />

      {isPending ? (
        <div className={styles.content} aria-busy="true">
          <Skeleton height="4.5rem" />
          <div className={styles.grid}>
            {Array.from({ length: 6 }, (_, index) => (
              <Skeleton key={index} height="9rem" />
            ))}
          </div>
        </div>
      ) : isError ? (
        <ErrorState title="Não foi possível carregar suas conquistas" onRetry={() => refetch()} />
      ) : data.length === 0 ? (
        <EmptyState
          icon={Trophy}
          title="Nenhuma conquista disponível"
          description="As conquistas aparecem aqui assim que forem cadastradas."
        />
      ) : (
        <div className={styles.content}>
          <Summary unlocked={data.filter((a) => a.isUnlocked).length} total={data.length} />
          <ul className={styles.grid}>
            {data.map((achievement) => (
              <AchievementCard key={achievement.id} achievement={achievement} />
            ))}
          </ul>
        </div>
      )}
    </>
  )
}

function Summary({ unlocked, total }: { unlocked: number; total: number }) {
  return (
    <section className={styles.summary} aria-label="Resumo das conquistas">
      <Trophy size={28} className={styles.summaryIcon} aria-hidden="true" />
      <div className={styles.summaryBody}>
        <p className={styles.summaryText}>
          <strong>{unlocked}</strong> de {total} conquistas desbloqueadas
        </p>
        <ProgressBar value={unlocked} max={total} label="Conquistas desbloqueadas" />
      </div>
    </section>
  )
}
