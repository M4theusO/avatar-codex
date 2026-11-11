import type { TestUser } from '../support/commands'

const apiUrl = () => Cypress.expose('apiUrl') as string

/** Troca a sessão do navegador: o login pela API grava o cookie de refresh do outro usuário. */
function switchTo(user: TestUser) {
  cy.clearAllCookies()
  cy.request('POST', `${apiUrl()}/api/auth/login`, { email: user.email, password: user.password })
}

// Fase 23: perfil público e privacidade, com dois usuários.
describe('Perfil público', () => {
  it('outro usuário vê o perfil; depois de ficar privado, só o dono vê', () => {
    cy.newUser().then((owner) => {
      cy.newUser().then((viewer) => {
        cy.registerViaApi(owner)
        cy.clearAllCookies()
        cy.registerViaApi(viewer)

        // Público: quem está logado vê nível, estatísticas e conquistas, sem atalho de edição.
        cy.visit(`/users/${owner.username}`)
        cy.findByRole('heading', { level: 1, name: owner.username }).should('be.visible')
        cy.findByText('Nível 1').should('be.visible')
        cy.findByRole('heading', { name: 'Conquistas (1)' }).should('be.visible')
        cy.findByRole('heading', { level: 3, name: 'Primeiro passo' }).should('be.visible')
        cy.findByRole('link', { name: /Editar perfil/ }).should('not.exist')
        cy.contains(owner.email).should('not.exist')

        // O dono torna o perfil privado pela seção de edição.
        switchTo(owner)
        cy.visit('/profile')
        cy.findByRole('switch', { name: 'Perfil privado' }).should('not.be.checked').click()
        cy.findByText('Seu perfil agora é privado.').should('be.visible')
        cy.findByRole('switch', { name: 'Perfil privado' }).should('be.checked')
        cy.findByRole('link', { name: 'Ver meu perfil público' }).click()
        cy.location('pathname').should('eq', `/users/${owner.username}`)
        cy.findByText(/só você vê esta página/).should('be.visible')
        cy.findByRole('link', { name: /Editar perfil/ }).should('have.attr', 'href', '/profile')

        // Para os outros, privado é igual a inexistente.
        switchTo(viewer)
        cy.visit(`/users/${owner.username}`)
        cy.findByText('Perfil não encontrado').should('be.visible')
        cy.findByRole('heading', { level: 1, name: owner.username }).should('not.exist')
      })
    })
  })

  // Fase 24: seguir, ranking "Seguindo" e conexões no perfil.
  it('segue outro usuário, compara no ranking "Seguindo" e deixa de seguir', () => {
    cy.newUser().then((star) => {
      cy.newUser().then((fan) => {
        cy.registerViaApi(star)
        cy.clearAllCookies()
        cy.registerViaApi(fan)

        // O botão muda na hora (otimista): espera a API confirmar antes de trocar de página.
        cy.intercept('PUT', '**/api/users/*/follow').as('follow')
        cy.visit(`/users/${star.username}`)
        cy.findByRole('button', { name: 'Seguir' }).click()
        cy.findByRole('button', { name: 'Seguindo' }).should('have.attr', 'aria-pressed', 'true')
        cy.wait('@follow').its('response.statusCode').should('eq', 204)
        cy.contains('dt', 'Seguidores').next().should('have.text', '1')

        // Ranking só com você e quem você segue.
        cy.visit('/ranking')
        cy.findByRole('button', { name: 'Seguindo' }).click()
        cy.location('search').should('eq', '?following=1')
        cy.findByRole('list', { name: 'Ranking' }).within(() => {
          cy.findAllByRole('listitem').should('have.length', 2)
          cy.findByRole('link', { name: star.username }).should('be.visible')
          cy.findByText('Você').should('be.visible')
        })

        // Conexões no próprio perfil, com link para o perfil seguido.
        cy.visit('/profile')
        cy.findByRole('list', { name: 'Seguindo' })
          .findByRole('link', { name: star.username })
          .click()
        cy.location('pathname').should('eq', `/users/${star.username}`)
        cy.findByRole('button', { name: 'Seguindo' }).click()
        cy.findByRole('button', { name: 'Seguir' }).should('have.attr', 'aria-pressed', 'false')
        cy.contains('dt', 'Seguidores').next().should('have.text', '0')
      })
    })
  })
})
