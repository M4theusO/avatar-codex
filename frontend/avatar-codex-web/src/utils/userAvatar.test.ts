import { describe, expect, it } from 'vitest'
import { checkAvatarFile, userAvatarImage } from './userAvatar'

describe('userAvatar', () => {
  it('monta a imagem a partir da URL relativa da API', () => {
    expect(userAvatarImage(null)).toBeNull()
    expect(userAvatarImage('/api/avatars/abc')?.url).toMatch(/\/api\/avatars\/abc$/)
  })

  it('aceita JPEG, PNG e WebP e recusa outros tipos ou arquivos enormes', () => {
    const file = (type: string, size = 10) => new File([new Uint8Array(size)], 'x', { type })
    expect(checkAvatarFile(file('image/png'))).toBeNull()
    expect(checkAvatarFile(file('image/webp'))).toBeNull()
    expect(checkAvatarFile(file('image/gif'))).toBe('Escolha uma imagem JPEG, PNG ou WebP.')
    expect(checkAvatarFile(file('image/jpeg', 16 * 1024 * 1024))).toBe(
      'A imagem deve ter no máximo 15 MB.',
    )
  })
})
