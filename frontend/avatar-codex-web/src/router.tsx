import type { ComponentType } from 'react'
import { createBrowserRouter, type RouteObject } from 'react-router'
import { GuestOnly, RequireAuth } from './components/auth/RouteGuards'
import { ErrorState } from './components/common/StatusMessage'
import AppLayout from './components/layout/AppLayout'
import HomePage from './pages/Home/HomePage'

// Home entra no bundle principal; as demais páginas são carregadas sob demanda.
function page(load: () => Promise<{ default: ComponentType }>): RouteObject['lazy'] {
  return async () => ({ Component: (await load()).default })
}

export const routes: RouteObject[] = [
  {
    element: <AppLayout />,
    errorElement: (
      <ErrorState
        title="Algo deu errado"
        description="Ocorreu um erro inesperado ao exibir esta página."
        onRetry={() => window.location.reload()}
      />
    ),
    children: [
      { index: true, element: <HomePage /> },
      { path: 'characters', lazy: page(() => import('./pages/Characters/CharactersPage')) },
      {
        path: 'characters/:id',
        lazy: page(() => import('./pages/CharacterDetails/CharacterDetailsPage')),
      },
      { path: 'nations', lazy: page(() => import('./pages/Nations/NationsPage')) },
      { path: 'nations/:id', lazy: page(() => import('./pages/NationDetails/NationDetailsPage')) },
      { path: 'timeline', lazy: page(() => import('./pages/Timeline/TimelinePage')) },
      { path: 'compare', lazy: page(() => import('./pages/Compare/ComparePage')) },
      { path: 'quizzes', lazy: page(() => import('./pages/Quizzes/QuizzesPage')) },
      {
        element: <GuestOnly />,
        children: [
          { path: 'login', lazy: page(() => import('./pages/Login/LoginPage')) },
          { path: 'register', lazy: page(() => import('./pages/Register/RegisterPage')) },
        ],
      },
      {
        element: <RequireAuth />,
        children: [
          {
            path: 'challenges',
            lazy: page(() => import('./pages/Challenges/ChallengesPage')),
          },
          { path: 'dashboard', lazy: page(() => import('./pages/Dashboard/DashboardPage')) },
          { path: 'profile', lazy: page(() => import('./pages/Profile/ProfilePage')) },
          { path: 'favorites', lazy: page(() => import('./pages/Favorites/FavoritesPage')) },
          {
            path: 'quizzes/history',
            lazy: page(() => import('./pages/QuizHistory/QuizHistoryPage')),
          },
          { path: 'quizzes/:id', lazy: page(() => import('./pages/QuizPlay/QuizPlayPage')) },
          {
            path: 'achievements',
            lazy: page(() => import('./pages/Achievements/AchievementsPage')),
          },
        ],
      },
      { path: '*', lazy: page(() => import('./pages/NotFound/NotFoundPage')) },
    ],
  },
]

export const router = createBrowserRouter(routes)
