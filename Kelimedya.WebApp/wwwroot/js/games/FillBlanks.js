(function(){
  const items = [
    { parts:["Düzenli çalışmak, sınavda "," olmanın anahtarıdır."], answers:["başarılı"] },
    { parts:["Sınav tarihine göre ders programını "," çok önemlidir."], answers:["planlamak"] },
    { parts:["Sınavda soruları "," okuyun."], answers:["dikkatlice"] }
  ];
  let idx=0;

  function renderCard(){
    const fb=document.getElementById('fbFeedback');
    fb.textContent=''; fb.className='fb-feedback';
    document.querySelectorAll('.fb-input').forEach(i=>i.remove());
    const card=items[idx], cont=document.getElementById('fbSentence');
    cont.innerHTML='';
    card.parts.forEach((txt,i)=>{
      cont.appendChild(document.createTextNode(txt));
      if(i<card.answers.length){
        const inp=document.createElement('input');
        inp.type='text'; inp.className='fb-input';
        inp.dataset.index=i; inp.placeholder='_____';
        cont.appendChild(inp);
      }
    });
  }

  function check(){
    let all=true;
    items[idx].answers.forEach((a,i)=>{
      const inp=document.querySelector(`.fb-input[data-index="${i}"]`);
      if(inp.value.trim().toLowerCase()===a){
        inp.classList.add('correct'); inp.classList.remove('wrong');
      } else {
        inp.classList.add('wrong'); inp.classList.remove('correct');
        all=false;
      }
    });
    const fb=document.getElementById('fbFeedback');
    fb.textContent=all?'🎉 Doğru!':'❌ Yanlış yerler.';
    fb.className=all?'fb-feedback correct':'fb-feedback incorrect';
  }

  function reveal(){
    items[idx].answers.forEach((a,i)=>{
      const inp=document.querySelector(`.fb-input[data-index="${i}"]`);
      inp.value=a; inp.classList.remove('wrong'); inp.classList.add('revealed');
    });
    const fb=document.getElementById('fbFeedback');
    fb.textContent='🔍 Cevap gösterildi.'; fb.className='fb-feedback revealed';
  }

  function init(){
    document.getElementById('fbCheck').onclick=check;
    document.getElementById('fbReveal').onclick=reveal;
    document.getElementById('fbNext').onclick=()=>{
      idx=(idx+1)%items.length; renderCard();
    };
    renderCard();
  }

  document.addEventListener('DOMContentLoaded', init);
})();
