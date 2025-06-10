import {fetchLearnedWords, awardScore} from './common.js';

let items=[], idx=0, start;

function createItems(words){
  return words.map(w=>{
    const sentence=w.exampleSentence||`${w.word} cümle içinde`;
    const regex=new RegExp(w.word,'i');
    const parts=sentence.split(regex);
    return {parts, answers:[w.word.toLowerCase()]};
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
}

function reveal(){
  items[idx].answers.forEach((a,i)=>{
    const inp=document.querySelector(`.fb-input[data-index="${i}"]`);
    if(inp) inp.value=a;
  });
}

export async function initFillBlanks(studentId, gameId){
  const words=await fetchLearnedWords(studentId);
  items=createItems(words);
  if(items.length===0) items=[{parts:['___ örnek cümle'],answers:['örnek']}];
  document.getElementById('fbCheck').onclick=()=>check(studentId, gameId);
  document.getElementById('fbNext').onclick=()=>{idx=(idx+1)%items.length;renderCard();};
  document.getElementById('fbReveal').onclick=reveal;
  renderCard();
}
