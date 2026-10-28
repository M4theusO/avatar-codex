import { useId } from 'react'
import type { TimelineEra } from '../../types/content'
import TimelineEventCard from './TimelineEventCard'
import styles from './Timeline.module.css'

export default function TimelineEraSection({ era }: { era: TimelineEra }) {
  const titleId = useId()

  return (
    <section className={styles.era} aria-labelledby={titleId}>
      <header className={styles.eraHeader}>
        <h2 id={titleId} className={styles.eraTitle}>
          {era.name}
        </h2>
        <p className={styles.eraDescription}>{era.description}</p>
      </header>
      <ol className={styles.events}>
        {era.events.map((event) => (
          <TimelineEventCard key={event.id} event={event} />
        ))}
      </ol>
    </section>
  )
}
