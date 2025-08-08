import { fetchLearnedWords, awardScore, fetchWordCardWithQuestions } from "./common.js"

let cards = [],
    idx = 0,
    start
let singleMode = false

function load() {
  document.getElementById("kbFeedback").innerHTML = "";
  const card = cards[idx]
  const gid = Number.parseInt(document.getElementById("gameRoot").dataset.gameId)
  const q = card.gameQuestions?.find((g) => g.gameId === gid)

  document.getElementById("questionBox").innerHTML =
      `<div class="text-xl text-orange-800 font-bold">${q?.questionText || card.definition}</div>`

  const opts = document.getElementById("optionsArea")
  opts.innerHTML = ""

  let answers = []
  let correct = ""

  if (q && (q.optionA || q.optionB || q.optionC || q.optionD)) {
    answers = [q.optionA, q.optionB, q.optionC, q.optionD].filter(Boolean)
    const idxCorrect = q.correctOption ? q.correctOption - 1 : 0
    correct = answers[idxCorrect] || answers[0]
  } else {
    correct = q?.answerText || card.word
    answers = [correct, ...cards.slice(idx + 1, idx + 4).map((c) => c.word)]
    while (answers.length < 4) answers.push(card.word)
  }

  for (let i = answers.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1))
    ;[answers[i], answers[j]] = [answers[j], answers[i]]
  }

  answers.forEach(ans => {
    const b = document.createElement("button");
    // Düzeltilmiş buton stilleri - daha okunabilir
    b.className = "option-btn bg-gradient-to-r from-orange-100 to-orange-200 text-orange-800 font-bold py-4 px-6 rounded-xl border-2 border-orange-300 hover:from-orange-200 hover:to-orange-300 hover:border-orange-400 transition-all duration-300 shadow-lg hover:shadow-xl transform hover:scale-105 w-full text-left";
    b.textContent = ans;
    b.onclick = () => select(b, ans, correct);
    opts.appendChild(b);
  });

  start = Date.now()
}

function select(buttonEl, ans, correct) {
  const ok = ans === correct;

  // Tüm butonları devre dışı bırak
  document.querySelectorAll('.option-btn').forEach(btn => {
    btn.disabled = true;
    btn.classList.add('opacity-50', 'cursor-not-allowed');
  });

  if (ok) {
    // Doğruysa seçilen butona correct sınıfını ekle
    buttonEl.classList.add('correct');
  } else {
    // Yanlışsa seçilen butona wrong sınıfını ekle
    buttonEl.classList.add('wrong');

    // Doğru cevabı gösterirken correct sınıfını ekle
    document.querySelectorAll('.option-btn').forEach(btn => {
      if (btn.textContent === correct) {
        btn.disabled = false;   // istersen kaldırabilirsin
        btn.classList.remove('opacity-50', 'cursor-not-allowed');
        btn.classList.add('correct');
      }
    });
  }

  // Geri bildirimi göster
  const fb = document.getElementById("kbFeedback");
  fb.innerHTML = ok
      ? '<span class="text-green-600">🎉 Tebrikler! Doğru cevap!</span>'
      : '<span class="text-red-600">❌ Yanlış! Doğru cevap gösterildi.</span>';

  setTimeout(() => {
        fb.innerHTML = "";
      }, 2000);
  
  const duration = (Date.now() - start) / 1000;
  const gid = document.getElementById("gameRoot").dataset.gameId;
  awardScore(document.getElementById("gameRoot").dataset.studentId, gid, ok, duration);

  // Kartı listeden çıkar ve sonraki karta geç
  cards.splice(idx, 1);
  if (cards.length === 0) {
    notifyParent();
    return;
  }
  if (idx >= cards.length) idx = 0;
  setTimeout(load, 2000);
}


export async function initWordQuiz(studentId, gameId, single, wordId) {
  if (single) {
    if (wordId) {
      const card = await fetchWordCardWithQuestions(wordId)
      cards = card ? [card] : [{ word: single, definition: "", synonym: "", exampleSentence: "" }]
    } else {
      cards = [{ word: single, definition: "", synonym: "", exampleSentence: "" }]
    }
    singleMode = true
  } else {
    cards = await fetchLearnedWords(studentId)
    if (cards.length === 0) cards = [{ word: "örnek", definition: "örnek tanım" }]
    singleMode = false
  }

  const endBtn = document.getElementById("kbEndGame")
  if (endBtn) {
    endBtn.onclick = () => {
      window.location = endBtn.dataset.home
    }
    if (singleMode) endBtn.style.display = "none"
  }

  load()
}

function notifyParent() {
  if (window.parent !== window) window.parent.postMessage("next-game", "*")
}