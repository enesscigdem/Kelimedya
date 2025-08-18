import {fetchLearnedWords, awardScore, fetchWordCardWithQuestions} from './common.js';

let items=[], idx=0, start;
let singleMode=false;
let revealUsed=false;
let checking=false;

function createItems(words, gameId){
  return words.map(w=>{
    const q=w.gameQuestions?.find(g=>g.gameId===parseInt(gameId));
    const ans=(q?.answerText||w.word).toLocaleLowerCase('tr');
    const sentence=q?.questionText||w.exampleSentence||`${w.word} cümle içinde`;
    const regex=new RegExp(ans,'i');
    const parts=sentence.split(regex);
    return {parts, answers:[ans]};
  });
}

function renderCard(){
  revealUsed=false;
  const fb=document.getElementById('fbFeedback');fb.textContent='';fb.className='fb-feedback';
  document.querySelectorAll('.fb-input').forEach(i=>i.remove());
  const card=items[idx], cont=document.getElementById('fbSentence');
  cont.innerHTML='';
  card.parts.forEach((txt,i)=>{
    cont.appendChild(document.createTextNode(txt));
    if(i<card.answers.length){const inp=document.createElement('input');inp.type='text';inp.className='fb-input';inp.dataset.index=i;inp.placeholder='_____';cont.appendChild(inp);} });
  start=Date.now();
}

function disableButtons(disabled){
  document.querySelectorAll('#fbCheck,#fbNext,#fbReveal').forEach(btn=>{ if(btn) btn.disabled=disabled; });
}

function check(studentId, gameId){
  if(checking) return; checking=true;
  disableButtons(true);
  let all=true;items[idx].answers.forEach((a,i)=>{const inp=document.querySelector(`.fb-input[data-index="${i}"]`);if(inp.value.trim().toLocaleLowerCase('tr')===a){inp.classList.add('correct');}else{inp.classList.add('wrong');all=false;}});
  const correctText=items[idx].answers.join(', ');
  document.getElementById('fbFeedback').textContent=all? 'Doğru!' : `Yanlış! Doğru: ${correctText}`;
  const duration=(Date.now()-start)/1000;
  awardScore(studentId, gameId, all && !revealUsed, duration, correctText, !revealUsed);
  if(all && !revealUsed) items.splice(idx,1);
  setTimeout(()=>{
    if(items.length===0){
      notifyParent();
      return;
    }
    idx = idx % items.length;
    checking=false;
    disableButtons(false);
    renderCard();
  },2000);
}

function reveal(){
  revealUsed=true;
  items[idx].answers.forEach((a,i)=>{
    const inp=document.querySelector(`.fb-input[data-index="${i}"]`);
    if(inp) inp.value=a;
  });
}

export async function initFillBlanks(studentId, gameId, single, wordId){
  let words;
  if(single){
    if(wordId){
      const card=await fetchWordCardWithQuestions(wordId);
      words=card ? [card] : [{word:single,synonym:'',definition:'',exampleSentence:''}];
    }else{
      words=[{word:single,synonym:'',definition:'',exampleSentence:''}];
    }
    singleMode=true;
  }else{
    words=await fetchLearnedWords(studentId);
    singleMode=false;
  }
  items=createItems(words, gameId);
  if(items.length===0) items=[{parts:['___ örnek cümle'],answers:['örnek']}];
  document.getElementById('fbCheck').onclick=()=>check(studentId, gameId);
  const nextBtn=document.getElementById('fbNext');
  if(nextBtn){
    nextBtn.onclick=()=>{idx=(idx+1)%items.length;renderCard();};
    if(singleMode) nextBtn.style.display='none';
  }
  document.getElementById('fbReveal').onclick=reveal;
  renderCard();
}

function notifyParent(){
  if(window.parent!==window) window.parent.postMessage('next-game','*');
}
