import { fetchLearnedWords, awardScore, fetchWordCardWithQuestions } from "./common.js"

// Tam alfabeyi oluştur
const alphabet = 'abcdefghijklmnopqrstuvwxyz'.split('');

let targetWord = "",
    currentWord = [],
    availableLetters = [];
let cards = [],
    idx = 0,
    singleMode = false,
    startTime;

function shuffle(arr) {
  for (let i = arr.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [arr[i], arr[j]] = [arr[j], arr[i]];
  }
}

function createTargetSlots() {
  const container = document.getElementById("targetSlots");
  if (!container) return;
  container.innerHTML = "";

  for (let i = 0; i < targetWord.length; i++) {
    const slot = document.createElement("div");
    slot.className = "bl-letter" + (currentWord[i] ? " filled-clickable" : "");
    slot.textContent = currentWord[i] || "";
    if (currentWord[i]) {
      slot.classList.add("filled");
      // Tekrar tıklanınca harfi geri gönder
      slot.onclick = () => removeLetterAt(i);
    }
    container.appendChild(slot);
  }
}

function createBubbles() {
  const container = document.getElementById("bubblesArea");
  if (!container) return;
  container.innerHTML = "";

  availableLetters.forEach((obj) => {
    const bubble = document.createElement("button");
    bubble.className = "bubble-btn" + (obj.used ? " used" : "");
    bubble.textContent = obj.letter.toUpperCase();
    bubble.onclick = () => selectLetter(obj.letter);
    container.appendChild(bubble);
  });
}

function selectLetter(letter) {
  const emptyIndex = currentWord.findIndex((l) => !l);
  if (emptyIndex === -1) return;

  currentWord[emptyIndex] = letter;
  const obj = availableLetters.find(o => o.letter === letter && !o.used);
  if (obj) obj.used = true;

  createTargetSlots();
  createBubbles();
}

function removeLetterAt(slotIndex) {
  const letter = currentWord[slotIndex];
  if (!letter) return;
  currentWord[slotIndex] = "";
  const obj = availableLetters.find(o => o.letter === letter && o.used);
  if (obj) obj.used = false;
  createTargetSlots();
  createBubbles();
}

function removeLastLetter() {
  const filled = currentWord
      .map((l, i) => l ? i : -1)
      .filter(i => i !== -1);
  if (!filled.length) return;
  const lastIdx = filled[filled.length - 1];
  removeLetterAt(lastIdx);
}

function clearWord() {
  currentWord = new Array(targetWord.length).fill("");
  availableLetters.forEach(o => o.used = false);
  createTargetSlots();
  createBubbles();
}

function checkWord() {
  const formed = currentWord.join("");
  const isCorrect = formed.toLowerCase() === targetWord.toLowerCase();
  const duration = (Date.now() - startTime) / 1000;
  const gid = document.getElementById("gameRoot").dataset.gameId;
  const sid = document.getElementById("gameRoot").dataset.studentId;

  awardScore(sid, gid, isCorrect, duration);

  const feedbackEl = document.getElementById("blFeedback");
  if (feedbackEl) {
    if (isCorrect) {
      feedbackEl.innerHTML = '<span class="text-green-600">🎉 Tebrikler! Doğru kelimeyi oluşturdun!</span>';
      setTimeout(() => nextWord(), 2000);
    } else {
      feedbackEl.innerHTML = '<span class="text-red-600">❌ Yanlış! Tekrar dene.</span>';
      setTimeout(() => feedbackEl.innerHTML = "", 2000);
    }
  }
}

function revealAnswer() {
  currentWord = targetWord.split("");
  availableLetters.forEach(o => o.used = true);
  createTargetSlots();
  createBubbles();

  const feedbackEl = document.getElementById("blFeedback");
  if (feedbackEl) {
    feedbackEl.innerHTML = '<span class="text-blue-600">💡 Doğru cevap gösterildi!</span>';
    setTimeout(() => nextWord(), 2000);
  }
}

function nextWord() {
  if (!cards.length) return;
  idx = (idx + 1) % cards.length;
  setupWord();
}

function updateHints() {
  const card = cards[idx];
  const isEmbed = document.getElementById("gameRoot").dataset.embed === "true";

  if (isEmbed) {
    const hintEl = document.getElementById("hintText");
    if (hintEl) hintEl.textContent = card.synonym || card.meaning || "İpucu yok";
  } else {
    document.getElementById("hintSynonym").textContent = card.synonym || "Belirtilmemiş";
    document.getElementById("hintDefinition").textContent = card.meaning || "Belirtilmemiş";
    document.getElementById("hintSentence").textContent = card.exampleSentence || "Belirtilmemiş";
  }
}

function setupWord() {
  const card = cards[idx];
  const gid = Number(document.getElementById("gameRoot").dataset.gameId);
  const q = card.gameQuestions?.find(g => g.gameId === gid);

  targetWord = (q?.answerText || card.word).toLowerCase();
  currentWord = new Array(targetWord.length).fill("");
  availableLetters = alphabet.map(l => ({ letter: l, used: false }));
  shuffle(availableLetters);
  startTime = Date.now();
  updateHints();
  createTargetSlots();
  createBubbles();
  document.getElementById("blFeedback").innerHTML = "";
}

export async function initBubbleLetters(studentId, gameId, single, wordId) {
  if (single) {
    if (wordId) {
      const card = await fetchWordCardWithQuestions(wordId);
      cards = card ? [card] : [{ word: single, synonym: "", meaning: "", exampleSentence: "" }];
    } else {
      cards = [{ word: single, synonym: "", meaning: "", exampleSentence: "" }];
    }
    singleMode = true;
  } else {
    cards = await fetchLearnedWords(studentId);
    if (!cards.length) {
      cards = [{ word: "kelime", synonym: "sözcük", meaning: "Anlamlı ses birliği", exampleSentence: "Bu bir kelime örneğidir." }];
    }
    singleMode = false;
  }

  idx = 0;
  setupWord();

  document.getElementById("blClear").onclick = clearWord;
  document.getElementById("blSubmit").onclick = checkWord;
  document.getElementById("blReveal").onclick = revealAnswer;
  document.getElementById("blNext").onclick = nextWord;
  document.getElementById("blBack").onclick = () => window.history.back();

  document.addEventListener('keydown', (e) => {
    if (/^[a-zA-Z]$/.test(e.key)) {
      selectLetter(e.key.toLowerCase());
    } else if (e.key === 'Backspace') {
      e.preventDefault();
      removeLastLetter();
    } else if (e.key === 'Enter') {
      e.preventDefault();
      checkWord();
    }
  });
}
