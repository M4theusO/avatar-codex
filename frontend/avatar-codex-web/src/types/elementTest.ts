import type { Element, ElementSummary } from './content'
import type { Reward } from './progress'

/** Opções sem o elemento: quem decide o resultado é o backend. */
export interface ElementTestQuestion {
  id: number
  text: string
  options: { id: number; text: string }[]
}

export interface ElementTest {
  questions: ElementTestQuestion[]
  experienceReward: number
}

export interface ElementTestAnswer {
  questionId: number
  optionId: number
}

export interface ElementScore {
  element: ElementSummary
  points: number
}

export interface ElementTestResult {
  element: Element
  scores: ElementScore[]
  /** XP do próprio teste: o valor da recompensa na primeira vez, 0 ao refazer. */
  experienceEarned: number
  /** Só na primeira conclusão (+XP); ao refazer vem null. */
  reward: Reward | null
}
