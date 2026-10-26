import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { setSessionExpiredHandler } from '../services/api'
import * as authService from '../services/authService'
import type { LoginData, RegisterData, User } from '../types/auth'
import { AuthContext, type AuthContextValue, type AuthStatus } from './authContext'

export default function AuthProvider({ children }: { children: ReactNode }) {
  const [status, setStatus] = useState<AuthStatus>('loading')
  const [user, setUser] = useState<User | null>(null)

  const signIn = useCallback((signedUser: User) => {
    setUser(signedUser)
    setStatus('authenticated')
    return signedUser
  }, [])

  const signOut = useCallback(() => {
    setUser(null)
    setStatus('anonymous')
  }, [])

  useEffect(() => {
    let active = true
    setSessionExpiredHandler(signOut)

    authService
      .restoreSession()
      .then((restored) => active && signIn(restored))
      .catch(() => active && signOut())

    return () => {
      active = false
      setSessionExpiredHandler(null)
    }
  }, [signIn, signOut])

  const updateUser = useCallback((changes: Partial<User>) => {
    setUser((current) => (current ? { ...current, ...changes } : current))
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      status,
      user,
      login: async (data: LoginData) => signIn(await authService.login(data)),
      register: async (data: RegisterData) => signIn(await authService.register(data)),
      logout: async () => {
        try {
          await authService.logout()
        } finally {
          signOut()
        }
      },
      updateUser,
    }),
    [status, user, signIn, signOut, updateUser],
  )

  return <AuthContext value={value}>{children}</AuthContext>
}
