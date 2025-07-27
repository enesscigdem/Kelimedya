import { fetchLearnedWords, awardScore, fetchWordCardWithQuestions, toThumbnailUrl } from "./common.js"

let cards = [],
    idx = 0,
    start
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

  wordEl.textContent = q?.questionText || card.word

  let opts = [q?.imageUrl, q?.imageUrl2, q?.imageUrl3, q?.imageUrl4].filter(Boolean)
  if (opts.length === 0 && card.imageUrl) opts = [card.imageUrl]

  const correct = q?.correctOption ? opts[q.correctOption - 1] : opts[0]

  optionsEl.innerHTML = ""
  opts.forEach((img) => {
    const i = document.createElement("img")
    i.src = toThumbnailUrl(img)
    i.className = "wti-option"
    i.alt = card.word
    i.onclick = () => select(img === correct)
    optionsEl.appendChild(i)
  })

  feedbackEl.textContent = ""
  start = Date.now()
}

function select(success) {
  const studentId = document.getElementById("gameRoot").dataset.studentId
  const gameId = document.getElementById("gameRoot").dataset.gameId
  feedbackEl.textContent = success ? "Doğru!" : "Yanlış"

  const duration = (Date.now() - start) / 1000
  awardScore(studentId, gameId, success, duration)

  if (success) cards.splice(idx, 1)
  if (cards.length === 0) {
    notifyParent()
    return
  }
  idx = (idx + 1) % cards.length
  loadCard()
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
