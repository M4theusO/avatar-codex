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

  it('descobre o elemento no teste e ele vai para o perfil', () => {
    cy.visit('/element-test')
    cy.findByRole('button', { name: /Começar/ }).click()

    // Escolhe sempre a primeira opção; o elemento é calculado no backend.
    for (let question = 1; question <= 8; question++) {
      cy.findByText(`Pergunta ${question} de 8`).should('be.visible')
      cy.findAllByRole('radio').first().check({ force: true })
      cy.findByRole('button', { name: question < 8 ? /Próxima/ : /Ver meu elemento/ }).click()
    }

    cy.findByText('Seu elemento é').should('be.visible')
    cy.findByText(/Elemento descoberto — .+! \+30 XP/).should('be.visible')
    cy.dismissLevelUp()
    cy.findByRole('heading', { level: 2 })
      .invoke('text')
      .then((element) => {
        cy.findByRole('link', { name: /Ver meu perfil/ }).click()
        cy.location('pathname').should('eq', '/profile')
        cy.findByRole('link', { name: 'Refazer o teste' }).should('be.visible')
        cy.get('dd').contains(element).should('be.visible')
      })
  })

  it('vê a própria posição no ranking geral e no do mês', () => {
    cy.visit('/dashboard')
    cy.findByRole('link', { name: 'Ranking' }).click()
    cy.location('pathname').should('eq', '/ranking')

    // O número fica num <strong>: cy.contains lê o texto completo do parágrafo.
    cy.contains('p', /Sua posição: \d+º de \d+/).should('be.visible')
    cy.findByRole('list', { name: 'Ranking' }).within(() => {
      cy.findByText('Você').should('be.visible')
    })

    cy.findByRole('button', { name: 'Este mês' }).click()
    cy.location('search').should('eq', '?period=month')
    cy.findByText(/recomeça em/).should('be.visible')
    // O cadastro rendeu o "Primeiro passo" neste mês.
    cy.contains('p', /Sua posição: \d+º de \d+ · 10 XP/).should('be.visible')
  })
})
