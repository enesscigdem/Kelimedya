import {fetchLearnedWords, recordGameStat} from './common.js';

let cards=[], idx=0, start;
let imgEl, guessEl, feedbackEl;

function loadCard(){
  const card=cards[idx];
  imgEl.src=card.imageUrl||'';
  guessEl.value='';
  feedbackEl.textContent='';
  start=Date.now();
}

function submit(studentId){
  const user=guessEl.value.trim().toLowerCase();
  const correct=cards[idx].word.toLowerCase();
  const success=user===correct;
  feedbackEl.textContent=success?'Doğru':'Yanlış';
  const duration=(Date.now()-start)/1000;
  recordGameStat({studentId,gameId:6,score:success?1:0,durationSeconds:duration});
}

export async function initVisualPrompt(studentId){
  cards=await fetchLearnedWords(studentId);
  if(cards.length===0) cards=[{word:'örnek',imageUrl:'',definition:'',exampleSentence:''}];
  imgEl=document.getElementById('vpImage');
  guessEl=document.getElementById('vpGuess');
  feedbackEl=document.getElementById('vpFeedback');
  document.getElementById('vpSubmit').onclick=()=>submit(studentId);
  document.getElementById('vpNext').onclick=()=>{idx=(idx+1)%cards.length;loadCard();};
  loadCard();
}
