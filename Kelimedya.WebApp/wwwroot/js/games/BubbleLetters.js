import {fetchLearnedWords, recordGameStat} from './common.js';

let cards = [], cardIdx = 0, filled = [], answer = '', start;

function shuffle(a){ for(let i=a.length-1;i>0;i--){ const j=Math.floor(Math.random()*(i+1)); [a[i],a[j]]=[a[j],a[i]]; } }

function loadCard(){
  const card = cards[cardIdx];
  answer = card.word.toLowerCase().replace(/\s+/g,'');
  filled = Array(answer.length).fill('');
  document.getElementById('hintSynonym').textContent = card.synonym;
  document.getElementById('hintDefinition').textContent = card.definition;
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
  recordGameStat({studentId, gameId, score: correct?1:0, durationSeconds: duration});
  if(correct) cards.splice(cardIdx,1);
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

export async function initBubbleLetters(studentId, gameId){
  cards = await fetchLearnedWords(studentId);
  if(cards.length===0) cards=[{word:'örnek', synonym:'', definition:'', exampleSentence:''}];
  document.getElementById('blSubmit').onclick=()=>check(studentId, gameId);
  document.getElementById('blNext').onclick=()=>{cardIdx=(cardIdx+1)%cards.length;loadCard();};
  document.getElementById('blClear').onclick=clearLetters;
  document.getElementById('blReveal').onclick=reveal;
  loadCard();
}
