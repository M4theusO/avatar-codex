import { CircleAlert, CircleCheck, Info, X } from 'lucide-react'
import { useCallback, useMemo, useRef, useState, type ReactNode } from 'react'
import { ToastContext, type ToastTone } from './toastContext'
import styles from './ToastProvider.module.css'

interface Toast {
  id: number
  message: string
  tone: ToastTone
}

const DURATION_MS = 4000
const icons = { success: CircleCheck, info: Info, error: CircleAlert }

export default function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([])
  const nextId = useRef(0)

  const dismiss = useCallback((id: number) => {
    setToasts((current) => current.filter((toast) => toast.id !== id))
  }, [])

  const showToast = useCallback(
    (message: string, tone: ToastTone = 'success') => {
      const id = ++nextId.current
      setToasts((current) => [...current, { id, message, tone }])
      setTimeout(() => dismiss(id), DURATION_MS)
    },
    [dismiss],
  )

  const value = useMemo(() => ({ showToast }), [showToast])

  return (
    <ToastContext value={value}>
      {children}
      {/* Região sempre presente para o leitor de tela anunciar novos toasts */}
      <div className={styles.region} aria-live="polite" aria-label="Notificações">
        {toasts.map(({ id, message, tone }) => {
          const Icon = icons[tone]
          return (
            <div key={id} className={`${styles.toast} ${styles[tone]}`}>
              <Icon size={18} className={styles.icon} aria-hidden="true" />
              <p className={styles.message}>{message}</p>
              <button
                type="button"
                className={styles.close}
                onClick={() => dismiss(id)}
                aria-label="Fechar notificação"
              >
                <X size={16} aria-hidden="true" />
              </button>
            </div>
          )
        })}
      </div>
    </ToastContext>
  )
}
