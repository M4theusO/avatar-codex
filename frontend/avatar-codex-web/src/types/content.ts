// Espelham os DTOs de AvatarCodex.Application (ver docs/api.md).

export type CharacterStatus = 'Alive' | 'Deceased' | 'Unknown'

export type RelationshipType =
  'Family' | 'Friend' | 'Mentor' | 'Student' | 'Romantic' | 'Ally' | 'Rival' | 'Enemy'

export interface Image {
  url: string
  sourceUrl: string | null
  credit: string | null
}

export interface Reference {
  id: number
  name: string
}

export interface ElementSummary {
  id: number
  name: string
  color: string
  icon: string
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
}

export interface CharacterListItem {
  id: number
  name: string
  description: string
  image: Image | null
  nation: Reference | null
  element: ElementSummary | null
  isBender: boolean
  isAvatar: boolean
  status: CharacterStatus
}

export interface CharacterSummary {
  id: number
  name: string
  image: Image | null
  element: ElementSummary | null
}

export interface CharacterRelationship {
  character: CharacterSummary
  type: RelationshipType
  description: string | null
}

export interface Ability {
  id: number
  name: string
  description: string
  element: ElementSummary | null
  image: Image | null
}

export interface Series {
  id: number
  code: string
  name: string
}

export interface CharacterDetails extends Omit<CharacterListItem, 'description'> {
  description: string
  aliases: string[]
  biography: string
  firstAppearance: string | null
  bendingSubtypes: Reference[]
  abilities: Ability[]
  series: Series[]
  relationships: CharacterRelationship[]
}

export interface Nation {
  id: number
  name: string
  description: string
  symbolImage: Image | null
  element: ElementSummary | null
  characterCount: number
}

export interface NationDetails extends Omit<Nation, 'characterCount'> {
  characters: CharacterListItem[]
}

export interface BendingSubtype {
  id: number
  name: string
  description: string
}

export interface Element extends ElementSummary {
  description: string
  bendingSubtypes: BendingSubtype[]
}

export type CharacterType = 'avatar' | 'bender' | 'nonBender'
export type CharacterSort = 'name' | 'nation' | 'element'
export type SortOrder = 'asc' | 'desc'

/** Parâmetros de GET /api/characters. */
export interface CharacterQuery {
  page?: number
  pageSize?: number
  search?: string
  nationId?: number
  elementId?: number
  type?: CharacterType
  status?: CharacterStatus
  seriesId?: number
  sort?: CharacterSort
  order?: SortOrder
}
