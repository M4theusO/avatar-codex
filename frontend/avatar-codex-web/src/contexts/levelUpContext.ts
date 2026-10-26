import { createContext } from 'react'

export interface LevelUpContextValue {
  /** Enfileira o modal de novo nível; vários eventos aparecem um de cada vez. */
  showLevelUp: (level: number) => void
}

export const LevelUpContext = createContext<LevelUpContextValue | null>(null)
