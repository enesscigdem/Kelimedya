import {fetchLearnedWords, awardScore} from './common.js';

let word = '', guessed = new Set(), wrong = 0, startTime, cards=[], idx=0;
let singleMode = false;

function draw(){
  const display = word.split('').map(ch=>guessed.has(ch)?ch:'_').join(' ');
  document.getElementById('hangmanWord').textContent = display;
  document.getElementById('wrongCount').textContent = wrong;
}

function finish(success, studentId, gameId){
  const duration = (Date.now()-startTime)/1000;
  awardScore(studentId, gameId, success, duration);
}

export async function initHangman(studentId, gameId, single){
  if(single){
    cards=[{word:single}];
    singleMode=true;
  }else{
    cards = await fetchLearnedWords(studentId);
    if(cards.length===0) cards=[{word:'kelime'}];
    singleMode=false;
  }
  idx=0;
  const q=cards[idx].gameQuestions?.find(g=>g.gameId===parseInt(gameId));
  word = (q?.answerText||cards[idx].word).toLowerCase();
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
  guessed.clear(); wrong=0; startTime=Date.now(); draw();
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
