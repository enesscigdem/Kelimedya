import {awardScore, fetchLearnedWords} from './common.js';

const puzzle = { size:7, across:[], down:[] };
let start;

async function buildPuzzle(studentId, gameId){
  const cards = await fetchLearnedWords(studentId);
  if(!cards || cards.length < 2){
    puzzle.size = 7;
    puzzle.across=[{num:1,row:1,col:1,answer:'ELMA',clue:'1'}];
    puzzle.down=[{num:2,row:1,col:4,answer:'AGAC',clue:'2'}];
    return;
  }
  const a = cards[0];
  const b = cards[1];
  const qa = a.gameQuestions?.find(g=>g.gameId===parseInt(gameId));
  const qb = b.gameQuestions?.find(g=>g.gameId===parseInt(gameId));
  const ansA = (qa?.answerText||a.word).toLocaleUpperCase('tr');
  const ansB = (qb?.answerText||b.word).toLocaleUpperCase('tr');
  const clueA = qa?.questionText || a.definition || '';
  const clueB = qb?.questionText || b.definition || '';
  let col = ansA.indexOf(ansB[0]);
  col = col === -1 ? 1 : col+1;
  puzzle.size = Math.max(ansA.length, col + ansB.length -1) + 2;
  puzzle.across=[{num:1,row:1,col:1,answer:ansA,clue:clueA}];
  puzzle.down=[{num:2,row:1,col:col,answer:ansB,clue:clueB}];
}

function buildClues(){
  const a=document.getElementById('across-clues'),d=document.getElementById('down-clues');
  a.innerHTML='';d.innerHTML='';
  puzzle.across.forEach(i=>{const li=document.createElement('li');li.textContent=`${i.num}. ${i.clue}`;a.appendChild(li);});
  puzzle.down.forEach(i=>{const li=document.createElement('li');li.textContent=`${i.num}. ${i.clue}`;d.appendChild(li);});
}

function buildGrid(){
  const grid=document.getElementById('puzzleGrid'),N=puzzle.size;
  grid.innerHTML='';grid.style.gridTemplate=`repeat(${N},1fr)/repeat(${N},1fr)`;
  const cells=[];
  for(let r=1;r<=N;r++){
    for(let c=1;c<=N;c++){
      const inp=document.createElement('input');inp.type='text';inp.maxLength=1;inp.className='cp-cell blocked';inp.dataset.r=r;inp.dataset.c=c;inp.readOnly=true;grid.appendChild(inp);cells.push(inp);
    }
  }
  puzzle.across.forEach(item=>{
    for(let i=0;i<item.answer.length;i++){
      const idx=(item.row-1)*N+(item.col-1+i);
      const cell=cells[idx];cell.readOnly=false;cell.classList.remove('blocked');cell.dataset.answer=item.answer[i];
    }
    const numCell=cells[(item.row-1)*N+(item.col-1)];const badge=document.createElement('span');badge.className='cp-num';badge.textContent=item.num;numCell.parentElement.appendChild(badge);
  });
  puzzle.down.forEach(item=>{
    for(let i=0;i<item.answer.length;i++){
      const idx=(item.row-1+i)*N+(item.col-1);
      const cell=cells[idx];cell.readOnly=false;cell.classList.remove('blocked');cell.dataset.answer=item.answer[i];
    }
    const numCell=cells[(item.row-1)*N+(item.col-1)];const badge=document.createElement('span');badge.className='cp-num';badge.textContent=item.num;numCell.parentElement.appendChild(badge);
  });
}

function check(studentId, gameId){
  let correct=true;document.querySelectorAll('.cp-cell:not(.blocked)').forEach(c=>{if(c.value.toLocaleUpperCase('tr')!==c.dataset.answer){correct=false;c.classList.add('wrong');}else c.classList.remove('wrong');});
  document.getElementById('cpFeedback').textContent=correct?'Tebrikler':'Yanlışlıklar var';
  const duration=(Date.now()-start)/1000;
  awardScore(studentId, gameId, correct, duration);
  if(correct && window.parent!==window) window.parent.postMessage('next-game','*');
}

function reveal(){
  document.querySelectorAll('.cp-cell:not(.blocked)').forEach(c=>{c.value=c.dataset.answer;});
}

export async function initCrossPuzzle(studentId, gameId){
  await buildPuzzle(studentId, gameId);
  buildClues();
  buildGrid();
  start=Date.now();
  document.getElementById('cpCheck').onclick=()=>check(studentId, gameId);
  const btn=document.getElementById('cpReveal'); if(btn) btn.onclick=reveal;
  const back=document.getElementById('cpBack');
  if(back && document.getElementById('gameRoot').dataset.embed==='true') back.style.display='none';
}
