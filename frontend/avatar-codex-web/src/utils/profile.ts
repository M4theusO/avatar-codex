const memberSinceFormat = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'long' })

/** Endereço do perfil público de um usuário. */
export function profilePath(username: string) {
  return `/users/${encodeURIComponent(username)}`
}

/** "26 de outubro de 2026" — data de cadastro no cabeçalho do perfil. */
export function formatMemberSince(createdAt: string) {
  return memberSinceFormat.format(new Date(createdAt))
}
