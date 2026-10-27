import type { CSSProperties } from 'react'
import type { CharacterStatus, CharacterType, RelationshipType } from '../types/content'
import type { QuizDifficulty } from '../types/quiz'

// Rótulos de UI para os enums da API (traduções, não regra de negócio).

export const statusLabels: Record<CharacterStatus, string> = {
  Alive: 'Vivo',
  Deceased: 'Falecido',
  Unknown: 'Desconhecido',
}

export const typeLabels: Record<CharacterType, string> = {
  avatar: 'Avatares',
  bender: 'Dobradores',
  nonBender: 'Não dobradores',
}

export const relationshipLabels: Record<RelationshipType, string> = {
  Family: 'Família',
  Friend: 'Amizade',
  Mentor: 'Mentor',
  Student: 'Aprendiz',
  Romantic: 'Romance',
  Ally: 'Aliado',
  Rival: 'Rival',
  Enemy: 'Inimigo',
}

export const difficultyLabels: Record<QuizDifficulty, string> = {
  Easy: 'Fácil',
  Medium: 'Médio',
  Hard: 'Difícil',
}

export function getInitials(name: string): string {
  const words = name.trim().split(/\s+/).filter(Boolean)
  if (words.length === 0) return '?'
  const first = words[0][0]
  const last = words.length > 1 ? words[words.length - 1][0] : ''
  return (first + last).toUpperCase()
}

export function elementStyle(color: string | null | undefined) {
  return color ? ({ '--element-color': color } as CSSProperties) : undefined
}
