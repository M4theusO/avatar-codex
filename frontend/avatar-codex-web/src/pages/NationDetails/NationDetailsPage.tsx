import { Landmark, Users } from 'lucide-react'
import { useParams } from 'react-router'
import { CharacterGrid, CharacterGridSkeleton } from '../../components/characters/CharacterGrid'
import BackLink from '../../components/common/BackLink'
import { ButtonLink } from '../../components/common/Button'
import DetailHero from '../../components/common/DetailHero'
import Section from '../../components/common/Section'
import Skeleton from '../../components/common/Skeleton'
import { EmptyState, ErrorState } from '../../components/common/StatusMessage'
import { useNation } from '../../hooks/useContent'
import { useDiscovery } from '../../hooks/useDiscovery'
import type { NationDetails } from '../../types/content'
import { isNotFoundError } from '../../utils/apiError'
import styles from './NationDetailsPage.module.css'

export default function NationDetailsPage() {
  const nationId = Number(useParams().id)
  const { data, isPending, isError, error, refetch } = useNation(nationId)
  // Só depois que o conteúdo foi carregado (data existe) — RN-12, DA-01.
  useDiscovery('nation', nationId, data?.name)

  const invalidId = !Number.isInteger(nationId) || nationId <= 0

  return (
    <>
      <BackLink to="/nations" label="Todas as nações" />
      {invalidId || isNotFoundError(error) ? (
        <EmptyState
          icon={Landmark}
          title="Nação não encontrada"
          description="O endereço pode estar incorreto."
          action={<ButtonLink to="/nations">Ver nações</ButtonLink>}
        />
      ) : isPending ? (
        <div className={styles.content}>
          <Skeleton height="12rem" />
          <CharacterGridSkeleton count={4} />
        </div>
      ) : isError ? (
        <ErrorState onRetry={() => refetch()} />
      ) : (
        <NationContent nation={data} />
      )}
    </>
  )
}

function NationContent({ nation }: { nation: NationDetails }) {
  const { name, description, symbolImage, element, characters } = nation

  return (
    <article className={styles.content}>
      <title>{`${name} · Avatar Codex`}</title>
      <DetailHero title={name} description={description} element={element} image={symbolImage} />

      <Section title="Personagens">
        {characters.length > 0 ? (
          <CharacterGrid characters={characters} />
        ) : (
          <EmptyState icon={Users} title="Nenhum personagem desta nação cadastrado ainda" />
        )}
      </Section>
    </article>
  )
}
