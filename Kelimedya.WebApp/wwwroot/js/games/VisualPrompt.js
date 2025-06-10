import {fetchLearnedWords, recordGameStat} from './common.js';

let cards=[], idx=0, start;
let imgEl, guessEl, feedbackEl, questionEl;

function loadCard(){
  const card=cards[idx];
  const q=card.gameQuestions?.find(g=>g.gameId===6);
  imgEl.src=(q?.imageUrl)||card.imageUrl||'';
  questionEl.textContent=q?.questionText||'';
  guessEl.value='';
  feedbackEl.textContent='';
  start=Date.now();
}

function submit(studentId){
  const user=guessEl.value.trim().toLowerCase();
  const card=cards[idx];
  const q=card.gameQuestions?.find(g=>g.gameId===6);
  const answer=(q?.answerText||card.word).toLowerCase();
  const success=user===answer;
  feedbackEl.textContent=success?'Doğru':'Yanlış';
  const duration=(Date.now()-start)/1000;
  recordGameStat({studentId,gameId:6,score:success?1:0,durationSeconds:duration});
}

export async function initVisualPrompt(studentId){
  cards=await fetchLearnedWords(studentId);
  if(cards.length===0) cards=[{word:'örnek',imageUrl:'',definition:'',exampleSentence:''}];
  imgEl=document.getElementById('vpImage');
  questionEl=document.getElementById('vpQuestion');
  guessEl=document.getElementById('vpGuess');
  feedbackEl=document.getElementById('vpFeedback');
  document.getElementById('vpSubmit').onclick=()=>submit(studentId);
  document.getElementById('vpNext').onclick=()=>{idx=(idx+1)%cards.length;loadCard();};
  loadCard();
}
