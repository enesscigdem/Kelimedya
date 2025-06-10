(function(){
  const puzzle = {
    size: 7,
    across: [
      { num: 1, row: 1, col: 1, answer: "ELMA", clue: "Meyve" },
      { num: 5, row: 3, col: 2, answer: "KITAP", clue: "Okunan nesne" }
    ],
    down: [
      { num: 2, row: 1, col: 4, answer: "AGAÇ", clue: "Odun kaynağı" },
      { num: 6, row: 2, col: 6, answer: "SARAP", clue: "Üzümden yapılan içki" }
    ]
  };

  function buildClues(){
    const a=document.getElementById('across-clues'),
          d=document.getElementById('down-clues');
    a.innerHTML=''; d.innerHTML='';
    puzzle.across.forEach(i=>{
      const li=document.createElement('li');
      li.textContent=`${i.num}. ${i.clue}`;
      a.appendChild(li);
    });
    puzzle.down.forEach(i=>{
      const li=document.createElement('li');
      li.textContent=`${i.num}. ${i.clue}`;
      d.appendChild(li);
    });
  }

  function buildGrid(){
    const grid=document.getElementById('puzzleGrid'),N=puzzle.size;
    grid.innerHTML='';
    grid.style.gridTemplate=`repeat(${N},1fr) / repeat(${N},1fr)`;
    const cells=[];
    for(let r=1;r<=N;r++){
      for(let c=1;c<=N;c++){
        const inp=document.createElement('input');
        inp.type='text';
        inp.maxLength=1;
        inp.className='cp-cell blocked';
        inp.dataset.r=r;
        inp.dataset.c=c;
        inp.readOnly=true;
        grid.appendChild(inp);
        cells.push(inp);
      }
    }
    puzzle.across.forEach(item=>{
      for(let i=0;i<item.answer.length;i++){
        const idx=(item.row-1)*N+(item.col-1+i);
        const cell=cells[idx];
        cell.readOnly=false;
        cell.classList.remove('blocked');
        cell.dataset.answer=item.answer[i];
      }
      const numCell=cells[(item.row-1)*N+(item.col-1)];
      const badge=document.createElement('span');
      badge.className='cp-num';
      badge.textContent=item.num;
      numCell.parentElement.appendChild(badge);
    });
    puzzle.down.forEach(item=>{
      for(let i=0;i<item.answer.length;i++){
        const idx=(item.row-1+i)*N+(item.col-1);
        const cell=cells[idx];
        cell.readOnly=false;
        cell.classList.remove('blocked');
        cell.dataset.answer=item.answer[i];
      }
      const numCell=cells[(item.row-1)*N+(item.col-1)];
      const badge=document.createElement('span');
      badge.className='cp-num';
      badge.textContent=item.num;
      numCell.parentElement.appendChild(badge);
    });
  }

  function checkAnswers(){
    let correct=true;
    document.querySelectorAll('.cp-cell:not(.blocked)').forEach(cell=>{
      if(cell.value.toUpperCase()!==cell.dataset.answer){
        cell.classList.add('wrong');
        correct=false;
      }else{
        cell.classList.remove('wrong');
      }
    });
    const fb=document.getElementById('cpFeedback');
    fb.textContent=correct? '🎉 Tüm cevaplar doğru!' : '❌ Bazı yerler yanlış.';
    fb.className=correct? 'cp-feedback correct':'cp-feedback incorrect';
  }

  function revealAnswers(){
    document.querySelectorAll('.cp-cell:not(.blocked)').forEach(cell=>{
      cell.value=cell.dataset.answer;
      cell.classList.remove('wrong');
    });
  }

  function init(){
    buildClues();
    buildGrid();
    document.getElementById('cpCheck').onclick=checkAnswers;
    document.getElementById('cpReveal').onclick=revealAnswers;
    const backBtn=document.getElementById('cpBack');
    if(backBtn){
      const home=backBtn.dataset.home;
      backBtn.addEventListener('click',()=>{ if(home) window.location=home; });
    }
  }

  document.addEventListener('DOMContentLoaded', init);
})();
