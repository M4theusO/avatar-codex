import { Link } from 'react-router'
import type { CharacterListItem } from '../../types/content'
import { elementStyle } from '../../utils/labels'
import { Badge, ElementBadge } from '../common/Badge'
import CharacterAvatar from '../common/CharacterAvatar'
import styles from './CharacterCard.module.css'

export default function CharacterCard({ character }: { character: CharacterListItem }) {
  const { id, name, description, image, nation, element, isAvatar, isBender } = character

  return (
    <article className={styles.card} style={elementStyle(element?.color)}>
      <CharacterAvatar name={name} image={image} color={element?.color} />
      <div className={styles.body}>
        <h3 className={styles.name}>
          {/* O link cobre o card inteiro (ver ::after no CSS) */}
          <Link to={`/characters/${id}`} className={styles.link}>
            {name}
          </Link>
        </h3>
        {nation && <p className={styles.nation}>{nation.name}</p>}
        <div className={styles.badges}>
          {element ? <ElementBadge element={element} /> : !isBender && <Badge>Não dobrador</Badge>}
          {isAvatar && <Badge>Avatar</Badge>}
        </div>
        <p className={styles.description}>{description}</p>
      </div>
    </article>
  )
}
