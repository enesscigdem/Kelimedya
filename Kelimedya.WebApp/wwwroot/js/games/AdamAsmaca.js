import { fetchLearnedWords, awardScore, fetchWordCardWithQuestions } from "./common.js";

let word = "",
    guessed = new Set(),
    wrong = 0,
    startTime = 0,
    cards = [],
    idx = 0;
let singleMode = false;
let keyListener = null;

const TURKISH_ALPHABET = [
  "A","B","C","Ç","D","E","F","G","Ğ","H",
  "I","İ","J","K","L","M","N","O","Ö","P",
  "R","S","Ş","T","U","Ü","V","Y","Z"
];

function createLetterButtons() {
  const container = document.getElementById("letterButtons");
  if (!container) return;
  container.innerHTML = "";

  TURKISH_ALPHABET.forEach(letter => {
    const btn = document.createElement("button");
    btn.className = "letter-btn";
    btn.textContent = letter;
    btn.setAttribute("data-letter", letter);
    btn.onclick = () =>
        handleGuess(
            letter.toLowerCase(),
            document.getElementById("gameRoot").dataset.studentId,
            document.getElementById("gameRoot").dataset.gameId,
            btn
        );
    container.appendChild(btn);
  });
}

function drawHangman() {
  const svg = document.getElementById("hangmanSvg");
  if (!svg) return;
  svg.innerHTML = "";

  // Base, pole, beam, noose always drawn
  const drawLine = (x1,y1,x2,y2,color) => {
    const ln = document.createElementNS("http://www.w3.org/2000/svg","line");
    ln.setAttribute("x1",x1); ln.setAttribute("y1",y1);
    ln.setAttribute("x2",x2); ln.setAttribute("y2",y2);
    ln.setAttribute("stroke",color); ln.setAttribute("stroke-width","3");
    svg.appendChild(ln);
  };
  drawLine(10, svg.getAttribute("height")-10, 150, svg.getAttribute("height")-10, "#ea580c");
  drawLine(40, svg.getAttribute("height")-10, 40, 20, "#ea580c");
  drawLine(40, 20, 120, 20, "#ea580c");
  drawLine(120, 20, 120, 40, "#ea580c");

  if (wrong > 0) {
    const head = document.createElementNS("http://www.w3.org/2000/svg","circle");
    head.setAttribute("cx","120"); head.setAttribute("cy","60");
    head.setAttribute("r","20"); head.setAttribute("stroke","#dc2626");
    head.setAttribute("stroke-width","3"); head.setAttribute("fill","none");
    svg.appendChild(head);
  }
  if (wrong > 1) drawLine(120,80,120,150,"#dc2626");
  if (wrong > 2) drawLine(120,100,90,130,"#dc2626");
  if (wrong > 3) drawLine(120,100,150,130,"#dc2626");
  if (wrong > 4) drawLine(120,150,90,180,"#dc2626");
  if (wrong > 5) drawLine(120,150,150,180,"#dc2626");
}

function updateDisplay() {
  const display = word
      .split("")
      .map(ch => (guessed.has(ch) ? ch.toUpperCase() : "_"))
      .join(" ");
  document.getElementById("wordDisplay").textContent = display;

  const wrongLetters = Array.from(guessed)
      .filter(ch => !word.includes(ch))
      .map(ch => ch.toUpperCase())
      .join(" ");
  document.getElementById("wrongLetters").textContent = wrongLetters;

  drawHangman();
}

function updateHints() {
  const card = cards[idx];
  document.getElementById("hintSynonym").textContent = card.synonym || "Belirtilmemiş";
  document.getElementById("hintDefinition").textContent = card.meaning || "Belirtilmemiş";
  document.getElementById("hintSentence").textContent = card.exampleSentence || "Belirtilmemiş";
}

function handleGuess(ch, studentId, gameId, btn) {
  if (guessed.has(ch)) return;
  guessed.add(ch);

  if (word.includes(ch)) {
    btn.classList.add("correct");
  } else {
    wrong++;
    btn.classList.add("wrong");
  }
  btn.disabled = true;

  updateDisplay();

  // Win?
  if (word.split("").every(c => guessed.has(c))) {
    const duration = (Date.now() - startTime) / 1000;
    awardScore(studentId, gameId, true, duration);
    document.getElementById("aaFeedback").innerHTML =
        '<span class="text-green-600">🎉 Tebrikler! Kelimeyi buldun!</span>';
    cleanupAndNext(studentId, gameId);
    return;
  }

  // Lose?
  if (wrong >= 6) {
    const duration = (Date.now() - startTime) / 1000;
    awardScore(studentId, gameId, false, duration);
    document.getElementById("aaFeedback").innerHTML =
        `<span class="text-red-600">😞 Kaybettin! Kelime: <strong>${word.toUpperCase()}</strong></span>`;
    cleanupAndNext(studentId, gameId);
  }
}

function cleanupAndNext(studentId, gameId) {
  // remove key listener
  if (keyListener) {
    document.removeEventListener("keydown", keyListener);
    keyListener = null;
  }
  setTimeout(() => {
    if (window.parent !== window) {
      window.parent.postMessage("next-game", "*");
    } else {
      startNext(studentId, gameId);
    }
  }, 2000);
}

function startNext(studentId, gameId) {
  idx = (idx + 1) % cards.length;
  setup(studentId, gameId);
}

function setup(studentId, gameId) {
  // reset
  guessed.clear();
  wrong = 0;
  startTime = Date.now();
  document.getElementById("aaFeedback").innerHTML = "";

  // pick word
  const card = cards[idx];
  const q = card.gameQuestions?.find(g => g.gameId === Number(gameId));
  word = (q?.answerText || card.word).toLowerCase();

  updateDisplay();
  updateHints();
  createLetterButtons();

  // reveal button
  const revealBtn = document.getElementById("aaReveal");
  revealBtn.onclick = () => {
    document.getElementById("aaFeedback").innerHTML =
        `<span class="text-blue-600">💡 Cevap: <strong>${word.toUpperCase()}</strong></span>`;
    cleanupAndNext(studentId, gameId);
  };

  // key listener
  if (keyListener) {
    document.removeEventListener("keydown", keyListener);
  }
  keyListener = e => {
    const k = e.key.toUpperCase();
    if (TURKISH_ALPHABET.includes(k)) {
      const btn = document.querySelector(
          `#letterButtons button[data-letter="${k}"]`
      );
      handleGuess(k.toLowerCase(), studentId, gameId, btn);
    }
  };
  document.addEventListener("keydown", keyListener);
}

export async function initHangman(studentId, gameId, single, wordId) {
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
      cards = [{
        word: "kelime",
        synonym: "sözcük",
        meaning: "Bir dili oluşturan anlamlı ses birliği",
        exampleSentence: "Bu kelime çok güzel."
      }];
    }
    singleMode = false;
  }

  idx = 0;
  setup(studentId, gameId);
}
