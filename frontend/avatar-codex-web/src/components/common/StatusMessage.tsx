import { CircleAlert, type LucideIcon } from 'lucide-react'
import type { ReactNode } from 'react'
import { Button } from './Button'
import styles from './StatusMessage.module.css'

interface EmptyStateProps {
  icon: LucideIcon
  title: string
  description?: string
  action?: ReactNode
}

export function EmptyState({ icon: Icon, title, description, action }: EmptyStateProps) {
  return (
    <div className={styles.box}>
      <Icon size={40} className={styles.icon} aria-hidden="true" />
      <h2 className={styles.title}>{title}</h2>
      {description && <p className={styles.description}>{description}</p>}
      {action}
    </div>
  )
}

interface ErrorStateProps {
  title?: string
  description?: string
  onRetry?: () => void
}

/** Mensagem amigável; nunca mostra detalhes técnicos do erro. */
export function ErrorState({
  title = 'Não foi possível carregar o conteúdo',
  description = 'Verifique sua conexão e tente novamente.',
  onRetry,
}: ErrorStateProps) {
  return (
    <div className={styles.box} role="alert">
      <CircleAlert size={40} className={styles.errorIcon} aria-hidden="true" />
      <h2 className={styles.title}>{title}</h2>
      <p className={styles.description}>{description}</p>
      {onRetry && <Button onClick={onRetry}>Tentar novamente</Button>}
    </div>
  )
}
