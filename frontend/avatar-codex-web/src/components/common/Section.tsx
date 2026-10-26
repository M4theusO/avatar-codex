import type { ReactNode } from 'react'
import { useId } from 'react'
import styles from './Section.module.css'

interface SectionProps {
  title: string
  action?: ReactNode
  children: ReactNode
}

export default function Section({ title, action, children }: SectionProps) {
  const titleId = useId()
  return (
    <section className={styles.section} aria-labelledby={titleId}>
      <div className={styles.header}>
        <h2 id={titleId} className={styles.title}>
          {title}
        </h2>
        {action}
      </div>
      {children}
    </section>
  )
}

export function ChipList({ items }: { items: { id: number; name: string }[] }) {
  return (
    <ul className={styles.chips}>
      {items.map((item) => (
        <li key={item.id} className={styles.chip}>
          {item.name}
        </li>
      ))}
    </ul>
  )
}
