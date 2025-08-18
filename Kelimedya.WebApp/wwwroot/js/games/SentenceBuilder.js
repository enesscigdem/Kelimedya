import { fetchLearnedWords, awardScore, fetchWordCardWithQuestions } from "./common.js"

let targetSentence = [],
    currentSentence = [],
    availableWords = []
let cards = [],
    idx = 0,
    singleMode = false,
    startTime
let locked = false

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

    let html = ""
    if (currentSentence.length > 0) {
        html = currentSentence
            .map((word, i) => `<span class="sentence-slot filled cursor-pointer" data-index="${i}">${word}</span>`)
            .join(" ")
    } else {
        html = '<span class="text-gray-400">Kelimeleri seçerek cümle kurun...</span>'
    }
    sentenceEl.innerHTML = html

    sentenceEl.querySelectorAll(".sentence-slot.filled").forEach(el => {
        el.onclick = () => {
            if (locked) return
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
    if (locked) return
    const word = availableWords.splice(index, 1)[0]
    currentSentence.push(word)
    render()

    // tüm parçalar yerleştiyse ve sıra doğruysa otomatik bitir
    if (currentSentence.length === targetSentence.length) {
        const ok = currentSentence.every((w, i) => normalizeChunk(w) === normalizeChunk(targetSentence[i]))
        if (ok) autoSuccess()
    }
}

function normalizeWord(w) {
    return w
        .toLocaleLowerCase("tr")
        .normalize("NFD")
        .replace(/\p{M}/gu, "")
        .replace(/[^\p{L}\p{N}]/gu, "")
}

function checkSentence() {
    if (locked) return
    locked = true

    const chk = document.getElementById("sbCheck")
    const rev = document.getElementById("sbReveal")
    const nxt = document.getElementById("sbNext")
    if (chk) chk.disabled = true
    if (rev) rev.disabled = true
    if (nxt) nxt.disabled = true

    const lengthMatch = currentSentence.length === targetSentence.length
    const orderMatch =
        lengthMatch && currentSentence.every((w, i) => normalizeChunk(w) === normalizeChunk(targetSentence[i]))
    const isCorrect = lengthMatch && orderMatch

    awardScore(
        document.getElementById("gameRoot").dataset.studentId,
        document.getElementById("gameRoot").dataset.gameId,
        isCorrect,
        (Date.now() - startTime) / 1000
    )

    const feedbackEl = document.getElementById("sbFeedback")
    if (!feedbackEl) return

    document.querySelectorAll(".sb-word").forEach(btn => (btn.disabled = true))
    document.querySelectorAll(".sentence-slot.filled").forEach(el => (el.style.pointerEvents = "none"))

    if (isCorrect) {
        feedbackEl.innerHTML = '<span class="text-green-600">🎉 Tebrikler! Doğru cümle kurdun!</span>'
    } else {
        feedbackEl.innerHTML = '<span class="text-red-600">❌ Yanlış! Doğru cevap gösterildi.</span>'
        const sentenceContainer = document.getElementById("sbSentence")
        sentenceContainer.innerHTML = targetSentence
            .map(word => `<button class="sb-word correct" disabled>${word}</button>`)
            .join(" ")
    }

    setTimeout(() => {
        feedbackEl.innerHTML = ""
        nextSentence()
    }, 2000)
}

function revealAnswer() {
    if (locked) return
    locked = true
    currentSentence = [...targetSentence]
    availableWords = []
    render()

    document.querySelectorAll(".sb-word").forEach(btn => (btn.disabled = true))
    const chk = document.getElementById("sbCheck")
    const rev = document.getElementById("sbReveal")
    const nxt = document.getElementById("sbNext")
    if (chk) chk.disabled = true
    if (rev) rev.disabled = true
    if (nxt) nxt.disabled = true

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
    // son karta geldiysek pop-up’ta parent’a geç sinyali gönder
    if (idx + 1 >= cards.length) {
        if (window.parent !== window) {
            window.parent.postMessage("next-game", "*")
            return
        }
    }
    idx = (idx + 1) % cards.length
    setupSentence()
}

function normalizeText(s) {
    return (s || "")
        .toLocaleLowerCase("tr")
        .normalize("NFD")
        .replace(/\p{M}/gu, "")
        .replace(/[^\p{L}\p{N}\s]/gu, " ")
        .replace(/\s+/g, " ")
        .trim()
}

function normalizeChunk(s) {
    return normalizeText(s)
}
function enableControls() {
    const chk = document.getElementById("sbCheck");
    const rev = document.getElementById("sbReveal");
    const nxt = document.getElementById("sbNext");
    if (chk) chk.disabled = false;
    if (rev) rev.disabled = false;
    if (nxt) nxt.disabled = false;
}

function setupSentence() {
    const gid = parseInt(document.getElementById("gameRoot").dataset.gameId)
    const card = cards[idx]
    const q = card.gameQuestions?.find(g => g.gameId === gid) || {}

    // 1) Parçaları (chunk) soru metninden al
    const chunks = (q.questionText || card.word || "")
        .split("-")
        .map(w => w.trim())
        .filter(Boolean)

    // 2) Doğru sıralamayı answerText’e göre oluştur
    const ansNorm = normalizeText(q.answerText || card.word || "")
    const withPos = chunks.map(ch => ({
        ch,
        pos: ansNorm.indexOf(normalizeChunk(ch))
    }))

    // eşleşmeyen varsa (pos === -1) stabil kalması için sona at
    withPos.sort((a, b) => {
        if (a.pos === -1 && b.pos === -1) return 0
        if (a.pos === -1) return 1
        if (b.pos === -1) return -1
        return a.pos - b.pos
    })

    // target ve seçenekler chunk bazlı
    targetSentence = withPos.map(x => x.ch)
    availableWords = [...chunks]

    currentSentence = []
    shuffle(availableWords)

    locked = false;
    enableControls();
    startTime = Date.now()
    render()
    const feedbackEl = document.getElementById("sbFeedback")
    if (feedbackEl) feedbackEl.innerHTML = ""
}

function autoSuccess() {
    if (locked) return
    locked = true

    awardScore(
        document.getElementById("gameRoot").dataset.studentId,
        document.getElementById("gameRoot").dataset.gameId,
        true,
        (Date.now() - startTime) / 1000
    )

    // UI kilitle
    document.querySelectorAll(".sb-word").forEach(btn => (btn.disabled = true))
    document.querySelectorAll(".sentence-slot.filled").forEach(el => (el.style.pointerEvents = "none"))
    const chk = document.getElementById("sbCheck")
    const rev = document.getElementById("sbReveal")
    const nxt = document.getElementById("sbNext")
    if (chk) chk.disabled = true
    if (rev) rev.disabled = true
    if (nxt) nxt.disabled = true

    const feedbackEl = document.getElementById("sbFeedback")
    if (feedbackEl) {
        feedbackEl.innerHTML = '<span class="text-green-600">🎉 Tebrikler! Doğru cümle kurdun!</span>'
    }

    setTimeout(() => {
        if (feedbackEl) feedbackEl.innerHTML = ""
        if (idx + 1 >= cards.length && window.parent !== window) {
            window.parent.postMessage("next-game", "*")
            return
        }
        nextSentence()
    }, 1200)
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
