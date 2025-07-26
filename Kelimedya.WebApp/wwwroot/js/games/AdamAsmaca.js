import { fetchLearnedWords, awardScore, fetchWordCardWithQuestions } from "./common.js"

let word = "",
    guessed = new Set(),
    wrong = 0,
    startTime,
    cards = [],
    idx = 0
let singleMode = false

const TURKISH_ALPHABET = [
  "A",
  "B",
  "C",
  "Ç",
  "D",
  "E",
  "F",
  "G",
  "Ğ",
  "H",
  "I",
  "İ",
  "J",
  "K",
  "L",
  "M",
  "N",
  "O",
  "Ö",
  "P",
  "R",
  "S",
  "Ş",
  "T",
  "U",
  "Ü",
  "V",
  "Y",
  "Z",
]

function createLetterButtons() {
  const container = document.getElementById("letterButtons")
  if (!container) return

  container.innerHTML = ""

  TURKISH_ALPHABET.forEach((letter) => {
    const btn = document.createElement("button")
    btn.className = "letter-btn"
    btn.textContent = letter
    btn.onclick = () =>
        handleGuess(
            letter.toLowerCase(),
            document.getElementById("gameRoot").dataset.studentId,
            document.getElementById("gameRoot").dataset.gameId,
            btn,
        )
    container.appendChild(btn)
  })
}

function drawHangman() {
  const svg = document.getElementById("hangmanSvg")
  if (!svg) return

  svg.innerHTML = ""

  // Base
  if (wrong >= 0) {
    const base = document.createElementNS("http://www.w3.org/2000/svg", "line")
    base.setAttribute("x1", "10")
    base.setAttribute("y1", svg.getAttribute("height") - 10)
    base.setAttribute("x2", "150")
    base.setAttribute("y2", svg.getAttribute("height") - 10)
    base.setAttribute("stroke", "#ea580c")
    base.setAttribute("stroke-width", "3")
    svg.appendChild(base)
  }

  // Pole
  if (wrong >= 0) {
    const pole = document.createElementNS("http://www.w3.org/2000/svg", "line")
    pole.setAttribute("x1", "40")
    pole.setAttribute("y1", svg.getAttribute("height") - 10)
    pole.setAttribute("x2", "40")
    pole.setAttribute("y2", "20")
    pole.setAttribute("stroke", "#ea580c")
    pole.setAttribute("stroke-width", "3")
    svg.appendChild(pole)
  }

  // Top beam
  if (wrong >= 0) {
    const beam = document.createElementNS("http://www.w3.org/2000/svg", "line")
    beam.setAttribute("x1", "40")
    beam.setAttribute("y1", "20")
    beam.setAttribute("x2", "120")
    beam.setAttribute("y2", "20")
    beam.setAttribute("stroke", "#ea580c")
    beam.setAttribute("stroke-width", "3")
    svg.appendChild(beam)
  }

  // Noose
  if (wrong >= 0) {
    const noose = document.createElementNS("http://www.w3.org/2000/svg", "line")
    noose.setAttribute("x1", "120")
    noose.setAttribute("y1", "20")
    noose.setAttribute("x2", "120")
    noose.setAttribute("y2", "40")
    noose.setAttribute("stroke", "#ea580c")
    noose.setAttribute("stroke-width", "3")
    svg.appendChild(noose)
  }

  // Head
  if (wrong > 0) {
    const head = document.createElementNS("http://www.w3.org/2000/svg", "circle")
    head.setAttribute("cx", "120")
    head.setAttribute("cy", "60")
    head.setAttribute("r", "20")
    head.setAttribute("stroke", "#dc2626")
    head.setAttribute("stroke-width", "3")
    head.setAttribute("fill", "none")
    svg.appendChild(head)
  }

  // Body
  if (wrong > 1) {
    const body = document.createElementNS("http://www.w3.org/2000/svg", "line")
    body.setAttribute("x1", "120")
    body.setAttribute("y1", "80")
    body.setAttribute("x2", "120")
    body.setAttribute("y2", "150")
    body.setAttribute("stroke", "#dc2626")
    body.setAttribute("stroke-width", "3")
    svg.appendChild(body)
  }

  // Left arm
  if (wrong > 2) {
    const leftArm = document.createElementNS("http://www.w3.org/2000/svg", "line")
    leftArm.setAttribute("x1", "120")
    leftArm.setAttribute("y1", "100")
    leftArm.setAttribute("x2", "90")
    leftArm.setAttribute("y2", "130")
    leftArm.setAttribute("stroke", "#dc2626")
    leftArm.setAttribute("stroke-width", "3")
    svg.appendChild(leftArm)
  }

  // Right arm
  if (wrong > 3) {
    const rightArm = document.createElementNS("http://www.w3.org/2000/svg", "line")
    rightArm.setAttribute("x1", "120")
    rightArm.setAttribute("y1", "100")
    rightArm.setAttribute("x2", "150")
    rightArm.setAttribute("y2", "130")
    rightArm.setAttribute("stroke", "#dc2626")
    rightArm.setAttribute("stroke-width", "3")
    svg.appendChild(rightArm)
  }

  // Left leg
  if (wrong > 4) {
    const leftLeg = document.createElementNS("http://www.w3.org/2000/svg", "line")
    leftLeg.setAttribute("x1", "120")
    leftLeg.setAttribute("y1", "150")
    leftLeg.setAttribute("x2", "90")
    leftLeg.setAttribute("y2", "180")
    leftLeg.setAttribute("stroke", "#dc2626")
    leftLeg.setAttribute("stroke-width", "3")
    svg.appendChild(leftLeg)
  }

  // Right leg
  if (wrong > 5) {
    const rightLeg = document.createElementNS("http://www.w3.org/2000/svg", "line")
    rightLeg.setAttribute("x1", "120")
    rightLeg.setAttribute("y1", "150")
    rightLeg.setAttribute("x2", "150")
    rightLeg.setAttribute("y2", "180")
    rightLeg.setAttribute("stroke", "#dc2626")
    rightLeg.setAttribute("stroke-width", "3")
    svg.appendChild(rightLeg)
  }
}

function updateDisplay() {
  const display = word
      .split("")
      .map((ch) => (guessed.has(ch) ? ch.toUpperCase() : "_"))
      .join(" ")
  const wordDisplayEl = document.getElementById("wordDisplay")
  if (wordDisplayEl) {
    wordDisplayEl.textContent = display
  }

  const wrongLettersEl = document.getElementById("wrongLetters")
  if (wrongLettersEl) {
    const wrongLetters = Array.from(guessed).filter((ch) => !word.includes(ch))
    wrongLettersEl.textContent = wrongLetters.map((ch) => ch.toUpperCase()).join(" ")
  }

  drawHangman()
}

function updateHints() {
  const card = cards[idx]
  const gameId = Number.parseInt(document.getElementById("gameRoot").dataset.gameId)
  const q = card.gameQuestions?.find((g) => g.gameId === gameId)

  const synonymEl = document.getElementById("hintSynonym")
  const definitionEl = document.getElementById("hintDefinition")
  const sentenceEl = document.getElementById("hintSentence")

  if (synonymEl) synonymEl.textContent = card.synonym || "Belirtilmemiş"
  if (definitionEl) definitionEl.textContent = card.meaning || "Belirtilmemiş"
  if (sentenceEl) sentenceEl.textContent = card.exampleSentence || "Belirtilmemiş"
}

function handleGuess(ch, studentId, gameId, btn) {
  if (guessed.has(ch)) return

  guessed.add(ch)

  if (word.includes(ch)) {
    if (btn) {
      btn.classList.add("correct")
      btn.disabled = true
    }
  } else {
    wrong++
    if (btn) {
      btn.classList.add("wrong")
      btn.disabled = true
    }
  }

  updateDisplay()

  // Check win condition
  if (word.split("").every((c) => guessed.has(c))) {
    const duration = (Date.now() - startTime) / 1000
    awardScore(studentId, gameId, true, duration)

    const feedbackEl = document.getElementById("aaFeedback")
    if (feedbackEl) {
      feedbackEl.innerHTML = '<span class="text-green-600">🎉 Tebrikler! Kelimeyi buldun!</span>'
    }

    setTimeout(() => {
      if (window.parent !== window) {
        window.parent.postMessage("next-game", "*")
      } else {
        startNext(studentId, gameId)
      }
    }, 2000)
    return
  }

  // Check lose condition
  if (wrong >= 6) {
    const duration = (Date.now() - startTime) / 1000
    awardScore(studentId, gameId, false, duration)

    const feedbackEl = document.getElementById("aaFeedback")
    if (feedbackEl) {
      feedbackEl.innerHTML = `<span class="text-red-600">😞 Kaybettin! Kelime: <strong>${word.toUpperCase()}</strong></span>`
    }

    setTimeout(() => {
      if (window.parent !== window) {
        window.parent.postMessage("next-game", "*")
      } else {
        startNext(studentId, gameId)
      }
    }, 2000)
    return
  }
}

function startNext(studentId, gameId) {
  if (cards.length === 0) {
    if (window.parent !== window) {
      window.parent.postMessage("next-game", "*")
    }
    return
  }

  idx = (idx + 1) % cards.length
  setup(studentId, gameId)
}

function setup(studentId, gameId) {
  guessed.clear()
  wrong = 0
  startTime = Date.now()

  const card = cards[idx]
  const q = card.gameQuestions?.find((g) => g.gameId === Number.parseInt(gameId))
  word = (q?.answerText || card.word).toLowerCase()

  updateDisplay()
  updateHints()
  createLetterButtons()

  const feedbackEl = document.getElementById("aaFeedback")
  if (feedbackEl) {
    feedbackEl.innerHTML = ""
  }

  // Setup reveal button
  const revealBtn = document.getElementById("aaReveal")
  if (revealBtn) {
    revealBtn.onclick = () => {
      const feedbackEl = document.getElementById("aaFeedback")
      if (feedbackEl) {
        feedbackEl.innerHTML = `<span class="text-blue-600">💡 Cevap: <strong>${word.toUpperCase()}</strong></span>`
      }

      setTimeout(() => {
        if (window.parent !== window) {
          window.parent.postMessage("next-game", "*")
        } else {
          startNext(studentId, gameId)
        }
      }, 2000)
    }
  }
}

export async function initHangman(studentId, gameId, single, wordId) {
  if (single) {
    if (wordId) {
      const card = await fetchWordCardWithQuestions(wordId)
      cards = card ? [card] : [{ word: single, synonym: "", meaning: "", exampleSentence: "" }]
    } else {
      cards = [{ word: single, synonym: "", meaning: "", exampleSentence: "" }]
    }
    singleMode = true
  } else {
    cards = await fetchLearnedWords(studentId)
    if (cards.length === 0) {
      cards = [
        {
          word: "kelime",
          synonym: "sözcük",
          meaning: "Bir dili oluşturan anlamlı ses birliği",
          exampleSentence: "Bu kelime çok güzel.",
        },
      ]
    }
    singleMode = false
  }

  idx = 0
  setup(studentId, gameId)
}
