import { fetchLearnedWords, awardScore, fetchWordCardWithQuestions, toThumbnailUrl } from "./common.js"

let cards = [],
    idx = 0,
    start,
    correctIdx = 0
let singleMode = false
let wordEl, optionsEl, feedbackEl

function findQuestion(card, gid) {
  let q = card.gameQuestions?.find((g) => g.gameId === gid)
  if (!q) {
    q = card.gameQuestions?.find((g) => g.imageUrl || g.imageUrl2 || g.imageUrl3 || g.imageUrl4)
  }
  return q
}

function loadCard() {
  const card = cards[idx]
  const gid = Number.parseInt(document.getElementById("gameRoot").dataset.gameId)
  const q = findQuestion(card, gid)

  wordEl.innerHTML = q?.questionText || card.word

  let opts = [q?.imageUrl, q?.imageUrl2, q?.imageUrl3, q?.imageUrl4].filter(Boolean)
  if (opts.length === 0 && card.imageUrl) opts = [card.imageUrl]

  correctIdx = q?.correctOption ? q.correctOption - 1 : 0
  const correct = opts[correctIdx]

  optionsEl.innerHTML = ""
  opts.forEach((img, i) => {
    const el = document.createElement("img")
    el.src = toThumbnailUrl(img)
    el.className = "wti-option"
    el.alt = card.word
    el.onclick = () => select(el, i === correctIdx, card.word)
    optionsEl.appendChild(el)
  })

  feedbackEl.textContent = ""
  start = Date.now()
}

function select(el, success, correctWord) {
  const studentId = document.getElementById("gameRoot").dataset.studentId
  const gameId = document.getElementById("gameRoot").dataset.gameId

  // tıklamaları kilitle
  optionsEl.querySelectorAll(".wti-option").forEach(o => o.onclick = null)

  const duration = (Date.now() - start) / 1000
  awardScore(studentId, gameId, success, duration, correctWord)

  if (success) {
    el.classList.add("correct")
    feedbackEl.textContent = "Doğru!"
  } else {
    el.classList.add("incorrect")
    const correctEl = optionsEl.querySelectorAll(".wti-option")[correctIdx]
    if (correctEl) correctEl.classList.add("correct")
    feedbackEl.textContent = `Yanlış! Doğru: ${correctWord}`
  }

  cards.splice(idx, 1)

  const proceed = () => {
    if (cards.length === 0) {
      notifyParent()
    } else {
      if (idx >= cards.length) idx = 0
      loadCard()
    }
  }

  setTimeout(proceed, 2000)
}

export async function initWordImage(studentId, gameId, single, wordId) {
  if (single) {
    if (wordId) {
      const card = await fetchWordCardWithQuestions(wordId)
      cards = card ? [card] : [{ word: single, imageUrl: "", definition: "", exampleSentence: "" }]
    } else {
      cards = [{ word: single, imageUrl: "", definition: "", exampleSentence: "" }]
    }
    singleMode = true
  } else {
    cards = await fetchLearnedWords(studentId)
    if (cards.length === 0) cards = [{ word: "örnek", imageUrl: "", definition: "", exampleSentence: "" }]
    singleMode = false
  }

  wordEl = document.getElementById("wtiWord")
  optionsEl = document.getElementById("wtiOptions")
  feedbackEl = document.getElementById("wtiFeedback")

  const nextBtn = document.getElementById("wtiNext")
  if (nextBtn) {
    nextBtn.onclick = () => {
      idx = (idx + 1) % cards.length
      loadCard()
    }
    if (singleMode) nextBtn.style.display = "none"
  }

  if (singleMode) {
    const back = document.getElementById("wtiBack")
    if (back) back.style.display = "none"
  }

  loadCard()
}

function notifyParent() {
  if (window.parent !== window) window.parent.postMessage("next-game", "*")
}
