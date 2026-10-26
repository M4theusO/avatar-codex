import { useCallback, useMemo, useState, type ReactNode } from 'react'
import LevelUpModal from '../components/progress/LevelUpModal'
import { LevelUpContext } from './levelUpContext'

export default function LevelUpProvider({ children }: { children: ReactNode }) {
  const [queue, setQueue] = useState<number[]>([])

  const showLevelUp = useCallback((level: number) => {
    setQueue((current) => [...current, level])
  }, [])

  const dismiss = useCallback(() => setQueue((current) => current.slice(1)), [])

  const value = useMemo(() => ({ showLevelUp }), [showLevelUp])

  return (
    <LevelUpContext value={value}>
      {children}
      <LevelUpModal level={queue[0]} onClose={dismiss} />
    </LevelUpContext>
  )
}
