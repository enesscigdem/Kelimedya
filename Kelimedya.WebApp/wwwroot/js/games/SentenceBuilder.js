import { fetchLearnedWords, awardScore, fetchWordCardWithQuestions } from "./common.js"

let targetSentence = [],
    currentSentence = [],
    availableWords = []
let cards = [],
    idx = 0,
    singleMode = false,
    startTime
let locked = false
let roundFinished = false;
let scoredThisRound = false;   // ✅ eklendi
let revealedThisRound = false; // ✅ eklendi

function setControlsDisabled(disabled) {
    const chk = document.getElementById("sbCheck");
    const rev = document.getElementById("sbReveal");
    const nxt = document.getElementById("sbNext");
    if (chk) chk.disabled = disabled;
    if (rev) rev.disabled = disabled;
    if (nxt) nxt.disabled = disabled;
}

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
    if (locked || roundFinished) return
    if (revealedThisRound) return       // ✅ önce cevap gösterildiyse buton etkisiz
    locked = true
    setControlsDisabled(true)

    const lengthMatch = currentSentence.length === targetSentence.length
    const orderMatch = lengthMatch && currentSentence.every((w, i) => normalizeChunk(w) === normalizeChunk(targetSentence[i]))
    const isCorrect = lengthMatch && orderMatch

    // ✅ sadece doğruysa ve ilk kezse puan ver
    if (isCorrect) {
        tryScore(true) // puanı tek yerden, idempotent
    } else {
        // ❌ yanlış: skor yazma, sadece “yanlış” bildirimi / ses için
        awardScore(
            document.getElementById("gameRoot").dataset.studentId,
            document.getElementById("gameRoot").dataset.gameId,
            false, // <<<<<< yanlış
            (Date.now() - startTime) / 1000
        )
    }
    roundFinished = true
    
    const feedbackEl = document.getElementById("sbFeedback")
    document.querySelectorAll(".sb-word").forEach(btn => (btn.disabled = true))
    document.querySelectorAll(".sentence-slot.filled").forEach(el => (el.style.pointerEvents = "none"))

    if (isCorrect) {
        if (feedbackEl) feedbackEl.innerHTML = '<span class="text-green-600">🎉 Tebrikler! Doğru cümle kurdun!</span>'
    } else {
        if (feedbackEl) feedbackEl.innerHTML = '<span class="text-red-600">❌ Yanlış! Doğru cevap gösterildi.</span>'
        const sentenceContainer = document.getElementById("sbSentence")
        sentenceContainer.innerHTML = targetSentence
            .map(word => `<button class="sb-word correct" disabled>${word}</button>`)
            .join(" ")
    }

    setTimeout(() => {
        if (feedbackEl) feedbackEl.innerHTML = ""
        nextSentence()
    }, 2000)
}


function revealAnswer() {
    if (locked || roundFinished) return; // ✅
    locked = true
    revealedThisRound = true    // ✅ bu elde puan kilitlensin
    roundFinished = true // ✅ puanlama kapanır

    currentSentence = [...targetSentence]
    availableWords = []
    render()

    document.querySelectorAll(".sb-word").forEach(btn => (btn.disabled = true))
    setControlsDisabled(true)

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
function tryScore(isCorrect) {
    if (!isCorrect) return;            // yanlışsa asla puan yok
    if (revealedThisRound) return;     // cevap gösterildiyse puan yok
    if (scoredThisRound) return;       // zaten puanlandıysa tekrar yok

    scoredThisRound = true;            // 👈 idempotent kilit
    awardScore(
        document.getElementById("gameRoot").dataset.studentId,
        document.getElementById("gameRoot").dataset.gameId,
        true,
        (Date.now() - startTime) / 1000
    )
}

function setupSentence() {
    const gid = parseInt(document.getElementById("gameRoot").dataset.gameId)
    const card = cards[idx]
    const q = card.gameQuestions?.find(g => g.gameId === gid) || {}

    const chunks = (q.questionText || card.word || "")
        .split("-").map(w => w.trim()).filter(Boolean)

    const ansNorm = normalizeText(q.answerText || card.word || "")
    const withPos = chunks.map(ch => ({ ch, pos: ansNorm.indexOf(normalizeChunk(ch)) }))
    withPos.sort((a, b) => (a.pos === -1) - (b.pos === -1) || a.pos - b.pos)

    targetSentence = withPos.map(x => x.ch)
    availableWords = [...chunks]
    currentSentence = []
    shuffle(availableWords)

    locked = false
    roundFinished = false // ✅ tur sıfırlandı
    scoredThisRound = false        // ✅
    revealedThisRound = false      // ✅
    setControlsDisabled(false) // ✅ aç
    startTime = Date.now()
    render()
    const feedbackEl = document.getElementById("sbFeedback")
    if (feedbackEl) feedbackEl.innerHTML = ""
}


function autoSuccess() {
    if (locked || roundFinished) return; // ✅
    locked = true
    tryScore(true)   // yeterli
    roundFinished = true
    
    document.querySelectorAll(".sb-word").forEach(btn => (btn.disabled = true))
    document.querySelectorAll(".sentence-slot.filled").forEach(el => (el.style.pointerEvents = "none"))
    setControlsDisabled(true)

    const feedbackEl = document.getElementById("sbFeedback")
    if (feedbackEl) feedbackEl.innerHTML = '<span class="text-green-600">🎉 Tebrikler! Doğru cümle kurdun!</span>'

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
