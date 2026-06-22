// Funções auxiliares para o Shopping Cart
window.ShoppingCartHelpers = {
    ensureCartPosition: function() {
        const cartElement = document.querySelector('.container-cart');
        if (cartElement) {
            console.log('[Cart JS] Elemento do carrinho encontrado');
            console.log('[Cart JS] Classes atuais:', cartElement.className);
            console.log('[Cart JS] Posição atual (right):', getComputedStyle(cartElement).right);
            
            // Força a aplicação das classes corretas
            if (cartElement.classList.contains('open')) {
                cartElement.style.right = '0px';
                console.log('[Cart JS] Forçando posição right: 0px');
            } else {
                cartElement.style.right = '-480px';
                console.log('[Cart JS] Forçando posição right: -480px');
            }
        } else {
            console.log('[Cart JS] ERRO: Elemento do carrinho não encontrado');
        }
    },

    openCart: function() {
        console.log('[Cart JS] openCart chamado');
        const cartElement = document.querySelector('.container-cart');
        if (cartElement) {
            cartElement.classList.add('open');
            cartElement.style.right = '0px';
            document.body.classList.add('no-scroll');
            console.log('[Cart JS] Carrinho aberto com sucesso');
            return true;
        }
        console.log('[Cart JS] ERRO: Não foi possível abrir o carrinho');
        return false;
    },

    closeCart: function() {
        console.log('[Cart JS] closeCart chamado');
        const cartElement = document.querySelector('.container-cart');
        if (cartElement) {
            cartElement.classList.remove('open');
            cartElement.style.right = '-480px';
            document.body.classList.remove('no-scroll');
            console.log('[Cart JS] Carrinho fechado com sucesso');
            return true;
        }
        console.log('[Cart JS] ERRO: Não foi possível fechar o carrinho');
        return false;
    }
};

// Função para verificar se é mobile
window.isMobile = function() {
    return /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
};

// Observer para mudanças no DOM
const observer = new MutationObserver(function(mutations) {
    mutations.forEach(function(mutation) {
        if (mutation.type === 'childList' || mutation.type === 'attributes') {
            // Verifica se o carrinho foi adicionado ao DOM
            const cartElement = document.querySelector('.container-cart');
            if (cartElement && !cartElement.hasAttribute('data-initialized')) {
                console.log('[Cart JS] Carrinho detectado no DOM, inicializando...');
                cartElement.setAttribute('data-initialized', 'true');
                window.ShoppingCartHelpers.ensureCartPosition();
            }
        }
    });
});

// Inicia o observer quando o DOM estiver pronto
document.addEventListener('DOMContentLoaded', function() {
    console.log('[Cart JS] DOM carregado, iniciando observer...');
    observer.observe(document.body, {
        childList: true,
        subtree: true,
        attributes: true,
        attributeFilter: ['class']
    });
});
