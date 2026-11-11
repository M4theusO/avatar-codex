import { Lock, Pencil, Trophy, UserX } from 'lucide-react'
import { useParams } from 'react-router'
import AchievementCard from '../../components/achievements/AchievementCard'
import BackLink from '../../components/common/BackLink'
import { ElementBadge } from '../../components/common/Badge'
import { ButtonLink } from '../../components/common/Button'
import Section from '../../components/common/Section'
import Skeleton from '../../components/common/Skeleton'
import { EmptyState, ErrorState } from '../../components/common/StatusMessage'
import { StatsGrid, StatsGridSkeleton } from '../../components/profile/ExplorationStats'
import ProfileHero from '../../components/profile/ProfileHero'
import FollowButton from '../../components/social/FollowButton'
import { usePublicProfile } from '../../hooks/usePublicProfile'
import type { PublicProfile } from '../../types/profile'
import { isNotFoundError } from '../../utils/apiError'
import { formatMemberSince } from '../../utils/profile'
import styles from './UserProfilePage.module.css'

/** Perfil público de outro usuário (ou o seu, como os outros veem). Só para quem está logado. */
export default function UserProfilePage() {
  const username = useParams().username ?? ''
  const { data, isPending, isError, error, refetch } = usePublicProfile(username)

  return (
    <>
      <BackLink to="/ranking" label="Ranking" />
      {isNotFoundError(error) ? (
        <EmptyState
          icon={UserX}
          title="Perfil não encontrado"
          description="O usuário não existe ou deixou o perfil privado."
          action={<ButtonLink to="/ranking">Ver ranking</ButtonLink>}
        />
      ) : isPending ? (
        <div className={styles.page} aria-busy="true">
          <Skeleton height="14rem" />
          <StatsGridSkeleton />
        </div>
      ) : isError ? (
        <ErrorState title="Não foi possível carregar o perfil" onRetry={() => refetch()} />
      ) : (
        <ProfileContent profile={data} />
      )}
    </>
  )
}

function ProfileContent({ profile }: { profile: PublicProfile }) {
  const { username, element, isCurrentUser, isProfilePrivate, statistics, achievements } = profile

  return (
    <article className={styles.page}>
      <title>{`${username} · Avatar Codex`}</title>
      <ProfileHero
        profile={profile}
        subtitle={
          isCurrentUser &&
          (isProfilePrivate ? (
            <span className={styles.notice}>
              <Lock size={14} aria-hidden="true" />
              Seu perfil é privado: só você vê esta página.
            </span>
          ) : (
            'É assim que os outros usuários veem seu perfil.'
          ))
        }
        details={
          <>
            <div>
              <dt>Elemento</dt>
              <dd>{element ? <ElementBadge element={element} /> : 'Ainda não definido'}</dd>
            </div>
            <div>
              <dt>Membro desde</dt>
              <dd>{formatMemberSince(profile.createdAt)}</dd>
            </div>
            <div>
              <dt>Seguidores</dt>
              <dd>{profile.followersCount}</dd>
            </div>
            <div>
              <dt>Seguindo</dt>
              <dd>{profile.followingCount}</dd>
            </div>
          </>
        }
        action={
          isCurrentUser ? (
            <ButtonLink to="/profile" variant="ghost">
              <Pencil size={18} aria-hidden="true" />
              Editar perfil
            </ButtonLink>
          ) : (
            <FollowButton username={username} isFollowing={profile.isFollowing} />
          )
        }
      />

      <Section title="Exploração">
        <StatsGrid statistics={statistics} />
      </Section>

      <Section title={`Conquistas (${achievements.length})`}>
        {achievements.length > 0 ? (
          <ul className={styles.achievements}>
            {achievements.map((achievement) => (
              <AchievementCard
                key={achievement.id}
                headingLevel="h3"
                achievement={{ ...achievement, isUnlocked: true, progress: 1, target: 1 }}
              />
            ))}
          </ul>
        ) : (
          <EmptyState icon={Trophy} title="Nenhuma conquista desbloqueada ainda" />
        )}
      </Section>
    </article>
  )
}
