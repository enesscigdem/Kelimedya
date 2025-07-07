import {fetchLearnedWords, awardScore, fetchWordCardWithQuestions} from './common.js';

let cards = [], cardIdx = 0, filled = [], answer = '', start;
let singleMode=false;

function shuffle(a){ for(let i=a.length-1;i>0;i--){ const j=Math.floor(Math.random()*(i+1)); [a[i],a[j]]=[a[j],a[i]]; } }

function loadCard(){
  const card = cards[cardIdx];
  const q = card.gameQuestions?.find(g=>g.gameId===parseInt(document.getElementById('gameRoot').dataset.gameId));
  answer = (q?.answerText || card.word).toLowerCase().replace(/\s+/g,'');
  filled = Array(answer.length).fill('');
  document.getElementById('hintSynonym').textContent = card.synonym;
  document.getElementById('hintDefinition').textContent = q?.questionText || card.definition;
  document.getElementById('hintSentence').textContent = card.exampleSentence;
  const target=document.getElementById('targetSlots');
  target.innerHTML='';
  for(let i=0;i<answer.length;i++){
    const span=document.createElement('span'); span.className='bl-letter'; span.id=`slot${i}`; span.textContent='_';
    target.appendChild(span);
  }
  createBubbles();
  start = Date.now();
}

function createBubbles(){
  const area=document.getElementById('bubblesArea');
  area.innerHTML='';
  let letters=answer.split('');
  const extras='abcçdefgğhıijklmnoöprsştuüvyz'.split('');
  while(letters.length<answer.length*2) letters.push(extras[Math.floor(Math.random()*extras.length)]);
  shuffle(letters);
  letters.forEach(ch=>{
    const btn=document.createElement('button');
    btn.className='bubble-btn';
    btn.textContent=ch;
    btn.onclick=()=>selectLetter(ch,btn);
    area.appendChild(btn);
  });
}

function selectLetter(ch,btn){
  const idx=filled.findIndex(x=>x==='');
  if(idx===-1) return;
  filled[idx]=ch;
  document.getElementById(`slot${idx}`).textContent=ch;
  btn.disabled=true;
}

function check(studentId, gameId){
  const guess=filled.join('');
  const correct=guess===answer;
  document.getElementById('blFeedback').textContent = correct ? 'Doğru!' : 'Yanlış';
  const duration=(Date.now()-start)/1000;
  awardScore(studentId, gameId, correct, duration);
  if(correct) cards.splice(cardIdx,1);
  if(cards.length===0){
    notifyParent();
    return;
  }
}

function clearLetters(){
  filled = Array(answer.length).fill('');
  document.querySelectorAll('.bl-letter').forEach(l=>l.textContent='_');
  document.querySelectorAll('.bubble-btn').forEach(b=>b.disabled=false);
}

function reveal(){
  filled = answer.split('');
  filled.forEach((c,i)=>document.getElementById(`slot${i}`).textContent=c);
}

export async function initBubbleLetters(studentId, gameId, single, wordId){
  if(single){
    if(wordId){
      const card=await fetchWordCardWithQuestions(wordId);
      cards=card ? [card] : [{word:single,synonym:'',definition:'',exampleSentence:''}];
    }else{
      cards=[{word:single,synonym:'',definition:'',exampleSentence:''}];
    }
    singleMode=true;
  }else{
    cards = await fetchLearnedWords(studentId);
    if(cards.length===0) cards=[{word:'örnek', synonym:'', definition:'', exampleSentence:''}];
    singleMode=false;
  }
  document.getElementById('blSubmit').onclick=()=>check(studentId, gameId);
  const nextBtn=document.getElementById('blNext');
  if(nextBtn){
    nextBtn.onclick=()=>{cardIdx=(cardIdx+1)%cards.length;loadCard();};
  }
  document.getElementById('blClear').onclick=clearLetters;
  document.getElementById('blReveal').onclick=reveal;
  if(singleMode){
    if(nextBtn) nextBtn.style.display='none';
    const back=document.getElementById('blBack');
    if(back) back.style.display='none';
  }
  loadCard();
}

function notifyParent(){
  if(window.parent!==window) window.parent.postMessage('next-game','*');
}
