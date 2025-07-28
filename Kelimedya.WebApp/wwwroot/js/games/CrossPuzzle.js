// CrossPuzzle.js
import { awardScore, fetchLearnedWords } from './common.js';

const PAGE_SIZE = 5;
let start;
let offset = 0;
let currentBatch = [];
const puzzle = { size: 0, across: [], down: [] };

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
  }
}

function placeCrosswords() {
  puzzle.across = [];
  puzzle.down = [];
  if (currentBatch.length === 0) return;
  const first = currentBatch[0].answerRaw;
  puzzle.across.push({ num: 1, row: 1, col: 1, answer: first, clue: extractClue(0), dir: 'across' });

  let placed = [{ dir: 'across', word: first, row: 1, col: 1 }];
  let nextNum = 2;

  for (let i = 1; i < currentBatch.length; i++) {
    const word = currentBatch[i].answerRaw;
    const clue = extractClue(i);
    let placedThis = false;
    for (const p of placed) {
      for (let a = 0; a < p.word.length; a++) {
        for (let b = 0; b < word.length; b++) {
          if (p.word[a] === word[b]) {
            if (p.dir === 'across') {
              const row = p.row + a;
              const col = p.col + b;
              puzzle.down.push({ num: nextNum, row, col, answer: word, clue, dir: 'down' });
              placed.push({ dir: 'down', word, row, col });
            } else {
              const row = p.row + b;
              const col = p.col + a;
              puzzle.across.push({ num: nextNum, row, col, answer: word, clue, dir: 'across' });
              placed.push({ dir: 'across', word, row, col });
            }
            nextNum++;
            placedThis = true;
            break;
          }
        }
        if (placedThis) break;
      }
      if (placedThis) break;
    }
    if (!placedThis) {
      const row = puzzle.across.length + 2;
      puzzle.across.push({ num: nextNum, row, col: 1, answer: word, clue, dir: 'across' });
      placed.push({ dir: 'across', word, row, col: 1 });
      nextNum++;
    }
  }

  let maxR = 0, maxC = 0;
  [...puzzle.across, ...puzzle.down].forEach(item => {
    const len = item.answer.length;
    if (item.dir === 'across') {
      maxR = Math.max(maxR, item.row);
      maxC = Math.max(maxC, item.col + len - 1);
    } else {
      maxR = Math.max(maxR, item.row + len - 1);
      maxC = Math.max(maxC, item.col);
    }
  });
  puzzle.size = Math.max(maxR, maxC) + 2;
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
  puzzle.across.forEach(i => {
    const li = document.createElement('li');
    li.innerHTML = `${i.num}. ${i.clue}`;
    acrossEl.appendChild(li);
  });
  puzzle.down.forEach(i => {
    const li = document.createElement('li');
    li.innerHTML = `${i.num}. ${i.clue}`;
    downEl.appendChild(li);
  });
}

function buildGrid() {
  const grid = document.getElementById('puzzleGrid');
  const N = puzzle.size;
  grid.innerHTML = '';
  grid.style.gridTemplate = `repeat(${N},1fr)/repeat(${N},1fr)`;
  const cells = [];
  for (let r = 1; r <= N; r++) {
    for (let c = 1; c <= N; c++) {
      const inp = document.createElement('input');
      inp.type = 'text'; inp.maxLength = 1;
      inp.className = 'cp-cell blocked';
      inp.dataset.r = r; inp.dataset.c = c;
      inp.readOnly = true;
      grid.appendChild(inp);
      cells.push(inp);
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
    const idx0 = (item.row - 1) * N + (item.col - 1);
    const badge = document.createElement('span'); badge.className = 'cp-num'; badge.textContent = item.num;
    const baseCell = cells[idx0];
    if (baseCell && baseCell.parentElement) baseCell.parentElement.appendChild(badge);
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
    const idx0 = (item.row - 1) * N + (item.col - 1);
    const badge = document.createElement('span'); badge.className = 'cp-num'; badge.textContent = item.num;
    const baseCell = cells[idx0];
    if (baseCell && baseCell.parentElement) baseCell.parentElement.appendChild(badge);
  });
}

function check(studentId, gameId) {
  let correct = true;
  document.querySelectorAll('.cp-cell:not(.blocked)').forEach(c => {
    if (c.value.toLocaleUpperCase('tr') !== c.dataset.answer) {
      correct = false; c.classList.add('wrong');
    } else c.classList.remove('wrong');
  });
  document.getElementById('cpFeedback').textContent = correct ? 'Tebrikler' : 'Yanlışlıklar var';
  const duration = (Date.now() - start) / 1000;
  awardScore(studentId, gameId, correct, duration);
  if (correct) setTimeout(() => refreshPuzzle(studentId, gameId), 1000);
}

async function reveal() {
  document.querySelectorAll('.cp-cell:not(.blocked)').forEach(c => c.value = c.dataset.answer);
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
