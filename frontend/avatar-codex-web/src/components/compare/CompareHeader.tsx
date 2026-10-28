import { Fragment } from 'react'
import { Link } from 'react-router'
import type { ComparedCharacter } from '../../types/content'
import { elementStyle } from '../../utils/labels'
import CharacterAvatar from '../common/CharacterAvatar'
import styles from './Compare.module.css'

const seriesClass = [styles.seriesA, styles.seriesB]

/** Os dois personagens frente a frente; a faixa colorida é a legenda das cores dos gráficos. */
export default function CompareHeader({ characters }: { characters: ComparedCharacter[] }) {
  return (
    <div className={styles.versus}>
      {characters.map((character, index) => (
        <Fragment key={character.id}>
          {index > 0 && (
            <span className={styles.vs} aria-hidden="true">
              vs
            </span>
          )}
          <Link
            to={`/characters/${character.id}`}
            className={`${styles.contender} ${seriesClass[index]}`}
            style={elementStyle(character.element?.color)}
          >
            <CharacterAvatar
              name={character.name}
              image={character.image}
              element={character.element}
              size="lg"
            />
            <span className={styles.contenderName}>{character.name}</span>
            <span className={styles.contenderMeta}>
              {character.element?.name ?? 'Não dobrador'}
              {character.nation && ` · ${character.nation.name}`}
            </span>
            <span className={styles.legendMark} aria-hidden="true" />
          </Link>
        </Fragment>
      ))}
    </div>
  )
}
