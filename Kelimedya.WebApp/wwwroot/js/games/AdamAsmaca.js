(function(){
  function init(){
    const btn = document.getElementById('endGameBtn');
    if(btn){
      const home = btn.dataset.home;
      btn.addEventListener('click', function(){
        if(home) window.location = home;
      });
    }
    // TODO: oyun başlangıç kodu
  }
  document.addEventListener('DOMContentLoaded', init);
})();
