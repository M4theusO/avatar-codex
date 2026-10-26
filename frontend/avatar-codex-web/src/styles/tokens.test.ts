import { describe, expect, it } from 'vitest'
import { contrastRatio } from '../utils/contrast'
import tokensCss from './tokens.css?raw'

// Valida as regras de contraste da docs/ui-ux.md §2.3 contra os valores reais de tokens.css.

function readTokens(css: string): Record<string, string> {
  const tokens: Record<string, string> = {}
  for (const [, name, value] of css.matchAll(/--([\w-]+):\s*(#[0-9a-fA-F]{6})\s*;/g)) {
    tokens[name] = value
  }
  return tokens
}

const tokens = readTokens(tokensCss)
const backgrounds = ['color-bg', 'color-surface']

describe('tokens de cor', () => {
  it('mantém os valores aprovados na DA-02', () => {
    expect(tokens).toMatchObject({
      'color-bg': '#101820',
      'color-surface': '#1b2835',
      'color-water': '#3186b5',
      'color-fire': '#b86b35',
      'color-earth': '#638b58',
      'color-air': '#d5c7a2',
    })
  })

  const smallText = [
    'color-text',
    'color-text-muted',
    'color-air',
    'color-success',
    'color-danger',
    'color-warning',
  ]

  it.each(smallText.flatMap((fg) => backgrounds.map((bg) => [fg, bg])))(
    '%s sobre %s atinge 4,5:1 (texto pequeno)',
    (foreground, background) => {
      expect(contrastRatio(tokens[foreground], tokens[background])).toBeGreaterThanOrEqual(4.5)
    },
  )

  const accents = ['color-water', 'color-fire', 'color-earth']

  it.each(accents.flatMap((fg) => backgrounds.map((bg) => [fg, bg])))(
    '%s sobre %s atinge 3:1 (ícones, bordas, texto grande)',
    (foreground, background) => {
      expect(contrastRatio(tokens[foreground], tokens[background])).toBeGreaterThanOrEqual(3)
    },
  )

  it('botão primário (bg sobre text) atinge 4,5:1', () => {
    expect(contrastRatio(tokens['color-bg'], tokens['color-text'])).toBeGreaterThanOrEqual(4.5)
  })
})
