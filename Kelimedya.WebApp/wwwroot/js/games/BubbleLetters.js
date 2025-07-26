import { fetchLearnedWords, awardScore, fetchWordCardWithQuestions } from "./common.js"

let targetWord = "",
    currentWord = [],
    availableLetters = []
let cards = [],
    idx = 0,
    singleMode = false,
    startTime

function shuffle(arr) {
  for (let i = arr.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1))
    ;[arr[i], arr[j]] = [arr[j], arr[i]]
  }
}

function createTargetSlots() {
  const container = document.getElementById("targetSlots")
  if (!container) return

  container.innerHTML = ""

  for (let i = 0; i < targetWord.length; i++) {
    const slot = document.createElement("div")
    slot.className = "bl-letter"
    slot.textContent = currentWord[i] || ""
    if (currentWord[i]) {
      slot.classList.add("filled")
    }
    container.appendChild(slot)
  }
}

function createBubbles() {
  const container = document.getElementById("bubblesArea")
  if (!container) return

  container.innerHTML = ""

  availableLetters.forEach((letter, index) => {
    const bubble = document.createElement("button")
    bubble.className = "bubble-btn"
    bubble.textContent = letter.toUpperCase()
    bubble.onclick = () => selectLetter(index)
    if (!letter) {
      bubble.disabled = true
    }
    container.appendChild(bubble)
  })
}

function selectLetter(index) {
  const letter = availableLetters[index]
  if (!letter) return

  // Find first empty slot
  const emptyIndex = currentWord.findIndex((l) => !l)
  if (emptyIndex !== -1) {
    currentWord[emptyIndex] = letter
    availableLetters[index] = null

    createTargetSlots()
    createBubbles()
  }
}

function clearWord() {
  // Move all letters back to bubbles
  currentWord.forEach((letter, index) => {
    if (letter) {
      const emptyIndex = availableLetters.findIndex((l) => !l)
      if (emptyIndex !== -1) {
        availableLetters[emptyIndex] = letter
      }
    }
  })

  currentWord = new Array(targetWord.length).fill("")
  createTargetSlots()
  createBubbles()
}

function checkWord() {
  const formed = currentWord.join("").toLowerCase()
  const isCorrect = formed === targetWord.toLowerCase()
  const duration = (Date.now() - startTime) / 1000
  const gid = document.getElementById("gameRoot").dataset.gameId
  const sid = document.getElementById("gameRoot").dataset.studentId

  awardScore(sid, gid, isCorrect, duration)

  const feedbackEl = document.getElementById("blFeedback")
  if (feedbackEl) {
    if (isCorrect) {
      feedbackEl.innerHTML = '<span class="text-green-600">🎉 Tebrikler! Doğru kelimeyi oluşturdun!</span>'
      setTimeout(() => {
        if (window.parent !== window) {
          window.parent.postMessage("next-game", "*")
        } else {
          nextWord()
        }
      }, 2000)
    } else {
      feedbackEl.innerHTML = '<span class="text-red-600">❌ Yanlış! Tekrar dene.</span>'
      setTimeout(() => {
        feedbackEl.innerHTML = ""
      }, 2000)
    }
  }
}

function revealAnswer() {
  currentWord = targetWord.split("")
  availableLetters = availableLetters.map(() => null)

  createTargetSlots()
  createBubbles()

  const feedbackEl = document.getElementById("blFeedback")
  if (feedbackEl) {
    feedbackEl.innerHTML = '<span class="text-blue-600">💡 Doğru cevap gösterildi!</span>'
    setTimeout(() => {
      if (window.parent !== window) {
        window.parent.postMessage("next-game", "*")
      } else {
        nextWord()
      }
    }, 2000)
  }
}

function nextWord() {
  if (cards.length === 0) {
    if (window.parent !== window) {
      window.parent.postMessage("next-game", "*")
    }
    return
  }

  idx = (idx + 1) % cards.length
  setupWord()
}

function updateHints() {
  const card = cards[idx]
  const isEmbed = document.getElementById("gameRoot").dataset.embed === "true"

  if (isEmbed) {
    const hintEl = document.getElementById("hintText")
    if (hintEl) {
      hintEl.textContent = card.synonym || card.meaning || "İpucu yok"
    }
  } else {
    const synonymEl = document.getElementById("hintSynonym")
    const definitionEl = document.getElementById("hintDefinition")
    const sentenceEl = document.getElementById("hintSentence")

    if (synonymEl) synonymEl.textContent = card.synonym || "Belirtilmemiş"
    if (definitionEl) definitionEl.textContent = card.meaning || "Belirtilmemiş"
    if (sentenceEl) sentenceEl.textContent = card.exampleSentence || "Belirtilmemiş"
  }
}

function setupWord() {
  const card = cards[idx]
  const gid = Number.parseInt(document.getElementById("gameRoot").dataset.gameId)
  const q = card.gameQuestions?.find((g) => g.gameId === gid)

  targetWord = (q?.answerText || card.word).toLowerCase()
  currentWord = new Array(targetWord.length).fill("")

  // Create available letters (scrambled word + some extra letters)
  const wordLetters = targetWord.split("")
  const extraLetters = "abcdefghijklmnopqrstuvwxyz".split("").filter((l) => !wordLetters.includes(l))
  shuffle(extraLetters)

  availableLetters = [...wordLetters, ...extraLetters.slice(0, 3)]
  shuffle(availableLetters)

  startTime = Date.now()

  updateHints()
  createTargetSlots()
  createBubbles()

  const feedbackEl = document.getElementById("blFeedback")
  if (feedbackEl) {
    feedbackEl.innerHTML = ""
  }
}

export async function initBubbleLetters(studentId, gameId, single, wordId) {
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
          meaning: "Anlamlı ses birliği",
          exampleSentence: "Bu bir kelime örneğidir.",
        },
      ]
    }
    singleMode = false
  }

  idx = 0
  setupWord()

  // Setup buttons
  const clearBtn = document.getElementById("blClear")
  const submitBtn = document.getElementById("blSubmit")
  const revealBtn = document.getElementById("blReveal")
  const nextBtn = document.getElementById("blNext")
  const backBtn = document.getElementById("blBack")

  if (clearBtn) clearBtn.onclick = clearWord
  if (submitBtn) submitBtn.onclick = checkWord
  if (revealBtn) revealBtn.onclick = revealAnswer
  if (nextBtn) nextBtn.onclick = nextWord
  if (backBtn) backBtn.onclick = () => window.history.back()

  if (singleMode && nextBtn) {
    nextBtn.style.display = "none"
  }
}
