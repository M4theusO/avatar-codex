import type { ReactNode } from 'react'
import type { ElementSummary } from '../../types/content'
import { elementStyle } from '../../utils/labels'
import styles from './Badge.module.css'
import ElementIcon from './ElementIcon'

export function Badge({ children }: { children: ReactNode }) {
  return <span className={styles.badge}>{children}</span>
}

/** Cor do elemento só no fundo/borda/ícone; o texto fica neutro (DA-02). */
export function ElementBadge({ element }: { element: ElementSummary }) {
  return (
    <span className={`${styles.badge} ${styles.element}`} style={elementStyle(element.color)}>
      <ElementIcon icon={element.icon} size={14} className={styles.icon} />
      {element.name}
    </span>
  )
}
