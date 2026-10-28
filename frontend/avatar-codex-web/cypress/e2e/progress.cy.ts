// Fluxos 6, 7, 8 e 9: fazer um quiz, ganhar XP, desbloquear uma conquista e consultar o progresso.
describe('Progresso', () => {
  beforeEach(() => {
    cy.newUser().then((user) => cy.registerViaApi(user))
  })

  it('faz um quiz: a correção vem do backend e a primeira conclusão dá XP', () => {
    cy.visit('/quizzes')
    cy.findAllByRole('link', { name: /^Jogar:/ })
      .first()
      .click()

    // Responde a primeira opção de cada pergunta (a correção é do backend).
    const answerAll = (): void => {
      cy.get('input[type=radio]').first().check({ force: true })
      cy.findByRole('button', { name: /Próxima|Enviar respostas/ }).then(($button) => {
        const submit = $button.text().includes('Enviar')
        cy.wrap($button).click()
        if (!submit) answerAll()
      })
    }
    answerAll()

    cy.contains(/\d+% de acertos/).should('be.visible')
    cy.contains(/\+\d+ XP/).should('be.visible')
    cy.dismissLevelUp()

    cy.visit('/quizzes/history')
    cy.findAllByRole('listitem').should('have.length.at.least', 1)
  })

  it('descobre todas as nações, desbloqueia "Historiador" e vê no perfil e nas conquistas', () => {
    cy.request(`${Cypress.expose('apiUrl')}/api/nations`).then(({ body }) => {
      const nations = body as { id: number; name: string }[]
      nations.forEach((nation, index) => {
        cy.visit(`/nations/${nation.id}`)
        cy.findByRole('heading', { level: 1, name: nation.name }).should('be.visible')
        cy.findByText(new RegExp(`Você descobriu ${nation.name}! \\+10 XP`)).should('be.visible')
        if (index === nations.length - 1) {
          cy.findByText(/Conquista desbloqueada — Historiador! \+50 XP/).should('be.visible')
        }
        cy.dismissLevelUp()
      })
    })

    cy.visit('/achievements')
    cy.findByRole('heading', { name: 'Historiador' })
      .closest('li')
      .should('contain.text', 'Desbloqueada')

    // 10 do cadastro + 5 nações × 10 + 50 da conquista = 110 XP no mínimo (desafios do dia podem somar mais).
    cy.visit('/profile')
    cy.findByText(/Nível \d+/).should('be.visible')
    cy.contains(/\d+ XP/)
      .invoke('text')
      .then((text) => {
        const xp = Number(text.match(/(\d+) XP/)![1])
        expect(xp).to.be.at.least(110)
      })
  })
})
