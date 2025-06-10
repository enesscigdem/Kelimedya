import {fetchLearnedWords, recordGameStat} from './common.js';

let word = '', guessed = new Set(), wrong = 0, startTime;

function draw(){
  const display = word.split('').map(ch=>guessed.has(ch)?ch:'_').join(' ');
  document.getElementById('hangmanWord').textContent = display;
  document.getElementById('wrongCount').textContent = wrong;
}

function finish(success, studentId){
  const duration = (Date.now()-startTime)/1000;
  recordGameStat({studentId, gameId:1, score: success?1:0, durationSeconds: duration});
}

export async function initHangman(studentId){
  const words = await fetchLearnedWords(studentId);
  word = (words[0]?.word || 'kelime').toLowerCase();
  guessed.clear();
  wrong = 0;
  startTime = Date.now();
  draw();
  document.addEventListener('keydown', onKey);
}

function onKey(e){
  if(e.key.length!==1) return;
  const ch = e.key.toLowerCase();
  if(word.includes(ch)) guessed.add(ch); else wrong++;
  draw();
  if(word.split('').every(c=>guessed.has(c))){
    document.removeEventListener('keydown', onKey);
    finish(true, document.getElementById('gameRoot').dataset.studentId);
  }
  if(wrong>=6){
    document.removeEventListener('keydown', onKey);
    finish(false, document.getElementById('gameRoot').dataset.studentId);
  }
}
