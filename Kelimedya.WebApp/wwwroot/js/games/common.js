export async function fetchLearnedWords(studentId){
  const res = await fetch(`/api/progress/wordcards/learned/${studentId}`);
  if(!res.ok) return [];
  const data = await res.json();
  return data.map(x => x.wordCard);
}

export async function recordGameStat(stat){
  await fetch('/api/gamestats/record', {
    method:'POST',
    headers:{'Content-Type':'application/json'},
    body:JSON.stringify(stat)
  });
}
