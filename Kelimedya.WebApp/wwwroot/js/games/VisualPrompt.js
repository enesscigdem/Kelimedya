(function(){
  const images = [
    { src: "https://images.unsplash.com/photo-1593642634367-d91a135587b5?ixlib=rb-1.2.1...", answer: "dikkat etmek" },
    { src: "https://images.unsplash.com/photo-1517487881594-2787fef5ebf7?ixlib=rb-1.2.1...", answer: "planlamak" },
    { src: "https://images.unsplash.com/photo-1503676260728-1c00da094a0b?ixlib=rb-1.2.1...", answer: "başarmak" }
  ];
  let idx = 0;
  let imgEl, guessEl, feedbackEl, submitBtn, revealBtn, nextBtn;

  function loadCard(){
    const card = images[idx];
    imgEl.src = card.src;
    guessEl.value = '';
    feedbackEl.innerHTML = '';
    feedbackEl.className = 'vp-feedback';
  }

  function init(){
    imgEl = document.getElementById('vpImage');
    guessEl = document.getElementById('vpGuess');
    feedbackEl = document.getElementById('vpFeedback');
    submitBtn = document.getElementById('vpSubmit');
    revealBtn = document.getElementById('vpReveal');
    nextBtn = document.getElementById('vpNext');
    const backBtn = document.getElementById('vpBack');
    if(backBtn){
      const home = backBtn.dataset.home;
      backBtn.addEventListener('click', () => { if(home) window.location = home; });
    }

    submitBtn.onclick = () => {
      const user = guessEl.value.trim().toLowerCase();
      const correct = images[idx].answer.toLowerCase();
      if(!user) return;
      if(user === correct){
        feedbackEl.innerHTML = '<span class="icon">✔︎</span> Doğru!';
        feedbackEl.classList.add('correct');
      } else {
        feedbackEl.innerHTML = '<span class="icon">✖︎</span> Yanlış, tekrar deneyin.';
        feedbackEl.classList.add('incorrect');
      }
    };

    revealBtn.onclick = () => {
      feedbackEl.innerHTML = '<span class="icon">🔍</span> Cevap: ' + images[idx].answer;
      feedbackEl.classList.add('revealed');
    };

    nextBtn.onclick = () => {
      idx = (idx + 1) % images.length;
      loadCard();
    };

    loadCard();
  }

  document.addEventListener('DOMContentLoaded', init);
})();
