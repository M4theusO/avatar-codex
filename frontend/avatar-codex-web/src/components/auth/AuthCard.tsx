import { CircleAlert } from 'lucide-react'
import type { FormEventHandler, ReactNode } from 'react'
import styles from './AuthCard.module.css'

interface AuthCardProps {
  title: string
  description: string
  /** Erro geral do formulário (credenciais inválidas, conflito, rede). */
  error?: string
  onSubmit: FormEventHandler<HTMLFormElement>
  children: ReactNode
  footer: ReactNode
}

export default function AuthCard({
  title,
  description,
  error,
  onSubmit,
  children,
  footer,
}: AuthCardProps) {
  return (
    <div className={styles.page}>
      <section className={styles.card} aria-labelledby="auth-title">
        <span className={styles.mark} aria-hidden="true" />
        <h1 id="auth-title" className={styles.title}>
          {title}
        </h1>
        <p className={styles.description}>{description}</p>

        {error && (
          <div className={styles.alert} role="alert">
            <CircleAlert size={18} aria-hidden="true" />
            {error}
          </div>
        )}

        <form className={styles.form} onSubmit={onSubmit} noValidate>
          {children}
        </form>
        <p className={styles.footer}>{footer}</p>
      </section>
    </div>
  )
}
