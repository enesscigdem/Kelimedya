(function(){
  function init(){
    const btn = document.getElementById('kbEndGame');
    if(btn){
      const home = btn.dataset.home;
      btn.addEventListener('click', function(){
        if(home) window.location = home;
      });
    }
    // TODO: oyun verilerini yükle, soruları göster, vb.
  }
  document.addEventListener('DOMContentLoaded', init);
})();
