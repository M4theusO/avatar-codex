import { UserX } from 'lucide-react'
import { useParams } from 'react-router'
import AbilityList from '../../components/characters/AbilityList'
import CharacterHero from '../../components/characters/CharacterHero'
import RelationshipList from '../../components/characters/RelationshipList'
import BackLink from '../../components/common/BackLink'
import { ButtonLink } from '../../components/common/Button'
import Section, { ChipList } from '../../components/common/Section'
import Skeleton from '../../components/common/Skeleton'
import { EmptyState, ErrorState } from '../../components/common/StatusMessage'
import { useCharacter } from '../../hooks/useContent'
import type { CharacterDetails } from '../../types/content'
import { isNotFoundError } from '../../utils/apiError'
import styles from './CharacterDetailsPage.module.css'

export default function CharacterDetailsPage() {
  const characterId = Number(useParams().id)
  const { data, isPending, isError, error, refetch } = useCharacter(characterId)

  const invalidId = !Number.isInteger(characterId) || characterId <= 0

  return (
    <>
      <BackLink to="/characters" label="Todos os personagens" />
      {invalidId || isNotFoundError(error) ? (
        <EmptyState
          icon={UserX}
          title="Personagem não encontrado"
          description="Ele pode ter sido removido ou o endereço está incorreto."
          action={<ButtonLink to="/characters">Ver personagens</ButtonLink>}
        />
      ) : isPending ? (
        <DetailsSkeleton />
      ) : isError ? (
        <ErrorState onRetry={() => refetch()} />
      ) : (
        <CharacterContent character={data} />
      )}
    </>
  )
}

function CharacterContent({ character }: { character: CharacterDetails }) {
  const { name, description, biography, bendingSubtypes, abilities, series, relationships } =
    character

  return (
    <article className={styles.content}>
      <title>{`${name} · Avatar Codex`}</title>
      <CharacterHero character={character} />

      <Section title="Sobre">
        <p className={styles.lead}>{description}</p>
        {biography.split(/\n{2,}/).map((paragraph, index) => (
          <p key={index} className={styles.paragraph}>
            {paragraph}
          </p>
        ))}
      </Section>

      {bendingSubtypes.length > 0 && (
        <Section title="Subtipos de dobra">
          <ChipList items={bendingSubtypes} />
        </Section>
      )}

      {abilities.length > 0 && (
        <Section title="Habilidades">
          <AbilityList abilities={abilities} />
        </Section>
      )}

      {relationships.length > 0 && (
        <Section title="Relações">
          <RelationshipList relationships={relationships} />
        </Section>
      )}

      {series.length > 0 && (
        <Section title="Aparições">
          <ChipList items={series} />
        </Section>
      )}
    </article>
  )
}

function DetailsSkeleton() {
  return (
    <div className={styles.content} aria-busy="true">
      <span className="visually-hidden" role="status">
        Carregando personagem…
      </span>
      <div className={styles.skeletonHero} aria-hidden="true">
        <Skeleton width="8rem" height="8rem" round />
        <Skeleton width="14rem" height="2.5rem" />
        <Skeleton width="10rem" height="1.25rem" />
      </div>
      <div className={styles.skeletonText} aria-hidden="true">
        <Skeleton height="1.25rem" />
        <Skeleton height="1rem" />
        <Skeleton width="80%" height="1rem" />
      </div>
    </div>
  )
}
