import { useContext } from 'react'
import { LevelUpContext } from '../contexts/levelUpContext'

export function useLevelUp() {
  const context = useContext(LevelUpContext)
  if (!context) throw new Error('useLevelUp precisa estar dentro de <LevelUpProvider>.')
  return context
}
