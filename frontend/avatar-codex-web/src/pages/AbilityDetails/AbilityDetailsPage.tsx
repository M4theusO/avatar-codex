import { Sparkles, Users } from 'lucide-react'
import { useParams } from 'react-router'
import { CharacterGrid, CharacterGridSkeleton } from '../../components/characters/CharacterGrid'
import BackLink from '../../components/common/BackLink'
import { Badge, ElementBadge } from '../../components/common/Badge'
import { ButtonLink } from '../../components/common/Button'
import DetailHero from '../../components/common/DetailHero'
import Section from '../../components/common/Section'
import Skeleton from '../../components/common/Skeleton'
import { EmptyState, ErrorState } from '../../components/common/StatusMessage'
import { useAbility } from '../../hooks/useContent'
import type { AbilityDetails } from '../../types/content'
import { characterCountLabel } from '../../utils/abilities'
import { isNotFoundError } from '../../utils/apiError'
import styles from './AbilityDetailsPage.module.css'

export default function AbilityDetailsPage() {
  const abilityId = Number(useParams().id)
  const { data, isPending, isError, error, refetch } = useAbility(abilityId)

  const invalidId = !Number.isInteger(abilityId) || abilityId <= 0

  return (
    <>
      <BackLink to="/abilities" label="Todas as habilidades" />
      {invalidId || isNotFoundError(error) ? (
        <EmptyState
          icon={Sparkles}
          title="Habilidade não encontrada"
          description="O endereço pode estar incorreto."
          action={<ButtonLink to="/abilities">Ver habilidades</ButtonLink>}
        />
      ) : isPending ? (
        <div className={styles.content}>
          <Skeleton height="12rem" />
          <CharacterGridSkeleton count={4} />
        </div>
      ) : isError ? (
        <ErrorState onRetry={() => refetch()} />
      ) : (
        <AbilityContent ability={data} />
      )}
    </>
  )
}

function AbilityContent({ ability }: { ability: AbilityDetails }) {
  const { name, description, image, element, characters } = ability

  return (
    <article className={styles.content}>
      <title>{`${name} · Avatar Codex`}</title>
      <DetailHero title={name} description={description} element={element} image={image}>
        {element ? <ElementBadge element={element} /> : <Badge>Sem elemento</Badge>}
      </DetailHero>

      <Section title={`Quem domina (${characterCountLabel(characters.length)})`}>
        {characters.length > 0 ? (
          <CharacterGrid characters={characters} />
        ) : (
          <EmptyState icon={Users} title="Nenhum personagem com esta habilidade cadastrado ainda" />
        )}
      </Section>
    </article>
  )
}
