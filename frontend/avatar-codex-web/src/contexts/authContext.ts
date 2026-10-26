import { createContext } from 'react'
import type { LoginData, RegisterData, User } from '../types/auth'

export type AuthStatus = 'loading' | 'authenticated' | 'anonymous'

export interface AuthContextValue {
  status: AuthStatus
  user: User | null
  login: (data: LoginData) => Promise<User>
  register: (data: RegisterData) => Promise<User>
  logout: () => Promise<void>
  /** Atualiza o usuário em memória com dados vindos da API (ex.: XP e nível de um `reward`). */
  updateUser: (changes: Partial<User>) => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)
