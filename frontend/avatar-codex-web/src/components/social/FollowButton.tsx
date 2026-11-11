import { UserCheck, UserPlus } from 'lucide-react'
import { useToggleFollow } from '../../hooks/useFollow'
import { Button } from '../common/Button'

interface FollowButtonProps {
  username: string
  isFollowing: boolean
}

/** "Seguir" / "Seguindo" no perfil público. A regra (privado, a si mesmo, conta demo) fica no backend. */
export default function FollowButton({ username, isFollowing }: FollowButtonProps) {
  const toggle = useToggleFollow(username)

  return (
    <Button
      variant={isFollowing ? 'secondary' : 'primary'}
      aria-pressed={isFollowing}
      onClick={() => toggle.mutate(!isFollowing)}
      disabled={toggle.isPending}
    >
      {isFollowing ? (
        <UserCheck size={18} aria-hidden="true" />
      ) : (
        <UserPlus size={18} aria-hidden="true" />
      )}
      {isFollowing ? 'Seguindo' : 'Seguir'}
    </Button>
  )
}
