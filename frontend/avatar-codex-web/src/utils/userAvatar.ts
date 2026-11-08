import type { Image } from '../types/content'

/** Lado da foto enviada: o backend aceita até 512 KB, e 256×256 em JPEG fica bem abaixo. */
export const AVATAR_SIZE = 256

/** Arquivo original aceito antes de reduzir (só para não decodificar arquivos enormes no navegador). */
export const MAX_ORIGINAL_BYTES = 15 * 1024 * 1024

export const ACCEPTED_TYPES = ['image/jpeg', 'image/png', 'image/webp']

/**
 * Imagem da foto de perfil para o CharacterAvatar. A URL vem relativa da API (`/api/avatars/...`):
 * em produção a Vercel repassa para a API; em dev ela fica em outra porta (VITE_API_URL).
 */
export function userAvatarImage(avatarUrl: string | null | undefined): Image | null {
  if (!avatarUrl) return null
  return { url: `${import.meta.env.VITE_API_URL ?? ''}${avatarUrl}`, sourceUrl: null, credit: null }
}

/** Mensagem de erro para um arquivo que nem vale a pena reduzir, ou null se ele pode seguir. */
export function checkAvatarFile(file: File): string | null {
  if (!ACCEPTED_TYPES.includes(file.type)) return 'Escolha uma imagem JPEG, PNG ou WebP.'
  if (file.size > MAX_ORIGINAL_BYTES) return 'A imagem deve ter no máximo 15 MB.'
  return null
}

/**
 * Recorta o centro da imagem num quadrado e reduz para 256×256 em JPEG, antes do envio:
 * a foto fica leve no banco e igual em qualquer tela. O backend confere tipo e tamanho de novo.
 */
export async function resizeAvatar(file: File): Promise<Blob> {
  const bitmap = await createImageBitmap(file)
  try {
    const side = Math.min(bitmap.width, bitmap.height)
    const canvas = document.createElement('canvas')
    canvas.width = AVATAR_SIZE
    canvas.height = AVATAR_SIZE
    const context = canvas.getContext('2d')
    if (!context) throw new Error('Canvas indisponível')
    // Fundo para PNGs com transparência (o JPEG não tem canal alfa).
    context.fillStyle = '#1b2835'
    context.fillRect(0, 0, AVATAR_SIZE, AVATAR_SIZE)
    context.drawImage(
      bitmap,
      (bitmap.width - side) / 2,
      (bitmap.height - side) / 2,
      side,
      side,
      0,
      0,
      AVATAR_SIZE,
      AVATAR_SIZE,
    )
    return await new Promise<Blob>((resolve, reject) =>
      canvas.toBlob(
        (blob) => (blob ? resolve(blob) : reject(new Error('Falha ao gerar a imagem'))),
        'image/jpeg',
        0.85,
      ),
    )
  } finally {
    bitmap.close()
  }
}
