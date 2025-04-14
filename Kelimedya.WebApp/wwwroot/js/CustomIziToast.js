function showIziToastSuccess(message) {
    iziToast.show({
        title: 'Başarılı!',
        message: message,
        position: 'topRight',
        theme: 'light',
        color: 'green',
        timeout: 5000,
        icon: 'fa-solid fa-circle-check',
        transitionIn: 'fadeInDown',
        transitionOut: 'fadeOutUp',
        class: 'iziToastGlassy',
        closeOnClick: true,
        progressBar: false,
        layout: 2
    });
}

function showIziToastError(message) {
    iziToast.show({
        title: 'Hata!',
        message: message,
        position: 'topRight',
        theme: 'light',
        color: 'red',
        timeout: 7000,
        icon: 'fa-solid fa-triangle-exclamation',
        transitionIn: 'fadeInDown',
        transitionOut: 'fadeOutUp',
        class: 'iziToastGlassyError',
        closeOnClick: true,
        progressBar: false,
        layout: 2
    });
}
