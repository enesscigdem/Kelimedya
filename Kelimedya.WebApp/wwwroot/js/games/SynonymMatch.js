import {fetchLearnedWords, awardScore, fetchWordCardWithQuestions} from './common.js';

let pairs=[], displayed=[], pool=[], start;
let selectedLeft=null, selectedRight=null; let singleMode=false;

function shuffle(arr){ for(let i=arr.length-1;i>0;i--){const j=Math.floor(Math.random()*(i+1));[arr[i],arr[j]]=[arr[j],arr[i]];} }

function render(){
  const left=document.getElementById('leftColumn');
  const right=document.getElementById('rightColumn');
  left.innerHTML=''; right.innerHTML='';
  const lItems=displayed.map(p=>({id:p.id,text:p.left}));
  const rItems=displayed.map(p=>({id:p.id,text:p.right}));
  shuffle(lItems); shuffle(rItems);
  lItems.forEach(it=>{
    const div=document.createElement('div'); div.className='match-item'; div.textContent=it.text;
    div.onclick=()=>chooseLeft(it.id,div); left.appendChild(div);
  });
  rItems.forEach(it=>{
    const div=document.createElement('div'); div.className='match-item'; div.textContent=it.text;
    div.onclick=()=>chooseRight(it.id,div); right.appendChild(div);
  });
  start=Date.now();
}

function resetSelections(){
  if(selectedLeft){selectedLeft.el.classList.remove('active');}
  if(selectedRight){selectedRight.el.classList.remove('active');}
  selectedLeft=null; selectedRight=null;
}

function chooseLeft(id,el){
  if(selectedLeft) selectedLeft.el.classList.remove('active');
  selectedLeft={id,el}; el.classList.add('active');
  if(selectedRight) checkMatch();
}

function chooseRight(id,el){
  if(selectedRight) selectedRight.el.classList.remove('active');
  selectedRight={id,el}; el.classList.add('active');
  if(selectedLeft) checkMatch();
}

function checkMatch(){
  const ok=selectedLeft.id===selectedRight.id;
  const duration=(Date.now()-start)/1000;
  const gid=document.getElementById('gameRoot').dataset.gameId;
  const sid=document.getElementById('gameRoot').dataset.studentId;
  awardScore(sid,gid,ok,duration);
  if(ok){
    selectedLeft.el.classList.add('correct');
    selectedRight.el.classList.add('correct');
    displayed=displayed.filter(p=>p.id!==selectedLeft.id);
    setTimeout(()=>{ refill(); },300);
  }
  resetSelections();
  if(displayed.length===0 && pool.length===0){ notifyParent(); }
}

function refill(){
  while(displayed.length<5 && pool.length>0){ displayed.push(pool.shift()); }
  if(displayed.length===0){ notifyParent(); return; }
  render();
}

export async function initSynonymMatch(studentId, gameId, single, wordId){
  let cards;
  if(single){
    if(wordId){
      const card=await fetchWordCardWithQuestions(wordId);
      cards=card? [card] : [{word:single,synonym:single,gameQuestions:[]}];
    }else{
      cards=[{word:single,synonym:single,gameQuestions:[]}];
    }
    singleMode=true;
  }else{
    cards=await fetchLearnedWords(studentId);
    if(cards.length===0) cards=[{word:'örnek',synonym:'benzer',gameQuestions:[]}];
    singleMode=false;
  }
  const gid=parseInt(gameId);
  pairs=cards.map((c,i)=>{
    const q=c.gameQuestions?.find(g=>g.gameId===gid);
    return {id:i,left:q?.questionText||c.word,right:q?.answerText||c.synonym};
  });
  shuffle(pairs);
  displayed=pairs.slice(0,5);
  pool=pairs.slice(5);
  const endBtn=document.getElementById('smEndGame');
  if(endBtn){ endBtn.onclick=()=>{window.location=endBtn.dataset.home;}; if(singleMode) endBtn.style.display='none'; }
  render();
}

function notifyParent(){ if(window.parent!==window) window.parent.postMessage('next-game','*'); }
