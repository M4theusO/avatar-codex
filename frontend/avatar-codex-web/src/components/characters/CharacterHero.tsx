import { Landmark } from 'lucide-react'
import { Link } from 'react-router'
import type { CharacterDetails } from '../../types/content'
import { elementStyle, statusLabels } from '../../utils/labels'
import { Badge, ElementBadge } from '../common/Badge'
import CharacterAvatar from '../common/CharacterAvatar'
import styles from './CharacterHero.module.css'

export default function CharacterHero({ character }: { character: CharacterDetails }) {
  const { name, aliases, image, element, nation, isAvatar, isBender, status, firstAppearance } =
    character

  return (
    <header className={styles.hero} style={elementStyle(element?.color)}>
      <CharacterAvatar name={name} image={image} color={element?.color} size="lg" />
      <div className={styles.info}>
        <h1 className={styles.name}>{name}</h1>
        {aliases.length > 0 && (
          <p className={styles.aliases}>Também conhecido como {aliases.join(', ')}</p>
        )}
        <div className={styles.badges}>
          {element ? <ElementBadge element={element} /> : !isBender && <Badge>Não dobrador</Badge>}
          {isAvatar && <Badge>Avatar</Badge>}
          <Badge>{statusLabels[status]}</Badge>
        </div>
        <dl className={styles.facts}>
          {nation && (
            <div>
              <dt>Nação</dt>
              <dd>
                <Link to={`/nations/${nation.id}`} className={styles.factLink}>
                  <Landmark size={16} aria-hidden="true" />
                  {nation.name}
                </Link>
              </dd>
            </div>
          )}
          {firstAppearance && (
            <div>
              <dt>Primeira aparição</dt>
              <dd>{firstAppearance}</dd>
            </div>
          )}
        </dl>
      </div>
    </header>
  )
}
