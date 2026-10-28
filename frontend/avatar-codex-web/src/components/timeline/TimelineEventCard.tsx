import { Link } from 'react-router'
import { useReveal } from '../../hooks/useReveal'
import type { TimelineEvent } from '../../types/content'
import { elementStyle } from '../../utils/labels'
import CharacterAvatar from '../common/CharacterAvatar'
import styles from './Timeline.module.css'

/** Um acontecimento: marcador na linha, data, título, descrição e personagens relacionados. */
export default function TimelineEventCard({ event }: { event: TimelineEvent }) {
  const { ref, visible } = useReveal<HTMLLIElement>()

  return (
    <li ref={ref} className={`${styles.event} ${visible ? styles.visible : ''}`}>
      <span className={styles.marker} aria-hidden="true" />
      <article className={styles.card}>
        <p className={styles.date}>{event.dateLabel}</p>
        <h3 className={styles.eventTitle}>{event.title}</h3>
        <p className={styles.description}>{event.description}</p>
        {event.characters.length > 0 && (
          <ul className={styles.characters} aria-label={`Personagens em ${event.title}`}>
            {event.characters.map((character) => (
              <li key={character.id}>
                <Link
                  to={`/characters/${character.id}`}
                  className={styles.character}
                  style={elementStyle(character.element?.color)}
                >
                  <CharacterAvatar
                    name={character.name}
                    image={character.image}
                    element={character.element}
                    size="sm"
                  />
                  <span>{character.name}</span>
                </Link>
              </li>
            ))}
          </ul>
        )}
      </article>
    </li>
  )
}
