import {fetchLearnedWords, recordGameStat} from './common.js';

let cards=[], idx=0, start;

function load(){
  const card=cards[idx];
  document.getElementById('questionBox').textContent=card.definition;
  const opts=document.getElementById('optionsArea');
  opts.innerHTML='';
  const answers=[card.word, ...(cards.slice(idx+1,idx+4).map(c=>c.word))];
  while(answers.length<4) answers.push(card.word);
  for(let i=answers.length-1;i>0;i--){const j=Math.floor(Math.random()*(i+1));[answers[i],answers[j]]=[answers[j],answers[i]];}
  answers.forEach(ans=>{
    const b=document.createElement('button');b.className='option-btn';b.textContent=ans;b.onclick=()=>select(ans,card.word);
    opts.appendChild(b);
  });
  start=Date.now();
}

function select(ans,correct){
  const ok=ans===correct;document.getElementById('questionBox').textContent= ok?'Doğru':'Yanlış';
  const duration=(Date.now()-start)/1000;
  recordGameStat({studentId:document.getElementById('gameRoot').dataset.studentId,gameId:5,score:ok?1:0,durationSeconds:duration});
}

export async function initWordQuiz(studentId){
  cards=await fetchLearnedWords(studentId);
  if(cards.length===0) cards=[{word:'örnek',definition:'örnek tanım'}];
  document.getElementById('kbEndGame').onclick=()=>{window.location=document.getElementById('kbEndGame').dataset.home;};
  document.getElementById('kbNext').onclick=()=>{idx=(idx+1)%cards.length;load();};
  load();
}
