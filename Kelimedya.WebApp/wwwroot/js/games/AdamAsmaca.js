import {fetchLearnedWords, awardScore, fetchWordCardWithQuestions} from './common.js';

let word = '', guessed = new Set(), wrong = 0, startTime, cards=[], idx=0;
let singleMode = false;
let ctx, questionEl;

function draw(){
  const display = word.split('').map(ch=>guessed.has(ch)?ch:'_').join(' ');
  document.getElementById('hangmanWord').textContent = display;
  document.getElementById('wrongCount').textContent = wrong;
  drawHangman();
}

function drawHangman(){
  if(!ctx) return;
  ctx.clearRect(0,0,300,300);
  ctx.lineWidth=2;
  ctx.beginPath(); ctx.moveTo(10,290); ctx.lineTo(150,290); ctx.stroke();
  ctx.beginPath(); ctx.moveTo(40,290); ctx.lineTo(40,20); ctx.lineTo(120,20); ctx.lineTo(120,40); ctx.stroke();
  if(wrong>0){ ctx.beginPath(); ctx.arc(120,60,20,0,Math.PI*2); ctx.stroke(); }
  if(wrong>1){ ctx.beginPath(); ctx.moveTo(120,80); ctx.lineTo(120,150); ctx.stroke(); }
  if(wrong>2){ ctx.beginPath(); ctx.moveTo(120,90); ctx.lineTo(90,120); ctx.stroke(); }
  if(wrong>3){ ctx.beginPath(); ctx.moveTo(120,90); ctx.lineTo(150,120); ctx.stroke(); }
  if(wrong>4){ ctx.beginPath(); ctx.moveTo(120,150); ctx.lineTo(90,180); ctx.stroke(); }
  if(wrong>5){ ctx.beginPath(); ctx.moveTo(120,150); ctx.lineTo(150,180); ctx.stroke(); }
}

function finish(success, studentId, gameId){
  const duration = (Date.now()-startTime)/1000;
  awardScore(studentId, gameId, success, duration);
}

export async function initHangman(studentId, gameId, single, wordId){
  if(single){
    if(wordId){
      const card=await fetchWordCardWithQuestions(wordId);
      cards=card? [card] : [{word:single}];
    }else{
      cards=[{word:single}];
    }
    singleMode=true;
  }else{
    cards = await fetchLearnedWords(studentId);
    if(cards.length===0) cards=[{word:'kelime'}];
    singleMode=false;
  }
  idx=0;
  const q=cards[idx].gameQuestions?.find(g=>g.gameId===parseInt(gameId));
  word = (q?.answerText||cards[idx].word).toLowerCase();
  const canvas=document.getElementById('hangmanBoard');
  ctx=canvas.getContext('2d');
  questionEl=document.getElementById('hangmanQuestion');
  setup(studentId, gameId);
  if(singleMode){
    const btn=document.getElementById('endGameBtn');
    if(btn) btn.style.display='none';
  }
}

function onKey(e){
  if(e.key.length!==1) return;
  const ch = e.key.toLowerCase();
  handleGuess(ch, document.getElementById('gameRoot').dataset.studentId, document.getElementById('gameRoot').dataset.gameId, null);
}

function handleGuess(ch, studentId, gameId, btn){
  if(word.includes(ch)) guessed.add(ch); else wrong++;
  if(btn) btn.disabled=true;
  draw();
  if(word.split('').every(c=>guessed.has(c))){
    document.removeEventListener('keydown', onKey);
    finish(true, studentId, gameId);
    cards.splice(idx,1);
    idx = idx % (cards.length||1);
    startNext(studentId, gameId);
  }
  if(wrong>=6){
    document.removeEventListener('keydown', onKey);
    finish(false, studentId, gameId);
    startNext(studentId, gameId);
  }
}

function setup(studentId, gameId){
  guessed.clear(); wrong=0; startTime=Date.now();
  const card=cards[idx];
  const q=card.gameQuestions?.find(g=>g.gameId===parseInt(gameId));
  questionEl.textContent=q?.questionText||'';
  draw();
  document.addEventListener('keydown', onKey);
  document.querySelectorAll('.letter-btn').forEach(b=>{b.disabled=false; b.onclick=()=>handleGuess(b.textContent.toLowerCase(), studentId, gameId, b);});
}

function notifyParent(){
  if(window.parent!==window) window.parent.postMessage('next-game','*');
}

function startNext(studentId, gameId){
  if(cards.length===0){
    notifyParent();
    return;
  }
  idx=(idx)%cards.length;
  const q=cards[idx].gameQuestions?.find(g=>g.gameId===parseInt(gameId));
  word=(q?.answerText||cards[idx].word).toLowerCase();
  setup(studentId, gameId);
}
