import { Users } from 'lucide-react'
import { useState } from 'react'
import { Link } from 'react-router'
import { useFollowList, type FollowList } from '../../hooks/useFollow'
import type { FollowUser } from '../../types/profile'
import { profilePath } from '../../utils/profile'
import { userAvatarImage } from '../../utils/userAvatar'
import { ButtonLink } from '../common/Button'
import CharacterAvatar from '../common/CharacterAvatar'
import Pagination from '../common/Pagination'
import Skeleton from '../common/Skeleton'
import { EmptyState, ErrorState } from '../common/StatusMessage'
import styles from './Connections.module.css'

const tabs: { value: FollowList; label: string }[] = [
  { value: 'following', label: 'Seguindo' },
  { value: 'followers', label: 'Seguidores' },
]

/** Seção do próprio perfil: quem você segue e quem segue você, com o total de cada lista. */
export default function Connections() {
  const [list, setList] = useState<FollowList>('following')
  const [page, setPage] = useState(1)
  const following = useFollowList('following', list === 'following' ? page : 1)
  const followers = useFollowList('followers', list === 'followers' ? page : 1)
  const current = list === 'following' ? following : followers

  const select = (next: FollowList) => {
    setList(next)
    setPage(1)
  }

  return (
    <div className={styles.connections}>
      <div className={styles.segmented} role="group" aria-label="Conexões">
        {tabs.map(({ value, label }) => {
          const total = (value === 'following' ? following : followers).data?.totalItems
          return (
            <button
              key={value}
              type="button"
              className={styles.segment}
              aria-pressed={list === value}
              onClick={() => select(value)}
            >
              {label}
              {total !== undefined && <span className={styles.count}> {total}</span>}
            </button>
          )
        })}
      </div>

      {current.isPending ? (
        <div className={styles.loading} aria-busy="true">
          {Array.from({ length: 3 }, (_, index) => (
            <Skeleton key={index} height="3.5rem" />
          ))}
        </div>
      ) : current.isError ? (
        <ErrorState title="Não foi possível carregar a lista" onRetry={() => current.refetch()} />
      ) : current.data.items.length === 0 ? (
        list === 'following' ? (
          <EmptyState
            icon={Users}
            title="Você ainda não segue ninguém"
            description="Abra o perfil de alguém pelo ranking e toque em Seguir."
            action={<ButtonLink to="/ranking">Ver ranking</ButtonLink>}
          />
        ) : (
          <EmptyState icon={Users} title="Ninguém segue você ainda" />
        )
      ) : (
        <>
          <UserList
            users={current.data.items}
            label={list === 'following' ? 'Seguindo' : 'Seguidores'}
          />
          <Pagination
            page={current.data.page}
            totalPages={current.data.totalPages}
            onPageChange={setPage}
          />
        </>
      )}
    </div>
  )
}

function UserList({ users, label }: { users: FollowUser[]; label: string }) {
  return (
    <ul className={styles.list} aria-label={label}>
      {users.map((person) => (
        <li key={person.username} className={styles.row}>
          <CharacterAvatar
            name={person.username}
            image={userAvatarImage(person.avatarUrl)}
            element={person.element}
            size="sm"
          />
          <span className={styles.person}>
            {person.isProfilePrivate ? (
              <span className={styles.name}>{person.username}</span>
            ) : (
              <Link to={profilePath(person.username)} className={`${styles.name} ${styles.link}`}>
                {person.username}
              </Link>
            )}
            <span className={styles.meta}>
              Nv. {person.level}
              {person.element && ` · ${person.element.name}`}
              {person.isProfilePrivate && ' · Perfil privado'}
            </span>
          </span>
        </li>
      ))}
    </ul>
  )
}
