import type { AbilityListItem, ElementSummary } from '../types/content'

export interface AbilityGroup {
  /** Nulo para as habilidades que não dependem de dobra. */
  element: ElementSummary | null
  abilities: AbilityListItem[]
}

/**
 * Agrupa as habilidades por elemento, mantendo a ordem em que a API já as entrega
 * (elementos na ordem do catálogo, sem elemento por último). Só apresentação.
 */
export function groupAbilitiesByElement(abilities: AbilityListItem[]): AbilityGroup[] {
  const groups: AbilityGroup[] = []
  for (const ability of abilities) {
    const elementId = ability.element?.id ?? null
    let group = groups.find((g) => (g.element?.id ?? null) === elementId)
    if (!group) {
      group = { element: ability.element, abilities: [] }
      groups.push(group)
    }
    group.abilities.push(ability)
  }
  return groups
}

export function characterCountLabel(count: number): string {
  return `${count} ${count === 1 ? 'personagem' : 'personagens'}`
}
