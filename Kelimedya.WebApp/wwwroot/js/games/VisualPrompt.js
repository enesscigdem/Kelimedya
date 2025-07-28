import {
    fetchLearnedWords,
    awardScore,
    fetchWordCardWithQuestions,
    toThumbnailUrl
} from "./common.js";

let cards = [], idx = 0, start;
let singleMode = false;
let imgEl, feedbackEl, questionEl, optionsContainer;

// 1→"A", 2→"B", 3→"C", 4→"D"
const letters = ["A", "B", "C", "D"];

function loadCard(studentId, gameId) {
    const card = cards[idx];
    const q = card.gameQuestions?.find(g => g.gameId === Number(gameId)) || {};

    // Resim ve soru
    imgEl.src = q.imageUrl || card.imageUrl || "/placeholder.svg";
    questionEl.innerHTML = q.questionText || card.questionText || "Bu görsel hangi kelimeyi ifade ediyor?";

    // Temizlik
    feedbackEl.textContent = "";
    optionsContainer.innerHTML = "";

    // Seçenek butonları
    letters.forEach((letter, i) => {
        const optKey = `option${letter}`;
        const text = q[optKey] || card[optKey] || `Şık ${letter}`;
        const btn = document.createElement("button");
        btn.textContent = text;
        btn.className = [
            "bg-gradient-to-r", "from-orange-500", "to-orange-600",
            "text-white", "px-6", "py-2", "rounded-xl", "font-bold",
            "hover:from-orange-600", "hover:to-orange-700",
            "transition-all", "duration-200", "shadow-lg",
            "hover:shadow-xl", "transform", "hover:scale-105",
            "text-center", "whitespace-normal"
        ].join(" ");
        btn.onclick = () => submit(studentId, gameId, i + 1);
        optionsContainer.append(btn);
    });

    start = Date.now();
}

function submit(studentId, gameId, selectedOption) {
    const card = cards[idx];
    const q = card.gameQuestions?.find(g => g.gameId === Number(gameId)) || {};
    const correctOption = q.correctOption ?? card.correctOption;
    const success = selectedOption === correctOption;
    const duration = (Date.now() - start) / 1000;

    feedbackEl.textContent = success
        ? "🎉 Tebrikler, doğru bildiniz!"
        : "❌ Maalesef, yanlış bildiniz.";

    awardScore(studentId, gameId, success, duration);

    if (success) {
        cards.splice(idx, 1);
    }

    if (cards.length === 0) {
        notifyParent();
        return;
    }

    setTimeout(() => {
        if (!success) {
            idx = (idx + 1) % cards.length;
        } else {
            if (idx >= cards.length) idx = 0;
        }

        loadCard(studentId, gameId);
    }, 800); // 0.8 saniye gecikme, istersen 0ms de yapabilirsin
}


export async function initVisualPrompt(studentId, gameId, single, wordId) {
    if (single) {
        if (wordId) {
            const card = await fetchWordCardWithQuestions(wordId);
            cards = card ? [card] : [{
                optionA: "", optionB: "", optionC: "", optionD: "",
                correctOption: 1, gameQuestions: []
            }];
        } else {
            cards = [{
                optionA: "", optionB: "", optionC: "", optionD: "",
                correctOption: 1, gameQuestions: []
            }];
        }
        singleMode = true;
    } else {
        cards = await fetchLearnedWords(studentId) || [];
        if (!cards.length) {
            cards = [{
                optionA: "", optionB: "", optionC: "", optionD: "",
                correctOption: 1, gameQuestions: []
            }];
        }
        singleMode = false;
    }

    // DOM elemanları
    imgEl = document.getElementById("vpImage");
    questionEl = document.getElementById("vpQuestion");
    feedbackEl = document.getElementById("vpFeedback");
    optionsContainer = document.getElementById("vpOptions");

    // Geri Dön / Sonraki
    const nextBtn = document.getElementById("vpNext");
    const backBtn = document.getElementById("vpBack");
    nextBtn.onclick = () => {
        idx = (idx + 1) % cards.length;
        loadCard(studentId, gameId);
    };
    backBtn.onclick = () => {
        idx = (idx - 1 + cards.length) % cards.length;
        loadCard(studentId, gameId);
    };

    if (singleMode) {
        nextBtn.style.display = "none";
        backBtn.style.display = "none";
    }

    loadCard(studentId, gameId);
}

function notifyParent() {
    if (window.parent !== window) window.parent.postMessage("next-game", "*");
}
