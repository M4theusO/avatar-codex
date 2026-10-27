import { createContext } from 'react'

/** `reward`: XP, descoberta, nível (gamificação). `achievement`: conquista desbloqueada. */
export type ToastTone = 'success' | 'info' | 'error' | 'reward' | 'achievement'

export interface ToastContextValue {
  showToast: (message: string, tone?: ToastTone) => void
}

export const ToastContext = createContext<ToastContextValue | null>(null)
