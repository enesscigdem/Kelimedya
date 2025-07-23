const API_BASE_URL = 'http://localhost:5001';

export async function fetchLearnedWords(studentId) {
  const res = await fetch(`${API_BASE_URL}/api/progress/wordcards/learned/${studentId}`);
  if (!res.ok) return [];
  const data = await res.json();
  return data.map(x => ({ ...x.wordCard, gameQuestions: x.gameQuestions }));
}

export async function fetchWordCardWithQuestions(id) {
  const cardRes = await fetch(`${API_BASE_URL}/api/wordcards/${id}`);
  if (!cardRes.ok) return null;
  const card = await cardRes.json();
  const qRes = await fetch(`${API_BASE_URL}/api/wordcards/${id}/questions`);
  const questions = qRes.ok ? await qRes.json() : [];
  return { ...card, gameQuestions: questions };
}

export async function recordGameStat(stat) {
  const payload = { ...stat, gameId: parseInt(stat.gameId, 10) };

  const response = await fetch(`${API_BASE_URL}/api/gamestats/record`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });

  if (!response.ok) return null;

  // Kayıt başarılıysa güncel skoru al ve UI'ı güncelle
  const scoreResponse = await fetch(`${API_BASE_URL}/api/gamestats/score/${stat.studentId}`);
  if (!scoreResponse.ok) return null;

  const scoreInfo = await scoreResponse.json();
  if (typeof updateScoreDisplay === 'function') {
    updateScoreDisplay(scoreInfo.totalScore);
  } else {
    // fallback
    document.getElementById('scorePoints').textContent = scoreInfo.totalScore;
    document.getElementById('leagueLabel').textContent = scoreInfo.league;
  }

  return scoreInfo.totalScore;
}


export async function awardScore(studentId, gameId, success, durationSeconds) {
  const score = success
      ? Math.max(10, Math.floor(100 - durationSeconds * 5))
      : 0;

  const newTotal = await recordGameStat({ studentId, gameId, score, durationSeconds });

  if (success) {
    showIziToastSuccess(`+${score} puan`);

    const correctAudio = document.getElementById('correctAudio');
    if (correctAudio) {
      correctAudio.currentTime = 0;
      correctAudio.play().catch(() => {
        console.warn('Doğru cevap sesi çalınamadı');
      });
    }
  }
}



export function incrementScore(amount) {
  const el = document.getElementById('scorePoints');
  if (!el) return;
  const newScore = parseInt(el.textContent || '0', 10) + amount;

  // Sayfada tanımlı updateScoreDisplay fonksiyonunu çağır
  if (typeof updateScoreDisplay === 'function') {
    updateScoreDisplay(newScore);
  } else {
    // Fallback eski davranış
    el.textContent = newScore;
    const leagueEl = document.getElementById('leagueLabel');
    if (leagueEl) {
      let league = 'Bronz';
      if (newScore >= 6000) league = 'Altın';
      else if (newScore >= 2500) league = 'Gümüş';
      leagueEl.textContent = league;
    }
  }
}
