import { ArrowRight } from 'lucide-react'
import { Link } from 'react-router'
import { CharacterGrid, CharacterGridSkeleton } from '../../components/characters/CharacterGrid'
import { ButtonLink } from '../../components/common/Button'
import Section from '../../components/common/Section'
import Skeleton from '../../components/common/Skeleton'
import { ErrorState } from '../../components/common/StatusMessage'
import AchievementsSummary from '../../components/dashboard/AchievementsSummary'
import DashboardHero from '../../components/dashboard/DashboardHero'
import DashboardStats, { DashboardStatsSkeleton } from '../../components/dashboard/DashboardStats'
import styles from '../../components/dashboard/Dashboard.module.css'
import { DiscoveryList } from '../../components/profile/RecentDiscoveries'
import QuizCard from '../../components/quiz/QuizCard'
import { useAuth } from '../../hooks/useAuth'
import { useDashboard } from '../../hooks/useUserData'
import type { Dashboard } from '../../types/progress'
import pageStyles from './DashboardPage.module.css'

export default function DashboardPage() {
  const { user } = useAuth()
  const { data, isPending, isError, refetch } = useDashboard()

  // RequireAuth garante a sessão; o null só existe durante o logout.
  if (!user) return null

  return (
    <div className={pageStyles.page}>
      <title>Painel · Avatar Codex</title>
      <DashboardHero user={user} />

      {isPending ? (
        <DashboardSkeleton />
      ) : isError ? (
        <ErrorState title="Não foi possível carregar seu painel" onRetry={() => refetch()} />
      ) : (
        <DashboardContent dashboard={data} />
      )}
    </div>
  )
}

function DashboardContent({ dashboard }: { dashboard: Dashboard }) {
  const { statistics, featuredQuiz, recommendations, recentDiscoveries, favorites } = dashboard

  return (
    <>
      <DashboardStats statistics={statistics} />

      <div className={styles.columns}>
        <Section title="Quiz em destaque" action={<SeeAll to="/quizzes">Ver quizzes</SeeAll>}>
          {featuredQuiz ? (
            <QuizCard quiz={featuredQuiz} headingLevel="h3" />
          ) : (
            <div className={styles.note}>
              <p>Você gabaritou todos os quizzes. Impressionante!</p>
              <ButtonLink to="/quizzes/history">Ver histórico</ButtonLink>
            </div>
          )}
        </Section>

        <Section title="Conquistas" action={<SeeAll to="/achievements">Ver todas</SeeAll>}>
          <AchievementsSummary
            recent={dashboard.recentAchievements}
            next={dashboard.nextAchievement}
          />
        </Section>
      </div>

      <Section
        title="Continue explorando"
        action={<SeeAll to="/characters">Ver personagens</SeeAll>}
      >
        {recommendations.length > 0 ? (
          <CharacterGrid characters={recommendations} />
        ) : (
          <p className={styles.note}>Você já descobriu todos os personagens!</p>
        )}
      </Section>

      <Section title="Descobertas recentes" action={<SeeAll to="/profile">Ver perfil</SeeAll>}>
        {recentDiscoveries.length > 0 ? (
          <DiscoveryList discoveries={recentDiscoveries} />
        ) : (
          <div className={styles.note}>
            <p>Abra a página de um personagem ou de uma nação para descobri-lo e ganhar XP.</p>
            <ButtonLink to="/nations">Conhecer as nações</ButtonLink>
          </div>
        )}
      </Section>

      <Section title="Favoritos" action={<SeeAll to="/favorites">Ver todos</SeeAll>}>
        {favorites.length > 0 ? (
          <CharacterGrid characters={favorites} />
        ) : (
          <p className={styles.note}>
            Use a estrela nos cards de personagem para guardar seus favoritos aqui.
          </p>
        )}
      </Section>
    </>
  )
}

function SeeAll({ to, children }: { to: string; children: string }) {
  return (
    <Link to={to} className={styles.seeAll}>
      {children} <ArrowRight size={16} aria-hidden="true" />
    </Link>
  )
}

function DashboardSkeleton() {
  return (
    <>
      <span className="visually-hidden" role="status">
        Carregando seu painel…
      </span>
      <DashboardStatsSkeleton />
      <div className={styles.columns} aria-hidden="true">
        <Skeleton height="14rem" />
        <Skeleton height="14rem" />
      </div>
      <CharacterGridSkeleton count={4} />
    </>
  )
}
