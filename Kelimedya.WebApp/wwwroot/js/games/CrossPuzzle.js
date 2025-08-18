// CrossPuzzle.js
import { awardScore, fetchLearnedWords, disableTemporary } from './common.js';

const DEFAULT_PAGE_SIZE = 5;
let PAGE_SIZE = DEFAULT_PAGE_SIZE;
let TOTAL_BATCHES = 1;
let BATCH_INDEX = 0;
const BASE_SIZE = 15;      // minimum grid
const MAX_LIMIT = 15;      // yatay genişlik tabanı (en uzun kelime bunu aşarsa genişlik büyür)
const MAX_ROWS  = BASE_SIZE; // dikey satır tavanı (yükseklik büyümesin diye)
const PADDING   = 2;

let start;
let offset = 0;
let currentBatch = [];
const puzzle = { size: 0, across: [], down: [] };

function preferAcross(word){ return word.length >= 10; }

function longestLen(){
    return currentBatch.length ? Math.max(...currentBatch.map(x => x.answerRaw.length)) : 0;
}

function withinBounds(row, col, dir, wordLen, limit){
    const dr = dir === 'across' ? 0 : 1;
    const dc = dir === 'across' ? 1 : 0;
    const endRow = row + dr * (wordLen - 1);
    const endCol = col + dc * (wordLen - 1);
    return row >= 1 && col >= 1 && endRow <= limit && endCol <= limit;
}

function shuffleInPlace(arr){
    for (let i = arr.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [arr[i], arr[j]] = [arr[j], arr[i]];
    }
    return arr;
}
function pickRandom(arr){ return arr[Math.floor(Math.random() * arr.length)]; }

async function loadBatch(studentId, gameId, lessonId, { recalc = true } = {}) {
    const cards = await fetchLearnedWords(studentId, lessonId);

    if (!cards || cards.length === 0) {
        currentBatch = [];
        return;
    }

    // batch sayısına göre sayfa boyutunu ayarla
    if (recalc) {
        // en az 3, en fazla 5; iki bulmaca toplamı tüm kelimeleri kaplasın
        const needPerBatch = Math.ceil(cards.length / Math.max(1, TOTAL_BATCHES));
        PAGE_SIZE = Math.min(DEFAULT_PAGE_SIZE, Math.max(3, needPerBatch));
        // bu batch'in başlangıç index'i
        BATCH_INDEX = Math.max(0, BATCH_INDEX);
        offset = BATCH_INDEX * PAGE_SIZE;
    }

    const start = Math.min(offset, Math.max(0, cards.length - 1));
    const end = Math.min(start + PAGE_SIZE, cards.length);

    // 👇 wrap YOK — ikinci batch, birincinin kaldığı yerden farklı parça alır
    const slice = cards.slice(start, end);

    currentBatch = slice.map(card => {
        const qa  = card.gameQuestions?.find(g => g.gameId === Number(gameId));
        const raw = (qa?.answerText || card.word || '').toUpperCase('tr');
        return { ...card, answerRaw: raw.replace(/\s+/g, '') };
    });

    shuffleInPlace(currentBatch);
    currentBatch.sort((a,b)=> b.answerRaw.length - a.answerRaw.length);
}


function placeCrosswords(){
    puzzle.across = [];
    puzzle.down   = [];
    if (currentBatch.length === 0) return;

    // genişlik: en uzun kelime kadar büyüyebilir
    const GRID_LIMIT = Math.max(BASE_SIZE, longestLen(), MAX_LIMIT);

    // --- İLK KELİME: uzun ise YATAY ve ORTALANMIŞ yerleştir
    const first = currentBatch[0].answerRaw;
    let dir0, row0, col0;

    if (preferAcross(first)) {
        dir0 = 'across';
        row0 = Math.ceil(MAX_ROWS / 2);
        // yatayda ortala; dışarı taşmayacak en uygun başlangıç
        col0 = Math.floor((GRID_LIMIT - first.length) / 2) + 1;
        col0 = Math.max(1, Math.min(col0, GRID_LIMIT - first.length + 1));
    } else {
        // kısa ise serbest ama sınırlar içinde
        const anchorRow = Math.ceil(MAX_ROWS / 2);
        const anchorCol = Math.ceil(GRID_LIMIT / 2);
        const tryDir = Math.random() < 0.5 ? 'across' : 'down';
        const limitA = tryDir === 'down' ? MAX_ROWS : GRID_LIMIT;

        if (withinBounds(anchorRow, anchorCol, tryDir, first.length, limitA)) {
            dir0 = tryDir; row0 = anchorRow; col0 = anchorCol;
        } else if (withinBounds(anchorRow, anchorCol, 'across', first.length, GRID_LIMIT)) {
            dir0 = 'across'; row0 = anchorRow; col0 = anchorCol;
        } else {
            dir0 = 'down'; row0 = anchorRow; col0 = anchorCol;
        }
    }

    const firstItem = { num: 1, row: row0, col: col0, answer: first, clue: extractClue(0), dir: dir0 };
    (dir0 === 'across' ? puzzle.across : puzzle.down).push(firstItem);
    let placed  = [{ dir: dir0, word: first, row: row0, col: col0 }];
    let nextNum = 2;

    // --- KALAN KELİMELER
    for (let i = 1; i < currentBatch.length; i++) {
        const word = currentBatch[i].answerRaw;
        const clue = extractClue(i);
        let chosen = null;
        const candidates = [];

        // 1) Kesişim tabanlı adaylar
        for (const p of placed) {
            for (let a = 0; a < p.word.length; a++) {
                for (let b = 0; b < word.length; b++) {
                    if (p.word[a] !== word[b]) continue;

                    let row, col, dir;
                    if (p.dir === 'across') { dir = 'down';   row = p.row - b; col = p.col + a; }
                    else                     { dir = 'across';row = p.row + a; col = p.col - b; }
                    if (row < 1 || col < 1) continue;

                    const limit = (dir === 'down') ? MAX_ROWS : GRID_LIMIT;
                    if (withinBounds(row, col, dir, word.length, limit) && !conflicts(row, col, dir, word)) {
                        candidates.push({ row, col, dir });
                    }
                }
            }
        }

        const longWord = preferAcross(word);
        let filtered = candidates;
        if (longWord) filtered = candidates.filter(c => c.dir === 'across'); // uzun kelime: sadece yatay

        if (filtered.length) {
            chosen = pickRandom(filtered);
        } else {
            // 2) Kesişim yoksa: önce yatay tara
            const B = getBounds();
            let row = Math.max(1, (B.maxRow || Math.ceil(MAX_ROWS/2)) + 2);
            let col = Math.max(1, B.minCol);
            let tries = 0;
            while (tries < 220) {
                if (withinBounds(row, col, 'across', word.length, GRID_LIMIT) && !conflicts(row, col, 'across', word)) {
                    chosen = { row, col, dir: 'across' }; break;
                }
                if (row < MAX_ROWS) row++; else break; // yükseklik tavanı
                tries++;
            }

            // 3) Hâlâ olmadıysa ve kelime kısa ise dikey dene (MAX_ROWS sınırıyla)
            if (!chosen && !longWord) {
                let row2 = Math.max(1, B.minRow);
                let col2 = Math.max(1, (B.maxCol || Math.ceil(GRID_LIMIT/2)) + 2);
                let tries2 = 0;
                while (tries2 < 220) {
                    if (withinBounds(row2, col2, 'down', word.length, MAX_ROWS) && !conflicts(row2, col2, 'down', word)) {
                        chosen = { row: row2, col: col2, dir: 'down' }; break;
                    }
                    if (col2 < GRID_LIMIT) col2++; else break;
                    tries2++;
                }
            }
        }

        if (chosen) {
            const item = { num: nextNum, row: chosen.row, col: chosen.col, answer: word, clue, dir: chosen.dir };
            (chosen.dir === 'across' ? puzzle.across : puzzle.down).push(item);
            placed.push({ dir: chosen.dir, word, row: chosen.row, col: chosen.col });
            nextNum++;
        }
    }

    normalizePuzzle();
    const { maxRow, maxCol } = getBounds();
    const occupied   = Math.max(maxRow, maxCol);
    const needByWord = longestLen();
    puzzle.size = Math.max(BASE_SIZE, needByWord, occupied + PADDING);

    puzzle.across.sort((a,b)=>a.num-b.num);
    puzzle.down.sort((a,b)=>a.num-b.num);
}

function letterAt(row, col){
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

const STRICT_SIDE_TOUCH = false;

function conflicts(row, col, dir, word){
    const dr = dir === 'across' ? 0 : 1;
    const dc = dir === 'across' ? 1 : 0;
    const N  = word.length;

    if (letterAt(row - dr, col - dc)) return true;
    if (letterAt(row + dr * N, col + dc * N)) return true;

    for (let k = 0; k < N; k++) {
        const r = row + dr * k;
        const c = col + dc * k;
        const existing = letterAt(r, c);
        if (existing && existing !== word[k]) return true;

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

function getBounds(){
    let minRow = Infinity, minCol = Infinity, maxRow = -Infinity, maxCol = -Infinity;
    const all = [...puzzle.across, ...puzzle.down];
    if (all.length === 0) return { minRow:1, minCol:1, maxRow:1, maxCol:1 };

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

function normalizePuzzle(){
    const { minRow, minCol } = getBounds();
    const shiftR = minRow > 1 ? 0 : 1 - minRow;
    const shiftC = minCol > 1 ? 0 : 1 - minCol;
    if (!shiftR && !shiftC) return;
    for (const it of [...puzzle.across, ...puzzle.down]) {
        it.row += shiftR;
        it.col += shiftC;
    }
}

function extractClue(i){
    const card = currentBatch[i];
    const qa = card.gameQuestions?.find(g => g.gameId === Number(document.getElementById('gameRoot').dataset.gameId));
    return qa?.questionText || card.definition || '';
}

function buildClues(){
    const acrossEl = document.getElementById('across-clues');
    const downEl   = document.getElementById('down-clues');
    if (!acrossEl || !downEl) return;

    acrossEl.innerHTML = '';
    downEl.innerHTML   = '';

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
    puzzle.down.forEach(i   => downEl.appendChild(makeLi(i)));

    if (typeof window.__updatePuzzleChips === 'function') {
        window.__updatePuzzleChips(puzzle.size, puzzle.across.length, puzzle.down.length);
    }
}

function buildGrid(){
    const grid = document.getElementById('puzzleGrid');
    const N = puzzle.size;
    grid.innerHTML = '';
    grid.style.gridTemplate = `repeat(${N},1fr)/repeat(${N},1fr)`;

    const cells = [];
    const wraps = [];

    const idxOf = (r,c) => (r - 1) * N + (c - 1);
    const getCell = (r,c) => {
        if (r < 1 || c < 1 || r > N || c > N) return null;
        return cells[idxOf(r,c)] || null;
    };
    const isWritable = (r,c) => {
        const el = getCell(r,c);
        return el && !el.classList.contains('blocked');
    };

    for (let r = 1; r <= N; r++) {
        for (let c = 1; c <= N; c++) {
            const wrap = document.createElement('div');
            wrap.className = 'cp-wrap';
            const inp = document.createElement('input');
            inp.type = 'text'; inp.maxLength = 1;
            inp.className = 'cp-cell blocked';
            inp.dataset.r = r; inp.dataset.c = c;
            inp.readOnly = true;

            // Odak: yön tayini
            inp.addEventListener('focus', () => {
                const rr = parseInt(inp.dataset.r,10), cc = parseInt(inp.dataset.c,10);
                const leftBlocked   = !isWritable(rr, cc-1);
                const rightWritable =  isWritable(rr, cc+1);
                const upBlocked     = !isWritable(rr-1, cc);
                const downWritable  =  isWritable(rr+1, cc);
                let dir = null;
                if (leftBlocked && rightWritable) dir = 'across';
                if (upBlocked   && downWritable)  dir = dir || 'down';
                if (!dir) dir = rightWritable ? 'across' : (downWritable ? 'down' : 'across');
                grid.dataset.activeDir = dir;
            });

            // Yazınca ilerle
            inp.addEventListener('input', () => {
                const val = (inp.value || '').toLocaleUpperCase('tr');
                inp.value = val.slice(-1);
                const rr = parseInt(inp.dataset.r,10), cc = parseInt(inp.dataset.c,10);
                const dir = grid.dataset.activeDir || 'across';
                let nr = rr, nc = cc;
                if (dir === 'across') nc = cc + 1; else nr = rr + 1;
                const next = getCell(nr, nc);
                if (next && !next.readOnly) next.focus();
            });

            // ok tuşlarıyla gerçek gezinme (blocked'ları atla)
            const stepToWritable = (r0, c0, dr, dc) => {
                let nr = r0 + dr, nc = c0 + dc, safety = 0;
                while (nr >= 1 && nc >= 1 && nr <= N && nc <= N && safety < N * 2) {
                    const nx = getCell(nr, nc);
                    if (nx && !nx.readOnly) return nx;
                    nr += dr; nc += dc; safety++;
                }
                return null;
            };

            inp.addEventListener('keydown', (e) => {
                const rr = parseInt(inp.dataset.r,10), cc = parseInt(inp.dataset.c,10);

                if (e.key === 'Backspace' && inp.value === '') {
                    const dir = grid.dataset.activeDir || 'across';
                    let pr = rr, pc = cc;
                    if (dir === 'across') pc = cc - 1; else pr = rr - 1;
                    const prev = getCell(pr, pc);
                    if (prev && !prev.readOnly) { e.preventDefault(); prev.focus(); }
                    return;
                }

                let dr = 0, dc = 0, newDir = null;
                if (e.key === 'ArrowLeft')  { dr = 0;  dc = -1; newDir = 'across'; }
                if (e.key === 'ArrowRight') { dr = 0;  dc =  1; newDir = 'across'; }
                if (e.key === 'ArrowUp')    { dr = -1; dc =  0; newDir = 'down';   }
                if (e.key === 'ArrowDown')  { dr =  1; dc =  0; newDir = 'down';   }

                if (newDir) {
                    e.preventDefault();
                    grid.dataset.activeDir = newDir;
                    const next = stepToWritable(rr, cc, dr, dc);
                    if (next) next.focus();
                }
            });

            wrap.appendChild(inp);
            grid.appendChild(wrap);
            cells.push(inp);
            wraps.push(wrap);
        }
    }

    // aktif hücreler ve numara rozetleri
    puzzle.across.forEach(item => {
        for (let k = 0; k < item.answer.length; k++) {
            const idx  = (item.row - 1) * N + (item.col - 1 + k);
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
            const idx  = (item.row - 1 + k) * N + (item.col - 1);
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

function addBadge(row, col, num, N, cells){
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

function check(studentId, gameId, lessonId){
    let correct = true;
    const cells = document.querySelectorAll('.cp-cell:not(.blocked)');
    cells.forEach(c => {
        if (c.value.toLocaleUpperCase('tr') !== c.dataset.answer) {
            correct = false; c.classList.add('wrong');
        } else c.classList.remove('wrong');
    });

    const feedbackEl = document.getElementById('cpFeedback');
    if (feedbackEl) {
        feedbackEl.textContent = correct
            ? "🎉 Tebrikler, doğru bildiniz!"
            : "❌ Maalesef, yanlış bildiniz!";
        feedbackEl.classList.remove("text-green-600", "text-red-600");
        feedbackEl.classList.add(correct ? "text-green-600" : "text-red-600");
    }

    const duration = (Date.now() - start) / 1000;
    awardScore(studentId, gameId, correct, duration);

    const checkBtn  = document.getElementById('cpCheck');
    const revealBtn = document.getElementById('cpReveal');

    if (!correct) {
        cells.forEach(c => { c.value = c.dataset.answer; c.disabled = true; });
        disableTemporary([checkBtn, revealBtn], 2500);
    }

    const embedded = document.getElementById('gameRoot')?.dataset.embed === 'true';
    const proceed = () => {
        if (embedded) {
            if (window.parent !== window) window.parent.postMessage('next-game', '*');
            else refreshPuzzle(studentId, gameId, lessonId);
        } else {
            refreshPuzzle(studentId, gameId, lessonId);
        }
    };

    if (correct) {
        setTimeout(proceed, 1200);
    } else {
        setTimeout(proceed, 2000);
    }
}

async function reveal(studentId, gameId, lessonId){
    document.querySelectorAll('.cp-cell:not(.blocked)')
        .forEach(c => c.value = c.dataset.answer);

    const feedbackEl = document.getElementById('cpFeedback');
    if (feedbackEl) {
        feedbackEl.textContent = 'Cevap gösterildi';
        feedbackEl.classList.remove("text-green-600", "text-red-600");
    }
}

async function refreshPuzzle(studentId, gameId, lessonId){
    offset += PAGE_SIZE;
    await loadBatch(studentId, gameId, lessonId, { recalc: false });
    placeCrosswords();
    buildGrid();
    buildClues();
    start = Date.now();
}



async function initCrossPuzzle(studentId, gameId, lessonId, batch = 0, batches = 1){
    BATCH_INDEX = Number(batch) || 0;
    TOTAL_BATCHES = Number(batches) || 1;

    await loadBatch(studentId, gameId, lessonId, { recalc: true });
    placeCrosswords();
    buildGrid();
    buildClues();
    start = Date.now();

    const checkBtn  = document.getElementById('cpCheck');
    const revealBtn = document.getElementById('cpReveal');
    const backBtn   = document.getElementById('cpBack');

    if (checkBtn)  checkBtn.onclick  = () => check(studentId, gameId, lessonId);
    if (revealBtn) revealBtn.onclick = () => reveal(studentId, gameId, lessonId);

    const embedded = document.getElementById('gameRoot')?.dataset.embed === 'true';
    if (backBtn) {
        if (embedded) backBtn.style.display = 'none';
        else backBtn.onclick = () => history.back();
    }
}


export {
    loadBatch,
    placeCrosswords,
    buildGrid,
    buildClues,
    check,
    reveal,
    refreshPuzzle,
    initCrossPuzzle
};
