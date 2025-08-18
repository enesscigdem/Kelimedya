// BubbleLetters.js
import {
  fetchLearnedWords,
  awardScore,
  fetchWordCardWithQuestions
} from "./common.js";

const alphabet = [
  "a","b","c","ç","d","e","f","g","ğ","h",
  "ı","i","j","k","l","m","n","o","ö","p",
  "r","s","ş","t","u","ü","v","y","z"
];

let targetWord = "",
    currentWord = [],
    availableLetters = [];
let cards = [],
    idx = 0,
    singleMode = false,
    startTime = 0;

let keyListener = null; // 👈 Adam Asmaca’daki gibi tek noktadan yönetilecek
let answered   = false; // aynı sorudan tekrar puan alınmasını engelle

// === Embed (popup) tespiti ve tek çıkış noktası ===
function isEmbedded() {
  const root = document.getElementById("gameRoot");
  return (root && root.dataset.embed === "true") || (window.parent !== window);
}

function proceedNext(delay = 900) {
  setTimeout(() => {
    // klavye dinleyicisini güvenle kaldır
    if (keyListener) {
      document.removeEventListener("keydown", keyListener);
      keyListener = null;
    }
    if (isEmbedded()) {
      // popup akışı: üst sayfaya haber ver
      window.parent.postMessage("next-game", "*");
    } else {
      // normal sayfa: sıradaki kelime
      nextWord();
    }
  }, delay);
}

// === INIT ===
export async function initBubbleLetters(studentId, gameId, single, wordId) {
  if (single && wordId) {
    const card = await fetchWordCardWithQuestions(wordId);
    cards = card ? [card] : [{ word: single, gameQuestions: [] }];
    singleMode = true;
  } else if (single) {
    cards = [{ word: single, gameQuestions: [] }];
    singleMode = true;
  } else {
    cards = await fetchLearnedWords(studentId);
    if (!cards.length) {
      cards = [{ word: "kelime", gameQuestions: [] }];
    }
    singleMode = false;
  }

  idx = 0;
  setupWord();

  // Butonlar
  const clearBtn  = document.getElementById("blClear");
  const submitBtn = document.getElementById("blSubmit");
  const revealBtn = document.getElementById("blReveal");
  const nextBtn   = document.getElementById("blNext");
  const backBtn   = document.getElementById("blBack");

  if (clearBtn)  clearBtn.onclick  = clearWord;
  if (submitBtn) submitBtn.onclick = checkWord;
  if (revealBtn) revealBtn.onclick = revealAnswer;

  // Popup ise “Sonraki” butonunu gizle; değilse normal çalışsın
  if (nextBtn) {
    if (isEmbedded()) nextBtn.style.display = "none";
    else nextBtn.onclick = nextWord;
  }
  if (backBtn) backBtn.onclick = () => window.history.back();
}

// === Yardımcılar ===
function normalize(s) {
  return (s || "").toLocaleLowerCase("tr")
  .replace(/\s+/g, "");
}

function shuffle(arr) {
  for (let i = arr.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [arr[i], arr[j]] = [arr[j], arr[i]];
  }
  return arr;
}

// === Oyun akışı ===
function setupWord() {
  const gid = Number(document.getElementById("gameRoot").dataset.gameId);
  const card = cards[idx];
  const q = card.gameQuestions?.find(g => g.gameId === gid);

  // Hedef kelime
  targetWord = normalize(q?.answerText || card.word);
  currentWord = Array(targetWord.length).fill("");

  // Baloncuklar: sınırsız kullanım, yalnızca alfabeti karıştır
  availableLetters = shuffle(alphabet.slice());

  startTime = Date.now();
  answered = false;
  enableButtons();

  // Soru & görsel
  const imgEl = document.getElementById("questionImage");
  const txtEl = document.getElementById("questionText");
  if (q?.imageUrl) { imgEl.src = q.imageUrl; imgEl.style.display = ""; }
  else { imgEl.style.display = "none"; }
  txtEl.textContent = q?.questionText || "";

  // UI
  createTargetSlots();
  createBubbles();
  const fb = document.getElementById("blFeedback");
  if (fb) fb.innerHTML = "";

  // Klavye dinleyicisi — Adam Asmaca’daki gibi önce kaldır, sonra ekle
  if (keyListener) {
    document.removeEventListener("keydown", keyListener);
    keyListener = null;
  }
  keyListener = (e) => {
    const k = e.key;
    if (k.length === 1 && alphabet.includes(k.toLocaleLowerCase("tr"))) {
      selectLetter(k.toLocaleLowerCase("tr"));
    } else if (k === "Backspace") {
      e.preventDefault();
      removeLastLetter();
    } else if (k === "Enter") {
      e.preventDefault();
      checkWord();
    }
  };
  document.addEventListener("keydown", keyListener);
}

function enableButtons() {
  document.querySelectorAll('#blClear,#blSubmit,#blReveal').forEach(b => { if (b) b.disabled = false; });
}

function disableButtons() {
  document.querySelectorAll('#blClear,#blSubmit,#blReveal').forEach(b => { if (b) b.disabled = true; });
}

function autoAdvanceIfCorrect() {
  // tüm slotlar dolmadan kontrol etme
  if (currentWord.some(l => !l)) return;

  const guess = currentWord.join("");
  if (guess.toLocaleLowerCase("tr") !== targetWord) return;

  const gid = Number(document.getElementById("gameRoot").dataset.gameId);
  const sid = document.getElementById("gameRoot").dataset.studentId;
  const duration = (Date.now() - startTime) / 1000;

  awardScore(sid, gid, true, duration);

  const fb = document.getElementById("blFeedback");
  if (fb) fb.innerHTML = '<span class="text-green-600">🎉 Tebrikler, doğru bildiniz!</span>';

  // Adam Asmaca’daki gibi: embed ise üst sayfaya haber ver, değilse sonraki kelime
  proceedNext(800);
}

function nextWord() {
  idx = (idx + 1) % cards.length;
  setupWord();
}

// === UI üretimi ===
function createTargetSlots() {
  const container = document.getElementById("targetSlots");
  container.innerHTML = "";
  for (let i = 0; i < targetWord.length; i++) {
    const filled = Boolean(currentWord[i]);
    const slot = document.createElement("div");
    slot.className = "bl-letter" + (filled ? " filled filled-clickable" : "");
    slot.textContent = currentWord[i] || "";
    if (filled) slot.onclick = () => removeLetterAt(i);
    container.appendChild(slot);
  }
}

function createBubbles() {
  const container = document.getElementById("bubblesArea");
  container.innerHTML = "";
  availableLetters.forEach((l) => {
    const btn = document.createElement("button");
    btn.className = "bubble-btn";
    btn.textContent = l.toLocaleUpperCase("tr");
    btn.onclick = () => selectLetter(l);
    container.appendChild(btn);
  });
}

// === Etkileşimler ===
function selectLetter(letter) {
  const i = currentWord.findIndex((l) => !l);
  if (i < 0) return; // doluysa görmezden gel
  currentWord[i] = letter;

  createTargetSlots();
  createBubbles();

  autoAdvanceIfCorrect(); // doluysa ve doğruysa otomatik geç
}

function removeLetterAt(i) {
  if (!currentWord[i]) return;
  currentWord[i] = "";
  createTargetSlots();
  createBubbles();
}

function removeLastLetter() {
  const filledIdx = currentWord
      .map((l, i) => (l ? i : -1))
      .filter(i => i >= 0);
  if (!filledIdx.length) return;
  removeLetterAt(filledIdx.pop());
}

function clearWord() {
  currentWord.fill("");
  createTargetSlots();
  createBubbles();
}

function checkWord() {
  if (answered) return;
  answered = true;
  disableButtons();
  if (keyListener) {
    document.removeEventListener("keydown", keyListener);
    keyListener = null;
  }

  const guess = currentWord.join("");
  const correct = guess.toLocaleLowerCase("tr") === targetWord;
  const duration = (Date.now() - startTime) / 1000;
  const gid = Number(document.getElementById("gameRoot").dataset.gameId);
  const sid = document.getElementById("gameRoot").dataset.studentId;

  awardScore(sid, gid, correct, duration, targetWord, true);

  const fb = document.getElementById("blFeedback");
  if (correct) {
    if (fb) fb.innerHTML = '<span class="text-green-600">🎉 Doğru!</span>';
    proceedNext(800);
  } else {
    currentWord = targetWord.split("");
    createTargetSlots();
    if (fb) fb.innerHTML = `<span class="text-red-600">❌ Yanlış! Doğru: ${targetWord}</span>`;
    setTimeout(() => { proceedNext(); }, 2000);
  }
}

function revealAnswer() {
  if (answered) return;
  answered = true;
  disableButtons();
  if (keyListener) {
    document.removeEventListener("keydown", keyListener);
    keyListener = null;
  }
  const gid = Number(document.getElementById("gameRoot").dataset.gameId);
  const sid = document.getElementById("gameRoot").dataset.studentId;
  const duration = (Date.now() - startTime) / 1000;
  awardScore(sid, gid, false, duration, targetWord, false);

  currentWord = targetWord.split("");
  createTargetSlots();
  createBubbles();

  const fb = document.getElementById("blFeedback");
  if (fb) fb.innerHTML = '<span class="text-blue-600">💡 Cevap gösterildi!</span>';

  setTimeout(() => { proceedNext(); }, 2000);
}
