import {
  fetchLearnedWords,
  awardScore,
  fetchWordCardWithQuestions
} from "./common.js";

const alphabet = [
  'a','b','c','ç','d','e','f','g','ğ','h',
  'ı','i','j','k','l','m','n','o','ö','p',
  'r','s','ş','t','u','ü','v','y','z'
];

let targetWord = "",
    currentWord = [],
    availableLetters = [];
let cards = [],
    idx = 0,
    singleMode = false,
    startTime;

export async function initBubbleLetters(studentId, gameId, single, wordId) {
  // Kartları yükle
  if (single && wordId) {
    const card = await fetchWordCardWithQuestions(wordId);
    cards = card ? [card] : [{ word: single, gameQuestions: [] }];
  } else if (single) {
    cards = [{ word: single, gameQuestions: [] }];
  } else {
    cards = await fetchLearnedWords(studentId);
    if (!cards.length) {
      cards = [{ word: "kelime", gameQuestions: [] }];
    }
  }

  idx = 0;
  setupWord();

  document.getElementById("blClear").onclick = clearWord;
  document.getElementById("blSubmit").onclick = checkWord;
  document.getElementById("blReveal").onclick = revealAnswer;
  document.getElementById("blNext").onclick = nextWord;
  document.getElementById("blBack").onclick = () => window.history.back();

  document.addEventListener("keydown", (e) => {
    if (/^[a-zA-Z]$/.test(e.key)) {
      selectLetter(e.key.toLocaleLowerCase('tr'));
    } else if (e.key === "Backspace") {
      e.preventDefault();
      removeLastLetter();
    } else if (e.key === "Enter") {
      e.preventDefault();
      checkWord();
    }
  });
}

function setupWord() {
  const gid = Number(document.getElementById("gameRoot").dataset.gameId);
  const card = cards[idx];
  const q = card.gameQuestions?.find((g) => g.gameId === gid);

  // Hedef kelime
  targetWord = (q?.answerText || card.word).toLocaleLowerCase('tr');
  currentWord = Array(targetWord.length).fill("");
  availableLetters = alphabet.map((l) => ({ letter: l, used: false }));
  shuffle(availableLetters);
  startTime = Date.now();

  // Soru ve görseli güncelle
  const imgEl = document.getElementById("questionImage");
  const txtEl = document.getElementById("questionText");
  if (q?.imageUrl) {
    imgEl.src = q.imageUrl;
    imgEl.style.display = "";
  } else {
    imgEl.style.display = "none";
  }
  txtEl.textContent = q?.questionText || "";

  createTargetSlots();
  createBubbles();
  document.getElementById("blFeedback").innerHTML = "";
}

function shuffle(arr) {
  for (let i = arr.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [arr[i], arr[j]] = [arr[j], arr[i]];
  }
}

function createTargetSlots() {
  const container = document.getElementById("targetSlots");
  container.innerHTML = "";
  for (let i = 0; i < targetWord.length; i++) {
    const slot = document.createElement("div");
    slot.className =
        "bl-letter" + (currentWord[i] ? " filled-clickable" : "");
    slot.textContent = currentWord[i] || "";
    if (currentWord[i]) slot.onclick = () => removeLetterAt(i);
    container.appendChild(slot);
  }
}

function createBubbles() {
  const container = document.getElementById("bubblesArea");
  container.innerHTML = "";
  availableLetters.forEach((obj) => {
    const btn = document.createElement("button");
    btn.className = "bubble-btn" + (obj.used ? " used" : "");
    btn.textContent = obj.letter.toLocaleUpperCase('tr');
    btn.onclick = () => selectLetter(obj.letter);
    container.appendChild(btn);
  });
}

function selectLetter(letter) {
  const idx0 = currentWord.findIndex((l) => !l);
  if (idx0 < 0) return;
  currentWord[idx0] = letter;
  availableLetters.find((o) => o.letter === letter && !o.used).used = true;
  createTargetSlots();
  createBubbles();
}

function removeLetterAt(i) {
  const l = currentWord[i];
  if (!l) return;
  currentWord[i] = "";
  availableLetters.find((o) => o.letter === l && o.used).used = false;
  createTargetSlots();
  createBubbles();
}

function removeLastLetter() {
  const filled = currentWord
      .map((l,i) => (l ? i : -1))
      .filter((i) => i >= 0);
  if (!filled.length) return;
  removeLetterAt(filled.pop());
}

function clearWord() {
  currentWord.fill("");
  availableLetters.forEach((o) => (o.used = false));
  createTargetSlots();
  createBubbles();
}

function checkWord() {
  const guess = currentWord.join("");
  const correct = guess.toLocaleLowerCase('tr') === targetWord;
  const duration = (Date.now() - startTime) / 1000;
  const gid = Number(
      document.getElementById("gameRoot").dataset.gameId
  );
  const sid = document.getElementById("gameRoot").dataset.studentId;
  awardScore(sid, gid, correct, duration);

  const fb = document.getElementById("blFeedback");
  fb.innerHTML = correct
      ? '<span class="text-green-600">🎉 Doğru!</span>'
      : '<span class="text-red-600">❌ Yanlış!</span>';
  if (correct) setTimeout(nextWord, 2000);
  else setTimeout(() => (fb.innerHTML = ""), 2000);
}

function revealAnswer() {
  currentWord = targetWord.split("");
  availableLetters.forEach((o) => (o.used = true));
  createTargetSlots();
  createBubbles();
  const fb = document.getElementById("blFeedback");
  fb.innerHTML =
      '<span class="text-blue-600">💡 Cevap gösterildi!</span>';
  setTimeout(nextWord, 2000);
}

function nextWord() {
  idx = (idx + 1) % cards.length;
  setupWord();
}
