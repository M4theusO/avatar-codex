import { Hourglass } from 'lucide-react'
import { ButtonLink } from '../../components/common/Button'
import PageHeader from '../../components/common/PageHeader'
import { EmptyState } from '../../components/common/StatusMessage'

export default function TimelinePage() {
  return (
    <>
      <title>Linha do tempo · Avatar Codex</title>
      <PageHeader
        title="Linha do tempo"
        description="As eras do mundo de Avatar, dos primeiros Avatares à República Unida."
      />
      <EmptyState
        icon={Hourglass}
        title="Em preparação"
        description="A linha do tempo com eras e eventos está sendo preparada. Enquanto isso, explore os personagens."
        action={<ButtonLink to="/characters">Explorar personagens</ButtonLink>}
      />
    </>
  )
}
