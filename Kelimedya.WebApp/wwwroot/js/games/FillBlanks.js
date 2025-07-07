import {fetchLearnedWords, awardScore} from './common.js';

let items=[], idx=0, start;
let singleMode=false;

function createItems(words, gameId){
  return words.map(w=>{
    const q=w.gameQuestions?.find(g=>g.gameId===parseInt(gameId));
    const ans=(q?.answerText||w.word).toLowerCase();
    const sentence=q?.questionText||w.exampleSentence||`${w.word} cümle içinde`;
    const regex=new RegExp(ans,'i');
    const parts=sentence.split(regex);
    return {parts, answers:[ans]};
  });
}

function renderCard(){
  const fb=document.getElementById('fbFeedback');fb.textContent='';fb.className='fb-feedback';
  document.querySelectorAll('.fb-input').forEach(i=>i.remove());
  const card=items[idx], cont=document.getElementById('fbSentence');
  cont.innerHTML='';
  card.parts.forEach((txt,i)=>{
    cont.appendChild(document.createTextNode(txt));
    if(i<card.answers.length){const inp=document.createElement('input');inp.type='text';inp.className='fb-input';inp.dataset.index=i;inp.placeholder='_____';cont.appendChild(inp);} });
  start=Date.now();
}

function check(studentId, gameId){
  let all=true;items[idx].answers.forEach((a,i)=>{const inp=document.querySelector(`.fb-input[data-index="${i}"]`);if(inp.value.trim().toLowerCase()===a){inp.classList.add('correct');}else{inp.classList.add('wrong');all=false;}});
  document.getElementById('fbFeedback').textContent=all?'Doğru!':'Yanlış';
  const duration=(Date.now()-start)/1000;
  awardScore(studentId, gameId, all, duration);
  if(all) items.splice(idx,1);
  if(items.length===0){
    notifyParent();
    return;
  }
}

function reveal(){
  items[idx].answers.forEach((a,i)=>{
    const inp=document.querySelector(`.fb-input[data-index="${i}"]`);
    if(inp) inp.value=a;
  });
}

export async function initFillBlanks(studentId, gameId, single){
  let words;
  if(single){
    words=[{word:single,synonym:'',definition:'',exampleSentence:''}];
    singleMode=true;
  }else{
    words=await fetchLearnedWords(studentId);
    singleMode=false;
  }
  items=createItems(words, gameId);
  if(items.length===0) items=[{parts:['___ örnek cümle'],answers:['örnek']}];
  document.getElementById('fbCheck').onclick=()=>check(studentId, gameId);
  const nextBtn=document.getElementById('fbNext');
  nextBtn.onclick=()=>{idx=(idx+1)%items.length;renderCard();};
  document.getElementById('fbReveal').onclick=reveal;
  if(singleMode) nextBtn.style.display='none';
  renderCard();
}

function notifyParent(){
  if(window.parent!==window) window.parent.postMessage('next-game','*');
}
