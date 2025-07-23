import {fetchLearnedWords, awardScore, fetchWordCardWithQuestions} from './common.js';

let items=[], idx=0, start;
let selected=[], singleMode=false;

function shuffle(arr){ for(let i=arr.length-1;i>0;i--){ const j=Math.floor(Math.random()*(i+1)); [arr[i],arr[j]]=[arr[j],arr[i]]; } }

function createItems(cards, gameId){
  return cards.map(c=>{
    const q=c.gameQuestions?.find(g=>g.gameId===parseInt(gameId));
    if(!q) return null;
    const words=q.questionText.split('-');
    const scrambled=[...words];
    shuffle(scrambled);
    return {words:scrambled, answer:q.answerText};
  }).filter(Boolean);
}

function render(){
  const item=items[idx];
  const opts=document.getElementById('sbOptions');
  opts.innerHTML='';
  item.words.forEach((w,i)=>{
    const b=document.createElement('button'); b.className='sb-word'; b.textContent=w; b.onclick=()=>selectWord(i,b); opts.appendChild(b);
  });
  document.getElementById('sbSentence').textContent='';
  selected=[];
  start=Date.now();
}

function selectWord(i,btn){
  const item=items[idx];
  selected.push(item.words[i]);
  btn.disabled=true;
  document.getElementById('sbSentence').textContent=selected.join(' ');
}

function check(studentId, gameId){
  const item=items[idx];
  const success=selected.join(' ').trim()===item.answer.trim();
  document.getElementById('sbFeedback').textContent=success?'Doğru!':'Yanlış';
  const duration=(Date.now()-start)/1000;
  awardScore(studentId, gameId, success, duration);
  if(success) items.splice(idx,1);
  if(items.length===0){ notifyParent(); return; }
}

function reveal(){ document.getElementById('sbSentence').textContent=items[idx].answer; }

function next(){ idx=(idx+1)%items.length; render(); }

export async function initSentenceBuilder(studentId, gameId, single, wordId){
  let cards;
  if(single){
    if(wordId){
      const card=await fetchWordCardWithQuestions(wordId);
      cards=card? [card] : [{gameQuestions:[{gameId:parseInt(gameId),questionText:single,answerText:single}]}];
    }else{
      cards=[{gameQuestions:[{gameId:parseInt(gameId),questionText:single,answerText:single}]}];
    }
    singleMode=true;
  }else{
    cards=await fetchLearnedWords(studentId);
    singleMode=false;
  }
  items=createItems(cards, gameId);
  if(items.length===0) items=[{words:['örnek','cümle'],answer:'örnek cümle'}];
  document.getElementById('sbCheck').onclick=()=>check(studentId, gameId);
  document.getElementById('sbReveal').onclick=reveal;
  const nextBtn=document.getElementById('sbNext');
  if(nextBtn){ nextBtn.onclick=next; if(singleMode) nextBtn.style.display='none'; }
  if(singleMode){ const back=document.getElementById('sbBack'); if(back) back.style.display='none'; }
  render();
}

function notifyParent(){ if(window.parent!==window) window.parent.postMessage('next-game','*'); }
