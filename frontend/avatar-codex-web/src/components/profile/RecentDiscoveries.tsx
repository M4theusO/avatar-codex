import { Compass } from 'lucide-react'
import { Link } from 'react-router'
import { useRecentDiscoveries } from '../../hooks/useUserData'
import type { Discovery } from '../../types/progress'
import { elementStyle } from '../../utils/labels'
import { ButtonLink } from '../common/Button'
import CharacterAvatar from '../common/CharacterAvatar'
import ElementIcon from '../common/ElementIcon'
import Skeleton from '../common/Skeleton'
import { EmptyState, ErrorState } from '../common/StatusMessage'
import styles from './Profile.module.css'

const dateFormat = new Intl.DateTimeFormat('pt-BR', { day: 'numeric', month: 'short' })

export default function RecentDiscoveries() {
  const { data, isPending, isError, refetch } = useRecentDiscoveries()

  if (isPending) return <Skeleton height="10rem" />
  if (isError) {
    return (
      <ErrorState title="Não foi possível carregar suas descobertas" onRetry={() => refetch()} />
    )
  }
  if (data.items.length === 0) {
    return (
      <EmptyState
        icon={Compass}
        title="Sua jornada está começando"
        description="Abra a página de um personagem ou de uma nação para descobri-lo e ganhar XP."
        action={<ButtonLink to="/characters">Explorar personagens</ButtonLink>}
      />
    )
  }

  return <DiscoveryList discoveries={data.items} />
}

/** Descobertas já carregadas (perfil e dashboard). */
export function DiscoveryList({ discoveries }: { discoveries: Discovery[] }) {
  return (
    <ul className={styles.discoveries}>
      {discoveries.map((discovery) => {
        const isCharacter = discovery.kind === 'Character'
        return (
          <li key={`${discovery.kind}-${discovery.id}`}>
            <Link
              to={`/${isCharacter ? 'characters' : 'nations'}/${discovery.id}`}
              className={styles.discovery}
              style={elementStyle(discovery.element?.color)}
            >
              {isCharacter ? (
                <CharacterAvatar
                  name={discovery.name}
                  image={discovery.image}
                  element={discovery.element}
                  size="sm"
                />
              ) : (
                <span className={styles.nationIcon}>
                  <ElementIcon icon={discovery.element?.icon} size={18} />
                </span>
              )}
              <span className={styles.discoveryText}>
                <span className={styles.discoveryName}>{discovery.name}</span>
                <span className={styles.discoveryMeta}>
                  {isCharacter ? 'Personagem' : 'Nação'} ·{' '}
                  {dateFormat.format(new Date(discovery.discoveredAt))}
                </span>
              </span>
            </Link>
          </li>
        )
      })}
    </ul>
  )
}
