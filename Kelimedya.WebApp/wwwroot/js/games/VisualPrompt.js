import {fetchLearnedWords, awardScore} from './common.js';

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

function submit(studentId, gameId){
  const user=guessEl.value.trim().toLowerCase();
  const card=cards[idx];
  const q=card.gameQuestions?.find(g=>g.gameId===6);
  const answer=(q?.answerText||card.word).toLowerCase();
  const success=user===answer;
  feedbackEl.textContent=success?'Doğru':'Yanlış';
  const duration=(Date.now()-start)/1000;
  awardScore(studentId, gameId, success, duration);
  if(success) cards.splice(idx,1);
}

function reveal(){
  const card=cards[idx];
  const q=card.gameQuestions?.find(g=>g.gameId===6);
  guessEl.value=q?.answerText||card.word;
}

export async function initVisualPrompt(studentId, gameId){
  cards=await fetchLearnedWords(studentId);
  if(cards.length===0) cards=[{word:'örnek',imageUrl:'',definition:'',exampleSentence:''}];
  imgEl=document.getElementById('vpImage');
  questionEl=document.getElementById('vpQuestion');
  guessEl=document.getElementById('vpGuess');
  feedbackEl=document.getElementById('vpFeedback');
  document.getElementById('vpSubmit').onclick=()=>submit(studentId, gameId);
  document.getElementById('vpNext').onclick=()=>{idx=(idx+1)%cards.length;loadCard();};
  document.getElementById('vpReveal').onclick=reveal;
  loadCard();
}
