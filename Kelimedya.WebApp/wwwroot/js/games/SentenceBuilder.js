import { fetchLearnedWords, awardScore, fetchWordCardWithQuestions } from "./common.js"

let targetSentence = [],
    currentSentence = [],
    availableWords = []
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

function render() {
  const sentenceEl = document.getElementById("sbSentence")
  const optionsEl = document.getElementById("sbOptions")

  if (!sentenceEl || !optionsEl) return

  // Render current sentence
  sentenceEl.innerHTML =
      currentSentence.length > 0
          ? currentSentence.map((word) => `<span class="sentence-slot filled">${word}</span>`).join(" ")
          : '<span class="text-gray-400">Kelimeleri seçerek cümle kurun...</span>'

  // Render available words
  optionsEl.innerHTML = ""
  availableWords.forEach((word, index) => {
    const btn = document.createElement("button")
    btn.className = "sb-word"
    btn.textContent = word
    btn.onclick = () => selectWord(index)
    optionsEl.appendChild(btn)
  })
}

function selectWord(index) {
  const word = availableWords[index]
  currentSentence.push(word)
  availableWords.splice(index, 1)
  render()
}

function checkSentence() {
  const isCorrect = JSON.stringify(currentSentence) === JSON.stringify(targetSentence)
  const duration = (Date.now() - startTime) / 1000
  const gid = document.getElementById("gameRoot").dataset.gameId
  const sid = document.getElementById("gameRoot").dataset.studentId

  awardScore(sid, gid, isCorrect, duration)

  const feedbackEl = document.getElementById("sbFeedback")
  if (feedbackEl) {
    if (isCorrect) {
      feedbackEl.innerHTML = '<span class="text-green-600">🎉 Tebrikler! Doğru cümle kurdun!</span>'
      setTimeout(() => {
        if (window.parent !== window) {
          window.parent.postMessage("next-game", "*")
        } else {
          nextSentence()
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
  currentSentence = [...targetSentence]
  availableWords = []
  render()

  const feedbackEl = document.getElementById("sbFeedback")
  if (feedbackEl) {
    feedbackEl.innerHTML = '<span class="text-blue-600">💡 Doğru cevap gösterildi!</span>'
    setTimeout(() => {
      if (window.parent !== window) {
        window.parent.postMessage("next-game", "*")
      } else {
        nextSentence()
      }
    }, 2000)
  }
}

function nextSentence() {
  if (cards.length === 0) {
    if (window.parent !== window) {
      window.parent.postMessage("next-game", "*")
    }
    return
  }

  idx = (idx + 1) % cards.length
  setupSentence()
}

function setupSentence() {
  const card = cards[idx]
  const gid = Number.parseInt(document.getElementById("gameRoot").dataset.gameId)
  const q = card.gameQuestions?.find((g) => g.gameId === gid)

  // Create a simple sentence with the word
  const word = q?.answerText || card.word
  const sentences = [
    ["Bu", "bir", word, "örneğidir"],
    ["Kelime", word, "anlamına", "gelir"],
    [word, "çok", "güzel", "bir", "kelimedir"],
    ["Bugün", word, "kelimesini", "öğrendim"],
  ]

  targetSentence = sentences[Math.floor(Math.random() * sentences.length)]
  currentSentence = []
  availableWords = [...targetSentence]
  shuffle(availableWords)

  startTime = Date.now()
  render()

  const feedbackEl = document.getElementById("sbFeedback")
  if (feedbackEl) {
    feedbackEl.innerHTML = ""
  }
}

export async function initSentenceBuilder(studentId, gameId, single, wordId) {
  if (single) {
    if (wordId) {
      const card = await fetchWordCardWithQuestions(wordId)
      cards = card ? [card] : [{ word: single, gameQuestions: [] }]
    } else {
      cards = [{ word: single, gameQuestions: [] }]
    }
    singleMode = true
  } else {
    cards = await fetchLearnedWords(studentId)
    if (cards.length === 0) cards = [{ word: "örnek", gameQuestions: [] }]
    singleMode = false
  }

  idx = 0
  setupSentence()

  // Setup buttons
  const checkBtn = document.getElementById("sbCheck")
  const revealBtn = document.getElementById("sbReveal")
  const nextBtn = document.getElementById("sbNext")

  if (checkBtn) checkBtn.onclick = checkSentence
  if (revealBtn) revealBtn.onclick = revealAnswer
  if (nextBtn) nextBtn.onclick = nextSentence

  if (singleMode && nextBtn) {
    nextBtn.style.display = "none"
  }
}
