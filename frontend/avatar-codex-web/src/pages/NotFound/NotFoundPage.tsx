import { Compass } from 'lucide-react'
import { ButtonLink } from '../../components/common/Button'
import { EmptyState } from '../../components/common/StatusMessage'

export default function NotFoundPage() {
  return (
    <>
      <title>Página não encontrada · Avatar Codex</title>
      <EmptyState
        icon={Compass}
        title="Página não encontrada"
        description="O caminho que você procurou não existe. Que tal voltar ao início?"
        action={<ButtonLink to="/">Voltar ao início</ButtonLink>}
      />
    </>
  )
}
