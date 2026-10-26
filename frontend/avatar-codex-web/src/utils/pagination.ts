/** Páginas visíveis: primeira, última e vizinhas da atual; o resto vira reticências. */
export function getPageItems(page: number, totalPages: number): (number | 'gap')[] {
  const pages = new Set([1, totalPages, page - 1, page, page + 1])
  const sorted = [...pages].filter((p) => p >= 1 && p <= totalPages).sort((a, b) => a - b)

  const items: (number | 'gap')[] = []
  sorted.forEach((p, index) => {
    if (index > 0 && p - sorted[index - 1] > 1) items.push('gap')
    items.push(p)
  })
  return items
}
