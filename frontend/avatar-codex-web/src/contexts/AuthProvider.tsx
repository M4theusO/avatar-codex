import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { setSessionExpiredHandler } from '../services/api'
import * as authService from '../services/authService'
import type { LoginData, RegisterData, SessionResult, User } from '../types/auth'
import { AuthContext, type AuthContextValue, type AuthStatus } from './authContext'

export default function AuthProvider({ children }: { children: ReactNode }) {
  const [status, setStatus] = useState<AuthStatus>('loading')
  const [user, setUser] = useState<User | null>(null)

  const signIn = useCallback((signedUser: User) => {
    setUser(signedUser)
    setStatus('authenticated')
  }, [])

  const startSession = useCallback(
    (result: SessionResult) => {
      signIn(result.user)
      return result
    },
    [signIn],
  )

  const signOut = useCallback(() => {
    setUser(null)
    setStatus('anonymous')
  }, [])

  useEffect(() => {
    let active = true
    setSessionExpiredHandler(signOut)

    authService
      .restoreSession()
      .then((restored) => {
        if (active) signIn(restored)
      })
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
      login: async (data: LoginData) => startSession(await authService.login(data)),
      register: async (data: RegisterData) => startSession(await authService.register(data)),
      logout: async () => {
        try {
          await authService.logout()
        } finally {
          signOut()
        }
      },
      updateUser,
    }),
    [status, user, signOut, startSession, updateUser],
  )

  return <AuthContext value={value}>{children}</AuthContext>
}
