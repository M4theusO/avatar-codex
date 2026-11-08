import { Link } from 'react-router'
import type { Discovery, DiscoveryKind } from '../../types/progress'
import { elementStyle } from '../../utils/labels'
import CharacterAvatar from '../common/CharacterAvatar'
import ElementIcon from '../common/ElementIcon'
import styles from './Dashboard.module.css'

const dateFormat = new Intl.DateTimeFormat('pt-BR', { day: 'numeric', month: 'short' })

const kinds: Record<DiscoveryKind, { path: string; label: string }> = {
  Character: { path: 'characters', label: 'Personagem' },
  Nation: { path: 'nations', label: 'Nação' },
  Ability: { path: 'abilities', label: 'Habilidade' },
}

/** Descobertas recentes do painel (já vêm no agregado do dashboard). */
export default function DiscoveryList({ discoveries }: { discoveries: Discovery[] }) {
  return (
    <ul className={styles.discoveries}>
      {discoveries.map((discovery) => {
        const kind = kinds[discovery.kind]
        return (
          <li key={`${discovery.kind}-${discovery.id}`}>
            <Link
              to={`/${kind.path}/${discovery.id}`}
              className={styles.discovery}
              style={elementStyle(discovery.element?.color)}
            >
              {discovery.kind === 'Character' ? (
                <CharacterAvatar
                  name={discovery.name}
                  image={discovery.image}
                  element={discovery.element}
                  size="sm"
                />
              ) : (
                <span className={styles.discoveryIcon}>
                  <ElementIcon icon={discovery.element?.icon} size={18} />
                </span>
              )}
              <span className={styles.discoveryText}>
                <span className={styles.discoveryName}>{discovery.name}</span>
                <span className={styles.discoveryMeta}>
                  {kind.label} · {dateFormat.format(new Date(discovery.discoveredAt))}
                </span>
              </span>
            </Link>
          </li>
        )
      })}
    </ul>
  )
}
