import { fetchLearnedWords, awardScore, fetchWordCardWithQuestions } from "./common.js"

let pairs = [],
    displayed = [],
    pool = []
let selectedLeft = null,
    selectedRight = null
let singleMode = false,
    startTime,
    matchedCount = 0

function shuffle(arr) {
  for (let i = arr.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1))
    ;[arr[i], arr[j]] = [arr[j], arr[i]]
  }
}

function render() {
  const left = document.getElementById("leftColumn")
  const right = document.getElementById("rightColumn")
  const isEmbed = document.getElementById("gameRoot").dataset.embed === "true"

  if (!left || !right) return

  left.innerHTML = ""
  right.innerHTML = ""

  const lItems = displayed.map((p) => ({ id: p.id, text: p.left }))
  const rItems = displayed.map((p) => ({ id: p.id, text: p.right }))
  shuffle(lItems)
  shuffle(rItems)

  // Create left column items
  lItems.forEach((it) => {
    const div = document.createElement("div")
    div.className = "match-item"
    div.textContent = it.text
    div.onclick = () => chooseLeft(it.id, div)
    left.appendChild(div)
  })

  // Create right column items
  rItems.forEach((it) => {
    const div = document.createElement("div")
    div.className = "match-item"
    div.textContent = it.text
    div.onclick = () => chooseRight(it.id, div)
    right.appendChild(div)
  })

  startTime = Date.now()
}

function resetSelections() {
  if (selectedLeft) {
    selectedLeft.el.classList.remove("active")
  }
  if (selectedRight) {
    selectedRight.el.classList.remove("active")
  }
  selectedLeft = null
  selectedRight = null
}

function chooseLeft(id, el) {
  // Remove previous selection
  if (selectedLeft) selectedLeft.el.classList.remove("active")

  selectedLeft = { id, el }
  el.classList.add("active")

  if (selectedRight) checkMatch()
}

function chooseRight(id, el) {
  // Remove previous selection
  if (selectedRight) selectedRight.el.classList.remove("active")

  selectedRight = { id, el }
  el.classList.add("active")

  if (selectedLeft) checkMatch()
}

function checkMatch() {
  const isCorrect = selectedLeft.id === selectedRight.id
  const duration = (Date.now() - startTime) / 1000
  const gid = document.getElementById("gameRoot").dataset.gameId
  const sid = document.getElementById("gameRoot").dataset.studentId

  awardScore(sid, gid, isCorrect, duration)

  if (isCorrect) {
    // Mark as correct
    selectedLeft.el.classList.add("correct")
    selectedRight.el.classList.add("correct")
    selectedLeft.el.classList.add("disabled")
    selectedRight.el.classList.add("disabled")

    // Remove from displayed array
    displayed = displayed.filter((p) => p.id !== selectedLeft.id)
    matchedCount++

    // Show feedback
    const feedbackEl = document.getElementById("smFeedback")
    if (feedbackEl) {
      feedbackEl.innerHTML = '<span class="text-green-600">✅ Doğru eşleştirme!</span>'
      setTimeout(() => {
        feedbackEl.innerHTML = ""
      }, 1000)
    }

    // Check if all matched
    if (displayed.length === 0) {
      setTimeout(() => {
        const feedbackEl = document.getElementById("smFeedback")
        if (feedbackEl) {
          feedbackEl.innerHTML = '<span class="text-green-600">🎉 Tebrikler! Tüm eşleştirmeleri tamamladın!</span>'
        }

        setTimeout(() => {
          if (window.parent !== window) {
            window.parent.postMessage("next-game", "*")
          }
        }, 2000)
      }, 500)
    } else {
      // Refill if needed
      setTimeout(() => {
        refill()
      }, 1000)
    }
  } else {
    // Show wrong feedback
    const feedbackEl = document.getElementById("smFeedback")
    if (feedbackEl) {
      feedbackEl.innerHTML = '<span class="text-red-600">❌ Yanlış eşleştirme, tekrar dene!</span>'
      setTimeout(() => {
        feedbackEl.innerHTML = ""
      }, 1500)
    }
  }

  // Reset selections after a delay
  setTimeout(() => {
    resetSelections()
  }, 500)
}

function refill() {
  // For popup mode, show 5 pairs at a time
  const isEmbed = document.getElementById("gameRoot").dataset.embed === "true"
  const maxDisplay = isEmbed ? 5 : 5

  while (displayed.length < maxDisplay && pool.length > 0) {
    displayed.push(pool.shift())
  }

  if (displayed.length === 0 && pool.length === 0) {
    if (window.parent !== window) {
      window.parent.postMessage("next-game", "*")
    }
    return
  }

  render()
}

export async function initSynonymMatch(studentId, gameId, single, wordId, lessonId) {
  let cards
  if (single) {
    if (wordId) {
      const card = await fetchWordCardWithQuestions(wordId)
      cards = card ? [card] : [{ word: single, synonym: single, gameQuestions: [] }]
    } else {
      cards = [{ word: single, synonym: single, gameQuestions: [] }]
    }
    singleMode = true
  } else {
    cards = await fetchLearnedWords(studentId, lessonId)
    if (cards.length === 0) {
      cards = [
        { word: "güzel", synonym: "hoş", gameQuestions: [] },
        { word: "büyük", synonym: "iri", gameQuestions: [] },
        { word: "hızlı", synonym: "süratli", gameQuestions: [] },
        { word: "akıllı", synonym: "zeki", gameQuestions: [] },
        { word: "mutlu", synonym: "sevinçli", gameQuestions: [] },
      ]
    }
    singleMode = false
  }

  const gid = Number.parseInt(gameId)
  pairs = cards.map((c, i) => {
    const q = c.gameQuestions?.find((g) => g.gameId === gid)
    return {
      id: i,
      left: q?.questionText || c.word,
      right: q?.answerText || c.synonym || c.word,
    }
  })

  shuffle(pairs)

  // For popup, show 5 pairs at a time
  const isEmbed = document.getElementById("gameRoot").dataset.embed === "true"
  const maxDisplay = isEmbed ? 5 : 5

  displayed = pairs.slice(0, maxDisplay)
  pool = pairs.slice(maxDisplay)
  matchedCount = 0

  const endBtn = document.getElementById("smEndGame")
  if (endBtn) {
    endBtn.onclick = () => {
      window.location = endBtn.dataset.home
    }
    if (singleMode) endBtn.style.display = "none"
  }

  render()
}
