import { Eye, Landmark, Scale, Star } from 'lucide-react'
import { Link, useLocation } from 'react-router'
import { useAuth } from '../../hooks/useAuth'
import type { CharacterDetails } from '../../types/content'
import { elementStyle, statusLabels } from '../../utils/labels'
import { loginPath } from '../../utils/redirect'
import { Badge, ElementBadge } from '../common/Badge'
import { ButtonLink } from '../common/Button'
import CharacterAvatar from '../common/CharacterAvatar'
import FavoriteButton from './FavoriteButton'
import styles from './CharacterHero.module.css'

export default function CharacterHero({ character }: { character: CharacterDetails }) {
  const { id, name, aliases, image, element, nation, isAvatar, isBender, status } = character
  const { status: sessionStatus } = useAuth()
  const location = useLocation()

  return (
    <header className={styles.hero} style={elementStyle(element?.color)}>
      <CharacterAvatar name={name} image={image} element={element} size="lg" />
      <div className={styles.info}>
        <h1 className={styles.name}>{name}</h1>
        {aliases.length > 0 && (
          <p className={styles.aliases}>Também conhecido como {aliases.join(', ')}</p>
        )}
        <div className={styles.badges}>
          {element ? <ElementBadge element={element} /> : !isBender && <Badge>Não dobrador</Badge>}
          {isAvatar && <Badge>Avatar</Badge>}
          <Badge>{statusLabels[status]}</Badge>
          {sessionStatus === 'authenticated' && character.isDiscovered && (
            <Badge>
              <Eye size={12} aria-hidden="true" />
              Descoberto
            </Badge>
          )}
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
          {character.firstAppearance && (
            <div>
              <dt>Primeira aparição</dt>
              <dd>{character.firstAppearance}</dd>
            </div>
          )}
        </dl>
        <div className={styles.actions}>
          {sessionStatus === 'authenticated' ? (
            <FavoriteButton
              characterId={id}
              name={name}
              isFavorite={character.isFavorite}
              variant="full"
            />
          ) : (
            sessionStatus === 'anonymous' && (
              <ButtonLink to={loginPath(location.pathname)} variant="ghost">
                <Star size={18} aria-hidden="true" />
                Entre para favoritar
              </ButtonLink>
            )
          )}
          <ButtonLink to={`/compare?a=${id}`} variant="ghost">
            <Scale size={18} aria-hidden="true" />
            Comparar
          </ButtonLink>
        </div>
      </div>
    </header>
  )
}
