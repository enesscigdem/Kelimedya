import {fetchLearnedWords, awardScore} from './common.js';

let cards=[], idx=0, start;
let singleMode=false;

function load(){
  const card=cards[idx];
  const gid=parseInt(document.getElementById('gameRoot').dataset.gameId);
  const q=card.gameQuestions?.find(g=>g.gameId===gid);

  document.getElementById('questionBox').textContent=q?.questionText||card.definition;
  const opts=document.getElementById('optionsArea');
  opts.innerHTML='';

  let answers=[]; let correct='';
  if(q && (q.optionA||q.optionB||q.optionC||q.optionD)){
    answers=[q.optionA,q.optionB,q.optionC,q.optionD].filter(Boolean);
    const idxCorrect=(q.correctOption? q.correctOption-1:0);
    correct=answers[idxCorrect]||answers[0];
  } else {
    correct=q?.answerText||card.word;
    answers=[correct, ...(cards.slice(idx+1,idx+4).map(c=>c.word))];
    while(answers.length<4) answers.push(card.word);
  }

  for(let i=answers.length-1;i>0;i--){const j=Math.floor(Math.random()*(i+1));[answers[i],answers[j]]=[answers[j],answers[i]];}
  answers.forEach(ans=>{
    const b=document.createElement('button');b.className='option-btn';b.textContent=ans;b.onclick=()=>select(ans,correct);
    opts.appendChild(b);
  });
  start=Date.now();
}

function select(ans,correct){
  const ok=ans===correct;document.getElementById('questionBox').textContent= ok?'Doğru':'Yanlış';
  const duration=(Date.now()-start)/1000;
  const gid=document.getElementById('gameRoot').dataset.gameId;
  awardScore(document.getElementById('gameRoot').dataset.studentId, gid, ok, duration);
  if(ok) cards.splice(idx,1);
  if(cards.length===0){
    notifyParent();
    return;
  }
  idx=(idx+1)%cards.length;
  setTimeout(load,500);
}

export async function initWordQuiz(studentId, gameId, single){
  if(single){
    cards=[{word:single,definition:'',synonym:'',exampleSentence:''}];
    singleMode=true;
  }else{
    cards=await fetchLearnedWords(studentId);
    if(cards.length===0) cards=[{word:'örnek',definition:'örnek tanım'}];
    singleMode=false;
  }
  const endBtn=document.getElementById('kbEndGame');
  if(endBtn){
    endBtn.onclick=()=>{window.location=endBtn.dataset.home;};
    if(singleMode) endBtn.style.display='none';
  }
  load();
}

function notifyParent(){
  if(window.parent!==window) window.parent.postMessage('next-game','*');
}
