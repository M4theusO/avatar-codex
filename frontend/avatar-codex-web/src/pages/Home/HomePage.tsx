import { ArrowRight } from 'lucide-react'
import { Link } from 'react-router'
import { CharacterGrid, CharacterGridSkeleton } from '../../components/characters/CharacterGrid'
import { ButtonLink } from '../../components/common/Button'
import ElementIcon from '../../components/common/ElementIcon'
import Section from '../../components/common/Section'
import Skeleton from '../../components/common/Skeleton'
import { ErrorState } from '../../components/common/StatusMessage'
import { useAuth } from '../../hooks/useAuth'
import { useCharacters, useElements } from '../../hooks/useContent'
import { elementStyle } from '../../utils/labels'
import styles from './HomePage.module.css'

const AVATARS_QUERY = { type: 'avatar', pageSize: 8 } as const

export default function HomePage() {
  return (
    <div className={styles.page}>
      <title>Avatar Codex</title>
      <section className={styles.hero} aria-labelledby="hero-title">
        <div className={styles.heroGlow} aria-hidden="true" />
        <p className={styles.eyebrow}>Enciclopédia interativa</p>
        <h1 id="hero-title" className={styles.title}>
          Avatar Codex
        </h1>
        <p className={styles.tagline}>
          Explore personagens, nações e dobras do universo de Avatar: A Lenda de Aang e A Lenda de
          Korra.
        </p>
        <div className={styles.actions}>
          <ButtonLink to="/characters" variant="primary">
            Explorar personagens
            <ArrowRight size={18} aria-hidden="true" />
          </ButtonLink>
          <ButtonLink to="/nations">Conhecer as nações</ButtonLink>
        </div>
      </section>

      <AvatarsSection />
      <ElementsSection />
      <JoinSection />
    </div>
  )
}

/** Chamada para cadastro, só para quem ainda não entrou. */
function JoinSection() {
  const { status } = useAuth()
  if (status !== 'anonymous') return null

  return (
    <section className={styles.join} aria-labelledby="join-title">
      <h2 id="join-title" className={styles.joinTitle}>
        Comece sua jornada
      </h2>
      <p className={styles.joinText}>
        Crie uma conta para favoritar personagens, ganhar XP ao explorar e acompanhar sua evolução.
      </p>
      <ButtonLink to="/register" variant="primary">
        Criar conta grátis
      </ButtonLink>
    </section>
  )
}

function AvatarsSection() {
  const { data, isPending, isError, refetch } = useCharacters(AVATARS_QUERY)

  return (
    <Section
      title="Os Avatares"
      action={
        <Link to="/characters?type=avatar" className={styles.seeAll}>
          Ver todos <ArrowRight size={16} aria-hidden="true" />
        </Link>
      }
    >
      {isPending ? (
        <CharacterGridSkeleton count={4} />
      ) : isError ? (
        <ErrorState onRetry={() => refetch()} />
      ) : (
        <CharacterGrid characters={data.items} />
      )}
    </Section>
  )
}

function ElementsSection() {
  const { data, isPending, isError, refetch } = useElements()

  return (
    <Section title="Os quatro elementos">
      {isPending ? (
        <div className={styles.elements} aria-busy="true">
          {Array.from({ length: 4 }, (_, index) => (
            <Skeleton key={index} height="12rem" />
          ))}
        </div>
      ) : isError ? (
        <ErrorState onRetry={() => refetch()} />
      ) : (
        <ul className={styles.elements}>
          {data.map((element) => (
            <li key={element.id} className={styles.element} style={elementStyle(element.color)}>
              <span className={styles.elementIcon}>
                <ElementIcon icon={element.icon} size={24} />
              </span>
              <h3 className={styles.elementName}>{element.name}</h3>
              <p className={styles.elementDescription}>{element.description}</p>
              <Link to={`/characters?elementId=${element.id}`} className={styles.seeAll}>
                Ver dobradores <ArrowRight size={16} aria-hidden="true" />
              </Link>
            </li>
          ))}
        </ul>
      )}
    </Section>
  )
}
