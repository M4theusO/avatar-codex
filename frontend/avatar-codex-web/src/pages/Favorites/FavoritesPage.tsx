import { Star } from 'lucide-react'
import { useSearchParams } from 'react-router'
import { CharacterGrid, CharacterGridSkeleton } from '../../components/characters/CharacterGrid'
import { ButtonLink } from '../../components/common/Button'
import PageHeader from '../../components/common/PageHeader'
import Pagination from '../../components/common/Pagination'
import { EmptyState, ErrorState } from '../../components/common/StatusMessage'
import { useFavorites } from '../../hooks/useUserData'
import styles from './FavoritesPage.module.css'

export default function FavoritesPage() {
  const [params, setParams] = useSearchParams()
  const page = Math.max(1, Number(params.get('page')) || 1)
  const { data, isPending, isError, refetch } = useFavorites(page)

  function handlePageChange(next: number) {
    setParams(next > 1 ? { page: String(next) } : {})
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  return (
    <>
      <title>Favoritos · Avatar Codex</title>
      <PageHeader title="Favoritos" description="Os personagens que você marcou com uma estrela." />

      {isPending ? (
        <CharacterGridSkeleton count={4} />
      ) : isError ? (
        <ErrorState onRetry={() => refetch()} />
      ) : data.items.length === 0 ? (
        <EmptyState
          icon={Star}
          title="Você ainda não possui favoritos"
          description="Explore os personagens e adicione seus favoritos."
          action={<ButtonLink to="/characters">Explorar personagens</ButtonLink>}
        />
      ) : (
        <div className={styles.content}>
          <CharacterGrid characters={data.items} />
          <Pagination
            page={data.page}
            totalPages={data.totalPages}
            onPageChange={handlePageChange}
          />
        </div>
      )}
    </>
  )
}
