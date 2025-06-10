const API_BASE_URL = 'http://localhost:5001';

export async function fetchLearnedWords(studentId) {
  const res = await fetch(`${API_BASE_URL}/api/progress/wordcards/learned/${studentId}`);
  if (!res.ok) return [];
  const data = await res.json();
  return data.map(x => ({ ...x.wordCard, gameQuestions: x.gameQuestions }));
}

export async function recordGameStat(stat) {
  const payload = {
    ...stat,
    gameId: parseInt(stat.gameId, 10),
  };
  await fetch(`${API_BASE_URL}/api/gamestats/record`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
}

export async function awardScore(studentId, gameId, success, durationSeconds) {
  const score = success ? Math.max(10, Math.floor(100 - durationSeconds * 5)) : 0;
  await recordGameStat({ studentId, gameId, score, durationSeconds });
  if (success) {
    if (window.showIziToastSuccess) {
      window.showIziToastSuccess(`+${score} puan`);
    }
    incrementScore(score);
  }
}

export function incrementScore(amount) {
  const el = document.getElementById('scorePoints');
  if (!el) return;
  const newScore = parseInt(el.textContent || '0', 10) + amount;
  el.textContent = newScore;
  const leagueEl = document.getElementById('leagueLabel');
  if (leagueEl) {
    let league = 'Bronz';
    if (newScore >= 6000) league = 'Altın';
    else if (newScore >= 2500) league = 'Gümüş';
    leagueEl.textContent = league;
  }
}