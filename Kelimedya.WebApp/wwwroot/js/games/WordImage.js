import {fetchLearnedWords, awardScore, fetchWordCardWithQuestions} from './common.js';

let cards=[], idx=0, start;
let singleMode=false;
let wordEl, optionsEl, feedbackEl;

function loadCard(){
  const card=cards[idx];
  const gid=parseInt(document.getElementById('gameRoot').dataset.gameId);
  const q=card.gameQuestions?.find(g=>g.gameId===gid);
  wordEl.textContent=q?.questionText||card.word;
  const opts=[q?.imageUrl,q?.imageUrl2,q?.imageUrl3,q?.imageUrl4].filter(Boolean);
  const correct=q?.correctOption? opts[q.correctOption-1]:opts[0];
  optionsEl.innerHTML='';
  opts.forEach((img)=>{
    const i=document.createElement('img');
    i.src=img;i.className='wti-option';
    i.onclick=()=>select(img===correct);
    optionsEl.appendChild(i);
  });
  feedbackEl.textContent='';
  start=Date.now();
}

function select(success){
  const studentId=document.getElementById('gameRoot').dataset.studentId;
  const gameId=document.getElementById('gameRoot').dataset.gameId;
  feedbackEl.textContent=success?'Doğru':'Yanlış';
  const duration=(Date.now()-start)/1000;
  awardScore(studentId, gameId, success, duration);
  if(success) cards.splice(idx,1);
  if(cards.length===0){notifyParent();return;}
  idx=(idx+1)%cards.length;
  loadCard();
}

function reveal(){
  const card=cards[idx];
  const gid=parseInt(document.getElementById('gameRoot').dataset.gameId);
  const q=card.gameQuestions?.find(g=>g.gameId===gid);
  guessEl.value=q?.answerText||card.word;
}

export async function initWordImage(studentId, gameId, single, wordId){
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
  wordEl=document.getElementById('wtiWord');
  optionsEl=document.getElementById('wtiOptions');
  feedbackEl=document.getElementById('wtiFeedback');
  const nextBtn=document.getElementById('wtiNext');
  if(nextBtn){
    nextBtn.onclick=()=>{idx=(idx+1)%cards.length;loadCard();};
    if(singleMode) nextBtn.style.display='none';
  }
  if(singleMode){
    const back=document.getElementById('wtiBack');
    if(back) back.style.display='none';
  }
  loadCard();
}

function notifyParent(){
  if(window.parent!==window) window.parent.postMessage('next-game','*');
}
