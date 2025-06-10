const API_BASE_URL = 'http://localhost:5001';

export async function fetchLearnedWords(studentId) {
  const res = await fetch(`${API_BASE_URL}/api/progress/wordcards/learned/${studentId}`);
  if (!res.ok) return [];
  const data = await res.json();
  return data.map(x => ({ ...x.wordCard, gameQuestions: x.gameQuestions }));
}

export async function recordGameStat(stat) {
  await fetch(`${API_BASE_URL}/api/gamestats/record`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(stat),
  });
}