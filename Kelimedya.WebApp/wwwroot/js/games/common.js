const API_BASE_URL = window.API_BASE_URL;
const recentAwards = new Map();

// Declare updateScoreDisplay function if not already declared
const updateScoreDisplay = (score) => {
    const scoreEl = document.getElementById("scorePoints")
    if (scoreEl) scoreEl.textContent = score
    const leagueEl = document.getElementById("leagueLabel")
    if (leagueEl) {
        let league = "Bronz"
        if (score >= 6000) league = "Altın"
        else if (score >= 2500) league = "Gümüş"
        leagueEl.textContent = league
    }
}

export function toThumbnailUrl(url) {
    if (!url) return url;
    const m = url.match(/\/d\/([^/]+)/);
    return m ? `https://drive.google.com/thumbnail?authuser=0&sz=w320&id=${m[1]}` : url;
}

// Declare iziToast object if not already declared
window.iziToast = window.iziToast || {
    show: (options) => {
        console.log("iziToast show:", options)
    },
}

function shuffleArray(array) {
    const a = [...(array || [])];
    for (let i = a.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [a[i], a[j]] = [a[j], a[i]];
    }
    return a;
}

export async function fetchLearnedWords(studentId, lessonId) {
    const url = lessonId
        ? `${API_BASE_URL}/api/progress/wordcards/learned/${studentId}?lessonId=${lessonId}`
        : `${API_BASE_URL}/api/progress/wordcards/learned/${studentId}`;

    const res = await fetch(url);
    if (!res.ok) return [];

    const data = await res.json();

    const filtered = lessonId ? data.filter(x => x.progress?.lessonId == lessonId) : data;

    const mapped = filtered.map((x) => ({
        ...x.wordCard,
        gameQuestions: shuffleArray(x.gameQuestions),
    }));

    return shuffleArray(mapped);
}



export async function fetchWordCardWithQuestions(id) {
    const cardRes = await fetch(`${API_BASE_URL}/api/wordcards/${id}`)
    if (!cardRes.ok) return null
    const card = await cardRes.json()
    const qRes = await fetch(`${API_BASE_URL}/api/wordcards/${id}/questions`)
    const questions = qRes.ok ? await qRes.json() : []
    return {...card, gameQuestions: questions}
}

export async function recordGameStat(stat) {
    const payload = {...stat, gameId: Number.parseInt(stat.gameId, 10)}
    const response = await fetch(`${API_BASE_URL}/api/gamestats/record`, {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify(payload),
    })
    if (!response.ok) return null

    // Kayıt başarılıysa güncel skoru al ve UI'ı güncelle
    const scoreResponse = await fetch(`${API_BASE_URL}/api/gamestats/score/${stat.studentId}`)
    if (!scoreResponse.ok) return null
    const scoreInfo = await scoreResponse.json()

    if (typeof updateScoreDisplay === "function") {
        updateScoreDisplay(scoreInfo.totalScore)
    } else {
        // fallback
        const scoreEl = document.getElementById("scorePoints")
        if (scoreEl) scoreEl.textContent = scoreInfo.totalScore
        const leagueEl = document.getElementById("leagueLabel")
        if (leagueEl) leagueEl.textContent = scoreInfo.league
    }

    return scoreInfo.totalScore
}

export async function awardScore(studentId, gameId, success, durationSeconds, correctAnswer = "", allowScore = true) {
    const root = document.getElementById('gameRoot');
    const embedded = root && root.dataset.embed === 'true';
    const score = success && allowScore ? Math.max(10, Math.floor(100 - durationSeconds * 5)) : 0;

    const key = `${gameId}-${correctAnswer}`;
    const now = Date.now();
    if (success && allowScore) {
        const last = recentAwards.get(key);
        if (last && now - last < 1000) {
            return; // prevent double scoring
        }
        recentAwards.set(key, now);
    }

    let newTotal = null;
    if (embedded) {
        newTotal = await recordGameStat({studentId, gameId, score, durationSeconds});
        if (newTotal === null && success && allowScore) {
            incrementScore(score);
        }
    } else if (success && allowScore) {
        incrementScore(score);
    }

    if (success && allowScore) {
        // Toast içeriği: embed modda +puan, normal sayfada sadece "Başarılı!"
        if (embedded) {
            showIziToastSuccess(`+${score} puan`);
        } else {
            showIziToastSuccess("Başarılı!");
        }

        const correctAudio = document.getElementById("correctAudio");
        if (correctAudio) {
            correctAudio.currentTime = 0;
            correctAudio.play().catch(() => console.warn("Doğru cevap sesi çalınamadı"));
        }
    } else {
        const message = correctAnswer ? `Yanlış cevap! Doğrusu: ${correctAnswer}` : "Yanlış cevap!";
        showIziToastError(message);

        const incorrectAudio = document.getElementById("incorrectAudio");
        if (incorrectAudio) {
            incorrectAudio.currentTime = 0;
            incorrectAudio.play().catch(() => console.warn("Yanlış cevap sesi çalınamadı"));
        }
    }
}

// Utility to temporarily disable a set of buttons/elements
export function disableTemporary(elements = [], ms = 2000) {
    (elements || []).forEach(el => { if (el) el.disabled = true; });
    setTimeout(() => {
        (elements || []).forEach(el => { if (el) el.disabled = false; });
    }, ms);
}

export function incrementScore(amount) {
    const el = document.getElementById("scorePoints")
    if (!el) return
    const newScore = Number.parseInt(el.textContent || "0", 10) + amount

    // Sayfada tanımlı updateScoreDisplay fonksiyonunu çağır
    if (typeof updateScoreDisplay === "function") {
        updateScoreDisplay(newScore)
    } else {
        // Fallback eski davranış
        el.textContent = newScore
        const leagueEl = document.getElementById("leagueLabel")
        if (leagueEl) {
            let league = "Bronz"
            if (newScore >= 6000) league = "Altın"
            else if (newScore >= 2500) league = "Gümüş"
            leagueEl.textContent = league
        }
    }
}

function showIziToastSuccess(message) {
    if (typeof window.iziToast !== "undefined") {
        window.iziToast.show({
            title: "Başarılı!",
            message: message,
            position: "topRight",
            theme: "light",
            color: "green",
            timeout: 3000,
            icon: "fa-solid fa-circle-check",
            transitionIn: "fadeInDown",
            transitionOut: "fadeOutUp",
            closeOnClick: true,
            progressBar: false,
        })
    }
}

export function showIziToastError(message) {
    if (typeof window.iziToast !== "undefined") {
        window.iziToast.show({
            title: "Hata!",
            message: message,
            position: "topRight",
            theme: "light",
            color: "red",
            timeout: 5000,
            icon: "fa-solid fa-circle-xmark",
            transitionIn: "fadeInDown",
            transitionOut: "fadeOutUp",
            closeOnClick: true,
            progressBar: false,
        })
    }
}
