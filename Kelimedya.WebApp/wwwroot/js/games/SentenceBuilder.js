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

    // Render current sentence slots (click to remove)
    let html = ""
    if (currentSentence.length > 0) {
        html = currentSentence.map((word, i) =>
            `<span class="sentence-slot filled cursor-pointer" data-index="${i}">${word}</span>`
        ).join(" ")
    } else {
        html = '<span class="text-gray-400">Kelimeleri seçerek cümle kurun...</span>'
    }
    sentenceEl.innerHTML = html

    sentenceEl.querySelectorAll('.sentence-slot.filled').forEach(el => {
        el.onclick = () => {
            const i = parseInt(el.dataset.index)
            const word = currentSentence.splice(i, 1)[0]
            availableWords.push(word)
            render()
        }
    })

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
    const word = availableWords.splice(index, 1)[0]
    currentSentence.push(word)
    render()
}

function normalizeWord(w) {
    return w
        .toLocaleLowerCase('tr')
        .normalize('NFD')
        .replace(/\p{M}/gu, '')
        .replace(/[^\p{L}\p{N}]/gu, '')
}

function checkSentence() {
    const lengthMatch = currentSentence.length === targetSentence.length
    const orderMatch = currentSentence.every((w, i) =>
        normalizeWord(w) === normalizeWord(targetSentence[i])
    )
    const isCorrect = lengthMatch && orderMatch

    awardScore(
        document.getElementById("gameRoot").dataset.studentId,
        document.getElementById("gameRoot").dataset.gameId,
        isCorrect,
        (Date.now() - startTime) / 1000
    )

    const feedbackEl = document.getElementById("sbFeedback")
    if (!feedbackEl) return

    // Disable further selection
    document.querySelectorAll('.sb-word').forEach(btn => btn.disabled = true)
    document.querySelectorAll('.sentence-slot.filled').forEach(el => el.style.pointerEvents = 'none')

    if (isCorrect) {
        feedbackEl.innerHTML = '<span class="text-green-600">🎉 Tebrikler! Doğru cümle kurdun!</span>'
    } else {
        feedbackEl.innerHTML = '<span class="text-red-600">❌ Yanlış! Doğru cevap gösterildi.</span>'
        // Highlight and show correct sentence
        const sentenceContainer = document.getElementById("sbSentence")
        sentenceContainer.innerHTML = targetSentence.map(word =>
            `<button class=\"sb-word correct\" disabled>${word}</button>`
        ).join(" ")
    }

    setTimeout(() => {
        feedbackEl.innerHTML = ""
        nextSentence()
    }, 2000)
}

function revealAnswer() {
    currentSentence = [...targetSentence]
    availableWords = []
    render()
    const feedbackEl = document.getElementById("sbFeedback")
    if (feedbackEl) feedbackEl.innerHTML = '<span class="text-blue-600">💡 Doğru cevap gösterildi!</span>'
    setTimeout(() => {
        if (window.parent !== window) window.parent.postMessage("next-game", "*")
        else nextSentence()
    }, 2000)
}

function nextSentence() {
    if (!cards.length) {
        if (window.parent !== window) window.parent.postMessage("next-game", "*")
        return
    }
    idx = (idx + 1) % cards.length
    setupSentence()
}

function setupSentence() {
    const gid = parseInt(document.getElementById("gameRoot").dataset.gameId)
    const card = cards[idx]
    const q = card.gameQuestions?.find(g => g.gameId === gid) || {}

    // Target sentence: split answerText by spaces, remove punctuation
    const rawAnswer = q.answerText || card.word || ""
    targetSentence = rawAnswer
        .replace(/[^\p{L}\p{N}\s]/gu, '')
        .split(/\s+/)
        .filter(Boolean)

    // Available words: split questionText by hyphens
    const rawQuestion = q.questionText || card.word || ""
    availableWords = rawQuestion.split('-').map(w => w.trim())

    currentSentence = []
    shuffle(availableWords)

    startTime = Date.now()
    render()
    const feedbackEl = document.getElementById("sbFeedback")
    if (feedbackEl) feedbackEl.innerHTML = ""
}

export async function initSentenceBuilder(studentId, gameId, single, wordId) {
    if (single && wordId) {
        const card = await fetchWordCardWithQuestions(wordId)
        cards = card ? [card] : [{ word: single, gameQuestions: [] }]
        singleMode = true
    } else if (!single) {
        cards = await fetchLearnedWords(studentId)
        if (!cards.length) cards = [{ word: "örnek-örnek-örnek", gameQuestions: [] }]
    }
    idx = 0
    setupSentence()

    document.getElementById("sbCheck").onclick = checkSentence
    document.getElementById("sbReveal").onclick = revealAnswer
    document.getElementById("sbNext").onclick = nextSentence
    if (singleMode) document.getElementById("sbNext").style.display = "none"
}
