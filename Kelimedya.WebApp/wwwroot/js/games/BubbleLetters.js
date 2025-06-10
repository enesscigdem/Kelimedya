(function(){
  const wordCards = [
    {
      word: "başarmak",
      synonym: "Muvaffak Olmak",
      definition: "İstenen sonuca ulaşmak, başarılı olmak.",
      sentence: "Düzenli çalışmak, sınavda başarılı olmanın anahtarıdır."
    },
    {
      word: "planlamak",
      synonym: "Tasarlamak",
      definition: "Bir işi önceden düzenlemek, hazırlamak.",
      sentence: "Sınav tarihine göre ders programını planlamak çok önemlidir."
    },
    {
      word: "dikkat etmek",
      synonym: "Itina Göstermek",
      definition: "Özen göstermek, önemsemek.",
      sentence: "Sınavda soruları dikkatlice okuyun."
    }
  ];

  let cardIdx = 0,
      answer = "",
      filled = [],
      letters = [],
      history = [];

  function shuffle(a){
    for(let i=a.length-1;i>0;i--){
      const j=Math.floor(Math.random()*(i+1));
      [a[i],a[j]]=[a[j],a[i]];
    }
  }

  function loadCard(){
    const card = wordCards[cardIdx];
    answer = card.word.replace(/\s+/g, "");
    filled = Array(answer.length).fill("");
    history = [];

    document.getElementById('hintSynonym').textContent = card.synonym;
    document.getElementById('hintDefinition').textContent = card.definition;
    document.getElementById('hintSentence').textContent = card.sentence;

    const target = document.getElementById('targetSlots');
    target.innerHTML='';
    for(let i=0;i<answer.length;i++){
      const span=document.createElement('span');
      span.className='bl-letter';
      span.id=`slot${i}`;
      span.textContent='_';
      target.appendChild(span);
    }

    document.getElementById('blFeedback').textContent='';
    createBubbles();
  }

  function createBubbles(){
    const area=document.getElementById('bubblesArea');
    area.innerHTML='';
    letters=answer.split('');
    const extras='abcçdefgğhıijklmnoöprsştuüvyz'.split('');
    while(letters.length < answer.length*2){
      letters.push(extras[Math.floor(Math.random()*extras.length)]);
    }
    shuffle(letters);
    letters.forEach(ch=>{
      const btn=document.createElement('button');
      btn.className='bubble-btn';
      btn.textContent=ch;
      btn.onclick=()=>selectLetter(ch,btn);
      area.appendChild(btn);
    });
  }

  function selectLetter(ch,btn){
    const idx=filled.findIndex(x=>x==='');
    if(idx===-1) return;
    filled[idx]=ch;
    document.getElementById(`slot${idx}`).textContent=ch;
    btn.disabled=true;
    history.push(btn);
  }

  function removeLast(){
    const last=filled.map((v,i)=>v?i:null).filter(i=>i!==null).pop();
    if(last==null) return;
    filled[last]='';
    document.getElementById(`slot${last}`).textContent='_';
    const btn=history.pop();
    if(btn) btn.disabled=false;
  }

  function init(){
    document.getElementById('blClear').onclick=loadCard;
    document.getElementById('blSubmit').onclick=()=>{
      const guess=filled.join('');
      const fb=document.getElementById('blFeedback');
      if(guess===answer){
        fb.textContent=`🎉 Doğru! Kelime: ${wordCards[cardIdx].word}`;
        fb.className='bl-feedback correct';
      }else{
        fb.textContent=`❌ Yanlış: ${guess || '(boş)'}`;
        fb.className='bl-feedback incorrect';
      }
    };
    document.getElementById('blReveal').onclick=()=>{
      const fb=document.getElementById('blFeedback');
      fb.textContent=`🔍 Cevap: ${wordCards[cardIdx].word}`;
      fb.className='bl-feedback revealed';
    };
    document.getElementById('blNext').onclick=()=>{
      cardIdx=(cardIdx+1)%wordCards.length;
      loadCard();
    };
    const backBtn=document.getElementById('blBack');
    if(backBtn){
      const home=backBtn.dataset.home;
      backBtn.addEventListener('click',()=>{ if(home) window.location=home; });
    }
    document.addEventListener('keydown',e=>{
      if(e.key.length===1 && /^[a-zçğıİöÖşŞüÜ]$/i.test(e.key)){
        const ch=e.key.toLowerCase();
        const btn=Array.from(document.querySelectorAll('.bubble-btn')).find(b=>!b.disabled && b.textContent.toLowerCase()===ch);
        if(btn) selectLetter(ch,btn);
      }else if(e.key==='Backspace'){
        removeLast();
      }
    });
    loadCard();
  }

  document.addEventListener('DOMContentLoaded', init);
})();
