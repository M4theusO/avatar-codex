import { createContext } from 'react'

/** `reward`: XP, descoberta, nível (gamificação). */
export type ToastTone = 'success' | 'info' | 'error' | 'reward'

export interface ToastContextValue {
  showToast: (message: string, tone?: ToastTone) => void
}

export const ToastContext = createContext<ToastContextValue | null>(null)
