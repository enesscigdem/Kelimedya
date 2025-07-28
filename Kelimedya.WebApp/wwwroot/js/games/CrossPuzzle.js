// CrossPuzzle.js
import { awardScore, fetchLearnedWords } from './common.js';

const PAGE_SIZE = 5;
let start;
let offset = 0;
let currentBatch = [];
const puzzle = { size: 0, across: [], down: [] };

// ---- Random yardımcıları
function shuffleInPlace(arr) {
  for (let i = arr.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [arr[i], arr[j]] = [arr[j], arr[i]];
  }
  return arr;
}
function pickRandom(arr) {
  return arr[Math.floor(Math.random() * arr.length)];
}

// ---- TEK loadBatch (karıştırılmış)
async function loadBatch(studentId, gameId) {
  const cards = await fetchLearnedWords(studentId);
  if (cards.length === 0) {
    currentBatch = [];
  } else {
    const slice = cards.slice(offset, offset + PAGE_SIZE);
    if (slice.length < PAGE_SIZE) {
      slice.push(...cards.slice(0, PAGE_SIZE - slice.length));
    }
    currentBatch = slice.map(card => {
      const qa = card.gameQuestions?.find(g => g.gameId === Number(gameId));
      const raw = (qa?.answerText || card.word || '').toUpperCase('tr');
      return { ...card, answerRaw: raw.replace(/\s+/g, '') };
    });
    shuffleInPlace(currentBatch); // her yüklemede farklı sıra
  }
}

function placeCrosswords() {
  puzzle.across = [];
  puzzle.down = [];
  if (currentBatch.length === 0) return;

  // İlk kelime rastgele yön
  const first = currentBatch[0].answerRaw;
  const anchorRow = 10, anchorCol = 10;
  const firstDir = Math.random() < 0.5 ? 'across' : 'down';

  const firstItem = { num: 1, row: anchorRow, col: anchorCol, answer: first, clue: extractClue(0), dir: firstDir };
  (firstDir === 'across' ? puzzle.across : puzzle.down).push(firstItem);

  let placed = [{ dir: firstDir, word: first, row: anchorRow, col: anchorCol }];
  let nextNum = 2;

  for (let i = 1; i < currentBatch.length; i++) {
    const word = currentBatch[i].answerRaw;
    const clue = extractClue(i);
    let chosen = null;
    const candidates = [];

    for (const p of placed) {
      for (let a = 0; a < p.word.length; a++) {
        for (let b = 0; b < word.length; b++) {
          if (p.word[a] !== word[b]) continue;

          let row, col, dir;
          if (p.dir === 'across') { dir = 'down'; row = p.row - b; col = p.col + a; }
          else { dir = 'across'; row = p.row + a; col = p.col - b; }
          if (row < 1 || col < 1) continue;
          if (!conflicts(row, col, dir, word)) candidates.push({ row, col, dir });
        }
      }
    }

    if (candidates.length) {
      chosen = pickRandom(candidates);
    } else {
      // Kesişim yoksa, yanına rastgele yönle ekle
      const B = getBounds();
      const dir = Math.random() < 0.5 ? 'across' : 'down';
      let row = dir === 'across' ? (B.maxRow || anchorRow) + 2 : Math.max(1, B.minRow);
      let col = dir === 'across' ? Math.max(1, B.minCol) : (B.maxCol || anchorCol) + 2;

      let tries = 0;
      while (conflicts(row, col, dir, word) && tries < 60) {
        if (dir === 'across') row++; else col++;
        tries++;
      }
      chosen = { row, col, dir };
    }

    const item = { num: nextNum, row: chosen.row, col: chosen.col, answer: word, clue, dir: chosen.dir };
    (chosen.dir === 'across' ? puzzle.across : puzzle.down).push(item);
    placed.push({ dir: chosen.dir, word, row: chosen.row, col: chosen.col });
    nextNum++;
  }

  normalizePuzzle();
  const { maxRow, maxCol } = getBounds();
  puzzle.size = Math.max(maxRow, maxCol) + 2;

  puzzle.across.sort((a,b)=>a.num-b.num);
  puzzle.down.sort((a,b)=>a.num-b.num);
}

// Çakışma kontrolü

function letterAt(row, col) {
  const items = [...puzzle.across, ...puzzle.down];
  for (const it of items) {
    const dr = it.dir === 'across' ? 0 : 1;
    const dc = it.dir === 'across' ? 1 : 0;
    for (let t = 0; t < it.answer.length; t++) {
      const rr = it.row + dr * t;
      const cc = it.col + dc * t;
      if (rr === row && cc === col) return it.answer[t];
    }
  }
  return null;
}

// Yan yana dokunmayı da yasaklamak istersen true yap
const STRICT_SIDE_TOUCH = false;

function conflicts(row, col, dir, word) {
  const dr = dir === 'across' ? 0 : 1;
  const dc = dir === 'across' ? 1 : 0;
  const N = word.length;

  // A) Başlangıç ve bitiş kapıları boş olmalı
  if (letterAt(row - dr, col - dc)) return true;          // kelimenin hemen önü
  if (letterAt(row + dr * N, col + dc * N)) return true;  // kelimenin hemen sonu

  // B) Her hücre: ya boş olmalı ya da aynı harfle KESİŞMELİ
  for (let k = 0; k < N; k++) {
    const r = row + dr * k;
    const c = col + dc * k;
    const existing = letterAt(r, c);
    if (existing && existing !== word[k]) return true;

    // C) (Opsiyonel) Yan komşular da boş olsun (kesişim hücresi hariç)
    if (STRICT_SIDE_TOUCH && !existing) {
      if (dir === 'across') {
        if (letterAt(r - 1, c) || letterAt(r + 1, c)) return true;
      } else {
        if (letterAt(r, c - 1) || letterAt(r, c + 1)) return true;
      }
    }
  }
  return false;
}


function getBounds() {
  let minRow = Infinity, minCol = Infinity, maxRow = -Infinity, maxCol = -Infinity;
  const all = [...puzzle.across, ...puzzle.down];
  if (all.length === 0) return { minRow: 1, minCol: 1, maxRow: 1, maxCol: 1 };

  for (const it of all) {
    const len = it.answer.length;
    if (it.dir === 'across') {
      minRow = Math.min(minRow, it.row);
      minCol = Math.min(minCol, it.col);
      maxRow = Math.max(maxRow, it.row);
      maxCol = Math.max(maxCol, it.col + len - 1);
    } else {
      minRow = Math.min(minRow, it.row);
      minCol = Math.min(minCol, it.col);
      maxRow = Math.max(maxRow, it.row + len - 1);
      maxCol = Math.max(maxCol, it.col);
    }
  }
  return { minRow, minCol, maxRow, maxCol };
}

function normalizePuzzle() {
  const { minRow, minCol } = getBounds();
  const shiftR = minRow > 1 ? 0 : 1 - minRow;
  const shiftC = minCol > 1 ? 0 : 1 - minCol;
  if (!shiftR && !shiftC) return;
  for (const it of [...puzzle.across, ...puzzle.down]) {
    it.row += shiftR;
    it.col += shiftC;
  }
}

function extractClue(i) {
  const card = currentBatch[i];
  const qa = card.gameQuestions?.find(g => g.gameId === Number(document.getElementById('gameRoot').dataset.gameId));
  return qa?.questionText || card.definition || '';
}

function buildClues() {
  const acrossEl = document.getElementById('across-clues');
  const downEl = document.getElementById('down-clues');
  acrossEl.innerHTML = '';
  downEl.innerHTML = '';

  const makeLi = (item) => {
    const li = document.createElement('li');
    li.className = 'clue-item';
    li.innerHTML = `
      <span class="clue-num">${item.num}</span>
      <span class="clue-text">${item.clue}</span>
    `;
    return li;
  };

  puzzle.across.forEach(i => acrossEl.appendChild(makeLi(i)));
  puzzle.down.forEach(i => downEl.appendChild(makeLi(i)));

  if (typeof window.__updatePuzzleChips === 'function') {
    window.__updatePuzzleChips(puzzle.size, puzzle.across.length, puzzle.down.length);
  }
}

function buildGrid() {
  const grid = document.getElementById('puzzleGrid');
  const N = puzzle.size;
  grid.innerHTML = '';
  grid.style.gridTemplate = `repeat(${N},1fr)/repeat(${N},1fr)`;

  const cells = [];
  const wraps = [];

  for (let r = 1; r <= N; r++) {
    for (let c = 1; c <= N; c++) {
      const wrap = document.createElement('div');
      wrap.className = 'cp-wrap';
      const inp = document.createElement('input');
      inp.type = 'text'; inp.maxLength = 1;
      inp.className = 'cp-cell blocked';
      inp.dataset.r = r; inp.dataset.c = c;
      inp.readOnly = true;
      wrap.appendChild(inp);
      grid.appendChild(wrap);
      cells.push(inp);
      wraps.push(wrap);
    }
  }

  puzzle.across.forEach(item => {
    for (let k = 0; k < item.answer.length; k++) {
      const idx = (item.row - 1) * N + (item.col - 1 + k);
      const cell = cells[idx];
      if (cell) {
        cell.readOnly = false;
        cell.classList.remove('blocked');
        cell.dataset.answer = item.answer[k];
      }
    }
    addBadge(item.row, item.col, item.num, N, cells);
  });

  puzzle.down.forEach(item => {
    for (let k = 0; k < item.answer.length; k++) {
      const idx = (item.row - 1 + k) * N + (item.col - 1);
      const cell = cells[idx];
      if (cell) {
        cell.readOnly = false;
        cell.classList.remove('blocked');
        cell.dataset.answer = item.answer[k];
      }
    }
    addBadge(item.row, item.col, item.num, N, cells);
  });
}

function addBadge(row, col, num, N, cells) {
  const idx = (row - 1) * N + (col - 1);
  const baseCell = cells[idx];
  if (!baseCell) return;
  const wrapper = baseCell.parentElement;
  let badge = wrapper.querySelector('.cp-num');
  if (!badge) {
    badge = document.createElement('span');
    badge.className = 'cp-num';
    wrapper.appendChild(badge);
  }
  const cur = parseInt(badge.textContent || '', 10);
  if (!cur || num < cur) badge.textContent = num;
}

function check(studentId, gameId) {
  let correct = true;
  document.querySelectorAll('.cp-cell:not(.blocked)').forEach(c => {
    if (c.value.toLocaleUpperCase('tr') !== c.dataset.answer) {
      correct = false; c.classList.add('wrong');
    } else c.classList.remove('wrong');
  });

  const feedbackEl = document.getElementById('cpFeedback');
  feedbackEl.textContent = correct
      ? "🎉 Tebrikler, doğru bildiniz!"
      : "❌ Maalesef, yanlış bildiniz!";

// Remove any previous success/failure classes and add the new one
  feedbackEl.classList.remove("text-green-600", "text-red-600");
  feedbackEl.classList.add(correct ? "text-green-600" : "text-red-600");

  const duration = (Date.now() - start) / 1000;
  awardScore(studentId, gameId, correct, duration);

  // 👇 Pop-up/Embed ise sonraki oyuna geç
  const embedded = document.getElementById('gameRoot')?.dataset.embed === 'true';

  if (correct) {
    if (embedded) {
      setTimeout(() => {
        if (window.parent !== window) {
          window.parent.postMessage('next-game', '*');
        } else {
          // fallback: sayfa gömülü değilse yine yeni bulmaca üret
          refreshPuzzle(studentId, gameId);
        }
      }, 1200);
    } else {
      setTimeout(() => refreshPuzzle(studentId, gameId), 1000);
    }
  }
}


async function reveal() {
  document.querySelectorAll('.cp-cell:not(.blocked)')
      .forEach(c => c.value = c.dataset.answer);

  const embedded = document.getElementById('gameRoot')?.dataset.embed === 'true';
  const feedbackEl = document.getElementById('cpFeedback');
  feedbackEl.textContent = 'Cevap gösterildi';

  if (embedded) {
    setTimeout(() => {
      if (window.parent !== window) {
        window.parent.postMessage('next-game', '*');
      }
    }, 1200);
  }
}


async function refreshPuzzle(studentId, gameId) {
  await loadBatch(studentId, gameId);
  placeCrosswords();
  buildClues();
  buildGrid();
  start = Date.now();
  document.getElementById('cpFeedback').textContent = '';
}

export async function initCrossPuzzle(studentId, gameId) {
  await loadBatch(studentId, gameId);
  placeCrosswords();
  buildClues();
  buildGrid();
  start = Date.now();
  document.getElementById('cpCheck').onclick = () => check(studentId, gameId);
  const revealBtn = document.getElementById('cpReveal'); if (revealBtn) revealBtn.onclick = reveal;
  const back = document.getElementById('cpBack');
  if (back && document.getElementById('gameRoot').dataset.embed === 'true') back.style.display = 'none';
}
