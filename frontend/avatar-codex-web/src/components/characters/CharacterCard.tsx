import { Eye } from 'lucide-react'
import { Link } from 'react-router'
import { useAuth } from '../../hooks/useAuth'
import type { CharacterListItem } from '../../types/content'
import { elementStyle } from '../../utils/labels'
import { Badge, ElementBadge } from '../common/Badge'
import CharacterAvatar from '../common/CharacterAvatar'
import styles from './CharacterCard.module.css'
import FavoriteButton from './FavoriteButton'

export default function CharacterCard({ character }: { character: CharacterListItem }) {
  const { id, name, description, image, nation, element, isAvatar, isBender } = character
  // Favorito e "descoberto" só fazem sentido para quem está logado (RF-CHAR-02).
  const { status } = useAuth()
  const signedIn = status === 'authenticated'

  return (
    <article className={styles.card} style={elementStyle(element?.color)}>
      <CharacterAvatar name={name} image={image} element={element} />
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
          {signedIn && character.isDiscovered && (
            <Badge>
              <Eye size={12} aria-hidden="true" />
              Descoberto
            </Badge>
          )}
        </div>
        <p className={styles.description}>{description}</p>
      </div>
      {signedIn && (
        <div className={styles.favorite}>
          <FavoriteButton characterId={id} name={name} isFavorite={character.isFavorite} />
        </div>
      )}
    </article>
  )
}
