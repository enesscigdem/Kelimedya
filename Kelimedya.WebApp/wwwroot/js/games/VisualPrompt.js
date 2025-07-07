import {fetchLearnedWords, awardScore, fetchWordCardWithQuestions} from './common.js';

let cards=[], idx=0, start;
let singleMode=false;
let imgEl, guessEl, feedbackEl, questionEl;

function loadCard(){
  const card=cards[idx];
  const gid=parseInt(document.getElementById('gameRoot').dataset.gameId);
  const q=card.gameQuestions?.find(g=>g.gameId===gid);
  imgEl.src=(q?.imageUrl)||card.imageUrl||'';
  questionEl.textContent=q?.questionText||'';
  guessEl.value='';
  feedbackEl.textContent='';
  start=Date.now();
}

function submit(studentId, gameId){
  const user=guessEl.value.trim().toLowerCase();
  const card=cards[idx];
  const q=card.gameQuestions?.find(g=>g.gameId===parseInt(gameId));
  const answer=(q?.answerText||card.word).toLowerCase();
  const success=user===answer;
  feedbackEl.textContent=success?'Doğru':'Yanlış';
  const duration=(Date.now()-start)/1000;
  awardScore(studentId, gameId, success, duration);
  if(success) cards.splice(idx,1);
  if(cards.length===0){
    notifyParent();
    return;
  }
}

function reveal(){
  const card=cards[idx];
  const gid=parseInt(document.getElementById('gameRoot').dataset.gameId);
  const q=card.gameQuestions?.find(g=>g.gameId===gid);
  guessEl.value=q?.answerText||card.word;
}

export async function initVisualPrompt(studentId, gameId, single, wordId){
  if(single){
    if(wordId){
      const card=await fetchWordCardWithQuestions(wordId);
      cards=card ? [card] : [{word:single,imageUrl:'',definition:'',exampleSentence:''}];
    }else{
      cards=[{word:single,imageUrl:'',definition:'',exampleSentence:''}];
    }
    singleMode=true;
  }else{
    cards=await fetchLearnedWords(studentId);
    if(cards.length===0) cards=[{word:'örnek',imageUrl:'',definition:'',exampleSentence:''}];
    singleMode=false;
  }
  imgEl=document.getElementById('vpImage');
  questionEl=document.getElementById('vpQuestion');
  guessEl=document.getElementById('vpGuess');
  feedbackEl=document.getElementById('vpFeedback');
  document.getElementById('vpSubmit').onclick=()=>submit(studentId, gameId);
  const nextBtn=document.getElementById('vpNext');
  if(nextBtn){
    nextBtn.onclick=()=>{idx=(idx+1)%cards.length;loadCard();};
    if(singleMode) nextBtn.style.display='none';
  }
  document.getElementById('vpReveal').onclick=reveal;
  if(singleMode){
    const back=document.getElementById('vpBack');
    if(back) back.style.display='none';
  }
  loadCard();
}

function notifyParent(){
  if(window.parent!==window) window.parent.postMessage('next-game','*');
}
