import { Link } from 'react-router'
import type { CharacterRelationship } from '../../types/content'
import { relationshipLabels } from '../../utils/labels'
import CharacterAvatar from '../common/CharacterAvatar'
import styles from './DetailLists.module.css'

export default function RelationshipList({
  relationships,
}: {
  relationships: CharacterRelationship[]
}) {
  return (
    <ul className={styles.cardGrid}>
      {relationships.map(({ character, type, description }) => {
        const label = relationshipLabels[type]
        // Evita "Mentor · Mentor" quando a descrição só repete o tipo.
        const detail =
          description && description.toLowerCase() !== label.toLowerCase() ? description : null
        return (
          <li key={`${character.id}-${type}`}>
            <Link to={`/characters/${character.id}`} className={styles.relationship}>
              <CharacterAvatar
                name={character.name}
                image={character.image}
                element={character.element}
                size="sm"
              />
              <span className={styles.relationshipText}>
                <span className={styles.relationshipName}>{character.name}</span>
                <span className={styles.muted}>
                  {label}
                  {detail && ` · ${detail}`}
                </span>
              </span>
            </Link>
          </li>
        )
      })}
    </ul>
  )
}
