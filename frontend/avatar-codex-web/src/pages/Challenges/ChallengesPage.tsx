import ChallengeHistory from '../../components/challenges/ChallengeHistory'
import DailyChallengeList from '../../components/challenges/DailyChallengeList'
import PageHeader from '../../components/common/PageHeader'
import Section from '../../components/common/Section'
import Skeleton from '../../components/common/Skeleton'
import { ErrorState } from '../../components/common/StatusMessage'
import { useDailyChallenges } from '../../hooks/useUserData'
import styles from './ChallengesPage.module.css'

export default function ChallengesPage() {
  const { data, isPending, isError, refetch } = useDailyChallenges()

  return (
    <div className={styles.page}>
      <title>Desafios diários · Avatar Codex</title>
      <PageHeader
        title="Desafios diários"
        description="Três desafios novos por dia. Eles são concluídos sozinhos quando você cumpre a meta, e o XP entra na hora."
      />

      <Section title="Desafios de hoje">
        {isPending ? (
          <Skeleton height="9rem" />
        ) : isError ? (
          <ErrorState
            title="Não foi possível carregar os desafios de hoje"
            onRetry={() => refetch()}
          />
        ) : (
          <DailyChallengeList daily={data} headingLevel="h3" />
        )}
      </Section>

      <Section title="Concluídos">
        <ChallengeHistory />
      </Section>
    </div>
  )
}
