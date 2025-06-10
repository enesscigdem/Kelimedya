import {awardScore} from './common.js';

const puzzle = {
  size:7,
  across:[{num:1,row:1,col:1,answer:'ELMA',clue:'Meyve'}],
  down:[{num:2,row:1,col:4,answer:'AGAÇ',clue:'Odun'}]
};
let start;

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
  let correct=true;document.querySelectorAll('.cp-cell:not(.blocked)').forEach(c=>{if(c.value.toUpperCase()!==c.dataset.answer){correct=false;c.classList.add('wrong');}else c.classList.remove('wrong');});
  document.getElementById('cpFeedback').textContent=correct?'Tebrikler':'Yanlışlıklar var';
  const duration=(Date.now()-start)/1000;
  awardScore(studentId, gameId, correct, duration);
}

function reveal(){
  document.querySelectorAll('.cp-cell:not(.blocked)').forEach(c=>{c.value=c.dataset.answer;});
}

export function initCrossPuzzle(studentId, gameId){
  buildClues();buildGrid();start=Date.now();
  document.getElementById('cpCheck').onclick=()=>check(studentId, gameId);
  const btn=document.getElementById('cpReveal'); if(btn) btn.onclick=reveal;
}
